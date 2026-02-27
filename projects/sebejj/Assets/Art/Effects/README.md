# SebeJJ 战斗特效使用文档

## 概述

本文档详细说明赛博机甲SebeJJ游戏中所有战斗特效的使用方法、触发时机和美术规范。

---

## 特效清单

### 一、武器特效

| 文件名 | 尺寸 | 用途 | 动画帧数 | 循环 |
|--------|------|------|----------|------|
| fx_drill_spark.svg | 128x128 | 钻头攻击火花 | 4-6帧 | 是 |
| fx_drill_trail.svg | 128x128 | 钻头攻击轨迹 | 4-6帧 | 是 |
| fx_laser_beam.svg | 256x64 | 激光束发射 | 2-4帧 | 是 |
| fx_laser_explosion.svg | 128x128 | 激光命中爆炸 | 6-8帧 | 否 |
| fx_claw_tear.svg | 128x128 | 机械爪撕裂 | 4-6帧 | 否 |
| fx_hit_spark.svg | 128x128 | 通用击中火花 | 4-6帧 | 否 |

### 二、敌人特效

| 文件名 | 尺寸 | 用途 | 动画帧数 | 循环 |
|--------|------|------|----------|------|
| fx_enemy_fish_hit.svg | 128x128 | 机械鱼受击闪烁 | 3-5次闪烁 | 否 |
| fx_enemy_crab_shield.svg | 128x128 | 机械蟹防御护盾 | 持续循环 | 是 |
| fx_enemy_jellyfish_electric.svg | 256x256 | 机械水母电击范围 | 持续循环 | 是 |
| fx_shield_impact.svg | 128x128 | 护盾受击涟漪 | 4-6帧 | 否 |

### 三、环境特效

| 文件名 | 尺寸 | 用途 | 动画帧数 | 循环 |
|--------|------|------|----------|------|
| fx_thruster_flame.svg | 128x128 | 推进器尾焰 | 持续循环 | 是 |
| fx_bubble_trail.svg | 128x128 | 水下气泡轨迹 | 持续循环 | 是 |
| fx_depth_pressure.svg | 128x128 | 深度压力视觉 | 持续循环 | 是 |

### 四、击杀/掉落特效

| 文件名 | 尺寸 | 用途 | 动画帧数 | 循环 |
|--------|------|------|----------|------|
| fx_explosion_small.svg | 128x128 | 小型爆炸 | 6-8帧 | 否 |
| fx_enemy_death_explosion.svg | 128x128 | 敌人死亡爆炸 | 8-10帧 | 否 |
| fx_item_drop_glow.svg | 64x64 | 资源掉落光效 | 持续循环 | 是 |
| fx_item_pickup.svg | 128x128 | 拾取反馈 | 6-8帧 | 否 |

---

## 使用指南

### 武器特效

#### 1. 钻头攻击 (Drill Attack)

**触发时机**: 玩家使用钻头武器攻击时

**特效组合**:
- 主特效: fx_drill_trail (旋转轨迹)
- 辅助特效: fx_drill_spark (接触火花)

**实现代码示例**:
```csharp
public void PlayDrillEffect()
{
    // 播放轨迹特效
    var trail = Instantiate(drillTrailPrefab, transform.position, Quaternion.identity);
    trail.transform.SetParent(transform);
    
    // 当击中敌人时播放火花
    if (hitEnemy)
    {
        Instantiate(drillSparkPrefab, hitPoint, Quaternion.identity);
    }
}
```

#### 2. 激光射击 (Laser Shot)

**触发时机**: 玩家使用激光武器时

**特效组合**:
- 发射特效: fx_laser_beam (激光束)
- 命中特效: fx_laser_explosion (命中爆炸)

**实现代码示例**:
```csharp
public void FireLaser(Vector3 target)
{
    // 创建激光束
    var beam = Instantiate(laserBeamPrefab, firePoint.position, 
        Quaternion.LookRotation(target - firePoint.position));
    
    // 射线检测命中
    if (Physics.Raycast(firePoint.position, direction, out hit))
    {
        Instantiate(laserExplosionPrefab, hit.point, Quaternion.identity);
    }
}
```

#### 3. 机械爪攻击 (Claw Attack)

**触发时机**: 玩家使用机械爪近战攻击时

**特效**: fx_claw_tear

**实现代码示例**:
```csharp
public void ClawAttack()
{
    // 在攻击方向生成撕裂特效
    var tear = Instantiate(clawTearPrefab, attackPoint.position, 
        Quaternion.Euler(0, attackAngle, 0));
}
```

### 敌人特效

#### 1. 机械鱼受击 (Mech Fish Hit)

**触发时机**: 机械鱼受到任何伤害

**特效**: fx_enemy_fish_hit

**实现代码示例**:
```csharp
public void OnDamage(float damage)
{
    health -= damage;
    
    // 播放受击闪烁
    var flash = Instantiate(fishHitPrefab, transform.position, Quaternion.identity);
    flash.transform.SetParent(transform);
    
    // 闪烁3-5次
    StartCoroutine(FlashEffect(flash));
}
```

#### 2. 机械蟹护盾 (Mech Crab Shield)

**触发时机**: 机械蟹进入防御状态

**特效**: fx_enemy_crab_shield

**实现代码示例**:
```csharp
public void ActivateShield()
{
    isShieldActive = true;
    
    // 播放护盾特效
    shieldEffect = Instantiate(crabShieldPrefab, transform.position, Quaternion.identity);
    shieldEffect.transform.SetParent(transform);
    shieldEffect.transform.localScale = Vector3.one * shieldRadius;
}

public void DeactivateShield()
{
    isShieldActive = false;
    
    // 销毁护盾特效
    if (shieldEffect != null)
        Destroy(shieldEffect);
}
```

#### 3. 机械水母电击 (Mech Jellyfish Electric)

**触发时机**: 机械水母释放电击攻击前预警

**特效**: fx_enemy_jellyfish_electric

**实现代码示例**:
```csharp
public IEnumerator ElectricAttack()
{
    // 预警阶段
    var warning = Instantiate(jellyfishElectricPrefab, transform.position, Quaternion.identity);
    warning.transform.localScale = Vector3.one * attackRange;
    
    yield return new WaitForSeconds(warningDuration);
    
    // 释放电击
    Destroy(warning);
    DealElectricDamage();
}
```

### 环境特效

#### 1. 推进器尾焰 (Thruster Flame)

**触发时机**: 玩家移动时

**特效**: fx_thruster_flame

**实现代码示例**:
```csharp
public void Update()
{
    if (isMoving)
    {
        if (thrusterEffect == null)
        {
            thrusterEffect = Instantiate(thrusterFlamePrefab, thrusterPoint.position, 
                Quaternion.Euler(90, 0, 0));
            thrusterEffect.transform.SetParent(thrusterPoint);
        }
    }
    else
    {
        if (thrusterEffect != null)
        {
            Destroy(thrusterEffect);
            thrusterEffect = null;
        }
    }
}
```

#### 2. 气泡轨迹 (Bubble Trail)

**触发时机**: 玩家在水下移动时

**特效**: fx_bubble_trail

**实现代码示例**:
```csharp
public void Update()
{
    if (isUnderwater && isMoving)
    {
        // 间隔生成气泡
        if (Time.time > nextBubbleTime)
        {
            Instantiate(bubbleTrailPrefab, transform.position, Quaternion.identity);
            nextBubbleTime = Time.time + bubbleInterval;
        }
    }
}
```

#### 3. 深度压力 (Depth Pressure)

**触发时机**: 玩家进入深海区域

**特效**: fx_depth_pressure

**实现代码示例**:
```csharp
public void OnEnterDeepSea()
{
    // 启用深度压力视觉效果
    depthPressureEffect = Instantiate(depthPressurePrefab);
    depthPressureEffect.transform.SetParent(Camera.main.transform);
    depthPressureEffect.transform.localPosition = Vector3.forward * 10;
}

public void OnExitDeepSea()
{
    if (depthPressureEffect != null)
        Destroy(depthPressureEffect);
}
```

### 击杀/掉落特效

#### 1. 敌人死亡 (Enemy Death)

**触发时机**: 敌人生命值归零

**特效**: fx_enemy_death_explosion

**实现代码示例**:
```csharp
public void Die()
{
    // 播放死亡爆炸
    Instantiate(deathExplosionPrefab, transform.position, Quaternion.identity);
    
    // 掉落资源
    DropItems();
    
    Destroy(gameObject);
}
```

#### 2. 资源掉落 (Item Drop)

**触发时机**: 敌人死亡或破坏可破坏物

**特效**: fx_item_drop_glow

**实现代码示例**:
```csharp
public void DropItem(ItemData item)
{
    var drop = Instantiate(itemPrefab, dropPosition, Quaternion.identity);
    
    // 添加光效
    var glow = Instantiate(itemDropGlowPrefab, drop.transform);
    glow.transform.localPosition = Vector3.zero;
}
```

#### 3. 拾取反馈 (Item Pickup)

**触发时机**: 玩家拾取资源

**特效**: fx_item_pickup

**实现代码示例**:
```csharp
public void OnPickup(Item item)
{
    // 添加到背包
    inventory.Add(item);
    
    // 播放拾取特效
    Instantiate(itemPickupPrefab, transform.position, Quaternion.identity);
    
    // 播放音效
    AudioManager.Play("Pickup");
}
```

---

## 美术规范

### 颜色规范

| 类型 | 主色 | 辅助色 | 强调色 |
|------|------|--------|--------|
| 火焰/爆炸 | #FF4400 | #FF8800 | #FFFF00 |
| 能量/激光 | #FF00FF | #FF00A0 | #FFAAFF |
| 护盾/科技 | #00F0FF | #0080FF | #8B5CF6 |
| 金币/奖励 | #FFFF00 | #FFAA00 | #FFFFFF |

### 尺寸规范

- 小型特效: 64x64 (拾取光效等)
- 中型特效: 128x128 (爆炸、火花等)
- 大型特效: 256x256 (范围指示、激光束等)

### 动画规范

- 快速反馈: 4-6帧 (击中、火花)
- 标准特效: 6-8帧 (爆炸、拾取)
- 持续循环: 无缝循环 (护盾、尾焰)

---

## V2版本特效

V2版本新增了12个高质量特效，详见 [V2/README.md](./V2/README.md)

### V2新增武器特效
- fx_plasma_cannon_charge.svg - 等离子炮充能
- fx_emp_pulse.svg - EMP电磁脉冲扩散
- fx_chainsaw_spin.svg - 链锯锯齿旋转
- fx_laser_charge.svg - 激光炮蓄力

### V2新增敌人特效
- fx_boss_skill_warning.svg - Boss技能预警
- fx_enemy_rage_outline.svg - 敌人狂暴状态
- fx_enemy_death_dissolve.svg - 敌人死亡溶解
- fx_hit_freeze_frame.svg - 受击停顿帧

### V2新增环境特效
- fx_depth_pressure_v2.svg - 深度压力视觉
- fx_volcano_vent.svg - 海底火山喷口
- fx_ancient_runes.svg - 古代遗迹发光符文
- fx_bio_luminescence.svg - 生物荧光粒子

---

## 文件结构

```
Assets/Art/Effects/
├── V1特效文件 (17个)
├── V2/                              # V2版本特效目录
│   ├── fx_plasma_cannon_charge.svg
│   ├── fx_emp_pulse.svg
│   ├── fx_chainsaw_spin.svg
│   ├── fx_laser_charge.svg
│   ├── fx_boss_skill_warning.svg
│   ├── fx_enemy_rage_outline.svg
│   ├── fx_enemy_death_dissolve.svg
│   ├── fx_hit_freeze_frame.svg
│   ├── fx_depth_pressure_v2.svg
│   ├── fx_volcano_vent.svg
│   ├── fx_ancient_runes.svg
│   ├── fx_bio_luminescence.svg
│   ├── UnityParticleConfig.md       # V2 Unity粒子配置
│   ├── PerformanceOptimization.md   # 性能优化指南
│   └── README.md                    # V2文档
├── UnityParticleConfig.md           # V1 Unity粒子配置
└── README.md                        # 本文档
```

---

## 版本历史

- **v1.0** (2026-02-27) - 初始版本，包含17个特效文件
  - 武器特效: 6个
  - 敌人特效: 4个
  - 环境特效: 3个
  - 击杀/掉落特效: 4个

- **v2.0** (2026-02-27) - V2版本，新增12个特效
  - 武器特效: 4个 (等离子炮、EMP、链锯、激光蓄力)
  - 敌人特效: 4个 (Boss预警、狂暴、死亡溶解、停顿帧)
  - 环境特效: 4个 (深度压力、火山喷口、古代符文、生物荧光)
  - 完整性能优化方案 (LOD、对象池)

---

## 联系

如有问题请联系特效美术团队。