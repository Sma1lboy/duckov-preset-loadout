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
        private Rect _windowRect = new Rect(20, 100, 450, 400); // 初始位置（会在打开时动态调整）
        private const int WINDOW_ID = 12345;
        private bool _windowPositionInitialized = false; // 标记是否已初始化窗口位置

        // 重命名状态
        private int _renamingIndex = -1;
        private string _renamingText = "";

        // 背包UI检测
        private readonly bool _requireInventoryOpen = true; // 是否要求背包打开才能使用面板

        void Awake()
        {
            Debug.Log("PresetLoadout Mod Loaded!");
            LoadConfig();
        }

        void Start()
        {
        }

        /// <summary>
        /// 检测玩家背包UI是否打开
        /// </summary>
        private bool IsInventoryUIOpen()
        {
            try
            {
                // 直接查找游戏的背包UI对象 "EquipmentAndInventory"
                GameObject inventoryUI = GameObject.Find("EquipmentAndInventory");
                if (inventoryUI != null && inventoryUI.activeInHierarchy)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PresetLoadout] Error detecting inventory UI: {ex.Message}");
            }

            return false;
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

            // 检测背包关闭，自动关闭预设面板
            if (_showWindow && _requireInventoryOpen)
            {
                if (!IsInventoryUIOpen())
                {
                    _showWindow = false;
                    Debug.Log("[PresetLoadout] Auto-closing preset panel: Inventory UI closed");
                }
            }

            // 打开/关闭 GUI 窗口: L 键
            if (Input.GetKeyDown(KeyCode.L))
            {
                // 如果要求背包打开，则检查背包状态
                if (_requireInventoryOpen && !_showWindow)
                {
                    bool inventoryOpen = IsInventoryUIOpen();
                    if (!inventoryOpen)
                    {
                        ShowMessage("请先打开背包/仓库界面");
                        Debug.Log("[PresetLoadout] Cannot open preset panel: Inventory UI not detected");
                        return;
                    }
                }

                _showWindow = !_showWindow;

                // 第一次打开时，将窗口定位到屏幕中间偏左
                if (_showWindow && !_windowPositionInitialized)
                {
                    // 窗口宽度 450，高度 400
                    float windowWidth = 450f;
                    float windowHeight = 400f;

                    // 水平方向：屏幕中心偏左一点，垂直居中
                    float x = (Screen.width - windowWidth) / 2f - 100f; // 中心偏左100px
                    float y = (Screen.height - windowHeight) / 2f;

                    _windowRect = new Rect(x, y, windowWidth, windowHeight);
                    _windowPositionInitialized = true;

                    Debug.Log($"[PresetLoadout] Initialized window position: x={x}, y={y}, width={windowWidth}, height={windowHeight} (Screen: {Screen.width}x{Screen.height})");
                }

                Debug.Log($"[PresetLoadout] GUI Window: {(_showWindow ? "Opened" : "Closed")}");
                ShowMessage(_showWindow ? "已打开装备预设面板 (按L关闭)" : "已关闭装备预设面板");
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
            GUILayout.Label("按 L 键打开/关闭此面板", EditorStyles());
            GUILayout.Space(10);

            // 遍历所有预设
            for (int i = 0; i < _presetStorage.Presets.Count; i++)
            {
                DrawPresetSlot(i);
                GUILayout.Space(5);
            }

            GUILayout.Space(10);

            // 添加新预设按钮
            if (GUILayout.Button("+ 添加新预设", GUILayout.Height(30)))
            {
                _presetStorage.AddPreset();
                SaveConfig();
                ShowMessage($"已添加预设 {_presetStorage.Presets.Count}");
            }

            GUILayout.Space(10);
            GUILayout.Label("提示: 应用预设会从仓库中获取装备", CenterLabelStyle());

            GUILayout.EndVertical();

            // 让窗口可拖动
            GUI.DragWindow();
        }

        void DrawPresetSlot(int index)
        {
            if (index < 0 || index >= _presetStorage.Presets.Count)
                return;

            PresetConfig preset = _presetStorage.Presets[index];
            int totalCount = preset.GetTotalItemCount();
            bool hasItems = totalCount > 0;
            string statusText = hasItems ? preset.GetDescription() : "空";

            GUILayout.BeginVertical("box");

            // 第一行：预设名称（可编辑）和删除按钮
            GUILayout.BeginHorizontal();

            if (_renamingIndex == index)
            {
                // 正在重命名：显示输入框
                _renamingText = GUILayout.TextField(_renamingText, GUILayout.Width(200));

                // 确认按钮
                if (GUILayout.Button("✓", GUILayout.Width(30)))
                {
                    if (_presetStorage.RenamePreset(index, _renamingText))
                    {
                        SaveConfig();
                        ShowMessage($"预设已重命名为: {_renamingText}");
                    }
                    _renamingIndex = -1;
                }

                // 取消按钮
                if (GUILayout.Button("✗", GUILayout.Width(30)))
                {
                    _renamingIndex = -1;
                }
            }
            else
            {
                // 显示预设名称，点击可重命名
                if (GUILayout.Button(preset.PresetName, BoldLabelStyle(), GUILayout.Width(200)))
                {
                    _renamingIndex = index;
                    _renamingText = preset.PresetName;
                }

                // 物品数量
                GUILayout.Label($"[{statusText}]", GUILayout.Width(120));

                GUILayout.FlexibleSpace();

                // 删除按钮（至少保留3个预设）
                GUI.enabled = _presetStorage.Presets.Count > 3;
                if (GUILayout.Button("删除", GUILayout.Width(50)))
                {
                    if (_presetStorage.RemovePreset(index))
                    {
                        SaveConfig();
                        ShowMessage($"已删除预设: {preset.PresetName}");
                    }
                }
                GUI.enabled = true;
            }

            GUILayout.EndHorizontal();

            // 第二行：保存和应用按钮（仅在非重命名状态显示）
            if (_renamingIndex != index)
            {
                GUILayout.BeginHorizontal();

                // 保存按钮
                if (GUILayout.Button("保存当前装备", GUILayout.Height(25)))
                {
                    SaveCurrentLoadout(index + 1); // slotNumber = index + 1
                }

                // 应用按钮
                GUI.enabled = hasItems;
                if (GUILayout.Button("应用此预设", GUILayout.Height(25)))
                {
                    ApplyLoadout(index + 1); // slotNumber = index + 1
                }
                GUI.enabled = true;

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
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
                // 方法1: 检查 inInventory 字段
                var inInventoryField = item.GetType().GetField("inInventory",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                if (inInventoryField != null)
                {
                    var inventory = inInventoryField.GetValue(item);
                    if (inventory != null)
                    {
                        Debug.Log($"[PresetLoadout]     Item {item.TypeID} ({item.name}) - inInventory: EXISTS -> INVENTORY");
                        return true; // 在背包中
                    }
                }

                // 方法2: 检查 pluggedIntoSlot 字段
                var pluggedIntoSlotField = item.GetType().GetField("pluggedIntoSlot",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                if (pluggedIntoSlotField != null)
                {
                    var slot = pluggedIntoSlotField.GetValue(item);
                    if (slot != null)
                    {
                        Debug.Log($"[PresetLoadout]     Item {item.TypeID} ({item.name}) - pluggedIntoSlot: EXISTS -> EQUIPPED");
                        return false; // 插在槽位上，是装备
                    }
                }

                // 默认：如果两个字段都是 null，可能是顶层装备（如背包本身）
                Debug.Log($"[PresetLoadout]     Item {item.TypeID} ({item.name}) - Both fields NULL -> EQUIPPED (likely top-level equipment)");
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PresetLoadout] Error checking if item {item.TypeID} is in inventory: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 应用指定的装备预设
        /// </summary>
        private void ApplyLoadout(int slotNumber)
        {
            try
            {
                int index = slotNumber - 1;
                if (index < 0 || index >= _presetStorage.Presets.Count)
                {
                    ShowMessage($"预设 {slotNumber} 不存在!");
                    Debug.Log($"[PresetLoadout] Apply Preset {slotNumber}: Preset does not exist");
                    return;
                }

                PresetConfig preset = _presetStorage.Presets[index];

                int totalItems = preset.GetTotalItemCount();
                if (totalItems == 0)
                {
                    ShowMessage($"预设 {slotNumber} 是空的! 使用 Ctrl+{slotNumber} 保存装备");
                    Debug.Log($"[PresetLoadout] Apply Preset {slotNumber}: Preset is empty");
                    return;
                }

                Debug.Log($"[PresetLoadout] ===== Applying Preset {slotNumber} =====");
                Debug.Log($"[PresetLoadout] Equipped items: {preset.EquippedItemTypeIDs?.Count ?? 0}, Inventory items: {preset.InventoryItemTypeIDs?.Count ?? 0}");

                // 第一步：将玩家身上的所有物品放回仓库
                Debug.Log($"[PresetLoadout] Step 0: Sending all player items to storage...");
                int movedToStorage = SendAllPlayerItemsToStorage();
                Debug.Log($"[PresetLoadout] Moved {movedToStorage} items to storage");

                // 获取仓库和玩家身上的物品（分别统计）
                List<Item> storageItems = GetPlayerStorageItems();
                List<Item> playerItems = GetPlayerItems();

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
                        Item matchingItem = storageItems?.FirstOrDefault(item =>
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
                        Item matchingItem = storageItems?.FirstOrDefault(item =>
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

                ShowMessage(message);
                Debug.Log($"[PresetLoadout] Apply complete - From storage: {successCount}, Already on player: {alreadyOnPlayer}, Missing: {failCount}");
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
        /// 获取玩家身上的所有背包物品
        /// </summary>
        private List<Item> GetPlayerBackpacks()
        {
            List<Item> backpacks = new List<Item>();

            try
            {
                Item[] allItems = FindObjectsOfType<Item>();

                foreach (Item item in allItems)
                {
                    if (item != null && item.IsInPlayerCharacter() && item.TypeID > 0)
                    {
                        // 检查物品是否有 inventory 字段（说明它是背包）
                        var inventoryField = item.GetType().GetField("inventory",
                            System.Reflection.BindingFlags.Public |
                            System.Reflection.BindingFlags.NonPublic |
                            System.Reflection.BindingFlags.Instance);

                        if (inventoryField != null)
                        {
                            var inventory = inventoryField.GetValue(item);
                            if (inventory != null)
                            {
                                backpacks.Add(item);
                                Debug.Log($"[PresetLoadout] Found backpack: {item.name} (TypeID: {item.TypeID})");
                            }
                        }
                    }
                }

                Debug.Log($"[PresetLoadout] GetPlayerBackpacks: Found {backpacks.Count} backpack(s)");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PresetLoadout] Error getting player backpacks: {ex.Message}\n{ex.StackTrace}");
            }

            return backpacks;
        }

        /// <summary>
        /// 尝试将物品添加到背包的 Inventory 中
        /// </summary>
        private bool TryAddItemToBackpack(Item item, Item backpack)
        {
            try
            {
                // 获取背包的 inventory 字段
                var inventoryField = backpack.GetType().GetField("inventory",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);

                if (inventoryField == null)
                {
                    Debug.LogWarning($"[PresetLoadout] Backpack {backpack.name} has no inventory field");
                    return false;
                }

                var inventory = inventoryField.GetValue(backpack);
                if (inventory == null)
                {
                    Debug.LogWarning($"[PresetLoadout] Backpack {backpack.name} inventory is null");
                    return false;
                }

                // 获取 Inventory 类型的 Add 方法
                var inventoryType = inventory.GetType();
                var addMethod = inventoryType.GetMethod("Add",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance,
                    null,
                    new Type[] { typeof(Item) },
                    null);

                if (addMethod == null)
                {
                    Debug.LogWarning($"[PresetLoadout] Inventory.Add method not found");
                    return false;
                }

                // 调用 Add 方法
                var result = addMethod.Invoke(inventory, new object[] { item });

                // Add 方法可能返回 bool 或 void
                if (result is bool boolResult)
                {
                    return boolResult;
                }

                // 如果是 void 方法，假设成功
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PresetLoadout] Error adding item to backpack: {ex.Message}");
                return false;
            }
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
        /// 将玩家身上的所有物品发送到仓库
        /// </summary>
        /// <returns>移动的物品数量</returns>
        private int SendAllPlayerItemsToStorage()
        {
            int movedCount = 0;

            try
            {
                // 获取玩家身上的所有物品
                List<Item> playerItems = GetPlayerItems();

                if (playerItems == null || playerItems.Count == 0)
                {
                    Debug.Log($"[PresetLoadout] No items on player to move to storage");
                    return 0;
                }

                Debug.Log($"[PresetLoadout] Found {playerItems.Count} items on player, moving to storage...");

                // 逐个发送到仓库
                foreach (Item item in playerItems)
                {
                    if (item != null && item.TypeID > 0)
                    {
                        try
                        {
                            ItemUtilities.SendToPlayerStorage(item, directToBuffer: false);
                            movedCount++;
                            Debug.Log($"[PresetLoadout]   → Moved TypeID {item.TypeID} ({item.name}) to storage");
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"[PresetLoadout]   ✗ Failed to move item {item.TypeID} ({item.name}): {ex.Message}");
                        }
                    }
                }

                Debug.Log($"[PresetLoadout] Successfully moved {movedCount}/{playerItems.Count} items to storage");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PresetLoadout] Error in SendAllPlayerItemsToStorage: {ex.Message}\n{ex.StackTrace}");
            }

            return movedCount;
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
                Debug.Log($"[PresetLoadout] Loading config from: {ConfigFilePath}");

                if (File.Exists(ConfigFilePath))
                {
                    string json = File.ReadAllText(ConfigFilePath);
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
        private void SaveConfig()
        {
            try
            {
                // 使用 JsonHelper 序列化
                string json = JsonHelper.SerializePresetStorage(_presetStorage);
                File.WriteAllText(ConfigFilePath, json);

                Debug.Log($"[PresetLoadout] ✓ Saved config to {ConfigFilePath}");
                Debug.Log($"[PresetLoadout] JSON length: {json.Length} bytes");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PresetLoadout] ✗ Failed to save config: {ex.Message}\n{ex.StackTrace}");
            }
        }

        void OnDestroy()
        {
            Debug.Log("PresetLoadout Mod Unloaded");
        }
    }
}
