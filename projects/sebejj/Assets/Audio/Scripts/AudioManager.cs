using UnityEngine;
using UnityEngine.Audio;

namespace SebeJJ.Audio
{
    /// <summary>
    /// 音乐类型枚举
    /// </summary>
    public enum MusicType
    {
        Base,           // 基地主题
        ShallowExplore, // 浅海探索
        DeepCombat,     // 深海战斗
        BossBattle      // Boss战
    }

    /// <summary>
    /// 音效类型枚举
    /// </summary>
    public enum SFXType
    {
        // 机甲音效
        MechaMove,
        MechaThrusterStart,
        MechaThrusterLoop,
        MechaThrusterStop,
        MechaHitLight,
        MechaHitHeavy,
        MechaShield,
        MechaTransform,

        // 武器音效
        WeaponPrimaryFire,
        WeaponPrimaryHit,
        WeaponSecondaryFire,
        WeaponSecondaryHit,
        WeaponMissileLaunch,
        WeaponMissileExplode,
        WeaponReloadStart,
        WeaponReloadComplete,
        WeaponOverheat,
        WeaponCharge,

        // 敌人音效
        EnemyAttack,
        EnemyAttackRange,
        EnemyHitLight,
        EnemyHitHeavy,
        EnemyDeathSmall,
        EnemyDeathLarge,
        EnemyAlert,
        EnemyPatrol,

        // 环境音效
        EnvBubblesUp,
        EnvBubblePop,
        EnvPressureChange,
        EnvAlarm,
        EnvWaterCurrent,
        EnvMetalScrape,
        EnvSonarPing,
        EnvCreatureDistant
    }

    /// <summary>
    /// UI音效类型枚举
    /// </summary>
    public enum UISFXType
    {
        Click,
        Hover,
        Confirm,
        Cancel,
        Warning,
        Error,
        MenuOpen,
        MenuClose,
        Unlock,
        MissionComplete
    }

    /// <summary>
    /// 音频管理器 - 游戏音频系统的核心控制器
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer audioMixer;

        [Header("Music Snapshots")]
        [SerializeField] private AudioMixerSnapshot baseSnapshot;
        [SerializeField] private AudioMixerSnapshot exploreSnapshot;
        [SerializeField] private AudioMixerSnapshot combatSnapshot;
        [SerializeField] private AudioMixerSnapshot bossSnapshot;

        [Header("Music AudioSources")]
        [SerializeField] private AudioSource[] musicLayers; // 3层动态音乐

        [Header("Settings")]
        [SerializeField] private float musicTransitionTime = 3.0f;
        [SerializeField] private float combatFadeOutDelay = 5.0f;
        [SerializeField] private float maxSFXDistance = 100f;

        [Header("Volume Settings")]
        [Range(0f, 1f)] [SerializeField] private float masterVolume = 1.0f;
        [Range(0f, 1f)] [SerializeField] private float bgmVolume = 0.8f;
        [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1.0f;
        [Range(0f, 1f)] [SerializeField] private float uiVolume = 0.8f;

        // 当前状态
        private MusicType currentMusicType = MusicType.Base;
        private float combatEndTime;
        private bool isInCombat;

        // 对象池
        private System.Collections.Generic.Queue<AudioSource> sfxPool;
        private const int POOL_SIZE = 20;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializePool();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            ApplyVolumeSettings();
            TransitionToMusic(MusicType.Base);
        }

        private void Update()
        {
            // 自动退出战斗音乐
            if (isInCombat && Time.time > combatEndTime)
            {
                isInCombat = false;
                if (currentMusicType == MusicType.DeepCombat)
                {
                    TransitionToMusic(MusicType.ShallowExplore);
                }
            }
        }

        #region 初始化

        private void InitializePool()
        {
            sfxPool = new System.Collections.Generic.Queue<AudioSource>();
            for (int i = 0; i < POOL_SIZE; i++)
            {
                CreatePooledAudioSource();
            }
        }

        private AudioSource CreatePooledAudioSource()
        {
            GameObject go = new GameObject("PooledSFX");
            go.transform.SetParent(transform);
            AudioSource source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 1.0f; // 3D音效
            source.rolloffMode = AudioRolloffMode.Custom;
            source.minDistance = 5f;
            source.maxDistance = maxSFXDistance;
            sfxPool.Enqueue(source);
            return source;
        }

        #endregion

        #region 背景音乐控制

        /// <summary>
        /// 切换到指定音乐
        /// </summary>
        public void TransitionToMusic(MusicType type, float? customTransitionTime = null)
        {
            float transition = customTransitionTime ?? musicTransitionTime;
            currentMusicType = type;

            AudioMixerSnapshot targetSnapshot = type switch
            {
                MusicType.Base => baseSnapshot,
                MusicType.ShallowExplore => exploreSnapshot,
                MusicType.DeepCombat => combatSnapshot,
                MusicType.BossBattle => bossSnapshot,
                _ => baseSnapshot
            };

            targetSnapshot.TransitionTo(transition);
        }

        /// <summary>
        /// 进入战斗状态（自动切换战斗音乐）
        /// </summary>
        public void EnterCombat()
        {
            isInCombat = true;
            combatEndTime = Time.time + combatFadeOutDelay;
            
            if (currentMusicType != MusicType.BossBattle)
            {
                TransitionToMusic(MusicType.DeepCombat);
            }
        }

        /// <summary>
        /// 设置音乐强度（控制动态层）
        /// </summary>
        public void SetMusicIntensity(float intensity)
        {
            intensity = Mathf.Clamp01(intensity);
            
            // 控制3层音乐的音量
            if (musicLayers.Length >= 3)
            {
                SetLayerVolume(0, Mathf.Lerp(-10f, 0f, intensity));      // 基础层
                SetLayerVolume(1, Mathf.Lerp(-20f, -5f, intensity));     // 旋律层
                SetLayerVolume(2, Mathf.Lerp(-80f, 0f, intensity));      // 紧张层
            }
        }

        private void SetLayerVolume(int layerIndex, float volumeDb)
        {
            if (layerIndex < musicLayers.Length)
            {
                musicLayers[layerIndex].volume = Mathf.Pow(10f, volumeDb / 20f);
            }
        }

        #endregion

        #region 音效播放

        /// <summary>
        /// 在指定位置播放3D音效
        /// </summary>
        public void PlaySFX(SFXType type, Vector3 position, float volumeScale = 1.0f)
        {
            AudioClip clip = GetSFXClip(type);
            if (clip == null) return;

            AudioSource source = GetPooledAudioSource();
            source.transform.position = position;
            source.clip = clip;
            source.volume = GetSFXVolume(type) * volumeScale;
            source.spatialBlend = 1.0f;
            source.Play();

            StartCoroutine(ReturnToPool(source, clip.length));
        }

        /// <summary>
        /// 播放2D音效（UI等）
        /// </summary>
        public void PlaySFX2D(SFXType type, float volumeScale = 1.0f)
        {
            AudioClip clip = GetSFXClip(type);
            if (clip == null) return;

            AudioSource source = GetPooledAudioSource();
            source.clip = clip;
            source.volume = GetSFXVolume(type) * volumeScale;
            source.spatialBlend = 0f; // 2D
            source.Play();

            StartCoroutine(ReturnToPool(source, clip.length));
        }

        /// <summary>
        /// 播放UI音效
        /// </summary>
        public void PlayUISFX(UISFXType type)
        {
            AudioClip clip = GetUISFXClip(type);
            if (clip == null) return;

            AudioSource source = GetPooledAudioSource();
            source.clip = clip;
            source.volume = uiVolume;
            source.spatialBlend = 0f;
            source.outputAudioMixerGroup = GetUIGroup();
            source.Play();

            StartCoroutine(ReturnToPool(source, clip.length));
        }

        /// <summary>
        /// 播放循环音效
        /// </summary>
        public AudioSource PlaySFXLoop(SFXType type, Vector3 position)
        {
            AudioClip clip = GetSFXClip(type);
            if (clip == null) return null;

            GameObject go = new GameObject($"LoopSFX_{type}");
            go.transform.position = position;
            AudioSource source = go.AddComponent<AudioSource>();
            source.clip = clip;
            source.loop = true;
            source.volume = GetSFXVolume(type);
            source.spatialBlend = 1.0f;
            source.rolloffMode = AudioRolloffMode.Custom;
            source.minDistance = 5f;
            source.maxDistance = maxSFXDistance;
            source.Play();

            return source;
        }

        /// <summary>
        /// 停止循环音效
        /// </summary>
        public void StopSFXLoop(AudioSource source, float fadeOutTime = 0.5f)
        {
            if (source == null) return;
            StartCoroutine(FadeOutAndDestroy(source, fadeOutTime));
        }

        #endregion

        #region 音量控制

        /// <summary>
        /// 设置主音量
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }

        /// <summary>
        /// 设置BGM音量
        /// </summary>
        public void SetBGMVolume(float volume)
        {
            bgmVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }

        /// <summary>
        /// 设置SFX音量
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }

        /// <summary>
        /// 设置UI音量
        /// </summary>
        public void SetUIVolume(float volume)
        {
            uiVolume = Mathf.Clamp01(volume);
            ApplyVolumeSettings();
        }

        private void ApplyVolumeSettings()
        {
            if (audioMixer != null)
            {
                audioMixer.SetFloat("MasterVolume", VolumeToDb(masterVolume));
                audioMixer.SetFloat("BGMVolume", VolumeToDb(bgmVolume));
                audioMixer.SetFloat("SFXVolume", VolumeToDb(sfxVolume));
                audioMixer.SetFloat("UIVolume", VolumeToDb(uiVolume));
            }
        }

        private float VolumeToDb(float volume)
        {
            return volume > 0.001f ? 20f * Mathf.Log10(volume) : -80f;
        }

        #endregion

        #region 辅助方法

        private AudioSource GetPooledAudioSource()
        {
            if (sfxPool.Count > 0)
            {
                return sfxPool.Dequeue();
            }
            return CreatePooledAudioSource();
        }

        private System.Collections.IEnumerator ReturnToPool(AudioSource source, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (source != null)
            {
                source.Stop();
                source.clip = null;
                source.transform.position = Vector3.zero;
                sfxPool.Enqueue(source);
            }
        }

        private System.Collections.IEnumerator FadeOutAndDestroy(AudioSource source, float duration)
        {
            float startVolume = source.volume;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
                yield return null;
            }

            if (source != null)
            {
                source.Stop();
                Destroy(source.gameObject);
            }
        }

        private AudioClip GetSFXClip(SFXType type)
        {
            // TODO: 从资源管理器加载对应音效
            // return ResourceManager.Instance.LoadSFX(type.ToString());
            return null;
        }

        private AudioClip GetUISFXClip(UISFXType type)
        {
            // TODO: 从资源管理器加载对应UI音效
            // return ResourceManager.Instance.LoadUISFX(type.ToString());
            return null;
        }

        private float GetSFXVolume(SFXType type)
        {
            return type switch
            {
                // 武器音效 - 较高优先级
                SFXType.WeaponPrimaryFire or 
                SFXType.WeaponSecondaryFire or 
                SFXType.WeaponMissileLaunch => sfxVolume * 1.0f,

                // 机甲音效
                SFXType.MechaHitHeavy or 
                SFXType.MechaTransform => sfxVolume * 0.9f,

                SFXType.MechaHitLight or 
                SFXType.MechaShield => sfxVolume * 0.8f,

                SFXType.MechaMove or 
                SFXType.MechaThrusterLoop => sfxVolume * 0.7f,

                // 敌人音效
                SFXType.EnemyDeathLarge => sfxVolume * 0.9f,
                SFXType.EnemyAttack or 
                SFXType.EnemyAlert => sfxVolume * 0.8f,
                SFXType.EnemyPatrol => sfxVolume * 0.4f,

                // 环境音效 - 较低音量
                SFXType.EnvAlarm => sfxVolume * 0.8f,
                SFXType.EnvSonarPing => sfxVolume * 0.6f,
                SFXType.EnvBubblesUp or 
                SFXType.EnvWaterCurrent => sfxVolume * 0.3f,

                _ => sfxVolume * 0.7f
            };
        }

        private UnityEngine.Audio.AudioMixerGroup GetUIGroup()
        {
            if (audioMixer != null)
            {
                return audioMixer.FindMatchingGroups("UI")[0];
            }
            return null;
        }

        #endregion
    }
}
