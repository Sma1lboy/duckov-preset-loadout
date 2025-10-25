using System;
using System.Collections.Generic;

namespace PresetLoadout
{
    /// <summary>
    /// 手动 JSON 序列化工具类
    /// 因为 Unity 的 JsonUtility 在某些环境下无法正确序列化我们的数据结构
    /// </summary>
    public static class JsonHelper
    {
        /// <summary>
        /// 序列化 PresetStorage 为 JSON 字符串
        /// </summary>
        public static string SerializePresetStorage(PresetStorage storage)
        {
            if (storage == null || storage.Presets == null)
            {
                return "{}";
            }

            var lines = new List<string>();
            lines.Add("{");
            lines.Add("  \"Presets\": [");

            for (int i = 0; i < storage.Presets.Length; i++)
            {
                var preset = storage.Presets[i];
                if (preset == null) continue;

                lines.Add("    {");
                lines.Add($"      \"PresetName\": \"{EscapeString(preset.PresetName)}\",");

                // EquippedItemTypeIDs
                lines.Add("      \"EquippedItemTypeIDs\": [" +
                    SerializeIntList(preset.EquippedItemTypeIDs) + "],");

                // InventoryItemTypeIDs
                lines.Add("      \"InventoryItemTypeIDs\": [" +
                    SerializeIntList(preset.InventoryItemTypeIDs) + "]");

                lines.Add("    }" + (i < storage.Presets.Length - 1 ? "," : ""));
            }

            lines.Add("  ]");
            lines.Add("}");

            return string.Join("\n", lines);
        }

        /// <summary>
        /// 反序列化 JSON 字符串为 PresetStorage
        /// </summary>
        public static PresetStorage DeserializePresetStorage(string json)
        {
            var storage = new PresetStorage();

            if (string.IsNullOrWhiteSpace(json) || json == "{}")
            {
                return storage;
            }

            try
            {
                // 简单的手动解析：查找每个预设
                for (int i = 0; i < 3; i++)
                {
                    int presetIndex = json.IndexOf($"\"PresetName\": \"预设 {i + 1}\"");
                    if (presetIndex == -1) continue;

                    var preset = storage.Presets[i];

                    // 解析 EquippedItemTypeIDs
                    int equippedStart = json.IndexOf("\"EquippedItemTypeIDs\": [", presetIndex);
                    if (equippedStart != -1)
                    {
                        int equippedEnd = json.IndexOf("]", equippedStart);
                        if (equippedEnd != -1)
                        {
                            string equippedStr = json.Substring(equippedStart + 24, equippedEnd - equippedStart - 24);
                            preset.EquippedItemTypeIDs = ParseIntList(equippedStr);
                        }
                    }

                    // 解析 InventoryItemTypeIDs
                    int inventoryStart = json.IndexOf("\"InventoryItemTypeIDs\": [", presetIndex);
                    if (inventoryStart != -1)
                    {
                        int inventoryEnd = json.IndexOf("]", inventoryStart);
                        if (inventoryEnd != -1)
                        {
                            string inventoryStr = json.Substring(inventoryStart + 25, inventoryEnd - inventoryStart - 25);
                            preset.InventoryItemTypeIDs = ParseIntList(inventoryStr);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // 解析失败，返回空的 storage
                return new PresetStorage();
            }

            return storage;
        }

        /// <summary>
        /// 序列化整数列表为字符串 "1, 2, 3"
        /// </summary>
        private static string SerializeIntList(List<int> list)
        {
            if (list == null || list.Count == 0)
            {
                return "";
            }
            return string.Join(", ", list);
        }

        /// <summary>
        /// 解析整数列表字符串 "1, 2, 3" -> List<int>
        /// </summary>
        private static List<int> ParseIntList(string str)
        {
            var result = new List<int>();
            if (string.IsNullOrWhiteSpace(str))
            {
                return result;
            }

            var parts = str.Split(',');
            foreach (var part in parts)
            {
                if (int.TryParse(part.Trim(), out int value))
                {
                    result.Add(value);
                }
            }
            return result;
        }

        /// <summary>
        /// 转义 JSON 字符串中的特殊字符
        /// </summary>
        private static string EscapeString(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }

            return str
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }
    }
}
