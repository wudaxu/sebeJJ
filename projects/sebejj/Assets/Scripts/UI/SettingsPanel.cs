using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace SebeJJ.UI
{
    /// <summary>
    /// 设置面板UI控制器
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        [Header("音频设置")]
        public Slider masterVolumeSlider;
        public Slider musicVolumeSlider;
        public Slider sfxVolumeSlider;
        public TextMeshProUGUI masterVolumeText;
        public TextMeshProUGUI musicVolumeText;
        public TextMeshProUGUI sfxVolumeText;
        
        [Header("画质设置")]
        public Dropdown qualityDropdown;
        public Dropdown resolutionDropdown;
        public Toggle fullscreenToggle;
        public Toggle vSyncToggle;
        public Slider frameRateSlider;
        public TextMeshProUGUI frameRateText;
        
        [Header("游戏设置")]
        public Slider mouseSensitivitySlider;
        public TextMeshProUGUI mouseSensitivityText;
        public Toggle vibrationToggle;
        public Toggle subtitlesToggle;
        public Dropdown languageDropdown;
        
        [Header("按钮")]
        public Button saveButton;
        public Button resetButton;
        public Button backButton;
        
        private Resolution[] availableResolutions;
        
        void Start()
        {
            InitializeUI();
            LoadCurrentSettings();
            BindEvents();
        }
        
        void InitializeUI()
        {
            // 初始化画质下拉菜单
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(QualitySettings.names.ToList());
            
            // 初始化分辨率下拉菜单
            availableResolutions = Screen.resolutions;
            resolutionDropdown.ClearOptions();
            var resolutionOptions = availableResolutions
                .Select(r => $"{r.width} x {r.height} @ {r.refreshRate}Hz")
                .ToList();
            resolutionDropdown.AddOptions(resolutionOptions);
            
            // 初始化语言下拉菜单
            languageDropdown.ClearOptions();
            languageDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "简体中文",
                "繁體中文",
                "English",
                "日本語"
            });
        }
        
        void LoadCurrentSettings()
        {
            if (Core.SettingsManager.Instance == null) return;
            
            var settings = Core.SettingsManager.Instance.CurrentSettings;
            
            // 音频
            masterVolumeSlider.value = settings.masterVolume;
            musicVolumeSlider.value = settings.musicVolume;
            sfxVolumeSlider.value = settings.sfxVolume;
            
            // 画质
            qualityDropdown.value = settings.qualityLevel;
            fullscreenToggle.isOn = settings.fullscreen;
            vSyncToggle.isOn = settings.vSync;
            frameRateSlider.value = settings.targetFrameRate;
            
            // 游戏
            mouseSensitivitySlider.value = settings.mouseSensitivity;
            vibrationToggle.isOn = settings.vibrationEnabled;
            subtitlesToggle.isOn = settings.subtitlesEnabled;
            
            // 更新文本显示
            UpdateVolumeTexts();
            UpdateFrameRateText();
            UpdateMouseSensitivityText();
        }
        
        void BindEvents()
        {
            // 音频滑块
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            
            // 画质设置
            qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            vSyncToggle.onValueChanged.AddListener(OnVSyncChanged);
            frameRateSlider.onValueChanged.AddListener(OnFrameRateChanged);
            
            // 游戏设置
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSensitivityChanged);
            vibrationToggle.onValueChanged.AddListener(OnVibrationChanged);
            subtitlesToggle.onValueChanged.AddListener(OnSubtitlesChanged);
            languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
            
            // 按钮
            saveButton?.onClick.AddListener(OnSaveClicked);
            resetButton?.onClick.AddListener(OnResetClicked);
            backButton?.onClick.AddListener(OnBackClicked);
        }
        
        #region 事件处理
        
        private void OnMasterVolumeChanged(float value)
        {
            Core.SettingsManager.Instance?.SetMasterVolume(value);
            UpdateVolumeTexts();
        }
        
        private void OnMusicVolumeChanged(float value)
        {
            Core.SettingsManager.Instance?.SetMusicVolume(value);
            UpdateVolumeTexts();
        }
        
        private void OnSFXVolumeChanged(float value)
        {
            Core.SettingsManager.Instance?.SetSFXVolume(value);
            UpdateVolumeTexts();
        }
        
        private void OnQualityChanged(int index)
        {
            Core.SettingsManager.Instance?.SetQualityLevel(index);
        }
        
        private void OnResolutionChanged(int index)
        {
            if (index < availableResolutions.Length)
            {
                var res = availableResolutions[index];
                Core.SettingsManager.Instance?.SetResolution(res.width, res.height, fullscreenToggle.isOn);
            }
        }
        
        private void OnFullscreenChanged(bool isOn)
        {
            int resIndex = resolutionDropdown.value;
            if (resIndex < availableResolutions.Length)
            {
                var res = availableResolutions[resIndex];
                Core.SettingsManager.Instance?.SetResolution(res.width, res.height, isOn);
            }
        }
        
        private void OnVSyncChanged(bool isOn)
        {
            Core.SettingsManager.Instance?.SetVSync(isOn);
        }
        
        private void OnFrameRateChanged(float value)
        {
            int fps = Mathf.RoundToInt(value);
            Core.SettingsManager.Instance?.SetTargetFrameRate(fps);
            UpdateFrameRateText();
        }
        
        private void OnMouseSensitivityChanged(float value)
        {
            Core.SettingsManager.Instance?.SetMouseSensitivity(value);
            UpdateMouseSensitivityText();
        }
        
        private void OnVibrationChanged(bool isOn)
        {
            Core.SettingsManager.Instance?.SetVibration(isOn);
        }
        
        private void OnSubtitlesChanged(bool isOn)
        {
            Core.SettingsManager.Instance?.SetSubtitles(isOn);
        }
        
        private void OnLanguageChanged(int index)
        {
            string[] languages = { "zh-CN", "zh-TW", "en", "ja" };
            if (index < languages.Length)
            {
                Core.SettingsManager.Instance?.SetLanguage(languages[index]);
            }
        }
        
        private void OnSaveClicked()
        {
            Core.SettingsManager.Instance?.SaveSettings();
            Core.UINotification.Instance?.ShowNotification("设置已保存", Core.NotificationType.Success);
        }
        
        private void OnResetClicked()
        {
            Core.SettingsManager.Instance?.ResetToDefault();
            LoadCurrentSettings();
            Core.UINotification.Instance?.ShowNotification("设置已重置为默认值", Core.NotificationType.Info);
        }
        
        private void OnBackClicked()
        {
            // 关闭设置面板
            gameObject.SetActive(false);
        }
        
        #endregion
        
        #region 更新UI文本
        
        private void UpdateVolumeTexts()
        {
            if (masterVolumeText != null)
                masterVolumeText.text = $"{Mathf.RoundToInt(masterVolumeSlider.value * 100)}%";
            if (musicVolumeText != null)
                musicVolumeText.text = $"{Mathf.RoundToInt(musicVolumeSlider.value * 100)}%";
            if (sfxVolumeText != null)
                sfxVolumeText.text = $"{Mathf.RoundToInt(sfxVolumeSlider.value * 100)}%";
        }
        
        private void UpdateFrameRateText()
        {
            if (frameRateText != null)
                frameRateText.text = $"{Mathf.RoundToInt(frameRateSlider.value)} FPS";
        }
        
        private void UpdateMouseSensitivityText()
        {
            if (mouseSensitivityText != null)
                mouseSensitivityText.text = $"{mouseSensitivitySlider.value:F1}x";
        }
        
        #endregion
    }
}
