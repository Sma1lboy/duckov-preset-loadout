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
        public List<int> ItemTypeIDs { get; set; } = new List<int>();

        public PresetConfig()
        {
        }

        public PresetConfig(string name)
        {
            PresetName = name;
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
