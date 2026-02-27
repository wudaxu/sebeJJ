/**
 * @file BossArtRequirements.md
 * @brief 铁钳巨兽美术资源需求文档
 * @description Boss战所需的所有美术资源清单
 * @author Boss战设计师
 * @date 2026-02-27
 */

# 铁钳巨兽 美术资源需求文档

## 1. 角色模型

### 1.1 Boss主体
| 资源名称 | 类型 | 规格 | 描述 |
|----------|------|------|------|
| IronClawBeast_Body | Sprite/Model | 512x512 | 主体躯干，机械螃蟹造型 |
| IronClawBeast_LeftClaw | Sprite/Model | 256x256 | 左钳子，较大 |
| IronClawBeast_RightClaw | Sprite/Model | 256x256 | 右钳子，较小 |
| IronClawBeast_Legs | Sprite/Model | 128x128 x6 | 六条机械腿 |
| IronClawBeast_Eye | Sprite/Model | 64x64 | 发光的红色眼睛 |
| IronClawBeast_Core | Sprite/Model | 128x128 | 背部核心（弱点）|

### 1.2 阶段变体
| 资源名称 | 类型 | 描述 |
|----------|------|------|
| IronClawBeast_Phase1 | Material/Shader | 第一阶段：蓝色能量纹理 |
| IronClawBeast_Phase2 | Material/Shader | 第二阶段：橙色能量纹理 |
| IronClawBeast_Phase3 | Material/Shader | 第三阶段：红色狂暴纹理 |

---

## 2. 动画资源

### 2.1 基础动画
| 动画名称 | 类型 | 帧数/时长 | 描述 |
|----------|------|-----------|------|
| Idle | Loop | 2s | 待机，轻微浮动 |
| Walk | Loop | 1s | 行走，腿部交替 |
| Turn | OneShot | 0.5s | 转向 |
| Hit | OneShot | 0.3s | 受击抖动 |

### 2.2 攻击动画
| 动画名称 | 类型 | 帧数/时长 | 描述 |
|----------|------|-----------|------|
| ClawAttack_1 | OneShot | 0.8s | 钳击第1击 |
| ClawAttack_2 | OneShot | 0.8s | 钳击第2击 |
| ClawAttack_3 | OneShot | 1.0s | 钳击第3击（重击）|
| Charge_Start | OneShot | 1.5s | 冲撞蓄力 |
| Charge_Execute | Loop | - | 冲撞执行（移动中）|
| Charge_End | OneShot | 0.5s | 冲撞结束/撞墙 |
| Charge_Stun | OneShot | 2s | 撞墙眩晕 |

### 2.3 技能动画
| 动画名称 | 类型 | 帧数/时长 | 描述 |
|----------|------|-----------|------|
| Defend_Start | OneShot | 0.5s | 防御姿态开始 |
| Defend_Loop | Loop | - | 防御姿态持续 |
| Defend_End | OneShot | 0.5s | 防御姿态结束 |
| Defend_Break | OneShot | 2s | 破防眩晕 |
| Laser_Start | OneShot | 1s | 激光充能 |
| Laser_Loop | Loop | 3s | 激光扫射 |
| Laser_End | OneShot | 0.5s | 激光结束 |
| Summon | OneShot | 2s | 召唤机械蟹 |
| Earthquake_Start | OneShot | 2s | 地震预警 |
| Earthquake_Loop | Loop | 4s | 地震持续 |
| Earthquake_End | OneShot | 1s | 地震结束 |
| WeakPoint_Expose | OneShot | 5s | 弱点暴露 |

### 2.4 阶段转换动画
| 动画名称 | 类型 | 帧数/时长 | 描述 |
|----------|------|-----------|------|
| PhaseTransition_1to2 | OneShot | 3s | 第一阶段→第二阶段 |
| PhaseTransition_2to3 | OneShot | 3s | 第二阶段→第三阶段 |
| Death | OneShot | 3s | 死亡动画 |

---

## 3. 特效资源

### 3.1 攻击特效
| 特效名称 | 类型 | 描述 |
|----------|------|------|
| VFX_ClawSwipe | Particle | 钳击挥砍轨迹 |
| VFX_ClawImpact | Particle | 钳击命中火花 |
| VFX_ChargeTrail | Particle | 冲撞尾迹 |
| VFX_ChargeImpact | Particle | 冲撞撞击效果 |

### 3.2 技能特效
| 特效名称 | 类型 | 描述 |
|----------|------|------|
| VFX_DefendShield | Particle | 防御能量盾 |
| VFX_DefendBreak | Particle | 破防特效 |
| VFX_LaserBeam | Particle/Line | 激光束 |
| VFX_LaserCharge | Particle | 激光充能 |
| VFX_SummonPortal | Particle | 召唤传送门 |
| VFX_EarthquakeWave | Particle | 地震波 |
| VFX_EarthquakeImpact | Particle | 地震地面裂痕 |
| VFX_WeakPointGlow | Particle | 弱点发光 |

### 3.3 阶段特效
| 特效名称 | 类型 | 描述 |
|----------|------|------|
| VFX_PhaseTransition | Particle | 阶段转换能量爆发 |
| VFX_RageAura | Particle | 狂暴红色光环 |
| VFX_EyeGlow_Phase1 | Particle | 第一阶段眼睛蓝光 |
| VFX_EyeGlow_Phase2 | Particle | 第二阶段眼睛橙光 |
| VFX_EyeGlow_Phase3 | Particle | 第三阶段眼睛红光 |

### 3.4 受击特效
| 特效名称 | 类型 | 描述 |
|----------|------|------|
| VFX_HitSpark | Particle | 受击火花 |
| VFX_HitMetal | Particle | 金属受击碎片 |
| VFX_WeakPointHit | Particle | 弱点受击特效 |

---

## 4. UI资源

### 4.1 血条UI
| 资源名称 | 类型 | 规格 | 描述 |
|----------|------|------|------|
| UI_BossHealthBar_Bg | Sprite | 512x64 | 血条背景 |
| UI_BossHealthBar_Fill | Sprite | 512x64 | 血条填充 |
| UI_BossHealthBar_Delayed | Sprite | 512x64 | 延迟血条 |
| UI_BossHealthBar_Frame | Sprite | 512x64 | 血条边框 |
| UI_BossHealthBar_Marker | Sprite | 16x64 | 阶段分隔标记 |

### 4.2 状态图标
| 资源名称 | 类型 | 规格 | 描述 |
|----------|------|------|------|
| UI_Icon_Defend | Sprite | 64x64 | 防御状态图标 |
| UI_Icon_Rage | Sprite | 64x64 | 狂暴状态图标 |
| UI_Icon_WeakPoint | Sprite | 64x64 | 弱点暴露图标 |
| UI_Icon_Invincible | Sprite | 64x64 | 无敌状态图标 |
| UI_Icon_Phase1 | Sprite | 64x64 | 第一阶段图标 |
| UI_Icon_Phase2 | Sprite | 64x64 | 第二阶段图标 |
| UI_Icon_Phase3 | Sprite | 64x64 | 第三阶段图标 |

### 4.3 预警UI
| 资源名称 | 类型 | 规格 | 描述 |
|----------|------|------|------|
| UI_Warning_Charge | Sprite | 256x32 | 冲撞预警线 |
| UI_Warning_Laser | Sprite | 128x128 | 激光预警扇形 |
| UI_Warning_Earthquake | Sprite | 128x128 | 地震预警图标 |
| UI_Warning_Danger | Sprite | 64x64 | 危险警告图标 |

---

## 5. 场景资源

### 5.1 场景物体
| 资源名称 | 类型 | 规格 | 描述 |
|----------|------|------|------|
| Arena_Rock_Large | Sprite/Model | 256x256 | 大型可破坏岩石 |
| Arena_Rock_Medium | Sprite/Model | 128x128 | 中型可破坏岩石 |
| Arena_Rock_Small | Sprite/Model | 64x64 | 小型可破坏岩石 |
| Arena_Rock_Debris | Sprite | 32x32 x5 | 岩石碎片 |
| Arena_Ground | Sprite/Texture | 1024x1024 | 地面纹理 |
| Arena_Wall | Sprite/Texture | 512x512 | 墙壁纹理 |

### 5.2 环境特效
| 资源名称 | 类型 | 描述 |
|----------|------|------|
| VFX_Ambient_Dust | Particle | 环境灰尘 |
| VFX_Ambient_Sparks | Particle | 环境电火花 |
| VFX_Abyss | Particle | 深渊特效 |

### 5.3 传送门
| 资源名称 | 类型 | 规格 | 描述 |
|----------|------|------|------|
| Portal_Frame | Sprite/Model | 256x256 | 传送门框架 |
| Portal_Vortex | Particle | - | 传送门漩涡 |
| Portal_Activate | Particle | - | 传送门激活 |

---

## 6. 音频资源

### 6.1 音效
| 音效名称 | 类型 | 时长 | 描述 |
|----------|------|------|------|
| SFX_ClawAttack | SFX | 0.5s | 钳击音效 |
| SFX_Charge_Start | SFX | 1.5s | 冲撞蓄力 |
| SFX_Charge_Impact | SFX | 0.5s | 冲撞撞击 |
| SFX_Defend_Start | SFX | 0.5s | 防御开始 |
| SFX_Defend_Break | SFX | 0.8s | 破防音效 |
| SFX_Laser_Charge | SFX | 1s | 激光充能 |
| SFX_Laser_Loop | SFX | Loop | 激光持续 |
| SFX_Summon | SFX | 2s | 召唤音效 |
| SFX_Earthquake_Warning | SFX | 2s | 地震预警 |
| SFX_Earthquake_Impact | SFX | 0.5s | 地震冲击 |
| SFX_WeakPoint_Expose | SFX | 1s | 弱点暴露 |
| SFX_Hit_Metal | SFX | 0.3s | 金属受击 |
| SFX_Hit_WeakPoint | SFX | 0.5s | 弱点受击 |

### 6.2 阶段音效
| 音效名称 | 类型 | 时长 | 描述 |
|----------|------|------|------|
| SFX_PhaseTransition | SFX | 3s | 阶段转换 |
| SFX_Rage_Start | SFX | 2s | 狂暴开始 |
| SFX_Boss_Death | SFX | 3s | Boss死亡 |
| SFX_Portal_Activate | SFX | 2s | 传送门激活 |

### 6.3 背景音乐
| 音乐名称 | 类型 | 时长 | 描述 |
|----------|------|------|------|
| BGM_Boss_Phase1 | Music | Loop | 第一阶段战斗音乐 |
| BGM_Boss_Phase2 | Music | Loop | 第二阶段战斗音乐 |
| BGM_Boss_Phase3 | Music | Loop | 第三阶段战斗音乐 |
| BGM_Boss_Victory | Music | 30s | 胜利音乐 |

---

## 7. 材质与着色器

### 7.1 Boss材质
| 材质名称 | 类型 | 描述 |
|----------|------|------|
| Mat_Boss_Metal | PBR | 金属主体材质 |
| Mat_Boss_Glow_Blue | Emissive | 蓝色发光部件 |
| Mat_Boss_Glow_Orange | Emissive | 橙色发光部件 |
| Mat_Boss_Glow_Red | Emissive | 红色发光部件 |
| Mat_Boss_Rust | PBR | 锈迹细节 |

### 7.2 特效着色器
| 着色器名称 | 类型 | 描述 |
|------------|------|------|
| Shader_Laser | VFX | 激光束着色器 |
| Shader_Shield | VFX | 能量盾着色器 |
| Shader_Distortion | VFX | 扭曲效果 |
| Shader_Glow | VFX | 发光效果 |

---

## 8. 资源优先级

### 8.1 必须资源（P0）
- Boss基础模型和贴图
- 基础动画（Idle, Walk, Attack, Death）
- 基础特效（攻击、受击）
- 血条UI
- 基础音效

### 8.2 重要资源（P1）
- 阶段变体材质
- 技能动画和特效
- 阶段转换特效
- 预警UI
- 背景音乐

### 8.3 可选资源（P2）
- 环境细节
- 额外特效
- 高级材质
- 额外音效变体

---

## 9. 技术规格

### 9.1 图片格式
- 贴图：PNG/TGA，带Alpha通道
- UI：PNG，9-slice支持
- 动画：Sprite Sheet或独立帧

### 9.2 模型规格
- 多边形数：Boss主体 < 5000三角面
- 贴图尺寸：最大1024x1024
- 骨骼：20-30根

### 9.3 音频规格
- 音效：44.1kHz, 16bit, Mono/Stereo
- 音乐：44.1kHz, 16bit, Stereo
- 格式：WAV/OGG

---

## 10. 参考风格

### 10.1 视觉参考
- 机械风格：赛博朋克 + 深海生物
- 颜色：金属灰 + 能量发光
- 质感：生锈金属 + 光滑装甲

### 10.2 动画参考
- 移动：螃蟹横向移动 + 机械感
- 攻击：沉重有力 + 工业感
- 受击：金属变形 + 火花

### 10.3 音效参考
- 机械音效：变形金刚电影
- 能量音效：科幻游戏风格
- 环境音效：深海压抑感
