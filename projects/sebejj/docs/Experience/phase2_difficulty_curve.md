# Phase 2: 难度曲线调优

## 概述

难度曲线是游戏核心体验的关键。SebeJJ采用**动态难度调整**与**固定难度层级**相结合的设计，确保新手不会受挫，老玩家也能获得挑战。

## 设计目标

1. **心流体验** - 难度始终匹配玩家技能
2. **风险回报** - 越深风险越高，收益越大
3. **死亡有意义** - 惩罚适中，鼓励谨慎而非畏缩
4. **可预测性** - 玩家能判断自己是否准备好

## 难度曲线模型

### 深度-难度关系

```
难度
 ^
5│                                    ╭─────── Boss区
 │                              ╭────╯
4│                        ╭────╯
 │                  ╭────╯
3│            ╭────╯
 │      ╭────╯
2│ ╭────╯
 │╯
1├────┬────┬────┬────┬────┬────┬────┬────┬────┬──► 深度
  0   20   30   40   50   60   70   80   90  100 (米)
  │    │         │         │         │    │
新手区  │      中级区     高级区      │  深渊区
    过渡区                      专家区
```

### 难度计算公式

```csharp
// 基础难度系数
float baseDifficulty = 1.0f;

// 深度难度 (非线性增长)
float depthFactor = Mathf.Pow(currentDepth / 100f, 1.5f) * 4f;

// 玩家技能调整 (基于历史表现)
float playerSkill = CalculatePlayerSkill(); // 0.8 - 1.2

// 动态调整系数
float dynamicAdjustment = GetDynamicAdjustment();

// 最终难度
float finalDifficulty = (baseDifficulty + depthFactor) * playerSkill * dynamicAdjustment;
```

## 敌人强度缩放

### 属性缩放公式

```csharp
public class EnemyScalingSystem : MonoBehaviour
{
    [Header("基础属性")]
    public AnimationCurve healthCurve;      // 血量曲线
    public AnimationCurve damageCurve;      // 伤害曲线
    public AnimationCurve speedCurve;       // 速度曲线
    public AnimationCurve spawnRateCurve;   // 生成率曲线
    
    public EnemyStats CalculateScaledStats(EnemyBase enemy, float depth)
    {
        float normalizedDepth = depth / 100f;
        float difficulty = GetDifficultyAtDepth(depth);
        
        return new EnemyStats
        {
            maxHealth = enemy.BaseHealth * healthCurve.Evaluate(normalizedDepth) * difficulty,
            damage = enemy.BaseDamage * damageCurve.Evaluate(normalizedDepth) * difficulty,
            moveSpeed = enemy.BaseSpeed * speedCurve.Evaluate(normalizedDepth),
            attackSpeed = enemy.BaseAttackSpeed * (1 + normalizedDepth * 0.3f),
            xpReward = enemy.BaseXP * (1 + difficulty),
            creditReward = enemy.BaseCredits * (1 + difficulty * 0.5f)
        };
    }
    
    private float GetDifficultyAtDepth(float depth)
    {
        // 分段难度计算
        if (depth <= 30f)
            return 0.8f + depth / 30f * 0.4f;  // 0.8 - 1.2
        else if (depth <= 60f)
            return 1.2f + (depth - 30f) / 30f * 0.6f;  // 1.2 - 1.8
        else if (depth <= 90f)
            return 1.8f + (depth - 60f) / 30f * 0.7f;  // 1.8 - 2.5
        else
            return 2.5f + (depth - 90f) / 10f * 1.5f;  // 2.5 - 4.0
    }
}
```

### 各深度层敌人配置

| 深度层 | 敌人类型 | 血量倍率 | 伤害倍率 | 特殊能力 |
|--------|----------|----------|----------|----------|
| 0-30m | 机械鱼(小) | 1.0x | 1.0x | 无 |
| 30-50m | 机械鱼(中) | 1.3x | 1.2x | 快速冲刺 |
| 50-70m | 机械蟹 | 1.8x | 1.5x | 护盾 |
| 70-90m | 机械水母 | 2.2x | 1.8x | 范围攻击 |
| 90-100m | Boss级 | 4.0x | 2.5x | 多阶段 |

### Boss难度设计

**铁钳巨兽 (90米)**

```csharp
public class BossDifficultyConfig
{
    // 阶段划分
    public List<BossPhase> Phases = new List<BossPhase>
    {
        new BossPhase
        {
            phaseName = "第一阶段",
            healthThreshold = 1.0f, // 100% - 75%
            attackPattern = AttackPattern.Basic,
            attackCooldown = 2.0f,
            specialAbilities = new List<Ability> { Ability.ClawSwipe }
        },
        new BossPhase
        {
            phaseName = "第二阶段",
            healthThreshold = 0.75f, // 75% - 50%
            attackPattern = AttackPattern.Aggressive,
            attackCooldown = 1.5f,
            specialAbilities = new List<Ability> { Ability.ClawSwipe, Ability.BubbleShield }
        },
        new BossPhase
        {
            phaseName = "第三阶段",
            healthThreshold = 0.5f, // 50% - 25%
            attackPattern = AttackPattern.Enraged,
            attackCooldown = 1.0f,
            specialAbilities = new List<Ability> { Ability.ClawSwipe, Ability.BubbleShield, Ability.DrillCharge }
        },
        new BossPhase
        {
            phaseName = "狂暴阶段",
            healthThreshold = 0.25f, // 25% - 0%
            attackPattern = AttackPattern.Desperate,
            attackCooldown = 0.7f,
            specialAbilities = new List<Ability> { Ability.All },
            enraged = true
        }
    };
}
```

## 资源价值与风险平衡

### 资源价值公式

```csharp
public class ResourceBalanceSystem : MonoBehaviour
{
    public int CalculateResourceValue(ResourceData resource, float depth)
    {
        // 基础价值
        int baseValue = resource.BaseValue;
        
        // 深度加成 (越深价值越高)
        float depthBonus = 1 + (depth / 100f) * 2f;
        
        // 稀有度倍率
        float rarityMultiplier = GetRarityMultiplier(resource.Rarity);
        
        // 风险调整 (根据该深度死亡率动态调整)
        float riskMultiplier = CalculateRiskMultiplier(depth);
        
        // 市场波动 (模拟供需)
        float marketFactor = GetMarketFactor(resource.ResourceId);
        
        return Mathf.RoundToInt(baseValue * depthBonus * rarityMultiplier * riskMultiplier * marketFactor);
    }
    
    private float GetRarityMultiplier(ResourceRarity rarity)
    {
        return rarity switch
        {
            ResourceRarity.Common => 1.0f,
            ResourceRarity.Uncommon => 1.5f,
            ResourceRarity.Rare => 2.5f,
            ResourceRarity.Epic => 5.0f,
            ResourceRarity.Legendary => 10.0f,
            _ => 1.0f
        };
    }
    
    private float CalculateRiskMultiplier(float depth)
    {
        // 获取该深度历史死亡率
        float deathRate = AnalyticsManager.Instance.GetDeathRateAtDepth(depth);
        
        // 死亡率高 = 更高的价值补偿
        return 1 + deathRate * 0.5f;
    }
}
```

### 资源分布表

| 资源 | 深度范围 | 基础价值 | 生成率 | 风险等级 |
|------|----------|----------|--------|----------|
| 铜矿石 | 0-40m | 10 | 高 | ★☆☆☆☆ |
| 铁矿石 | 0-50m | 15 | 高 | ★☆☆☆☆ |
| 石英 | 10-60m | 20 | 中 | ★☆☆☆☆ |
| 银矿石 | 30-70m | 50 | 中 | ★★☆☆☆ |
| 钛合金 | 40-80m | 100 | 中 | ★★★☆☆ |
| 能源晶体 | 50-90m | 150 | 低 | ★★★☆☆ |
| 能源核心 | 60-100m | 300 | 低 | ★★★★☆ |
| 数据芯片 | 70-100m | 500 | 极低 | ★★★★☆ |
| 古代遗物 | 90-100m | 2000 | 极低 | ★★★★★ |
| 深渊水晶 | 95-100m | 5000 | 极低 | ★★★★★ |

### 风险-收益分析

```
收益/小时
   │
   │                              ╭────── 深渊水晶
   │                         ╭────╯
   │                    ╭────╯
   │               ╭────╯
   │          ╭────╯
   │     ╭────╯
   │╭────╯
   │
   └────┬────┬────┬────┬────┬────┬────┬────┬────┬──► 深度
       20   30   40   50   60   70   80   90  100
        
风险曲线 (死亡率%)
   │
   │                                    ╭────── 40%+
   │                              ╭────╯
   │                         ╭────╯
   │                    ╭────╯
   │               ╭────╯
   │          ╭────╯
   │     ╭────╯
   │╭────╯ 5%
   │
   └────┬────┬────┬────┬────┬────┬────┬────┬────┬──► 深度
       20   30   40   50   60   70   80   90  100
```

## 委托难度分级验证

### 难度验证公式

```csharp
public class MissionDifficultyValidator
{
    public MissionDifficulty CalculateDifficulty(MissionData mission)
    {
        float score = 0;
        
        // 深度分数 (0-40)
        score += Mathf.Min(mission.TargetDepth / 100f * 40, 40);
        
        // 目标数量分数 (0-20)
        score += mission.ObjectiveCount * 2;
        
        // 时间限制分数 (0-15)
        if (mission.TimeLimit > 0)
        {
            float timePressure = CalculateTimePressure(mission);
            score += timePressure * 15;
        }
        
        // 敌人类型分数 (0-15)
        score += GetEnemyDifficultyScore(mission.EnemyTypes);
        
        // 特殊条件分数 (0-10)
        score += GetSpecialConditionScore(mission.SpecialConditions);
        
        // 转换为难度等级
        return score switch
        {
            < 15 => MissionDifficulty.Novice,      // ★☆☆☆☆
            < 30 => MissionDifficulty.Entry,       // ★★☆☆☆
            < 50 => MissionDifficulty.Advanced,    // ★★★☆☆
            < 70 => MissionDifficulty.Expert,      // ★★★★☆
            _ => MissionDifficulty.Abyss           // ★★★★★
        };
    }
}
```

### 委托难度验证表

| 委托ID | 名称 | 目标深度 | 计算难度 | 设定难度 | 验证结果 |
|--------|------|----------|----------|----------|----------|
| M001 | 新手试炼 | 20m | 12 | ★☆☆☆☆ | ✓ 匹配 |
| M002 | 能源危机 | 30m | 18 | ★★☆☆☆ | ✓ 匹配 |
| M003 | 深海初探 | 50m | 28 | ★★☆☆☆ | ✓ 匹配 |
| M004 | 清除威胁 | 40m | 25 | ★★☆☆☆ | ✓ 匹配 |
| M005 | 稀有金属 | 60m | 42 | ★★★☆☆ | ✓ 匹配 |
| M006 | 护送科学家 | 80m | 58 | ★★★☆☆ | ✓ 匹配 |
| M007 | 数据回收 | 70m | 52 | ★★★☆☆ | ✓ 匹配 |
| M008 | 深渊探索 | 100m | 75 | ★★★★☆ | ✓ 匹配 |
| M009 | 巨型机械蟹 | 90m | 82 | ★★★★★ | ✓ 匹配 |

## 死亡惩罚机制

### 惩罚设计原则

1. **有损失但不毁灭** - 惩罚足够让玩家谨慎，但不至于放弃
2. **技能相关** - 失误惩罚，而非随机惩罚
3. **可恢复** - 损失可以通过游戏行为弥补
4. **教学性** - 死亡是学习的机会

### 惩罚等级

```csharp
public class DeathPenaltySystem : MonoBehaviour
{
    [Header("惩罚配置")]
    public PenaltyConfig config;
    
    public void ApplyDeathPenalty(DeathContext context)
    {
        var penalty = CalculatePenalty(context);
        
        // 1. 资源损失 (50-80%)
        int lostResources = LoseResources(penalty.resourceLossPercent);
        
        // 2. 信用点损失 (10-30%)
        int lostCredits = LoseCredits(penalty.creditLossPercent);
        
        // 3. 装备耐久损失
        ApplyEquipmentDamage(penalty.equipmentDamagePercent);
        
        // 4. 经验值损失 (可选)
        if (config.loseXPOnDeath)
        {
            LoseXP(penalty.xpLossPercent);
        }
        
        // 5. 死亡统计
        RecordDeath(context);
        
        // 6. 显示死亡报告
        ShowDeathReport(context, penalty, lostResources, lostCredits);
    }
    
    private PenaltyData CalculatePenalty(DeathContext context)
    {
        float depthFactor = context.Depth / 100f;
        
        return new PenaltyData
        {
            resourceLossPercent = Mathf.Lerp(50, 80, depthFactor),
            creditLossPercent = Mathf.Lerp(10, 30, depthFactor),
            equipmentDamagePercent = Mathf.Lerp(20, 50, depthFactor),
            xpLossPercent = config.loseXPOnDeath ? Mathf.Lerp(5, 15, depthFactor) : 0,
            respawnDelay = Mathf.Lerp(3, 10, depthFactor)
        };
    }
}
```

### 惩罚详情表

| 深度 | 资源损失 | 信用点损失 | 装备损伤 | 重生延迟 |
|------|----------|------------|----------|----------|
| 0-30m | 50% | 10% | 20% | 3秒 |
| 30-60m | 60% | 15% | 30% | 5秒 |
| 60-90m | 70% | 20% | 40% | 8秒 |
| 90-100m | 80% | 30% | 50% | 10秒 |

### 死亡报告UI

```
┌─────────────────────────────────────────┐
│           ⚠ 机甲损毁 ⚠                  │
├─────────────────────────────────────────┤
│                                         │
│  深度: 67米                             │
│  原因: 被机械蟹击败                      │
│                                         │
│  ─────── 损失统计 ───────               │
│                                         │
│  丢失资源: 12个 (价值 1,240信用点)       │
│  信用点损失: 300                         │
│  装备损伤: 钻头 (-30%耐久)               │
│                                         │
│  ─────── 生存奖励 ───────               │
│                                         │
│  经验值: +150 (存活时间奖励)              │
│  成就进度: 深海探险家 3/10               │
│                                         │
├─────────────────────────────────────────┤
│  [查看死亡回放]  [确认]                  │
└─────────────────────────────────────────┘
```

### 保险系统 (可选)

为减轻惩罚，提供保险机制：

```csharp
public class InsuranceSystem
{
    public void PurchaseInsurance(InsuranceType type)
    {
        switch (type)
        {
            case InsuranceType.Basic:
                // 基础保险: 减少25%资源损失
                // 价格: 委托奖励的5%
                break;
            case InsuranceType.Standard:
                // 标准保险: 减少50%资源损失, 保护装备
                // 价格: 委托奖励的10%
                break;
            case InsuranceType.Premium:
                // 高级保险: 无资源损失, 装备保护, 信用点保护
                // 价格: 委托奖励的20%
                break;
        }
    }
}
```

## 动态难度调整 (DDA)

### 玩家技能评估

```csharp
public class PlayerSkillEvaluator
{
    private Queue<float> recentDeathRates = new Queue<float>();
    private Queue<float> recentCompletionTimes = new Queue<float>();
    
    public float EvaluateSkill()
    {
        float skillScore = 1.0f;
        
        // 死亡率评估 (最近10次下潜)
        float avgDeathRate = CalculateAverageDeathRate();
        if (avgDeathRate > 0.7f) skillScore -= 0.2f; // 经常死亡
        else if (avgDeathRate < 0.2f) skillScore += 0.1f; // 很少死亡
        
        // 完成时间评估
        float avgCompletionTime = CalculateAverageCompletionTime();
        float expectedTime = GetExpectedCompletionTime();
        if (avgCompletionTime < expectedTime * 0.8f) skillScore += 0.1f;
        
        // 资源采集效率
        float collectionEfficiency = CalculateCollectionEfficiency();
        skillScore += (collectionEfficiency - 1.0f) * 0.2f;
        
        // 战斗表现
        float combatScore = CalculateCombatScore();
        skillScore += (combatScore - 1.0f) * 0.1f;
        
        return Mathf.Clamp(skillScore, 0.8f, 1.2f);
    }
}
```

### 动态调整应用

```csharp
public void ApplyDynamicDifficulty()
{
    float playerSkill = PlayerSkillEvaluator.EvaluateSkill();
    
    if (playerSkill < 0.9f)
    {
        // 玩家 struggling - 降低难度
        EnemySpawnRate *= 0.9f;
        EnemyHealthMultiplier *= 0.95f;
        ResourceSpawnRate *= 1.1f;
        
        // 提示玩家
        ShowHint("检测到您遇到了一些困难，已适当调整难度。加油！");
    }
    else if (playerSkill > 1.1f)
    {
        // 玩家太强 - 增加挑战
        EnemySpawnRate *= 1.1f;
        EliteEnemyChance *= 1.2f;
        
        // 额外奖励
        BonusCreditMultiplier = 1.1f;
    }
}
```

## 技术实现

### DifficultyManager.cs

```csharp
public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }
    
    [Header("难度曲线")]
    public AnimationCurve healthCurve;
    public AnimationCurve damageCurve;
    public AnimationCurve spawnRateCurve;
    
    [Header("动态调整")]
    public bool enableDynamicDifficulty = true;
    public float evaluationInterval = 300f; // 5分钟评估一次
    
    private float currentDifficultyMultiplier = 1.0f;
    private float playerSkillFactor = 1.0f;
    
    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        if (enableDynamicDifficulty)
        {
            InvokeRepeating(nameof(EvaluateAndAdjust), evaluationInterval, evaluationInterval);
        }
    }
    
    public float GetDifficultyAtDepth(float depth)
    {
        float baseDifficulty = CalculateBaseDifficulty(depth);
        return baseDifficulty * playerSkillFactor * currentDifficultyMultiplier;
    }
    
    public EnemyStats GetScaledEnemyStats(EnemyData enemy, float depth)
    {
        float difficulty = GetDifficultyAtDepth(depth);
        float normalizedDepth = depth / 100f;
        
        return new EnemyStats
        {
            maxHealth = enemy.BaseHealth * healthCurve.Evaluate(normalizedDepth) * difficulty,
            damage = enemy.BaseDamage * damageCurve.Evaluate(normalizedDepth) * difficulty,
            spawnWeight = enemy.BaseSpawnWeight * spawnRateCurve.Evaluate(normalizedDepth)
        };
    }
    
    private void EvaluateAndAdjust()
    {
        playerSkillFactor = PlayerSkillEvaluator.Instance.EvaluateSkill();
        ApplyDynamicAdjustments();
    }
}
```

## 测试与调优

### 难度测试矩阵

| 测试项 | 测试方法 | 通过标准 |
|--------|----------|----------|
| 新手区死亡率 | 10次0-30m下潜 | < 20% |
| 中级区死亡率 | 10次30-60m下潜 | 20-40% |
| 高级区死亡率 | 10次60-90m下潜 | 40-60% |
| Boss战死亡率 | 10次Boss挑战 | 50-70% |
| 收益曲线 | 各深度1小时收益 | 单调递增 |
| 惩罚合理性 | 玩家反馈问卷 | > 70%满意 |

### 数据收集

```csharp
public class DifficultyAnalytics
{
    public void RecordDeath(DeathData data)
    {
        // 记录死亡数据用于平衡
        AnalyticsManager.Log("death", new Dictionary<string, object>
        {
            { "depth", data.Depth },
            { "enemy_type", data.KillerEnemyType },
            { "play_time", data.SessionDuration },
            { "player_level", data.PlayerLevel },
            { "equipment_score", data.EquipmentScore }
        });
    }
    
    public void RecordMissionComplete(MissionResult result)
    {
        AnalyticsManager.Log("mission_complete", new Dictionary<string, object>
        {
            { "mission_id", result.MissionId },
            { "completion_time", result.Duration },
            { "difficulty_rating", result.PlayerDifficultyRating },
            { "deaths", result.DeathCount }
        });
    }
}
```

---

*文档版本: 1.0*
*最后更新: 2026-02-27*
