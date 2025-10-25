#!/bin/bash
# 查看游戏日志的快捷脚本
# 用途: 实时监控游戏日志,方便调试

LOG_PATH=~/Library/Logs/TeamSoda/Duckov/Player.log

if [ ! -f "$LOG_PATH" ]; then
    echo "❌ 日志文件不存在: $LOG_PATH"
    echo "请先运行游戏!"
    exit 1
fi

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "📋 实时监控游戏日志"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "日志位置: $LOG_PATH"
echo "按 Ctrl+C 停止监控"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

# 只显示 PresetLoadout 相关的日志
tail -f "$LOG_PATH" | grep --line-buffered -i "preset\|error\|exception"
