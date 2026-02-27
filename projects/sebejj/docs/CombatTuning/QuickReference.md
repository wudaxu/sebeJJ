# 战斗手感调优快速参考

## 新增组件列表

### 武器相关
1. **WeaponComboSystem** - 武器连招系统
   - 路径: `Assets/Scripts/Combat/WeaponComboSystem.cs`
   - 功能: 管理连击、连招窗口、连招奖励

2. **WeaponFeelController** - 武器手感控制器
   - 路径: `Assets/Scripts/Combat/WeaponFeelController.cs`
   - 功能: 管理攻击前摇/后摇、取消窗口、命中反馈

### 敌人相关
3. **EnemyHitReaction** - 敌人受击控制器
   - 路径: `Assets/Scripts/AI/EnemyHitReaction.cs`
   - 功能: 管理受击硬直、击退、无敌帧

### 战斗系统
4. **CombatWarningSystem** - 战斗预警系统
   - 路径: `Assets/Scripts/Combat/CombatWarningSystem.cs`
   - 功能: 敌人攻击预警、危险提示

5. **EnemySpawnController** - 敌人生成控制器
   - 路径: `Assets/Scripts/Combat/EnemySpawnController.cs`
   - 功能: 控制敌人生成节奏和波次

6. **CombatMusicController** - 战斗音乐控制器
   - 路径: `Assets/Scripts/Combat/CombatMusicController.cs`
   - 功能: 根据战斗强度动态切换音乐

7. **CombatManager** - 战斗管理器
   - 路径: `Assets/Scripts/Combat/CombatManager.cs`
   - 功能: 整合所有战斗系统

## 修改的现有文件

1. **WeaponData.cs** - 添加手感调优参数
2. **MeleeWeapon.cs** - 集成连招和手感系统
3. **RangedWeapon.cs** - 集成连招和手感系统
4. **EnemyBase.cs** - 添加受击反应支持
5. **MechFishAI.cs** - 添加预警和受击反馈
6. **MechCrabAI.cs** - 添加预警和受击反馈

## 关键参数速查

### 武器参数
```csharp
// WeaponData 新增参数
attackWindup = 0.15f;           // 攻击前摇
attackRecovery = 0.25f;         // 攻击后摇
hitStopDuration = 0.05f;        // 命中停顿
windupCancelWindow = 0.3f;      // 前摇取消窗口
recoveryCancelWindow = 0.5f;    // 后摇取消窗口
```

### 连招参数
```csharp
// WeaponComboSystem 默认参数
comboWindow = 0.8f;             // 连招窗口时间
comboResetTime = 2f;            // 连招重置时间
maxComboCount = 3;              // 最大连击数
damageBonusPerCombo = 0.1f;     // 每层伤害加成
speedBonusPerCombo = 0.1f;      // 每层速度加成
```

### 敌人参数
```csharp
// 机械鱼
hitStunDuration = 0.15f;        // 受击硬直
hitKnockbackForce = 6f;         // 受击击退
chargeWarningTime = 0.3f;       // 冲撞预警

// 机械蟹
hitStunDuration = 0.2f;         // 受击硬直
hitKnockbackForce = 4f;         // 受击击退
defendBreakStun = 0.8f;         // 破防眩晕
attackWarningTime = 0.25f;      // 攻击预警
```

## 使用说明

### 1. 为武器添加连招系统
```csharp
// 在武器预制体上添加 WeaponComboSystem 组件
WeaponComboSystem combo = gameObject.AddComponent<WeaponComboSystem>();
```

### 2. 为武器添加手感控制
```csharp
// 在武器预制体上添加 WeaponFeelController 组件
WeaponFeelController feel = gameObject.AddComponent<WeaponFeelController>();
```

### 3. 配置武器数据
在 WeaponData ScriptableObject 中设置：
- 攻击前摇 (attackWindup)
- 攻击后摇 (attackRecovery)
- 命中停顿 (hitStopDuration)
- 取消窗口 (windupCancelWindow, recoveryCancelWindow)

### 4. 敌人自动集成
EnemyBase 会自动添加 EnemyHitReaction 组件

### 5. 战斗管理器设置
创建一个空物体并添加 CombatManager 组件，配置：
- CombatFeedback 引用
- CombatWarningSystem 引用
- CombatMusicController 引用
- EnemySpawnController 引用
- 玩家 Transform 和 CombatStats

## 调优建议

### 近战武器
- 快速武器: 前摇 0.08-0.12s，后摇 0.15-0.2s
- 标准武器: 前摇 0.15-0.2s，后摇 0.25-0.35s
- 重型武器: 前摇 0.25-0.35s，后摇 0.4-0.6s

### 远程武器
- 弹道速度: 15-40 m/s
- 命中判定: 0.1-0.2m 半径
- 连射扩散: 每发增加 10-25%

### 敌人
- 脆皮敌人: 硬直 0.15s，击退 6-8
- 坦克敌人: 硬直 0.2s，击退 3-5
- 快速敌人: 硬直 0.1s，击退 8-10

## 文档位置

所有调优文档位于:
```
/root/.openclaw/workspace/projects/sebejj/docs/CombatTuning/
├── README.md        # 概述
├── WeaponTuning.md  # 武器参数表
├── EnemyTuning.md   # 敌人参数表
└── TuningLog.md     # 调优记录
```
