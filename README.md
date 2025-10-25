# PresetLoadout - 装备预设系统

一个用于《逃离鸭科夫》的 Mod，允许玩家快速保存和应用装备配置。

## 功能特性

- ✅ **保存装备预设**: 保存当前身上的所有装备到 3 个预设槽位
- ✅ **快速应用预设**: 从仓库中自动获取装备并装配
- ✅ **智能检查**: 只装备仓库中存在的物品，避免错误
- ✅ **可视化反馈**: 使用游戏内对话气泡显示操作结果
- ✅ **持久化存储**: 预设配置自动保存到文件，重启游戏后依然有效

## 使用方法

### 打开面板
- 按 **P** 键打开/关闭装备预设面板

### 保存预设
1. 装备好你想要保存的装备（装备槽位 + 背包）
2. 在预设面板中点击"保存当前"按钮
3. 或按 **Ctrl + 1/2/3** 快捷键保存到对应槽位
4. 游戏会显示"✓ 预设 X 已保存 (装备:N 背包:M)"

### 应用预设
1. 确保仓库中有足够的装备
2. 在预设面板中点击"应用此预设"按钮
3. 或按 **1/2/3** 快捷键应用对应的预设
4. 系统会：
   - 优先装备"装备槽位"物品
   - 然后放入"背包"物品
5. 游戏会显示成功和失败的数量

## 快捷键一览

| 快捷键 | 功能 |
|--------|------|
| P | 打开/关闭预设面板 |
| Ctrl + 1 | 保存预设 1 |
| Ctrl + 2 | 保存预设 2 |
| Ctrl + 3 | 保存预设 3 |
| 1 | 应用预设 1 |
| 2 | 应用预设 2 |
| 3 | 应用预设 3 |

## 安装方法

### 方法 1: 使用编译好的版本
1. 下载发布的 `PresetLoadout` 文件夹
2. 将文件夹复制到游戏目录: `<游戏安装路径>/Duckov_Data/Mods/`
3. 启动游戏，在 Mods 界面启用此 Mod

### 方法 2: 从源码编译
1. 打开 `PresetLoadout.csproj`
2. 修改 `<DuckovPath>` 为你的游戏安装路径
3. 运行快速部署脚本:
   ```bash
   ./scripts/deploy.sh
   ```
   或手动编译:
   ```bash
   dotnet build -c Release
   ```
4. 编译后的 DLL 在 `bin/Release/netstandard2.1/PresetLoadout.dll`
5. 创建发布文件夹并复制以下文件:
   ```
   PresetLoadout/
   ├── PresetLoadout.dll
   ├── info.ini
   └── preview.png
   ```

## 开发工具

详细的开发脚本和工作流，请参考 [scripts/README.md](scripts/README.md)。

**快速开发命令**:
- `./scripts/deploy.sh` - 编译并部署
- `./scripts/watch-log.sh` - 实时查看游戏日志

## 技术细节

### 项目结构
```
PresetLoadout/
├── ModBehaviour.cs       # 主逻辑：UI、按键处理、预设管理
├── PresetConfig.cs       # 数据模型：PresetConfig 和 PresetStorage
├── JsonHelper.cs         # JSON 序列化工具（手动实现）
├── scripts/              # 开发脚本
│   ├── deploy.sh         # 快速编译部署
│   ├── watch-log.sh      # 实时日志监控
│   └── README.md         # 脚本文档
├── PresetLoadout.csproj  # 项目配置
└── README.md             # 本文件
```

### 数据存储
- **配置文件位置**: `~/Library/Application Support/TeamSoda/Duckov/PresetLoadout_Config.json` (macOS)
- **格式**: 手动实现的 JSON 序列化（Unity 的 JsonUtility 不支持当前数据结构）
- **数据结构**:
  - 每个预设分为两部分：
    - `EquippedItemTypeIDs` - 装备槽位的物品
    - `InventoryItemTypeIDs` - 背包中的物品

### 工作原理
1. **保存预设**:
   - 扫描玩家角色身上的所有物品
   - 通过 `IsItemInInventory()` 判断物品在装备槽位还是背包
   - 分别保存到 `EquippedItemTypeIDs` 和 `InventoryItemTypeIDs`

2. **应用预设**:
   - 优先应用装备槽位物品（使用 `SendToPlayerCharacter`）
   - 然后应用背包物品（使用 `SendToPlayerCharacterInventory`）
   - 从仓库中查找匹配物品并转移

3. **序列化**:
   - 使用 `JsonHelper` 手动构建/解析 JSON
   - 避开 Unity JsonUtility 的限制

## 注意事项

- ⚠️ 应用预设时，如果仓库中没有某个物品，该物品会被跳过
- ⚠️ 预设只保存物品的 TypeID，不保存物品的具体配置（如枪械配件）
- ⚠️ 如果角色身上装备栏已满，可能会导致部分物品无法装备

- [ ] 添加配件支持（保存枪械的配件配置）
- [ ] 添加更多预设槽位
- [ ] 添加预设重命名功能
- [ ] 添加导入/导出预设功能
- [ ] 添加自动应用预设（如：出门时自动应用）

## 许可证

本 Mod 遵循《鸭科夫社区准则》。

## 反馈与支持

如遇问题或有建议，请在 GitHub 上提 Issue。
