# SebeJJ 赛博机甲 - 项目架构文档

## 1. 项目概述

### 游戏定位
- **名称**: 赛博机甲 SebeJJ
- **类型**: 深海资源猎人（委托驱动型Roguelite）
- **平台**: PC / 移动端
- **引擎**: Unity 2D
- **美术风格**: 赛博朋克

### 核心循环
```
接受委托 → 配置机甲 → 下潜探索 → 采集资源 → 遭遇战斗 → 返回基地 → 升级机甲 → 接受新委托
```

---

## 2. 系统架构

### 2.1 核心系统

```
┌─────────────────────────────────────────────────────────────┐
│                      GameManager (单例)                       │
├─────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐          │
│  │ MissionSystem│  │  MechSystem │  │DivingSystem │          │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘          │
│         │                │                │                  │
│  ┌──────▼──────┐  ┌──────▼──────┐  ┌──────▼──────┐          │
│  │ResourceSystem│  │CombatSystem │  │  UISystem   │          │
│  └─────────────┘  └─────────────┘  └─────────────┘          │
└─────────────────────────────────────────────────────────────┘
```

### 2.2 系统职责

| 系统 | 职责 | 关键类 |
|------|------|--------|
| MissionSystem | 委托管理、任务生成、奖励结算 | MissionManager, MissionData, MissionGenerator |
| MechSystem | 机甲配置、装备管理、属性计算 | MechManager, MechConfig, Equipment, Module |
| DivingSystem | 下潜流程、深度管理、环境效果 | DiveManager, DepthController, EnvironmentManager |
| ResourceSystem | 资源采集、背包管理、物品系统 | ResourceManager, Inventory, ItemData |
| CombatSystem | 战斗逻辑、敌人AI、伤害计算 | CombatManager, EnemyController, DamageSystem |
| UISystem | 界面管理、HUD、菜单系统 | UIManager, HUDController, MenuBase |

---

## 3. 核心系统详细设计

### 3.1 委托系统 (MissionSystem)

```csharp
// 委托类型枚举
public enum MissionType {
    Collection,     // 采集类：收集指定资源
    Elimination,    // 歼灭类：击败特定敌人
    Exploration,    // 探索类：到达指定深度/区域
    Escort,         // 护送类：保护目标
    BossHunt        // 狩猎类：击败Boss
}

// 委托数据结构
[System.Serializable]
public class MissionData {
    public string missionId;
    public string title;
    public string description;
    public MissionType type;
    public int targetDepth;          // 目标深度（米）
    public int difficulty;           // 难度等级 1-5
    public RewardData reward;
    public List<ObjectiveData> objectives;
    public float timeLimit;          // 时间限制（秒），0为无限制
}
```

**初始15个委托设计：**
1. 新手试炼 - 采集10个基础矿石（深度20米）
2. 深海初探 - 到达50米深度
3. 能源危机 - 采集5个能源核心（深度30米）
4. 清除威胁 - 击败3只机械鱼（深度40米）
5. 稀有金属 - 采集3个钛合金（深度60米）
6. 深渊探索 - 到达100米深度
7. 护送科学家 - 护送NPC到达80米
8. 巨型机械蟹 - 击败Boss（深度90米）
9. 数据回收 - 收集3个数据核心（深度70米）
10. 能源补给 - 采集15个能源晶体（深度50米）
11. 深海遗迹 - 探索古代遗迹（深度85米）
12. 机械军团 - 击败10个机械敌人（深度75米）
13. 极限挑战 - 到达100米并生存5分钟
14. 稀有生物 - 捕获2个发光水母（深度95米）
15. 终极试炼 - 完成所有类型委托各1次

### 3.2 机甲系统 (MechSystem)

```csharp
// 机甲配置
[System.Serializable]
public class MechConfig {
    public string mechName;
    public MechFrame frame;          // 机甲框架
    public List<EquipmentSlot> equipmentSlots;
    public List<ModuleSlot> moduleSlots;
    
    // 基础属性
    public float maxHull;            // 船体耐久
    public float maxEnergy;          // 能量上限
    public float moveSpeed;          // 移动速度
    public float cargoCapacity;      // 货舱容量
    public float scanRange;          // 扫描范围
}

// 装备类型
public enum EquipmentType {
    Drill,           // 钻头 - 采集速度
    Claw,            // 机械爪 - 采集范围
    Weapon,          // 武器 - 攻击力
    Shield,          // 护盾 - 防御
    Scanner,         // 扫描器 - 探测范围
    Engine,          // 引擎 - 移动速度
    Battery,         // 电池 - 能量上限
    Armor            // 装甲 - 耐久上限
}

// 模块类型（被动加成）
public enum ModuleType {
    Efficiency,      // 效率模块 - 采集效率
    Reinforced,      // 强化模块 - 耐久加成
    Overclock,       // 超频模块 - 速度加成
    Stealth,         // 隐形模块 - 降低仇恨
    AutoRepair,      // 自修模块 - 自动修复
    CargoExpand      // 扩容模块 - 增加货舱
}
```

### 3.3 资源系统 (ResourceSystem)

```csharp
// 资源类型
public enum ResourceType {
    Common,          // 普通：铜矿、铁矿
    Uncommon,        // 罕见：银矿、钛矿
    Rare,            // 稀有：能源核心、数据芯片
    Epic,            // 史诗：古代遗物、稀有金属
    Legendary        // 传说：深渊水晶
}

// 资源数据
[System.Serializable]
public class ResourceData {
    public string resourceId;
    public string displayName;
    public ResourceType rarity;
    public float weight;             // 重量（影响货舱）
    public int baseValue;            // 基础价值
    public int minDepth;             // 最小出现深度
    public float spawnChance;        // 生成概率
    public Sprite icon;
}

// 背包系统
public class Inventory {
    public float maxCapacity;
    public float currentWeight;
    public List<InventoryItem> items;
    
    public bool AddItem(ResourceData resource, int amount);
    public bool RemoveItem(string resourceId, int amount);
    public float GetRemainingCapacity();
}
```

### 3.4 下潜系统 (DivingSystem)

```csharp
// 深度层级
public enum DepthLayer {
    Shallow,         // 0-30米：新手区，基础资源
    Mid,             // 30-60米：中级区，中等难度
    Deep,            // 60-90米：高级区，稀有资源
    Abyss            // 90-100米：深渊区，Boss区域
}

// 下潜状态
public enum DiveState {
    Surface,         // 基地准备
    Diving,          // 下潜中
    Exploring,       // 探索中
    Ascending,       // 上浮中
    Emergency        // 紧急上浮
}

// 环境效果
[System.Serializable]
public class EnvironmentEffect {
    public DepthLayer layer;
    public float pressureDamage;     // 压强伤害（每秒）
    public float visibility;         // 能见度 0-1
    public float enemySpawnRate;     // 敌人生成率
    public List<ResourceData> availableResources;
}
```

### 3.5 战斗系统 (CombatSystem)

```csharp
// 伤害类型
public enum DamageType {
    Kinetic,         // 动能 - 对装甲有效
    Energy,          // 能量 - 对护盾有效
    Explosive,       // 爆炸 - 范围伤害
    Corrosion        // 腐蚀 - 持续伤害
}

// 敌人基类
public abstract class EnemyBase : MonoBehaviour {
    public string enemyId;
    public float maxHealth;
    public float currentHealth;
    public float attackPower;
    public float moveSpeed;
    public int minDepth;
    
    public abstract void AIBehavior();
    public abstract void OnTakeDamage(float damage, DamageType type);
}

// 武器数据
[System.Serializable]
public class WeaponData {
    public string weaponId;
    public string displayName;
    public DamageType damageType;
    public float baseDamage;
    public float fireRate;
    public float energyCost;
    public float range;
    public GameObject projectilePrefab;
}
```

---

## 4. 数据流架构

```
┌─────────────────────────────────────────────────────────────┐
│                        数据持久化层                           │
│              (PlayerPrefs / JSON / SQLite)                   │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────┐
│                      SaveManager                            │
│         存档管理、数据序列化、跨会话状态保持                    │
└─────────────────────────────────────────────────────────────┘
                              │
        ┌─────────────────────┼─────────────────────┐
        ▼                     ▼                     ▼
┌───────────────┐    ┌───────────────┐    ┌───────────────┐
│  PlayerData   │    │  MechData     │    │  MissionData  │
│  - 玩家等级    │    │  - 机甲配置    │    │  - 委托进度    │
│  - 货币       │    │  - 装备列表    │    │  - 完成记录    │
│  - 解锁内容    │    │  - 模块列表    │    │  - 统计数据    │
└───────────────┘    └───────────────┘    └───────────────┘
```

---

## 5. 项目目录结构

```
SebeJJ/
├── docs/                          # 项目文档
│   ├── GDD.md                     # 游戏设计文档
│   ├── Architecture.md            # 架构文档（本文件）
│   ├── ArtBible.md                # 美术规范
│   └── API.md                     # 代码规范
│
├── src/                           # 源代码
│   ├── Scripts/
│   │   ├── Systems/               # 核心系统
│   │   │   ├── GameManager.cs
│   │   │   ├── MissionSystem/
│   │   │   ├── MechSystem/
│   │   │   ├── DivingSystem/
│   │   │   ├── ResourceSystem/
│   │   │   ├── CombatSystem/
│   │   │   └── UISystem/
│   │   ├── Entities/              # 游戏实体
│   │   │   ├── Player/
│   │   │   ├── Enemies/
│   │   │   ├── Resources/
│   │   │   └── Projectiles/
│   │   ├── UI/                    # UI脚本
│   │   │   ├── HUD/
│   │   │   ├── Menus/
│   │   │   └── Components/
│   │   └── Utils/                 # 工具类
│   │       ├── ObjectPool.cs
│   │       ├── EventBus.cs
│   │       └── Helpers.cs
│   │
│   ├── Art/
│   │   ├── Sprites/               # 精灵图
│   │   │   ├── Characters/
│   │   │   ├── Enemies/
│   │   │   ├── Environment/
│   │   │   ├── Resources/
│   │   │   └── UI/
│   │   ├── Animations/            # 动画
│   │   ├── Tilemaps/              # 地图资源
│   │   └── UI/                    # UI资源
│   │
│   ├── Audio/
│   │   ├── Music/                 # 背景音乐
│   │   └── SFX/                   # 音效
│   │
│   ├── Prefabs/                   # 预制体
│   │   ├── Player/
│   │   ├── Enemies/
│   │   ├── Resources/
│   │   ├── UI/
│   │   └── Effects/
│   │
│   ├── Scenes/                    # 场景
│   │   ├── Boot.unity             # 启动场景
│   │   ├── MainMenu.unity         # 主菜单
│   │   ├── Base.unity             # 基地场景
│   │   └── Dive.unity             # 下潜场景
│   │
│   └── Resources/                 # 动态加载资源
│       ├── Data/
│       │   ├── Missions/          # 委托数据
│       │   ├── Resources/         # 资源数据
│       │   ├── Equipment/         # 装备数据
│       │   └── Enemies/           # 敌人数据
│       └── Configs/               # 配置文件
│
├── tools/                         # 工具脚本
│   └── DataImporter/              # 数据导入工具
│
└── tests/                         # 测试
    ├── UnitTests/
    └── PlayModeTests/
```

---

## 6. 技术规范

### 6.1 命名规范
- **类名**: PascalCase (e.g., `MissionManager`)
- **方法名**: PascalCase (e.g., `StartMission()`)
- **变量名**: camelCase (e.g., `currentDepth`)
- **常量名**: UPPER_SNAKE_CASE (e.g., `MAX_DEPTH`)
- **私有字段**: _camelCase (e.g., `_instance`)

### 6.2 代码组织
- 每个类一个文件
- 使用命名空间 `SebeJJ.Systems`
- 遵循 SOLID 原则
- 优先使用事件驱动而非直接引用

### 6.3 性能考虑
- 对象池管理频繁创建/销毁的对象
- 使用 ScriptableObject 存储配置数据
- 避免在 Update 中进行重型计算
- 使用 Tilemap 优化地图渲染

---

## 7. 扩展性设计

### 7.1 模块化设计
- 所有系统通过接口交互
- 使用事件总线解耦系统间依赖
- 数据驱动配置，便于平衡调整

### 7.2 内容扩展
- 委托数据使用 JSON/ScriptableObject，便于添加新委托
- 装备和资源使用数据表配置
- 敌人行为使用状态机，易于添加新类型

### 7.3 未来扩展方向
- 多人合作模式
- 机甲自定义外观
- 基地建造系统
- 成就系统
- 排行榜

---

*文档版本: 1.0*
*最后更新: 2026-02-26*
*作者: 首席架构师*
