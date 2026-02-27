using UnityEngine;
using System.Collections.Generic;

namespace SebeJJ.Audio
{
    /// <summary>
    /// 音效类型枚举
    /// </summary>
    public enum SoundType
    {
        // UI音效
        UI_Click,
        UI_Hover,
        UI_Confirm,
        UI_Cancel,
        UI_Error,
        UI_Notification,

        // 武器音效
        Weapon_Laser_Fire,
        Weapon_Laser_Loop,
        Weapon_Missile_Fire,
        Weapon_Missile_Explode,
        Weapon_Harpoon_Fire,
        Weapon_Harpoon_Hit,
        Weapon_Drill,
        Weapon_Reload,

        // 机甲音效
        Mech_Engine,
        Mech_Boost,
        Mech_Damage,
        Mech_Repair,
        Mech_Collect,
        Mech_Footstep,

        // 环境音效
        Ambient_Water,
        Ambient_Deep,
        Ambient_Cave,
        Ambient_Wreck,

        // 敌人音效
        Enemy_Jellyfish_Idle,
        Enemy_Jellyfish_Attack,
        Enemy_Drone_Alert,
        Enemy_Drone_Laser,
        Enemy_Angler_Ambush,
        Enemy_Angler_Dash,

        // 特效音效
        Explosion_Small,
        Explosion_Large,
        Impact_Metal,
        Impact_Water,
        Spark,
        Alarm,

        // 交互音效
        Collect_Resource,
        Open_Door,
        Hack_Device,
        Scan_Area,

        // 音乐
        Music_Menu,
        Music_Explore,
        Music_Combat,
        Music_Boss,
        Music_Victory,
        Music_GameOver
    }

    /// <summary>
    /// 音频数据
    /// </summary>
    [System.Serializable]
    public class AudioData
    {
        public SoundType soundType;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
        public bool loop = false;
        [Range(0f, 1f)] public float spatialBlend = 0f; // 0 = 2D, 1 = 3D
        [Range(0f, 5f)] public float randomPitchVariation = 0f;
    }

    /// <summary>
    /// 音效管理器 - 单例
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("音频配置")]
        [SerializeField] private List<AudioData> audioDatabase = new List<AudioData>();
        [SerializeField] private int poolSize = 20;

        [Header("音量设置")]
        [Range(0f, 1f)] public float masterVolume = 1f;
        [Range(0f, 1f)] public float musicVolume = 0.7f;
        [Range(0f, 1f)] public float sfxVolume = 0.8f;
        [Range(0f, 1f)] public float ambientVolume = 0.5f;

        [Header("音乐")]
        [SerializeField] private AudioSource musicSource;

        // 对象池
        private Queue<AudioSource> _audioPool;
        private List<AudioSource> _activeSources;
        private Dictionary<SoundType, AudioData> _audioMap;

        // 当前播放的环境音
        private AudioSource _currentAmbient;
        private SoundType _currentAmbientType;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            // 初始化对象池
            _audioPool = new Queue<AudioSource>();
            _activeSources = new List<AudioSource>();
            _audioMap = new Dictionary<SoundType, AudioData>();

            // 创建音频源池
            for (int i = 0; i < poolSize; i++)
            {
                CreateAudioSource();
            }

            // 构建音频映射
            foreach (var data in audioDatabase)
            {
                _audioMap[data.soundType] = data;
            }

            // 确保有音乐源
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
            }
        }

        private AudioSource CreateAudioSource()
        {
            GameObject obj = new GameObject("AudioSource_Pooled");
            obj.transform.SetParent(transform);
            AudioSource source = obj.AddComponent<AudioSource>();
            obj.SetActive(false);
            _audioPool.Enqueue(source);
            return source;
        }

        private AudioSource GetAudioSource()
        {
            if (_audioPool.Count > 0)
            {
                AudioSource source = _audioPool.Dequeue();
                source.gameObject.SetActive(true);
                _activeSources.Add(source);
                return source;
            }

            // 如果池满了，找一个已经停止的源
            foreach (var source in _activeSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }

            // 创建新的源
            return CreateAudioSource();
        }

        private void ReturnAudioSource(AudioSource source)
        {
            if (_activeSources.Remove(source))
            {
                source.Stop();
                source.clip = null;
                source.gameObject.SetActive(false);
                _audioPool.Enqueue(source);
            }
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        public AudioSource PlaySound(SoundType soundType, Vector3? position = null)
        {
            if (!_audioMap.TryGetValue(soundType, out var audioData))
            {
                Debug.LogWarning($"Audio not found for type: {soundType}");
                return null;
            }

            AudioSource source = GetAudioSource();
            if (source == null) return null;

            // 设置音频属性
            source.clip = audioData.clip;
            source.volume = audioData.volume * GetVolumeForType(soundType);
            source.pitch = audioData.pitch + Random.Range(-audioData.randomPitchVariation, audioData.randomPitchVariation);
            source.loop = audioData.loop;
            source.spatialBlend = audioData.spatialBlend;

            // 设置位置
            if (position.HasValue)
            {
                source.transform.position = position.Value;
                source.spatialBlend = 1f;
            }

            source.Play();

            // 如果不是循环播放，播放完后归还
            if (!audioData.loop)
            {
                StartCoroutine(ReturnSourceWhenFinished(source));
            }

            return source;
        }

        /// <summary>
        /// 在指定位置播放音效
        /// </summary>
        public void PlaySoundAtPosition(SoundType soundType, Vector3 position)
        {
            PlaySound(soundType, position);
        }

        /// <summary>
        /// 停止指定音效
        /// </summary>
        public void StopSound(AudioSource source)
        {
            if (source != null)
            {
                source.Stop();
                ReturnAudioSource(source);
            }
        }

        /// <summary>
        /// 停止所有音效
        /// </summary>
        public void StopAllSounds()
        {
            var activeCopy = new List<AudioSource>(_activeSources);
            foreach (var source in activeCopy)
            {
                ReturnAudioSource(source);
            }
        }

        /// <summary>
        /// 播放音乐
        /// </summary>
        public void PlayMusic(SoundType musicType, bool fade = true, float fadeDuration = 1f)
        {
            if (!_audioMap.TryGetValue(musicType, out var audioData))
            {
                Debug.LogWarning($"Music not found for type: {musicType}");
                return;
            }

            if (fade)
            {
                StartCoroutine(FadeMusic(audioData.clip, fadeDuration));
            }
            else
            {
                musicSource.clip = audioData.clip;
                musicSource.volume = audioData.volume * musicVolume * masterVolume;
                musicSource.Play();
            }
        }

        /// <summary>
        /// 停止音乐
        /// </summary>
        public void StopMusic(bool fade = true, float fadeDuration = 1f)
        {
            if (fade)
            {
                StartCoroutine(FadeOutMusic(fadeDuration));
            }
            else
            {
                musicSource.Stop();
            }
        }

        /// <summary>
        /// 播放环境音
        /// </summary>
        public void PlayAmbient(SoundType ambientType, bool fade = true)
        {
            if (_currentAmbientType == ambientType) return;

            // 停止当前环境音
            if (_currentAmbient != null)
            {
                if (fade)
                {
                    StartCoroutine(FadeOutAmbient(_currentAmbient, 1f));
                }
                else
                {
                    StopSound(_currentAmbient);
                }
            }

            // 播放新环境音
            _currentAmbient = PlaySound(ambientType);
            _currentAmbientType = ambientType;

            if (_currentAmbient != null)
            {
                _currentAmbient.volume = GetVolumeForType(ambientType);
            }
        }

        /// <summary>
        /// 获取音量
        /// </summary>
        private float GetVolumeForType(SoundType soundType)
        {
            return soundType switch
            {
                SoundType.Music_Menu or SoundType.Music_Explore or 
                SoundType.Music_Combat or SoundType.Music_Boss or 
                SoundType.Music_Victory or SoundType.Music_GameOver 
                    => musicVolume * masterVolume,
                
                SoundType.Ambient_Water or SoundType.Ambient_Deep or 
                SoundType.Ambient_Cave or SoundType.Ambient_Wreck 
                    => ambientVolume * masterVolume,
                
                _ => sfxVolume * masterVolume
            };
        }

        // 协程
        private System.Collections.IEnumerator ReturnSourceWhenFinished(AudioSource source)
        {
            yield return new WaitWhile(() => source.isPlaying);
            ReturnAudioSource(source);
        }

        private System.Collections.IEnumerator FadeMusic(AudioClip newClip, float duration)
        {
            float startVolume = musicSource.volume;
            float timer = 0f;

            // 淡出
            while (timer < duration / 2f)
            {
                timer += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / (duration / 2f));
                yield return null;
            }

            // 切换音乐
            musicSource.clip = newClip;
            musicSource.Play();

            // 淡入
            timer = 0f;
            float targetVolume = 1f;
            if (_audioMap.TryGetValue(SoundType.Music_Explore, out var data))
            {
                targetVolume = data.volume * musicVolume * masterVolume;
            }

            while (timer < duration / 2f)
            {
                timer += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0f, targetVolume, timer / (duration / 2f));
                yield return null;
            }

            musicSource.volume = targetVolume;
        }

        private System.Collections.IEnumerator FadeOutMusic(float duration)
        {
            float startVolume = musicSource.volume;
            float timer = 0f;

            while (timer < duration)
            {
                timer += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
                yield return null;
            }

            musicSource.Stop();
        }

        private System.Collections.IEnumerator FadeOutAmbient(AudioSource source, float duration)
        {
            if (source == null) yield break;

            float startVolume = source.volume;
            float timer = 0f;

            while (timer < duration && source != null)
            {
                timer += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
                yield return null;
            }

            if (source != null)
            {
                StopSound(source);
            }
        }

        // 音量控制方法
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
        }

        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            if (musicSource != null)
            {
                musicSource.volume = musicVolume * masterVolume;
            }
        }

        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
        }

        public void SetAmbientVolume(float volume)
        {
            ambientVolume = Mathf.Clamp01(volume);
            if (_currentAmbient != null)
            {
                _currentAmbient.volume = ambientVolume * masterVolume;
            }
        }

        private void UpdateAllVolumes()
        {
            if (musicSource != null)
            {
                musicSource.volume = musicVolume * masterVolume;
            }

            if (_currentAmbient != null)
            {
                _currentAmbient.volume = ambientVolume * masterVolume;
            }
        }
    }
}
