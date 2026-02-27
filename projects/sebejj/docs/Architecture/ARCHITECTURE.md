# SebeJJ 赛博机甲 - 技术架构图文档

> 本文档包含赛博机甲项目的完整技术架构可视化图表
> 生成时间: 2026-02-27
> 版本: 2.0

---

## 目录

1. [系统架构图](#1-系统架构图)
2. [类关系图](#2-类关系图)
3. [数据流图](#3-数据流图)
4. [模块依赖图](#4-模块依赖图)

---

## 1. 系统架构图

### 1.1 核心系统关系图

```mermaid
graph TB
    subgraph Core["核心层 Core"]
        GM[GameManager<br/>游戏管理器]
        SM[SaveManager<br/>存档管理器]
        EM[EventSystem<br/>事件系统]
        SL[ServiceLocator<br/>服务定位器]
    end

    subgraph Systems["系统层 Systems"]
        MM[MissionManager<br/>委托管理器]
        RM[ResourceManager<br/>资源管理器]
        DM[DiveManager<br/>下潜管理器]
        CM[CombatManager<br/>战斗管理器]
    end

    subgraph Managers["管理层 Managers"]
        UM[UIManager<br/>UI管理器]
        WM[WeaponManager<br/>武器管理器]
        AM[AudioManagerExtended<br/>音频管理器]
        EM2[EffectManager<br/>特效管理器]
    end

    subgraph AI["AI系统 AI"]
        ASM[AIStateMachine<br/>状态机]
        AP[AIPerception<br/>感知系统]
        ASP[AStarPathfinding<br/>寻路系统]
    end

    subgraph Combat["战斗系统 Combat"]
        DC[DamageCalculator<br/>伤害计算器]
        DF[DefenseSystem<br/>防御系统]
        CF[CombatFeedback<br/>战斗反馈]
        SS[ShieldSystem<br/>护盾系统]
    end

    subgraph Data["数据层 Data"]
        GD[GameData<br/>游戏数据]
        MD[MissionData<br/>委托数据]
        WD[WeaponData<br/>武器数据]
    end

    %% 核心层关系
    GM --> SM
    GM --> MM
    GM --> RM
    GM --> DM
    GM --> UM
    
    SM -.->|保存/加载| GD
    EM -.->|事件通知| GM
    
    %% 系统层关系
    MM -.->|委托完成| RM
    DM -.->|深度变化| CM
    RM -.->|资源采集| MM
    
    %% 管理层关系
    UM -.->|显示更新| GM
    WM -.->|武器切换| CM
    
    %% AI系统关系
    ASM --> AP
    ASM --> ASP
    AP -.->|目标发现| ASM
    
    %% 战斗系统关系
    CM --> DC
    CM --> CF
    DC --> DF
    DC --> SS
    
    %% 数据依赖
    MM --> MD
    WM --> WD
    GM --> GD
```

### 1.2 战斗系统架构

```mermaid
graph TB
    subgraph CombatFlow["战斗流程"]
        direction TB
        Attack[攻击发起] --> DamageCalc[伤害计算]
        DamageCalc --> Defense[防御判定]
        Defense --> Apply[伤害应用]
        Apply --> Feedback[反馈触发]
    end

    subgraph DamageSystem["伤害系统 DM"]
        DI[DamageInfo<br/>伤害信息]
        DC[DamageCalculator<br/>伤害计算器]
        DT[DamageType<br/>伤害类型]
        TT[TargetType<br/>目标类型]
    end

    subgraph DefenseSystem["防御系统 DF"]
        AS[ArmorSystem<br/>护甲系统]
        SS[ShieldSystem<br/>护盾系统]
        DS[DefenseSystem<br/>防御系统]
    end

    subgraph FeedbackSystem["反馈系统 FB"]
        CF[CombatFeedback<br/>战斗反馈]
        HN[HitReactionController<br/>受击反应]
        KF[KillFeedbackController<br/>击杀反馈]
        DN[DamageNumber<br/>伤害数字]
    end

    subgraph WeaponSystem["武器系统 WP"]
        WB[WeaponBase<br/>武器基类]
        MW[MeleeWeapon<br/>近战武器]
        RW[RangedWeapon<br/>远程武器]
        WC[WeaponComboSystem<br/>连击系统]
    end

    %% 伤害流程
    WB -->|创建| DI
    DI --> DC
    DC -->|计算| AS
    DC -->|计算| SS
    AS --> Apply
    SS --> Apply
    
    %% 反馈流程
    Apply --> CF
    CF --> HN
    CF --> KF
    CF --> DN
    
    %% 武器到伤害
    MW -->|近战攻击| DI
    RW -->|远程攻击| DI
    WC --> MW
```

### 1.3 AI系统架构

```mermaid
graph TB
    subgraph AIFramework["AI框架"]
        ASM[AIStateMachine<br/>状态机核心]
        IAS[IAIState<br/>状态接口]
        ASB[AIStateBase<br/>状态基类]
    end

    subgraph AIStates["AI状态"]
        Idle[IdleState<br/>待机]
        Patrol[PatrolState<br/>巡逻]
        Alert[AlertState<br/>警戒]
        Chase[ChaseState<br/>追击]
        Attack[AttackState<br/>攻击]
        Flee[FleeState<br/>逃跑]
        Stun[StunState<br/>眩晕]
        Dead[DeadState<br/>死亡]
    end

    subgraph Perception["感知系统 AI-004"]
        AP[AIPerception<br/>感知系统]
        PT[PerceptionTarget<br/>感知目标]
        PTY[PerceptionType<br/>感知类型]
    end

    subgraph Pathfinding["寻路系统 AI-003"]
        ASP[AStarPathfinding<br/>A*寻路]
        PF[PathFollower<br/>路径跟随]
        NG[NodeGrid<br/>节点网格]
    end

    subgraph EnemyAI["敌人AI实现"]
        EB[EnemyBase<br/>敌人基类]
        MFA[MechFishAI<br/>机械鱼AI]
        MCA[MechCrabAI<br/>机械蟹AI]
        MJA[MechJellyfishAI<br/>机械水母AI]
    end

    %% 框架关系
    ASM -->|管理| IAS
    IAS -->|实现| ASB
    ASB -->|派生| Idle
    ASB -->|派生| Patrol
    ASB -->|派生| Alert
    ASB -->|派生| Chase
    ASB -->|派生| Attack
    ASB -->|派生| Flee
    ASB -->|派生| Stun
    ASB -->|派生| Dead
    
    %% 感知关系
    AP -->|检测| PT
    PT -->|类型| PTY
    AP -.->|通知| ASM
    
    %% 寻路关系
    ASP -->|生成路径| PF
    ASP -->|使用| NG
    PF -.->|移动| ASM
    
    %% 敌人实现
    EB -->|包含| ASM
    EB -->|包含| AP
    EB -->|使用| ASP
    MFA -->|继承| EB
    MCA -->|继承| EB
    MJA -->|继承| EB
```

### 1.4 委托系统架构

```mermaid
graph TB
    subgraph MissionCore["委托核心"]
        MM[MissionManager<br/>委托管理器]
        MD[MissionData<br/>委托数据SO]
        M[Mission<br/>运行时委托]
    end

    subgraph MissionTypes["委托类型"]
        MT[MissionType<br/>类型枚举]
        Collection[Collection<br/>采集]
        Exploration[Exploration<br/>探索]
        Scan[Scan<br/>扫描]
        Delivery[Delivery<br/>运送]
        Elimination[Elimination<br/>清除]
    end

    subgraph MissionStatus["委托状态"]
        MS[MissionStatus<br/>状态枚举]
        Available[Available<br/>可用]
        Active[Active<br/>进行中]
        Completed[Completed<br/>已完成]
        Failed[Failed<br/>失败]
    end

    subgraph Objectives["目标系统"]
        MO[MissionObjective<br/>委托目标]
        RI[RewardItem<br/>奖励物品]
        MT2[MissionTracker<br/>委托追踪]
    end

    subgraph Events["委托事件"]
        OnAccept[OnMissionAccepted]
        OnComplete[OnMissionCompleted]
        OnFail[OnMissionFailed]
        OnUpdate[OnMissionUpdated]
    end

    %% 核心关系
    MM -->|管理| M
    MD -->|实例化| M
    MM -->|读取| MD
    
    %% 类型关系
    MM -->|使用| MT
    MT --> Collection
    MT --> Exploration
    MT --> Scan
    MT --> Delivery
    MT --> Elimination
    
    %% 状态关系
    M -->|状态| MS
    MS --> Available
    MS --> Active
    MS --> Completed
    MS --> Failed
    
    %% 目标关系
    M -->|包含| MO
    M -->|包含| RI
    MM -->|使用| MT2
    
    %% 事件关系
    MM -.->|触发| OnAccept
    MM -.->|触发| OnComplete
    MM -.->|触发| OnFail
    MM -.->|触发| OnUpdate
```

---

## 2. 类关系图

### 2.1 继承关系图

```mermaid
graph TB
    subgraph Unity["Unity基类"]
        MB[MonoBehaviour]
        SO[ScriptableObject]
    end

    subgraph CoreClasses["核心类"]
        GM[GameManager]
        SM[SaveManager]
        UM[UIManager]
        EM[EventSystem]
    end

    subgraph CombatClasses["战斗类"]
        WB[WeaponBase]
        MW[MeleeWeapon]
        RW[RangedWeapon]
        EB[EnemyBase]
    end

    subgraph AIClasses["AI类"]
        ASM[AIStateMachine]
        ASB[AIStateBase]
        IAS[IAIState<br/>接口]
    end

    subgraph DataClasses["数据类"]
        MD[MissionData]
        WD[WeaponData]
        CD[ChainsawData]
        ED[EMPData]
        PD[PlasmaCannonData]
    end

    subgraph EnemyImpl["敌人实现"]
        MFA[MechFishAI]
        MCA[MechCrabAI]
        MJA[MechJellyfishAI]
        DO[DeepOctopus]
        MS[MechShark]
    end

    subgraph BossClasses["Boss类"]
        ICB[IronClawBeastBoss]
        BS[BossStates]
        BA[BossArena]
    end

    %% Unity继承
    MB --> GM
    MB --> SM
    MB --> UM
    MB --> EM
    MB --> WB
    MB --> EB
    MB --> ASM
    
    SO --> MD
    SO --> WD
    
    %% 战斗继承
    WB --> MW
    WB --> RW
    
    %% AI继承
    IAS -->|实现| ASB
    
    %% 数据继承
    WD --> CD
    WD --> ED
    WD --> PD
    
    %% 敌人继承
    EB --> MFA
    EB --> MCA
    EB --> MJA
    EB --> DO
    EB --> MS
    EB --> ICB
```

### 2.2 接口实现图

```mermaid
graph TB
    subgraph Interfaces["接口定义"]
        IAS[IAIState<br/>AI状态接口]
        INM[INoiseMaker<br/>噪音接口]
        IDamageable[IDamageable<br/>可伤害接口]
        ICollectible[ICollectible<br/>可采集接口]
    end

    subgraph AIImplementations["AI实现"]
        ASB[AIStateBase]
        Idle[IdleState]
        Patrol[PatrolState]
        Attack[AttackState]
    end

    subgraph PerceptionImpl["感知实现"]
        AP[AIPerception]
        PT[PerceptionTarget]
    end

    subgraph CombatImpl["战斗实现"]
        EB[EnemyBase]
        PC[PlayerController]
        CR[CollectibleResource]
    end

    %% AI接口实现
    IAS -->|实现| ASB
    ASB -->|派生| Idle
    ASB -->|派生| Patrol
    ASB -->|派生| Attack
    
    %% 噪音接口
    INM -->|实现| PC
    
    %% 可伤害接口
    IDamageable -->|实现| EB
    IDamageable -->|实现| PC
    
    %% 可采集接口
    ICollectible -->|实现| CR
```

### 2.3 核心类依赖关系

```mermaid
graph LR
    subgraph Dependencies["依赖关系"]
        direction TB
        
        GM[GameManager]
        SM[SaveManager]
        UM[UIManager]
        MM[MissionManager]
        RM[ResourceManager]
        DM[DiveManager]
        CM[CombatManager]
        EM[EventSystem]
    end

    %% 依赖关系
    GM -->|依赖| SM
    GM -->|依赖| UM
    GM -->|依赖| MM
    GM -->|依赖| RM
    GM -->|依赖| DM
    
    SM -->|依赖| EM
    MM -->|依赖| RM
    MM -->|依赖| EM
    CM -->|依赖| EM
    UM -->|依赖| EM
    
    DM -->|依赖| CM
    RM -->|依赖| DM
```

---

## 3. 数据流图

### 3.1 战斗数据流

```mermaid
sequenceDiagram
    participant Player as 玩家
    participant Weapon as 武器系统
    participant Damage as 伤害计算
    participant Target as 目标
    participant Feedback as 反馈系统
    participant UI as UI系统

    Player->>Weapon: 发起攻击
    Weapon->>Weapon: 命中检测
    Weapon->>Damage: 创建DamageInfo
    
    Note over Damage: 包含基础伤害<br/>伤害类型<br/>攻击者信息
    
    Damage->>Damage: 计算类型克制
    Damage->>Target: 查询防御属性
    Target-->>Damage: 返回护甲/护盾值
    
    Damage->>Damage: 应用防御减免
    Damage->>Damage: 计算暴击
    Damage->>Target: 应用最终伤害
    
    Target->>Target: 更新生命值
    Target->>Feedback: 触发受击反馈
    
    Feedback->>Feedback: 屏幕震动
    Feedback->>Feedback: 命中停顿
    Feedback->>Feedback: 播放特效
    Feedback->>UI: 显示伤害数字
    
    alt 目标死亡
        Target->>Feedback: 触发击杀反馈
        Feedback->>UI: 显示击杀确认
        Target->>Loot: 掉落物品
    end
```

### 3.2 委托数据流

```mermaid
sequenceDiagram
    participant Player as 玩家
    participant Board as 委托板
    participant MM as MissionManager
    participant Mission as 委托实例
    participant Tracker as MissionTracker
    participant RM as ResourceManager
    participant SM as SaveManager

    %% 接取委托
    Player->>Board: 查看可用委托
    Board->>MM: 获取AvailableMissions
    MM-->>Board: 返回委托列表
    Board-->>Player: 显示委托信息
    
    Player->>Board: 接取委托
    Board->>MM: AcceptMission(id)
    MM->>Mission: 创建运行时实例
    MM->>Tracker: 开始追踪
    MM->>SM: 触发存档
    MM-->>Player: 接取成功

    %% 执行委托
    loop 委托进行中
        Player->>RM: 采集资源/击败敌人
        RM->>Tracker: 更新进度
        Tracker->>MM: CheckCompletion
        
        alt 完成条件满足
            MM->>MM: CompleteMission
            MM->>RM: 发放奖励
            MM->>SM: 触发存档
            MM->>Player: 显示完成通知
        end
    end

    %% 委托失败
    alt 时间耗尽
        MM->>MM: FailMission
        MM->>Player: 显示失败通知
    end
```

### 3.3 存档数据流

```mermaid
sequenceDiagram
    participant Player as 玩家/系统
    participant GM as GameManager
    participant SM as SaveManager
    participant Systems as 各系统
    participant File as 文件系统

    %% 保存流程
    Player->>GM: 请求保存
    GM->>SM: SaveGame(slot)
    
    SM->>Systems: 收集数据
    Systems-->>SM: GameSaveData
    
    SM->>SM: 计算校验和
    SM->>SM: 包装SaveDataWrapper
    SM->>SM: 序列化为JSON
    
    SM->>File: 写入临时文件
    SM->>File: 创建备份
    SM->>File: 原子性重命名
    
    SM-->>GM: 保存成功
    GM-->>Player: 通知完成

    %% 加载流程
    Player->>GM: 请求加载
    GM->>SM: LoadGame(slot)
    
    SM->>File: 读取存档文件
    SM->>SM: 验证校验和
    
    alt 校验失败
        SM->>File: 尝试备份恢复
    end
    
    SM->>SM: 版本兼容性检查
    SM->>SM: 数据迁移(如需要)
    
    SM->>Systems: 应用数据
    Systems-->>SM: 加载完成
    
    SM-->>GM: 加载成功
    GM-->>Player: 进入游戏
```

### 3.4 AI决策数据流

```mermaid
sequenceDiagram
    participant Enemy as 敌人
    participant Perception as AIPerception
    participant StateMachine as AIStateMachine
    participant CurrentState as 当前状态
    participant Pathfinding as AStarPathfinding

    loop 每帧更新
        Enemy->>Perception: UpdatePerception
        Perception->>Perception: 视觉检测
        Perception->>Perception: 听觉检测
        Perception->>Perception: 更新记忆
        
        alt 发现新目标
            Perception->>StateMachine: OnTargetDetected
        else 丢失目标
            Perception->>StateMachine: OnTargetLost
        end
        
        StateMachine->>CurrentState: OnUpdate
        
        alt 需要状态切换
            CurrentState->>StateMachine: ChangeState
            StateMachine->>CurrentState: OnExit
            StateMachine->>StateMachine: 切换状态
            StateMachine->>CurrentState: OnEnter
        end
        
        alt 需要移动
            CurrentState->>Pathfinding: FindPath
            Pathfinding-->>CurrentState: 返回路径
            CurrentState->>Enemy: 执行移动
        end
    end
```

---

## 4. 模块依赖图

### 4.1 项目模块结构

```mermaid
graph TB
    subgraph CoreModule["Core模块"]
        GM[GameManager]
        SM[SaveManager]
        UM[UIManager]
        EM[EventSystem]
        CM[ConfigManager]
        SL[ServiceLocator]
    end

    subgraph SystemsModule["Systems模块"]
        MM[MissionManager]
        RM[ResourceManager]
        DM[DiveManager]
        MT[MissionTracker]
    end

    subgraph CombatModule["Combat模块"]
        COM[CombatManager]
        DC[DamageCalculator]
        WB[WeaponBase]
        SS[ShieldSystem]
        AS[ArmorSystem]
        CF[CombatFeedback]
    end

    subgraph AIModule["AI模块"]
        ASM[AIStateMachine]
        AP[AIPerception]
        ASP[AStarPathfinding]
        EB[EnemyBase]
    end

    subgraph PlayerModule["Player模块"]
        PMC[MechController]
        PMM[MechMovement]
        PC[MechCollector]
    end

    subgraph UIModule["UI模块"]
        UIA[UIAnimation]
        HUD[HUDController]
        Menu[MenuSystem]
    end

    subgraph DataModule["Data模块"]
        GD[GameData]
        MD[MissionData]
        WD[WeaponData]
    end

    subgraph UtilsModule["Utils模块"]
        AU[AudioManagerExtended]
        EU[EffectManager]
        GU[GameUtils]
        GE[GameEvents]
    end

    subgraph IntegrationModule["Integration模块"]
        CIS[CombatIntegrationSystem]
        CSM[CombatSceneManager]
        LDS[LootDropSystem]
    end

    %% 依赖关系
    CoreModule --> UtilsModule
    SystemsModule --> CoreModule
    SystemsModule --> DataModule
    CombatModule --> CoreModule
    CombatModule --> UtilsModule
    AIModule --> CoreModule
    AIModule --> CombatModule
    PlayerModule --> CoreModule
    PlayerModule --> CombatModule
    UIModule --> CoreModule
    UIModule --> SystemsModule
    IntegrationModule --> CombatModule
    IntegrationModule --> AIModule
    IntegrationModule --> PlayerModule
```

### 4.2 命名空间依赖

```mermaid
graph LR
    subgraph Namespaces["命名空间依赖"]
        SebeJJ[SebeJJ]
        Core[SebeJJ.Core]
        Systems[SebeJJ.Systems]
        Combat[SebeJJ.Combat]
        AI[SebeJJ.AI]
        Player[SebeJJ.Player]
        UI[SebeJJ.UI]
        Data[SebeJJ.Data]
        Utils[SebeJJ.Utils]
        Integration[SebeJJ.Integration]
        Enemy[SebeJJ.Enemy]
        Boss[SebeJJ.Boss]
        Weapons[SebeJJ.Weapons]
        Experience[SebeJJ.Experience]
    end

    SebeJJ --> Core
    Core --> Systems
    Core --> Utils
    Systems --> Data
    Combat --> Core
    Combat --> Utils
    AI --> Core
    AI --> Combat
    Player --> Core
    Player --> Combat
    UI --> Core
    UI --> Systems
    Integration --> Combat
    Integration --> AI
    Integration --> Player
    Enemy --> AI
    Enemy --> Combat
    Boss --> Enemy
    Boss --> Combat
    Weapons --> Combat
    Experience --> Core
    Experience --> Systems
```

---

## 附录: 图表使用说明

### Mermaid图表渲染

本文档中的所有图表使用Mermaid语法编写，可以通过以下方式查看：

1. **GitHub/GitLab**: 原生支持Mermaid渲染
2. **VS Code**: 安装Mermaid插件
3. **在线工具**: 
   - [Mermaid Live Editor](https://mermaid.live/)
   - [Mermaid Diagram Renderer](https://mermaid.ink/)

### 图表类型说明

| 图表类型 | 用途 | 文件位置 |
|---------|------|---------|
| 系统架构图 | 展示系统间关系 | 第1节 |
| 类关系图 | 展示继承和实现 | 第2节 |
| 数据流图 | 展示数据流转 | 第3节 |
| 模块依赖图 | 展示模块关系 | 第4节 |

---

*文档版本: 2.0*
*最后更新: 2026-02-27*
*作者: 架构可视化工程师*
