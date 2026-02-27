# SebeJJ - API 代码规范

## 命名规范

### 类名
- 使用 PascalCase
- 名词或名词短语
- 示例: `MissionManager`, `MechConfig`

### 接口名
- 使用 PascalCase
- 以 I 开头
- 示例: `ISaveable`, `IDamageable`

### 方法名
- 使用 PascalCase
- 动词或动词短语
- 示例: `StartMission()`, `CalculateDamage()`

### 变量名
- 使用 camelCase
- 私有字段使用 _ 前缀
- 示例: `currentDepth`, `_instance`

### 常量名
- 使用 UPPER_SNAKE_CASE
- 示例: `MAX_DEPTH`, `DEFAULT_SPEED`

### 枚举名
- 使用 PascalCase
- 枚举值使用 PascalCase
- 示例:
```csharp
public enum MissionType {
    Collection,
    Elimination,
    Exploration
}
```

## 代码组织

### 命名空间
```csharp
namespace SebeJJ.Systems {
    // 系统相关
}

namespace SebeJJ.Entities {
    // 实体相关
}

namespace SebeJJ.UI {
    // UI相关
}

namespace SebeJJ.Utils {
    // 工具类
}
```

### 类结构
```csharp
public class ExampleClass : MonoBehaviour {
    // 1. 常量
    public const float MAX_VALUE = 100f;
    
    // 2. 静态字段
    private static int _instanceCount;
    
    // 3. 序列化字段
    [SerializeField] private float _moveSpeed;
    
    // 4. 私有字段
    private float _currentHealth;
    
    // 5. 属性
    public float Health => _currentHealth;
    
    // 6. Unity生命周期
    private void Awake() { }
    private void Start() { }
    private void Update() { }
    
    // 7. 公共方法
    public void TakeDamage(float damage) { }
    
    // 8. 私有方法
    private void Die() { }
}
```

## 注释规范

### XML文档注释
```csharp
/// <summary>
/// 开始一个新的委托任务
/// </summary>
/// <param name="missionId">委托ID</param>
/// <returns>是否成功开始</returns>
public bool StartMission(string missionId) {
    // 实现
}
```

### 行内注释
```csharp
// 检查玩家是否有足够的能量
if (currentEnergy < requiredEnergy) {
    return false;
}
```

## 最佳实践

### 单例模式
```csharp
public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }
    
    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
```

### 事件定义
```csharp
public static class GameEvents {
    public static event Action<MissionData> OnMissionStarted;
    public static event Action<MissionData, bool> OnMissionCompleted;
    
    public static void MissionStarted(MissionData mission) {
        OnMissionStarted?.Invoke(mission);
    }
}
```

### ScriptableObject
```csharp
[CreateAssetMenu(fileName = "NewMechConfig", menuName = "SebeJJ/MechConfig")]
public class MechConfig : ScriptableObject {
    [Header("基础属性")]
    public float maxHull = 100f;
    public float maxEnergy = 100f;
    
    [Header("装备槽位")]
    public List<EquipmentSlot> equipmentSlots;
}
```

## 性能建议

1. **缓存组件引用**
```csharp
private Transform _transform;
private void Awake() {
    _transform = transform;
}
```

2. **避免在Update中分配内存**
```csharp
// 不好
void Update() {
    var enemies = FindObjectsOfType<Enemy>();
}

// 好
private List<Enemy> _enemies = new List<Enemy>();
void Update() {
    // 使用缓存的列表
}
```

3. **使用对象池**
```csharp
var projectile = ObjectPool.Get(PrefabType.Projectile);
// 使用后
ObjectPool.Return(projectile);
```

## 错误处理
```csharp
public bool LoadMission(string missionId) {
    if (string.IsNullOrEmpty(missionId)) {
        Debug.LogError("[MissionManager] Mission ID cannot be null or empty");
        return false;
    }
    
    var mission = GetMissionData(missionId);
    if (mission == null) {
        Debug.LogError($"[MissionManager] Mission not found: {missionId}");
        return false;
    }
    
    // 正常逻辑
    return true;
}
```

## Week 2 新增规范

### 委托系统接口
```csharp
public interface IMissionSystem {
    MissionData CurrentMission { get; }
    
    // 委托管理
    bool AcceptMission(string missionId);
    void AbandonMission();
    void CompleteMission(bool success);
    
    // 委托查询
    List<MissionData> GetAvailableMissions();
    List<MissionData> GetCompletedMissions();
    
    // 进度更新
    void UpdateObjective(string objectiveId, int progress);
    
    // 事件
    event Action<MissionData> OnMissionStarted;
    event Action<MissionData, bool> OnMissionCompleted;
}
```

### 资源系统接口
```csharp
public interface IResourceSystem {
    Inventory PlayerInventory { get; }
    Inventory Storage { get; }
    
    // 采集
    CollectResult TryCollectResource(ResourceNode node);
    float CalculateCollectTime(ResourceData resource);
    
    // 背包管理
    bool AddToInventory(ResourceData resource, int amount);
    bool RemoveFromInventory(string resourceId, int amount);
    bool TransferToStorage(string resourceId, int amount);
    
    // 查询
    int GetItemCount(string resourceId);
    float GetInventoryWeight();
    float GetRemainingCapacity();
    
    // 事件
    event Action<ResourceData, int> OnItemAdded;
    event Action<ResourceData, int> OnItemRemoved;
}
```

### 事件定义规范
```csharp
// 所有游戏事件实现此接口
public interface IGameEvent { }

// 委托事件
public struct MissionStartedEvent : IGameEvent {
    public string MissionId;
    public string Title;
    public int TargetDepth;
}

public struct MissionCompletedEvent : IGameEvent {
    public string MissionId;
    public bool Success;
    public RewardData Reward;
}

// 资源事件
public struct ResourceCollectedEvent : IGameEvent {
    public string ResourceId;
    public string ResourceName;
    public int Amount;
    public float Weight;
}

public struct InventoryChangedEvent : IGameEvent {
    public float CurrentWeight;
    public float MaxCapacity;
    public int ItemCount;
}
```

### 泛型事件总线 (推荐)
```csharp
// 类型安全的事件总线
public static class EventBus<T> where T : IGameEvent {
    public static event Action<T> OnEvent;
    
    public static void Trigger(T evt) {
        OnEvent?.Invoke(evt);
    }
    
    public static void Subscribe(Action<T> handler) {
        OnEvent += handler;
    }
    
    public static void Unsubscribe(Action<T> handler) {
        OnEvent -= handler;
    }
}

// 使用示例
public class MissionUI : MonoBehaviour {
    private void OnEnable() {
        EventBus<MissionStartedEvent>.Subscribe(OnMissionStarted);
    }
    
    private void OnDisable() {
        EventBus<MissionStartedEvent>.Unsubscribe(OnMissionStarted);
    }
    
    private void OnMissionStarted(MissionStartedEvent evt) {
        UpdateMissionDisplay(evt.Title, evt.TargetDepth);
    }
}
```

### Service Locator 模式 (可选)
```csharp
public static class ServiceLocator {
    private static readonly Dictionary<Type, object> _services = new();
    
    public static void Register<T>(T service) where T : class {
        _services[typeof(T)] = service;
    }
    
    public static T Get<T>() where T : class {
        return _services.TryGetValue(typeof(T), out var service) 
            ? service as T 
            : null;
    }
    
    public static void Unregister<T>() where T : class {
        _services.Remove(typeof(T));
    }
}

// 使用示例
public class PlayerController : MonoBehaviour {
    private IResourceSystem _resourceSystem;
    
    private void Awake() {
        _resourceSystem = ServiceLocator.Get<IResourceSystem>();
    }
}
```
