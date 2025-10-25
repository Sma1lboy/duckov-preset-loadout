using System;
using System.Collections.Generic;

namespace PresetLoadout
{
    /// <summary>
    /// 装备预设配置数据结构
    /// </summary>
    [Serializable]
    public class PresetConfig
    {
        // Unity JsonUtility 只支持公共字段，不支持属性！
        public string PresetName = "";

        /// <summary>
        /// 装备在角色身上的物品 (装备槽位)
        /// 应用时优先装备这些物品
        /// </summary>
        public List<int> EquippedItemTypeIDs = new List<int>();

        /// <summary>
        /// 在背包中的物品
        /// 应用时放入背包
        /// </summary>
        public List<int> InventoryItemTypeIDs = new List<int>();

        public PresetConfig()
        {
        }

        public PresetConfig(string name)
        {
            PresetName = name;
        }

        /// <summary>
        /// 获取总物品数量
        /// </summary>
        public int GetTotalItemCount()
        {
            return (EquippedItemTypeIDs?.Count ?? 0) + (InventoryItemTypeIDs?.Count ?? 0);
        }

        /// <summary>
        /// 获取装备描述
        /// </summary>
        public string GetDescription()
        {
            int equippedCount = EquippedItemTypeIDs?.Count ?? 0;
            int inventoryCount = InventoryItemTypeIDs?.Count ?? 0;

            if (equippedCount == 0 && inventoryCount == 0)
                return "空";

            return $"装备:{equippedCount} 背包:{inventoryCount}";
        }
    }

    /// <summary>
    /// 存储所有预设的容器
    /// </summary>
    [Serializable]
    public class PresetStorage
    {
        // 使用数组而不是 Dictionary，因为 Unity 的 JsonUtility 不支持 Dictionary
        // 必须先声明为 null，然后在构造函数中初始化，否则 JsonUtility 无法序列化
        public PresetConfig[] Presets;

        public PresetStorage()
        {
            // 在构造函数中初始化
            Presets = new PresetConfig[3];
            Presets[0] = new PresetConfig("预设 1");
            Presets[1] = new PresetConfig("预设 2");
            Presets[2] = new PresetConfig("预设 3");
        }

        /// <summary>
        /// 获取预设 (1-indexed)
        /// </summary>
        public PresetConfig GetPreset(int slotNumber)
        {
            if (slotNumber < 1 || slotNumber > Presets.Length)
                return null;
            return Presets[slotNumber - 1];
        }

        /// <summary>
        /// 设置预设 (1-indexed)
        /// </summary>
        public void SetPreset(int slotNumber, PresetConfig preset)
        {
            if (slotNumber < 1 || slotNumber > Presets.Length)
                return;
            Presets[slotNumber - 1] = preset;
        }
    }
}
