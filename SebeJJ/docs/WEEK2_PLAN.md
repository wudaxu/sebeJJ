# SebeJJ - 第二周开发计划

## 目标
实现战斗系统、武器系统和基础敌人AI

---

## 里程碑
- [ ] 武器系统完整实现
- [ ] 伤害系统与生命值
- [ ] 基础敌人类型实现
- [ ] 敌人AI状态机
- [ ] 战斗循环可玩

---

## 每日任务分解

### Day 8: 武器系统框架
**目标:** 建立武器系统基础架构

#### 任务清单
- [ ] 创建 `WeaponData` ScriptableObject
- [ ] 实现 `WeaponSystem` 管理器
- [ ] 实现武器基类 `Weapon`
- [ ] 实现武器切换逻辑
- [ ] 创建武器配置数据

#### 武器类型定义
```csharp
public enum WeaponType
{
    MiningLaser,    // 采矿激光 - 持续伤害，适合采矿
    Harpoon,        // 鱼叉 - 单体高伤，可拖拽
    Torpedo,        // 鱼雷 - 范围伤害，慢速
    SonicPulse,     // 声波脉冲 - 击退效果
    PlasmaCutter    // 等离子切割器 - 近战高伤
}

[CreateAssetMenu(fileName = "Weapon", menuName = "SebeJJ/Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    public WeaponType type;
    public float damage;
    public float fireRate;
    public float range;
    public float energyCost;
    public GameObject projectilePrefab;
}
```

#### 验收标准
- [ ] 武器SO可配置
- [ ] 武器切换功能正常
- [ ] 能量消耗计算正确

---

### Day 9: 投射物与伤害系统
**目标:** 实现投射物和伤害计算

#### 任务清单
- [ ] 实现 `Projectile` 投射物基类
- [ ] 实现 `DamageSystem` 伤害管理器
- [ ] 实现 `IDamageable` 接口
- [ ] 实现伤害类型系统
- [ ] 实现暴击/弱点系统

#### 核心代码
```csharp
public interface IDamageable
{
    void TakeDamage(DamageInfo damageInfo);
    void Heal(float amount);
    bool IsAlive { get; }
}

public struct DamageInfo
{
    public float amount;
    public DamageType type;
    public Vector2 direction;
    public GameObject source;
    public bool isCritical;
}

public enum DamageType
{
    Physical,
    Energy,
    Explosive,
    Pressure,
    Corrosive
}
```

#### 验收标准
- [ ] 投射物飞行轨迹正常
- [ ] 伤害计算正确
- [ ] 伤害类型抗性生效
- [ ] 死亡事件触发

---

### Day 10: 玩家战斗实现
**目标:** 完成玩家机甲的战斗功能

#### 任务清单
- [ ] 集成武器系统到MechController
- [ ] 实现瞄准系统 (鼠标/右摇杆)
- [ ] 实现射击输入处理
- [ ] 实现武器冷却和能量管理
- [ ] 创建基础武器特效

#### 瞄准系统
```csharp
public class MechAim : MonoBehaviour
{
    public Transform weaponPivot;
    public float aimSmoothness = 10f;
    
    private void Update()
    {
        // 获取瞄准方向
        Vector2 aimInput = GetAimInput();
        float targetAngle = Mathf.Atan2(aimInput.y, aimInput.x) * Mathf.Rad2Deg;
        
        // 平滑旋转
        float currentAngle = weaponPivot.eulerAngles.z;
        float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, 
            aimSmoothness * Time.deltaTime);
        weaponPivot.rotation = Quaternion.Euler(0, 0, newAngle);
    }
}
```

#### 验收标准
- [ ] 瞄准响应灵敏
- [ ] 射击输入无延迟
- [ ] 武器冷却显示正确
- [ ] 能量不足时无法射击

---

### Day 11: 敌人基类与状态机
**目标:** 建立敌人AI基础框架

#### 任务清单
- [ ] 创建 `Enemy` 基类
- [ ] 实现 `StateMachine` 状态机
- [ ] 实现AI状态基类
- [ ] 实现Idle/Patrol状态
- [ ] 实现Chase状态

#### 状态机架构
```csharp
public class StateMachine
{
    private IState currentState;
    private Dictionary<Type, IState> states = new();
    
    public void AddState(IState state);
    public void ChangeState<T>() where T : IState;
    public void Update();
}

public interface IState
{
    void Enter();
    void Update();
    void Exit();
}

// 具体状态实现
public class IdleState : IState { }
public class PatrolState : IState { }
public class ChaseState : IState { }
public class AttackState : IState { }
```

#### 验收标准
- [ ] 状态切换正常
- [ ] 状态行为正确执行
- [ ] 状态退出清理正常

---

### Day 12: 敌人类型实现 - Part 1
**目标:** 实现基础敌人类型

#### 任务清单
- [ ] 实现 `DeepJellyfish` (深海水母)
  - 漂浮移动
  - 触手攻击
  - 发光效果
- [ ] 实现 `SecurityDrone` (安保无人机)
  - 巡逻行为
  - 激光射击
  - 警报系统
- [ ] 创建敌人预制体
- [ ] 配置敌人数据SO

#### DeepJellyfish配置
```csharp
[CreateAssetMenu(fileName = "JellyfishStats", menuName = "SebeJJ/EnemyStats")]
public class EnemyStats : ScriptableObject
{
    public string enemyName;
    public float maxHealth = 50f;
    public float moveSpeed = 2f;
    public float detectionRange = 8f;
    public float attackRange = 3f;
    public float attackDamage = 10f;
    public float attackCooldown = 2f;
    public List<DropItem> dropTable;
}
```

#### 验收标准
- [ ] 水母漂浮动画自然
- [ ] 无人机巡逻路径合理
- [ ] 敌人检测玩家正常
- [ ] 敌人死亡掉落物品

---

### Day 13: 敌人类型实现 - Part 2
**目标:** 实现更多敌人类型和Boss原型

#### 任务清单
- [ ] 实现 `AnglerFish` (鮟鱇鱼)
  - 伪装待机
  - 快速冲刺攻击
  - 诱饵灯效果
- [ ] 实现 `DefenseTurret` (防御炮塔)
  - 固定位置
  - 旋转瞄准
  - 连发子弹
- [ ] 设计Boss原型框架
- [ ] 实现攻击预警系统

#### 攻击预警
```csharp
public class AttackWarning : MonoBehaviour
{
    public float warningTime = 1f;
    public GameObject warningIndicator;
    
    public void ShowWarning(Vector2 position)
    {
        // 显示预警指示器
        // 延迟后执行攻击
    }
}
```

#### 验收标准
- [ ] 鮟鱇鱼伪装/攻击切换正常
- [ ] 炮塔旋转瞄准平滑
- [ ] 攻击预警清晰可见
- [ ] 不同敌人行为差异化明显

---

### Day 14: 战斗整合与平衡
**目标:** 整合战斗系统，进行平衡调整

#### 任务清单
- [ ] 整合所有战斗系统
- [ ] 调整武器伤害数值
- [ ] 调整敌人属性数值
- [ ] 实现战斗反馈 (屏幕震动、音效)
- [ ] 添加伤害数字显示
- [ ] 进行战斗测试

#### 数值平衡表
| 武器 | 伤害 | 射速 | 能量消耗 | 备注 |
|------|------|------|----------|------|
| 采矿激光 | 10/s | 持续 | 5/s | 基础武器 |
| 鱼叉 | 50 | 0.5/s | 20 | 高单发 |
| 鱼雷 | 80 | 0.3/s | 40 | 范围伤害 |
| 声波脉冲 | 20 | 1/s | 15 | 击退 |
| 等离子切割 | 100 | 0.2/s | 50 | 近战 |

#### 验收标准
- [ ] 战斗节奏合理
- [ ] 武器各有特色
- [ ] 敌人难度递进
- [ ] 无明显Bug

---

## 技术依赖

| 依赖项 | 用途 | 状态 |
|--------|------|------|
| 第一周代码 | 基础框架 | 依赖 |
| 2D Physics | 碰撞检测 | 必须 |
| Particle System | 特效 | 推荐 |

---

## 风险与应对

| 风险 | 可能性 | 应对措施 |
|------|--------|----------|
| AI行为不自然 | 中 | 参考优秀2D游戏AI设计 |
| 战斗手感差 | 中 | 多轮测试调参 |
| 性能问题 | 低 | 对象池，限制同屏敌人 |

---

## 成功标准

- [ ] 玩家可以使用多种武器战斗
- [ ] 敌人AI行为合理
- [ ] 战斗反馈清晰
- [ ] 数值基本平衡
- [ ] 无明显Bug

---

**计划版本:** 1.0  
**创建日期:** 2026-02-26  
**负责人:** 架构师 Agent
