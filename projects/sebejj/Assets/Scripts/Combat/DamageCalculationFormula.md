# 赛博机甲 SebeJJ - 伤害计算公式文档

**文档版本**: 1.0  
**创建日期**: 2026-02-27  
**作者**: 战斗系统工程师

---

## 目录

1. [基础伤害公式](#基础伤害公式)
2. [伤害类型克制](#伤害类型克制)
3. [暴击系统](#暴击系统)
4. [防御计算](#防御计算)
5. [完整计算流程](#完整计算流程)
6. [代码示例](#代码示例)

---

## 基础伤害公式

### 最终伤害计算

```
最终伤害 = 基础伤害 × 类型克制倍率 × 暴击倍率 × 防御减免 × 其他修正
```

### 基础伤害 (Base Damage)

基础伤害由武器/技能决定，可通过以下方式计算：

```csharp
基础伤害 = 武器基础伤害 + (等级 - 1) × 每级伤害增长
```

---

## 伤害类型克制

### 伤害类型

| 类型 | 说明 |
|------|------|
| Kinetic (动能) | 物理冲击，对装甲有效 |
| Energy (能量) | 激光/等离子，对护盾有效 |
| Explosive (爆炸) | 范围伤害，均衡 |
| Corrosive (腐蚀) | 持续伤害，对生物有效 |
| True (真实) | 无视防御 |

### 克制倍率表

| 伤害类型 | 对装甲 | 对护盾 | 对生物 |
|----------|--------|--------|--------|
| 动能 | 150% | 75% | 100% |
| 能量 | 75% | 150% | 100% |
| 爆炸 | 125% | 125% | 100% |
| 腐蚀 | 100% | 100% | 150% |
| 真实 | 100% | 100% | 100% |

### 计算公式

```
类型倍率 = 克制表[伤害类型, 目标类型]
```

---

## 暴击系统

### 暴击判定

```
是否暴击 = Random(0, 1) < 暴击率
```

### 暴击伤害

```
暴击倍率 = 基础暴击倍率 (默认 2.0x)

if (暴击)
    伤害 = 伤害 × 暴击倍率
```

### 暴击属性

| 属性 | 默认值 | 说明 |
|------|--------|------|
| 暴击率 | 5% | 触发暴击的概率 |
| 暴击倍率 | 200% | 暴击时的伤害倍率 |

---

## 防御计算

### 护盾系统

#### 护盾减伤公式

```
护盾减免 = 护盾值 / (护盾值 + 100)
护盾减免上限 = 80%

实际伤害 = 原始伤害 × (1 - 护盾减免)
```

#### 护盾穿透

```
有效护盾 = 当前护盾 × (1 - 护盾穿透率)
```

### 装甲系统

#### 装甲减伤公式

```
装甲减免 = 100 / (100 + 装甲值)

实际伤害 = 原始伤害 × 装甲减免
```

#### 装甲穿透

```
有效装甲 = 当前装甲 × (1 - 装甲穿透率)
```

### 防御优先级

1. **护盾优先**: 先消耗护盾，护盾耗尽后再伤害生命值
2. **装甲减免**: 伤害生命值时，先经过装甲减免计算

---

## 完整计算流程

```
1. 基础伤害
   ↓
2. 应用类型克制倍率
   ↓
3. 判定暴击 (如果是)
   ↓
4. 应用暴击倍率
   ↓
5. 检查护盾
   - 如果有护盾: 消耗护盾，计算穿透伤害
   - 护盾耗尽: 剩余伤害继续
   ↓
6. 应用装甲减伤
   ↓
7. 应用其他伤害减免
   ↓
8. 最终伤害 (至少为1)
```

### 完整公式

```csharp
// 1. 基础计算
float damage = baseDamage;

// 2. 类型克制
damage *= GetTypeMultiplier(damageType, targetType);

// 3. 暴击判定
if (RollCritical(criticalChance)) {
    damage *= criticalMultiplier;
}

// 4. 护盾处理 (如果有)
if (hasShield) {
    float shieldMitigation = currentShield / (currentShield + 100);
    shieldMitigation = Min(shieldMitigation, 0.8f);
    
    float shieldDamage = damage * shieldMitigation;
    currentShield -= shieldDamage;
    damage -= shieldDamage * shieldAbsorption;
}

// 5. 装甲减伤
if (hasArmor && damage > 0) {
    float armorMitigation = 100 / (100 + currentArmor * armorEffectiveness);
    damage *= armorMitigation;
}

// 6. 其他减免
damage *= (1 - damageReduction);

// 7. 确保最小伤害
damage = Max(damage, 1);
```

---

## 代码示例

### 创建伤害信息

```csharp
// 基础伤害
var damage = new DamageInfo(50f, DamageType.Kinetic);

// 带暴击的伤害
var damage = DamageCalculator.CreateDamageWithCritical(
    50f,                    // 基础伤害
    DamageType.Energy,      // 伤害类型
    0.15f,                  // 暴击率 15%
    2.5f                    // 暴击倍率 250%
);

// 完整配置
damage.Attacker = player;
damage.HitPosition = hitPoint;
damage.HitDirection = (target - attacker).normalized;
damage.KnockbackForce = 10f;
damage.StunDuration = 0.5f;
damage.ArmorPenetration = 0.3f;  // 30%护甲穿透
```

### 计算最终伤害

```csharp
// 获取目标属性
float armor = targetStats.CurrentArmor;
float shield = targetStats.CurrentShield;
float reduction = targetStats.DamageReduction;

// 计算
float finalDamage = DamageCalculator.CalculateDamage(
    damageInfo,
    TargetType.Armor,    // 目标类型
    armor,               // 装甲值
    shield,              // 护盾值
    reduction            // 额外减免
);
```

### 应用伤害到目标

```csharp
// 方式1: 通过接口
IDamageable damageable = target.GetComponent<IDamageable>();
damageable?.TakeDamage(damageInfo);

// 方式2: 通过CombatStats
CombatStats stats = target.GetComponent<CombatStats>();
stats.TakeDamage(damageInfo);
```

### 自定义伤害计算

```csharp
public class CustomDamageCalculator : MonoBehaviour
{
    public float CalculateExplosiveDamage(float baseDamage, float distance, float maxRadius)
    {
        // 爆炸伤害随距离衰减
        float distanceFactor = 1 - (distance / maxRadius);
        distanceFactor = Mathf.Clamp01(distanceFactor);
        
        // 爆炸中心伤害加成
        float centerBonus = distanceFactor < 0.3f ? 1.5f : 1f;
        
        return baseDamage * distanceFactor * centerBonus;
    }
}
```

---

## 伤害数字显示

### 颜色规则

| 类型 | 颜色 |
|------|------|
| 普通伤害 | 白色 |
| 暴击伤害 | 黄色 |
| 治疗 | 绿色 |
| 护盾伤害 | 青色 |

### 显示规则

```csharp
// 显示伤害数字
CombatFeedback.Instance.ShowDamageNumber(
    finalDamage,      // 伤害值
    hitPosition,      // 显示位置
    isCritical,       // 是否暴击
    isHeal            // 是否治疗
);
```

---

## 性能优化建议

1. **缓存计算结果**: 对于不频繁变化的属性，缓存计算结果
2. **避免频繁分配**: 使用对象池复用DamageInfo
3. **批量处理**: 范围伤害时批量检测和计算
4. **LOD系统**: 远距离敌人简化伤害计算

---

## 调试工具

### 伤害日志

```csharp
// 启用伤害日志
DamageLogger.Instance.enableLogging = true;

// 查看统计
DamageLogger.Instance.PrintReport();
```

### 可视化调试

```csharp
// 在Scene视图中显示攻击范围
[SerializeField] private bool showDebugGizmos = true;

private void OnDrawGizmosSelected()
{
    if (!showDebugGizmos) return;
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, attackRange);
}
```

---

*文档结束*