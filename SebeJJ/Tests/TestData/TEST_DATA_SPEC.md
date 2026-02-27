# SebeJJ 测试数据规范

## 文档信息

| 项目 | 内容 |
|------|------|
| 用途 | Week 1 测试数据定义 |
| 适用范围 | 单元测试、集成测试、功能测试 |

---

## 1. 机甲属性测试数据

### 1.1 标准机甲配置 (StandardMech)
```json
{
  "configName": "StandardMech",
  "maxHealth": 100.0,
  "maxEnergy": 100.0,
  "maxOxygen": 100.0,
  "armor": 10.0,
  "pressureResistance": 100.0,
  "speed": 5.0,
  "turnRate": 180.0,
  "miningPower": 1.0,
  "cargoCapacity": 50.0
}
```

### 1.2 轻型机甲配置 (LightMech)
```json
{
  "configName": "LightMech",
  "maxHealth": 70.0,
  "maxEnergy": 120.0,
  "maxOxygen": 80.0,
  "armor": 5.0,
  "pressureResistance": 80.0,
  "speed": 8.0,
  "turnRate": 240.0,
  "miningPower": 0.8,
  "cargoCapacity": 30.0
}
```

### 1.3 重型机甲配置 (HeavyMech)
```json
{
  "configName": "HeavyMech",
  "maxHealth": 150.0,
  "maxEnergy": 80.0,
  "maxOxygen": 120.0,
  "armor": 20.0,
  "pressureResistance": 150.0,
  "speed": 3.0,
  "turnRate": 120.0,
  "miningPower": 1.5,
  "cargoCapacity": 80.0
}
```

### 1.4 边界值测试配置
```json
{
  "configName": "BoundaryTestMech",
  "maxHealth": 1.0,
  "maxEnergy": 0.0,
  "maxOxygen": 9999.0,
  "armor": 0.0,
  "pressureResistance": 0.0,
  "speed": 0.1,
  "turnRate": 1.0,
  "miningPower": 0.0,
  "cargoCapacity": 1.0
}
```

---

## 2. 玩家数据测试数据

### 2.1 新游戏初始数据
```json
{
  "playerName": "TestPlayer",
  "saveVersion": 1,
  "playTime": 0,
  "currentDepth": 0.0,
  "position": {"x": 0.0, "y": 0.0},
  "health": 100.0,
  "energy": 100.0,
  "oxygen": 100.0,
  "currency": 0,
  "inventory": [],
  "equippedUpgrades": [],
  "discoveredAreas": [],
  "completedTutorials": []
}
```

### 2.2 中期游戏数据
```json
{
  "playerName": "SebeHunter",
  "saveVersion": 1,
  "playTime": 3600,
  "currentDepth": 250.5,
  "position": {"x": 45.5, "y": -250.5},
  "health": 85.0,
  "energy": 60.0,
  "oxygen": 45.0,
  "currency": 1250,
  "inventory": [
    {"type": "CopperOre", "count": 15},
    {"type": "ScrapMetal", "count": 8},
    {"type": "CrystalShard", "count": 3},
    {"type": "BioSample", "count": 2}
  ],
  "equippedUpgrades": ["SpeedBoost", "OxygenTankV1"],
  "discoveredAreas": ["Shallows", "CoralReef", "DeepTrench"],
  "completedTutorials": ["Movement", "Mining", "Combat"]
}
```

### 2.3 满级/满载数据
```json
{
  "playerName": "MaxedPlayer",
  "saveVersion": 1,
  "playTime": 99999,
  "currentDepth": 999.9,
  "position": {"x": 999.9, "y": -999.9},
  "health": 999.0,
  "energy": 999.0,
  "oxygen": 999.0,
  "currency": 999999,
  "inventory": [
    {"type": "CopperOre", "count": 99},
    {"type": "ScrapMetal", "count": 99},
    {"type": "IronOre", "count": 99},
    {"type": "GoldOre", "count": 99},
    {"type": "CrystalShard", "count": 99},
    {"type": "Uranium", "count": 99},
    {"type": "BioSample", "count": 99},
    {"type": "DataFragment", "count": 99},
    {"type": "AncientTech", "count": 99}
  ],
  "equippedUpgrades": ["AllUpgrades"],
  "discoveredAreas": ["AllAreas"],
  "completedTutorials": ["AllTutorials"]
}
```

### 2.4 损坏/异常数据（用于容错测试）
```json
{
  "playerName": "",
  "saveVersion": 999,
  "playTime": -1,
  "currentDepth": -100.0,
  "position": {"x": null, "y": "invalid"},
  "health": -50.0,
  "energy": null,
  "oxygen": "NaN",
  "currency": -9999,
  "inventory": "corrupted",
  "equippedUpgrades": null,
  "discoveredAreas": "not_an_array",
  "completedTutorials": {}
}
```

---

## 3. 资源类型测试数据

### 3.1 基础矿物
| ID | 名称 | 基础价值 | 重量 | 最大堆叠 | 稀有度 |
|----|------|----------|------|----------|--------|
| ScrapMetal | 废金属 | 5 | 1.0 | 99 | Common |
| CopperOre | 铜矿 | 10 | 1.2 | 99 | Common |
| IronOre | 铁矿 | 15 | 1.5 | 99 | Common |

### 3.2 稀有矿物
| ID | 名称 | 基础价值 | 重量 | 最大堆叠 | 稀有度 |
|----|------|----------|------|----------|--------|
| GoldOre | 金矿 | 50 | 2.0 | 50 | Rare |
| CrystalShard | 水晶碎片 | 80 | 0.5 | 30 | Rare |
| Uranium | 铀 | 150 | 3.0 | 10 | Epic |

### 3.3 特殊资源
| ID | 名称 | 基础价值 | 重量 | 最大堆叠 | 稀有度 |
|----|------|----------|------|----------|--------|
| BioSample | 生物样本 | 100 | 0.8 | 20 | Rare |
| DataFragment | 数据碎片 | 200 | 0.1 | 50 | Epic |
| AncientTech | 古代科技 | 500 | 2.5 | 5 | Legendary |

---

## 4. 输入测试数据

### 4.1 键盘输入映射
| 按键 | 功能 | 测试场景 |
|------|------|----------|
| W / Up | 向上移动 | 持续按住、快速连按 |
| S / Down | 向下移动 | 持续按住、快速连按 |
| A / Left | 向左移动 | 持续按住、快速连按 |
| D / Right | 向右移动 | 持续按住、快速连按 |
| E | 交互/采集 | 单击、靠近时 |
| Tab | 打开背包 | 切换显示 |
| Esc | 暂停菜单 | 打开/关闭 |
| H | 切换HUD | 显示/隐藏 |

### 4.2 鼠标输入
| 操作 | 功能 | 测试场景 |
|------|------|----------|
| 移动 | 瞄准/旋转 | 快速移动、边界测试 |
| 左键 | 主武器 | 单击、连点、按住 |
| 右键 | 副武器 | 单击、连点、按住 |

---

## 5. 场景测试数据

### 5.1 测试场景配置
```json
{
  "sceneName": "TestScene",
  "mapWidth": 100,
  "mapHeight": 100,
  "roomCount": 5,
  "playerStartPos": {"x": 0, "y": 0},
  "cameraBounds": {
    "min": {"x": -50, "y": -50},
    "max": {"x": 50, "y": 50}
  },
  "resourceNodes": [
    {"type": "CopperOre", "pos": {"x": 10, "y": 10}, "amount": 5},
    {"type": "ScrapMetal", "pos": {"x": -10, "y": 20}, "amount": 3}
  ]
}
```

### 5.2 深度区域定义
| 区域 | 深度范围 | 氧气消耗倍率 | 压强伤害 |
|------|----------|--------------|----------|
| 浅水区 | 0-50m | 1.0x | 0 |
| 中等深度 | 50-200m | 1.5x | 0 |
| 深水区 | 200-500m | 2.0x | 1/s |
| 超深渊 | 500m+ | 3.0x | 5/s |

---

## 6. 性能测试基准

### 6.1 目标性能指标
| 指标 | 目标值 | 最低可接受 |
|------|--------|------------|
| 帧率 | 60 FPS | 30 FPS |
| 内存使用 | < 500MB | < 1GB |
| 场景加载时间 | < 3s | < 5s |
| 存档加载时间 | < 1s | < 2s |
| 对象池命中率 | > 90% | > 80% |

### 6.2 压力测试参数
```
对象池压力测试:
- 同时活跃对象: 1000
- 获取/回收频率: 60次/秒
- 持续时间: 60秒

事件系统压力测试:
- 订阅者数量: 100
- 事件触发频率: 100次/秒
- 持续时间: 60秒
```

---

**文档版本:** 1.0  
**创建日期:** 2026-02-26  
**作者:** 测试工程师 Agent
