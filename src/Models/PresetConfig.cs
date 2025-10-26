using System;
using System.Collections.Generic;
using ItemStatsSystem;
using UnityEngine;

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

        /// <summary>
        /// 获取物品显示名称（使用游戏 API）
        /// </summary>
        private static string GetItemDisplayName(int typeID)
        {
            try
            {
                Item tempItem = ItemAssetsCollection.InstantiateSync(typeID);
                if (tempItem != null)
                {
                    string name = tempItem.DisplayName;
                    UnityEngine.Object.Destroy(tempItem.gameObject);
                    return name;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[PresetLoadout] Failed to get item name for TypeID {typeID}: {ex.Message}");
            }
            return $"物品 {typeID}";
        }

        /// <summary>
        /// 获取预设中所有物品的详细信息
        /// </summary>
        public List<PresetItemInfo> GetItemInfos()
        {
            List<PresetItemInfo> infos = new List<PresetItemInfo>();

            // 添加装备物品
            if (EquippedItemTypeIDs != null)
            {
                foreach (int typeID in EquippedItemTypeIDs)
                {
                    infos.Add(new PresetItemInfo
                    {
                        TypeID = typeID,
                        IsEquipped = true,
                        DisplayName = GetItemDisplayName(typeID)
                    });
                }
            }

            // 添加背包物品
            if (InventoryItemTypeIDs != null)
            {
                foreach (int typeID in InventoryItemTypeIDs)
                {
                    infos.Add(new PresetItemInfo
                    {
                        TypeID = typeID,
                        IsEquipped = false,
                        DisplayName = GetItemDisplayName(typeID)
                    });
                }
            }

            return infos;
        }
    }

    /// <summary>
    /// 预设物品信息
    /// </summary>
    public class PresetItemInfo
    {
        public int TypeID;
        public bool IsEquipped; // true=装备, false=背包
        public string DisplayName;
    }

    /// <summary>
    /// 预设对比结果
    /// </summary>
    public class PresetCompareResult
    {
        public List<PresetItemInfo> ToAdd = new List<PresetItemInfo>();      // 需要添加的物品
        public List<PresetItemInfo> ToRemove = new List<PresetItemInfo>();   // 需要移除的物品
        public List<PresetItemInfo> Unchanged = new List<PresetItemInfo>();  // 不变的物品
        public List<int> MissingInStorage = new List<int>();                 // 仓库中缺少的物品

        /// <summary>
        /// 获取对比结果的文本描述
        /// </summary>
        public string GetSummary()
        {
            string summary = $"需要添加: {ToAdd.Count} | 需要移除: {ToRemove.Count} | 不变: {Unchanged.Count}";
            if (MissingInStorage.Count > 0)
            {
                summary += $"\n[警告] 仓库缺少 {MissingInStorage.Count} 个物品";
            }
            return summary;
        }
    }

    /// <summary>
    /// 存储所有预设的容器
    /// </summary>
    [Serializable]
    public class PresetStorage
    {
        // 使用 List 支持动态添加/删除
        public List<PresetConfig> Presets = new List<PresetConfig>();

        public PresetStorage()
        {
            // 初始化至少 3 个预设
            if (Presets == null || Presets.Count == 0)
            {
                Presets = new List<PresetConfig>
                {
                    new PresetConfig("预设 1"),
                    new PresetConfig("预设 2"),
                    new PresetConfig("预设 3")
                };
            }
        }

        /// <summary>
        /// 添加新预设
        /// </summary>
        public void AddPreset()
        {
            int nextNumber = Presets.Count + 1;
            Presets.Add(new PresetConfig($"预设 {nextNumber}"));
        }

        /// <summary>
        /// 删除预设（但至少保留 3 个）
        /// </summary>
        public bool RemovePreset(int index)
        {
            if (Presets.Count <= 3 || index < 0 || index >= Presets.Count)
            {
                return false;
            }
            Presets.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// 重命名预设
        /// </summary>
        public bool RenamePreset(int index, string newName)
        {
            if (index < 0 || index >= Presets.Count || string.IsNullOrWhiteSpace(newName))
            {
                return false;
            }
            Presets[index].PresetName = newName;
            return true;
        }
    }
}
