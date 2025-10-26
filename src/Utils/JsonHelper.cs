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

            for (int i = 0; i < storage.Presets.Count; i++)
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

                lines.Add("    }" + (i < storage.Presets.Count - 1 ? "," : ""));
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
                // 清空默认的3个预设，准备从JSON加载
                storage.Presets.Clear();

                // 查找所有预设块 "PresetName"
                int searchStart = 0;
                while (true)
                {
                    int presetNameIndex = json.IndexOf("\"PresetName\":", searchStart);
                    if (presetNameIndex == -1) break;

                    // 提取预设名称
                    int nameStart = json.IndexOf("\"", presetNameIndex + 13);
                    if (nameStart == -1) break;
                    nameStart++; // 跳过引号

                    int nameEnd = json.IndexOf("\"", nameStart);
                    if (nameEnd == -1 || nameEnd <= nameStart) break;

                    string presetName = json.Substring(nameStart, nameEnd - nameStart);
                    var preset = new PresetConfig(presetName);

                    // 查找下一个预设的位置（用于限制搜索范围）
                    int nextPresetIndex = json.IndexOf("\"PresetName\":", presetNameIndex + 13);
                    int searchLimit = nextPresetIndex != -1 ? nextPresetIndex : json.Length;

                    // 解析 EquippedItemTypeIDs
                    int equippedStart = json.IndexOf("\"EquippedItemTypeIDs\": [", presetNameIndex);
                    if (equippedStart != -1 && equippedStart < searchLimit)
                    {
                        int equippedEnd = json.IndexOf("]", equippedStart);
                        if (equippedEnd != -1 && equippedEnd > equippedStart + 24)
                        {
                            string equippedStr = json.Substring(equippedStart + 24, equippedEnd - equippedStart - 24);
                            preset.EquippedItemTypeIDs = ParseIntList(equippedStr);
                        }
                    }

                    // 解析 InventoryItemTypeIDs
                    int inventoryStart = json.IndexOf("\"InventoryItemTypeIDs\": [", presetNameIndex);
                    if (inventoryStart != -1 && inventoryStart < searchLimit)
                    {
                        int inventoryEnd = json.IndexOf("]", inventoryStart);
                        if (inventoryEnd != -1 && inventoryEnd > inventoryStart + 25)
                        {
                            string inventoryStr = json.Substring(inventoryStart + 25, inventoryEnd - inventoryStart - 25);
                            preset.InventoryItemTypeIDs = ParseIntList(inventoryStr);
                        }
                    }

                    storage.Presets.Add(preset);
                    searchStart = presetNameIndex + 13;
                }

                // 如果没有加载到任何预设，使用默认的3个
                if (storage.Presets.Count == 0)
                {
                    storage.Presets.Add(new PresetConfig("预设 1"));
                    storage.Presets.Add(new PresetConfig("预设 2"));
                    storage.Presets.Add(new PresetConfig("预设 3"));
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
