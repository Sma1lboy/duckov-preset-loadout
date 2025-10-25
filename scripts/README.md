# PresetLoadout 开发脚本

这个目录包含用于开发和调试 PresetLoadout Mod 的实用脚本。

## 脚本列表

### 📦 deploy.sh
**快速编译和部署脚本**

自动完成编译并部署到游戏目录的完整流程。

**使用方法**:
```bash
cd /Volumes/ssd/i/duckov/PresetLoadout
./scripts/deploy.sh
```

或者从任何位置：
```bash
/Volumes/ssd/i/duckov/PresetLoadout/scripts/deploy.sh
```

**功能**:
1. 编译 Mod (`dotnet build -c Release`)
2. 复制 DLL 到 Steam Workshop 目录
3. 显示部署状态

**输出**:
- ✅ 编译成功并部署
- ❌ 编译失败并显示错误

---

### 📋 watch-log.sh
**实时日志监控脚本**

实时查看游戏日志，方便调试 Mod。

**使用方法**:
```bash
./scripts/watch-log.sh
```

**功能**:
- 实时跟踪 Unity Player.log
- 自动过滤显示包含 "preset", "error", "exception" 的行
- 使用 `Ctrl+C` 停止监控

**日志位置**:
- macOS: `~/Library/Logs/TeamSoda/Duckov/Player.log`
- Windows: `C:\Users\<YourUsername>\AppData\LocalLow\TeamSoda\Duckov\Player.log`

---

## 开发工作流

### 标准开发流程

1. **修改代码**
   ```bash
   # 编辑 ModBehaviour.cs, PresetConfig.cs 等
   ```

2. **编译并部署**
   ```bash
   ./scripts/deploy.sh
   ```

3. **启动日志监控** (可选，在新终端窗口)
   ```bash
   ./scripts/watch-log.sh
   ```

4. **启动游戏测试**
   - 打开 Steam
   - 运行《逃离鸭科夫》
   - 测试 Mod 功能

5. **查看日志**
   - 日志监控窗口会实时显示
   - 或查看完整日志: `cat ~/Library/Logs/TeamSoda/Duckov/Player.log`

---

## 快速命令参考

### 一键部署
```bash
./scripts/deploy.sh && echo "🎮 部署完成，可以启动游戏了！"
```

### 边开发边监控日志
```bash
# 终端 1: 监控日志
./scripts/watch-log.sh

# 终端 2: 修改代码后快速部署
./scripts/deploy.sh
```

### 清理编译输出
```bash
dotnet clean
rm -rf bin/ obj/
```

### 查看最近的日志
```bash
tail -100 ~/Library/Logs/TeamSoda/Duckov/Player.log | grep -i preset
```

---

## 注意事项

⚠️ **临时部署方案**

当前使用 Steam Workshop ID `3591339491` 作为部署目标（替换了原 LiteNetLib Mod）。

这是一个**临时解决方案**，因为 macOS 上游戏只从 Workshop 目录加载 Mod。

**恢复原始 Mod** (如果需要):
```bash
rm -rf "/Users/jacksonc/Library/Application Support/Steam/steamapps/workshop/content/3167020/3591339491"
mv "/Users/jacksonc/Library/Application Support/Steam/steamapps/workshop/content/3167020/3591339491.backup" \
   "/Users/jacksonc/Library/Application Support/Steam/steamapps/workshop/content/3167020/3591339491"
```

---

**最后更新**: 2025-10-25
