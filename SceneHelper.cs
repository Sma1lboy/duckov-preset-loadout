using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PresetLoadout
{
    /// <summary>
    /// 场景检测辅助类
    /// </summary>
    public static class SceneHelper
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

        /// <summary>
        /// 打印当前场景信息（调试用）
        /// </summary>
        public static void PrintSceneInfo()
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
    }
}
