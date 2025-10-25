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
        public string PresetName { get; set; } = "";

        /// <summary>
        /// 装备在角色身上的物品 (装备槽位)
        /// 应用时优先装备这些物品
        /// </summary>
        public List<int> EquippedItemTypeIDs { get; set; } = new List<int>();

        /// <summary>
        /// 在背包中的物品
        /// 应用时放入背包
        /// </summary>
        public List<int> InventoryItemTypeIDs { get; set; } = new List<int>();

        // 为了兼容旧版本配置，保留这个字段但标记为过时
        [Obsolete("Use EquippedItemTypeIDs and InventoryItemTypeIDs instead")]
        public List<int> ItemTypeIDs { get; set; } = new List<int>();

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
        public Dictionary<int, PresetConfig> Presets { get; set; } = new Dictionary<int, PresetConfig>
        {
            { 1, new PresetConfig("预设 1") },
            { 2, new PresetConfig("预设 2") },
            { 3, new PresetConfig("预设 3") }
        };
    }
}
