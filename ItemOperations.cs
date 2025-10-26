using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ItemStatsSystem;

namespace PresetLoadout
{
    /// <summary>
    /// 物品操作工具类
    /// 负责获取玩家物品、仓库物品、判断物品位置等操作
    /// </summary>
    public static class ItemOperations
    {
        /// <summary>
        /// 获取玩家身上的所有物品
        /// </summary>
        public static List<Item> GetPlayerItems()
        {
            List<Item> items = new List<Item>();

            try
            {
                // 查找场景中所有的 Item 对象
                Item[] allItems = UnityEngine.Object.FindObjectsOfType<Item>();

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
        public static List<Item> GetPlayerStorageItems()
        {
            List<Item> items = new List<Item>();

            try
            {
                // 查找场景中所有的 Item 对象
                Item[] allItems = UnityEngine.Object.FindObjectsOfType<Item>();

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
        /// 判断物品是否在背包中（而不是装备槽位）
        /// </summary>
        public static bool IsItemInInventory(Item item)
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
        /// 将玩家身上的所有物品发送到仓库
        /// </summary>
        /// <returns>移动的物品数量</returns>
        public static int SendAllPlayerItemsToStorage()
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
    }
}
