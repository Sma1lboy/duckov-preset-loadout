# Changelog

所有重要的变更都会记录在这个文件中。

本项目遵循 [语义化版本控制](https://semver.org/lang/zh-CN/)。

---

## [Unreleased]

### 计划添加
- 预设数据持久化 (JSON 文件存储)
- 自定义预设名称
- 更多预设槽位 (可配置)
- 预设导入/导出功能
- 装备预览界面

---

## [1.0.0] - 2025-10-25

### 新增
- 🎉 初始版本发布
- ✨ 支持 3 个预设槽位
- ⌨️ Ctrl+1/2/3 保存当前装备到预设
- 🚀 1/2/3 快速应用预设
- 🎯 智能装备匹配系统
  - 自动从角色身上和仓库中查找装备
  - 优先使用角色身上的装备
  - 缺少装备时提供提示
- 💬 可视化反馈系统
  - 保存成功提示
  - 应用成功提示
  - 缺少装备警告
- ❓ 帮助系统 (按 H 键显示)
- 📋 预设内容显示
  - 显示每个预设包含的装备
  - 实时更新预设状态

### 技术特性
- 基于 Unity 输入系统
- 使用 TeamSoda Modding API
- .NET Standard 2.1
- 支持 Windows 和 macOS

### 文档
- 添加 README.md (功能说明)
- 添加 INSTALL.md (安装指南)
- 添加 RELEASE.md (发布指南)
- 添加脚本文档

---

## [0.1.0] - 2025-10-25

### 新增
- 初始开发版本
- 基本的预设保存和应用功能

---

[Unreleased]: https://github.com/Sma1lboy/duckov-preset-loadout/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/Sma1lboy/duckov-preset-loadout/releases/tag/v1.0.0
[0.1.0]: https://github.com/Sma1lboy/duckov-preset-loadout/releases/tag/v0.1.0
