using UnityEngine;

namespace SebeJJ.Utils
{
    /// <summary>
    /// 事件系统 - 全局游戏事件
    /// </summary>
    public static class GameEvents
    {
        // 游戏状态事件
        public static System.Action OnGameStarted;
        public static System.Action OnGamePaused;
        public static System.Action OnGameResumed;
        public static System.Action OnGameOver;
        
        // 玩家事件
        public static System.Action<float> OnPlayerDamaged;
        public static System.Action OnPlayerDied;
        public static System.Action<float> OnPlayerHealed;
        
        // 资源事件
        public static System.Action<string, int> OnItemCollected;
        public static System.Action<int> OnCreditsEarned;
        public static System.Action<int> OnCreditsSpent;
        
        // 委托事件
        public static System.Action<string> OnMissionAccepted;
        public static System.Action<string> OnMissionCompleted;
        public static System.Action<string> OnMissionFailed;
        
        // 下潜事件
        public static System.Action<float> OnDepthChanged;
        public static System.Action OnSurfaceReached;
        public static System.Action OnDangerZoneEntered;
        
        // UI事件
        public static System.Action<string> OnNotification;
        public static System.Action<string> OnShowTooltip;
        public static System.Action OnHideTooltip;
        
        /// <summary>
        /// 触发游戏开始
        /// </summary>
        public static void TriggerGameStarted()
        {
            OnGameStarted?.Invoke();
        }
        
        /// <summary>
        /// 触发物品收集
        /// </summary>
        public static void TriggerItemCollected(string itemId, int quantity)
        {
            OnItemCollected?.Invoke(itemId, quantity);
        }
        
        /// <summary>
        /// 触发通知
        /// </summary>
        public static void TriggerNotification(string message)
        {
            OnNotification?.Invoke(message);
        }
    }
    
    /// <summary>
    /// 音频管理器
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        
        [Header("音频源")]
        public AudioSource musicSource;
        public AudioSource sfxSource;
        public AudioSource ambientSource;
        
        [Header("音量设置")]
        [Range(0f, 1f)]
        public float masterVolume = 1f;
        [Range(0f, 1f)]
        public float musicVolume = 0.7f;
        [Range(0f, 1f)]
        public float sfxVolume = 1f;
        [Range(0f, 1f)]
        public float ambientVolume = 0.5f;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        /// <summary>
        /// 播放音效
        /// </summary>
        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip == null || sfxSource == null) return;
            sfxSource.PlayOneShot(clip, volume * sfxVolume * masterVolume);
        }
        
        /// <summary>
        /// 播放背景音乐
        /// </summary>
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null || musicSource == null) return;
            
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.volume = musicVolume * masterVolume;
            musicSource.Play();
        }
        
        /// <summary>
        /// 播放环境音
        /// </summary>
        public void PlayAmbient(AudioClip clip, bool loop = true)
        {
            if (clip == null || ambientSource == null) return;
            
            ambientSource.clip = clip;
            ambientSource.loop = loop;
            ambientSource.volume = ambientVolume * masterVolume;
            ambientSource.Play();
        }
        
        /// <summary>
        /// 停止音乐
        /// </summary>
        public void StopMusic()
        {
            if (musicSource != null)
                musicSource.Stop();
        }
        
        /// <summary>
        /// 设置主音量
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            UpdateVolumes();
        }
        
        private void UpdateVolumes()
        {
            if (musicSource != null)
                musicSource.volume = musicVolume * masterVolume;
            if (ambientSource != null)
                ambientSource.volume = ambientVolume * masterVolume;
        }
    }
}
