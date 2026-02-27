using UnityEngine;
using System;
using System.Collections;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 战斗音乐控制器 - CB-003
    /// 根据战斗强度动态切换音乐
    /// </summary>
    public class CombatMusicController : MonoBehaviour
    {
        public static CombatMusicController Instance { get; private set; }
        
        [Header("音乐片段")]
        [SerializeField] private AudioClip ambientMusic;             // 环境音乐
        [SerializeField] private AudioClip combatMusicLow;           // 低强度战斗音乐
        [SerializeField] private AudioClip combatMusicMedium;        // 中强度战斗音乐
        [SerializeField] private AudioClip combatMusicHigh;          // 高强度战斗音乐
        [SerializeField] private AudioClip bossMusic;                // Boss战音乐
        
        [Header("切换设置")]
        [SerializeField] private float fadeDuration = 1.5f;          // 淡入淡出时间
        [SerializeField] private float lowIntensityThreshold = 2f;   // 低强度阈值
        [SerializeField] private float mediumIntensityThreshold = 5f; // 中强度阈值
        [SerializeField] private float highIntensityThreshold = 8f;  // 高强度阈值
        
        [Header("音量设置")]
        [SerializeField] private float ambientVolume = 0.3f;         // 环境音乐音量
        [SerializeField] private float combatVolume = 0.7f;          // 战斗音乐音量
        [SerializeField] private float bossVolume = 0.8f;            // Boss音乐音量
        
        // 组件
        private AudioSource audioSource;
        private AudioSource secondarySource;
        
        // 运行时状态
        private CombatIntensity currentIntensity = CombatIntensity.Ambient;
        private bool isTransitioning = false;
        private Coroutine currentTransition;
        
        // 事件
        public event Action<CombatIntensity> OnIntensityChanged;
        
        // 战斗强度枚举
        public enum CombatIntensity
        {
            Ambient,        // 环境
            Low,            // 低强度
            Medium,         // 中强度
            High,           // 高强度
            Boss            // Boss战
        }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // 创建主音源
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;
            audioSource.playOnAwake = false;
            
            // 创建副音源用于交叉淡化
            secondarySource = gameObject.AddComponent<AudioSource>();
            secondarySource.loop = true;
            secondarySource.playOnAwake = false;
            secondarySource.volume = 0f;
        }
        
        private void Start()
        {
            // 播放环境音乐
            PlayMusic(ambientMusic, ambientVolume);
        }
        
        /// <summary>
        /// 更新战斗强度
        /// </summary>
        public void UpdateCombatIntensity(float enemyCount, float playerHealthPercent, bool isBossFight = false)
        {
            CombatIntensity newIntensity;
            
            if (isBossFight)
            {
                newIntensity = CombatIntensity.Boss;
            }
            else if (enemyCount >= highIntensityThreshold || playerHealthPercent < 0.3f)
            {
                newIntensity = CombatIntensity.High;
            }
            else if (enemyCount >= mediumIntensityThreshold)
            {
                newIntensity = CombatIntensity.Medium;
            }
            else if (enemyCount >= lowIntensityThreshold)
            {
                newIntensity = CombatIntensity.Low;
            }
            else
            {
                newIntensity = CombatIntensity.Ambient;
            }
            
            if (newIntensity != currentIntensity)
            {
                SetIntensity(newIntensity);
            }
        }
        
        /// <summary>
        /// 设置战斗强度
        /// </summary>
        public void SetIntensity(CombatIntensity intensity)
        {
            if (currentIntensity == intensity) return;
            
            currentIntensity = intensity;
            OnIntensityChanged?.Invoke(intensity);
            
            // 选择对应的音乐
            AudioClip newClip = null;
            float targetVolume = ambientVolume;
            
            switch (intensity)
            {
                case CombatIntensity.Ambient:
                    newClip = ambientMusic;
                    targetVolume = ambientVolume;
                    break;
                case CombatIntensity.Low:
                    newClip = combatMusicLow;
                    targetVolume = combatVolume;
                    break;
                case CombatIntensity.Medium:
                    newClip = combatMusicMedium;
                    targetVolume = combatVolume;
                    break;
                case CombatIntensity.High:
                    newClip = combatMusicHigh;
                    targetVolume = combatVolume;
                    break;
                case CombatIntensity.Boss:
                    newClip = bossMusic;
                    targetVolume = bossVolume;
                    break;
            }
            
            if (newClip != null)
            {
                TransitionToMusic(newClip, targetVolume);
            }
        }
        
        /// <summary>
        /// 切换到新音乐
        /// </summary>
        private void TransitionToMusic(AudioClip newClip, float targetVolume)
        {
            if (currentTransition != null)
            {
                StopCoroutine(currentTransition);
            }
            
            currentTransition = StartCoroutine(TransitionCoroutine(newClip, targetVolume));
        }
        
        /// <summary>
        /// 音乐过渡协程
        /// </summary>
        private IEnumerator TransitionCoroutine(AudioClip newClip, float targetVolume)
        {
            isTransitioning = true;
            
            // 如果当前没有播放，直接播放
            if (!audioSource.isPlaying)
            {
                audioSource.clip = newClip;
                audioSource.volume = targetVolume;
                audioSource.Play();
                isTransitioning = false;
                yield break;
            }
            
            // 设置副音源
            secondarySource.clip = newClip;
            secondarySource.volume = 0f;
            secondarySource.Play();
            
            // 交叉淡化
            float elapsed = 0f;
            float startVolume = audioSource.volume;
            
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                
                audioSource.volume = Mathf.Lerp(startVolume, 0f, t);
                secondarySource.volume = Mathf.Lerp(0f, targetVolume, t);
                
                yield return null;
            }
            
            // 交换音源
            audioSource.Stop();
            
            var temp = audioSource;
            audioSource = secondarySource;
            secondarySource = temp;
            
            secondarySource.volume = 0f;
            
            isTransitioning = false;
        }
        
        /// <summary>
        /// 播放指定音乐
        /// </summary>
        private void PlayMusic(AudioClip clip, float volume)
        {
            if (clip == null) return;
            
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.Play();
        }
        
        /// <summary>
        /// 暂停音乐
        /// </summary>
        public void PauseMusic()
        {
            audioSource.Pause();
            secondarySource.Pause();
        }
        
        /// <summary>
        /// 恢复音乐
        /// </summary>
        public void ResumeMusic()
        {
            audioSource.UnPause();
            secondarySource.UnPause();
        }
        
        /// <summary>
        /// 停止音乐
        /// </summary>
        public void StopMusic()
        {
            audioSource.Stop();
            secondarySource.Stop();
        }
        
        /// <summary>
        /// 设置主音量
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            AudioListener.volume = Mathf.Clamp01(volume);
        }
        
        /// <summary>
        /// 获取当前强度
        /// </summary>
        public CombatIntensity GetCurrentIntensity()
        {
            return currentIntensity;
        }
    }
}
