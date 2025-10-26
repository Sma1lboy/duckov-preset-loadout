# PresetLoadout 开发脚本

## 📦 快速部署 - deploy.sh

编译并部署到游戏目录。

```bash
./scripts/deploy.sh
```

---

## 🚀 版本发布 - release.sh

自动化版本管理和发布。

### 基本用法

```bash
./scripts/release.sh           # patch: 1.0.0 → 1.0.1 (默认)
./scripts/release.sh -t minor  # minor: 1.0.0 → 1.1.0
./scripts/release.sh -t major  # major: 1.0.0 → 2.0.0
./scripts/release.sh -v 2.0.0  # 直接指定版本
./scripts/release.sh -d        # 仅部署，不发布
./scripts/release.sh -h        # 帮助
```

### 功能

- 自动更新版本号 (VERSION, .csproj, info.ini)
- 编译 Release 版本
- 创建 ZIP 发布包 (`releases/`)
- 部署到 Workshop 目录
- 创建 Git 标签并推送
- 创建 GitHub Release (需要 `gh` CLI)
- **自动从 CHANGELOG.md 提取 Release Notes** ✨

### 版本类型

| 类型 | 何时使用 | 示例 |
|------|---------|------|
| patch | Bug 修复 | 1.0.0 → 1.0.1 |
| minor | 新功能 | 1.0.0 → 1.1.0 |
| major | 重大更新 | 1.0.0 → 2.0.0 |

---

## 📋 日志监控 - watch-log.sh

实时查看游戏日志。

```bash
./scripts/watch-log.sh
```

---

## 🔧 开发流程

1. 修改代码
2. `./scripts/deploy.sh` - 快速测试
3. `./scripts/watch-log.sh` - 查看日志（可选）
4. 测试通过后提交代码
5. **更新 CHANGELOG.md** - 在对应版本下添加更新内容
6. `./scripts/release.sh` - 发布新版本（自动提取 CHANGELOG 作为 Release Notes）

---

## 📦 GitHub Release

**首次使用需要安装 gh CLI**:

```bash
brew install gh
gh auth login
```

不想用 gh CLI？脚本会给出手动创建 Release 的链接。
