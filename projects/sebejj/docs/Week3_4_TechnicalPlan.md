# Week 3-4 技术方案规划

## 1. Week 3: 下潜系统技术方案

### 1.1 系统架构

```
┌─────────────────────────────────────────────────────────────────┐
│                      DivingSystem                               │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐             │
│  │DepthManager │  │Environment  │  │SpawnManager │             │
│  │             │  │   Manager   │  │             │             │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘             │
│         │                │                │                     │
│         └────────────────┼────────────────┘                     │
│                          ▼                                     │
│                   ┌─────────────┐                              │
│                   │  DiveState  │                              │
│                   │   Machine   │                              │
│                   └──────┬──────┘                              │
│                          │                                     │
│         ┌────────────────┼────────────────┐                    │
│         ▼                ▼                ▼                    │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐            │
│  │   Surface   │  │   Diving    │  │  Exploring  │            │
│  │   (基地)     │  │   (下潜中)   │  │   (探索中)   │            │
│  └─────────────┘  └─────────────┘  └─────────────┘            │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### 1.2 深度管理系统

```csharp
public class DepthManager : MonoBehaviour {
    [Header("深度配置")]
    [SerializeField] private float _maxDepth = 100f;
    [SerializeField] private float _diveSpeed = 5f;
    [SerializeField] private float _ascendSpeed = 8f;
    
    [Header("深度层配置")]
    [SerializeField] private List<DepthLayerConfig> _depthLayers;
    
    // 当前状态
    public float CurrentDepth { get; private set; }
    public DepthLayer CurrentLayer { get; private set; }
    public DiveState State { get; private set; }
    
    // 事件
    public event Action<float> OnDepthChanged;
    public event Action<DepthLayer> OnLayerChanged;
    
    public void StartDive() {
        State = DiveState.Diving;
        StartCoroutine(DiveCoroutine());
    }
    
    private IEnumerator DiveCoroutine() {
        while (State == DiveState.Diving && CurrentDepth < _maxDepth) {
            // 更新深度
            CurrentDepth += _diveSpeed * Time.deltaTime;
            
            // 检查层变化
            var newLayer = GetLayerAtDepth(CurrentDepth);
            if (newLayer != CurrentLayer) {
                CurrentLayer = newLayer;
                OnLayerChanged?.Invoke(CurrentLayer);
                
                // 触发环境变化
                EnvironmentManager.SetLayer(CurrentLayer);
            }
            
            OnDepthChanged?.Invoke(CurrentDepth);
            yield return null;
        }
        
        if (CurrentDepth >= _maxDepth) {
            State = DiveState.Exploring;
        }
    }
}

[System.Serializable]
public class DepthLayerConfig {
    public DepthLayer layer;
    public float minDepth;
    public float maxDepth;
    public float pressureDamage;
    public float visibility;
    public Color waterColor;
    public AnimationCurve spawnRateCurve;
}
```

### 1.3 环境效果系统

```csharp
public class EnvironmentManager : MonoBehaviour {
    [Header("视觉效果")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private SpriteRenderer _waterOverlay;
    [SerializeField] private ParticleSystem _bubbleEffect;
    
    [Header("光照")]
    [SerializeField] private Light2D _globalLight;
    [SerializeField] private AnimationCurve _lightIntensityCurve;
    
    private DepthLayer _currentLayer;
    
    public void SetLayer(DepthLayer layer) {
        if (_currentLayer == layer) return;
        _currentLayer = layer;
        
        var config = GetConfig(layer);
        
        // 渐变切换环境
        StartCoroutine(TransitionEnvironment(config));
    }
    
    private IEnumerator TransitionEnvironment(DepthLayerConfig config) {
        float duration = 2f;
        float elapsed = 0f;
        
        Color startColor = _waterOverlay.color;
        float startIntensity = _globalLight.intensity;
        
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // 水色渐变
            _waterOverlay.color = Color.Lerp(startColor, config.waterColor, t);
            
            // 光照渐变
            _globalLight.intensity = Mathf.Lerp(startIntensity, 
                _lightIntensityCurve.Evaluate(config.minDepth / 100f), t);
            
            yield return null;
        }
    }
    
    // 压强伤害
    public void ApplyPressureDamage(PlayerController player) {
        var config = GetConfig(_currentLayer);
        if (config.pressureDamage > 0) {
            player.TakeDamage(config.pressureDamage * Time.deltaTime, DamageType.Pressure);
        }
    }
}
```

### 1.4 场景生成系统

```csharp
public class DiveSceneGenerator : MonoBehaviour {
    [Header("地图配置")]
    [SerializeField] private Tilemap _groundTilemap;
    [SerializeField] private Tilemap _wallTilemap;
    [SerializeField] private int _mapWidth = 100;
    [SerializeField] private int _mapHeight = 200;
    
    [Header("生成配置")]
    [SerializeField] private List<ResourceSpawnConfig> _resourceConfigs;
    [SerializeField] private List<EnemySpawnConfig> _enemyConfigs;
    
    private System.Random _random;
    
    public void Generate(int seed, DepthLayer layer) {
        _random = new System.Random(seed);
        
        // 1. 生成地形
        GenerateTerrain(layer);
        
        // 2. 生成资源
        GenerateResources(layer);
        
        // 3. 生成敌人
        GenerateEnemies(layer);
        
        // 4. 生成兴趣点
        GeneratePointsOfInterest(layer);
    }
    
    private void GenerateTerrain(DepthLayer layer) {
        // 使用柏林噪声生成自然地形
        float scale = 0.1f;
        
        for (int x = 0; x < _mapWidth; x++) {
            for (int y = 0; y < _mapHeight; y++) {
                float noise = Mathf.PerlinNoise(x * scale, y * scale);
                
                if (noise > 0.6f) {
                    // 墙壁
                    _wallTilemap.SetTile(new Vector3Int(x, y, 0), GetWallTile(layer));
                } else {
                    // 地面
                    _groundTilemap.SetTile(new Vector3Int(x, y, 0), GetGroundTile(layer));
                }
            }
        }
    }
    
    private void GenerateResources(DepthLayer layer) {
        foreach (var config in _resourceConfigs) {
            if (config.minDepthLayer > layer) continue;
            
            int count = RandomRange(config.minCount, config.maxCount);
            for (int i = 0; i < count; i++) {
                Vector2 position = GetRandomValidPosition();
                SpawnResource(config.resourcePrefab, position);
            }
        }
    }
}
```

### 1.5 玩家控制器

```csharp
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour {
    [Header("移动")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _acceleration = 10f;
    [SerializeField] private float _deceleration = 10f;
    
    [Header("能量")]
    [SerializeField] private float _maxEnergy = 100f;
    [SerializeField] private float _energyRegen = 5f;
    [SerializeField] private float _moveEnergyCost = 1f;
    
    private Rigidbody2D _rigidbody;
    private Vector2 _input;
    private float _currentEnergy;
    
    private void Awake() {
        _rigidbody = GetComponent<Rigidbody2D>();
    }
    
    private void Update() {
        // 输入处理
        _input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
        
        // 能量恢复
        _currentEnergy = Mathf.Min(_currentEnergy + _energyRegen * Time.deltaTime, _maxEnergy);
    }
    
    private void FixedUpdate() {
        // 物理移动
        Vector2 targetVelocity = _input * _moveSpeed;
        
        if (_input.magnitude > 0.1f) {
            // 加速
            _rigidbody.velocity = Vector2.MoveTowards(
                _rigidbody.velocity, 
                targetVelocity, 
                _acceleration * Time.fixedDeltaTime
            );
            
            // 消耗能量
            _currentEnergy -= _moveEnergyCost * Time.fixedDeltaTime;
        } else {
            // 减速
            _rigidbody.velocity = Vector2.MoveTowards(
                _rigidbody.velocity, 
                Vector2.zero, 
                _deceleration * Time.fixedDeltaTime
            );
        }
    }
}
```

---

## 2. Week 4: 整合测试技术方案

### 2.1 系统集成策略

```
┌─────────────────────────────────────────────────────────────────┐
│                      集成测试层次                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Layer 3: 系统测试                                               │
│  ├─ 完整游戏流程测试                                             │
│  ├─ 场景切换测试                                                 │
│  └─ 存档/读档测试                                                │
│                                                                 │
│  Layer 2: 集成测试                                               │
│  ├─ MissionSystem + ResourceSystem                              │
│  ├─ DivingSystem + EnvironmentManager                           │
│  ├─ PlayerController + CombatSystem                             │
│  └─ UISystem + 所有后端系统                                      │
│                                                                 │
│  Layer 1: 单元测试                                               │
│  ├─ 各System独立测试                                             │
│  ├─ 工具类测试                                                   │
│  └─ 数据模型测试                                                 │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### 2.2 自动化测试框架

```csharp
// 集成测试基类
public abstract class IntegrationTest : MonoBehaviour {
    protected virtual void Setup() { }
    protected abstract IEnumerator RunTest();
    protected virtual void Teardown() { }
    
    public IEnumerator Execute() {
        Setup();
        yield return RunTest();
        Teardown();
    }
}

// 示例：委托-资源集成测试
public class MissionResourceIntegrationTest : IntegrationTest {
    private MissionSystem _missionSystem;
    private ResourceSystem _resourceSystem;
    
    protected override void Setup() {
        _missionSystem = FindObjectOfType<MissionSystem>();
        _resourceSystem = FindObjectOfType<ResourceSystem>();
    }
    
    protected override IEnumerator RunTest() {
        // 测试：采集委托完成流程
        var mission = CreateTestMission(MissionType.Collection, "copper_ore", 5);
        _missionSystem.AcceptMission(mission.missionId);
        
        // 模拟采集
        for (int i = 0; i < 5; i++) {
            _resourceSystem.AddToInventory(GetResourceData("copper_ore"), 1);
            yield return null;
        }
        
        // 验证委托完成
        Assert.IsTrue(_missionSystem.IsObjectiveComplete("collect_copper"));
    }
}
```

### 2.3 性能测试方案

```csharp
public class PerformanceTest : MonoBehaviour {
    [Header("测试配置")]
    [SerializeField] private float _testDuration = 60f;
    [SerializeField] private int _targetFPS = 60;
    
    private List<float> _frameTimes = new List<float>();
    private float _testTimer;
    private bool _isTesting;
    
    public void StartTest() {
        _isTesting = true;
        _testTimer = 0f;
        _frameTimes.Clear();
        StartCoroutine(TestCoroutine());
    }
    
    private void Update() {
        if (!_isTesting) return;
        
        _frameTimes.Add(Time.unscaledDeltaTime);
        _testTimer += Time.unscaledDeltaTime;
    }
    
    private IEnumerator TestCoroutine() {
        while (_testTimer < _testDuration) {
            yield return null;
        }
        
        _isTesting = false;
        GenerateReport();
    }
    
    private void GenerateReport() {
        float avgFPS = _frameTimes.Count / _frameTimes.Sum();
        float minFPS = 1f / _frameTimes.Max();
        float percentile1 = 1f / GetPercentile(_frameTimes, 0.99f);
        
        Debug.Log($"=== 性能测试报告 ===");
        Debug.Log($"平均FPS: {avgFPS:F2}");
        Debug.Log($"最低FPS: {minFPS:F2}");
        Debug.Log($"1%低帧FPS: {percentile1:F2}");
        Debug.Log($"目标FPS达成率: {(avgFPS >= _targetFPS ? 100 : avgFPS / _targetFPS * 100):F1}%");
    }
}
```

### 2.4 内存监控方案

```csharp
public class MemoryProfiler : MonoBehaviour {
    private List<MemorySnapshot> _snapshots = new List<MemorySnapshot>();
    
    [System.Serializable]
    public class MemorySnapshot {
        public float Time;
        public long TotalMemory;
        public long MonoHeap;
        public long GfxDriver;
        public int TextureCount;
        public int MeshCount;
    }
    
    public void TakeSnapshot(string tag) {
        var snapshot = new MemorySnapshot {
            Time = Time.time,
            TotalMemory = GC.GetTotalMemory(false),
            MonoHeap = Profiler.GetMonoHeapSizeLong(),
            GfxDriver = Profiler.GetAllocatedMemoryForGraphicsDriver(),
            TextureCount = Profiler.GetRuntimeMemorySizeLong(typeof(Texture2D)),
            MeshCount = Profiler.GetRuntimeMemorySizeLong(typeof(Mesh))
        };
        
        _snapshots.Add(snapshot);
        Debug.Log($"[{tag}] 内存快照: {snapshot.TotalMemory / 1024 / 1024}MB");
    }
    
    public void GenerateReport() {
        // 检测内存泄漏
        for (int i = 1; i < _snapshots.Count; i++) {
            var prev = _snapshots[i - 1];
            var curr = _snapshots[i];
            var growth = curr.TotalMemory - prev.TotalMemory;
            
            if (growth > 10 * 1024 * 1024) { // 10MB增长
                Debug.LogWarning($"检测到内存增长: {growth / 1024 / 1024}MB");
            }
        }
    }
}
```

---

## 3. 技术风险与应对

| 风险 | 影响 | 概率 | 应对方案 |
|------|------|------|----------|
| Tilemap性能问题 | 高 | 中 | 使用Chunk加载，对象池管理Tile |
| 物理碰撞开销 | 中 | 中 | 优化碰撞层，使用触发器替代碰撞器 |
| 存档数据兼容性 | 高 | 低 | 版本控制，迁移脚本 |
| 内存泄漏 | 高 | 中 | 定期内存分析，对象生命周期管理 |
| 帧率不稳定 | 高 | 中 | 性能分析，LOD系统 |

---

## 4. Week 3-4 任务清单

### Week 3 任务

| 任务 | 类型 | 优先级 | 预估工时 |
|------|------|--------|----------|
| DepthManager实现 | 程序 | P0 | 8h |
| EnvironmentManager实现 | 程序 | P0 | 8h |
| DiveSceneGenerator实现 | 程序 | P0 | 10h |
| PlayerController实现 | 程序 | P0 | 8h |
| 相机跟随系统 | 程序 | P0 | 4h |
| 场景切换管理 | 程序 | P0 | 4h |
| 深海环境Tilemap | 美术 | P0 | 16h |
| 深度层视觉效果 | 美术 | P0 | 8h |
| 玩家机甲动画 | 美术 | P0 | 12h |

### Week 4 任务

| 任务 | 类型 | 优先级 | 预估工时 |
|------|------|--------|----------|
| 系统集成测试 | 测试 | P0 | 12h |
| 性能测试 | 测试 | P0 | 8h |
| Bug修复 | 程序 | P0 | 16h |
| 性能优化 | 程序 | P1 | 8h |
| UI Polish | 美术 | P1 | 8h |
| 音效集成 | 程序 | P1 | 4h |
| MVP版本打包 | 程序 | P0 | 4h |

---

*文档版本: 1.0*
*日期: 2026-02-26*
