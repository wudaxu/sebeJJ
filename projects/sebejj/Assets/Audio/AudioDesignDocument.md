# SebeJJ 音效系统需求文档

## 项目概述
- **项目名称**: SebeJJ (赛博机甲)
- **文档版本**: v1.0
- **创建日期**: 2026-02-27
- **音效设计师**: Audio Design Team

---

## 1. 背景音乐 (BGM)

### 1.1 音乐分类

| 音乐名称 | 场景 | 时长 | BPM | 情绪描述 |
|---------|------|------|-----|---------|
| `BGM_Base_Ambient` | 基地主题 | 2:30-3:00 | 80-90 | 舒缓电子，科技感，放松 |
| `BGM_Shallow_Explore` | 浅海探索 | 3:00-4:00 | 70-85 | 神秘氛围，空灵，探索感 |
| `BGM_Deep_Combat` | 深海战斗 | 2:00-2:30 | 120-140 | 紧张低音，压迫感 |
| `BGM_Boss_Battle` | Boss战 | 3:00-4:00 | 150-170 | 激烈战斗，史诗感 |

### 1.2 音乐技术规格

```yaml
# 音频格式
Format: WAV (无损)
SampleRate: 48000 Hz
BitDepth: 24-bit
Channels: Stereo (2.0)

# 循环设置
LoopStart: 0:00:00
LoopEnd: 文件末尾
Crossfade: 2秒 (无缝循环)

# 动态音乐系统
Layers: 3层动态混音
  - Layer1: 基础节奏层
  - Layer2: 旋律层
  - Layer3: 紧张层
TransitionTime: 2-4秒
```

### 1.3 动态音乐切换配置

```csharp
// Unity AudioMixer 快照切换
public enum MusicState {
    Base,           // 基地
    ShallowExplore, // 浅海探索
    DeepCombat,     // 深海战斗
    BossBattle      // Boss战
}

// 切换参数
SnapshotTransitionTime: 3.0f
DuckVolumeOnDialogue: -10dB
```

---

## 2. 音效库 (SFX)

### 2.1 机甲音效 (Mecha)

| 音效名称 | 文件名 | 时长 | 音量 | 描述 |
|---------|--------|------|------|------|
| 机甲移动 | `SFX_Mecha_Move_Loop.wav` | 循环 | -12dB | 液压机械移动声 |
| 推进器启动 | `SFX_Mecha_Thruster_Start.wav` | 0.8s | -8dB | 喷射启动 |
| 推进器循环 | `SFX_Mecha_Thruster_Loop.wav` | 循环 | -10dB | 持续喷射 |
| 推进器停止 | `SFX_Mecha_Thruster_Stop.wav` | 0.5s | -10dB | 喷射停止 |
| 机甲受伤轻 | `SFX_Mecha_Hit_Light.wav` | 0.3s | -6dB | 轻微撞击 |
| 机甲受伤重 | `SFX_Mecha_Hit_Heavy.wav` | 0.5s | -4dB | 严重损坏 |
| 机甲护盾 | `SFX_Mecha_Shield.wav` | 0.6s | -8dB | 能量护盾激活 |
| 机甲变形 | `SFX_Mecha_Transform.wav` | 2.0s | -5dB | 形态转换 |

### 2.2 武器音效 (Weapon)

| 音效名称 | 文件名 | 时长 | 音量 | 描述 |
|---------|--------|------|------|------|
| 主武器开火 | `SFX_Weapon_Primary_Fire.wav` | 0.2s | -5dB | 标准射击 |
| 主武器命中 | `SFX_Weapon_Primary_Hit.wav` | 0.15s | -8dB | 命中金属 |
| 副武器开火 | `SFX_Weapon_Secondary_Fire.wav` | 0.4s | -4dB | 重型武器 |
| 副武器命中 | `SFX_Weapon_Secondary_Hit.wav` | 0.3s | -6dB | 爆炸命中 |
| 导弹发射 | `SFX_Weapon_Missile_Launch.wav` | 0.6s | -3dB | 导弹发射 |
| 导弹爆炸 | `SFX_Weapon_Missile_Explode.wav` | 1.0s | -2dB | 爆炸效果 |
| 换弹开始 | `SFX_Weapon_Reload_Start.wav` | 0.5s | -10dB | 弹匣卸下 |
| 换弹完成 | `SFX_Weapon_Reload_Complete.wav` | 0.4s | -10dB | 弹匣装上 |
| 武器过热 | `SFX_Weapon_Overheat.wav` | 1.5s | -6dB | 过热警告 |
| 能量充能 | `SFX_Weapon_Charge.wav` | 2.0s | -8dB | 蓄力攻击 |

### 2.3 敌人音效 (Enemy)

| 音效名称 | 文件名 | 时长 | 音量 | 描述 |
|---------|--------|------|------|------|
| 敌人攻击 | `SFX_Enemy_Attack_01.wav` | 0.4s | -8dB | 近战攻击 |
| 敌人远程 | `SFX_Enemy_Attack_Range.wav` | 0.3s | -8dB | 远程攻击 |
| 敌人受击轻 | `SFX_Enemy_Hit_Light.wav` | 0.2s | -10dB | 轻微伤害 |
| 敌人受击重 | `SFX_Enemy_Hit_Heavy.wav` | 0.4s | -8dB | 严重伤害 |
| 敌人死亡小 | `SFX_Enemy_Death_Small.wav` | 0.6s | -6dB | 小型敌人死亡 |
| 敌人死亡大 | `SFX_Enemy_Death_Large.wav` | 1.2s | -4dB | 大型敌人死亡 |
| 敌人发现 | `SFX_Enemy_Alert.wav` | 0.8s | -10dB | 发现玩家 |
| 敌人巡逻 | `SFX_Enemy_Patrol_Loop.wav` | 循环 | -18dB | 巡逻环境音 |

### 2.4 环境音效 (Environment)

| 音效名称 | 文件名 | 时长 | 音量 | 描述 |
|---------|--------|------|------|------|
| 气泡上升 | `SFX_Env_Bubbles_Up.wav` | 循环 | -20dB | 持续气泡 |
| 气泡爆裂 | `SFX_Env_Bubble_Pop.wav` | 0.2s | -15dB | 单个气泡 |
| 水压变化 | `SFX_Env_Pressure_Change.wav` | 2.0s | -12dB | 深度变化 |
| 警报声 | `SFX_Env_Alarm.wav` | 循环 | -8dB | 紧急警报 |
| 水流声 | `SFX_Env_Water_Current.wav` | 循环 | -22dB | 海底洋流 |
| 金属摩擦 | `SFX_Env_Metal_Scrape.wav` | 1.0s | -14dB | 结构摩擦 |
| 声纳脉冲 | `SFX_Env_Sonar_Ping.wav` | 0.5s | -16dB | 探测声纳 |
| 深海生物 | `SFX_Env_Creature_Distant.wav` | 循环 | -25dB | 远处生物 |

### 2.5 UI音效 (Interface)

| 音效名称 | 文件名 | 时长 | 音量 | 描述 |
|---------|--------|------|------|------|
| 按钮点击 | `SFX_UI_Click.wav` | 0.1s | -10dB | 标准点击 |
| 按钮悬停 | `SFX_UI_Hover.wav` | 0.05s | -18dB | 悬停反馈 |
| 确认成功 | `SFX_UI_Confirm.wav` | 0.3s | -8dB | 操作确认 |
| 取消/返回 | `SFX_UI_Cancel.wav` | 0.2s | -10dB | 取消操作 |
| 警告提示 | `SFX_UI_Warning.wav` | 0.5s | -6dB | 警告信息 |
| 错误提示 | `SFX_UI_Error.wav` | 0.4s | -6dB | 错误反馈 |
| 菜单打开 | `SFX_UI_Menu_Open.wav` | 0.4s | -12dB | 界面展开 |
| 菜单关闭 | `SFX_UI_Menu_Close.wav` | 0.3s | -12dB | 界面收起 |
| 升级解锁 | `SFX_UI_Unlock.wav` | 1.0s | -5dB | 解锁成就 |
| 任务完成 | `SFX_UI_Mission_Complete.wav` | 2.0s | -4dB | 任务完成 |

---

## 3. 3D音效配置

### 3.1 距离衰减曲线

```yaml
# 3D音效衰减设置
MinDistance: 5.0    # 最小距离 (音量最大)
MaxDistance: 100.0  # 最大距离 (音量最小)
RolloffMode: Custom # 自定义衰减曲线

# 衰减曲线点
RolloffCurve:
  - { distance: 0,   volume: 1.0 }   # 0m  = 0dB
  - { distance: 5,   volume: 1.0 }   # 5m  = 0dB
  - { distance: 25,  volume: 0.7 }   # 25m = -3dB
  - { distance: 50,  volume: 0.4 }   # 50m = -8dB
  - { distance: 100, volume: 0.0 }   # 100m= -∞dB

# 不同音效类型的衰减
MechaSounds:
  MinDistance: 3
  MaxDistance: 80
  
WeaponSounds:
  MinDistance: 2
  MaxDistance: 150
  
EnemySounds:
  MinDistance: 4
  MaxDistance: 100
  
EnvironmentSounds:
  MinDistance: 10
  MaxDistance: 200
```

### 3.2 混响设置 (深海环境)

```yaml
# Unity AudioMixer Reverb Zone 配置
ReverbPresets:
  ShallowWater:
    Room: -1000
    RoomHF: -400
    RoomLF: 0
    DecayTime: 2.5
    DecayHFRatio: 0.5
    Reflections: -1000
    ReflectionsDelay: 0.1
    Reverb: -500
    ReverbDelay: 0.05
    HFReference: 5000
    LFReference: 250
    Diffusion: 80
    Density: 70
    
  DeepWater:
    Room: -1500
    RoomHF: -800
    RoomLF: -200
    DecayTime: 4.0
    DecayHFRatio: 0.3
    Reflections: -1500
    ReflectionsDelay: 0.2
    Reverb: -800
    ReverbDelay: 0.1
    HFReference: 3000
    LFReference: 150
    Diffusion: 60
    Density: 50
    
  BaseInterior:
    Room: -500
    RoomHF: -200
    RoomLF: 0
    DecayTime: 1.5
    DecayHFRatio: 0.8
    Reflections: -300
    ReflectionsDelay: 0.02
    Reverb: -200
    ReverbDelay: 0.03
    HFReference: 5000
    LFReference: 250
    Diffusion: 90
    Density: 85
```

### 3.3 动态音乐切换系统

```csharp
// MusicManager.cs 配置参考
public class MusicManager : MonoBehaviour {
    [Header("Audio Mixer")]
    public AudioMixer audioMixer;
    
    [Header("Music Snapshots")]
    public AudioMixerSnapshot baseSnapshot;
    public AudioMixerSnapshot exploreSnapshot;
    public AudioMixerSnapshot combatSnapshot;
    public AudioMixerSnapshot bossSnapshot;
    
    [Header("Transition Settings")]
    public float transitionTime = 3.0f;
    public float combatFadeOutDelay = 5.0f; // 战斗结束后延迟淡出
    
    // 动态层控制
    public void SetIntensity(float intensity) {
        // intensity: 0-1
        // 控制音乐层混合
        audioMixer.SetFloat("Layer1_Volume", Mathf.Lerp(-10, 0, intensity));
        audioMixer.SetFloat("Layer2_Volume", Mathf.Lerp(-20, -5, intensity));
        audioMixer.SetFloat("Layer3_Volume", Mathf.Lerp(-80, 0, intensity));
    }
}
```

---

## 4. Unity AudioMixer 配置

### 4.1 混音器结构

```
Master
├── BGM (背景音乐)
│   ├── Base_Music
│   ├── Explore_Music
│   ├── Combat_Music
│   └── Boss_Music
├── SFX (音效)
│   ├── Mecha (机甲)
│   ├── Weapon (武器)
│   ├── Enemy (敌人)
│   └── Environment (环境)
└── UI (界面)
    ├── UI_Click
    ├── UI_Alert
    └── UI_Feedback
```

### 4.2 AudioMixer 参数设置

```yaml
# Master Group
Master:
  Volume: 0dB
  Attenuation: 1.0

# BGM Group
BGM:
  Volume: -5dB
  DuckVolume: -15dB  # 对话时降低
  Attenuation: 0.56
  
# SFX Group
SFX:
  Volume: 0dB
  Attenuation: 1.0
  
  Mecha:
    Volume: -2dB
    Attenuation: 0.79
    
  Weapon:
    Volume: 0dB
    Attenuation: 1.0
    
  Enemy:
    Volume: -3dB
    Attenuation: 0.71
    
  Environment:
    Volume: -10dB
    Attenuation: 0.32

# UI Group
UI:
  Volume: -5dB
  Attenuation: 0.56
```

### 4.3 效果器链

```yaml
# BGM Group Effects
BGM_Effects:
  - Compressor:
      Threshold: -10dB
      Ratio: 3:1
      Attack: 10ms
      Release: 100ms
  - LowPass:
      Cutoff: 20000Hz
      
# SFX Group Effects
SFX_Effects:
  - Compressor:
      Threshold: -5dB
      Ratio: 4:1
      Attack: 5ms
      Release: 50ms
  - SFX_Reverb:
      Send: 20%
      
# Environment Effects
Environment_Effects:
  - Reverb:
      Preset: DeepWater
      WetLevel: -12dB
      DryLevel: 0dB
  - LowPass:
      Cutoff: 8000Hz  # 水下高频衰减
```

---

## 5. 音量平衡建议

### 5.1 参考音量表

| 类别 | 峰值电平 | RMS电平 | 动态范围 |
|-----|---------|--------|---------|
| BGM | -6dB | -18dB | 12dB |
| SFX (武器) | -3dB | -12dB | 9dB |
| SFX (机甲) | -6dB | -15dB | 9dB |
| SFX (敌人) | -6dB | -16dB | 10dB |
| SFX (环境) | -20dB | -30dB | 10dB |
| UI | -8dB | -18dB | 10dB |

### 5.2 混音优先级

```
优先级 (高 -> 低):
1. UI警告/警报 (必须清晰可闻)
2. 武器开火 (玩家反馈)
3. 机甲受伤 (生存反馈)
4. 敌人攻击 (威胁提示)
5. BGM (氛围支撑)
6. 环境音 (沉浸感)
```

### 5.3 平台音量建议

```yaml
# PC/主机
MasterVolume: 0dB
BGMVolume: -5dB
SFXVolume: 0dB
UIVolume: -3dB

# 移动端 (建议降低整体音量)
MasterVolume: -3dB
BGMVolume: -8dB
SFXVolume: -3dB
UIVolume: -5dB
```

---

## 6. 音效文件清单

### 6.1 完整文件列表

```
Assets/Audio/
├── BGM/
│   ├── BGM_Base_Ambient.wav
│   ├── BGM_Shallow_Explore.wav
│   ├── BGM_Deep_Combat.wav
│   └── BGM_Boss_Battle.wav
├── SFX/
│   ├── Mecha/
│   │   ├── SFX_Mecha_Move_Loop.wav
│   │   ├── SFX_Mecha_Thruster_Start.wav
│   │   ├── SFX_Mecha_Thruster_Loop.wav
│   │   ├── SFX_Mecha_Thruster_Stop.wav
│   │   ├── SFX_Mecha_Hit_Light.wav
│   │   ├── SFX_Mecha_Hit_Heavy.wav
│   │   ├── SFX_Mecha_Shield.wav
│   │   └── SFX_Mecha_Transform.wav
│   ├── Weapon/
│   │   ├── SFX_Weapon_Primary_Fire.wav
│   │   ├── SFX_Weapon_Primary_Hit.wav
│   │   ├── SFX_Weapon_Secondary_Fire.wav
│   │   ├── SFX_Weapon_Secondary_Hit.wav
│   │   ├── SFX_Weapon_Missile_Launch.wav
│   │   ├── SFX_Weapon_Missile_Explode.wav
│   │   ├── SFX_Weapon_Reload_Start.wav
│   │   ├── SFX_Weapon_Reload_Complete.wav
│   │   ├── SFX_Weapon_Overheat.wav
│   │   └── SFX_Weapon_Charge.wav
│   ├── Enemy/
│   │   ├── SFX_Enemy_Attack_01.wav
│   │   ├── SFX_Enemy_Attack_Range.wav
│   │   ├── SFX_Enemy_Hit_Light.wav
│   │   ├── SFX_Enemy_Hit_Heavy.wav
│   │   ├── SFX_Enemy_Death_Small.wav
│   │   ├── SFX_Enemy_Death_Large.wav
│   │   ├── SFX_Enemy_Alert.wav
│   │   └── SFX_Enemy_Patrol_Loop.wav
│   ├── Environment/
│   │   ├── SFX_Env_Bubbles_Up.wav
│   │   ├── SFX_Env_Bubble_Pop.wav
│   │   ├── SFX_Env_Pressure_Change.wav
│   │   ├── SFX_Env_Alarm.wav
│   │   ├── SFX_Env_Water_Current.wav
│   │   ├── SFX_Env_Metal_Scrape.wav
│   │   ├── SFX_Env_Sonar_Ping.wav
│   │   └── SFX_Env_Creature_Distant.wav
│   └── UI/
│       ├── SFX_UI_Click.wav
│       ├── SFX_UI_Hover.wav
│       ├── SFX_UI_Confirm.wav
│       ├── SFX_UI_Cancel.wav
│       ├── SFX_UI_Warning.wav
│       ├── SFX_UI_Error.wav
│       ├── SFX_UI_Menu_Open.wav
│       ├── SFX_UI_Menu_Close.wav
│       ├── SFX_UI_Unlock.wav
│       └── SFX_UI_Mission_Complete.wav
└── Mixers/
    └── MainAudioMixer.mixer
```

### 6.2 文件命名规范

```
格式: [类型]_[类别]_[名称]_[变体].[格式]

类型:
  - BGM: 背景音乐
  - SFX: 音效
  - VO: 语音

类别:
  - Mecha: 机甲
  - Weapon: 武器
  - Enemy: 敌人
  - Environment: 环境
  - UI: 界面

变体 (可选):
  - Loop: 循环音效
  - Start/Stop: 开始/结束
  - Light/Heavy: 轻/重
  - 01, 02, 03: 变体编号
```

---

## 7. 实现建议

### 7.1 音频管理器架构

```csharp
// 建议的代码结构
AudioManager (Singleton)
├── MusicManager
│   ├── PlayMusic(MusicType type)
│   ├── TransitionTo(MusicType type, float duration)
│   └── SetIntensity(float intensity)
├── SFXManager
│   ├── PlaySFX(SFXType type, Vector3 position)
│   ├── PlaySFXLoop(SFXType type)
│   └── StopSFXLoop(SFXType type)
└── UIAudioManager
    ├── PlayClick()
    ├── PlayConfirm()
    └── PlayWarning()
```

### 7.2 性能优化建议

1. **音频池化**: 频繁播放的音效使用对象池
2. **动态加载**: BGM使用Addressables动态加载
3. **距离剔除**: 超出MaxDistance的音效不播放
4. **并发限制**: 同类型音效限制同时播放数量
5. **压缩格式**: 
   - BGM: Vorbis (高音质)
   - SFX: ADPCM (低延迟)
   - 移动端: MP3 (兼容)

### 7.3 测试清单

- [ ] 所有音效文件正确导入
- [ ] AudioMixer组正确配置
- [ ] 3D音效衰减正常
- [ ] 混响效果符合环境
- [ ] 动态音乐切换平滑
- [ ] 音量平衡符合建议
- [ ] 循环音效无缝衔接
- [ ] 多平台音量一致

---

## 附录

### A. 参考资源

- Unity Audio Documentation: https://docs.unity3d.com/Manual/Audio.html
- Audio Mixer Tutorial: https://learn.unity.com/tutorial/audio-mixing
- 3D Audio Best Practices: https://docs.unity3d.com/Manual/AudioSpatializerSDK.html

### B. 音效制作工具推荐

- **DAW**: Reaper, Ableton Live, FL Studio
- **音效库**: Boom Library, Sound Ideas, Pro Sound Effects
- **合成器**: Serum, Massive, Vital
- **中间件**: FMOD, Wwise (如需更高级功能)

---

*文档结束*
