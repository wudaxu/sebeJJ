using UnityEngine;
using System.Collections.Generic;

namespace SebeJJ.Utils
{
    /// <summary>
    /// 扩展音频管理器 - 添加音效库功能 (BUG-006 修复: 音效冷却机制)
    /// </summary>
    public partial class AudioManager : MonoBehaviour
    {
        [Header("音效库")]
        public List<AudioClipEntry> audioClips = new List<AudioClipEntry>();
        
        [Header("音效冷却")]
        public float defaultCooldown = 0.1f; // 默认冷却时间
        
        private Dictionary<string, AudioClip> clipDictionary;
        private Dictionary<string, float> lastPlayTime; // BUG-006: 记录上次播放时间
        private Dictionary<string, AudioSource> loopingSounds; // 循环音效管理
        
        [System.Serializable]
        public class AudioClipEntry
        {
            public string clipName;
            public AudioClip clip;
            [Range(0f, 1f)]
            public float volume = 1f;
            public float cooldown = 0.1f; // 单个音效冷却时间
            public bool preventOverlap = true; // 是否防止重叠
        }
        
        private void Start()
        {
            InitializeClipDictionary();
            lastPlayTime = new Dictionary<string, float>();
            loopingSounds = new Dictionary<string, AudioSource>();
        }
        
        private void InitializeClipDictionary()
        {
            clipDictionary = new Dictionary<string, AudioClip>();
            foreach (var entry in audioClips)
            {
                if (!string.IsNullOrEmpty(entry.clipName) && entry.clip != null)
                {
                    clipDictionary[entry.clipName] = entry.clip;
                }
            }
        }
        
        /// <summary>
        /// 获取音频片段
        /// </summary>
        public AudioClip GetClip(string clipName)
        {
            if (clipDictionary == null)
                InitializeClipDictionary();
            
            if (clipDictionary.TryGetValue(clipName, out AudioClip clip))
                return clip;
            
            Debug.LogWarning($"[AudioManager] 未找到音效: {clipName}");
            return null;
        }
        
        /// <summary>
        /// 播放指定名称的音效 (BUG-006 修复: 添加冷却机制)
        /// </summary>
        public void PlaySFX(string clipName, float volume = 1f)
        {
            // 检查冷却时间
            if (lastPlayTime.TryGetValue(clipName, out float lastTime))
            {
                float cooldown = GetCooldown(clipName);
                if (Time.time - lastTime < cooldown)
                {
                    return; // 冷却中，不播放
                }
            }
            
            AudioClip clip = GetClip(clipName);
            if (clip != null)
            {
                PlaySFX(clip, volume);
                lastPlayTime[clipName] = Time.time;
            }
        }
        
        /// <summary>
        /// 获取音效冷却时间
        /// </summary>
        private float GetCooldown(string clipName)
        {
            var entry = audioClips.Find(e => e.clipName == clipName);
            return entry?.cooldown ?? defaultCooldown;
        }
        
        /// <summary>
        /// 播放背景音乐（按名称）
        /// </summary>
        public void PlayMusic(string clipName, bool loop = true)
        {
            AudioClip clip = GetClip(clipName);
            if (clip != null)
            {
                PlayMusic(clip, loop);
            }
        }
        
        /// <summary>
        /// 播放环境音（按名称）
        /// </summary>
        public void PlayAmbient(string clipName, bool loop = true)
        {
            AudioClip clip = GetClip(clipName);
            if (clip != null)
            {
                PlayAmbient(clip, loop);
            }
        }
        
        /// <summary>
        /// 淡入背景音乐
        /// </summary>
        public void FadeInMusic(string clipName, float duration = 1f)
        {
            AudioClip clip = GetClip(clipName);
            if (clip == null) return;
            
            StartCoroutine(FadeInMusicCoroutine(clip, duration));
        }
        
        private System.Collections.IEnumerator FadeInMusicCoroutine(AudioClip clip, float duration)
        {
            if (musicSource == null) yield break;
            
            musicSource.clip = clip;
            musicSource.volume = 0f;
            musicSource.Play();
            
            float timer = 0f;
            float targetVolume = musicVolume * masterVolume;
            
            while (timer < duration)
            {
                timer += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(0f, targetVolume, timer / duration);
                yield return null;
            }
            
            musicSource.volume = targetVolume;
        }
        
        /// <summary>
        /// 淡出背景音乐
        /// </summary>
        public void FadeOutMusic(float duration = 1f)
        {
            if (musicSource == null || !musicSource.isPlaying) return;
            
            StartCoroutine(FadeOutMusicCoroutine(duration));
        }
        
        private System.Collections.IEnumerator FadeOutMusicCoroutine(float duration)
        {
            float timer = 0f;
            float startVolume = musicSource.volume;
            
            while (timer < duration)
            {
                timer += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / duration);
                yield return null;
            }
            
            musicSource.Stop();
        }
        
        /// <summary>
        /// 播放随机音效（从列表中）
        /// </summary>
        public void PlayRandomSFX(params string[] clipNames)
        {
            if (clipNames.Length == 0) return;
            
            string randomClip = clipNames[Random.Range(0, clipNames.Length)];
            PlaySFX(randomClip);
        }
        
        /// <summary>
        /// 播放3D音效
        /// </summary>
        public void PlaySFXAtPosition(string clipName, Vector3 position, float volume = 1f)
        {
            AudioClip clip = GetClip(clipName);
            if (clip == null) return;
            
            // 在指定位置创建临时音频源
            GameObject tempAudio = new GameObject("TempAudio");
            tempAudio.transform.position = position;
            
            AudioSource source = tempAudio.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = volume * sfxVolume * masterVolume;
            source.spatialBlend = 1f; // 3D音效
            source.Play();
            
            Destroy(tempAudio, clip.length);
        }
        
        /// <summary>
        /// 播放循环音效
        /// </summary>
        public void PlayLoopingSFX(string clipName, string loopId)
        {
            if (loopingSounds.ContainsKey(loopId))
            {
                // 已经在播放
                return;
            }
            
            AudioClip clip = GetClip(clipName);
            if (clip == null) return;
            
            GameObject loopObj = new GameObject($"LoopAudio_{loopId}");
            loopObj.transform.SetParent(transform);
            
            AudioSource source = loopObj.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = sfxVolume * masterVolume;
            source.loop = true;
            source.Play();
            
            loopingSounds[loopId] = source;
        }
        
        /// <summary>
        /// 停止循环音效
        /// </summary>
        public void StopLoopingSFX(string loopId)
        {
            if (loopingSounds.TryGetValue(loopId, out AudioSource source))
            {
                if (source != null)
                {
                    Destroy(source.gameObject);
                }
                loopingSounds.Remove(loopId);
            }
        }
        
        /// <summary>
        /// 停止所有循环音效
        /// </summary>
        public void StopAllLoopingSFX()
        {
            foreach (var kvp in loopingSounds)
            {
                if (kvp.Value != null)
                {
                    Destroy(kvp.Value.gameObject);
                }
            }
            loopingSounds.Clear();
        }
    }
}
