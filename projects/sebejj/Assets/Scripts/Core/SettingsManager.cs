using System;
using UnityEngine;
using UnityEngine.Audio;

namespace SebeJJ.Core
{
    /// <summary>
    /// 游戏设置管理器 - 管理音量、画质、控制等玩家设置
    /// </summary>    public class SettingsManager : MonoBehaviour
    {
        public static SettingsManager Instance { get; private set; }
        
        [Header("音频")]
        public AudioMixer audioMixer;
        
        [Header("默认设置")]
        public SettingsData defaultSettings;
        
        // 当前设置
        public SettingsData CurrentSettings { get; private set; }
        
        // 设置变更事件
        public event Action<SettingsData> OnSettingsChanged;
        public event Action OnSettingsSaved;
        public event Action OnSettingsLoaded;
        
        // 设置键名
        private const string SETTINGS_KEY = "GameSettings";
        
        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 加载设置
            LoadSettings();
        }
        
        #region 音量设置
        
        /// <summary>
        /// 设置主音量
        /// </summary>        public void SetMasterVolume(float volume)
        {
            CurrentSettings.masterVolume = Mathf.Clamp01(volume);
            ApplyAudioSettings();
            SaveSettings();
        }
        
        /// <summary>
        /// 设置音乐音量
        /// </summary>        public void SetMusicVolume(float volume)
        {
            CurrentSettings.musicVolume = Mathf.Clamp01(volume);
            ApplyAudioSettings();
            SaveSettings();
        }
        
        /// <summary>
        /// 设置音效音量
        /// </summary>        public void SetSFXVolume(float volume)
        {
            CurrentSettings.sfxVolume = Mathf.Clamp01(volume);
            ApplyAudioSettings();
            SaveSettings();
        }
        
        /// <summary>
        /// 应用音频设置
        /// </summary>        private void ApplyAudioSettings()
        {
            if (audioMixer != null)
            {
                // 转换为分贝 (0-1 映射到 -80dB 到 0dB)
                float masterDb = CurrentSettings.masterVolume > 0 ? Mathf.Log10(CurrentSettings.masterVolume) * 20 : -80f;
                float musicDb = CurrentSettings.musicVolume > 0 ? Mathf.Log10(CurrentSettings.musicVolume) * 20 : -80f;
                float sfxDb = CurrentSettings.sfxVolume > 0 ? Mathf.Log10(CurrentSettings.sfxVolume) * 20 : -80f;
                
                audioMixer.SetFloat("MasterVolume", masterDb);
                audioMixer.SetFloat("MusicVolume", musicDb);
                audioMixer.SetFloat("SFXVolume", sfxDb);
            }
            
            OnSettingsChanged?.Invoke(CurrentSettings);
        }
        
        #endregion
        
        #region 画质设置
        
        /// <summary>
        /// 设置画质等级
        /// </summary>        public void SetQualityLevel(int level)
        {
            CurrentSettings.qualityLevel = Mathf.Clamp(level, 0, QualitySettings.names.Length - 1);
            QualitySettings.SetQualityLevel(CurrentSettings.qualityLevel, true);
            SaveSettings();
            OnSettingsChanged?.Invoke(CurrentSettings);
        }
        
        /// <summary>
        /// 设置分辨率
        /// </summary>        public void SetResolution(int width, int height, bool fullscreen)
        {
            CurrentSettings.resolutionWidth = width;
            CurrentSettings.resolutionHeight = height;
            CurrentSettings.fullscreen = fullscreen;
            
            Screen.SetResolution(width, height, fullscreen);
            SaveSettings();
            OnSettingsChanged?.Invoke(CurrentSettings);
        }
        
        /// <summary>
        /// 设置帧率限制
        /// </summary>        public void SetTargetFrameRate(int fps)
        {
            CurrentSettings.targetFrameRate = fps;
            Application.targetFrameRate = fps;
            SaveSettings();
            OnSettingsChanged?.Invoke(CurrentSettings);
        }
        
        /// <summary>
        /// 设置垂直同步
        /// </summary>        public void SetVSync(bool enabled)
        {
            CurrentSettings.vSync = enabled;
            QualitySettings.vSyncCount = enabled ? 1 : 0;
            SaveSettings();
            OnSettingsChanged?.Invoke(CurrentSettings);
        }
        
        #endregion
        
        #region 游戏设置
        
        /// <summary>
        /// 设置鼠标灵敏度
        /// </summary>        public void SetMouseSensitivity(float sensitivity)
        {
            CurrentSettings.mouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 3f);
            SaveSettings();
            OnSettingsChanged?.Invoke(CurrentSettings);
        }
        
        /// <summary>
        /// 设置震动开关
        /// </summary>        public void SetVibration(bool enabled)
        {
            CurrentSettings.vibrationEnabled = enabled;
            SaveSettings();
            OnSettingsChanged?.Invoke(CurrentSettings);
        }
        
        /// <summary>
        /// 设置字幕开关
        /// </summary>        public void SetSubtitles(bool enabled)
        {
            CurrentSettings.subtitlesEnabled = enabled;
            SaveSettings();
            OnSettingsChanged?.Invoke(CurrentSettings);
        }
        
        /// <summary>
        /// 设置语言
        /// </summary>        public void SetLanguage(string language)
        {
            CurrentSettings.language = language;
            // TODO: 应用语言变更
            SaveSettings();
            OnSettingsChanged?.Invoke(CurrentSettings);
        }
        
        #endregion
        
        #region 存档/加载
        
        /// <summary>
        /// 保存设置
        /// </summary>        public void SaveSettings()
        {
            string json = JsonUtility.ToJson(CurrentSettings);
            PlayerPrefs.SetString(SETTINGS_KEY, json);
            PlayerPrefs.Save();
            
            OnSettingsSaved?.Invoke();
            Debug.Log("[SettingsManager] 设置已保存");
        }
        
        /// <summary>
        /// 加载设置
        /// </summary>        public void LoadSettings()
        {
            if (PlayerPrefs.HasKey(SETTINGS_KEY))
            {
                string json = PlayerPrefs.GetString(SETTINGS_KEY);
                CurrentSettings = JsonUtility.FromJson<SettingsData>(json);
                
                if (CurrentSettings == null)
                {
                    CurrentSettings = new SettingsData(defaultSettings);
                }
            }
            else
            {
                // 使用默认设置
                CurrentSettings = new SettingsData(defaultSettings);
            }
            
            // 应用设置
            ApplyAllSettings();
            
            OnSettingsLoaded?.Invoke();
            Debug.Log("[SettingsManager] 设置已加载");
        }
        
        /// <summary>
        /// 应用所有设置
        /// </summary>        private void ApplyAllSettings()
        {
            ApplyAudioSettings();
            
            // 应用画质
            QualitySettings.SetQualityLevel(CurrentSettings.qualityLevel, true);
            
            // 应用分辨率
            Screen.SetResolution(
                CurrentSettings.resolutionWidth,
                CurrentSettings.resolutionHeight,
                CurrentSettings.fullscreen
            );
            
            // 应用帧率
            Application.targetFrameRate = CurrentSettings.targetFrameRate;
            
            // 应用垂直同步
            QualitySettings.vSyncCount = CurrentSettings.vSync ? 1 : 0;
        }
        
        /// <summary>
        /// 重置为默认设置
        /// </summary>        public void ResetToDefault()
        {
            CurrentSettings = new SettingsData(defaultSettings);
            ApplyAllSettings();
            SaveSettings();
            OnSettingsChanged?.Invoke(CurrentSettings);
        }
        
        #endregion
    }
    
    /// <summary>
    /// 设置数据结构
    /// </summary>    [Serializable]
    public class SettingsData
    {
        // 音频
        public float masterVolume = 1f;
        public float musicVolume = 0.8f;
        public float sfxVolume = 1f;
        
        // 画质
        public int qualityLevel = 2; // 0=低, 1=中, 2=高, 3=极高
        public int resolutionWidth = 1920;
        public int resolutionHeight = 1080;
        public bool fullscreen = true;
        public int targetFrameRate = 60;
        public bool vSync = true;
        
        // 游戏
        public float mouseSensitivity = 1f;
        public bool vibrationEnabled = true;
        public bool subtitlesEnabled = true;
        public string language = "zh-CN";
        
        // 构造函数
        public SettingsData() { }
        
        public SettingsData(SettingsData other)
        {
            masterVolume = other.masterVolume;
            musicVolume = other.musicVolume;
            sfxVolume = other.sfxVolume;
            qualityLevel = other.qualityLevel;
            resolutionWidth = other.resolutionWidth;
            resolutionHeight = other.resolutionHeight;
            fullscreen = other.fullscreen;
            targetFrameRate = other.targetFrameRate;
            vSync = other.vSync;
            mouseSensitivity = other.mouseSensitivity;
            vibrationEnabled = other.vibrationEnabled;
            subtitlesEnabled = other.subtitlesEnabled;
            language = other.language;
        }
    }
}
