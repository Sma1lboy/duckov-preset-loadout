using System;
using System.IO;
using UnityEngine;

namespace PresetLoadout
{
    /// <summary>
    /// PresetLoadout Mod 主入口
    /// 负责生命周期管理和协调各个组件
    /// </summary>
    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        // 配置文件路径
        private string ConfigFilePath => Path.Combine(Application.persistentDataPath, "PresetLoadout_Config.json");

        // 组件管理器
        private PresetManager _presetManager = null!;
        private UIHelper _uiHelper = null!;

        // 面板状态
        private bool _showWindow = false;
        private bool _userManuallyClosedPanel = false; // 用户是否手动关闭了面板
        private bool _wasInventoryOpen = false; // 上一帧背包是否打开
        private readonly bool _requireInventoryOpen = true; // 是否要求背包打开才能使用面板

        void Awake()
        {
            Debug.Log("PresetLoadout Mod Loaded!");

            // 初始化预设管理器
            _presetManager = new PresetManager(ConfigFilePath);

            // 初始化UI
            _uiHelper = new UIHelper();
            _uiHelper.InitializeUI(_presetManager);
        }

        void Start()
        {
        }

        void Update()
        {
            // 更新消息显示计时器
            _uiHelper.UpdateMessageTimer(Time.deltaTime);

            // 检测背包打开/关闭状态变化
            bool isInventoryOpen = IsInventoryUIOpen();
            bool inventoryStateChanged = isInventoryOpen != _wasInventoryOpen;

            if (inventoryStateChanged)
            {
                _wasInventoryOpen = isInventoryOpen;

                if (isInventoryOpen)
                {
                    // 背包刚打开
                    Debug.Log("[PresetLoadout] Inventory opened");

                    // 如果用户没有手动关闭过，且在家/仓库可用，则自动打开面板
                    if (!_userManuallyClosedPanel && SceneUtils.IsStorageAvailable())
                    {
                        _showWindow = true;
                        if (_uiHelper.PanelRoot != null)
                        {
                            _uiHelper.PanelRoot.SetActive(true);
                            _uiHelper.RefreshPresetPanel();
                        }
                        Debug.Log("[PresetLoadout] Auto-opened preset panel");
                    }
                }
                else
                {
                    // 背包刚关闭
                    Debug.Log("[PresetLoadout] Inventory closed");

                    // 自动关闭面板，并重置"手动关闭"标记
                    if (_showWindow)
                    {
                        _showWindow = false;
                        if (_uiHelper.PanelRoot != null)
                        {
                            _uiHelper.PanelRoot.SetActive(false);
                        }
                        Debug.Log("[PresetLoadout] Auto-closed preset panel");
                    }

                    // 重置手动关闭标记
                    _userManuallyClosedPanel = false;
                }
            }

            // 打开/关闭面板: L 键（手动控制）
            if (Input.GetKeyDown(KeyCode.L))
            {
                // 如果当前面板是关闭的，需要检查是否允许打开
                if (!_showWindow)
                {
                    if (!ShouldOpenPanel())
                    {
                        _uiHelper.ShowMessage($"只能在家中使用装备预设");
                        return;
                    }
                }

                _showWindow = !_showWindow;

                // 记录用户手动关闭的状态
                if (!_showWindow)
                {
                    _userManuallyClosedPanel = true;
                    Debug.Log("[PresetLoadout] User manually closed panel");
                }
                else
                {
                    _userManuallyClosedPanel = false;
                    Debug.Log("[PresetLoadout] User manually opened panel");
                }

                if (_uiHelper.PanelRoot != null)
                {
                    _uiHelper.PanelRoot.SetActive(_showWindow);

                    // 如果打开面板，刷新预设显示
                    if (_showWindow)
                    {
                        _uiHelper.RefreshPresetPanel();
                    }
                }

                Debug.Log($"[PresetLoadout] Preset Panel: {(_showWindow ? "Opened" : "Closed")}");
                _uiHelper.ShowMessage(_showWindow ? "已打开装备预设面板 (按L关闭)" : "已关闭装备预设面板");
            }

            // F6 键：调试 - 打印当前场景信息
            if (Input.GetKeyDown(KeyCode.F6))
            {
                SceneUtils.PrintCurrentSceneInfo();
                _uiHelper.ShowMessage($"当前场景: {SceneUtils.GetCurrentSceneName()}");
            }
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

        /// <summary>
        /// 判断是否应该允许打开面板
        /// </summary>
        private bool ShouldOpenPanel()
        {
            // 必须在家的场景中
            if (!SceneUtils.IsInHomeScene())
            {
                Debug.Log("[PresetLoadout] Cannot open panel: Not in home scene (Base_SceneV2)");
                return false;
            }

            // 如果要求背包打开，则检查背包状态
            if (_requireInventoryOpen)
            {
                bool inventoryOpen = IsInventoryUIOpen();
                if (!inventoryOpen)
                {
                    Debug.Log("[PresetLoadout] Cannot open panel: Inventory UI not open");
                    return false;
                }
            }

            // 检查仓库是否可用
            if (!SceneUtils.IsStorageAvailable())
            {
                Debug.Log("[PresetLoadout] Cannot open panel: Storage not available");
                return false;
            }

            return true;
        }

        void OnDestroy()
        {
            Debug.Log("PresetLoadout Mod Unloaded");
        }
    }
}
