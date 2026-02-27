# 测试数据说明

**版本**: v1.0  
**日期**: 2026-02-26  

---

## 测试存档配置

### 存档1 - 新手起点 (save_slot_1)
```json
{
  "playerName": "TestPlayer_Newbie",
  "playTime": 0,
  "credits": 100,
  "mechLevel": 1,
  "maxDepth": 100,
  "inventory": {
    "capacity": 20,
    "items": []
  },
  "oxygen": {
    "current": 100,
    "max": 100
  },
  "energy": {
    "current": 100,
    "max": 100
  },
  "quests": {
    "active": [],
    "completed": [],
    "available": ["Q001"]
  },
  "unlockedAreas": ["shallow_sea"],
  "position": {
    "x": 0,
    "y": 0,
    "depth": 0
  }
}
```

### 存档2 - 中级玩家 (save_slot_2)
```json
{
  "playerName": "TestPlayer_Intermediate",
  "playTime": 3600,
  "credits": 2500,
  "mechLevel": 3,
  "maxDepth": 500,
  "inventory": {
    "capacity": 30,
    "items": [
      {"id": "oxygen_tank", "count": 3},
      {"id": "energy_cell", "count": 2},
      {"id": "glow_algae", "count": 10},
      {"id": "scrap_metal", "count": 15}
    ]
  },
  "oxygen": {
    "current": 100,
    "max": 150
  },
  "energy": {
    "current": 100,
    "max": 120
  },
  "quests": {
    "active": ["Q004", "Q005"],
    "completed": ["Q001", "Q002", "Q003"],
    "available": ["Q006", "Q007", "Q008"]
  },
  "unlockedAreas": ["shallow_sea", "mid_sea", "deep_sea_entrance"],
  "position": {
    "x": 150,
    "y": -200,
    "depth": 150
  }
}
```

### 存档3 - 高级玩家 (save_slot_3)
```json
{
  "playerName": "TestPlayer_Advanced",
  "playTime": 10800,
  "credits": 8000,
  "mechLevel": 5,
  "maxDepth": 1000,
  "inventory": {
    "capacity": 40,
    "items": [
      {"id": "oxygen_tank_large", "count": 5},
      {"id": "energy_cell_large", "count": 5},
      {"id": "abyss_crystal", "count": 3},
      {"id": "ancient_relic", "count": 1},
      {"id": "rare_mineral", "count": 8}
    ]
  },
  "oxygen": {
    "current": 200,
    "max": 200
  },
  "energy": {
    "current": 180,
    "max": 180
  },
  "quests": {
    "active": ["Q011", "Q012"],
    "completed": ["Q001", "Q002", "Q003", "Q004", "Q005", "Q006", "Q007", "Q008", "Q009", "Q010"],
    "available": ["Q013", "Q014", "Q015"]
  },
  "unlockedAreas": ["shallow_sea", "mid_sea", "deep_sea", "danger_zone", "ancient_ruins"],
  "position": {
    "x": 500,
    "y": -800,
    "depth": 600
  }
}
```

### 存档4 - 极限挑战 (save_slot_4)
```json
{
  "playerName": "TestPlayer_Extreme",
  "playTime": 18000,
  "credits": 15000,
  "mechLevel": 10,
  "maxDepth": 1500,
  "inventory": {
    "capacity": 50,
    "items": [
      {"id": "oxygen_tank_large", "count": 10},
      {"id": "energy_cell_large", "count": 10},
      {"id": "legendary_relic", "count": 1},
      {"id": "abyss_crystal", "count": 10}
    ]
  },
  "oxygen": {
    "current": 300,
    "max": 300
  },
  "energy": {
    "current": 250,
    "max": 250
  },
  "quests": {
    "active": ["Q015"],
    "completed": ["Q001", "Q002", "Q003", "Q004", "Q005", "Q006", "Q007", "Q008", "Q009", "Q010", "Q011", "Q012", "Q013", "Q014"],
    "available": []
  },
  "unlockedAreas": ["all"],
  "position": {
    "x": 800,
    "y": -1400,
    "depth": 1200
  }
}
```

---

## 委托测试配置

### 委托数据表
```json
{
  "quests": [
    {
      "id": "Q001",
      "name": "新手试炼：初次下潜",
      "type": "tutorial",
      "difficulty": 1,
      "requirements": {
        "minLevel": 1,
        "maxDepth": 100,
        "prerequisites": []
      },
      "objectives": [
        {"type": "reach_depth", "target": 50, "description": "下潜到50米深度"},
        {"type": "wait", "duration": 10, "description": "停留10秒"},
        {"type": "return", "description": "返回基地"}
      ],
      "rewards": {
        "credits": 100,
        "items": [],
        "unlockQuests": ["Q002", "Q003"]
      },
      "testData": {
        "testPosition": {"x": 0, "y": 0, "depth": 0},
        "expectedDuration": 60
      }
    },
    {
      "id": "Q002",
      "name": "收集荧光藻",
      "type": "collection",
      "difficulty": 1,
      "requirements": {
        "minLevel": 1,
        "maxDepth": 100,
        "prerequisites": ["Q001"]
      },
      "objectives": [
        {"type": "collect", "itemId": "glow_algae", "count": 5}
      ],
      "rewards": {
        "credits": 150,
        "items": [{"id": "oxygen_tank", "count": 1}]
      },
      "testData": {
        "spawnPoints": [
          {"x": 50, "y": -30, "depth": 30},
          {"x": 80, "y": -50, "depth": 50},
          {"x": 120, "y": -40, "depth": 40},
          {"x": 90, "y": -70, "depth": 70},
          {"x": 60, "y": -60, "depth": 60}
        ],
        "expectedDuration": 180
      }
    },
    {
      "id": "Q003",
      "name": "氧气补给任务",
      "type": "collection",
      "difficulty": 1,
      "requirements": {
        "minLevel": 1,
        "maxDepth": 150,
        "prerequisites": ["Q001"]
      },
      "objectives": [
        {"type": "collect", "itemId": "oxygen_tank", "count": 3}
      ],
      "rewards": {
        "credits": 200,
        "items": []
      },
      "testData": {
        "oxygenConsumptionRate": 1.0,
        "expectedDuration": 300
      }
    },
    {
      "id": "Q004",
      "name": "深海扫描训练",
      "type": "exploration",
      "difficulty": 2,
      "requirements": {
        "minLevel": 2,
        "maxDepth": 200,
        "prerequisites": ["Q001"]
      },
      "objectives": [
        {"type": "scan", "targetType": "hidden_item", "count": 3}
      ],
      "rewards": {
        "credits": 250,
        "items": [{"id": "scanner_upgrade", "count": 1}]
      },
      "testData": {
        "hiddenItems": [
          {"x": 100, "y": -100, "depth": 100, "rarity": "common"},
          {"x": 150, "y": -120, "depth": 120, "rarity": "uncommon"},
          {"x": 200, "y": -150, "depth": 150, "rarity": "rare"}
        ]
      }
    },
    {
      "id": "Q006",
      "name": "能源危机",
      "type": "timed",
      "difficulty": 2,
      "requirements": {
        "minLevel": 2,
        "maxDepth": 300,
        "prerequisites": ["Q004"]
      },
      "objectives": [
        {"type": "collect", "itemId": "energy_core", "count": 5}
      ],
      "timeLimit": 300,
      "rewards": {
        "credits": 350,
        "items": []
      },
      "testData": {
        "timeLimit": 300,
        "spawnRate": "high"
      }
    },
    {
      "id": "Q007",
      "name": "寻找古代遗迹",
      "type": "exploration",
      "difficulty": 3,
      "requirements": {
        "minLevel": 3,
        "maxDepth": 800,
        "prerequisites": ["Q005"]
      },
      "objectives": [
        {"type": "reach_depth", "target": 800},
        {"type": "interact", "targetId": "ancient_ruin", "count": 1}
      ],
      "rewards": {
        "credits": 500,
        "items": [{"id": "relic_codex", "count": 1}]
      },
      "testData": {
        "pressureDamage": 5,
        "ruinPosition": {"x": 400, "y": -750, "depth": 800}
      }
    },
    {
      "id": "Q009",
      "name": "危险区救援",
      "type": "escort",
      "difficulty": 3,
      "requirements": {
        "minLevel": 3,
        "maxDepth": 600,
        "prerequisites": ["Q007"]
      },
      "objectives": [
        {"type": "find", "targetId": "stranded_probe"},
        {"type": "escort", "targetId": "stranded_probe", "destination": "safe_zone"}
      ],
      "rewards": {
        "credits": 550,
        "items": []
      },
      "testData": {
        "dangerZoneDamage": 10,
        "probeHealth": 100,
        "probePosition": {"x": 300, "y": -550, "depth": 600}
      }
    },
    {
      "id": "Q012",
      "name": "压力测试",
      "type": "survival",
      "difficulty": 4,
      "requirements": {
        "minLevel": 5,
        "maxDepth": 1000,
        "prerequisites": ["Q009"]
      },
      "objectives": [
        {"type": "survive_at_depth", "depth": 1000, "duration": 180}
      ],
      "rewards": {
        "credits": 800,
        "items": []
      },
      "testData": {
        "extremePressureDamage": 15,
        "survivalTime": 180
      }
    },
    {
      "id": "Q015",
      "name": "深渊探险",
      "type": "exploration",
      "difficulty": 5,
      "requirements": {
        "minLevel": 10,
        "maxDepth": 1500,
        "prerequisites": ["Q012", "Q014"]
      },
      "objectives": [
        {"type": "reach_depth", "target": 1500},
        {"type": "return", "description": "安全返回基地"}
      ],
      "rewards": {
        "credits": 2000,
        "items": [],
        "title": "深渊探索者"
      },
      "testData": {
        "abyssalPressureDamage": 25,
        "visibility": 0.2,
        "expectedDuration": 600
      }
    }
  ]
}
```

---

## 自动化测试数据

### 性能测试场景配置
```json
{
  "performanceTestScenes": [
    {
      "name": "ShallowSea_Benchmark",
      "depth": 50,
      "objectCount": 30,
      "particleIntensity": "low",
      "duration": 60,
      "targetFPS": 60
    },
    {
      "name": "MidSea_Benchmark",
      "depth": 300,
      "objectCount": 80,
      "particleIntensity": "medium",
      "duration": 60,
      "targetFPS": 60
    },
    {
      "name": "DeepSea_Benchmark",
      "depth": 800,
      "objectCount": 150,
      "particleIntensity": "high",
      "duration": 60,
      "targetFPS": 45
    },
    {
      "name": "Abyss_StressTest",
      "depth": 1200,
      "objectCount": 200,
      "particleIntensity": "extreme",
      "duration": 300,
      "targetFPS": 30
    }
  ]
}
```

### 边界测试数据
```json
{
  "boundaryTests": [
    {"test": "max_inventory", "value": 50, "expected": "success"},
    {"test": "zero_oxygen", "value": 0, "expected": "damage"},
    {"test": "zero_energy", "value": 0, "expected": "functions_disabled"},
    {"test": "max_depth_exceeded", "value": 1600, "expected": "blocked"},
    {"test": "max_quests_active", "value": 10, "expected": "limit_reached"},
    {"test": "extreme_coordinates", "x": 999999, "y": 999999, "expected": "clamp_or_error"}
  ]
}
```

---

## 测试数据使用说明

### 存档数据位置
```
TestData/
├── SaveGames/
│   ├── save_slot_1.json    # 新手起点
│   ├── save_slot_2.json    # 中级玩家
│   ├── save_slot_3.json    # 高级玩家
│   └── save_slot_4.json    # 极限挑战
├── Quests/
│   └── quest_definitions.json
└── Performance/
    ├── benchmark_scenes.json
    └── boundary_tests.json
```

### 导入方法
1. 将JSON文件复制到游戏存档目录
2. 启动游戏选择对应存档槽位
3. 验证数据加载正确

### 自动化测试使用
```bash
# 运行特定存档的测试
./run_tests.sh --save=save_slot_2 --tests=quest_system

# 运行性能测试
./run_tests.sh --scene=DeepSea_Benchmark --duration=60

# 运行边界测试
./run_tests.sh --boundary-tests=all
```
