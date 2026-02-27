# 赛博机甲 SebeJJ - 敌人美术资源需求文档

## 1. 机械鲨鱼 (MechShark)

### 1.1 角色概述
机械鲨鱼是深海中的高速猎手，结合了生物流线型外形与机械装甲。设计灵感来源于大白鲨与赛博朋克机械美学的融合。

### 1.2 3D模型规格

#### 基础模型
| 项目 | 规格 |
|------|------|
| 多边形数 | 8,000-12,000 tris |
| 贴图分辨率 | 2048×2048 (Albedo, Normal, Metallic, Emission) |
| 骨骼数量 | 25-30根 |
| 模型长度 | 约6米 (游戏内比例) |

#### 骨骼结构
```
Root
├── Body_Main (主体)
│   ├── Body_Section1 (身体中段)
│   │   └── Body_Section2 (身体后段)
│   │       └── Tail_Root (尾巴根部)
│   │           ├── Tail_Mid (尾巴中段)
│   │           │   └── Tail_Tip (尾巴尖端)
│   │           └── Tail_Fin_Upper (上尾鳍)
│   │           └── Tail_Fin_Lower (下尾鳍)
│   ├── Head_Root (头部)
│   │   ├── Jaw_Upper (上颚)
│   │   ├── Jaw_Lower (下颚)
│   │   ├── Eye_Left (左眼)
│   │   ├── Eye_Right (右眼)
│   │   └── Gills (鳃 - 6片)
│   ├── Fin_Dorsal (背鳍)
│   ├── Fin_Pectoral_L (左胸鳍)
│   └── Fin_Pectoral_R (右胸鳍)
└── Engine_Jet (引擎喷射口)
```

### 1.3 材质需求

#### 主材质: 机械鲨鱼外壳
```
Shader: PBR Metallic
Properties:
- Albedo: 深灰色金属底色，带有磨损划痕
- Metallic: 0.9 (高金属度)
- Smoothness: 0.7 (较光滑)
- Normal: 细微的金属纹理
- Emission: 红色能量纹路 (眼睛、鳃、引擎)
```

#### 次级材质: 生物组织
```
Shader: PBR Specular
Properties:
- Albedo: 暗蓝色/黑色渐变
- Specular: 湿润的高光效果
- Normal: 皮肤纹理
```

### 1.4 动画列表

| 动画名称 | 时长 | 描述 | 循环 |
|----------|------|------|------|
| Idle_Swim | 4s | 缓慢游动，尾巴自然摆动 | 是 |
| Swim_Fast | 2s | 快速游动，尾巴快速摆动 | 是 |
| Alert | 2s | 警觉状态，停止游动，眼睛发光增强 | 否 |
| Charge_Windup | 0.8s | 冲撞蓄力，身体后仰 | 否 |
| Charge_Loop | 1s | 冲撞中，身体僵硬直线前进 | 是 |
| Charge_Recover | 1.5s | 冲撞后减速，摇晃恢复 | 否 |
| Bite_Attack | 1s | 撕咬攻击，嘴巴张开闭合 | 否 |
| Take_Damage | 0.5s | 受击，身体抖动 | 否 |
| Death | 3s | 死亡，身体下沉，眼睛熄灭 | 否 |

### 1.5 特效需求

#### 尾流特效 (Swim_Trail)
- **触发**: 游动时
- **效果**: 白色气泡轨迹
- **参数**: 跟随尾巴，持续2秒消散

#### 引擎喷射 (Engine_Jet)
- **触发**: 快速游动/冲撞时
- **效果**: 红色能量喷射
- **位置**: 身体两侧鳃后方

#### 眼睛发光 (Eye_Glow)
- **正常**: 暗红色呼吸灯效果
- **警觉**: 亮红色常亮
- **攻击**: 闪烁红光

#### 冲撞特效 (Charge_Effect)
- **触发**: 冲撞状态
- **效果**: 
  - 前方锥形冲击波
  - 身体周围红色能量场
  - 速度线效果

#### 受击特效 (Hit_Effect)
- **效果**: 金属火花 + 蓝色电火花
- **位置**: 受击点

#### 死亡特效 (Death_Effect)
- **效果**:
  - 身体零件散落
  - 短路电火花
  - 红色能量泄漏
  - 缓慢下沉

### 1.6 音效需求

| 音效 | 描述 | 时长 |
|------|------|------|
| Engine_Idle | 引擎怠速声 | 循环 |
| Engine_Accelerate | 引擎加速 | 2s |
| Charge_Windup | 冲撞蓄力，机械压缩声 | 0.8s |
| Charge_Impact | 冲撞命中，金属撞击声 | 1s |
| Alert_Roar | 警觉咆哮，机械合成音 | 1.5s |
| Damage_Metal | 金属受损声 | 0.5s |
| Death_Explosion | 死亡爆炸，短路声 | 3s |

---

## 2. 深海章鱼 (DeepOctopus)

### 2.1 角色概述
深海章鱼是神秘的深海生物，拥有半透明的生物组织与发光的神经网络。设计强调"深海恐怖"与"生物发光"的美学。

### 2.2 3D模型规格

#### 基础模型
| 项目 | 规格 |
|------|------|
| 多边形数 | 10,000-15,000 tris |
| 贴图分辨率 | 2048×2048 (Albedo, Normal, Emission, Subsurface) |
| 骨骼数量 | 40-50根 |
| 模型直径 | 约4米 (触手展开) |

#### 骨骼结构
```
Root
├── Body_Mantle (外套膜/头部)
│   ├── Eye_Left (左眼)
│   ├── Eye_Right (右眼)
│   ├── Beak_Upper (上喙)
│   ├── Beak_Lower (下喙)
│   └── Siphon (漏斗/喷射口)
├── Tentacle_1 (触手1)
│   ├── T1_Segment1 (段1)
│   ├── T1_Segment2 (段2)
│   ├── T1_Segment3 (段3)
│   └── T1_Tip (尖端)
├── Tentacle_2 (触手2) ...
├── Tentacle_3 (触手3) ...
├── Tentacle_4 (触手4) ...
├── Tentacle_5 (触手5) ...
├── Tentacle_6 (触手6) ...
├── Tentacle_7 (触手7) ...
└── Tentacle_8 (触手8) ...
```

### 2.3 材质需求

#### 主材质: 生物组织
```
Shader: PBR Subsurface Scattering
Properties:
- Albedo: 深紫/黑色，半透明
- Subsurface Color: 蓝紫色发光
- Subsurface Radius: 0.5
- Normal: 皮肤褶皱纹理
- Emission: 神经网络发光纹路
```

#### 次级材质: 眼睛
```
Shader: PBR Emission
Properties:
- Albedo: 黑色
- Emission: 黄色/绿色发光瞳孔
```

#### 触手吸盘材质
```
Shader: PBR
Properties:
- Albedo: 深红色
- Smoothness: 0.9 (湿润)
- Normal: 吸盘细节
```

### 2.4 动画列表

| 动画名称 | 时长 | 描述 | 循环 |
|----------|------|------|------|
| Idle_Float | 6s | 缓慢漂浮，触手自然飘动 | 是 |
| Tentacle_Windup | 0.5s | 触手收缩蓄力 | 否 |
| Tentacle_Attack | 1.5s | 触手向外扫击 | 否 |
| Tentacle_Reset | 0.5s | 触手复位 | 否 |
| Ink_Spray | 1s | 墨汁喷射，身体收缩后释放 | 否 |
| Ink_Flee | 循环 | 墨汁后逃跑，触手向后 | 是 |
| Take_Damage | 0.5s | 受击，触手收缩 | 否 |
| Death | 4s | 死亡，触手无力的下垂 | 否 |

### 2.5 特效需求

#### 生物发光 (Bioluminescence)
- **触发**: 始终
- **效果**: 
  - 身体表面神经网络发光
  - 呼吸灯效果 (4秒周期)
  - 颜色: 蓝紫渐变

#### 触手攻击预警 (Tentacle_Warning)
- **触发**: 攻击蓄力
- **效果**: 
  - 触手发光增强
  - 身体微微膨胀

#### 墨汁喷射 (Ink_Spray_Effect)
- **触发**: 墨汁技能
- **效果**:
  - 黑色墨汁粒子喷射
  - 扩散的墨汁云
  - 持续时间: 5秒
  - 范围: 10米球形

#### 墨汁云 (Ink_Cloud)
- **材质**: 体积雾/粒子
- **颜色**: 深黑带微弱紫色
- **动态**: 缓慢扩散后消散
- **交互**: 玩家进入后屏幕变黑

#### 受击特效 (Hit_Effect)
- **效果**: 生物组织破损 + 蓝色血液
- **位置**: 受击点

#### 死亡特效 (Death_Effect)
- **效果**:
  - 最后一次墨汁喷射
  - 生物发光逐渐熄灭
  - 触手无力的飘散
  - 缓慢下沉

### 2.6 音效需求

| 音效 | 描述 | 时长 |
|------|------|------|
| Ambient_Drone | 深海环境音 | 循环 |
| Tentacle_Whip | 触手挥动，湿润的鞭打声 | 1s |
| Ink_Spray | 墨汁喷射，液体喷射声 | 1.5s |
| Biolum_Pulse | 生物发光脉动 | 循环 |
| Damage_Wet | 生物组织受损 | 0.5s |
| Death_Gurgle | 死亡咕噜声 | 3s |

---

## 3. 通用资源

### 3.1 伤害数字/指示器
- 机械风格字体
- 红色(伤害)/黄色(暴击)/白色(普通)
- 向上飘动动画

### 3.2 血条UI
- 机械鲨鱼: 红色金属边框
- 深海章鱼: 生物组织纹理边框
- 分段显示，低血量闪烁

### 3.3 锁定指示器
- 准星样式
- 锁定后变红
- 距离显示

---

## 4. 技术规格汇总

### 4.1 性能预算

| 资源类型 | 预算 |
|----------|------|
| 同屏敌人数量 | 最多5个 |
| 单个敌人Draw Call | < 5 |
| 特效粒子数 | < 500/敌人 |
| 骨骼动画 | GPU Skinning |

### 4.2 LOD设置

#### 机械鲨鱼 LOD
| LOD级别 | 距离 | 多边形数 | 细节 |
|---------|------|----------|------|
| LOD0 | 0-20m | 10,000 | 完整细节 |
| LOD1 | 20-50m | 5,000 | 简化鳍 |
| LOD2 | 50-100m | 2,000 | 无骨骼动画 |
| Culled | >100m | - | 不渲染 |

#### 深海章鱼 LOD
| LOD级别 | 距离 | 多边形数 | 细节 |
|---------|------|----------|------|
| LOD0 | 0-20m | 12,000 | 完整触手 |
| LOD1 | 20-50m | 6,000 | 简化触手 |
| LOD2 | 50-100m | 2,500 | 静态模型 |
| Culled | >100m | - | 不渲染 |

### 4.3 文件命名规范

```
Models/
├── MechShark/
│   ├── MSK_Model.fbx
│   ├── MSK_Albedo.png
│   ├── MSK_Normal.png
│   ├── MSK_Metallic.png
│   ├── MSK_Emission.png
│   └── MSK_Animations.fbx
│
└── DeepOctopus/
    ├── DOP_Model.fbx
    ├── DOP_Albedo.png
    ├── DOP_Normal.png
    ├── DOP_Subsurface.png
    ├── DOP_Emission.png
    └── DOP_Animations.fbx

Effects/
├── MSK_Trail.prefab
├── MSK_ChargeEffect.prefab
├── MSK_HitEffect.prefab
├── DOP_Bioluminescence.prefab
├── DOP_InkCloud.prefab
└── DOP_HitEffect.prefab

Audio/
├── MSK_Engine_Loop.wav
├── MSK_Charge.wav
├── DOP_Ambient.wav
└── DOP_Tentacle.wav
```

---

## 5. 参考图集

### 5.1 机械鲨鱼参考
- 大白鲨流线型身体
- 赛博朋克机械装甲 (参考: 攻壳机动队)
- 红色LED能量纹路
- 喷气引擎推进器

### 5.2 深海章鱼参考
- 大王乌贼触手结构
- 生物发光水母
- 半透明深海生物
- 神经网络可视化

---

*文档版本: 1.0*
*创建日期: 2026-02-27*
*作者: 敌人设计师*
