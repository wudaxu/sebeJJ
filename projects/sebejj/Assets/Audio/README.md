# SebeJJ 音效系统快速参考

## 目录结构

```
Assets/Audio/
├── BGM/                          # 背景音乐
│   ├── BGM_Base_Ambient.wav      # 基地主题
│   ├── BGM_Shallow_Explore.wav   # 浅海探索
│   ├── BGM_Deep_Combat.wav       # 深海战斗
│   └── BGM_Boss_Battle.wav       # Boss战
├── SFX/                          # 音效
│   ├── Mecha/                    # 机甲音效
│   ├── Weapon/                   # 武器音效
│   ├── Enemy/                    # 敌人音效
│   ├── Environment/              # 环境音效
│   └── UI/                       # UI音效
├── Scripts/                      # 音频脚本
│   ├── AudioManager.cs           # 核心管理器
│   ├── MechaAudioController.cs   # 机甲音效
│   ├── WeaponAudioController.cs  # 武器音效
│   ├── EnvironmentAudioZone.cs   # 环境区域
│   └── UIAudioController.cs      # UI音效
├── MainAudioMixer.mixer          # Unity混音器
└── AudioDesignDocument.md        # 完整设计文档
```

## 快速使用

### 1. 播放背景音乐

```csharp
// 切换到指定场景音乐
AudioManager.Instance.TransitionToMusic(MusicType.DeepCombat);

// 设置音乐强度 (0-1)
AudioManager.Instance.SetMusicIntensity(0.8f);
```

### 2. 播放音效

```csharp
// 3D音效（带位置）
AudioManager.Instance.PlaySFX(SFXType.WeaponPrimaryFire, transform.position);

// 2D音效
AudioManager.Instance.PlaySFX2D(SFXType.EnvAlarm);

// UI音效
AudioManager.Instance.PlayUISFX(UISFXType.Click);
// 或使用快捷方式
UIAudio.PlayClick();
```

### 3. 机甲音效

```csharp
// 添加到机甲对象
MechaAudioController mechaAudio = GetComponent<MechaAudioController>();

// 移动
mechaAudio.SetMoving(true, 0.5f);  // 开始移动，强度0.5
mechaAudio.SetMoving(false);        // 停止移动

// 推进器
mechaAudio.StartThruster();
mechaAudio.StopThruster();

// 受伤
mechaAudio.PlayHit(isHeavy: true);
```

### 4. 武器音效

```csharp
// 添加到武器对象
WeaponAudioController weaponAudio = GetComponent<WeaponAudioController>();

weaponAudio.PlayFire();
weaponAudio.PlayReloadStart();
weaponAudio.PlayReloadComplete();
weaponAudio.StartCharge();
weaponAudio.StopCharge();
```

### 5. 环境音效区域

1. 创建空物体
2. 添加 Collider (Is Trigger = true)
3. 添加 `EnvironmentAudioZone` 脚本
4. 设置 ZoneType 和 ReverbZone

## 音量设置

```csharp
AudioManager.Instance.SetMasterVolume(0.8f);
AudioManager.Instance.SetBGMVolume(0.6f);
AudioManager.Instance.SetSFXVolume(1.0f);
AudioManager.Instance.SetUIVolume(0.8f);
```

## AudioMixer 快照

| 快照名称 | 用途 |
|---------|------|
| Base | 基地场景 |
| ShallowExplore | 浅海探索 |
| DeepCombat | 深海战斗 |
| BossBattle | Boss战 |

## 音效优先级

1. UI警告/警报
2. 武器开火
3. 机甲受伤
4. 敌人攻击
5. BGM
6. 环境音

## 技术规格

- **格式**: WAV (开发) / Vorbis (BGM发布) / ADPCM (SFX发布)
- **采样率**: 48000 Hz
- **位深**: 24-bit
- **声道**: Stereo (BGM) / Mono (SFX 3D)

## 联系

音效设计文档版本: v1.0
