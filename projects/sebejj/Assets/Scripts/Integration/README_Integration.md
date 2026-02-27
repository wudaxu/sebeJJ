/**
 * @file README_Integration.md
 * @brief 系统集成说明文档
 * @description 说明所有集成组件的功能和使用方法
 * @author 系统集成工程师
 * @date 2026-02-27
 */

# SebeJJ 战斗系统集成文档

## 概述

本文档描述了赛博机甲SebeJJ战斗系统的集成方案，包括敌人-机甲交互、战斗场景管理、掉落系统等。

## 核心集成组件

### 1. CombatIntegrationSystem (战斗集成系统)

**文件**: `CombatIntegrationSystem.cs`

**功能**:
- 连接 MechController 与 CombatStats
- 连接 EnemyBase 与 DamageCalculator
- 连接 WeaponManager 与敌人受击
- 连接 CombatFeedback 与相机震动
- 管理伤害数字显示
- 处理玩家和敌人的伤害事件

**使用方式**:
```csharp
// 获取实例
var combatSystem = CombatIntegrationSystem.Instance;

// 注册敌人
combatSystem.RegisterEnemy(enemyBase);

// 对玩家造成伤害
combatSystem.DamagePlayer(damageInfo);

// 治疗玩家
combatSystem.HealPlayer(50);
```

### 2. LootDropSystem (掉落系统)

**文件**: `LootDropSystem.cs`

**功能**:
- 管理敌人死亡后的资源掉落
- 支持必掉和随机掉落
- 掉落物自动被玩家吸引
- 信用点掉落

**使用方式**:
```csharp
// 生成掉落
LootDropSystem.Instance.SpawnLoot(position, EnemyType.MechFish);

// 配置掉落表（在Inspector中配置）
// 或通过代码：
var table = new EnemyLootTable();
table.enemyType = EnemyType.MechFish;
table.creditsDrop = 15;
```

### 3. EnemyDamageBridge (敌人伤害桥接器)

**文件**: `EnemyDamageBridge.cs`

**功能**:
- 为 EnemyBase 添加 CombatStats 支持
- 桥接旧的伤害系统和新的 DamageInfo 系统
- 自动处理伤害事件转发

**使用方式**:
自动附加到带有 EnemyBase 的物体上，无需手动调用。

### 4. CombatSceneManager (战斗场景管理器)

**文件**: `CombatSceneManager.cs`

**功能**:
- 管理波次生成
- 追踪活跃敌人
- 处理场景完成事件
- 生成点管理

**使用方式**:
```csharp
// 开始下一波
CombatSceneManager.Instance.StartNextWave();

// 生成测试敌人
CombatSceneManager.Instance.SpawnEnemyForTest(EnemyType.MechFish);

// 清理所有敌人
CombatSceneManager.Instance.ClearAllEnemies();
```

### 5. MechCombatController (机甲战斗控制器)

**文件**: `MechCombatController.cs`

**功能**:
- 整合机甲移动和战斗
- 处理攻击输入
- 武器瞄准
- 自动瞄准支持

**使用方式**:
自动附加到玩家机甲上，通过 Inspector 配置参数。

### 6. TestSceneSpawner (测试场景生成器)

**文件**: `TestSceneSpawner.cs`

**功能**:
- 快速生成测试场景
- 创建边界
- 生成玩家和敌人
- 设置集成系统

**使用方式**:
```csharp
// 初始化测试场景
TestSceneSpawner.Instance.InitializeTestScene();

// 重新生成
TestSceneSpawner.Instance.RespawnScene();

// 生成特定敌人
TestSceneSpawner.Instance.SpawnEnemyOfType(EnemyType.MechFish);
```

### 7. CombatTestQuickStart (快速启动器)

**文件**: `CombatTestQuickStart.cs`

**功能**:
- 一键启动测试场景
- 自动创建所有必要系统
- 调试快捷键支持

**使用方式**:
将脚本附加到场景中的空物体，勾选 autoInitialize。

## 系统集成架构

```
┌─────────────────────────────────────────────────────────────┐
│                    CombatIntegrationSystem                   │
│                      (中央集成管理器)                         │
└────────────────────┬────────────────────────────────────────┘
                     │
        ┌────────────┼────────────┐
        │            │            │
        ▼            ▼            ▼
┌──────────────┐ ┌──────────┐ ┌──────────────┐
│ MechController│ │ EnemyBase │ │ WeaponManager│
│  + CombatStats│ │+DamageBridge│              │
└──────────────┘ └──────────┘ └──────────────┘
        │            │            │
        │            │            │
        ▼            ▼            ▼
┌──────────────────────────────────────────────┐
│              CombatFeedback                   │
│         (屏幕震动/命中停顿/特效)               │
└──────────────────────────────────────────────┘
                     │
                     ▼
┌──────────────────────────────────────────────┐
│               LootDropSystem                  │
│              (击杀掉落系统)                    │
└──────────────────────────────────────────────┘
```

## 事件流程

### 玩家攻击敌人流程

1. 玩家输入攻击 → MechCombatController
2. 调用 WeaponManager.TryAttack()
3. 武器发射弹丸 → Projectile
4. 弹丸命中敌人 → OnTriggerEnter2D
5. 调用 ownerWeapon.OnProjectileHit()
6. 创建 DamageInfo
7. 调用 damageable.TakeDamage(damageInfo)
8. EnemyDamageBridge 接收伤害
9. 转发到 CombatStats.TakeDamage()
10. 计算最终伤害 (DamageCalculator)
11. 应用伤害，触发事件
12. CombatIntegrationSystem 处理事件
13. 显示伤害数字
14. 触发屏幕震动
15. 检查死亡 → 触发掉落

### 敌人攻击玩家流程

1. AI 决定攻击 → EnemyBase.PerformAttack()
2. 检测范围内玩家
3. 调用 playerCombatStats.TakeDamage()
4. CombatIntegrationSystem 处理玩家受伤
5. 触发屏幕震动
6. 显示伤害数字
7. 检查护盾破碎
8. 检查死亡

## 配置说明

### 场景配置 (SceneConfig_50m.json)

包含以下配置项：
- 环境参数（深度、氧气消耗等）
- 玩家初始状态
- 敌人波次配置
- 掉落表配置
- 目标条件

### 掉落表配置

在 LootDropSystem 中配置：
- 敌人类型
- 必掉物品列表
- 随机掉落列表
- 信用点范围

## 调试功能

### 快捷键

| 按键 | 功能 |
|------|------|
| R | 重新生成敌人 |
| K | 杀死所有敌人 |
| H | 治疗玩家50点 |
| F1 | 显示帮助 |

### 控制台命令

```csharp
// 生成敌人
TestSceneSpawner.Instance.SpawnEnemyOfType(EnemyType.MechFish);

// 清除敌人
TestSceneSpawner.Instance.ClearEnemies();

// 治疗
CombatIntegrationSystem.Instance.HealPlayer(100);

// 完成场景
CombatSceneManager.Instance.ClearAllEnemies();
```

## 测试场景设置步骤

1. 创建新场景
2. 添加空物体，命名为 "CombatTestSetup"
3. 附加 CombatTestQuickStart 脚本
4. 配置测试参数（敌人数、边界等）
5. 运行场景

或者使用 TestSceneSpawner：

1. 创建新场景
2. 添加空物体，附加 TestSceneSpawner
3. 配置预制体引用
4. 勾选 spawnOnStart 或手动调用 InitializeTestScene()

## 注意事项

1. 确保所有敌人预制体有 EnemyBase 组件
2. 确保敌人有碰撞器（Trigger 或 Collision）
3. 确保玩家有 CombatStats 组件
4. 确保场景中有 CombatFeedback（用于屏幕震动）
5. 确保相机标签为 "MainCamera"

## 扩展指南

### 添加新敌人类型

1. 创建继承 EnemyBase 的新类
2. 实现抽象方法
3. 在 LootDropSystem 中添加掉落表
4. 在 CombatSceneManager 中添加预制体引用

### 添加新武器

1. 创建 WeaponData ScriptableObject
2. 在 MechCombatController 中装备武器
3. 配置武器参数

### 自定义掉落物

1. 创建掉落物预制体
2. 添加 LootPickup 组件
3. 在掉落表中引用预制体

## 文件清单

```
Assets/Scripts/Integration/
├── CombatIntegrationSystem.cs    # 核心集成系统
├── LootDropSystem.cs             # 掉落系统
├── EnemyDamageBridge.cs          # 敌人伤害桥接
├── CombatSceneManager.cs         # 场景管理器
├── MechCombatController.cs       # 机甲战斗控制器
├── TestSceneSpawner.cs           # 测试场景生成器
└── CombatTestQuickStart.cs       # 快速启动器

Assets/Scenes/CombatTest/
├── SceneConfig_50m.json          # 场景配置
└── TestDocumentation.md          # 测试文档
```
