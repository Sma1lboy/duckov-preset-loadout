# Release PresetLoadout Mod

You are about to help release a new version of the PresetLoadout mod. Follow these steps carefully:

## Step 1: Review Changes Since Last Release

First, check the git log to see all commits since the last release tag:

```bash
git log $(git describe --tags --abbrev=0)..HEAD --oneline --no-decorate
```

Analyze these commits and categorize them into:
- **新增** (New features)
- **改进** (Improvements)
- **修复** (Bug fixes)
- **文档** (Documentation)
- **其他** (Other changes)

## Step 2: Determine Version Bump Type

Based on the changes, suggest the appropriate version bump:
- **patch** (x.x.X) - Bug fixes only
- **minor** (x.X.0) - New features, backwards compatible
- **major** (X.0.0) - Breaking changes

Show the user:
- Current version (from VERSION file)
- Suggested new version
- Reason for the version bump

## Step 3: Update CHANGELOG.md

Add a new version section to CHANGELOG.md with:
- Version number and date (format: `## [X.Y.Z] - YYYY-MM-DD`)
- Categorized changes based on Step 1
- Proper formatting with markdown

The format should be:

```markdown
## [X.Y.Z] - YYYY-MM-DD

### 新增
- Feature 1
- Feature 2

### 改进
- Improvement 1

### 修复
- Fix 1

---
```

Show the user the proposed CHANGELOG entry and ask for confirmation.

## Step 4: Execute Release Script

After user confirms the CHANGELOG update, run the release script:

```bash
./scripts/release.sh -t <patch|minor|major>
```

## Step 5: Verify and Report

After the script completes:
1. Confirm the release was successful
2. Show the release package location
3. Provide the GitHub Release URL
4. Remind the user to test the mod in-game

## Important Notes

- Always ask for user confirmation before updating CHANGELOG.md
- Always ask for user confirmation before running the release script
- If git log shows no commits since last tag, warn the user that there are no changes to release
- Handle errors gracefully and provide clear next steps

## Example Output Format

```
📊 发现以下更改 (自 v1.0.1 起):

### 新增
- [commit-hash] feat: 添加预设重命名功能
- [commit-hash] feat: 支持导入导出预设

### 改进
- [commit-hash] refactor: 优化装备匹配算法

建议版本号: 1.1.0 (minor - 新功能)

是否继续更新 CHANGELOG.md?
```
