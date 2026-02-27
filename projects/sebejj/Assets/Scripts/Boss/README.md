/**
 * @file README.md
 * @brief 铁钳巨兽Boss系统说明
 * @description Boss战系统的快速入门指南
 * @author Boss战设计师
 * @date 2026-02-27
 */

# 铁钳巨兽 Boss系统

## 系统概述

铁钳巨兽是赛博机甲SebeJJ的最终Boss，具有三阶段战斗系统、丰富的攻击方式和场景互动机制。

## 文件结构

```
Assets/Scripts/Boss/
├── IronClawBeastBoss.cs      # Boss主类 (1229行)
├── BossStates.cs              # AI状态类 (637行)
├── BossHealthBar.cs           # 血条UI (463行)
├── BossArena.cs               # 场景管理器 (732行)
├── CombatWarningSystem.cs     # 战斗预警系统 (303行)
├── BossTestScene.cs           # 测试场景 (161行)
├── BossDesignDocument.md      # 设计文档
├── BossArtRequirements.md     # 美术资源需求
└── README.md                  # 本文件
```

## 核心组件

### 1. IronClawBeastBoss (Boss主类)
- **功能**：Boss的核心逻辑，包含所有攻击技能和状态管理
- **关键特性**：
  - 三阶段血量阈值系统（100%/60%/30%）
  - 阶段转换动画和无敌时间
  - 6种攻击类型（钳击、冲撞、防御、激光、召唤、地震）
  - 弱点暴露机制
  - 狂暴模式

### 2. BossStates (AI状态)
- **功能**：Boss的AI状态机实现
- **状态列表**：
  - `BossIdleState` - 待机
  - `BossChaseState` - 追击
  - `BossAttackState` - 攻击
  - `BossDefendState` - 防御
  - `BossSpecialState` - 特殊技能
  - `BossDeadState` - 死亡

### 3. BossHealthBar (血条UI)
- **功能**：Boss血条和状态显示
- **特性**：
  - 延迟血条效果
  - 阶段颜色变化
  - 状态图标（防御、狂暴、弱点、无敌）
  - 阶段分隔标记

### 4. BossArena (场景管理)
- **功能**：Boss战场景管理
- **包含**：
  - 可破坏岩石系统
  - 即死深渊
  - 胜利传送门
  - 环境变化（光照随阶段改变）

### 5. CombatWarningSystem (预警系统)
- **功能**：为Boss攻击提供视觉预警
- **预警类型**：攻击、冲撞、激光、地震、危险区域

## 快速开始

### 1. 创建Boss战场景

```csharp
// 在场景中创建一个空物体，添加BossArena组件
BossArena arena = gameObject.AddComponent<BossArena>();

// 配置场景边界
arena.arenaWidth = 40f;
arena.arenaHeight = 25f;
```

### 2. 配置Boss

```csharp
// 创建Boss预制体，添加IronClawBeastBoss组件
IronClawBeastBoss boss = gameObject.AddComponent<IronClawBeastBoss>();

// 配置基础属性
boss.maxHealth = 5000f;
boss.moveSpeed = 3f;
boss.attackDamage = 50f;

// 配置阶段阈值
boss.phase2Threshold = 0.6f;  // 60%
boss.phase3Threshold = 0.3f;  // 30%
```

### 3. 启动Boss战

```csharp
// 自动开始
arena.StartBossFight();

// 或手动触发
boss.StartPhaseTransition(BossPhase.Phase1);
```

## 调试功能

在测试场景中，使用以下快捷键：

| 按键 | 功能 |
|------|------|
| F1 | 对Boss造成500点伤害 |
| F2 | 治疗Boss |
| F3 | 跳到第二阶段 |
| F4 | 跳到第三阶段 |
| F5 | 击杀Boss |
| F6 | 重置场景 |

## 扩展指南

### 添加新攻击类型

1. 在 `BossAttackType` 枚举中添加新类型
2. 在 `IronClawBeastBoss` 中实现攻击方法
3. 在 `BossSpecialState` 中添加选择逻辑

### 修改阶段属性

在 `ApplyPhaseAttributes` 方法中修改：

```csharp
private void ApplyPhaseAttributes(BossPhase phase)
{
    switch (phase)
    {
        case BossPhase.Phase2:
            moveSpeed = _baseMoveSpeed * 1.5f;  // 修改速度倍率
            break;
        case BossPhase.Phase3:
            attackDamage = _baseAttackDamage * 2f;  // 修改攻击倍率
            break;
    }
}
```

### 自定义场景物体

继承 `DestructibleRock` 类创建新的可破坏物体：

```csharp
public class CustomRock : DestructibleRock
{
    protected override void OnDestroyed()
    {
        // 自定义破坏效果
        base.OnDestroyed();
    }
}
```

## 事件订阅

Boss提供了多个事件供外部订阅：

```csharp
// 阶段变化
boss.OnPhaseChanged += (phase) => {
    Debug.Log($"进入阶段: {phase}");
};

// 受到伤害
boss.OnTakeDamage += (damage) => {
    Debug.Log($"Boss受到 {damage} 点伤害");
};

// 被击败
boss.OnDefeated += () => {
    Debug.Log("Boss被击败！");
    // 播放胜利音乐、显示奖励等
};

// 弱点暴露
boss.OnWeakPointExposed += () => {
    Debug.Log("弱点暴露，快攻击！");
};
```

## 性能优化建议

1. **激光检测**：使用射线检测而非碰撞体
2. **特效管理**：使用对象池管理特效实例
3. **AI更新**：减少不必要的路径计算
4. **血条更新**：只在血量变化时更新UI

## 已知限制

1. 需要配合玩家控制器才能完整测试地震躲避机制
2. 激光扫射的视觉表现需要额外的Shader支持
3. 阶段转换动画需要美术资源配合

## 后续开发计划

- [ ] 添加更多Boss攻击变体
- [ ] 实现玩家格挡反击机制
- [ ] 添加Boss战成就系统
- [ ] 优化网络同步支持
- [ ] 添加更多场景互动元素

## 参考文档

- `BossDesignDocument.md` - 完整设计文档
- `BossArtRequirements.md` - 美术资源需求

## 作者

Boss战设计师 - 2026-02-27
