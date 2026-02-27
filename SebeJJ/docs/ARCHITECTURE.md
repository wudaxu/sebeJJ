# SebeJJ - 系统架构文档

## 项目概述

**项目名称:** SebeJJ (赛博机甲深海资源猎人)  
**类型:** Unity 2D 游戏  
**风格:** 赛博朋克  
**引擎版本:** Unity 2022.3 LTS  

---

## 核心设计理念

### 游戏核心循环
```
下潜探索 → 采集资源 → 战斗/躲避 → 返回基地 → 升级机甲 → 更深探索
```

### 赛博朋克元素
- **视觉:** 霓虹光效、全息UI、故障艺术(Glitch Art)
- **音效:** 合成器波、电子音效、深海环境音
- **叙事:** 企业阴谋、AI觉醒、资源争夺

---

## 技术架构

### 1. 项目结构

```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Core/              # 核心系统
│   │   │   ├── GameManager.cs
│   │   │   ├── EventSystem.cs
│   │   │   ├── SaveSystem.cs
│   │   │   └── ObjectPool.cs
│   │   ├── Player/            # 玩家系统
│   │   │   ├── MechController.cs
│   │   │   ├── MechStats.cs
│   │   │   ├── Inventory.cs
│   │   │   └── Equipment.cs
│   │   ├── Combat/            # 战斗系统
│   │   │   ├── WeaponSystem.cs
│   │   │   ├── Projectile.cs
│   │   │   ├── DamageSystem.cs
│   │   │   └── EnemyAI.cs
│   │   ├── Exploration/       # 探索系统
│   │   │   ├── MapGenerator.cs
│   │   │   ├── RoomSystem.cs
│   │   │   ├── ResourceNode.cs
│   │   │   └── FogOfWar.cs
│   │   ├── Economy/           # 经济系统
│   │   │   ├── Currency.cs
│   │   │   ├── ShopSystem.cs
│   │   │   └── Crafting.cs
│   │   ├── UI/                # UI系统
│   │   │   ├── UIManager.cs
│   │   │   ├── HUD.cs
│   │   │   ├── InventoryUI.cs
│   │   │   └── Dialogue.cs
│   │   └── Utils/             # 工具类
│   │       ├── Singleton.cs
│   │       ├── Extensions.cs
│   │       └── Constants.cs
│   ├── Prefabs/
│   ├── Scenes/
│   ├── ScriptableObjects/
│   ├── Audio/
│   ├── Sprites/
│   ├── Animations/
│   └── Resources/
├── Plugins/
└── ThirdParty/
```

### 2. 核心系统架构

#### 2.1 游戏管理器 (GameManager)
```csharp
// 单例模式，管理游戏全局状态
public class GameManager : Singleton<GameManager>
{
    public GameState CurrentState { get; private set; }
    public PlayerData PlayerData { get; private set; }
    
    // 状态管理
    public void ChangeState(GameState newState);
    public void PauseGame();
    public void ResumeGame();
    
    // 场景管理
    public void LoadScene(string sceneName);
    public void LoadLevel(int depth);
}
```

#### 2.2 事件系统 (EventSystem)
```csharp
// 解耦的发布-订阅模式
public static class GameEvents
{
    // 玩家事件
    public static Action<float> OnHealthChanged;
    public static Action<ResourceType, int> OnResourceCollected;
    public static Action OnPlayerDeath;
    
    // 游戏事件
    public static Action OnLevelCompleted;
    public static Action OnGameOver;
    
    // UI事件
    public static Action<string> OnShowDialogue;
}
```

#### 2.3 存档系统 (SaveSystem)
```csharp
public class SaveSystem : Singleton<SaveSystem>
{
    // JSON序列化存档
    public void SaveGame(PlayerData data);
    public PlayerData LoadGame();
    public void DeleteSave();
    
    // 自动存档
    public void AutoSave();
}
```

### 3. 玩家系统

#### 3.1 机甲控制器 (MechController)
```csharp
[RequireComponent(typeof(Rigidbody2D))]
public class MechController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 180f;
    public float buoyancy = 2f;  // 浮力模拟
    
    [Header("Depth")]
    public float maxDepth = 1000f;
    public float currentDepth;
    public float pressureDamage;  // 压强伤害
    
    // 移动输入处理
    private void HandleMovement();
    private void HandleDepth();
    private void ApplyBuoyancy();
}
```

#### 3.2 机甲属性 (MechStats)
```csharp
[CreateAssetMenu(fileName = "MechStats", menuName = "SebeJJ/MechStats")]
public class MechStats : ScriptableObject
{
    // 基础属性
    public float maxHealth = 100f;
    public float maxEnergy = 100f;
    public float maxOxygen = 100f;
    
    // 防御属性
    public float armor = 10f;
    public float pressureResistance = 100f;
    
    // 机动属性
    public float speed = 5f;
    public float turnRate = 180f;
    
    // 采集属性
    public float miningPower = 1f;
    public float cargoCapacity = 50f;
}
```

### 4. 战斗系统

#### 4.1 武器系统 (WeaponSystem)
```csharp
public class WeaponSystem : MonoBehaviour
{
    [Header("Weapons")]
    public List<Weapon> equippedWeapons;
    public Transform weaponMount;
    
    // 武器类型枚举
    public enum WeaponType
    {
        MiningLaser,    // 采矿激光
        Harpoon,        // 鱼叉
        Torpedo,        // 鱼雷
        SonicPulse,     // 声波脉冲
        PlasmaCutter    // 等离子切割器
    }
    
    public void FirePrimary();
    public void FireSecondary();
    public void SwitchWeapon(int index);
}
```

#### 4.2 伤害系统 (DamageSystem)
```csharp
public interface IDamageable
{
    void TakeDamage(float damage, DamageType type);
    void Heal(float amount);
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

### 5. 探索系统

#### 5.1 地图生成器 (MapGenerator)
```csharp
public class MapGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    public int mapWidth = 100;
    public int mapHeight = 100;
    public int roomCount = 15;
    
    // 房间类型权重
    public AnimationCurve roomTypeDistribution;
    
    // 生成流程
    public void GenerateLevel(int depth);
    private void GenerateRooms();
    private void ConnectRooms();
    private void PlaceResources();
    private void SpawnEnemies();
}
```

#### 5.2 房间系统 (RoomSystem)
```csharp
public enum RoomType
{
    Empty,          // 空房间
    ResourceRich,   // 资源丰富
    EnemyNest,      // 敌巢
    HazardZone,     // 危险区域
    Secret,         // 隐藏房间
    Boss,           // Boss房间
    SafeZone        // 安全区
}

public class Room : MonoBehaviour
{
    public RoomType type;
    public Bounds bounds;
    public List<Room> connectedRooms;
    public List<GameObject> contents;
    
    public void OnPlayerEnter();
    public void OnPlayerExit();
}
```

### 6. 经济系统

#### 6.1 资源类型
```csharp
public enum ResourceType
{
    // 基础矿物
    ScrapMetal,     // 废金属
    CopperOre,      // 铜矿
    IronOre,        // 铁矿
    
    // 稀有矿物
    GoldOre,        // 金矿
    CrystalShard,   // 水晶碎片
    Uranium,        // 铀
    
    // 特殊资源
    BioSample,      // 生物样本
    DataFragment,   // 数据碎片
    AncientTech     // 古代科技
}
```

#### 6.2 商店系统 (ShopSystem)
```csharp
public class ShopSystem : MonoBehaviour
{
    public List<ShopItem> availableItems;
    public float priceMultiplier = 1f;
    
    public void BuyItem(ShopItem item);
    public void SellItem(Item item);
    public void RefreshStock();
}
```

### 7. 敌人AI系统

#### 7.1 AI状态机
```csharp
public class EnemyAI : MonoBehaviour
{
    private StateMachine stateMachine;
    
    // AI状态
    public enum AIState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Flee,
        Dead
    }
    
    // 行为配置
    public float detectionRange = 10f;
    public float attackRange = 3f;
    public float fleeHealthThreshold = 0.2f;
}
```

#### 7.2 敌人类型
```csharp
public enum EnemyType
{
    // 生物敌人
    DeepJellyfish,      // 深海水母
    AnglerFish,         // 鮟鱇鱼
    GiantSquid,         // 巨型乌贼
    
    // 机械敌人
    SecurityDrone,      // 安保无人机
    DefenseTurret,      // 防御炮塔
    CorruptedMech,      // 腐化机甲
    
    // 环境危害
    Undercurrent,       // 暗流
    ToxicVent,          // 毒液喷口
    CollapsingRock      // 崩塌岩石
}
```

### 8. UI系统

#### 8.1 UI架构
```csharp
public class UIManager : Singleton<UIManager>
{
    [Header("Panels")]
    public HUD hud;
    public InventoryUI inventoryUI;
    public ShopUI shopUI;
    public PauseMenu pauseMenu;
    public GameOverScreen gameOverScreen;
    
    // 面板管理
    public void OpenPanel(UIPanel panel);
    public void ClosePanel(UIPanel panel);
    public void CloseAllPanels();
}
```

#### 8.2 HUD元素
```csharp
public class HUD : MonoBehaviour
{
    // 状态条
    public Slider healthBar;
    public Slider energyBar;
    public Slider oxygenBar;
    
    // 深度显示
    public TextMeshProUGUI depthText;
    
    // 资源显示
    public TextMeshProUGUI cargoText;
    
    // 小地图
    public RawImage minimap;
    
    // 警告系统
    public void ShowWarning(string message);
    public void ShowDamageIndicator(Vector2 direction);
}
```

---

## 数据架构

### ScriptableObjects

```csharp
// 物品数据
[CreateAssetMenu(fileName = "Item", menuName = "SebeJJ/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite icon;
    public int maxStack;
    public int baseValue;
    public ItemType type;
}

// 武器数据
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
    public AudioClip fireSound;
}

// 升级数据
[CreateAssetMenu(fileName = "Upgrade", menuName = "SebeJJ/Upgrade")]
public class UpgradeData : ScriptableObject
{
    public string upgradeName;
    public UpgradeType type;
    public float value;
    public int cost;
    public List<UpgradeData> prerequisites;
}
```

---

## 音频架构

```csharp
public class AudioManager : Singleton<AudioManager>
{
    [Header("Mixers")]
    public AudioMixer masterMixer;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup sfxGroup;
    public AudioMixerGroup ambienceGroup;
    
    // 音频播放
    public void PlayMusic(AudioClip clip);
    public void PlaySFX(AudioClip clip, Vector3 position);
    public void PlayAmbience(AudioClip clip);
    
    // 动态混音
    public void SetDepthReverb(float depth);
}
```

---

## 性能优化策略

### 1. 对象池 (Object Pooling)
```csharp
public class ObjectPool<T> where T : MonoBehaviour
{
    private Queue<T> pool = new Queue<T>();
    private T prefab;
    
    public T Get();
    public void Return(T obj);
}
```

### 2. 视锥剔除
- 使用 Unity Culling Groups
- 动态加载/卸载远处房间

### 3. 光照优化
- 使用 2D 光照系统
- 限制光源数量
- 烘焙静态光照

### 4. 物理优化
- 使用 Layer-based 碰撞检测
- 合理设置 Rigidbody 类型
- 避免频繁的 GetComponent

---

## 扩展性设计

### 1. 模块化设计
- 每个系统独立，通过事件通信
- 使用接口定义契约
- 依赖注入支持测试

### 2. 配置驱动
- 大量使用 ScriptableObjects
- JSON 配置外部资源
- 热更新支持

### 3. 平台适配
```csharp
public interface IPlatformAdapter
{
    void Initialize();
    void SaveGame(string data);
    string LoadGame();
    void ShowAd();
    void SubmitAchievement(string id);
}
```

---

## 开发规范

### 命名规范
- **类名:** PascalCase (e.g., `PlayerController`)
- **方法名:** PascalCase (e.g., `TakeDamage`)
- **变量名:** camelCase (e.g., `currentHealth`)
- **常量名:** UPPER_SNAKE_CASE (e.g., `MAX_HEALTH`)
- **私有字段:** _camelCase (e.g., `_rigidbody`)

### 代码组织
- 单一职责原则
- 避免魔法数字
- 使用属性而非公共字段
- 充分注释复杂逻辑

### 版本控制
- 使用 Git
- .gitignore 配置
- 分支策略: main / develop / feature/*

---

## 技术栈

| 类别 | 技术 |
|------|------|
| 引擎 | Unity 2022.3 LTS |
| 语言 | C# |
| 2D渲染 | URP (Universal Render Pipeline) |
| 物理 | Unity Physics 2D |
| 输入 | Input System |
| UI | UI Toolkit + TextMeshPro |
| 动画 | Animator + Timeline |
| 音频 | FMOD 或 Unity Audio |
| 版本控制 | Git + Git LFS |
| 协作 | Plastic SCM 可选 |

---

## 风险与缓解

| 风险 | 影响 | 缓解措施 |
|------|------|----------|
| 性能问题 | 高 | 早期性能测试，对象池，LOD |
| 范围蔓延 | 高 | 严格遵循MVP，功能冻结 |
| 2D光照复杂 | 中 | 使用URP，限制光源数量 |
| 平衡性问题 | 中 | 数据驱动，频繁测试迭代 |

---

**文档版本:** 1.0  
**创建日期:** 2026-02-26  
**作者:** 架构师 Agent
