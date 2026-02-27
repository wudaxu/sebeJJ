# SebeJJ 性能优化文件索引

**文档日期**: 2026-02-27  
**优化版本**: v1.0  

---

## 优化文件清单

### 1. 核心优化文件

| 文件名 | 路径 | 优化类型 | 说明 |
|-------|------|---------|------|
| `MechController.Optimized.cs` | `Assets/Scripts/Player/` | 代码优化 | 高频调用优化，GC减少 |
| `EnemyBase.Optimized.cs` | `Assets/Scripts/Enemies/` | 代码优化 | AI更新频率控制 |
| `DeepOctopus.Optimized.cs` | `Assets/Scripts/Enemies/` | 代码优化 | 浮空动画优化 |
| `Chainsaw.Optimized.cs` | `Assets/Scripts/Weapons/` | 代码优化 | 持续伤害检测优化 |
| `EffectManager.Optimized.cs` | `Assets/Scripts/Utils/` | GC优化 | 对象池实现 |
| `HealthBarAnimator.Optimized.cs` | `Assets/Scripts/UI/Animation/` | 动画优化 | DOTween优化 |

### 2. 系统优化文件

| 文件名 | 路径 | 优化类型 | 说明 |
|-------|------|---------|------|
| `ObjectPool.cs` | `Assets/Scripts/Utils/` | 架构优化 | 通用对象池系统 |
| `ObjectPoolManager.cs` | `Assets/Scripts/Utils/` | 架构优化 | 对象池管理器 |
| `UpdateRateController.cs` | `Assets/Scripts/Core/` | 架构优化 | 更新频率控制 |
| `SceneLoader.Optimized.cs` | `Assets/Scripts/Core/` | 加载优化 | 异步场景加载 |
| `AssetPreloader.cs` | `Assets/Scripts/Core/` | 加载优化 | 资源预加载 |

### 3. 文档文件

| 文件名 | 路径 | 说明 |
|-------|------|------|
| `PERFORMANCE_REPORT.md` | `docs/Optimization/` | 完整性能优化报告 |
| `PERFORMANCE_TEST_DATA.md` | `docs/Optimization/` | 性能测试数据 |
| `OPTIMIZATION_INDEX.md` | `docs/Optimization/` | 本文件 |

---

## 使用方法

### 1. 启用优化版本

将优化后的文件与原文件替换，或同时存在并在场景中使用优化版本：

```csharp
// 原版本
// var controller = gameObject.AddComponent<MechController>();

// 优化版本
var controller = gameObject.AddComponent<MechControllerOptimized>();
```

### 2. 配置对象池

在场景中创建ObjectPoolManager：

```csharp
// 添加ObjectPoolManager到场景
var poolManager = gameObject.AddComponent<ObjectPoolManager>();

// 配置对象池
var configs = new ObjectPoolManager.PoolConfig[]
{
    new ObjectPoolManager.PoolConfig
    {
        poolName = "explosion",
        prefab = explosionPrefab,
        initialSize = 10,
        maxSize = 30
    },
    // ... 其他配置
};
```

### 3. 配置更新频率控制器

为需要控制更新频率的组件添加：

```csharp
public class MyComponent : MonoBehaviour
{
    private UpdateRateController updateController;
    
    private void Awake()
    {
        updateController = gameObject.AddComponent<UpdateRateController>();
        updateController.frequency = UpdateRateController.UpdateFrequency.Every4Frames;
    }
    
    private void Update()
    {
        if (!updateController.ShouldUpdate()) return;
        
        // 实际的更新逻辑
        // 使用 updateController.DeltaTime 而不是 Time.deltaTime
    }
}
```

### 4. 使用异步场景加载

```csharp
// 使用优化后的场景加载器
SceneLoaderOptimized.Instance.LoadSceneAsync("GameScene", () =>
{
    Debug.Log("场景加载完成！");
});

// 监听加载进度
SceneLoaderOptimized.Instance.OnLoadProgress += (progress) =>
{
    loadingBar.value = progress;
};
```

---

## 优化配置建议

### 1. 根据平台配置

#### 移动端配置
```csharp
// 降低更新频率
updateController.frequency = UpdateRateController.UpdateFrequency.Every4Frames;

// 减少对象池大小
poolConfig.initialSize = 5;
poolConfig.maxSize = 15;

// 启用距离裁剪
updateController.enableDistanceCulling = true;
```

#### PC/主机配置
```csharp
// 保持高更新频率
updateController.frequency = UpdateRateController.UpdateFrequency.EveryFrame;

// 增加对象池大小
poolConfig.initialSize = 20;
poolConfig.maxSize = 50;

// 可选距离裁剪
updateController.enableDistanceCulling = false;
```

### 2. 根据场景配置

#### 战斗场景
```csharp
// 预加载战斗相关资源
assetPreloader.PreloadGroup("CombatEffects");

// 增加敌人AI更新频率
enemy.aiUpdateInterval = 0.05f; // 20fps
```

#### 探索场景
```csharp
// 降低敌人AI更新频率
enemy.aiUpdateInterval = 0.2f; // 5fps

// 启用距离裁剪
updateController.enableDistanceCulling = true;
```

---

## 性能监控

### 1. 实时性能显示

```csharp
public class PerformanceMonitor : MonoBehaviour
{
    private float deltaTime = 0.0f;
    
    private void Update()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        
        // 显示FPS
        Debug.Log($"FPS: {fps:0.}");
        
        // 显示内存
        long totalMemory = GC.GetTotalMemory(false);
        Debug.Log($"Memory: {totalMemory / 1024 / 1024}MB");
    }
}
```

### 2. 对象池状态监控

```csharp
// 获取对象池统计
string stats = ObjectPoolManager.Instance.GetPoolStats();
Debug.Log(stats);

// 获取预加载统计
string preloadStats = AssetPreloader.Instance.GetPreloadStats();
Debug.Log(preloadStats);
```

---

## 常见问题

### Q1: 优化后功能异常
**A**: 确保所有依赖的组件都已正确配置，特别是：
- ObjectPoolManager必须在场景中存在
- 预制体名称必须与对象池配置匹配

### Q2: 内存没有明显减少
**A**: 检查：
- 是否正确使用了对象池
- 是否有其他脚本在频繁创建对象
- 纹理资源是否已压缩

### Q3: 帧率提升不明显
**A**: 检查：
- 是否是GPU瓶颈（查看Profiler的GPU时间）
- 是否有其他高开销的脚本
- 是否启用了垂直同步

---

## 版本历史

| 版本 | 日期 | 修改内容 |
|-----|------|---------|
| 1.0 | 2026-02-27 | 初始版本，包含完整优化文件 |

---

**维护者**: 性能优化团队  
**联系方式**: optimization@sebejj.team
