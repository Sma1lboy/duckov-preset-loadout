using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Duckov.Utilities;

namespace PresetLoadout
{
    /// <summary>
    /// UI 辅助类
    /// 负责创建和管理所有 UI 组件，包括面板、按钮、消息提示等
    /// </summary>
    public class UIHelper
    {
        // UI 组件
        private Canvas _mainCanvas = null!;
        private GameObject _panelRoot = null!;
        private GameObject _scrollContent = null!;
        private GameObject _addPresetButton = null!;
        private TextMeshProUGUI _messageText = null!;
        private GameObject _messagePanel = null!;

        // 预设槽位 UI 对象列表
        private readonly List<GameObject> _presetSlotUIObjects = new List<GameObject>();

        // 消息计时器
        private float _messageTimer = 0f;

        // 预设管理器的引用
        private PresetManager _presetManager = null!;

        public Canvas MainCanvas => _mainCanvas;
        public GameObject PanelRoot => _panelRoot;
        public float MessageTimer { get => _messageTimer; set => _messageTimer = value; }

        /// <summary>
        /// 初始化官方样式的UI系统
        /// </summary>
        public void InitializeUI(PresetManager presetManager)
        {
            _presetManager = presetManager;

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
                UnityEngine.Object.DontDestroyOnLoad(canvasObj);

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
            _messageText = UnityEngine.Object.Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI);
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

            // 添加可拖动组件 (整个面板可拖动)
            _panelRoot.AddComponent<DraggableWindow>();
            Debug.Log("[PresetLoadout] Added DraggableWindow component to panel");

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
                _presetManager.PresetStorage.AddPreset();
                _presetManager.SaveConfig();
                RefreshPresetPanel();
                ShowMessage($"已添加预设 {_presetManager.PresetStorage.Presets.Count}");
            });

            // 添加按钮文本
            TextMeshProUGUI btnText = UnityEngine.Object.Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI);
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

            TextMeshProUGUI titleText = UnityEngine.Object.Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI);
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
        /// 刷新预设面板显示
        /// </summary>
        public void RefreshPresetPanel()
        {
            // 清除现有的预设槽位UI
            foreach (var uiObj in _presetSlotUIObjects)
            {
                if (uiObj != null)
                {
                    UnityEngine.Object.Destroy(uiObj);
                }
            }
            _presetSlotUIObjects.Clear();

            // 为每个预设创建UI
            for (int i = 0; i < _presetManager.PresetStorage.Presets.Count; i++)
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

            Debug.Log($"[PresetLoadout] Refreshed preset panel with {_presetManager.PresetStorage.Presets.Count} presets");
        }

        /// <summary>
        /// 更新滚动内容的高度
        /// </summary>
        private void UpdateScrollContentHeight()
        {
            if (_scrollContent != null)
            {
                RectTransform contentRect = _scrollContent.GetComponent<RectTransform>();
                int presetCount = _presetManager.PresetStorage.Presets.Count;
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
            PresetConfig preset = _presetManager.PresetStorage.Presets[index];

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

            TextMeshProUGUI nameText = UnityEngine.Object.Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI);
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
                _presetManager.SaveCurrentLoadout(index + 1, ShowMessage);
                RefreshPresetPanel(); // 刷新显示
            });

            // 创建应用按钮
            bool hasItems = preset.GetTotalItemCount() > 0;
            CreateButton(buttonsObj, "应用此预设", new Vector2(0.52f, 0.5f), new Vector2(0.74f, 1), () =>
            {
                _presetManager.ApplyLoadout(index + 1, ShowMessage);
            }, !hasItems);

            // 创建预览按钮
            CreateButton(buttonsObj, "预览", new Vector2(0.76f, 0.5f), new Vector2(1, 1), () =>
            {
                ShowPresetPreview(preset);
            }, !hasItems);

            // 创建删除按钮（如果预设数量大于3）
            if (_presetManager.PresetStorage.Presets.Count > 3)
            {
                CreateButton(buttonsObj, "删除", new Vector2(0, 0), new Vector2(1, 0.4f), () =>
                {
                    if (_presetManager.PresetStorage.RemovePreset(index))
                    {
                        _presetManager.SaveConfig();
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

            TextMeshProUGUI btnText = UnityEngine.Object.Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI);
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
        /// 显示消息 (使用官方TextMeshPro样式)
        /// </summary>
        public void ShowMessage(string message)
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
        /// 更新消息显示计时器
        /// </summary>
        public void UpdateMessageTimer(float deltaTime)
        {
            if (_messageTimer > 0f)
            {
                _messageTimer -= deltaTime;
                if (_messageTimer <= 0f)
                {
                    if (_messagePanel != null)
                    {
                        _messagePanel.SetActive(false);
                    }
                }
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

        /// <summary>
        /// 显示预设预览窗口（纯文本版本）
        /// </summary>
        public void ShowPresetPreview(PresetConfig preset)
        {
            try
            {
                // 获取对比结果
                PresetCompareResult compareResult = _presetManager.CompareWithCurrent(preset);

                // 创建预览窗口
                GameObject previewWindow = new GameObject("PresetPreviewWindow");
                previewWindow.transform.SetParent(_panelRoot.transform, false);

                RectTransform windowRect = previewWindow.AddComponent<RectTransform>();
                windowRect.sizeDelta = new Vector2(550, 650);
                windowRect.anchoredPosition = new Vector2(320, 0); // 显示在预设面板右侧

                // 添加背景
                Image background = previewWindow.AddComponent<Image>();
                background.sprite = CreateRoundedSprite(12);
                background.type = Image.Type.Sliced;
                background.color = new Color(0.06f, 0.10f, 0.16f, 0.95f);

                // 创建标题区域（固定在顶部）
                GameObject titleArea = new GameObject("TitleArea");
                titleArea.transform.SetParent(previewWindow.transform, false);
                RectTransform titleRect = titleArea.AddComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0, 1);
                titleRect.anchorMax = new Vector2(1, 1);
                titleRect.pivot = new Vector2(0.5f, 1);
                titleRect.sizeDelta = new Vector2(-20, 60);
                titleRect.anchoredPosition = new Vector2(0, -10);

                // 添加标题文本
                TextMeshProUGUI titleText = UnityEngine.Object.Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI);
                titleText.transform.SetParent(titleArea.transform, false);
                titleText.transform.localScale = Vector3.one;
                RectTransform titleTextRect = titleText.GetComponent<RectTransform>();
                titleTextRect.anchorMin = Vector2.zero;
                titleTextRect.anchorMax = Vector2.one;
                titleTextRect.offsetMin = Vector2.zero;
                titleTextRect.offsetMax = Vector2.zero;
                titleText.text = $"预设预览: {preset.PresetName}";
                titleText.fontSize = 20f;
                titleText.alignment = TextAlignmentOptions.Center;
                titleText.fontStyle = FontStyles.Bold;

                // 创建滚动区域
                GameObject scrollObj = new GameObject("ScrollView");
                scrollObj.transform.SetParent(previewWindow.transform, false);

                RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
                scrollRect.anchorMin = new Vector2(0, 0.12f);
                scrollRect.anchorMax = new Vector2(1, 0.88f);
                scrollRect.offsetMin = new Vector2(15, 10);
                scrollRect.offsetMax = new Vector2(-15, -10);

                // 创建内容容器
                GameObject contentObj = new GameObject("Content");
                contentObj.transform.SetParent(scrollObj.transform, false);

                RectTransform contentRect = contentObj.AddComponent<RectTransform>();
                contentRect.anchorMin = new Vector2(0, 1);
                contentRect.anchorMax = new Vector2(1, 1);
                contentRect.pivot = new Vector2(0.5f, 1);
                contentRect.sizeDelta = new Vector2(0, 0);
                contentRect.anchoredPosition = Vector2.zero;

                float yPos = -10f;
                float lineHeight = 26f; // 增加行高

                // 显示统计信息
                yPos = AddTextToPreview(contentObj, $"需要添加: {compareResult.ToAdd.Count}  |  移除: {compareResult.ToRemove.Count}  |  不变: {compareResult.Unchanged.Count}", 15, yPos);
                yPos -= lineHeight + 10f;

                // 只显示仓库缺少的物品（如果有）
                if (compareResult.MissingInStorage.Count > 0)
                {
                    yPos = AddTextToPreview(contentObj, "仓库缺少以下物品:", 17, yPos, true);
                    yPos -= lineHeight + 5f;

                    // 从 ToAdd 列表中筛选出缺少的物品
                    foreach (var itemInfo in compareResult.ToAdd)
                    {
                        if (compareResult.MissingInStorage.Contains(itemInfo.TypeID))
                        {
                            string itemText = $"  {itemInfo.DisplayName}  ({(itemInfo.IsEquipped ? "装备" : "背包")})";
                            yPos = AddTextToPreview(contentObj, itemText, 15, yPos);
                            yPos -= lineHeight;
                        }
                    }
                }
                else
                {
                    // 如果仓库物品充足，显示提示
                    yPos = AddTextToPreview(contentObj, "仓库物品充足，可以应用此预设", 16, yPos);
                    yPos -= lineHeight + 15f;

                    // 显示预设内容概览
                    yPos = AddTextToPreview(contentObj, "预设内容:", 17, yPos, true);
                    yPos -= lineHeight + 5f;

                    // 显示装备
                    if (compareResult.ToAdd.Count > 0 || compareResult.Unchanged.Count > 0)
                    {
                        var allEquipped = compareResult.ToAdd.Where(x => x.IsEquipped)
                            .Concat(compareResult.Unchanged.Where(x => x.IsEquipped))
                            .ToList();

                        if (allEquipped.Count > 0)
                        {
                            yPos = AddTextToPreview(contentObj, $"装备槽位 ({allEquipped.Count} 件):", 15, yPos);
                            yPos -= lineHeight;

                            foreach (var item in allEquipped.Take(5))
                            {
                                yPos = AddTextToPreview(contentObj, $"  {item.DisplayName}", 14, yPos);
                                yPos -= lineHeight;
                            }

                            if (allEquipped.Count > 5)
                            {
                                yPos = AddTextToPreview(contentObj, $"  ... 还有 {allEquipped.Count - 5} 件", 14, yPos);
                                yPos -= lineHeight;
                            }
                            yPos -= 5f;
                        }
                    }

                    // 显示背包
                    if (compareResult.ToAdd.Count > 0 || compareResult.Unchanged.Count > 0)
                    {
                        var allInventory = compareResult.ToAdd.Where(x => !x.IsEquipped)
                            .Concat(compareResult.Unchanged.Where(x => !x.IsEquipped))
                            .ToList();

                        if (allInventory.Count > 0)
                        {
                            yPos = AddTextToPreview(contentObj, $"背包物品 ({allInventory.Count} 件):", 15, yPos);
                            yPos -= lineHeight;

                            foreach (var item in allInventory.Take(5))
                            {
                                yPos = AddTextToPreview(contentObj, $"  {item.DisplayName}", 14, yPos);
                                yPos -= lineHeight;
                            }

                            if (allInventory.Count > 5)
                            {
                                yPos = AddTextToPreview(contentObj, $"  ... 还有 {allInventory.Count - 5} 件", 14, yPos);
                                yPos -= lineHeight;
                            }
                        }
                    }
                }

                // 设置内容高度
                contentRect.sizeDelta = new Vector2(0, Mathf.Abs(yPos) + 20f);

                // 添加关闭按钮
                CreateButton(previewWindow, "关闭", new Vector2(0.25f, 0.02f), new Vector2(0.75f, 0.10f), () =>
                {
                    UnityEngine.Object.Destroy(previewWindow);
                });

                Debug.Log($"[PresetLoadout] Preview window created for preset: {preset.PresetName}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PresetLoadout] Failed to show preset preview: {ex.Message}");
                ShowMessage("预览功能出错!");
            }
        }

        /// <summary>
        /// 在预览窗口中添加文本（使用固定行高）
        /// </summary>
        private float AddTextToPreview(GameObject parent, string text, int fontSize, float yPos, bool bold = false)
        {
            GameObject textObj = new GameObject($"Text_{Guid.NewGuid()}");
            textObj.transform.SetParent(parent.transform, false);

            TextMeshProUGUI tmpText = UnityEngine.Object.Instantiate(GameplayDataSettings.UIStyle.TemplateTextUGUI);
            tmpText.transform.SetParent(textObj.transform, false);
            tmpText.transform.localScale = Vector3.one;

            RectTransform textRect = tmpText.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 1);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.pivot = new Vector2(0, 1);
            textRect.anchoredPosition = new Vector2(10, yPos);
            textRect.localRotation = Quaternion.identity; // 确保没有旋转

            // 使用固定高度
            float lineHeight = fontSize * 1.6f;
            textRect.sizeDelta = new Vector2(-20, lineHeight);

            tmpText.text = text;
            tmpText.fontSize = fontSize;
            tmpText.alignment = TextAlignmentOptions.Left;
            tmpText.enableWordWrapping = false;
            tmpText.overflowMode = TextOverflowModes.Overflow;
            tmpText.horizontalAlignment = HorizontalAlignmentOptions.Left;
            tmpText.verticalAlignment = VerticalAlignmentOptions.Top;

            if (bold)
            {
                tmpText.fontStyle = FontStyles.Bold;
            }

            return yPos;
        }
    }
}
