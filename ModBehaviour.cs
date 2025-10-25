using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using ItemStatsSystem;
using Cysharp.Threading.Tasks;
using Dialogues;

namespace PresetLoadout
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        // 预设存储
        private PresetStorage _presetStorage = null!;

        // 配置文件路径
        private string ConfigFilePath => Path.Combine(Application.persistentDataPath, "PresetLoadout_Config.json");

        // UI 消息显示
        private string _displayMessage = "";
        private float _messageTimer = 0f;

        // GUI 窗口
        private bool _showWindow = false;
        private Rect _windowRect = new Rect(100, 100, 400, 300);
        private const int WINDOW_ID = 12345;

        void Awake()
        {
            Debug.Log("PresetLoadout Mod Loaded!");
            LoadConfig();
        }

        void Start()
        {
        }

        void Update()
        {
            // 更新消息显示计时器
            if (_messageTimer > 0f)
            {
                _messageTimer -= Time.deltaTime;
                if (_messageTimer <= 0f)
                {
                    _displayMessage = "";
                }
            }

            // 保存预设: Ctrl + 1/2/3
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    SaveCurrentLoadout(1);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    SaveCurrentLoadout(2);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    SaveCurrentLoadout(3);
                }
            }
            // 应用预设: 单独按 1/2/3
            else
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    ApplyLoadout(1);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    ApplyLoadout(2);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    ApplyLoadout(3);
                }
            }

            // 打开/关闭 GUI 窗口: P 键
            if (Input.GetKeyDown(KeyCode.P))
            {
                _showWindow = !_showWindow;
                Debug.Log($"[PresetLoadout] GUI Window: {(_showWindow ? "Opened" : "Closed")}");
                ShowMessage(_showWindow ? "已打开装备预设面板" : "已关闭装备预设面板");
            }
        }

        void OnGUI()
        {
            // 显示浮动窗口
            if (_showWindow)
            {
                _windowRect = GUI.Window(WINDOW_ID, _windowRect, DrawWindow, "装备预设系统");
            }

            // 如果有消息要显示
            if (!string.IsNullOrEmpty(_displayMessage))
            {
                // 创建样式
                GUIStyle style = new GUIStyle(GUI.skin.box);
                style.fontSize = 20;
                style.alignment = TextAnchor.MiddleCenter;
                style.normal.textColor = Color.white;
                style.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.8f));
                style.padding = new RectOffset(20, 20, 10, 10);

                // 计算文本大小
                GUIContent content = new GUIContent(_displayMessage);
                Vector2 size = style.CalcSize(content);

                // 在屏幕中上方显示
                float x = (Screen.width - size.x) / 2;
                float y = Screen.height * 0.15f;

                GUI.Box(new Rect(x, y, size.x, size.y), _displayMessage, style);
            }
        }

        void DrawWindow(int windowID)
        {
            GUILayout.BeginVertical();

            // 标题说明
            GUILayout.Label("按 P 键打开/关闭此面板", EditorStyles());
            GUILayout.Space(10);

            // 遍历 3 个预设
            for (int i = 1; i <= 3; i++)
            {
                DrawPresetSlot(i);
                GUILayout.Space(10);
            }

            GUILayout.Space(10);
            GUILayout.Label("提示: 应用预设会从仓库中获取装备", CenterLabelStyle());

            GUILayout.EndVertical();

            // 让窗口可拖动
            GUI.DragWindow();
        }

        void DrawPresetSlot(int slotNumber)
        {
            GUILayout.BeginHorizontal("box");

            // 预设名称和物品数量
            string statusText = "空";
            bool hasItems = false;

            if (_presetStorage.Presets.ContainsKey(slotNumber))
            {
                PresetConfig preset = _presetStorage.Presets[slotNumber];
                int totalCount = preset.GetTotalItemCount();
                hasItems = totalCount > 0;

                if (hasItems)
                {
                    statusText = preset.GetDescription();
                }
            }

            GUILayout.Label($"预设 {slotNumber}: [{statusText}]", BoldLabelStyle(), GUILayout.Width(180));

            GUILayout.FlexibleSpace();

            // 保存按钮
            if (GUILayout.Button("保存当前", GUILayout.Width(100)))
            {
                SaveCurrentLoadout(slotNumber);
            }

            // 应用按钮
            GUI.enabled = hasItems;
            if (GUILayout.Button("应用此预设", GUILayout.Width(100)))
            {
                ApplyLoadout(slotNumber);
            }
            GUI.enabled = true;

            GUILayout.EndHorizontal();
        }

        // GUI 样式辅助方法
        private GUIStyle BoldLabelStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 14;
            return style;
        }

        private GUIStyle CenterLabelStyle()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 12;
            return style;
        }

        private GUIStyle EditorStyles()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 11;
            style.fontStyle = FontStyle.Italic;
            return style;
        }

        // 创建纯色纹理的辅助方法
        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        /// <summary>
        /// 保存当前装备配置到指定预设
        /// </summary>
        private void SaveCurrentLoadout(int slotNumber)
        {
            try
            {
                // 获取玩家当前身上的所有物品
                List<Item> currentItems = GetPlayerItems();

                if (currentItems == null || currentItems.Count == 0)
                {
                    ShowMessage($"预设 {slotNumber}: 没有找到可保存的装备!");
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
                    // 如果物品有 parentInventory 且不是 null，说明在背包中
                    // 否则认为是装备在身上
                    bool isInInventory = IsItemInInventory(item);

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
                _presetStorage.Presets[slotNumber].EquippedItemTypeIDs = equippedIDs;
                _presetStorage.Presets[slotNumber].InventoryItemTypeIDs = inventoryIDs;
                _presetStorage.Presets[slotNumber].PresetName = $"预设 {slotNumber}";

                // 保存到文件
                SaveConfig();

                int totalCount = equippedIDs.Count + inventoryIDs.Count;
                ShowMessage($"✓ 预设 {slotNumber} 已保存\n装备:{equippedIDs.Count} 背包:{inventoryIDs.Count}");
                Debug.Log($"[PresetLoadout] Preset {slotNumber} saved: Equipped={string.Join(",", equippedIDs)}, Inventory={string.Join(",", inventoryIDs)}");
                Debug.Log($"[PresetLoadout] ================================");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PresetLoadout] Failed to save preset {slotNumber}: {ex.Message}\n{ex.StackTrace}");
                ShowMessage($"✗ 保存预设 {slotNumber} 失败!");
            }
        }

        /// <summary>
        /// 判断物品是否在背包中（而不是装备槽位）
        /// </summary>
        private bool IsItemInInventory(Item item)
        {
            try
            {
                // 检查 parentInventory 字段
                var inventoryField = item.GetType().GetField("parentInventory");
                if (inventoryField != null)
                {
                    var inventory = inventoryField.GetValue(item);
                    return inventory != null;
                }

                // 备用方案：检查 transform 父对象名称
                if (item.transform.parent != null)
                {
                    string parentName = item.transform.parent.name.ToLower();
                    return parentName.Contains("inventory") || parentName.Contains("backpack");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PresetLoadout] Error checking if item {item.TypeID} is in inventory: {ex.Message}");
            }

            // 默认认为是装备槽位
            return false;
        }

        /// <summary>
        /// 应用指定的装备预设
        /// </summary>
        private void ApplyLoadout(int slotNumber)
        {
            try
            {
                if (!_presetStorage.Presets.ContainsKey(slotNumber))
                {
                    ShowMessage($"预设 {slotNumber} 不存在!");
                    Debug.Log($"[PresetLoadout] Apply Preset {slotNumber}: Preset does not exist");
                    return;
                }

                PresetConfig preset = _presetStorage.Presets[slotNumber];

                int totalItems = preset.GetTotalItemCount();
                if (totalItems == 0)
                {
                    ShowMessage($"预设 {slotNumber} 是空的! 使用 Ctrl+{slotNumber} 保存装备");
                    Debug.Log($"[PresetLoadout] Apply Preset {slotNumber}: Preset is empty");
                    return;
                }

                Debug.Log($"[PresetLoadout] ===== Applying Preset {slotNumber} =====");
                Debug.Log($"[PresetLoadout] Equipped items: {preset.EquippedItemTypeIDs?.Count ?? 0}, Inventory items: {preset.InventoryItemTypeIDs?.Count ?? 0}");

                // 获取玩家仓库中的所有物品
                List<Item> storageItems = GetPlayerStorageItems();

                if (storageItems == null || storageItems.Count == 0)
                {
                    ShowMessage("仓库中没有物品!");
                    Debug.Log($"[PresetLoadout] Apply Preset {slotNumber}: No items in storage");
                    return;
                }

                Debug.Log($"[PresetLoadout] Found {storageItems.Count} items in storage");

                int successCount = 0;
                int failCount = 0;

                // 第一步：优先应用装备槽位的物品（使用 SendToPlayerCharacter）
                if (preset.EquippedItemTypeIDs != null && preset.EquippedItemTypeIDs.Count > 0)
                {
                    Debug.Log($"[PresetLoadout] Step 1: Applying {preset.EquippedItemTypeIDs.Count} EQUIPPED items: [{string.Join(", ", preset.EquippedItemTypeIDs)}]");

                    foreach (int typeID in preset.EquippedItemTypeIDs)
                    {
                        Item matchingItem = storageItems.FirstOrDefault(item =>
                            item != null && item.TypeID == typeID && item.IsInPlayerStorage());

                        if (matchingItem != null)
                        {
                            // 优先尝试装备到角色身上
                            bool success = ItemUtilities.SendToPlayerCharacter(matchingItem, dontMerge: true);

                            if (success)
                            {
                                successCount++;
                                Debug.Log($"[PresetLoadout]   ✓ Equipped TypeID {typeID} ({matchingItem.name})");
                            }
                            else
                            {
                                // 如果装备失败，尝试放入背包
                                success = ItemUtilities.SendToPlayerCharacterInventory(matchingItem, dontMerge: true);

                                if (success)
                                {
                                    successCount++;
                                    Debug.Log($"[PresetLoadout]   ✓ Sent TypeID {typeID} to inventory (slot full)");
                                }
                                else
                                {
                                    failCount++;
                                    Debug.LogWarning($"[PresetLoadout]   ✗ Failed to apply TypeID {typeID} ({matchingItem.name})");
                                }
                            }
                        }
                        else
                        {
                            failCount++;
                            Debug.LogWarning($"[PresetLoadout]   ✗ TypeID {typeID} not found in storage");
                        }
                    }
                }

                // 第二步：应用背包物品（使用 SendToPlayerCharacterInventory）
                if (preset.InventoryItemTypeIDs != null && preset.InventoryItemTypeIDs.Count > 0)
                {
                    Debug.Log($"[PresetLoadout] Step 2: Applying {preset.InventoryItemTypeIDs.Count} INVENTORY items: [{string.Join(", ", preset.InventoryItemTypeIDs)}]");

                    foreach (int typeID in preset.InventoryItemTypeIDs)
                    {
                        Item matchingItem = storageItems.FirstOrDefault(item =>
                            item != null && item.TypeID == typeID && item.IsInPlayerStorage());

                        if (matchingItem != null)
                        {
                            // 发送到背包
                            bool success = ItemUtilities.SendToPlayerCharacterInventory(matchingItem, dontMerge: true);

                            if (success)
                            {
                                successCount++;
                                Debug.Log($"[PresetLoadout]   ✓ Sent TypeID {typeID} to inventory ({matchingItem.name})");
                            }
                            else
                            {
                                failCount++;
                                Debug.LogWarning($"[PresetLoadout]   ✗ Failed TypeID {typeID} ({matchingItem.name})");
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
                string message = $"预设 {slotNumber} 应用完成\n✓ 成功: {successCount}";
                if (failCount > 0)
                {
                    message += $"\n✗ 失败: {failCount} (仓库中没有)";
                }

                ShowMessage(message);
                Debug.Log($"[PresetLoadout] Apply complete - Success: {successCount}, Failed: {failCount}");
                Debug.Log($"[PresetLoadout] ================================");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PresetLoadout] Failed to apply preset {slotNumber}: {ex.Message}\n{ex.StackTrace}");
                ShowMessage($"✗ 应用预设 {slotNumber} 失败!");
            }
        }

        /// <summary>
        /// 获取玩家身上的所有物品
        /// </summary>
        private List<Item> GetPlayerItems()
        {
            List<Item> items = new List<Item>();

            try
            {
                // 查找场景中所有的 Item 对象
                Item[] allItems = FindObjectsOfType<Item>();

                Debug.Log($"[PresetLoadout] GetPlayerItems: Scanning {allItems.Length} total items in scene");

                foreach (Item item in allItems)
                {
                    if (item != null && item.IsInPlayerCharacter())
                    {
                        // 只添加有效的物品 (TypeID > 0)
                        if (item.TypeID > 0)
                        {
                            items.Add(item);
                        }
                        else
                        {
                            Debug.Log($"[PresetLoadout]   Skipping invalid item on player: TypeID={item.TypeID}, Name={item.name}");
                        }
                    }
                }

                Debug.Log($"[PresetLoadout] GetPlayerItems: Found {items.Count} valid items on player character");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PresetLoadout] Error getting player items: {ex.Message}\n{ex.StackTrace}");
            }

            return items;
        }

        /// <summary>
        /// 获取玩家仓库中的所有物品
        /// </summary>
        private List<Item> GetPlayerStorageItems()
        {
            List<Item> items = new List<Item>();

            try
            {
                // 查找场景中所有的 Item 对象
                Item[] allItems = FindObjectsOfType<Item>();

                int skippedCount = 0;
                foreach (Item item in allItems)
                {
                    if (item != null && item.IsInPlayerStorage())
                    {
                        // 只添加有效的物品 (TypeID > 0)
                        if (item.TypeID > 0)
                        {
                            items.Add(item);
                        }
                        else
                        {
                            skippedCount++;
                        }
                    }
                }

                Debug.Log($"[PresetLoadout] GetPlayerStorageItems: Found {items.Count} valid items in storage{(skippedCount > 0 ? $" (skipped {skippedCount} invalid)" : "")}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PresetLoadout] Error getting storage items: {ex.Message}\n{ex.StackTrace}");
            }

            return items;
        }

        /// <summary>
        /// 显示欢迎消息
        /// </summary>
        private async void ShowWelcomeMessage()
        {
            await System.Threading.Tasks.Task.Delay(2000); // 延迟 2 秒
            ShowMessage("装备预设系统已加载\n按 H 查看帮助");
        }

        /// <summary>
        /// 显示帮助信息
        /// </summary>
        private void ShowHelp()
        {
            string helpText =
                "=== 装备预设系统 ===\n" +
                "保存预设: Ctrl + 1/2/3\n" +
                "应用预设: 1/2/3\n" +
                "显示帮助: H\n" +
                "\n" +
                "注意: 应用预设时会从\n" +
                "仓库中获取装备,如果\n" +
                "仓库中没有则跳过";

            ShowMessage(helpText);
        }

        /// <summary>
        /// 显示消息 (使用 OnGUI 在屏幕上绘制)
        /// </summary>
        private void ShowMessage(string message)
        {
            _displayMessage = message;
            _messageTimer = 3f;  // 显示 3 秒
            Debug.Log($"[PresetLoadout] {message}");
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        private void LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    _presetStorage = JsonUtility.FromJson<PresetStorage>(json);
                    Debug.Log($"Loaded config from {ConfigFilePath}");
                }
                else
                {
                    _presetStorage = new PresetStorage();
                    Debug.Log("Created new preset storage");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load config: {ex.Message}");
                _presetStorage = new PresetStorage();
            }
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        private void SaveConfig()
        {
            try
            {
                string json = JsonUtility.ToJson(_presetStorage, true);
                File.WriteAllText(ConfigFilePath, json);
                Debug.Log($"Saved config to {ConfigFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save config: {ex.Message}");
            }
        }

        void OnDestroy()
        {
            Debug.Log("PresetLoadout Mod Unloaded");
        }
    }
}
