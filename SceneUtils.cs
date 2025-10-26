using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PresetLoadout
{
    /// <summary>
    /// 场景检测工具类
    /// 负责检测当前场景状态，判断玩家是否在家、仓库是否可用等
    /// </summary>
    public static class SceneUtils
    {
        /// <summary>
        /// 检测是否在家的场景
        /// </summary>
        public static bool IsInHomeScene()
        {
            try
            {
                Scene activeScene = SceneManager.GetActiveScene();
                string sceneName = activeScene.name;

                // 检测是否在家的场景
                // 家的场景名称: Base_SceneV2
                bool isInHome = sceneName.Contains("Base_Scene") ||
                               sceneName.Equals("Base_SceneV2", StringComparison.OrdinalIgnoreCase) ||
                               sceneName.Contains("Home") ||
                               sceneName.Contains("Hideout") ||
                               sceneName.Contains("SafeHouse");

                return isInHome;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PresetLoadout] Error checking home scene: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 检测仓库是否可用（通过场景检测玩家是否在家）
        /// </summary>
        public static bool IsStorageAvailable()
        {
            try
            {
                Scene activeScene = SceneManager.GetActiveScene();
                string sceneName = activeScene.name;

                bool isInHome = IsInHomeScene();

                Debug.Log($"[PresetLoadout] Scene: {sceneName}, IsHome: {isInHome}");

                // 如果不在家，检查是否有仓库物品可用（可能在野外打开了仓库箱子）
                if (!isInHome)
                {
                    var storageItems = ItemOperations.GetPlayerStorageItems();
                    bool hasStorage = storageItems != null && storageItems.Count > 0;
                    Debug.Log($"[PresetLoadout] Not in home, but storage items available: {hasStorage}");
                    return hasStorage;
                }

                return isInHome;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PresetLoadout] Error checking storage availability: {ex.Message}");
                return true; // 如果检测失败，默认认为可用
            }
        }

        /// <summary>
        /// 打印当前场景信息（调试用）
        /// </summary>
        public static void PrintCurrentSceneInfo()
        {
            try
            {
                Scene activeScene = SceneManager.GetActiveScene();
                int sceneCount = SceneManager.sceneCount;

                Debug.Log("========================================");
                Debug.Log("[PresetLoadout] 场景信息:");
                Debug.Log($"  当前激活场景: {activeScene.name}");
                Debug.Log($"  场景路径: {activeScene.path}");
                Debug.Log($"  场景索引: {activeScene.buildIndex}");
                Debug.Log($"  已加载场景数: {sceneCount}");

                for (int i = 0; i < sceneCount; i++)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    Debug.Log($"  场景 {i}: {scene.name} (已加载: {scene.isLoaded})");
                }

                Debug.Log("========================================");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[PresetLoadout] Error printing scene info: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取当前场景名称
        /// </summary>
        public static string GetCurrentSceneName()
        {
            try
            {
                return SceneManager.GetActiveScene().name;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PresetLoadout] Error getting scene name: {ex.Message}");
                return "Unknown";
            }
        }
    }
}
