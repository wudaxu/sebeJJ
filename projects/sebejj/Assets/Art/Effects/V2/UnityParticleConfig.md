# SebeJJ V2 战斗特效 - Unity粒子系统配置指南

本文档提供V2版本新增特效的Unity粒子系统配置参数。

## 目录
1. [武器特效](#武器特效)
2. [敌人特效](#敌人特效)
3. [环境特效](#环境特效)
4. [性能优化配置](#性能优化配置)

---

## 武器特效

### 1. 等离子炮充能特效 (fx_plasma_cannon_charge)

**用途**: 等离子炮武器充能时的能量聚集效果

**粒子系统配置**:
```
Duration: 2.0s (充能时间)
Looping: false
Start Lifetime: 1.5s
Start Speed: 0
Start Size: 0.5 → 2.0 (随充能进度增大)
Start Color: #00FFFF (青色)
Gravity Modifier: 0
Simulation Space: Local

Emission:
  - Rate over Time: 0
  - Bursts: 
    - Time 0.00, Count 50
    - Time 0.50, Count 30
    - Time 1.00, Count 20

Shape:
  - Shape: Sphere
  - Radius: 0.3
  - Emit from Shell: true

Velocity over Lifetime:
  - Orbital Velocity: 3 (Y轴旋转)
  - Radial: -2 (向中心聚集)

Color over Lifetime:
  - Gradient: #FFFFFF → #00FFFF → #0080FF → #4000FF → 透明

Size over Lifetime:
  - Curve: 0.5 → 1.5 → 2.0

Renderer:
  - Material: Additive Particle
  - Sprite: fx_plasma_cannon_charge (256x256)
  - Max Particles: 100
```

### 2. EMP电磁脉冲扩散 (fx_emp_pulse)

**用途**: EMP武器释放时的电磁脉冲扩散效果

**粒子系统配置**:
```
Duration: 1.5s
Looping: false
Start Lifetime: 1.0s
Start Speed: 15
Start Size: 0.2
Start Color: #00FFFF
Gravity Modifier: 0
Simulation Space: World

Emission:
  - Rate over Time: 0
  - Bursts: Time 0.00, Count 200

Shape:
  - Shape: Sphere
  - Radius: 0.1

Velocity over Lifetime:
  - Speed Modifier: 1.0 → 0.2
  - Linear: (向外扩散)

Color over Lifetime:
  - Gradient: #FFFFFF → #00FFFF → #0080FF → #000080 → 透明

Size over Lifetime:
  - Curve: 0.2 → 3.0 → 5.0

Renderer:
  - Material: Additive Particle
  - Sprite: fx_emp_pulse (512x512)
  - Max Particles: 200
```

### 3. 链锯锯齿旋转 (fx_chainsaw_spin)

**用途**: 链锯武器旋转时的锯齿和火花效果

**粒子系统配置**:
```
Duration: 1.0s
Looping: true
Start Lifetime: 0.5s
Start Speed: 5
Start Size: 0.3
Start Color: #C0C0C0 (银色)
Gravity Modifier: 0.5
Simulation Space: Local

Emission:
  - Rate over Time: 30
  - Bursts: None

Shape:
  - Shape: Circle
  - Radius: 0.8
  - Arc: 360°

Velocity over Lifetime:
  - Orbital Velocity: 15 (高速旋转)
  - Linear: (0, 0, 0)

Color over Lifetime:
  - Gradient: #FFFFFF → #FFFF00 → #FF8800 → 透明

Size over Lifetime:
  - Curve: 0.3 → 0.1

Renderer:
  - Material: Additive Particle
  - Sprite: fx_chainsaw_spin (256x256)
  - Max Particles: 50

附加组件 - 火花子系统:
  - 在击中时触发
  - 发射金属碎片粒子
  - 速度: 8-12
  - 生命周期: 0.3s
```

### 4. 激光炮蓄力 (fx_laser_charge)

**用途**: 激光炮蓄力时的能量聚集效果

**粒子系统配置**:
```
Duration: 1.5s
Looping: false
Start Lifetime: 1.0s
Start Speed: 0
Start Size: 0.3 → 1.5 (随蓄力增大)
Start Color: #FF0040 (红色)
Gravity Modifier: 0
Simulation Space: Local

Emission:
  - Rate over Time: 20
  - Bursts: 
    - Time 0.00, Count 30
    - Time 0.75, Count 50

Shape:
  - Shape: Sphere
  - Radius: 0.5

Velocity over Lifetime:
  - Radial: -5 (向中心聚集)
  - Speed Modifier: 1.0 → 0.3

Color over Lifetime:
  - Gradient: #FFFFFF → #FF0000 → #FF0040 → #FF8080 → 透明

Size over Lifetime:
  - Curve: 0.3 → 1.0 → 1.5

Renderer:
  - Material: Additive Particle
  - Sprite: fx_laser_charge (256x256)
  - Max Particles: 80
```

---

## 敌人特效

### 5. Boss技能预警 (fx_boss_skill_warning)

**用途**: Boss释放技能前的范围预警效果

**粒子系统配置**:
```
Duration: 3.0s (预警时间)
Looping: false
Start Lifetime: 0.5s
Start Speed: 0
Start Size: 1.0 → 3.0 (脉动效果)
Start Color: #FF0000
Gravity Modifier: 0
Simulation Space: World

Emission:
  - Rate over Time: 10
  - Bursts: None

Shape:
  - Shape: Circle
  - Radius: 3.0 (技能范围)
  - Arc: 360°

Color over Lifetime:
  - Gradient: #FF0000 (0.8) → #FF4040 (0.4) → #FF0000 (0.8)
  - 循环脉动

Size over Lifetime:
  - Curve: 1.0 → 1.2 → 1.0 (呼吸效果)

Renderer:
  - Material: Additive Particle
  - Sprite: fx_boss_skill_warning (512x512)
  - Render Mode: Billboard
  - Max Particles: 30

附加组件 - 倒计时数字:
  - 使用UI Text显示
  - 3秒倒计时
  - 颜色: #FF0000
  - 字体大小: 48
```

### 6. 敌人狂暴状态 (fx_enemy_rage_outline)

**用途**: 敌人进入狂暴状态时的红色描边效果

**粒子系统配置**:
```
Duration: 10.0s (狂暴持续时间)
Looping: true
Start Lifetime: 0.3s
Start Speed: 2
Start Size: 0.1
Start Color: #FF0000
Gravity Modifier: 0
Simulation Space: Local

Emission:
  - Rate over Time: 20

Shape:
  - Shape: Mesh (敌人模型轮廓)
  - Emit from Shell: true

Velocity over Lifetime:
  - Linear: (0, 1, 0) (向上飘散)

Color over Lifetime:
  - Gradient: #FF0000 → #FF4040 → 透明

Size over Lifetime:
  - Curve: 0.1 → 0.5 → 0

Renderer:
  - Material: Additive Particle
  - Sprite: fx_enemy_rage_outline (256x256)
  - Max Particles: 50

附加组件 - 屏幕后处理:
  - 敌人材质描边
  - 颜色: #FF0000
  - 描边宽度: 3px
  - 闪烁频率: 0.2s
```

### 7. 敌人死亡溶解 (fx_enemy_death_dissolve)

**用途**: 敌人死亡时的溶解消散效果

**粒子系统配置**:
```
Duration: 1.5s
Looping: false
Start Lifetime: 1.0s
Start Speed: 3
Start Size: 0.2 → 0.5
Start Color: #808080
Gravity Modifier: 0.8
Simulation Space: World

Emission:
  - Rate over Time: 0
  - Bursts: Time 0.00, Count 100

Shape:
  - Shape: Box
  - Box Size: (1, 1.5, 0.5) (敌人尺寸)

Velocity over Lifetime:
  - Speed Modifier: 1.0 → 0.5
  - Linear: (随机方向)

Color over Lifetime:
  - Gradient: #C0C0C0 → #808080 → #404040 → 透明

Size over Lifetime:
  - Curve: 0.2 → 0.3 → 0

Renderer:
  - Material: Alpha Blended Particle
  - Sprite: fx_enemy_death_dissolve (256x256)
  - Max Particles: 100

附加组件 - 溶解着色器:
  - 使用Dissolve Shader
  - 溶解阈值: 0 → 1
  - 溶解边缘发光: #00F0FF
  - 溶解时间: 1.5s
```

### 8. 受击停顿帧特效 (fx_hit_freeze_frame)

**用途**: 击中敌人时的停顿帧闪光效果

**粒子系统配置**:
```
Duration: 0.1s (停顿帧时长)
Looping: false
Start Lifetime: 0.08s
Start Speed: 0
Start Size: 2.0
Start Color: #FFFFFF
Gravity Modifier: 0
Simulation Space: World

Emission:
  - Rate over Time: 0
  - Bursts: Time 0.00, Count 1

Shape:
  - Shape: Sphere
  - Radius: 0.5

Color over Lifetime:
  - Gradient: #FFFFFF (1.0) → 透明

Size over Lifetime:
  - Curve: 2.0 → 3.0

Renderer:
  - Material: Additive Particle
  - Sprite: fx_hit_freeze_frame (128x128)
  - Max Particles: 1

附加组件 - 时间控制:
  - Time.timeScale = 0.05 (停顿)
  - 持续2-3帧后恢复
  - 屏幕轻微震动
```

---

## 环境特效

### 9. 深度压力视觉效果 (fx_depth_pressure_v2)

**用途**: 深海区域的深度压力视觉效果

**粒子系统配置**:
```
Duration: 10.0s
Looping: true
Start Lifetime: 5.0s
Start Speed: 0.1
Start Size: 0.05
Start Color: #004080
Gravity Modifier: 0
Simulation Space: World

Emission:
  - Rate over Time: 5

Shape:
  - Shape: Box (全屏幕)
  - Box Size: (20, 20, 10)

Velocity over Lifetime:
  - Linear: (0, 0.05, 0) (缓慢上升)

Color over Lifetime:
  - Gradient: #004080 (0.4) → #002040 (0.3) → #200020 (0.2) → #400000 (0.1) → 透明
  - 随深度变化颜色

Size over Lifetime:
  - Curve: 0.05 → 0.08

Renderer:
  - Material: Alpha Blended Particle
  - Sprite: fx_depth_pressure_v2 (512x512)
  - Max Particles: 50

附加组件 - 后处理:
  - Vignette暗角效果
  - 颜色分级: 偏红
  - 对比度: +20%
```

### 10. 海底火山喷口 (fx_volcano_vent)

**用途**: 海底火山喷口的岩浆和热浪效果

**粒子系统配置**:
```
Duration: 5.0s
Looping: true
Start Lifetime: 3.0s
Start Speed: 2
Start Size: 0.5 → 2.0
Start Color: #FF8800
Gravity Modifier: -0.3 (向上)
Simulation Space: World

Emission:
  - Rate over Time: 20

Shape:
  - Shape: Cone
  - Angle: 15°
  - Radius: 0.5

Velocity over Lifetime:
  - Linear: (0, 3, 0) (向上喷发)
  - Speed Modifier: 1.0 → 0.5

Color over Lifetime:
  - Gradient: #FFFF00 → #FF8800 → #FF0000 → #404040 → 透明

Size over Lifetime:
  - Curve: 0.5 → 1.5 → 2.0 → 1.0

Renderer:
  - Material: Additive Particle
  - Sprite: fx_volcano_vent (256x512)
  - Max Particles: 80

附加组件 - 热浪扭曲:
  - 使用Heat Distortion Shader
  - 扭曲强度: 0.1
  - 影响范围: 5米
```

### 11. 古代遗迹发光符文 (fx_ancient_runes)

**用途**: 古代遗迹上的发光符文效果

**粒子系统配置**:
```
Duration: 5.0s
Looping: true
Start Lifetime: 2.0s
Start Speed: 0.5
Start Size: 0.2
Start Color: #00FFFF
Gravity Modifier: 0
Simulation Space: Local

Emission:
  - Rate over Time: 10

Shape:
  - Shape: Mesh (符文形状)
  - Emit from Shell: true

Velocity over Lifetime:
  - Linear: (0, 0.5, 0) (向上飘散)

Color over Lifetime:
  - Gradient: #00FFFF → #0080FF → #4000FF → 透明

Size over Lifetime:
  - Curve: 0.2 → 0.4 → 0

Renderer:
  - Material: Additive Particle
  - Sprite: fx_ancient_runes (256x256)
  - Max Particles: 40

附加组件 - 符文发光:
  - 使用Emissive材质
  - 发光颜色: #00FFFF
  - 发光强度: 2.0
  - 脉动频率: 2s
```

### 12. 生物荧光粒子 (fx_bio_luminescence)

**用途**: 深海生物荧光粒子效果

**粒子系统配置**:
```
Duration: 10.0s
Looping: true
Start Lifetime: 4.0s
Start Speed: 0.2
Start Size: 0.1 → 0.3 (随机)
Start Color: #80FF80 (绿色) / #80FFFF (青色) / #FF80FF (紫色)
Gravity Modifier: 0
Simulation Space: World

Emission:
  - Rate over Time: 15

Shape:
  - Shape: Box (大范围)
  - Box Size: (30, 20, 30)

Velocity over Lifetime:
  - Linear: (随机漂移)
  - Speed Modifier: 0.5

Color over Lifetime:
  - Gradient: #80FF80 → #40FF40 → 透明
  - 或 #80FFFF → #40FFFF → 透明
  - 或 #FF80FF → #FF40FF → 透明

Size over Lifetime:
  - Curve: 0.1 → 0.3 → 0.1 (呼吸效果)

Renderer:
  - Material: Additive Particle
  - Sprite: fx_bio_luminescence (512x512)
  - Max Particles: 100

附加组件 - 闪烁效果:
  - 随机闪烁
  - 闪烁间隔: 0.5-2s
  - 闪烁强度: 0.5 → 1.0
```

---

## 性能优化配置

### LOD配置

```csharp
public class VFXLODConfig : MonoBehaviour
{
    [System.Serializable]
    public class LODLevel
    {
        public float distance;
        public int maxParticles;
        public float emissionRate;
        public bool useSimpleShader;
    }
    
    public LODLevel[] lodLevels = new LODLevel[]
    {
        new LODLevel { distance = 0, maxParticles = 100, emissionRate = 1.0f, useSimpleShader = false },
        new LODLevel { distance = 10, maxParticles = 50, emissionRate = 0.7f, useSimpleShader = false },
        new LODLevel { distance = 20, maxParticles = 20, emissionRate = 0.4f, useSimpleShader = true },
        new LODLevel { distance = 30, maxParticles = 0, emissionRate = 0f, useSimpleShader = true }
    };
}
```

### 粒子数量控制

| 特效类型 | 最大粒子数 | LOD1 | LOD2 | LOD3 |
|---------|----------|------|------|------|
| 武器特效 | 100 | 70 | 30 | 10 |
| 敌人特效 | 80 | 50 | 20 | 0 |
| 环境特效 | 100 | 60 | 30 | 10 |
| UI特效 | 50 | 30 | 10 | 0 |

### 材质优化

```csharp
// 高性能Additive材质
Material highPerfAdditive = new Material(Shader.Find("Particles/Standard Unlit"));
highPerfAdditive.SetFloat("_Mode", 1); // Additive
highPerfAdditive.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
highPerfAdditive.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
highPerfAdditive.SetInt("_ZWrite", 0);
highPerfAdditive.DisableKeyword("_ALPHATEST_ON");
highPerfAdditive.DisableKeyword("_ALPHABLEND_ON");
highPerfAdditive.EnableKeyword("_ALPHAPREMULTIPLY_ON");
highPerfAdditive.renderQueue = 3000;

// 简化版材质 (远距离使用)
Material simpleAdditive = new Material(Shader.Find("Mobile/Particles/Additive"));
```

### 对象池配置

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
    }
    
    public PoolConfig[] poolConfigs = new PoolConfig[]
    {
        new PoolConfig { effectName = "PlasmaCharge", poolSize = 5, autoReleaseTime = 3f },
        new PoolConfig { effectName = "EMPPulse", poolSize = 3, autoReleaseTime = 2f },
        new PoolConfig { effectName = "ChainsawSpin", poolSize = 10, autoReleaseTime = 1f },
        new PoolConfig { effectName = "LaserCharge", poolSize = 5, autoReleaseTime = 2f },
        new PoolConfig { effectName = "BossWarning", poolSize = 3, autoReleaseTime = 4f },
        new PoolConfig { effectName = "EnemyRage", poolSize = 10, autoReleaseTime = 12f },
        new PoolConfig { effectName = "EnemyDissolve", poolSize = 15, autoReleaseTime = 2f },
        new PoolConfig { effectName = "HitFreeze", poolSize = 20, autoReleaseTime = 0.2f },
        new PoolConfig { effectName = "DepthPressure", poolSize = 2, autoReleaseTime = 15f },
        new PoolConfig { effectName = "VolcanoVent", poolSize = 5, autoReleaseTime = 10f },
        new PoolConfig { effectName = "AncientRunes", poolSize = 10, autoReleaseTime = 8f },
        new PoolConfig { effectName = "BioLuminescence", poolSize = 3, autoReleaseTime = 12f }
    };
}
```

### 性能预算

```csharp
public class VFXPerformanceBudget
{
    // 每帧最大粒子数
    public const int MAX_PARTICLES_PER_FRAME = 1000;
    
    // 同时播放的最大特效数
    public const int MAX_CONCURRENT_EFFECTS = 50;
    
    // 每帧最大特效生成数
    public const int MAX_EFFECTS_SPAWN_PER_FRAME = 10;
    
    // 距离裁剪
    public const float CULL_DISTANCE = 40f;
    
    // 屏幕外裁剪
    public const bool CULL_WHEN_OFFSCREEN = true;
}
```

---

## 版本历史

- **v2.0** (2026-02-27) - V2版本，新增12个特效
  - 武器特效: 4个 (等离子炮、EMP、链锯、激光蓄力)
  - 敌人特效: 4个 (Boss预警、狂暴、死亡溶解、停顿帧)
  - 环境特效: 4个 (深度压力、火山喷口、古代符文、生物荧光)
  - 性能优化: LOD配置、粒子数量控制、对象池
