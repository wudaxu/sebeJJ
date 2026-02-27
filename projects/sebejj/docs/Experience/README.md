# SebeJJ 体验优化系统

## 系统概述

本系统负责赛博机甲SebeJJ的整体游戏体验打磨，包括新手引导、难度曲线调优、游戏节奏优化等核心体验模块。

## 文档结构

- [Phase1: 新手引导系统](phase1_tutorial_system.md) - 首次进入游戏引导、操作教学、系统解锁提示
- [Phase2: 难度曲线调优](phase2_difficulty_curve.md) - 敌人强度递增、资源价值平衡、死亡惩罚机制
- [Phase3: 游戏节奏优化](phase3_pacing.md) - 战斗与探索节奏、委托缓冲、存档点分布
- [体验优化文档](experience_optimization_doc.md) - 玩家旅程地图、痛点分析、A/B测试建议

## 代码结构

```
Assets/Scripts/Experience/
├── Tutorial/
│   ├── TutorialManager.cs          # 新手引导主管理器
│   ├── TutorialStep.cs             # 引导步骤基类
│   ├── TutorialUI.cs               # 引导UI控制器
│   ├── TutorialTrigger.cs          # 引导触发器
│   └── TutorialSaveData.cs         # 引导存档数据
├── Difficulty/
│   ├── DifficultyManager.cs        # 难度管理器
│   ├── EnemyScalingSystem.cs       # 敌人强度缩放
│   ├── ResourceBalanceSystem.cs    # 资源价值平衡
│   └── DeathPenaltySystem.cs       # 死亡惩罚系统
├── Pacing/
│   ├── PacingManager.cs            # 节奏管理器
│   ├── CombatPacingController.cs   # 战斗节奏控制
│   ├── SavePointSystem.cs          # 存档点系统
│   └── RewardTimingSystem.cs       # 奖励发放时机
└── Analytics/
    ├── PlayerJourneyTracker.cs     # 玩家旅程追踪
    ├── PainPointDetector.cs        # 痛点检测器
    └── ABTestManager.cs            # A/B测试管理器
```

## 快速开始

1. 将 `TutorialManager` 添加到场景中的 GameManager 对象
2. 配置 `TutorialConfig` ScriptableObject
3. 在 UI 层级添加 `TutorialUI` 预制体
4. 运行游戏，引导系统将自动检测首次进入并启动引导流程

## 配置说明

### 新手引导配置

在 Unity Inspector 中配置 TutorialConfig：
- `EnableTutorial`: 是否启用新手引导
- `Steps`: 引导步骤列表
- `SkipOnSecondPlay`: 二周目是否跳过
- `HelpPanelPrefab`: 帮助面板预制体

### 难度曲线配置

通过 DifficultyCurveConfig 配置：
- `BaseDifficulty`: 基础难度系数
- `DepthMultiplier`: 深度难度倍率
- `EnemyHealthCurve`: 敌人血量曲线
- `EnemyDamageCurve`: 敌人伤害曲线

---

*文档版本: 1.0*
*最后更新: 2026-02-27*
