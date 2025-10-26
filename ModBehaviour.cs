using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using ItemStatsSystem;
using Cysharp.Threading.Tasks;
using Dialogues;
using TMPro;
using Duckov.UI;
using Duckov.Utilities;

namespace PresetLoadout
{
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        // 预设存储
        private PresetStorage _presetStorage = null!;

        // 配置文件路径
        private string ConfigFilePath => Path.Combine(Application.persistentDataPath, "PresetLoadout_Config.json");

        // 官方UI组件
        private Canvas _mainCanvas = null!;
        private GameObject _panelRoot = null!;
        private GameObject _scrollContent = null!; // 滚动内容容器
        private GameObject _addPresetButton = null!; // "添加新预设"按钮
        private TextMeshProUGUI _messageText = null!;
        private GameObject _messagePanel = null!;
        private float _messageTimer = 0f;

        // 预设面板状态
        private bool _showWindow = false;
        private readonly List<GameObject> _presetSlotUIObjects = new List<GameObject>();

        // 背包UI检测
        private readonly bool _requireInventoryOpen = true; // 是否要求背包打开才能使用面板

        void Awake()
        {
            Debug.Log("PresetLoadout Mod Loaded!");
            LoadConfig();
            InitializeUI();
        }

        void Start()
        {
        }

        /// <summary>
        /// 初始化官方样式的UI系统
        /// </summary>
        private void InitializeUI()
        {
            try
            {
                // 创建Canvas
                GameObject canvasObj = new GameObject("PresetLoadout_Canvas");
                _mainCanvas = canvasObj.AddComponent<Canvas>();
                _mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _mainCanvas.sortingOrder = 100; // 确保在游戏UI上层

                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);

                canvasObj.AddComponent<GraphicRaycaster>();
                DontDestroyOnLoad(canvasObj);

                // 创建消息面板
                CreateMessagePanel();

                // 创建预设管理面板
                CreatePresetPanel();

                Debug.Log("[PresetLoadout] Official UI system initialized");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PresetLoadout] Failed to initialize UI: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 创建消息提示面板（使用官方TextMeshPro样式）
        /// </summary>
        private void CreateMessagePanel()
        {
            // 创建消息面板容器
            _messagePanel = new GameObject("MessagePanel");
            _messagePanel.transform.SetParent(_mainCanvas.transform, false);

            RectTransform msgRect = _messagePanel.AddComponent<RectTransform>();
            msgRect.anchorMin = new Vector2(0.5f, 0.8f);
            msgRect.anchorMax = new Vector2(0.5f, 0.8f);
            msgRect.pivot = new Vector2(0.5f, 0.5f);
            msgRect.sizeDelta = new Vector2(600, 100);

            // 添加背景 (圆角深蓝色背景)
            Image bgImage = _messagePanel.AddComponent<Image>();
            bgImage.sprite = CreateRoundedSprite(12);
            bgImage.type = Image.Type.Sliced;
            bgImage.color = new Color(0.06f, 0.10f, 0.16f, 0.94f);

            // 创建文本（使用官方模板）
            _messageText = Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI);
            _messageText.transform.SetParent(_messagePanel.transform, false);
            _messageText.transform.localScale = Vector3.one;

            RectTransform textRect = _messageText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20, 10);
            textRect.offsetMax = new Vector2(-20, -10);

            _messageText.fontSize = 24f;
            _messageText.alignment = TextAlignmentOptions.Center;
            _messageText.enableWordWrapping = true;

            _messagePanel.SetActive(false);
        }

        /// <summary>
        /// 创建预设管理面板
        /// </summary>
        private void CreatePresetPanel()
        {
            // 创建主面板
            _panelRoot = new GameObject("PresetPanel");
            _panelRoot.transform.SetParent(_mainCanvas.transform, false);

            RectTransform panelRect = _panelRoot.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(500, 600);
            panelRect.anchoredPosition = new Vector2(-100, 0);

            // 添加背景 (圆角深色背景 - 接近游戏官方UI色调)
            Image bgImage = _panelRoot.AddComponent<Image>();
            bgImage.sprite = CreateRoundedSprite(16);
            bgImage.type = Image.Type.Sliced;
            // 使用接近游戏UI的深蓝色调（更蓝）
            bgImage.color = new Color(0.08f, 0.12f, 0.20f, 0.96f);

            // 添加标题
            CreatePanelTitle();

            // 创建滚动视图
            CreateScrollView();

            // 创建底部"添加预设"按钮
            CreateAddPresetButton();

            // 初始时隐藏
            _panelRoot.SetActive(false);
        }

        /// <summary>
        /// 创建滚动视图
        /// </summary>
        private void CreateScrollView()
        {
            // 创建 ScrollView 容器
            GameObject scrollView = new GameObject("ScrollView");
            scrollView.transform.SetParent(_panelRoot.transform, false);

            RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0, 0);
            scrollRect.anchorMax = new Vector2(1, 1);
            scrollRect.pivot = new Vector2(0.5f, 0.5f);
            scrollRect.offsetMin = new Vector2(10, 65); // 底部留65给"添加"按钮（45高度+10底边距+10间距）
            scrollRect.offsetMax = new Vector2(-10, -60); // 顶部留60给标题

            // 创建 Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);

            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;

            // 使用 RectMask2D 而不是 Mask（不需要 Image）
            RectMask2D mask = viewport.AddComponent<RectMask2D>();

            // 创建 Content (滚动内容)
            _scrollContent = new GameObject("Content");
            _scrollContent.transform.SetParent(viewport.transform, false);

            RectTransform contentRect = _scrollContent.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = new Vector2(0, 500); // 初始高度500，会在刷新时动态调整
            contentRect.anchoredPosition = Vector2.zero;

            // 添加 ScrollRect 组件
            ScrollRect scrollRectComponent = scrollView.AddComponent<ScrollRect>();
            scrollRectComponent.content = contentRect;
            scrollRectComponent.viewport = viewportRect;
            scrollRectComponent.horizontal = false;
            scrollRectComponent.vertical = true;
            scrollRectComponent.scrollSensitivity = 20f;
            scrollRectComponent.movementType = ScrollRect.MovementType.Clamped;
        }

        /// <summary>
        /// 创建"添加预设"按钮
        /// </summary>
        private void CreateAddPresetButton()
        {
            _addPresetButton = new GameObject("AddPresetButton");
            _addPresetButton.transform.SetParent(_panelRoot.transform, false);

            RectTransform addBtnRect = _addPresetButton.AddComponent<RectTransform>();
            addBtnRect.anchorMin = new Vector2(0, 0);
            addBtnRect.anchorMax = new Vector2(1, 0);
            addBtnRect.pivot = new Vector2(0.5f, 0f);
            addBtnRect.sizeDelta = new Vector2(-20, 45);
            addBtnRect.anchoredPosition = new Vector2(0, 10);

            // 确保按钮在最上层（最后渲染）
            _addPresetButton.transform.SetAsLastSibling();

            Image btnImage = _addPresetButton.AddComponent<Image>();
            btnImage.sprite = CreateRoundedSprite(8);
            btnImage.type = Image.Type.Sliced;
            btnImage.color = new Color(0.20f, 0.60f, 0.40f, 0.95f); // 绿色调

            Button button = _addPresetButton.AddComponent<Button>();
            button.targetGraphic = btnImage;
            button.onClick.AddListener(() =>
            {
                _presetStorage.AddPreset();
                SaveConfig();
                RefreshPresetPanel();
                ShowMessage($"已添加预设 {_presetStorage.Presets.Count}");
            });

            // 添加按钮文本
            TextMeshProUGUI btnText = Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI);
            btnText.transform.SetParent(_addPresetButton.transform, false);
            btnText.transform.localScale = Vector3.one;

            RectTransform textRect = btnText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            btnText.text = "+ 添加新预设";
            btnText.fontSize = 18f;
            btnText.alignment = TextAlignmentOptions.Center;
            btnText.fontStyle = FontStyles.Bold;
        }

        /// <summary>
        /// 创建面板标题
        /// </summary>
        private void CreatePanelTitle()
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(_panelRoot.transform, false);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(0, 50);
            titleRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI titleText = Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI);
            titleText.transform.SetParent(titleObj.transform, false);
            titleText.transform.localScale = Vector3.one;

            RectTransform titleTextRect = titleText.GetComponent<RectTransform>();
            titleTextRect.anchorMin = Vector2.zero;
            titleTextRect.anchorMax = Vector2.one;
            titleTextRect.offsetMin = Vector2.zero;
            titleTextRect.offsetMax = Vector2.zero;

            titleText.text = "装备预设系统";
            titleText.fontSize = 28f;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
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
                    if (_messagePanel != null)
                    {
                        _messagePanel.SetActive(false);
                    }
                }
            }

            // 检测背包关闭，自动关闭预设面板
            if (_showWindow && _requireInventoryOpen)
            {
                if (!IsInventoryUIOpen())
                {
                    _showWindow = false;
                    if (_panelRoot != null)
                    {
                        _panelRoot.SetActive(false);
                    }
                    Debug.Log("[PresetLoadout] Auto-closing preset panel: Inventory UI closed");
                }
            }

            // 打开/关闭面板: L 键
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

                if (_panelRoot != null)
                {
                    _panelRoot.SetActive(_showWindow);

                    // 如果打开面板，刷新预设显示
                    if (_showWindow)
                    {
                        RefreshPresetPanel();
                    }
                }

                Debug.Log($"[PresetLoadout] Preset Panel: {(_showWindow ? "Opened" : "Closed")}");
                ShowMessage(_showWindow ? "已打开装备预设面板 (按L关闭)" : "已关闭装备预设面板");
            }
        }

        /// <summary>
        /// 刷新预设面板显示
        /// </summary>
        private void RefreshPresetPanel()
        {
            // 清除现有的预设槽位UI
            foreach (var uiObj in _presetSlotUIObjects)
            {
                if (uiObj != null)
                {
                    Destroy(uiObj);
                }
            }
            _presetSlotUIObjects.Clear();

            // 为每个预设创建UI
            for (int i = 0; i < _presetStorage.Presets.Count; i++)
            {
                CreatePresetSlotUI(i);
            }

            // 更新滚动内容的高度
            UpdateScrollContentHeight();

            // 确保"添加"按钮始终在最上层
            if (_addPresetButton != null)
            {
                _addPresetButton.transform.SetAsLastSibling();
            }

            Debug.Log($"[PresetLoadout] Refreshed preset panel with {_presetStorage.Presets.Count} presets");
        }

        /// <summary>
        /// 更新滚动内容的高度
        /// </summary>
        private void UpdateScrollContentHeight()
        {
            if (_scrollContent != null)
            {
                RectTransform contentRect = _scrollContent.GetComponent<RectTransform>();
                int presetCount = _presetStorage.Presets.Count;
                float slotHeight = 120f; // 每个槽位的高度（增加了）
                float spacing = 10f;     // 槽位之间的间距
                float totalHeight = (slotHeight + spacing) * presetCount + spacing;

                // 确保最小高度
                totalHeight = Mathf.Max(totalHeight, 300f);

                contentRect.sizeDelta = new Vector2(0, totalHeight);
                Debug.Log($"[PresetLoadout] UpdateScrollContentHeight: presetCount={presetCount}, totalHeight={totalHeight}");
            }
        }

        /// <summary>
        /// 创建单个预设槽位的UI
        /// </summary>
        private void CreatePresetSlotUI(int index)
        {
            PresetConfig preset = _presetStorage.Presets[index];

            // 创建槽位容器（添加到滚动内容中）
            GameObject slotObj = new GameObject($"PresetSlot_{index}");
            slotObj.transform.SetParent(_scrollContent.transform, false);

            float slotHeight = 120f; // 增加高度
            float spacing = 10f;

            RectTransform slotRect = slotObj.AddComponent<RectTransform>();
            slotRect.anchorMin = new Vector2(0, 1);
            slotRect.anchorMax = new Vector2(1, 1);
            slotRect.pivot = new Vector2(0.5f, 1f);
            slotRect.sizeDelta = new Vector2(-20, slotHeight);
            slotRect.anchoredPosition = new Vector2(0, -(spacing + (index * (slotHeight + spacing))));

            // 添加背景 (圆角槽位背景 - 稍亮的蓝灰色)
            Image slotBg = slotObj.AddComponent<Image>();
            slotBg.sprite = CreateRoundedSprite(10);
            slotBg.type = Image.Type.Sliced;
            slotBg.color = new Color(0.12f, 0.18f, 0.28f, 0.90f);

            // 创建预设名称文本
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(slotObj.transform, false);

            RectTransform nameRect = nameObj.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.55f); // 调整比例，给按钮更多空间
            nameRect.anchorMax = new Vector2(1, 1);
            nameRect.offsetMin = new Vector2(10, 0);
            nameRect.offsetMax = new Vector2(-10, -5);

            TextMeshProUGUI nameText = Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI);
            nameText.transform.SetParent(nameObj.transform, false);
            nameText.transform.localScale = Vector3.one;

            RectTransform nameTextRect = nameText.GetComponent<RectTransform>();
            nameTextRect.anchorMin = Vector2.zero;
            nameTextRect.anchorMax = Vector2.one;
            nameTextRect.offsetMin = Vector2.zero;
            nameTextRect.offsetMax = Vector2.zero;

            nameText.text = $"{preset.PresetName} [{preset.GetDescription()}]";
            nameText.fontSize = 18f;
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.fontStyle = FontStyles.Bold;

            // 创建按钮容器（增加高度）
            GameObject buttonsObj = new GameObject("Buttons");
            buttonsObj.transform.SetParent(slotObj.transform, false);

            RectTransform buttonsRect = buttonsObj.AddComponent<RectTransform>();
            buttonsRect.anchorMin = new Vector2(0, 0);
            buttonsRect.anchorMax = new Vector2(1, 0.50f); // 按钮占据下半部分，稍微增加
            buttonsRect.offsetMin = new Vector2(10, 8);
            buttonsRect.offsetMax = new Vector2(-10, 0);

            // 创建保存按钮
            CreateButton(buttonsObj, "保存当前装备", new Vector2(0, 0.5f), new Vector2(0.48f, 1), () =>
            {
                SaveCurrentLoadout(index + 1);
                RefreshPresetPanel(); // 刷新显示
            });

            // 创建应用按钮
            bool hasItems = preset.GetTotalItemCount() > 0;
            CreateButton(buttonsObj, "应用此预设", new Vector2(0.52f, 0.5f), new Vector2(1, 1), () =>
            {
                ApplyLoadout(index + 1);
            }, !hasItems);

            // 创建删除按钮（如果预设数量大于3）
            if (_presetStorage.Presets.Count > 3)
            {
                CreateButton(buttonsObj, "删除", new Vector2(0, 0), new Vector2(1, 0.4f), () =>
                {
                    if (_presetStorage.RemovePreset(index))
                    {
                        SaveConfig();
                        RefreshPresetPanel();
                        ShowMessage($"已删除预设 {index + 1}");
                    }
                }, false, new Color(0.75f, 0.25f, 0.25f, 0.85f)); // 红色调
            }

            _presetSlotUIObjects.Add(slotObj);
        }

        /// <summary>
        /// 创建按钮
        /// </summary>
        private void CreateButton(GameObject parent, string text, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction onClick, bool disabled = false, Color? customColor = null)
        {
            GameObject btnObj = new GameObject($"Btn_{text}");
            btnObj.transform.SetParent(parent.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = anchorMin;
            btnRect.anchorMax = anchorMax;
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.sprite = CreateRoundedSprite(8);
            btnImage.type = Image.Type.Sliced;

            // 使用自定义颜色，或默认颜色
            if (customColor.HasValue)
            {
                btnImage.color = customColor.Value;
            }
            else
            {
                // 使用游戏风格的青蓝色调 (正常) 和暗蓝灰色 (禁用)
                btnImage.color = disabled
                    ? new Color(0.22f, 0.28f, 0.35f, 0.75f)  // 禁用: 暗蓝灰色
                    : new Color(0.25f, 0.50f, 0.75f, 0.95f); // 正常: 青蓝色（接近游戏UI）
            }

            Button button = btnObj.AddComponent<Button>();
            button.targetGraphic = btnImage;
            button.interactable = !disabled;

            if (onClick != null)
            {
                button.onClick.AddListener(onClick);
            }

            // 添加按钮文本
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(btnObj.transform, false);

            TextMeshProUGUI btnText = Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI);
            btnText.transform.SetParent(textObj.transform, false);
            btnText.transform.localScale = Vector3.one;

            RectTransform textRect = btnText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            btnText.text = text;
            btnText.fontSize = 16f;
            btnText.alignment = TextAlignmentOptions.Center;
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
        /// 显示消息 (使用官方TextMeshPro样式)
        /// </summary>
        private void ShowMessage(string message)
        {
            if (_messageText != null && _messagePanel != null)
            {
                _messageText.text = message;
                _messagePanel.SetActive(true);
                _messageTimer = 3f;  // 显示 3 秒
            }
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

        /// <summary>
        /// 创建圆角矩形Sprite
        /// </summary>
        private Sprite CreateRoundedSprite(int cornerRadius)
        {
            int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // 计算到四个角的距离
                    float distToCorner = float.MaxValue;

                    // 左下角
                    if (x < cornerRadius && y < cornerRadius)
                        distToCorner = Mathf.Sqrt((x - cornerRadius) * (x - cornerRadius) + (y - cornerRadius) * (y - cornerRadius));
                    // 右下角
                    else if (x >= size - cornerRadius && y < cornerRadius)
                        distToCorner = Mathf.Sqrt((x - (size - cornerRadius - 1)) * (x - (size - cornerRadius - 1)) + (y - cornerRadius) * (y - cornerRadius));
                    // 左上角
                    else if (x < cornerRadius && y >= size - cornerRadius)
                        distToCorner = Mathf.Sqrt((x - cornerRadius) * (x - cornerRadius) + (y - (size - cornerRadius - 1)) * (y - (size - cornerRadius - 1)));
                    // 右上角
                    else if (x >= size - cornerRadius && y >= size - cornerRadius)
                        distToCorner = Mathf.Sqrt((x - (size - cornerRadius - 1)) * (x - (size - cornerRadius - 1)) + (y - (size - cornerRadius - 1)) * (y - (size - cornerRadius - 1)));

                    // 如果在角落区域且距离大于圆角半径，设为透明
                    if (distToCorner != float.MaxValue && distToCorner > cornerRadius)
                        pixels[y * size + x] = Color.clear;
                    else
                        pixels[y * size + x] = Color.white;
                }
            }

            texture.SetPixels(pixels);
            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;

            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(cornerRadius, cornerRadius, cornerRadius, cornerRadius));
        }

        void OnDestroy()
        {
            Debug.Log("PresetLoadout Mod Unloaded");
        }
    }
}
