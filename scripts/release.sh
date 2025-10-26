#!/bin/bash
# PresetLoadout Mod 发布脚本
# 用于自动化版本管理和发布到 GitHub Release

set -e  # 遇到错误立即退出

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# 项目路径
PROJECT_ROOT="/Volumes/ssd/i/duckov/PresetLoadout"
VERSION_FILE="$PROJECT_ROOT/VERSION"
CSPROJ_FILE="$PROJECT_ROOT/PresetLoadout.csproj"
INFO_INI="$PROJECT_ROOT/ReleaseExample/PresetLoadout/info.ini"

# Workshop 部署路径
WORKSHOP_PATH="/Users/jacksonc/Library/Application Support/Steam/steamapps/workshop/content/3167020/3591339491"

# 发布输出路径
RELEASE_DIR="$PROJECT_ROOT/releases"

# ==================== 函数定义 ====================

print_banner() {
    echo -e "${BLUE}"
    echo "╔══════════════════════════════════════════════╗"
    echo "║   PresetLoadout Mod 发布工具 v2.0           ║"
    echo "╚══════════════════════════════════════════════╝"
    echo -e "${NC}"
}

print_step() {
    echo -e "${GREEN}▶ $1${NC}"
}

print_error() {
    echo -e "${RED}✗ 错误: $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠ 警告: $1${NC}"
}

print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

# 读取当前版本
get_current_version() {
    if [ ! -f "$VERSION_FILE" ]; then
        echo "0.0.0"
        return
    fi
    cat "$VERSION_FILE"
}

# 解析版本号
parse_version() {
    local version=$1
    local major=$(echo $version | cut -d. -f1)
    local minor=$(echo $version | cut -d. -f2)
    local patch=$(echo $version | cut -d. -f3)
    echo "$major $minor $patch"
}

# 增加版本号
bump_version() {
    local current_version=$1
    local bump_type=$2

    read major minor patch <<< $(parse_version $current_version)

    case $bump_type in
        major)
            major=$((major + 1))
            minor=0
            patch=0
            ;;
        minor)
            minor=$((minor + 1))
            patch=0
            ;;
        patch)
            patch=$((patch + 1))
            ;;
        *)
            print_error "未知的版本类型: $bump_type"
            exit 1
            ;;
    esac

    echo "$major.$minor.$patch"
}

# 更新 VERSION 文件
update_version_file() {
    local new_version=$1
    echo "$new_version" > "$VERSION_FILE"
    print_success "VERSION 文件已更新为 $new_version"
}

# 更新 .csproj 文件
update_csproj() {
    local new_version=$1

    # 使用 sed 更新版本号（macOS 兼容版本）
    sed -i '' "s|<Version>.*</Version>|<Version>$new_version</Version>|g" "$CSPROJ_FILE"
    sed -i '' "s|<AssemblyVersion>.*</AssemblyVersion>|<AssemblyVersion>$new_version</AssemblyVersion>|g" "$CSPROJ_FILE"
    sed -i '' "s|<FileVersion>.*</FileVersion>|<FileVersion>$new_version</FileVersion>|g" "$CSPROJ_FILE"

    print_success ".csproj 文件已更新版本号"
}

# 更新 info.ini
update_info_ini() {
    local new_version=$1

    # 检查 info.ini 是否存在
    if [ ! -f "$INFO_INI" ]; then
        print_warning "info.ini 不存在，跳过更新"
        return
    fi

    # 如果 info.ini 中没有 version 字段，添加它
    if ! grep -q "^version" "$INFO_INI"; then
        echo "version = $new_version" >> "$INFO_INI"
    else
        sed -i '' "s|^version.*|version = $new_version|g" "$INFO_INI"
    fi

    print_success "info.ini 已更新版本号"
}

# 编译项目
build_project() {
    print_step "正在编译项目..."

    cd "$PROJECT_ROOT"
    dotnet build -c Release

    if [ $? -eq 0 ]; then
        print_success "编译成功"
    else
        print_error "编译失败"
        exit 1
    fi
}

# 创建发布包
create_release_package() {
    local version=$1
    local release_name="PresetLoadout-v$version"
    local release_path="$RELEASE_DIR/$release_name"

    print_step "正在创建发布包..."

    # 创建发布目录
    mkdir -p "$release_path/PresetLoadout"

    # 复制编译后的 DLL
    cp "$PROJECT_ROOT/bin/Release/netstandard2.1/PresetLoadout.dll" "$release_path/PresetLoadout/"

    # 复制 info.ini
    if [ -f "$INFO_INI" ]; then
        cp "$INFO_INI" "$release_path/PresetLoadout/"
    else
        # 创建默认的 info.ini
        cat > "$release_path/PresetLoadout/info.ini" <<EOF
name = PresetLoadout
displayName = 装备预设系统
description = 快速保存和应用装备配置。支持 3 个预设槽位，智能从仓库获取装备。
version = $version
author = jacksonc
publishedFileId = 3591339491
EOF
    fi

    # 复制文档
    cp "$PROJECT_ROOT/README.md" "$release_path/"
    cp "$PROJECT_ROOT/INSTALL.md" "$release_path/"

    # 创建压缩包
    cd "$RELEASE_DIR"
    zip -r "$release_name.zip" "$release_name"

    print_success "发布包已创建: $release_name.zip"
    echo -e "${BLUE}   路径: $RELEASE_DIR/$release_name.zip${NC}"
}

# 部署到 Workshop
deploy_to_workshop() {
    print_step "正在部署到 Workshop 目录..."

    cp "$PROJECT_ROOT/bin/Release/netstandard2.1/PresetLoadout.dll" "$WORKSHOP_PATH/"

    if [ -f "$INFO_INI" ]; then
        cp "$INFO_INI" "$WORKSHOP_PATH/"
    fi

    print_success "已部署到 Workshop 目录"
}

# Git 操作
git_commit_and_tag() {
    local version=$1

    print_step "正在提交到 Git..."

    cd "$PROJECT_ROOT"

    # 添加修改的文件
    git add VERSION PresetLoadout.csproj ReleaseExample/PresetLoadout/info.ini

    # 提交
    git commit -m "chore: bump version to v$version" || print_warning "没有需要提交的更改"

    # 创建标签
    git tag -a "v$version" -m "Release v$version"

    print_success "已创建 Git 标签 v$version"
}

# 推送到 GitHub
push_to_github() {
    local version=$1

    print_step "正在推送到 GitHub..."

    cd "$PROJECT_ROOT"

    # 推送代码
    git push origin main || git push origin master

    # 推送标签
    git push origin "v$version"

    print_success "已推送到 GitHub"
}

# 创建 GitHub Release
create_github_release() {
    local version=$1
    local release_file="$RELEASE_DIR/PresetLoadout-v$version.zip"

    print_step "正在创建 GitHub Release..."

    # 检查是否安装了 gh CLI
    if ! command -v gh &> /dev/null; then
        print_error "未安装 GitHub CLI (gh)。请先安装: brew install gh"
        echo -e "${YELLOW}手动创建 Release:${NC}"
        echo -e "  1. 访问: https://github.com/Sma1lboy/duckov-preset-loadout/releases/new"
        echo -e "  2. Tag: v$version"
        echo -e "  3. 上传: $release_file"
        return 1
    fi

    # 检查是否已登录
    if ! gh auth status &> /dev/null; then
        print_error "未登录 GitHub CLI。请先运行: gh auth login"
        return 1
    fi

    # 生成 Release Notes
    local release_notes="## PresetLoadout v$version

### 新特性
- 快速保存和应用装备配置
- 支持 3 个预设槽位
- 智能从仓库获取装备
- 可视化反馈

### 安装方法
1. 下载 \`PresetLoadout-v$version.zip\`
2. 解压到 Mod 目录
3. 在游戏中启用 Mod

详见 [INSTALL.md](https://github.com/Sma1lboy/duckov-preset-loadout/blob/main/INSTALL.md)
"

    # 创建 Release
    cd "$PROJECT_ROOT"
    gh release create "v$version" \
        "$release_file" \
        --title "PresetLoadout v$version" \
        --notes "$release_notes"

    if [ $? -eq 0 ]; then
        print_success "GitHub Release 创建成功"
        echo -e "${BLUE}   访问: https://github.com/Sma1lboy/duckov-preset-loadout/releases/tag/v$version${NC}"
    else
        print_error "GitHub Release 创建失败"
        return 1
    fi
}

# 显示使用说明
show_usage() {
    cat <<EOF
用法: $0 [选项]

选项:
    -t, --type <major|minor|patch>   指定版本增量类型 (默认: patch)
    -v, --version <version>          直接指定版本号 (例如: 1.2.3)
    -s, --skip-build                 跳过编译步骤
    -d, --deploy-only                仅部署到 Workshop，不创建 Release
    -n, --no-git                     不创建 Git 标签和推送
    -h, --help                       显示此帮助信息

示例:
    $0                              # 默认增加 patch 版本并发布
    $0 -t minor                     # 增加 minor 版本并发布
    $0 -v 2.0.0                     # 直接发布 2.0.0 版本
    $0 -d                           # 仅部署到 Workshop
    $0 -t major -n                  # 增加 major 版本但不推送 Git

EOF
}

# ==================== 主流程 ====================

main() {
    # 参数解析
    local bump_type="patch"
    local new_version=""
    local skip_build=false
    local deploy_only=false
    local no_git=false

    while [[ $# -gt 0 ]]; do
        case $1 in
            -t|--type)
                bump_type="$2"
                shift 2
                ;;
            -v|--version)
                new_version="$2"
                shift 2
                ;;
            -s|--skip-build)
                skip_build=true
                shift
                ;;
            -d|--deploy-only)
                deploy_only=true
                shift
                ;;
            -n|--no-git)
                no_git=true
                shift
                ;;
            -h|--help)
                show_usage
                exit 0
                ;;
            *)
                print_error "未知选项: $1"
                show_usage
                exit 1
                ;;
        esac
    done

    print_banner

    # 获取当前版本
    current_version=$(get_current_version)
    echo -e "${BLUE}当前版本: $current_version${NC}"

    # 确定新版本
    if [ -z "$new_version" ]; then
        new_version=$(bump_version $current_version $bump_type)
    fi

    echo -e "${GREEN}新版本: $new_version${NC}"
    echo ""

    # 确认操作
    read -p "确认要发布版本 v$new_version 吗? (y/N) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        print_warning "操作已取消"
        exit 0
    fi

    # 更新版本信息
    update_version_file "$new_version"
    update_csproj "$new_version"
    update_info_ini "$new_version"

    echo ""

    # 编译项目
    if [ "$skip_build" = false ]; then
        build_project
        echo ""
    fi

    # 如果仅部署模式
    if [ "$deploy_only" = true ]; then
        deploy_to_workshop
        print_success "部署完成！重启游戏以测试更新"
        exit 0
    fi

    # 创建发布包
    create_release_package "$new_version"
    echo ""

    # 部署到 Workshop
    deploy_to_workshop
    echo ""

    # Git 操作
    if [ "$no_git" = false ]; then
        git_commit_and_tag "$new_version"
        echo ""

        # 询问是否推送到 GitHub
        read -p "是否推送到 GitHub? (y/N) " -n 1 -r
        echo
        if [[ $REPLY =~ ^[Yy]$ ]]; then
            push_to_github "$new_version"
            echo ""

            # 询问是否创建 GitHub Release
            read -p "是否创建 GitHub Release? (y/N) " -n 1 -r
            echo
            if [[ $REPLY =~ ^[Yy]$ ]]; then
                create_github_release "$new_version" || true
            fi
        fi
    fi

    echo ""
    print_success "🎉 发布流程完成！"
    echo ""
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo -e "${GREEN}版本:${NC} v$new_version"
    echo -e "${GREEN}发布包:${NC} $RELEASE_DIR/PresetLoadout-v$new_version.zip"
    echo -e "${GREEN}Workshop:${NC} 已部署"
    echo -e "${BLUE}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
    echo ""
    echo "后续步骤:"
    echo "  1. 重启游戏测试 Mod"
    echo "  2. 访问 GitHub Release 页面确认发布"
    echo ""
}

# 运行主流程
main "$@"
