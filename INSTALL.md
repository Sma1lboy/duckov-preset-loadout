# PresetLoadout Mod 安装指南

## 第一步: 安装 .NET SDK

### 使用 Homebrew 安装 (推荐)

打开终端,运行:

```bash
brew install --cask dotnet-sdk
```

安装过程中会要求输入管理员密码,输入你的 macOS 密码即可。

### 验证安装

安装完成后,运行:

```bash
dotnet --version
```

如果显示版本号 (例如 `9.0.305`),说明安装成功!

---

## 第二步: 编译 Mod

### 2.1 进入项目目录

```bash
cd "/Users/jacksonc/i/duckov/duckov_modding/PresetLoadout"
```

### 2.2 编译项目

```bash
dotnet build -c Release
```

编译成功后,你会看到类似输出:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

编译后的 DLL 文件位置:
```
bin/Release/netstandard2.1/PresetLoadout.dll
```

---

## 第三步: 准备发布文件

### 3.1 创建 Mod 文件夹

```bash
mkdir -p "/Users/jacksonc/Library/Application Support/Steam/steamapps/common/Escape from Duckov/Duckov.app/Contents/Resources/Data/Mods/PresetLoadout"
```

### 3.2 复制文件

```bash
# 复制 DLL
cp bin/Release/netstandard2.1/PresetLoadout.dll "/Users/jacksonc/Library/Application Support/Steam/steamapps/common/Escape from Duckov/Duckov.app/Contents/Resources/Data/Mods/PresetLoadout/"

# 复制 info.ini
cp ReleaseExample/PresetLoadout/info.ini "/Users/jacksonc/Library/Application Support/Steam/steamapps/common/Escape from Duckov/Duckov.app/Contents/Resources/Data/Mods/PresetLoadout/"
```

### 3.3 创建预览图 (可选)

如果你有预览图,复制到 Mods 文件夹:

```bash
cp preview.png "/Users/jacksonc/Library/Application Support/Steam/steamapps/common/Escape from Duckov/Duckov.app/Contents/Resources/Data/Mods/PresetLoadout/"
```

---

## 第四步: 测试 Mod

### 4.1 启动游戏

启动《逃离鸭科夫》游戏

### 4.2 启用 Mod

1. 在游戏主菜单,点击 **Mods** 选项
2. 找到 **"装备预设系统"**
3. 启用该 Mod
4. 重启游戏 (如果需要)

### 4.3 测试功能

进入游戏后:

1. **按 H 键** - 查看帮助信息
2. **装备一些物品**
3. **按 Ctrl + 1** - 保存预设 1
4. **卸下装备并放回仓库**
5. **按 1** - 应用预设 1

如果装备自动从仓库装配到角色身上,说明成功! 🎉

---

## 快捷命令 (一键安装)

如果你已经安装了 .NET SDK,可以直接运行:

```bash
cd "/Users/jacksonc/i/duckov/duckov_modding/PresetLoadout" && \
dotnet build -c Release && \
mkdir -p "/Users/jacksonc/Library/Application Support/Steam/steamapps/common/Escape from Duckov/Duckov.app/Contents/Resources/Data/Mods/PresetLoadout" && \
cp bin/Release/netstandard2.1/PresetLoadout.dll "/Users/jacksonc/Library/Application Support/Steam/steamapps/common/Escape from Duckov/Duckov.app/Contents/Resources/Data/Mods/PresetLoadout/" && \
cp ReleaseExample/PresetLoadout/info.ini "/Users/jacksonc/Library/Application Support/Steam/steamapps/common/Escape from Duckov/Duckov.app/Contents/Resources/Data/Mods/PresetLoadout/" && \
echo "✅ Mod 安装完成!"
```

---

## 故障排除

### 问题 1: 找不到 dotnet 命令

**解决方案**:
- 重新打开终端窗口
- 或运行: `source ~/.zshrc` 或 `source ~/.bash_profile`

### 问题 2: 编译错误 - 找不到引用

**解决方案**:
- 确认游戏路径正确
- 检查 `PresetLoadout.csproj` 中的 `<DuckovPath>` 配置

### 问题 3: 游戏中找不到 Mod

**解决方案**:
- 确认文件复制到了正确的位置
- 检查是否有 3 个文件: `PresetLoadout.dll`, `info.ini`, `preview.png`
- 查看游戏日志文件

### 问题 4: Mod 加载但不工作

**解决方案**:
- 按 F12 打开开发者控制台 (如果游戏支持)
- 查看游戏日志: `~/Library/Logs/EscapeFromDuckov/` (可能的位置)
- 查找 "PresetLoadout" 相关的错误信息

---

## 开发模式 (快速测试)

如果你要频繁修改和测试,可以创建一个自动化脚本:

创建文件 `build-and-install.sh`:

```bash
#!/bin/bash
cd "/Users/jacksonc/i/duckov/duckov_modding/PresetLoadout"
dotnet build -c Release
if [ $? -eq 0 ]; then
    cp bin/Release/netstandard2.1/PresetLoadout.dll "/Users/jacksonc/Library/Application Support/Steam/steamapps/common/Escape from Duckov/Duckov.app/Contents/Resources/Data/Mods/PresetLoadout/"
    echo "✅ Build and install successful!"
else
    echo "❌ Build failed!"
fi
```

赋予执行权限:
```bash
chmod +x build-and-install.sh
```

每次修改代码后,运行:
```bash
./build-and-install.sh
```

---

## 需要帮助?

如果遇到问题,可以:
1. 查看错误日志
2. 检查游戏控制台输出
3. 向开发者反馈问题
