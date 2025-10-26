# PresetLoadout Mod 开发说明

## 📦 项目简介

**PresetLoadout** - 装备预设系统

一个用于《逃离鸭科夫》的装备预设管理 Mod，允许玩家保存和快速切换装备配置。

---

## 🚀 快速部署

### 编译 + 部署到游戏

```bash
./scripts/deploy.sh
```

这个脚本会：
1. ✅ 编译 Mod (Release 模式)
2. ✅ 自动部署到 Workshop 目录 (ID: 3591339491)
3. ✅ 显示部署结果

**⚠️ 注意：** 部署后需要**重启游戏**才能看到更新

### 实时查看游戏日志

```bash
./scripts/watch-log.sh
```

---

## 🎯 主要功能

### 1. 保存预设
- 保存当前身上的装备和背包物品
- 支持多个预设槽位（默认 3 个，可扩展）

### 2. 应用预设
- **Step 0**: 自动将当前装备放回仓库
- **Step 1**: 装备预设中的装备（护甲、背包等）
- **Step 2**: 将预设中的背包物品放入背包

### 3. GUI 面板
- 按 **L 键** 打开/关闭面板（需要先打开背包）
- 支持重命名预设
- 支持添加/删除预设（最少保留 3 个）

---

## 🗂️ 文件结构

```
PresetLoadout/
├── ModBehaviour.cs          # 主逻辑
├── PresetConfig.cs          # 数据结构
├── JsonHelper.cs            # JSON 序列化工具
├── PresetLoadout.csproj     # 项目配置
├── deploy.sh                # 快速部署脚本
├── info.ini                 # Mod 信息
├── README.md                # 功能说明
└── INSTALL.md               # 安装指南
```

---

## 🔧 开发工作流

### 1. 修改代码
编辑 `ModBehaviour.cs` 或其他文件

### 2. 部署测试
```bash
./deploy.sh
```

### 3. 重启游戏测试
- 重启游戏
- 打开背包 (Tab)
- 按 **L** 打开预设面板
- 测试功能

### 4. 查看日志
```bash
tail -f ~/Library/Logs/TeamSoda/Duckov/Player.log
```

---

## 🛠️ 关键 API 使用

### 物品管理
```csharp
// 获取玩家身上的物品
item.IsInPlayerCharacter()

// 获取仓库中的物品
item.IsInPlayerStorage()

// 发送到角色（尝试装备）
ItemUtilities.SendToPlayerCharacter(item, dontMerge: true)

// 发送到背包 Inventory
ItemUtilities.SendToPlayerCharacterInventory(item, dontMerge: false)

// 发送到仓库
ItemUtilities.SendToPlayerStorage(item, directToBuffer: false)
```

### 数据持久化
```csharp
// 序列化
string json = JsonHelper.SerializePresetStorage(presetStorage);

// 反序列化
PresetStorage storage = JsonHelper.DeserializePresetStorage(json);
```

---

## 📋 配置文件位置

**配置文件**:
```
~/Library/Application Support/Unity/com.TeamSoda.Duckov/PresetLoadout_Config.json
```

**部署目标**:
```
~/Library/Application Support/Steam/steamapps/workshop/content/3167020/3591339491/
```

---

## 🐛 常见问题

### 编译失败
```bash
dotnet clean
dotnet build -c Release
```

### Mod 不加载
- 检查 Workshop 目录中是否有 `PresetLoadout.dll`
- 查看游戏日志是否有错误信息

### 应用预设失败
- 确保仓库中有足够的物品
- 查看日志输出了解具体失败原因

---

## 📝 待优化功能

- [ ] 支持预设导入/导出
- [ ] 支持从不同预设复制配置
- [ ] 添加预设预览功能
- [ ] 优化 UI 布局和样式

---

**最后更新**: 2025-10-25
**当前版本**: 1.0
**开发状态**: ✅ 可用
