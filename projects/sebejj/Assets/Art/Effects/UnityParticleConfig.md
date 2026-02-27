# SebeJJ 战斗特效 - Unity粒子系统配置指南

本文档提供所有特效的Unity粒子系统配置参数，方便美术和程序快速实现。

## 目录
1. [武器特效](#武器特效)
2. [敌人特效](#敌人特效)
3. [环境特效](#环境特效)
4. [击杀/掉落特效](#击杀掉落特效)

---

## 武器特效

### 1. 钻头攻击轨迹 (fx_drill_trail)

**用途**: 钻头武器旋转攻击时的轨迹效果

**粒子系统配置**:
```
Duration: 0.5s
Looping: true
Start Lifetime: 0.3s
Start Speed: 0
Start Size: 1.0
Start Color: #FF6600 (橙色)
Gravity Modifier: 0
Simulation Space: Local

Emission:
  - Rate over Time: 50
  - Bursts: None

Shape:
  - Shape: Circle
  - Radius: 0.5
  - Arc: 360°

Velocity over Lifetime:
  - Orbital Velocity: 5 (Y轴旋转)
  
Color over Lifetime:
  - Gradient: #FFFFFF → #FFFF00 → #FFAA00 → #FF4400 (透明)

Size over Lifetime:
  - Curve: 1.0 → 0.5

Renderer:
  - Material: Additive Particle
  - Sprite: fx_drill_trark (64x64)
```

### 2. 激光命中爆炸 (fx_laser_explosion)

**用途**: 激光武器击中目标时的爆炸效果

**粒子系统配置**:
```
Duration: 0.3s
Looping: false
Start Lifetime: 0.5s
Start Speed: 3
Start Size: 0.5
Start Color: #FF00FF (紫色)
Gravity Modifier: 0
Simulation Space: World

Emission:
  - Rate over Time: 0
  - Bursts: Time 0.00, Count 30

Shape:
  - Shape: Sphere
  - Radius: 0.1

Velocity over Lifetime:
  - Speed Modifier: 1.0 → 0.5

Color over Lifetime:
  - Gradient: #FFFFFF → #FFAAFF → #FF00FF → 透明

Size over Lifetime:
  - Curve: 0.5 → 2.0 → 0

Renderer:
  - Material: Additive Particle
  - Sprite: fx_laser_explosion (128x128)
```

### 3. 机械爪撕裂 (fx_claw_tear)

**用途**: 机械爪攻击时的撕裂效果

**粒子系统配置**:
```
Duration: 0.4s
Looping: false
Start Lifetime: 0.3s
Start Speed: 5
Start Size: 0.8
Start Color: #00F0FF (青色)
Gravity Modifier: 0
Simulation Space: World

Emission:
  - Rate over Time: 0
  - Bursts: Time 0.00, Count 15

Shape:
  - Shape: Cone
  - Angle: 15°
  - Radius: 0.1

Velocity over Lifetime:
  - Linear: (根据攻击方向设置)

Color over Lifetime:
  - Gradient: #FFFFFF → #00F0FF → #0080FF → 透明

Size over Lifetime:
  - Curve: 1.0 → 1.5 → 0

Renderer:
  - Material: Additive Particle
  - Sprite: fx_claw_tear (128x128)
  - Render Mode: Billboard
```

---

## 敌人特效

### 4. 机械鱼受击闪烁 (fx_enemy_fish_hit)

**用途**: 机械鱼被击中时的闪烁反馈

**粒子系统配置**:
```
Duration: 0.2s
Looping: false
Start Lifetime: 0.15s
Start Speed: 0
Start Size: 1.0
Start Color: #FFFFFF
Gravity Modifier: 0
Simulation Space: Local

Emission:
  - Rate over Time: 0
  - Bursts: Time 0.00, Count 1

Shape:
  - Shape: Mesh (敌人模型)

Color over Lifetime:
  - Gradient: #FFFFFF (不透明) → #FFFFFF (透明)
  - 快速闪烁3-5次

Renderer:
  - Material: Additive Particle (Overlay)
  - Sprite: fx_enemy_fish_hit (128x128)
```

### 5. 机械蟹防御护盾 (fx_enemy_crab_shield)

**用途**: 机械蟹开启防御护盾时的效果

**粒子系统配置**:
```
Duration: 5.0s (或护盾持续时间)
Looping: true
Start Lifetime: 1.0s
Start Speed: 0
Start Size: 2.0
Start Color: #8B5CF6 (紫色)
Gravity Modifier: 0
Simulation Space: Local

Emission:
  - Rate over Time: 10

Shape:
  - Shape: Sphere
  - Radius: 1.0
  - Emit from Shell: true

Color over Lifetime:
  - Gradient: #8B5CF6 (0.6) → #00F0FF (0.8) → #8B5CF6 (0.6)
  - 循环脉动

Size over Lifetime:
  - Curve: 0.95 → 1.05 (脉动效果)

Rotation over Lifetime:
  - Angular Velocity: 30°/s (Y轴)

Renderer:
  - Material: Additive Particle
  - Sprite: fx_enemy_crab_shield (128x128)
  - Render Mode: Billboard
```

### 6. 机械水母电击范围 (fx_enemy_jellyfish_electric)

**用途**: 机械水母电击攻击的范围预警

**粒子系统配置**:
```
Duration: 2.0s
Looping: true
Start Lifetime: 0.5s
Start Speed: 0
Start Size: 1.0 → 4.0 (随时间扩大)
Start Color: #FFFF00 (黄色)
Gravity Modifier: 0
Simulation Space: Local

Emission:
  - Rate over Time: 20

Shape:
  - Shape: Circle
  - Radius: 0.5
  - Arc: 360°

Color over Lifetime:
  - Gradient: #FFFF00 (0.8) → #00F0FF (0.5) → 透明

Size over Lifetime:
  - Curve: 0.5 → 4.0 (扩散效果)

Renderer:
  - Material: Additive Particle
  - Sprite: fx_enemy_jellyfish_electric (256x256)
  
附加组件 - 电击效果:
  - 使用Line Renderer绘制闪电
  - 随机生成8方向闪电
  - 闪烁频率: 0.1s
```

---

## 环境特效

### 7. 推进器尾焰 (fx_thruster_flame)

**用途**: 机甲推进器喷射效果

**粒子系统配置**:
```
Duration: 1.0s
Looping: true
Start Lifetime: 0.4s
Start Speed: 2
Start Size: 0.5 → 1.0 (随机)
Start Color: #FF6600 (橙色)
Gravity Modifier: -0.5 (向上飘)
Simulation Space: Local

Emission:
  - Rate over Time: 30

Shape:
  - Shape: Cone
  - Angle: 10°
  - Radius: 0.2

Velocity over Lifetime:
  - Linear: (0, -2, 0) (向下喷射)

Color over Lifetime:
  - Gradient: #FFFFFF → #FFFF00 → #FF8800 → #FF4400 → 透明

Size over Lifetime:
  - Curve: 0.5 → 1.5 → 0.3

Renderer:
  - Material: Additive Particle
  - Sprite: fx_thruster_flame (128x128)
  - Max Particles: 50
```

### 8. 水下气泡轨迹 (fx_bubble_trail)

**用途**: 水下移动时的气泡效果

**粒子系统配置**:
```
Duration: 1.0s
Looping: true
Start Lifetime: 2.0s
Start Speed: 0.5
Start Size: 0.2 → 0.8 (随机)
Start Color: #00F0FF (青色)
Gravity Modifier: -0.3 (向上飘)
Simulation Space: World

Emission:
  - Rate over Time: 10

Shape:
  - Shape: Box
  - Box Size: (1, 0.2, 0.5)

Velocity over Lifetime:
  - Linear: (随机X偏移, 1, 0)

Color over Lifetime:
  - Gradient: #FFFFFF (0.8) → #00F0FF (0.4) → 透明

Size over Lifetime:
  - Curve: 0.5 → 1.0 (气泡膨胀)

Renderer:
  - Material: Alpha Blended Particle
  - Sprite: fx_bubble_trail (128x128)
```

### 9. 深度压力视觉 (fx_depth_pressure)

**用途**: 深海区域的压力视觉效果

**粒子系统配置**:
```
Duration: 10.0s
Looping: true
Start Lifetime: 5.0s
Start Speed: 0.1
Start Size: 0.05
Start Color: #0080FF
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
  - Gradient: #00F0FF (0.4) → #0080FF (0.2) → 透明

Renderer:
  - Material: Alpha Blended Particle
  - Sprite: fx_depth_pressure (128x128)
  - 作为后处理效果叠加
```

---

## 击杀/掉落特效

### 10. 敌人死亡爆炸 (fx_enemy_death_explosion)

**用途**: 敌人被击杀时的爆炸效果

**粒子系统配置**:
```
Duration: 0.5s
Looping: false
Start Lifetime: 0.8s
Start Speed: 5
Start Size: 0.8
Start Color: #FF8800
Gravity Modifier: 0.5
Simulation Space: World

Emission:
  - Rate over Time: 0
  - Bursts: Time 0.00, Count 50

Shape:
  - Shape: Sphere
  - Radius: 0.3

Velocity over Lifetime:
  - Speed Modifier: 1.0 → 0.3

Color over Lifetime:
  - Gradient: #FFFFFF → #FFFF00 → #FF8800 → #FF4400 → 透明

Size over Lifetime:
  - Curve: 0.5 → 2.0 → 0

Rotation over Lifetime:
  - Random Between: -180° ~ 180°

Renderer:
  - Material: Additive Particle
  - Sprite: fx_enemy_death_explosion (128x128)
  - Sorting Fudge: -50
```

### 11. 资源掉落光效 (fx_item_drop_glow)

**用途**: 资源掉落时的发光效果

**粒子系统配置**:
```
Duration: 5.0s (或直到被拾取)
Looping: true
Start Lifetime: 1.0s
Start Speed: 0
Start Size: 0.5
Start Color: #FFFF00
Gravity Modifier: 0
Simulation Space: Local

Emission:
  - Rate over Time: 5

Shape:
  - Shape: Sphere
  - Radius: 0.3

Color over Lifetime:
  - Gradient: #FFFF00 (0.8) → #FFAA00 (0.6) → #FFFF00 (0.8)
  - 循环脉动

Size over Lifetime:
  - Curve: 0.8 → 1.2 → 0.8 (呼吸效果)

Rotation over Lifetime:
  - Angular Velocity: 45°/s

Renderer:
  - Material: Additive Particle
  - Sprite: fx_item_drop_glow (64x64)
```

### 12. 拾取反馈 (fx_item_pickup)

**用途**: 玩家拾取资源时的反馈效果

**粒子系统配置**:
```
Duration: 0.4s
Looping: false
Start Lifetime: 0.6s
Start Speed: 3
Start Size: 0.5
Start Color: #FFFF00
Gravity Modifier: -0.5 (向上)
Simulation Space: World

Emission:
  - Rate over Time: 0
  - Bursts: Time 0.00, Count 20

Shape:
  - Shape: Circle
  - Radius: 0.2
  - Arc: 360°

Velocity over Lifetime:
  - Linear: (0, 2, 0) (向上飘散)

Color over Lifetime:
  - Gradient: #FFFFFF → #FFFF00 → #FFAA00 → 透明

Size over Lifetime:
  - Curve: 0.5 → 1.5 → 0

Renderer:
  - Material: Additive Particle
  - Sprite: fx_item_pickup (128x128)
```

---

## 通用材质设置

### Additive Particle 材质
```
Shader: Particles/Standard Unlit
Rendering Mode: Additive
Color: #FFFFFF
Albedo: [特效贴图]
```

### Alpha Blended Particle 材质
```
Shader: Particles/Standard Unlit
Rendering Mode: Transparent
Color: #FFFFFF
Albedo: [特效贴图]
```

---

## 性能优化建议

1. **最大粒子数**: 单个特效不超过100个粒子
2. **贴图尺寸**: 尽量使用64x64或128x128
3. **Overdraw**: 避免大量特效重叠
4. **LOD**: 远距离降低特效质量
5. **对象池**: 频繁使用的特效使用对象池

---

## 版本历史

- v1.0 (2026-02-27) - 初始版本，包含12个特效配置