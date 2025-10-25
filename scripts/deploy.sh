#!/bin/bash
# PresetLoadout Mod - 快速部署脚本
# 用途: 编译并部署到 Steam Workshop 目录 (临时方案)
# 日期: 2025-10-25

set -e  # 遇到错误立即退出

# 切换到项目根目录（脚本在 scripts/ 子目录中）
cd "$(dirname "$0")/.."

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "🔨 编译 PresetLoadout Mod..."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

dotnet build -c Release

if [ $? -eq 0 ]; then
    echo ""
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    echo "📦 部署到 Steam Workshop 目录..."
    echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

    WORKSHOP_PATH="/Users/jacksonc/Library/Application Support/Steam/steamapps/workshop/content/3167020/3591339491"

    cp bin/Release/netstandard2.1/PresetLoadout.dll "$WORKSHOP_PATH/"

    echo ""
    echo "✅ 部署完成!"
    echo ""
    echo "📍 Mod 位置: $WORKSHOP_PATH"
    echo "🎮 下一步: 重启游戏测试"
    echo ""
else
    echo ""
    echo "❌ 编译失败! 请检查错误信息"
    exit 1
fi
