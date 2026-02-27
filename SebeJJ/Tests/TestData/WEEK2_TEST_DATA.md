# SebeJJ Week 2 测试数据规范

## 文档信息

| 项目 | 内容 |
|------|------|
| 用途 | Week 2 战斗系统、武器系统、敌人AI测试数据 |
| 适用范围 | 战斗测试、武器测试、AI行为测试 |
| 创建日期 | 2026-02-26 |

---

## 1. 武器测试数据

### 1.1 采矿激光 (MiningLaser)
```json
{
  "weaponId": "WPN_MINING_LASER_01",
  "weaponName": "采矿激光 MK-I",
  "type": "MiningLaser",
  "damage": 10,
  "damageType": "Energy",
  "fireRate": 10,
  "isContinuous": true,
  "range": 15,
  "energyCost": 5,
  "energyCostPerSecond": 5,
  "miningPower": 2.0,
  "overheatThreshold": 100,
  "cooldownRate": 25,
  "rarity": "Common",
  "description": "基础采矿工具，也可用于自卫"
}
```

### 1.2 鱼叉发射器 (Harpoon)
```json
{
  "weaponId": "WPN_HARPOON_01",
  "weaponName": "深海鱼叉",
  "type": "Harpoon",
  "damage": 50,
  "damageType": "Physical",
  "fireRate": 0.5,
  "isContinuous": false,
  "range": 20,
  "projectileSpeed": 12,
  "energyCost": 20,
  "hasPullEffect": true,
  "pullForce": 10,
  "maxPullMass": 50,
  "rarity": "Uncommon",
  "description": "高伤害单体武器，可拖拽小型敌人"
}
```

### 1.3 鱼雷发射器 (Torpedo)
```json
{
  "weaponId": "WPN_TORPEDO_01",
  "weaponName": "爆破鱼雷",
  "type": "Torpedo",
  "damage": 80,
  "damageType": "Explosive",
  "fireRate": 0.3,
  "isContinuous": false,
  "range": 30,
  "projectileSpeed": 3,
  "energyCost": 40,
  "explosionRadius": 5,
  "explosionDamageFalloff": true,
  "friendlyFire": false,
  "rarity": "Rare",
  "description": "范围伤害武器，适合对付集群敌人"
}
```

### 1.4 声波脉冲 (SonicPulse)
```json
{
  "weaponId": "WPN_SONIC_01",
  "weaponName": "声波震荡器",
  "type": "SonicPulse",
  "damage": 20,
  "damageType": "Physical",
  "fireRate": 1,
  "isContinuous": false,
  "range": 8,
  "coneAngle": 60,
  "energyCost": 15,
  "knockbackForce": 15,
  "stunDuration": 0.5,
  "rarity": "Uncommon",
  "description": "击退敌人，创造喘息空间"
}
```

### 1.5 等离子切割器 (PlasmaCutter)
```json
{
  "weaponId": "WPN_PLASMA_01",
  "weaponName": "等离子切割器",
  "type": "PlasmaCutter",
  "damage": 100,
  "damageType": "Energy",
  "fireRate": 0.2,
  "isContinuous": false,
  "range": 3,
  "beamWidth": 2,
  "energyCost": 50,
  "armorPenetration": 0.5,
  "rarity": "Epic",
  "description": "近战高伤害，可穿透护甲"
}
```

### 1.6 武器平衡测试配置
```json
{
  "weaponBalanceTests": [
    {
      "testName": "DPS平衡验证",
      "expectedDPSRange": [20, 100],
      "weapons": [
        {"id": "WPN_MINING_LASER_01", "expectedDPS": 100},
        {"id": "WPN_HARPOON_01", "expectedDPS": 25},
        {"id": "WPN_TORPEDO_01", "expectedDPS": 24},
        {"id": "WPN_SONIC_01", "expectedDPS": 20},
        {"id": "WPN_PLASMA_01", "expectedDPS": 20}
      ]
    },
    {
      "testName": "能量效率验证",
      "expectedEfficiencyRange": [1.0, 3.0],
      "weapons": [
        {"id": "WPN_MINING_LASER_01", "damagePerEnergy": 2.0},
        {"id": "WPN_HARPOON_01", "damagePerEnergy": 2.5},
        {"id": "WPN_TORPEDO_01", "damagePerEnergy": 2.0},
        {"id": "WPN_SONIC_01", "damagePerEnergy": 1.33},
        {"id": "WPN_PLASMA_01", "damagePerEnergy": 2.0}
      ]
    }
  ]
}
```

---

## 2. 敌人测试数据

### 2.1 深海水母 (DeepJellyfish)
```json
{
  "enemyId": "ENM_JELLYFISH_01",
  "enemyName": "深海水母",
  "type": "DeepJellyfish",
  "maxHealth": 50,
  "armor": 0,
  "moveSpeed": 2,
  "turnRate": 90,
  "detectionRange": 5,
  "attackRange": 3,
  "attackDamage": 10,
  "attackCooldown": 2,
  "attackType": "Tentacle",
  "resistances": {
    "Physical": 0,
    "Energy": 0.3,
    "Explosive": 0,
    "Pressure": 0,
    "Corrosive": 0.5
  },
  "weakPointMultiplier": 1.5,
  "weakPointLocation": "Core",
  "behaviors": ["Float", "Passive", "TentacleAttack"],
  "floatAmplitude": 0.5,
  "floatSpeed": 1,
  "glowIntensity": 0.8,
  "isAggressive": false,
  "xpReward": 15,
  "dropTable": [
    {"itemId": "BIO_SAMPLE", "chance": 0.3, "minCount": 1, "maxCount": 2},
    {"itemId": "GLOW_GEL", "chance": 0.5, "minCount": 1, "maxCount": 3}
  ]
}
```

### 2.2 安保无人机 (SecurityDrone)
```json
{
  "enemyId": "ENM_DRONE_01",
  "enemyName": "安保无人机",
  "type": "SecurityDrone",
  "maxHealth": 80,
  "armor": 10,
  "moveSpeed": 4,
  "turnRate": 180,
  "detectionRange": 12,
  "attackRange": 15,
  "attackDamage": 8,
  "attackCooldown": 0.5,
  "attackType": "Laser",
  "resistances": {
    "Physical": 0.2,
    "Energy": 0.1,
    "Explosive": 0.5,
    "Pressure": 0,
    "Corrosive": 0.2
  },
  "behaviors": ["Patrol", "Alert", "LaserAttack", "CallBackup"],
  "patrolWaypoints": 4,
  "patrolRadius": 10,
  "hoverHeight": 3,
  "hoverStability": 0.1,
  "alertRadius": 20,
  "isAggressive": true,
  "xpReward": 25,
  "dropTable": [
    {"itemId": "SCRAP_METAL", "chance": 0.6, "minCount": 2, "maxCount": 4},
    {"itemId": "CIRCUIT_BOARD", "chance": 0.2, "minCount": 1, "maxCount": 1}
  ]
}
```

### 2.3 鮟鱇鱼 (AnglerFish)
```json
{
  "enemyId": "ENM_ANGLER_01",
  "enemyName": "深渊鮟鱇",
  "type": "AnglerFish",
  "maxHealth": 120,
  "armor": 5,
  "moveSpeed": 3,
  "turnRate": 120,
  "detectionRange": 8,
  "attackRange": 10,
  "attackDamage": 40,
  "attackCooldown": 5,
  "attackType": "Dash",
  "resistances": {
    "Physical": 0.1,
    "Energy": 0.2,
    "Explosive": 0,
    "Pressure": 0.3,
    "Corrosive": 0
  },
  "behaviors": ["Camouflage", "Ambush", "DashAttack", "Lure"],
  "camouflageOpacity": 0.3,
  "lureRange": 12,
  "dashSpeed": 15,
  "dashDistance": 10,
  "recoveryTime": 2,
  "isAggressive": true,
  "xpReward": 40,
  "dropTable": [
    {"itemId": "BIO_SAMPLE", "chance": 0.5, "minCount": 2, "maxCount": 3},
    {"itemId": "ANGLER_LIGHT", "chance": 0.15, "minCount": 1, "maxCount": 1}
  ]
}
```

### 2.4 防御炮塔 (DefenseTurret)
```json
{
  "enemyId": "ENM_TURRET_01",
  "enemyName": "自动防御炮塔",
  "type": "DefenseTurret",
  "maxHealth": 100,
  "armor": 20,
  "moveSpeed": 0,
  "turnRate": 90,
  "detectionRange": 20,
  "attackRange": 18,
  "attackDamage": 15,
  "attackCooldown": 0.2,
  "attackType": "Burst",
  "burstCount": 3,
  "burstInterval": 0.1,
  "resistances": {
    "Physical": 0.3,
    "Energy": 0.2,
    "Explosive": 0.4,
    "Pressure": 0,
    "Corrosive": 0.3
  },
  "behaviors": ["Static", "RotateAim", "BurstFire"],
  "rotationLimit": 180,
  "fireAngle": 60,
  "isStatic": true,
  "isAggressive": true,
  "xpReward": 30,
  "dropTable": [
    {"itemId": "SCRAP_METAL", "chance": 0.7, "minCount": 3, "maxCount": 5},
    {"itemId": "WEAPON_PART", "chance": 0.25, "minCount": 1, "maxCount": 2}
  ]
}
```

### 2.5 敌人状态机测试配置
```json
{
  "stateMachineTests": {
    "states": ["Idle", "Patrol", "Chase", "Attack", "Return", "Stunned", "Death"],
    "transitions": [
      {"from": "Idle", "to": "Patrol", "condition": "timer > 3s", "priority": 1},
      {"from": "Patrol", "to": "Chase", "condition": "playerInDetectionRange", "priority": 5},
      {"from": "Chase", "to": "Attack", "condition": "playerInAttackRange", "priority": 10},
      {"from": "Chase", "to": "Patrol", "condition": "lostPlayer && timer > 3s", "priority": 5},
      {"from": "Attack", "to": "Chase", "condition": "playerOutOfAttackRange", "priority": 8},
      {"from": "Any", "to": "Stunned", "condition": "takenDamage", "priority": 20},
      {"from": "Stunned", "to": "Chase", "condition": "stunEnd", "priority": 15},
      {"from": "Any", "to": "Death", "condition": "health <= 0", "priority": 100}
    ]
  }
}
```

---

## 3. 伤害系统测试数据

### 3.1 伤害类型测试
```json
{
  "damageTypeTests": [
    {
      "damageType": "Physical",
      "testCases": [
        {"baseDamage": 100, "resistance": 0, "expected": 100},
        {"baseDamage": 100, "resistance": 0.3, "expected": 70},
        {"baseDamage": 100, "resistance": 0.5, "expected": 50},
        {"baseDamage": 100, "resistance": 1.0, "expected": 0}
      ]
    },
    {
      "damageType": "Energy",
      "testCases": [
        {"baseDamage": 100, "resistance": 0, "expected": 100},
        {"baseDamage": 100, "resistance": 0.3, "expected": 70},
        {"baseDamage": 100, "resistance": 0.5, "expected": 50}
      ]
    },
    {
      "damageType": "Explosive",
      "testCases": [
        {"baseDamage": 100, "resistance": 0, "expected": 100},
        {"baseDamage": 100, "resistance": 0.5, "expected": 50}
      ]
    }
  ]
}
```

### 3.2 护甲减伤测试
```json
{
  "armorTests": [
    {"baseDamage": 100, "armor": 0, "expected": 100},
    {"baseDamage": 100, "armor": 10, "expected": 90},
    {"baseDamage": 100, "armor": 50, "expected": 50},
    {"baseDamage": 100, "armor": 100, "expected": 1},
    {"baseDamage": 10, "armor": 20, "expected": 1},
    {"baseDamage": 5, "armor": 10, "expected": 1}
  ]
}
```

### 3.3 暴击和弱点测试
```json
{
  "criticalTests": [
    {"baseDamage": 50, "critMultiplier": 2.0, "expected": 100},
    {"baseDamage": 50, "critMultiplier": 1.5, "expected": 75},
    {"baseDamage": 100, "critMultiplier": 3.0, "expected": 300}
  ],
  "weakPointTests": [
    {"baseDamage": 50, "weakMultiplier": 1.5, "expected": 75},
    {"baseDamage": 50, "weakMultiplier": 2.0, "expected": 100}
  ]
}
```

---

## 4. 投射物测试数据

### 4.1 投射物配置
```json
{
  "projectileTypes": [
    {
      "type": "StandardBullet",
      "speed": 20,
      "lifetime": 3,
      "gravity": 0,
      "pierceCount": 0,
      "bounceCount": 0,
      "homing": false
    },
    {
      "type": "Harpoon",
      "speed": 12,
      "lifetime": 5,
      "gravity": 2,
      "pierceCount": 1,
      "bounceCount": 0,
      "homing": false,
      "hasRope": true
    },
    {
      "type": "Torpedo",
      "speed": 3,
      "lifetime": 10,
      "gravity": 0,
      "pierceCount": 0,
      "bounceCount": 0,
      "homing": true,
      "homingStrength": 3,
      "explosionRadius": 5
    },
    {
      "type": "SonicWave",
      "speed": 15,
      "lifetime": 0.5,
      "gravity": 0,
      "expanding": true,
      "coneAngle": 60
    }
  ]
}
```

### 4.2 范围伤害测试
```json
{
  "explosionTests": [
    {
      "explosionRadius": 5,
      "baseDamage": 80,
      "falloff": true,
      "testPositions": [
        {"distance": 0, "expectedDamage": 80},
        {"distance": 2.5, "expectedDamage": 60},
        {"distance": 5, "expectedDamage": 40},
        {"distance": 6, "expectedDamage": 0}
      ]
    }
  ]
}
```

---

## 5. 能量系统测试数据

### 5.1 能量配置
```json
{
  "energyConfig": {
    "maxEnergy": 100,
    "regenRate": 10,
    "regenDelay": 1,
    "lowEnergyThreshold": 20,
    "criticalEnergyThreshold": 10
  },
  "energyTests": [
    {"initial": 100, "cost": 20, "expected": 80},
    {"initial": 50, "cost": 60, "canFire": false},
    {"initial": 10, "cost": 10, "expected": 0},
    {"initial": 0, "cost": 10, "canFire": false}
  ]
}
```

### 5.2 能量恢复测试
```json
{
  "regenTests": [
    {"startEnergy": 50, "waitTime": 1, "regenRate": 10, "expected": 60},
    {"startEnergy": 95, "waitTime": 1, "regenRate": 10, "expected": 100},
    {"startEnergy": 0, "waitTime": 10, "regenRate": 10, "expected": 100}
  ]
}
```

---

## 6. 瞄准系统测试数据

### 6.1 瞄准配置
```json
{
  "aimConfig": {
    "aimSmoothness": 10,
    "aimAssistStrength": 0.3,
    "aimAssistRange": 5,
    "maxAimAngle": 360,
    "aimSway": 0.5,
    "controllerDeadzone": 0.1
  }
}
```

### 6.2 瞄准测试场景
```json
{
  "aimTests": [
    {
      "testName": "鼠标瞄准",
      "inputType": "Mouse",
      "mousePosition": [100, 50],
      "expectedAngle": 26.57
    },
    {
      "testName": "手柄瞄准",
      "inputType": "Gamepad",
      "stickInput": [0.5, 0.5],
      "expectedAngle": 45
    },
    {
      "testName": "瞄准平滑",
      "inputType": "Mouse",
      "startAngle": 0,
      "targetAngle": 90,
      "smoothness": 10,
      "deltaTime": 0.1,
      "expectedAngle": 41.1
    }
  ]
}
```

---

## 7. 战斗反馈测试数据

### 7.1 屏幕震动配置
```json
{
  "screenShakeConfig": {
    "light": {"intensity": 0.2, "duration": 0.1},
    "medium": {"intensity": 0.5, "duration": 0.2},
    "heavy": {"intensity": 1.0, "duration": 0.3}
  },
  "shakeTriggers": [
    {"event": "playerShoot", "shakeType": "light"},
    {"event": "playerHit", "shakeType": "medium"},
    {"event": "explosion", "shakeType": "heavy"}
  ]
}
```

### 7.2 命中停顿配置
```json
{
  "hitStopConfig": {
    "lightHit": 0.02,
    "mediumHit": 0.05,
    "heavyHit": 0.1
  }
}
```

### 7.3 伤害数字配置
```json
{
  "damageNumberConfig": {
    "normalColor": "#FFFFFF",
    "criticalColor": "#FFD700",
    "healColor": "#00FF00",
    "floatSpeed": 50,
    "fadeDuration": 1,
    "scale": 1,
    "criticalScale": 1.5
  }
}
```

---

## 8. 性能测试基准

### 8.1 战斗性能目标
```json
{
  "performanceTargets": {
    "maxProjectiles": 100,
    "maxEnemies": 50,
    "maxSimultaneousAttacks": 20,
    "targetFPS": 60,
    "minAcceptableFPS": 30
  }
}
```

### 8.2 性能测试场景
```json
{
  "performanceTests": [
    {
      "testName": "投射物压力测试",
      "projectileCount": 100,
      "duration": 60,
      "targetFrameTime": 16.67
    },
    {
      "testName": "敌人AI压力测试",
      "enemyCount": 50,
      "duration": 60,
      "targetFrameTime": 16.67
    },
    {
      "testName": "综合战斗测试",
      "enemyCount": 20,
      "projectileCount": 50,
      "duration": 60,
      "targetFrameTime": 33.33
    }
  ]
}
```

---

**文档版本:** 1.0  
**创建日期:** 2026-02-26  
**作者:** 测试工程师 Agent
