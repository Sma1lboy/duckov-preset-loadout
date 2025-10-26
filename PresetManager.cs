using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using ItemStatsSystem;

namespace PresetLoadout
{
    /// <summary>
    /// 预设管理器
    /// 负责保存、加载、应用装备预设
    /// </summary>
    public class PresetManager
    {
        private PresetStorage _presetStorage = null!;
        private readonly string _configFilePath;

        public PresetStorage PresetStorage => _presetStorage;

        public PresetManager(string configFilePath)
        {
            _configFilePath = configFilePath;
            LoadConfig();
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        public void LoadConfig()
        {
            try
            {
                Debug.Log($"[PresetLoadout] Loading config from: {_configFilePath}");

                if (File.Exists(_configFilePath))
                {
                    string json = File.ReadAllText(_configFilePath);
                    Debug.Log($"[PresetLoadout] Config file exists, size: {json.Length} bytes");

                    // 使用 JsonHelper 反序列化
                    _presetStorage = JsonHelper.DeserializePresetStorage(json);

                    // 验证加载的数据
                    if (_presetStorage != null && _presetStorage.Presets != null)
                    {
                        Debug.Log($"[PresetLoadout] ✓ Loaded {_presetStorage.Presets.Count} presets from config");
                        for (int i = 0; i < _presetStorage.Presets.Count; i++)
                        {
                            var preset = _presetStorage.Presets[i];
                            int totalItems = preset?.GetTotalItemCount() ?? 0;
                            Debug.Log($"[PresetLoadout]   Preset {i + 1}: {totalItems} items ({preset?.GetDescription()})");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[PresetLoadout] Deserialization failed, creating new storage");
                        _presetStorage = new PresetStorage();
                    }
                }
                else
                {
                    _presetStorage = new PresetStorage();
                    Debug.Log($"[PresetLoadout] Config file does not exist, created new preset storage");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PresetLoadout] ✗ Failed to load config: {ex.Message}\n{ex.StackTrace}");
                _presetStorage = new PresetStorage();
            }
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        public void SaveConfig()
        {
            try
            {
                // 使用 JsonHelper 序列化
                string json = JsonHelper.SerializePresetStorage(_presetStorage);
                File.WriteAllText(_configFilePath, json);

                Debug.Log($"[PresetLoadout] ✓ Saved config to {_configFilePath}");
                Debug.Log($"[PresetLoadout] JSON length: {json.Length} bytes");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PresetLoadout] ✗ Failed to save config: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 保存当前装备配置到指定预设
        /// </summary>
        public void SaveCurrentLoadout(int slotNumber, Action<string> showMessage)
        {
            try
            {
                // 获取玩家当前身上的所有物品
                List<Item> currentItems = ItemOperations.GetPlayerItems();

                if (currentItems == null || currentItems.Count == 0)
                {
                    showMessage?.Invoke($"预设 {slotNumber}: 没有找到可保存的装备!");
                    Debug.Log($"[PresetLoadout] Save Preset {slotNumber}: No items found on player");
                    return;
                }

                Debug.Log($"[PresetLoadout] ===== Saving Preset {slotNumber} =====");
                Debug.Log($"[PresetLoadout] Total items: {currentItems.Count}");

                // 分类物品：装备槽位 vs 背包
                List<int> equippedIDs = new List<int>();
                List<int> inventoryIDs = new List<int>();

                for (int i = 0; i < currentItems.Count; i++)
                {
                    Item item = currentItems[i];

                    // 跳过无效的物品 (TypeID 为 0 或负数)
                    if (item.TypeID <= 0)
                    {
                        Debug.LogWarning($"[PresetLoadout]   [{i+1}] SKIPPED - Invalid TypeID: {item.TypeID}, Name: {item.name} (likely a system/placeholder item)");
                        continue;
                    }

                    // 判断物品是在装备槽位还是背包
                    bool isInInventory = ItemOperations.IsItemInInventory(item);

                    if (isInInventory)
                    {
                        inventoryIDs.Add(item.TypeID);
                        Debug.Log($"[PresetLoadout]   [{i+1}] INVENTORY - TypeID: {item.TypeID}, Name: {item.name}");
                    }
                    else
                    {
                        equippedIDs.Add(item.TypeID);
                        Debug.Log($"[PresetLoadout]   [{i+1}] EQUIPPED - TypeID: {item.TypeID}, Name: {item.name}");
                    }
                }

                // 保存到预设（使用新的数据结构）
                int index = slotNumber - 1;
                if (index < 0 || index >= _presetStorage.Presets.Count)
                {
                    Debug.LogError($"[PresetLoadout] Invalid preset slot: {slotNumber}");
                    return;
                }

                var preset = _presetStorage.Presets[index];
                preset.EquippedItemTypeIDs = equippedIDs;
                preset.InventoryItemTypeIDs = inventoryIDs;

                // 保存到文件
                SaveConfig();

                int totalCount = equippedIDs.Count + inventoryIDs.Count;
                showMessage?.Invoke($"✓ 预设 {slotNumber} 已保存\n装备:{equippedIDs.Count} 背包:{inventoryIDs.Count}");
                Debug.Log($"[PresetLoadout] Preset {slotNumber} saved: Equipped={string.Join(",", equippedIDs)}, Inventory={string.Join(",", inventoryIDs)}");
                Debug.Log($"[PresetLoadout] ================================");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PresetLoadout] Failed to save preset {slotNumber}: {ex.Message}\n{ex.StackTrace}");
                showMessage?.Invoke($"✗ 保存预设 {slotNumber} 失败!");
            }
        }

        /// <summary>
        /// 应用指定的装备预设
        /// </summary>
        public void ApplyLoadout(int slotNumber, Action<string> showMessage)
        {
            try
            {
                int index = slotNumber - 1;
                if (index < 0 || index >= _presetStorage.Presets.Count)
                {
                    showMessage?.Invoke($"预设 {slotNumber} 不存在!");
                    Debug.Log($"[PresetLoadout] Apply Preset {slotNumber}: Preset does not exist");
                    return;
                }

                PresetConfig preset = _presetStorage.Presets[index];

                int totalItems = preset.GetTotalItemCount();
                if (totalItems == 0)
                {
                    showMessage?.Invoke($"预设 {slotNumber} 是空的! 使用 Ctrl+{slotNumber} 保存装备");
                    Debug.Log($"[PresetLoadout] Apply Preset {slotNumber}: Preset is empty");
                    return;
                }

                Debug.Log($"[PresetLoadout] ===== Applying Preset {slotNumber} =====");
                Debug.Log($"[PresetLoadout] Equipped items: {preset.EquippedItemTypeIDs?.Count ?? 0}, Inventory items: {preset.InventoryItemTypeIDs?.Count ?? 0}");

                // 第一步：将玩家身上的所有物品放回仓库
                Debug.Log($"[PresetLoadout] Step 0: Sending all player items to storage...");
                int movedToStorage = ItemOperations.SendAllPlayerItemsToStorage();
                Debug.Log($"[PresetLoadout] Moved {movedToStorage} items to storage");

                // 获取仓库和玩家身上的物品（分别统计）
                List<Item> storageItems = ItemOperations.GetPlayerStorageItems();
                List<Item> playerItems = ItemOperations.GetPlayerItems();

                Debug.Log($"[PresetLoadout] Found {storageItems?.Count ?? 0} items in storage, {playerItems?.Count ?? 0} items on player");

                int successCount = 0;      // 从仓库成功获取的
                int alreadyOnPlayer = 0;   // 已经在玩家身上的
                int failCount = 0;          // 完全找不到的
                List<Item> usedItems = new List<Item>(); // 记录已使用的物品

                // 第一步：先装备所有装备（包括背包）
                if (preset.EquippedItemTypeIDs != null && preset.EquippedItemTypeIDs.Count > 0)
                {
                    Debug.Log($"[PresetLoadout] Step 1: Equipping {preset.EquippedItemTypeIDs.Count} items: [{string.Join(", ", preset.EquippedItemTypeIDs)}]");

                    foreach (int typeID in preset.EquippedItemTypeIDs)
                    {
                        // 先检查是否已经在玩家身上
                        bool alreadyHas = playerItems?.Any(item => item != null && item.TypeID == typeID) ?? false;

                        if (alreadyHas)
                        {
                            alreadyOnPlayer++;
                            Debug.Log($"[PresetLoadout]   ✓ TypeID {typeID} already on player (skipped)");
                            continue;
                        }

                        // 从仓库中查找未使用的物品
                        Item? matchingItem = storageItems?.FirstOrDefault(item =>
                            item != null &&
                            item.TypeID == typeID &&
                            !usedItems.Contains(item));

                        if (matchingItem != null)
                        {
                            bool success = ItemUtilities.SendToPlayerCharacter(matchingItem, dontMerge: true);

                            if (success)
                            {
                                successCount++;
                                usedItems.Add(matchingItem); // 标记为已使用
                                Debug.Log($"[PresetLoadout]   ✓ Equipped TypeID {typeID} from storage ({matchingItem.name})");
                            }
                            else
                            {
                                failCount++;
                                Debug.LogWarning($"[PresetLoadout]   ✗ Failed to equip TypeID {typeID} ({matchingItem.name})");
                            }
                        }
                        else
                        {
                            failCount++;
                            Debug.LogWarning($"[PresetLoadout]   ✗ TypeID {typeID} not found in storage");
                        }
                    }
                }

                // 第二步：把背包物品放入玩家背包 Inventory
                if (preset.InventoryItemTypeIDs != null && preset.InventoryItemTypeIDs.Count > 0)
                {
                    Debug.Log($"[PresetLoadout] Step 2: Sending {preset.InventoryItemTypeIDs.Count} items to player inventory: [{string.Join(", ", preset.InventoryItemTypeIDs)}]");

                    foreach (int typeID in preset.InventoryItemTypeIDs)
                    {
                        // 先检查是否已经在玩家身上
                        bool alreadyHas = playerItems?.Any(item => item != null && item.TypeID == typeID) ?? false;

                        if (alreadyHas)
                        {
                            alreadyOnPlayer++;
                            Debug.Log($"[PresetLoadout]   ✓ TypeID {typeID} already in player inventory (skipped)");
                            continue;
                        }

                        // 从仓库中查找未使用的物品
                        Item? matchingItem = storageItems?.FirstOrDefault(item =>
                            item != null &&
                            item.TypeID == typeID &&
                            !usedItems.Contains(item));

                        if (matchingItem != null)
                        {
                            // 直接使用 SendToPlayerCharacterInventory，允许合并
                            bool success = ItemUtilities.SendToPlayerCharacterInventory(matchingItem, dontMerge: false);

                            if (success)
                            {
                                successCount++;
                                usedItems.Add(matchingItem); // 标记为已使用
                                Debug.Log($"[PresetLoadout]   ✓ Sent TypeID {typeID} to inventory from storage ({matchingItem.name})");
                            }
                            else
                            {
                                failCount++;
                                Debug.LogWarning($"[PresetLoadout]   ✗ Failed to send TypeID {typeID} to inventory ({matchingItem.name})");
                            }
                        }
                        else
                        {
                            failCount++;
                            Debug.LogWarning($"[PresetLoadout]   ✗ TypeID {typeID} not found in storage");
                        }
                    }
                }

                // 显示结果
                string message = $"预设 {slotNumber} 应用完成";
                if (successCount > 0)
                {
                    message += $"\n✓ 从仓库获取: {successCount}";
                }
                if (alreadyOnPlayer > 0)
                {
                    message += $"\n✓ 已在身上: {alreadyOnPlayer}";
                }
                if (failCount > 0)
                {
                    message += $"\n✗ 缺失: {failCount}";
                }

                showMessage?.Invoke(message);
                Debug.Log($"[PresetLoadout] Apply complete - From storage: {successCount}, Already on player: {alreadyOnPlayer}, Missing: {failCount}");
                Debug.Log($"[PresetLoadout] ================================");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PresetLoadout] Failed to apply preset {slotNumber}: {ex.Message}\n{ex.StackTrace}");
                showMessage?.Invoke($"✗ 应用预设 {slotNumber} 失败!");
            }
        }
    }
}
