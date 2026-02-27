# SebeJJ V2 战斗特效文档

## 概述

本文档详细说明赛博机甲SebeJJ游戏V2版本新增的战斗特效。

V2版本新增了12个高质量特效，涵盖武器、敌人和环境三个类别，并提供了完整的性能优化方案。

---

## 新增特效清单

### 一、武器特效 (4个)

| 文件名 | 尺寸 | 用途 | 动画帧数 | 循环 |
|--------|------|------|----------|------|
| fx_plasma_cannon_charge.svg | 256x256 | 等离子炮充能 | 多阶段 | 否 |
| fx_emp_pulse.svg | 512x512 | EMP电磁脉冲扩散 | 波纹扩散 | 否 |
| fx_chainsaw_spin.svg | 256x256 | 链锯锯齿旋转 | 持续旋转 | 是 |
| fx_laser_charge.svg | 256x256 | 激光炮蓄力 | 能量聚集 | 否 |

### 二、敌人特效 (4个)

| 文件名 | 尺寸 | 用途 | 动画帧数 | 循环 |
|--------|------|------|----------|------|
| fx_boss_skill_warning.svg | 512x512 | Boss技能预警 | 红圈+倒计时 | 否 |
| fx_enemy_rage_outline.svg | 256x256 | 敌人狂暴状态 | 红色描边+蒸汽 | 是 |
| fx_enemy_death_dissolve.svg | 256x256 | 敌人死亡溶解 | 碎片消散 | 否 |
| fx_hit_freeze_frame.svg | 128x128 | 受击停顿帧 | 全屏闪光 | 否 |

### 三、环境特效 (4个)

| 文件名 | 尺寸 | 用途 | 动画帧数 | 循环 |
|--------|------|------|----------|------|
| fx_depth_pressure_v2.svg | 512x512 | 深度压力视觉 | 暗红渐变 | 是 |
| fx_volcano_vent.svg | 256x512 | 海底火山喷口 | 岩浆喷发 | 是 |
| fx_ancient_runes.svg | 256x256 | 古代遗迹发光符文 | 符文脉动 | 是 |
| fx_bio_luminescence.svg | 512x512 | 生物荧光粒子 | 多色闪烁 | 是 |

---

## 特效详情

### 武器特效

#### 1. 等离子炮充能特效 (fx_plasma_cannon_charge)

**描述**: 多阶段充能效果，包含能量核心、能量环、电弧和粒子效果

**阶段**:
1. 能量核心形成 (白色→青色→蓝色)
2. 能量环旋转扩散
3. 电弧向外辐射
4. 粒子向中心聚集

**颜色**: #FFFFFF → #00FFFF → #0080FF → #4000FF

**Unity配置**:
- 最大粒子数: 100
- 持续时间: 2.0s
- 发射模式: 多阶段爆发

#### 2. EMP电磁脉冲扩散 (fx_emp_pulse)

**描述**: 电磁脉冲从中心向外扩散的波纹效果

**特征**:
- 多层波纹扩散
- 电磁干扰线
- 电弧效果
- 数字干扰效果

**颜色**: #FFFFFF → #00FFFF → #0080FF → #000080

**Unity配置**:
- 最大粒子数: 200
- 持续时间: 1.5s
- 扩散速度: 15单位/秒

#### 3. 链锯锯齿旋转 (fx_chainsaw_spin)

**描述**: 链锯旋转时的锯齿效果和金属火花

**特征**:
- 12个锯齿旋转
- 运动模糊轨迹
- 金属火花飞溅
- 中心轴细节

**颜色**: #C0C0C0 (银色) → #FFFF00 (火花)

**Unity配置**:
- 最大粒子数: 50
- 旋转速度: 15弧度/秒
- 循环: 是

#### 4. 激光炮蓄力 (fx_laser_charge)

**描述**: 激光炮蓄力时的能量聚集效果

**特征**:
- 核心能量球
- 多层蓄能环
- 能量线向中心聚集
- 警告指示器

**颜色**: #FFFFFF → #FF0000 → #FF0040

**Unity配置**:
- 最大粒子数: 80
- 持续时间: 1.5s
- 聚集速度: 5单位/秒

### 敌人特效

#### 5. Boss技能预警 (fx_boss_skill_warning)

**描述**: Boss释放技能前的范围预警效果

**特征**:
- 多层红圈范围指示
- 激光扫描线
- 十字瞄准
- 倒计时数字
- 扇形预警区域

**颜色**: #FF0000 (警告红)

**Unity配置**:
- 最大粒子数: 30
- 预警时间: 3.0s
- 范围: 3-5米

#### 6. 敌人狂暴状态 (fx_enemy_rage_outline)

**描述**: 敌人进入狂暴状态时的红色描边效果

**特征**:
- 多层红色描边
- 狂暴能量溢出
- 红色蒸汽
- 眼睛红光
- 闪电效果

**颜色**: #FF0000 → #FF4040

**Unity配置**:
- 最大粒子数: 50
- 持续时间: 10.0s
- 描边宽度: 3px

#### 7. 敌人死亡溶解 (fx_enemy_death_dissolve)

**描述**: 敌人死亡时的溶解消散效果

**特征**:
- 噪点溶解效果
- 金属碎片飞散
- 能量粒子消散
- 火花效果
- 烟雾效果

**颜色**: #C0C0C0 → #808080 → #404040

**Unity配置**:
- 最大粒子数: 100
- 持续时间: 1.5s
- 重力: 0.8

#### 8. 受击停顿帧特效 (fx_hit_freeze_frame)

**描述**: 击中敌人时的停顿帧闪光效果

**特征**:
- 全屏闪光
- 冲击波纹
- 星形闪光
- 放射线
- 震动效果

**颜色**: #FFFFFF

**Unity配置**:
- 最大粒子数: 1
- 持续时间: 0.1s
- 时间缩放: 0.05

### 环境特效

#### 9. 深度压力视觉效果 (fx_depth_pressure_v2)

**描述**: 深海区域的深度压力视觉效果

**特征**:
- 从浅蓝到暗红的渐变
- 暗角效果
- 压力线扭曲
- 视野模糊区域
- 深度标记

**颜色**: #004080 → #002040 → #200020 → #400000

**Unity配置**:
- 最大粒子数: 50
- 循环: 是
- 后处理: Vignette + 颜色分级

#### 10. 海底火山喷口 (fx_volcano_vent)

**描述**: 海底火山喷口的岩浆和热浪效果

**特征**:
- 岩浆池发光
- 喷发柱
- 岩浆滴落
- 火山灰烟雾
- 裂缝发光

**颜色**: #FFFF00 → #FF8800 → #FF0000

**Unity配置**:
- 最大粒子数: 80
- 喷发速度: 3单位/秒
- 热浪扭曲: 启用

#### 11. 古代遗迹发光符文 (fx_ancient_runes)

**描述**: 古代遗迹上的发光符文效果

**特征**:
- 符文圆环
- 中心星形符文
- 8方向外围符文
- 能量粒子
- 连接线

**颜色**: #00FFFF → #0080FF → #4000FF

**Unity配置**:
- 最大粒子数: 40
- 脉动频率: 2s
- 发光强度: 2.0

#### 12. 生物荧光粒子 (fx_bio_luminescence)

**描述**: 深海生物荧光粒子效果

**特征**:
- 绿色荧光粒子
- 青色荧光粒子
- 紫色荧光粒子
- 多尺寸粒子
- 连接线/丝状物

**颜色**: #80FF80 / #80FFFF / #FF80FF

**Unity配置**:
- 最大粒子数: 100
- 循环: 是
- 闪烁: 随机

---

## 文件结构

```
Assets/Art/Effects/V2/
├── fx_plasma_cannon_charge.svg      # 等离子炮充能
├── fx_emp_pulse.svg                 # EMP电磁脉冲
├── fx_chainsaw_spin.svg             # 链锯锯齿旋转
├── fx_laser_charge.svg              # 激光炮蓄力
├── fx_boss_skill_warning.svg        # Boss技能预警
├── fx_enemy_rage_outline.svg        # 敌人狂暴描边
├── fx_enemy_death_dissolve.svg      # 敌人死亡溶解
├── fx_hit_freeze_frame.svg          # 受击停顿帧
├── fx_depth_pressure_v2.svg         # 深度压力视觉
├── fx_volcano_vent.svg              # 海底火山喷口
├── fx_ancient_runes.svg             # 古代遗迹符文
├── fx_bio_luminescence.svg          # 生物荧光粒子
├── UnityParticleConfig.md           # Unity粒子配置
├── PerformanceOptimization.md       # 性能优化指南
└── README.md                        # 本文档
```

---

## 使用指南

### 快速开始

1. 将SVG文件导入Unity作为Sprite
2. 创建Particle System并应用对应的材质
3. 参考UnityParticleConfig.md配置粒子参数
4. 使用PerformanceOptimization.md优化性能

### 代码示例

```csharp
// 播放等离子炮充能
public void PlayPlasmaCharge(Vector3 position)
{
    GameObject effect = VFXManager.Instance.SpawnEffect("PlasmaCharge", position);
    // 自动回收
}

// 播放Boss技能预警
public void PlayBossWarning(Vector3 position, float range)
{
    GameObject effect = VFXManager.Instance.SpawnEffect("BossWarning", position);
    effect.transform.localScale = Vector3.one * range;
}

// 播放受击停顿帧
public void PlayHitFreezeFrame(Vector3 position)
{
    StartCoroutine(FreezeFrameCoroutine(position));
}

IEnumerator FreezeFrameCoroutine(Vector3 position)
{
    GameObject effect = VFXManager.Instance.SpawnEffect("HitFreeze", position);
    
    // 停顿时间
    float originalTimeScale = Time.timeScale;
    Time.timeScale = 0.05f;
    yield return new WaitForSecondsRealtime(0.1f);
    Time.timeScale = originalTimeScale;
}
```

---

## 性能优化

### LOD配置

V2版本所有特效都支持LOD系统:

| LOD级别 | 距离 | 粒子数量 | 材质 |
|--------|------|---------|------|
| LOD0 | 0-10m | 100% | 完整 |
| LOD1 | 10-20m | 70% | 完整 |
| LOD2 | 20-30m | 40% | 简化 |
| LOD3 | 30m+ | 20% | 简化 |

### 对象池

所有特效都配置了对象池，避免运行时频繁创建/销毁:

```csharp
// 对象池配置示例
PoolConfig {
    effectName = "PlasmaCharge",
    poolSize = 5,
    autoReleaseTime = 3f
}
```

### 性能预算

- 最大粒子数: 1000/帧
- 最大特效数: 50/帧
- 最大生成数: 10/帧
- 裁剪距离: 40米

---

## 美术规范

### 颜色规范

| 类型 | 主色 | 辅助色 | 强调色 |
|------|------|--------|--------|
| 等离子/EMP | #00FFFF | #0080FF | #4000FF |
| 激光/狂暴 | #FF0000 | #FF0040 | #FF8080 |
| 岩浆/火焰 | #FFFF00 | #FF8800 | #FF0000 |
| 生物荧光 | #80FF80 | #80FFFF | #FF80FF |
| 古代符文 | #00FFFF | #0080FF | #4000FF |

### 尺寸规范

- 小型特效: 128x128 (停顿帧)
- 中型特效: 256x256 (武器、敌人)
- 大型特效: 512x512 (环境、Boss预警)
- 垂直特效: 256x512 (火山喷口)

### 动画规范

- 快速反馈: 0.1-0.3s (停顿帧)
- 标准特效: 1.0-2.0s (充能、死亡)
- 持续循环: 无缝循环 (环境、狂暴)

---

## 版本历史

- **v2.0** (2026-02-27) - V2版本发布
  - 新增武器特效: 4个
  - 新增敌人特效: 4个
  - 新增环境特效: 4个
  - 完整性能优化方案
  - LOD系统支持
  - 对象池配置

---

## 联系

如有问题请联系特效美术团队。
