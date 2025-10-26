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

## [1.1.0] - 2025-10-26

### 新增 / New Features
- ✨ 预设对比和预览功能 - 应用预设前可查看将要装备的物品差异
  - Preset comparison and preview - View item differences before applying presets
- ✨ 预设管理系统 - 新增 PresetManager 类统一管理预设操作
  - Preset management system - New PresetManager class for unified preset operations
- ✨ 可拖动窗口 - 预设面板支持鼠标拖动
  - Draggable window - Preset panel supports mouse dragging
- ✨ 物品操作工具 - 新增 ItemOperations 类封装物品查询和移动逻辑
  - Item operations utility - New ItemOperations class for item queries and movement
- ✨ UI助手类 - 新增 UIHelper 类管理UI交互
  - UI helper class - New UIHelper class for managing UI interactions
- ✨ 场景工具类 - 新增 SceneUtils 类检测游戏状态
  - Scene utility class - New SceneUtils class for detecting game state

### 改进 / Improvements
- 🏗️ 代码架构重构 - 将单文件 ModBehaviour 拆分为多个模块化组件
  - Code architecture refactoring - Split single ModBehaviour into modular components
- 🎨 UI体验优化 - 改进面板显示和交互逻辑
  - UI experience optimization - Improved panel display and interaction logic
- 📦 发布流程改进 - 添加自动化发布命令 `/release`
  - Release process improvement - Added automated release command `/release`

### 文档 / Documentation
- 📝 更新开发文档 - 同步版本说明到 v1.2
  - Updated development docs - Synced version notes to v1.2
- 📝 添加发布流程文档 - `.claude/commands/release.md`
  - Added release process documentation - `.claude/commands/release.md`

---

## [1.0.1] - 2025-10-25

### 改进
- 📦 添加自动化版本发布系统
- 🔧 改进发布脚本，支持从 CHANGELOG 自动生成 Release Notes
- 📝 精简项目文档

### 修复
- 修复文档冗余问题

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

### 安装方法
1. 下载 `PresetLoadout-v1.0.0.zip`
2. 解压到 Mod 目录
3. 在游戏中启用 Mod

详见 [INSTALL.md](https://github.com/Sma1lboy/duckov-preset-loadout/blob/main/INSTALL.md)

---

## [0.1.0] - 2025-10-25

### 新增
- 初始开发版本
- 基本的预设保存和应用功能

---

[Unreleased]: https://github.com/Sma1lboy/duckov-preset-loadout/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/Sma1lboy/duckov-preset-loadout/releases/tag/v1.0.0
[0.1.0]: https://github.com/Sma1lboy/duckov-preset-loadout/releases/tag/v0.1.0
