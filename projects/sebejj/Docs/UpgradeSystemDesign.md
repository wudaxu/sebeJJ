# 赛博机甲 SebeJJ 升级系统设计文档

## 1. 系统概述

### 1.1 设计目标
- 提供深度的角色成长系统
- 支持机甲和武器两条独立的升级线
- 材料驱动的升级机制，鼓励探索和战斗
- 直观的升级界面和反馈

### 1.2 核心特性
- **单例管理器模式**：全局统一的升级管理
- **数据驱动配置**：通过ScriptableObject配置升级数据
- **状态持久化**：支持存档/读档
- **模块化UI**：可复用的升级界面组件
- **事件驱动**：松耦合的系统交互

---

## 2. 系统架构

### 2.1 核心类图

```
┌─────────────────────────────────────────────────────────────┐
│                      UpgradeManager                          │
│                    (单例 - 核心管理器)                        │
├─────────────────────────────────────────────────────────────┤
│  - mechaUpgradeState: MechaUpgradeState                      │
│  - weaponUpgradeStates: Dictionary<string, WeaponUpgradeState>│
│  - upgradeConfig: UpgradeDataConfig                          │
├─────────────────────────────────────────────────────────────┤
│  + UpgradeMecha(type): bool                                  │
│  + UpgradeWeapon(weaponId, type): bool                       │
│  + GetMechaUpgradePreview(type): UpgradePreview              │
│  + GetWeaponUpgradePreview(weaponId, type): UpgradePreview   │
│  + GetSaveData(): UpgradeSaveData                            │
│  + LoadSaveData(data): void                                  │
└─────────────────────────────────────────────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        │                     │                     │
        ▼                     ▼                     ▼
┌───────────────┐    ┌───────────────┐    ┌───────────────┐
│ MaterialManager│    │UpgradeDataConfig│   │ UpgradeState  │
│   (单例)       │    │  (Scriptable)  │    │  (存档数据)    │
└───────────────┘    └───────────────┘    └───────────────┘
        │                     │
        ▼                     ▼
┌───────────────┐    ┌───────────────┐
│MaterialDatabase│    │MechaUpgradeData│
│ (Scriptable)  │    │WeaponUpgradeData│
└───────────────┘    └───────────────┘
```

### 2.2 目录结构

```
Assets/Scripts/Upgrade/
├── Core/
│   ├── UpgradeManager.cs          # 升级管理器（单例）
│   ├── UpgradeData.cs             # 升级数据定义
│   ├── UpgradeDataConfig.cs       # 升级配置SO
│   ├── UpgradeState.cs            # 升级状态（存档）
│   ├── MaterialManager.cs         # 材料管理器（单例）
│   ├── MaterialData.cs            # 材料数据定义
│   └── MaterialDatabase.cs        # 材料数据库SO
├── Connectors/
│   ├── WeaponUpgradeConnector.cs  # 武器升级连接器
│   └── MechaAttributeConnector.cs # 机甲属性连接器
└── UI/
    ├── UpgradeUIManager.cs        # 升级界面管理器
    ├── UpgradeNode.cs             # 升级树节点
    ├── MaterialSlotUI.cs          # 材料槽UI
    ├── UpgradePreviewUI.cs        # 升级预览UI
    └── UpgradeAnimationController.cs # 升级动画控制器
```

---

## 3. 升级类型

### 3.1 机甲升级 (MechaUpgradeType)

| 类型 | 名称 | 效果 | 基础值 | 最大等级 |
|------|------|------|--------|----------|
| Hull | 船体强化 | 增加耐久上限 | 100 HP | 10 |
| Energy | 能量核心 | 增加能量上限 | 100 EP | 10 |
| Speed | 推进系统 | 增加移动速度 | 5 m/s | 10 |
| Cargo | 货舱扩容 | 增加资源容量 | 50 slot | 8 |

#### 计算公式
- **加法模式**: `value = base + increment * (level - 1)`
- **乘法模式**: `value = base * multiplier ^ (level - 1)`
- **混合模式**: `value = (base + increment * (level - 1)) * multiplier ^ ((level - 1) * 0.5)`

### 3.2 武器升级 (WeaponUpgradeType)

| 类型 | 名称 | 效果 | 计算方式 |
|------|------|------|----------|
| Damage | 伤害强化 | 增加武器伤害 | 乘法 |
| Range | 射程扩展 | 增加攻击范围 | 乘法 |
| AttackSpeed | 攻速提升 | 减少攻击间隔 | 乘法 |
| EnergyEfficiency | 能量效率 | 减少能量消耗 | 乘法 |

---

## 4. 材料系统

### 4.1 材料类型 (MaterialType)

- **Metal** - 金属材料
- **Crystal** - 水晶材料
- **Organic** - 有机材料
- **Energy** - 能量材料
- **Ancient** - 古代遗物
- **Special** - 特殊材料

### 4.2 材料稀有度 (MaterialRarity)

| 稀有度 | 颜色 | 堆叠上限 | 典型用途 |
|--------|------|----------|----------|
| Common (普通) | 灰色 | 999 | 基础升级 |
| Uncommon (罕见) | 绿色 | 999 | 中级升级 |
| Rare (稀有) | 蓝色 | 99 | 高级升级 |
| Epic (史诗) | 紫色 | 99 | 顶级升级 |
| Legendary (传说) | 橙色 | 99 | 特殊升级 |

### 4.3 材料列表

| 材料ID | 名称 | 类型 | 稀有度 | 获取途径 |
|--------|------|------|--------|----------|
| scrap_metal | 废金属 | Metal | Common | 资源采集、敌人掉落 |
| scrap_circuit | 废弃电路 | Metal | Common | 资源采集 |
| coral_fragment | 珊瑚碎片 | Organic | Common | 资源采集 |
| energy_crystal | 能量水晶 | Crystal | Uncommon | 资源采集、敌人掉落 |
| bioluminescent_gland | 发光腺体 | Organic | Uncommon | 敌人掉落 |
| pressure_alloy | 耐压合金 | Metal | Uncommon | 合成、敌人掉落 |
| ancient_core | 古代核心 | Ancient | Rare | 稀有掉落、合成 |
| void_shard | 虚空碎片 | Crystal | Rare | 敌人掉落 |
| abyssal_essence | 深渊精华 | Energy | Rare | 敌人掉落 |
| titan_plate | 泰坦装甲板 | Ancient | Epic | 稀有掉落 |
| neural_processor | 神经处理器 | Ancient | Epic | 合成 |
| heart_of_abyss | 深渊之心 | Special | Legendary | 特殊事件 |

### 4.4 材料获取途径

#### 资源采集转换
```
金属资源 → 废金属 (100%) + 废弃电路 (30%概率)
能量资源 → 能量水晶 (50%) + 废弃电路 (50%)
有机资源 → 珊瑚碎片 (80%) + 发光腺体 (20%概率)
古代遗物 → 古代核心 (30%) + 虚空碎片 (50%)
```

#### 敌人掉落
- **机械鲨鱼**: 废金属(80%), 耐压合金(30%), 能量水晶(15%)
- **深海章鱼**: 发光腺体(70%), 虚空碎片(20%), 深渊精华(10%)

### 4.5 材料合成

| 配方ID | 名称 | 输入材料 | 输出 |
|--------|------|----------|------|
| craft_pressure_alloy | 合成耐压合金 | 废金属x5 + 能量水晶x1 | 耐压合金x1 |
| craft_energy_core | 合成能量核心 | 能量水晶x3 + 废弃电路x2 | 古代核心x1 |
| craft_advanced_circuit | 合成高级电路 | 废弃电路x3 + 发光腺体x1 | 神经处理器x1 |

---

## 5. 升级需求配置

### 5.1 机甲升级需求示例

| 等级 | 废金属 | 能量水晶 | 古代核心 |
|------|--------|----------|----------|
| 1 | 5 | - | - |
| 2 | 10 | - | - |
| 3 | 15 | - | - |
| 4 | 20 | - | - |
| 5 | 25 | 2 | - |
| 6 | 30 | 4 | - |
| 7 | 35 | 6 | - |
| 8 | 40 | 8 | 1 |
| 9 | 45 | 10 | 2 |
| 10 | 50 | 12 | 3 |

### 5.2 武器升级需求

武器升级需求与机甲类似，但材料需求量为机甲的60-80%。

---

## 6. 用户界面

### 6.1 界面组件

#### UpgradeUIManager
- 管理整个升级界面的生命周期
- 处理升级树展示
- 协调详情面板和预览面板

#### UpgradeNode
- 单个升级节点UI
- 显示等级、图标、进度
- 状态颜色：锁定(灰)、可升级(蓝)、已解锁(绿)、满级(金)

#### MaterialSlotUI
- 材料需求展示
- 显示当前数量/需求数量
- 颜色标识：充足(绿)、不足(红)

#### UpgradePreviewUI
- 升级成功后的预览弹窗
- 显示属性变化
- 自动消失(2.5秒)

#### UpgradeAnimationController
- 升级成功特效
- 升级失败特效
- 相机震动效果

### 6.2 界面流程

```
打开升级界面
    │
    ▼
显示升级树
    │
    ├── 机甲升级分支
    │   ├── 船体强化
    │   ├── 能量核心
    │   ├── 推进系统
    │   └── 货舱扩容
    │
    └── 武器升级分支
        ├── [武器1]
        │   ├── 伤害强化
        │   ├── 射程扩展
        │   ├── 攻速提升
        │   └── 能量效率
        └── [武器2]...
    │
    ▼
选择升级节点
    │
    ▼
显示详情面板
    ├── 当前等级/最大等级
    ├── 当前数值 → 升级后数值
    ├── 材料需求列表
    └── 升级按钮
    │
    ▼
点击升级
    │
    ├── 成功
    │   ├── 消耗材料
    │   ├── 播放动画
    │   ├── 刷新UI
    │   └── 显示预览
    │
    └── 失败
        └── 播放失败动画
```

---

## 7. 事件系统

### 7.1 UpgradeManager 事件

```csharp
// 机甲升级成功
OnMechaUpgraded(MechaUpgradeType type, int newLevel)

// 武器升级成功
OnWeaponUpgraded(string weaponId, WeaponUpgradeType type, int newLevel)

// 升级解锁
OnUpgradeUnlocked(UpgradeData data)

// 升级预览
OnUpgradePreview(UpgradeData data, List<MaterialRequirement> requirements)

// 升级失败
OnUpgradeFailed()
```

### 7.2 MaterialManager 事件

```csharp
// 材料增加
OnMaterialAdded(string materialId, int amount)

// 材料移除
OnMaterialRemoved(string materialId, int amount)

// 材料消耗
OnMaterialConsumed(string materialId, int amount)

// 库存变化
OnInventoryChanged()
```

---

## 8. 存档系统

### 8.1 存档数据结构

```csharp
[Serializable]
public class UpgradeSaveData
{
    public MechaUpgradeState mechaState;
    public Dictionary<string, WeaponUpgradeState> weaponStates;
}

[Serializable]
public class MechaUpgradeState
{
    private Dictionary<MechaUpgradeType, int> upgradeLevels;
}

[Serializable]
public class WeaponUpgradeState
{
    public string weaponId;
    private Dictionary<WeaponUpgradeType, int> upgradeLevels;
}
```

### 8.2 存档流程

1. **保存**: `UpgradeManager.GetSaveData()` → 序列化 → 存档文件
2. **加载**: 存档文件 → 反序列化 → `UpgradeManager.LoadSaveData()` → 重新应用升级效果

---

## 9. 连接器系统

### 9.1 WeaponUpgradeConnector
- 监听武器升级事件
- 将升级效果应用到实际武器
- 提供升级效果查询接口

### 9.2 MechaAttributeConnector
- 监听机甲升级事件
- 更新机甲控制器属性
- 管理属性倍率

---

## 10. 使用指南

### 10.1 基础配置

1. 创建 `UpgradeDataConfig` 资源文件
2. 创建 `MaterialDatabase` 资源文件
3. 在场景中放置 `UpgradeManager` 和 `MaterialManager`
4. 配置连接器（可选）

### 10.2 代码示例

```csharp
// 升级机甲
bool success = UpgradeManager.Instance.UpgradeMecha(MechaUpgradeType.Hull);

// 升级武器
bool success = UpgradeManager.Instance.UpgradeWeapon("PlasmaCannon", WeaponUpgradeType.Damage);

// 添加材料
MaterialManager.Instance.AddMaterial("scrap_metal", 10);

// 检查材料
bool hasEnough = MaterialManager.Instance.HasMaterial("scrap_metal", 5);

// 合成材料
bool crafted = MaterialManager.Instance.Craft("craft_pressure_alloy");

// 获取升级预览
var preview = UpgradeManager.Instance.GetMechaUpgradePreview(MechaUpgradeType.Speed);
Debug.Log($"升级后速度: {preview.nextValue}");
```

### 10.3 UI使用

```csharp
// 打开升级界面
UpgradeUIManager.Instance.OpenUpgradePanel();

// 关闭升级界面
UpgradeUIManager.Instance.CloseUpgradePanel();
```

---

## 11. 扩展指南

### 11.1 添加新的机甲升级类型

1. 在 `MechaUpgradeType` 枚举中添加新类型
2. 在 `UpgradeDataConfig` 中添加对应配置
3. 在 `MechaAttributeConnector` 中处理新类型的效果

### 11.2 添加新的材料

1. 在 `MaterialDatabase` 中添加材料定义
2. 配置获取途径（资源转换/敌人掉落/合成）
3. 更新升级需求配置

### 11.3 自定义升级公式

在 `MechaUpgradeData` 或 `WeaponUpgradeEntry` 中：
- 修改 `calculationMode`
- 调整 `valueIncrement` 和 `valueMultiplier`

---

## 12. 性能考虑

- **对象池**: 升级节点使用对象池管理
- **延迟加载**: 武器升级节点按需创建
- **事件优化**: 使用事件而非轮询
- **缓存策略**: 升级效果缓存避免重复计算

---

## 13. 版本历史

| 版本 | 日期 | 变更 |
|------|------|------|
| 1.0 | 2026-02-27 | 初始版本，包含完整的升级系统 |

---

**文档作者**: 升级系统工程师  
**最后更新**: 2026-02-27