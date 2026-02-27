# 赛博机甲 SebeJJ - 敌人AI行为树设计文档

## 概述

本文档描述了SebeJJ游戏中敌人的AI行为树设计，采用分层状态机（Hierarchical State Machine）实现。

---

## 1. 机械鲨鱼 (MechShark) AI行为树

### 1.1 属性参数

| 参数 | 数值 | 说明 |
|------|------|------|
| 血量 | 150 | 中等生命值 |
| 攻击力 | 20 | 基础攻击 |
| 冲撞伤害 | 40 (20×2) | 冲撞时伤害翻倍 |
| 巡逻速度 | 4 m/s | 正常游动速度 |
| 追击速度 | 12 m/s | 发现玩家后的速度 |
| 冲撞速度 | 20 m/s | 冲撞时的爆发速度 |
| 检测范围 | 15m | 发现玩家的距离 |
| 攻击范围 | 3m | 普通攻击距离 |
| 冲撞冷却 | 3s | 两次冲撞的间隔 |
| 出现深度 | 60-100米 | 生成深度限制 |

### 1.2 状态机图

```
                    ┌─────────────┐
         ┌─────────│   Patrol    │◄────────┐
         │         │   (巡逻)     │         │
         │         └──────┬──────┘         │
         │                │ 玩家进入检测范围  │
         │                ▼                │
         │         ┌─────────────┐         │
         │         │   Alert     │         │
         │         │   (警觉)     │         │
         │         └──────┬──────┘         │
         │        远离/    │    接近        │
         │       丢失目标   │   进入追击范围   │
         │                ▼                │
         │    ┌─────────────────────────┐  │
         └───►│        Chase            │  │
              │       (追击)            │──┘
              └───────────┬─────────────┘
                          │ 进入冲撞范围且CD完成
                          ▼
              ┌─────────────────────────┐
              │    Charge Windup        │
              │     (冲撞蓄力)           │
              │   - 后退准备姿势         │
              │   - 0.8秒预警时间        │
              └───────────┬─────────────┘
                          │ 蓄力完成
                          ▼
              ┌─────────────────────────┐
              │       Charge            │
              │      (冲撞中)           │
              │   - 直线高速移动         │
              │   - 检测碰撞             │
              │   - 1.5秒持续时间        │
              └───────────┬─────────────┘
                          │ 撞到目标/障碍物/超时
                          ▼
              ┌─────────────────────────┐
              │       Recover           │
              │      (恢复状态)          │
              │   - 减速停止             │
              │   - 等待CD               │
              └─────────────────────────┘
```

### 1.3 行为逻辑详解

#### Patrol (巡逻状态)
- **进入条件**: 初始状态 / 丢失目标后返回
- **行为**:
  - 在出生点周围随机巡逻
  - 使用缓慢游动动画
  - 定期更换巡逻目标点
- **退出条件**: 玩家进入检测范围(15m)

#### Alert (警觉状态)
- **进入条件**: 检测到玩家但距离较远
- **行为**:
  - 停止移动，朝向玩家
  - 播放警觉动画（眼睛发光）
  - 准备追击姿态
- **退出条件**: 
  - 玩家远离 → 返回Patrol
  - 玩家接近 → 进入Chase

#### Chase (追击状态)
- **进入条件**: 玩家进入追击范围
- **行为**:
  - 高速向玩家游动
  - 持续调整朝向
  - 尾流特效
- **退出条件**:
  - 玩家远离 → 返回Alert
  - 进入冲撞范围且CD完成 → 进入ChargeWindup

#### Charge Windup (冲撞蓄力)
- **进入条件**: 距离玩家足够近且冲撞冷却完成
- **行为**:
  - 短暂后退（0.8秒）
  - 身体后仰蓄力姿势
  - 发出警告音效
- **退出条件**: 蓄力时间结束 → 进入Charge

#### Charge (冲撞状态)
- **进入条件**: 蓄力完成
- **行为**:
  - 直线高速冲撞（20m/s）
  - 持续1.5秒或直到碰撞
  - 冲撞粒子特效
  - 检测前方碰撞
- **退出条件**:
  - 撞到玩家 → 造成伤害并进入Recover
  - 撞到障碍物 → 进入Recover
  - 超时 → 进入Recover

#### Recover (恢复状态)
- **进入条件**: 冲撞结束
- **行为**:
  - 快速减速
  - 短暂硬直
  - 等待冲撞CD（3秒）
- **退出条件**: CD完成 → 根据距离进入Chase或Alert

### 1.4 碰撞检测

```csharp
// 冲撞碰撞检测逻辑
RaycastHit hit;
float checkDistance = 3f;
if (Physics.Raycast(transform.position, chargeDirection, out hit, checkDistance))
{
    if (hit.collider.CompareTag("Player"))
    {
        // 造成40伤害 + 击退效果
        ApplyChargeDamage(hit.collider.gameObject);
    }
    else
    {
        // 撞到墙壁，停止冲撞
        TransitionToState(SharkAIState.Recover);
    }
}
```

---

## 2. 深海章鱼 (DeepOctopus) AI行为树

### 2.1 属性参数

| 参数 | 数值 | 说明 |
|------|------|------|
| 血量 | 200 | 较高生命值 |
| 触手伤害 | 15 | 每次扫击伤害 |
| 触手数量 | 8 | 可攻击的触手 |
| 触手范围 | 8m | 攻击范围 |
| 触手CD | 2s | 攻击间隔 |
| 漂浮速度 | 2 m/s | 缓慢漂浮 |
| 逃跑速度 | 4 m/s | 墨汁后逃跑速度 |
| 检测范围 | 20m | 发现玩家的距离 |
| 墨汁冷却 | 8s | 墨汁技能CD |
| 致盲时间 | 3s | 墨汁致盲持续时间 |
| 墨汁云持续 | 5s | 墨汁云存在时间 |
| 墨汁使用次数 | 3 | 最大使用次数 |
| 出现深度 | 80-100米 | 生成深度限制 |

### 2.2 状态机图

```
                    ┌─────────────┐
         ┌─────────│   Floating  │◄──────────────────┐
         │         │   (漂浮)     │                   │
         │         └──────┬──────┘                   │
         │                │                          │
         │    ┌───────────┼───────────┐              │
         │    │           │           │              │
         │    │ 玩家进入   │   低血量/  │              │
         │    │ 触手范围   │   玩家太近  │              │
         │    │           │           │              │
         │    ▼           │           ▼              │
         │ ┌──────────┐   │    ┌─────────────┐       │
         │ │ Tentacle │   │    │  Ink Escape │       │
         │ │ Attack   │   │    │  (墨汁逃跑)  │       │
         │ │(触手攻击) │   │    └──────┬──────┘       │
         │ └────┬─────┘   │           │              │
         │      │         │           ▼              │
         │      │         │    ┌─────────────┐       │
         │      │         │    │   Fleeing   │───────┘
         │      │         │    │   (逃跑中)   │ 远离成功
         │      │         │    └─────────────┘
         │      │         │
         └──────┴─────────┘
              攻击完成/逃跑完成
```

### 2.3 行为逻辑详解

#### Floating (漂浮状态)
- **进入条件**: 初始状态 / 攻击或逃跑结束
- **行为**:
  - 缓慢上下漂浮
  - 触手自然摆动
  - 轻微水平漂移
  - 朝向玩家但不主动攻击
- **退出条件**:
  - 玩家进入触手范围 → 进入TentacleAttack
  - 低血量或玩家太近 → 进入InkEscape

#### Tentacle Attack (触手攻击)
- **进入条件**: 玩家在攻击范围内且CD完成
- **行为阶段**:
  1. **蓄力阶段** (0.5秒):
     - 触手向后收缩
     - 身体微微发光
  2. **攻击阶段** (1.5秒):
     - 8条触手向外扫击
     - 120度扇形范围
     - 中段造成伤害
  3. **恢复阶段** (0.5秒):
     - 触手复位
- **退出条件**: 攻击完成 → 返回Floating

#### Ink Escape (墨汁逃跑)
- **进入条件**: 
  - 生命值 < 40% 或
  - 玩家距离 < 7.5m (检测范围一半)
  - 墨汁次数 > 0 且 CD完成
- **行为**:
  1. 喷射墨汁云（身后3m）
  2. 墨汁云持续5秒，范围10m
  3. 范围内玩家致盲3秒
  4. 快速向远离玩家方向逃跑
- **退出条件**: 墨汁释放完成 → 进入Fleeing

#### Fleeing (逃跑状态)
- **进入条件**: 墨汁释放后
- **行为**:
  - 向远离玩家方向高速移动
  - 添加随机垂直变化
  - 不再主动攻击
- **退出条件**: 距离玩家 > 22.5m (1.5倍逃跑范围) → 返回Floating

### 2.4 触手攻击检测

```csharp
// 触手扫击伤害检测
private void DealTentacleDamage()
{
    Collider[] hitColliders = Physics.OverlapSphere(transform.position, tentacleAttackRange);
    foreach (var hitCollider in hitColliders)
    {
        if (hitCollider.CompareTag("Player"))
        {
            // 检查是否在触手扫击角度内
            Vector3 directionToPlayer = (hitCollider.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToPlayer);
            
            if (angle < tentacleSweepAngle / 2)
            {
                var damageable = hitCollider.GetComponent<IDamageable>();
                damageable?.TakeDamage(tentacleDamage);
            }
        }
    }
}
```

### 2.5 墨汁效果实现

```csharp
// 墨汁云效果
public class InkCloud : MonoBehaviour
{
    // 扩散阶段: 0.5秒内从0扩展到最大
    // 维持阶段: 持续致盲进入的玩家
    // 消散阶段: 最后1秒逐渐缩小
    
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var blindable = other.GetComponent<IBlindable>();
            if (blindable != null && !blindable.IsBlinded)
            {
                blindable.ApplyBlind(blindDuration);
            }
        }
    }
}
```

---

## 3. 通用AI系统架构

### 3.1 类继承关系

```
MonoBehaviour
    │
    └── EnemyBase (抽象基类)
            │
            ├── MechShark (机械鲨鱼)
            │
            └── DeepOctopus (深海章鱼)
```

### 3.2 EnemyBase核心功能

```csharp
public abstract class EnemyBase : MonoBehaviour
{
    // 生命周期
    protected abstract void InitializeAI();  // 初始化AI
    protected abstract void UpdateAI();      // 每帧更新
    
    // 战斗
    public abstract void PerformAttack();    // 执行攻击
    public virtual void TakeDamage(float damage);  // 受到伤害
    protected virtual void Die();            // 死亡处理
    
    // 工具方法
    protected bool IsPlayerInDetectionRange();
    protected bool IsPlayerInAttackRange();
    protected Vector3 GetDirectionToPlayer();
    protected float GetDistanceToPlayer();
}
```

### 3.3 状态转换规则

1. **状态转换优先级**:
   - 死亡状态 > 所有其他状态
   - 控制状态（眩晕/致盲）> 攻击状态
   - 逃跑状态 > 追击状态

2. **状态退出清理**:
   - 每个状态退出时清理特效和动画
   - 停止相关协程
   - 重置物理状态

3. **状态进入准备**:
   - 播放进入动画
   - 初始化状态变量
   - 触发相关特效

---

## 4. 性能优化建议

### 4.1 距离检测优化

```csharp
// 使用分层检测，避免每帧检测所有玩家
private void Update()
{
    // 远距离: 每10帧检测一次
    // 中距离: 每5帧检测一次  
    // 近距离: 每帧检测
}
```

### 4.2 对象池使用

- 墨汁云对象池
- 粒子效果对象池
- 伤害数字对象池

### 4.3 LOD系统

- 远距离: 简化动画，关闭AI
- 中距离: 正常AI，简化特效
- 近距离: 完整效果

---

## 5. 扩展接口

### 5.1 伤害接口

```csharp
public interface IDamageable
{
    void TakeDamage(float damage);
    void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitDirection);
}
```

### 5.2 控制效果接口

```csharp
public interface IBlindable
{
    bool IsBlinded { get; }
    void ApplyBlind(float duration);
    void RemoveBlind();
}

public interface IStunnable
{
    bool IsStunned { get; }
    void ApplyStun(float duration);
    void RemoveStun();
}
```

---

## 6. 美术资源需求

### 6.1 机械鲨鱼

详见: `EnemyArtRequirements.md`

### 6.2 深海章鱼

详见: `EnemyArtRequirements.md`

---

*文档版本: 1.0*
*创建日期: 2026-02-27*
*作者: 敌人设计师*
