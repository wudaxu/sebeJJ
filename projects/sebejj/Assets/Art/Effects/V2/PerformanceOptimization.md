# SebeJJ V2 特效性能优化指南

本文档提供V2版本特效的性能优化配置和最佳实践。

## 1. LOD (Level of Detail) 配置

### 1.1 LOD距离设置

```csharp
public enum VFXQuality
{
    Ultra,    // 0-10米: 完整特效
    High,     // 10-20米: 中等特效
    Medium,   // 20-30米: 简化特效
    Low,      // 30米+: 最小化/关闭
    Culled    // 超出范围: 完全关闭
}

public class VFXLODManager
{
    // LOD距离阈值
    public static readonly float[] LODDistances = { 10f, 20f, 30f, 50f };
    
    // 根据距离获取LOD级别
    public static int GetLODLevel(float distance)
    {
        for (int i = 0; i < LODDistances.Length; i++)
        {
            if (distance < LODDistances[i])
                return i;
        }
        return LODDistances.Length; // Culled
    }
}
```

### 1.2 各LOD级别配置

| LOD级别 | 距离范围 | 粒子数量 | 发射率 | 材质复杂度 | 更新频率 |
|--------|---------|---------|--------|-----------|---------|
| LOD0 (Ultra) | 0-10m | 100% | 100% | 完整 | 每帧 |
| LOD1 (High) | 10-20m | 70% | 70% | 完整 | 每帧 |
| LOD2 (Medium) | 20-30m | 40% | 50% | 简化 | 每2帧 |
| LOD3 (Low) | 30-50m | 20% | 30% | 简化 | 每4帧 |
| Culled | 50m+ | 0% | 0% | - | - |

### 1.3 LOD切换代码示例

```csharp
public class VFXLODController : MonoBehaviour
{
    public ParticleSystem particleSystem;
    public float[] lodEmissionRates = { 1.0f, 0.7f, 0.4f, 0.2f };
    public int[] lodMaxParticles = { 100, 70, 40, 20 };
    
    private Transform cameraTransform;
    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.MainModule main;
    
    void Start()
    {
        cameraTransform = Camera.main.transform;
        emission = particleSystem.emission;
        main = particleSystem.main;
    }
    
    void Update()
    {
        float distance = Vector3.Distance(transform.position, cameraTransform.position);
        int lodLevel = VFXLODManager.GetLODLevel(distance);
        
        if (lodLevel >= lodEmissionRates.Length)
        {
            particleSystem.gameObject.SetActive(false);
            return;
        }
        
        particleSystem.gameObject.SetActive(true);
        
        // 应用LOD设置
        emission.rateOverTime = emission.rateOverTime.constant * lodEmissionRates[lodLevel];
        main.maxParticles = lodMaxParticles[lodLevel];
    }
}
```

## 2. 粒子数量控制

### 2.1 各类型特效粒子预算

```csharp
public class VFXParticleBudget
{
    // 武器特效
    public const int WEAPON_MAX_PARTICLES = 100;
    public const int WEAPON_EMISSION_RATE = 50;
    
    // 敌人特效
    public const int ENEMY_MAX_PARTICLES = 80;
    public const int ENEMY_EMISSION_RATE = 30;
    
    // 环境特效
    public const int ENVIRONMENT_MAX_PARTICLES = 100;
    public const int ENVIRONMENT_EMISSION_RATE = 15;
    
    // UI特效
    public const int UI_MAX_PARTICLES = 50;
    public const int UI_EMISSION_RATE = 20;
    
    // 全局限制
    public const int GLOBAL_MAX_PARTICLES = 1000;
    public const int GLOBAL_MAX_EFFECTS = 50;
}
```

### 2.2 粒子数量监控系统

```csharp
public class VFXParticleMonitor : MonoBehaviour
{
    private static int currentParticleCount = 0;
    private static int activeEffectCount = 0;
    
    public static bool CanSpawnEffect(int particleCount)
    {
        if (currentParticleCount + particleCount > VFXParticleBudget.GLOBAL_MAX_PARTICLES)
            return false;
        if (activeEffectCount >= VFXParticleBudget.GLOBAL_MAX_EFFECTS)
            return false;
        return true;
    }
    
    public static void RegisterEffect(int particleCount)
    {
        currentParticleCount += particleCount;
        activeEffectCount++;
    }
    
    public static void UnregisterEffect(int particleCount)
    {
        currentParticleCount = Mathf.Max(0, currentParticleCount - particleCount);
        activeEffectCount = Mathf.Max(0, activeEffectCount - 1);
    }
}
```

### 2.3 V2特效粒子配置表

| 特效名称 | 最大粒子数 | 发射率 | LOD1 | LOD2 | LOD3 |
|---------|----------|--------|------|------|------|
| fx_plasma_cannon_charge | 100 | 50 | 70 | 40 | 20 |
| fx_emp_pulse | 200 | 爆发 | 140 | 80 | 40 |
| fx_chainsaw_spin | 50 | 30 | 35 | 20 | 10 |
| fx_laser_charge | 80 | 20 | 56 | 32 | 16 |
| fx_boss_skill_warning | 30 | 10 | 21 | 12 | 6 |
| fx_enemy_rage_outline | 50 | 20 | 35 | 20 | 10 |
| fx_enemy_death_dissolve | 100 | 爆发 | 70 | 40 | 20 |
| fx_hit_freeze_frame | 1 | 爆发 | 1 | 1 | 0 |
| fx_depth_pressure_v2 | 50 | 5 | 35 | 20 | 10 |
| fx_volcano_vent | 80 | 20 | 56 | 32 | 16 |
| fx_ancient_runes | 40 | 10 | 28 | 16 | 8 |
| fx_bio_luminescence | 100 | 15 | 70 | 40 | 20 |

## 3. 材质优化

### 3.1 材质类型选择

```csharp
public enum VFXMaterialType
{
    Additive,       // 加法混合，用于光效
    AlphaBlend,     // Alpha混合，用于烟雾
    Multiplicative, // 乘法混合，用于阴影
    Simple          // 简化材质，远距离使用
}

public class VFXMaterialManager
{
    // 材质缓存
    private static Dictionary<VFXMaterialType, Material> materialCache = 
        new Dictionary<VFXMaterialType, Material>();
    
    public static Material GetMaterial(VFXMaterialType type, bool useSimple = false)
    {
        if (useSimple && type == VFXMaterialType.Additive)
            type = VFXMaterialType.Simple;
            
        if (!materialCache.ContainsKey(type))
        {
            materialCache[type] = CreateMaterial(type);
        }
        return materialCache[type];
    }
    
    private static Material CreateMaterial(VFXMaterialType type)
    {
        Material mat = null;
        
        switch (type)
        {
            case VFXMaterialType.Additive:
                mat = new Material(Shader.Find("Particles/Standard Unlit"));
                mat.SetFloat("_Mode", 1);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                break;
                
            case VFXMaterialType.AlphaBlend:
                mat = new Material(Shader.Find("Particles/Standard Unlit"));
                mat.SetFloat("_Mode", 2);
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                break;
                
            case VFXMaterialType.Simple:
                mat = new Material(Shader.Find("Mobile/Particles/Additive"));
                break;
        }
        
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.renderQueue = 3000;
        
        return mat;
    }
}
```

### 3.2 贴图优化

```csharp
public class VFXTextureOptimizer
{
    // 贴图尺寸规范
    public static readonly Dictionary<string, Vector2Int> TextureSizes = 
        new Dictionary<string, Vector2Int>
    {
        { "fx_plasma_cannon_charge", new Vector2Int(256, 256) },
        { "fx_emp_pulse", new Vector2Int(512, 512) },
        { "fx_chainsaw_spin", new Vector2Int(256, 256) },
        { "fx_laser_charge", new Vector2Int(256, 256) },
        { "fx_boss_skill_warning", new Vector2Int(512, 512) },
        { "fx_enemy_rage_outline", new Vector2Int(256, 256) },
        { "fx_enemy_death_dissolve", new Vector2Int(256, 256) },
        { "fx_hit_freeze_frame", new Vector2Int(128, 128) },
        { "fx_depth_pressure_v2", new Vector2Int(512, 512) },
        { "fx_volcano_vent", new Vector2Int(256, 512) },
        { "fx_ancient_runes", new Vector2Int(256, 256) },
        { "fx_bio_luminescence", new Vector2Int(512, 512) }
    };
    
    // 贴图导入设置
    public static void ConfigureTextureImport(TextureImporter importer, string effectName)
    {
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.textureCompression = TextureImporterCompression.Compressed;
        importer.compressionQuality = 50;
        
        // 移动端使用ETC2格式
        var platformSettings = importer.GetPlatformTextureSettings("Android");
        platformSettings.format = TextureImporterFormat.ETC2_RGBA8;
        platformSettings.compressionQuality = 50;
        importer.SetPlatformTextureSettings(platformSettings);
        
        var iosSettings = importer.GetPlatformTextureSettings("iPhone");
        iosSettings.format = TextureImporterFormat.ASTC_6x6;
        iosSettings.compressionQuality = 50;
        importer.SetPlatformTextureSettings(iosSettings);
    }
}
```

## 4. 对象池配置

### 4.1 对象池管理器

```csharp
public class VFXObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class PoolConfig
    {
        public string effectName;
        public GameObject prefab;
        public int poolSize;
        public float autoReleaseTime;
        public int priority; // 0 = 最高优先级
    }
    
    public PoolConfig[] poolConfigs = new PoolConfig[]
    {
        // 武器特效
        new PoolConfig { effectName = "PlasmaCharge", poolSize = 5, autoReleaseTime = 3f, priority = 0 },
        new PoolConfig { effectName = "EMPPulse", poolSize = 3, autoReleaseTime = 2f, priority = 1 },
        new PoolConfig { effectName = "ChainsawSpin", poolSize = 10, autoReleaseTime = 1f, priority = 0 },
        new PoolConfig { effectName = "LaserCharge", poolSize = 5, autoReleaseTime = 2f, priority = 0 },
        
        // 敌人特效
        new PoolConfig { effectName = "BossWarning", poolSize = 3, autoReleaseTime = 4f, priority = 1 },
        new PoolConfig { effectName = "EnemyRage", poolSize = 10, autoReleaseTime = 12f, priority = 2 },
        new PoolConfig { effectName = "EnemyDissolve", poolSize = 15, autoReleaseTime = 2f, priority = 1 },
        new PoolConfig { effectName = "HitFreeze", poolSize = 20, autoReleaseTime = 0.2f, priority = 0 },
        
        // 环境特效
        new PoolConfig { effectName = "DepthPressure", poolSize = 2, autoReleaseTime = 15f, priority = 3 },
        new PoolConfig { effectName = "VolcanoVent", poolSize = 5, autoReleaseTime = 10f, priority = 2 },
        new PoolConfig { effectName = "AncientRunes", poolSize = 10, autoReleaseTime = 8f, priority = 2 },
        new PoolConfig { effectName = "BioLuminescence", poolSize = 3, autoReleaseTime = 12f, priority = 3 }
    };
    
    private Dictionary<string, Queue<GameObject>> pools = 
        new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, PoolConfig> configMap = 
        new Dictionary<string, PoolConfig>();
    
    void Start()
    {
        InitializePools();
    }
    
    void InitializePools()
    {
        foreach (var config in poolConfigs)
        {
            configMap[config.effectName] = config;
            pools[config.effectName] = new Queue<GameObject>();
            
            for (int i = 0; i < config.poolSize; i++)
            {
                GameObject obj = Instantiate(config.prefab);
                obj.SetActive(false);
                pools[config.effectName].Enqueue(obj);
            }
        }
    }
    
    public GameObject GetFromPool(string effectName)
    {
        if (pools.ContainsKey(effectName) && pools[effectName].Count > 0)
        {
            GameObject obj = pools[effectName].Dequeue();
            obj.SetActive(true);
            StartCoroutine(ReturnToPoolAfterDelay(effectName, obj));
            return obj;
        }
        
        // 池为空时创建新对象
        if (configMap.ContainsKey(effectName))
        {
            return Instantiate(configMap[effectName].prefab);
        }
        
        return null;
    }
    
    System.Collections.IEnumerator ReturnToPoolAfterDelay(string effectName, GameObject obj)
    {
        yield return new WaitForSeconds(configMap[effectName].autoReleaseTime);
        
        obj.SetActive(false);
        if (pools.ContainsKey(effectName))
        {
            pools[effectName].Enqueue(obj);
        }
        else
        {
            Destroy(obj);
        }
    }
}
```

### 4.2 对象池使用示例

```csharp
public class VFXSpawner : MonoBehaviour
{
    public VFXObjectPool objectPool;
    
    public void SpawnPlasmaCharge(Vector3 position)
    {
        GameObject effect = objectPool.GetFromPool("PlasmaCharge");
        if (effect != null)
        {
            effect.transform.position = position;
            effect.transform.rotation = Quaternion.identity;
        }
    }
    
    public void SpawnHitFreezeFrame(Vector3 position)
    {
        GameObject effect = objectPool.GetFromPool("HitFreeze");
        if (effect != null)
        {
            effect.transform.position = position;
            // 停顿帧特效需要特殊处理
            StartCoroutine(FreezeFrameEffect());
        }
    }
    
    System.Collections.IEnumerator FreezeFrameEffect()
    {
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(0.1f);
        Time.timeScale = originalTimeScale;
    }
}
```

## 5. 性能监控

### 5.1 实时性能统计

```csharp
public class VFXPerformanceProfiler : MonoBehaviour
{
    private float updateInterval = 1f;
    private float lastUpdateTime;
    
    // 统计数据
    public int activeParticleCount { get; private set; }
    public int activeEffectCount { get; private set; }
    public float averageParticlePerEffect { get; private set; }
    public Dictionary<string, int> effectTypeCount = new Dictionary<string, int>();
    
    void Update()
    {
        if (Time.time - lastUpdateTime > updateInterval)
        {
            UpdateStats();
            lastUpdateTime = Time.time;
        }
    }
    
    void UpdateStats()
    {
        ParticleSystem[] allParticles = FindObjectsOfType<ParticleSystem>();
        activeEffectCount = 0;
        activeParticleCount = 0;
        effectTypeCount.Clear();
        
        foreach (var ps in allParticles)
        {
            if (ps.isPlaying)
            {
                activeEffectCount++;
                activeParticleCount += ps.particleCount;
                
                string effectName = ps.gameObject.name;
                if (!effectTypeCount.ContainsKey(effectName))
                    effectTypeCount[effectName] = 0;
                effectTypeCount[effectName]++;
            }
        }
        
        averageParticlePerEffect = activeEffectCount > 0 ? 
            (float)activeParticleCount / activeEffectCount : 0;
    }
    
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"=== VFX Performance ===");
        GUILayout.Label($"Active Effects: {activeEffectCount}");
        GUILayout.Label($"Active Particles: {activeParticleCount}");
        GUILayout.Label($"Avg Particles/Effect: {averageParticlePerEffect:F1}");
        
        if (activeParticleCount > VFXParticleBudget.GLOBAL_MAX_PARTICLES)
        {
            GUILayout.Label($"WARNING: Particle budget exceeded!", 
                new GUIStyle { normal = new GUIStyleState { textColor = Color.red } });
        }
        GUILayout.EndArea();
    }
}
```

### 5.2 性能警告系统

```csharp
public class VFXPerformanceWarnings
{
    public static void CheckPerformance()
    {
        // 检查粒子数量
        if (VFXParticleMonitor.currentParticleCount > VFXParticleBudget.GLOBAL_MAX_PARTICLES * 0.9f)
        {
            Debug.LogWarning($"[VFX] Particle count approaching limit: " +
                $"{VFXParticleMonitor.currentParticleCount}/{VFXParticleBudget.GLOBAL_MAX_PARTICLES}");
        }
        
        // 检查特效数量
        if (VFXParticleMonitor.activeEffectCount > VFXParticleBudget.GLOBAL_MAX_EFFECTS * 0.9f)
        {
            Debug.LogWarning($"[VFX] Effect count approaching limit: " +
                $"{VFXParticleMonitor.activeEffectCount}/{VFXParticleBudget.GLOBAL_MAX_EFFECTS}");
        }
        
        // 检查Overdraw
        // 需要集成Frame Debugger数据
    }
}
```

## 6. 平台特定优化

### 6.1 移动端优化

```csharp
public class VFXMobileOptimizer
{
    public static void ApplyMobileOptimizations(ParticleSystem ps)
    {
        var main = ps.main;
        var emission = ps.emission;
        
        // 降低粒子数量
        main.maxParticles = Mathf.RoundToInt(main.maxParticles * 0.6f);
        
        // 降低发射率
        if (emission.rateOverTime.constant > 0)
        {
            emission.rateOverTime = emission.rateOverTime.constant * 0.7f;
        }
        
        // 使用简化材质
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.material = VFXMaterialManager.GetMaterial(VFXMaterialType.Simple);
        
        // 禁用阴影
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
    }
}
```

### 6.2 主机/PC端优化

```csharp
public class VFXDesktopOptimizer
{
    public static void ApplyDesktopOptimizations(ParticleSystem ps)
    {
        // 桌面端可以使用更高质量设置
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        
        // 启用阴影 (如果性能允许)
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        renderer.receiveShadows = true;
        
        // 使用高质量材质
        renderer.material = VFXMaterialManager.GetMaterial(VFXMaterialType.Additive);
    }
}
```

## 7. 最佳实践清单

### 7.1 创建特效时

- [ ] 设置合理的最大粒子数 (不超过100)
- [ ] 使用对象池管理特效
- [ ] 配置LOD系统
- [ ] 选择合适的材质类型
- [ ] 优化贴图尺寸
- [ ] 添加距离裁剪

### 7.2 运行时

- [ ] 监控粒子数量
- [ ] 使用LOD控制远距离特效
- [ ] 及时回收不用的特效
- [ ] 避免特效重叠产生Overdraw
- [ ] 限制同屏特效数量

### 7.3 测试时

- [ ] 在目标设备上测试
- [ ] 使用Frame Debugger分析
- [ ] 检查内存使用情况
- [ ] 验证LOD切换效果
- [ ] 测试对象池性能
