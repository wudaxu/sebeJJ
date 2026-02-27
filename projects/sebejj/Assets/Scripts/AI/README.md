# AI系统开发文档

## 概述

本文档描述了赛博机甲SebeJJ项目的AI系统架构，包括状态机框架、A*寻路系统和三种敌人的AI行为实现。

---

## 文件结构

```
Assets/Scripts/AI/
├── AIStateMachine.cs      # AI状态机基类 (AI-001~003)
├── IAIState.cs            # AI状态接口 (AI-002)
├── AIPerception.cs        # AI感知系统 (AI-004~005)
├── AStarPathfinding.cs    # A*寻路系统 (PT-001~006)
├── PathFollower.cs        # 路径跟随组件
├── EnemyBase.cs           # 敌人基类 (E1-001, E2-001, E3-001)
├── MechFishAI.cs          # 机械鱼AI (E1-002~003)
├── MechCrabAI.cs          # 机械蟹AI (E2-002~004)
├── MechJellyfishAI.cs     # 机械水母AI (E3-002~003)
├── AIDebugger.cs          # AI调试工具 (AI-007)
├── AIStressTest.cs        # AI压力测试 (AI-008)
├── AIUnitTests.cs         # AI单元测试
└── AITestSceneSetup.cs    # 测试场景设置
```

---

## AI状态机框架

### 核心组件

#### 1. AIStateMachine (状态机基类)

负责管理所有AI状态的切换和生命周期。

**主要功能：**
- 状态注册/注销
- 状态切换（支持条件和强制切换）
- 状态转换条件注册
- 平滑过渡支持
- 调试可视化

**使用示例：**
```csharp
// 获取状态机
AIStateMachine stateMachine = GetComponent<AIStateMachine>();

// 注册状态
stateMachine.RegisterState(EnemyState.Idle, new IdleState(this));
stateMachine.RegisterState(EnemyState.Chase, new ChaseState(this));

// 切换状态
stateMachine.ChangeState(EnemyState.Chase);

// 注册转换条件
stateMachine.RegisterTransitionCondition((from, to) => {
    return health < maxHealth * 0.3f;
}, EnemyState.Flee);
```

#### 2. IAIState (状态接口)

定义AI状态的完整生命周期接口。

**接口方法：**
- `Initialize()` - 初始化状态
- `OnEnter()` - 进入状态时调用
- `OnUpdate()` - 每帧更新
- `OnFixedUpdate()` - 固定时间间隔更新
- `OnExit()` - 退出状态时调用
- `OnDispose()` - 销毁状态时调用

#### 3. AIStateBase (状态基类)

提供状态的默认实现，简化状态创建。

**使用示例：**
```csharp
public class IdleState : AIStateBase
{
    public override void OnEnter()
    {
        // 进入待机状态
    }
    
    public override void OnUpdate(float deltaTime)
    {
        // 每帧更新
        if (ShouldChase())
        {
            ChangeState(EnemyState.Chase);
        }
    }
    
    public override void OnExit()
    {
        // 退出待机状态
    }
}
```

---

## A*寻路系统

### 核心组件

#### 1. AStarPathfinding

实现2D网格A*寻路算法，支持动态障碍物、路径平滑和性能优化。

**主要功能：**
- 网格生成和动态更新
- 异步路径计算
- 路径平滑处理
- 路径缓存
- 视线检测

**使用示例：**
```csharp
// 请求路径
AStarPathfinding pathfinding = FindObjectOfType<AStarPathfinding>();
pathfinding.RequestPath(startPos, endPos, (path, success) => {
    if (success)
    {
        // 使用路径
        FollowPath(path);
    }
});

// 立即计算路径
var (path, success) = pathfinding.FindPathImmediate(startPos, endPos);

// 更新网格区域
pathfinding.UpdateGridRegion(obstaclePosition, radius);
```

#### 2. PathFollower

使游戏对象能够沿着计算出的路径移动。

**使用示例：**
```csharp
PathFollower follower = GetComponent<PathFollower>();

// 设置目标
follower.SetTarget(playerTransform);

// 或设置目标位置
follower.SetTargetPosition(targetPosition);

// 控制跟随
follower.StartFollowing();
follower.StopFollowing();
```

---

## AI感知系统

### 核心组件

#### AIPerception

提供视觉侦测、听觉侦测和记忆系统。

**主要功能：**
- 视野检测（角度和距离）
- 听觉检测（噪音感知）
- 目标记忆（遗忘机制）
- 主要目标选择

**使用示例：**
```csharp
AIPerception perception = GetComponent<AIPerception>();

// 检查是否有目标
if (perception.HasTarget)
{
    Transform target = perception.PrimaryTarget.Target;
    // 处理目标
}

// 强制设置目标
perception.ForceSetTarget(playerTransform);

// 获取最后已知位置
Vector3? lastPos = perception.GetLastKnownPosition(target);
```

---

## 敌人AI实现

### 1. 机械鱼 (MechFishAI)

**行为特点：**
- **Idle**: 原地悬浮，轻微摆动
- **Patrol**: 在区域内随机游动
- **Chase**: 发现玩家后快速追击
- **Attack**: 冲撞攻击（可蓄力冲撞）

**特殊能力：**
- 冲撞攻击：快速冲向目标，造成高额伤害
- 边界检测：不会游出指定范围

**配置参数：**
```csharp
[Header("机械鱼特有配置")]
public float wanderRadius = 8f;           // 游荡半径
public float chargeSpeed = 6f;            // 冲撞速度
public float chargeDuration = 0.5f;       // 冲撞持续时间
public float chargeCooldown = 2f;         // 冲撞冷却
```

### 2. 机械蟹 (MechCrabAI)

**行为特点：**
- **Patrol**: 在固定路线上巡逻
- **Defend**: 受到伤害时进入防御姿态，大幅减伤
- **Attack**: 钳击攻击，可连击
- **Alert**: 警戒状态，提高感知范围

**特殊能力：**
- 防御姿态：75%伤害减免
- 连击系统：最多3连击，伤害递增
- 巡逻路线：支持循环和往返模式

**配置参数：**
```csharp
[Header("机械蟹特有配置")]
public float defendDuration = 3f;              // 防御持续时间
public float defenseDamageReduction = 0.75f;   // 防御减伤率
public int maxComboCount = 3;                  // 最大连击数
public float comboDamageMultiplier = 1.2f;     // 连击伤害倍率
```

### 3. 机械水母 (MechJellyfishAI)

**行为特点：**
- **Idle**: 上下漂浮
- **Float**: 缓慢移动，保持漂浮动画
- **Charge**: 蓄力准备电击
- **Attack**: 释放电击AOE

**特殊能力：**
- 电击AOE：范围伤害，带距离衰减
- 漂浮动画：正弦波上下浮动
- 多段脉冲：可连续释放多次电击

**配置参数：**
```csharp
[Header("电击攻击")]
public float pulseRadius = 4f;             // 电击范围
public float pulseDamage = 15f;            // 基础伤害
public float pulseChargeTime = 1.5f;       // 蓄力时间
public int pulseTicks = 3;                 // 脉冲次数
```

---

## 调试工具

### 1. AIDebugger

提供AI状态机的实时调试信息和可视化。

**功能：**
- 实时显示所有AI状态
- 绘制状态标签
- 绘制路径线
- 绘制感知范围
- 状态转换日志

**快捷键：** F12 切换调试显示

### 2. AIStressTest

测试AI系统的性能和稳定性。

**使用方法：**
```csharp
// 在场景中添加AIStressTest组件
// 点击"开始压力测试"按钮
// 或调用：
GetComponent<AIStressTest>().StartTest();
```

### 3. AIUnitTests

提供AI系统的单元测试。

**测试内容：**
- 状态机初始化
- 状态注册和切换
- 感知系统
- 寻路系统
- 敌人基类功能

---

## 快速开始

### 1. 创建测试场景

1. 创建空场景
2. 添加 `AITestSceneSetup` 组件
3. 分配预制体引用
4. 点击 "设置测试场景"

### 2. 手动设置敌人

```csharp
// 创建机械鱼
GameObject fish = Instantiate(mechFishPrefab, position, rotation);
MechFishAI fishAI = fish.GetComponent<MechFishAI>();

// 设置目标
fishAI.Perception.ForceSetTarget(playerTransform);
```

### 3. 自定义敌人行为

继承 `EnemyBase` 类并实现抽象方法：

```csharp
public class CustomEnemy : EnemyBase
{
    protected override void InitializeStates()
    {
        // 注册自定义状态
        StateMachine.RegisterState(EnemyState.Idle, new CustomIdleState(this));
        // ...
    }
    
    public override void PerformAttack()
    {
        // 实现攻击逻辑
    }
}
```

---

## 性能优化建议

1. **状态机更新频率**：不需要每帧更新的AI可以降低更新频率
2. **寻路缓存**：利用A*寻路的路径缓存功能
3. **感知更新间隔**：调整AIPerception的更新间隔
4. **LOD系统**：远距离敌人降低AI复杂度
5. **对象池**：使用对象池管理敌人实例

---

## 扩展指南

### 添加新状态

1. 创建继承 `AIStateBase` 的类
2. 实现 `OnEnter`, `OnUpdate`, `OnExit` 方法
3. 在 `InitializeStates` 中注册状态

### 添加新敌人类型

1. 创建继承 `EnemyBase` 的类
2. 实现 `InitializeStates` 和 `PerformAttack`
3. 创建对应的预制体

### 扩展感知系统

实现 `INoiseMaker` 接口让对象能被听觉感知：

```csharp
public class PlayerController : MonoBehaviour, INoiseMaker
{
    public bool IsMakingNoise => isMoving;
    public float NoiseIntensity => moveSpeed;
}
```

---

## 版本历史

- **v1.0** (2026-02-27): 初始版本
  - 完成AI状态机框架
  - 完成A*寻路系统
  - 完成3种敌人AI实现
  - 添加调试和测试工具
