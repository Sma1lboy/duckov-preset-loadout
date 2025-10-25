# Item 类关键字段文档

本文档记录了《逃离鸭科夫》游戏中 `Item` 类的关键字段，用于 Mod 开发。

## 概述

`Item` 类来自 `ItemStatsSystem` 命名空间，代表游戏中的物品实例（武器、装备、弹药、背包等）。

## 关键字段列表

根据反射分析，Item 类共有 **41** 个字段。以下是与物品位置判断相关的重要字段：

### 1. 位置判断字段

| 字段名 | 类型 | 用途 | 说明 |
|--------|------|------|------|
| `inInventory` | `Inventory` | 判断物品是否在背包中 | 如果物品在背包格子中，此字段不为 null |
| `pluggedIntoSlot` | `Slot` | 判断物品是否插在槽位上 | 如果物品装备在槽位（如枪械槽、护甲槽），此字段不为 null |

**判断逻辑**：
```csharp
// 物品在背包中
if (item.inInventory != null) { /* 在背包 */ }

// 物品装备在槽位上
if (item.pluggedIntoSlot != null) { /* 在装备槽位 */ }

// 物品是顶层装备（如背包本身）
if (item.inInventory == null && item.pluggedIntoSlot == null) { /* 顶层装备 */ }
```

### 2. 物品容器字段

| 字段名 | 类型 | 用途 |
|--------|------|------|
| `slots` | `SlotCollection` | 物品自身包含的槽位集合（如枪械的配件槽） |
| `inventory` | `Inventory` | 物品自身包含的背包空间（如背包物品） |

### 3. 事件字段

| 字段名 | 类型 | 用途 |
|--------|------|------|
| `onParentChanged` | `Action<Item>` | 物品父对象改变时触发 |
| `onPluggedIntoSlot` | `Action<Slot>` | 物品插入槽位时触发 |
| `onUnpluggedFromSlot` | `Action<Slot>` | 物品从槽位移除时触发 |
| `onSlotContentChanged` | `Action<Slot, Item>` | 槽位内容改变时触发 |
| `onSlotTreeChanged` | `Action<Item>` | 槽位树结构改变时触发 |

## 游戏中的物品状态

### 状态 1: 装备在身上（装备槽位）
- **特征**: `pluggedIntoSlot != null`
- **示例**:
  - 主武器槽的枪械
  - 护甲槽的防弹衣
  - 背包槽的背包

### 状态 2: 放在背包中
- **特征**: `inInventory != null`
- **示例**:
  - 背包格子里的弹药
  - 背包格子里的备用武器
  - 背包格子里的医疗品

### 状态 3: 在仓库中
- **检测方式**: `item.IsInPlayerStorage()` 返回 true
- **说明**: 游戏提供的扩展方法，无需直接访问字段

### 状态 4: 顶层装备
- **特征**: `inInventory == null && pluggedIntoSlot == null`
- **示例**:
  - 直接装备在角色身上的背包
  - 某些直接附着在角色上的装备

## GameObject 层级结构

### 父对象信息

所有玩家身上的物品（无论装备还是背包）的父对象都是：
- **parent**: `Character(Clone)`
- **grandparent**: `Character(Clone)`

**注意**: 不能通过 GameObject 名称来区分装备和背包物品，必须使用 `inInventory` 和 `pluggedIntoSlot` 字段！

## UI 相关信息

### 背包UI对象
- **名称**: `EquipmentAndInventory`
- **检测方式**: `GameObject.Find("EquipmentAndInventory")`
- **用途**: 判断玩家是否打开了背包/装备界面

```csharp
GameObject inventoryUI = GameObject.Find("EquipmentAndInventory");
bool isOpen = inventoryUI != null && inventoryUI.activeInHierarchy;
```

## 使用示例

### 示例 1: 区分背包和装备

```csharp
bool IsItemInInventory(Item item)
{
    var inInventoryField = item.GetType().GetField("inInventory",
        System.Reflection.BindingFlags.Public |
        System.Reflection.BindingFlags.NonPublic |
        System.Reflection.BindingFlags.Instance);

    if (inInventoryField != null)
    {
        var inventory = inInventoryField.GetValue(item);
        if (inventory != null)
        {
            return true; // 在背包中
        }
    }

    var pluggedIntoSlotField = item.GetType().GetField("pluggedIntoSlot",
        System.Reflection.BindingFlags.Public |
        System.Reflection.BindingFlags.NonPublic |
        System.Reflection.BindingFlags.Instance);

    if (pluggedIntoSlotField != null)
    {
        var slot = pluggedIntoSlotField.GetValue(item);
        if (slot != null)
        {
            return false; // 在装备槽位
        }
    }

    return false; // 默认认为是装备
}
```

### 示例 2: 获取玩家身上所有物品

```csharp
List<Item> GetPlayerItems()
{
    List<Item> items = new List<Item>();
    Item[] allItems = FindObjectsOfType<Item>();

    foreach (Item item in allItems)
    {
        if (item != null && item.IsInPlayerCharacter() && item.TypeID > 0)
        {
            items.Add(item);
        }
    }

    return items;
}
```

### 示例 3: 获取仓库物品

```csharp
List<Item> GetPlayerStorageItems()
{
    List<Item> items = new List<Item>();
    Item[] allItems = FindObjectsOfType<Item>();

    foreach (Item item in allItems)
    {
        if (item != null && item.IsInPlayerStorage() && item.TypeID > 0)
        {
            items.Add(item);
        }
    }

    return items;
}
```

## 注意事项

1. **TypeID 为 0 的物品**: 这些是系统占位符，应该过滤掉
   ```csharp
   if (item.TypeID <= 0) continue;
   ```

2. **使用反射访问字段**: Item 类的这些字段可能是私有的，需要使用反射
   ```csharp
   System.Reflection.BindingFlags.Public |
   System.Reflection.BindingFlags.NonPublic |
   System.Reflection.BindingFlags.Instance
   ```

3. **性能考虑**: 反射比较慢，如果需要频繁调用，考虑缓存 FieldInfo

4. **版本兼容性**: 这些字段可能在游戏更新后改变，建议添加 null 检查和异常处理

## 相关文档

- 官方 API 文档: `/Users/jacksonc/i/duckov/duckov_modding/Documents/NotableAPIs_CN.md`
- 预设加载 Mod 实现: `/Volumes/ssd/i/duckov/PresetLoadout/ModBehaviour.cs`

---

**文档版本**: 1.0
**最后更新**: 2025-10-25
**游戏版本**: Escape From Duckov (未知具体版本)
