using UnityEngine;
using UnityEngine.UI;

namespace SebeJJ.Audio
{
    /// <summary>
    /// UI音效控制器 - 为UI元素添加音效
    /// </summary>
    public class UIAudioController : MonoBehaviour
    {
        [Header("音效类型")]
        [SerializeField] private UISFXType clickSFX = UISFXType.Click;
        [SerializeField] private UISFXType hoverSFX = UISFXType.Hover;
        [SerializeField] private UISFXType confirmSFX = UISFXType.Confirm;

        [Header("触发设置")]
        [SerializeField] private bool playOnClick = true;
        [SerializeField] private bool playOnHover = false;
        [SerializeField] private bool playOnValueChanged = false;

        [Header("音量缩放")]
        [Range(0.1f, 2f)] [SerializeField] private float volumeScale = 1f;

        private Button button;
        private Toggle toggle;
        private Slider slider;
        private Scrollbar scrollbar;
        private Dropdown dropdown;

        private void Awake()
        {
            GetComponents();
            RegisterEvents();
        }

        private void GetComponents()
        {
            button = GetComponent<Button>();
            toggle = GetComponent<Toggle>();
            slider = GetComponent<Slider>();
            scrollbar = GetComponent<Scrollbar>();
            dropdown = GetComponent<Dropdown>();
        }

        private void RegisterEvents()
        {
            if (playOnClick)
            {
                if (button != null)
                    button.onClick.AddListener(OnClick);
                
                if (toggle != null)
                    toggle.onValueChanged.AddListener(OnToggleChanged);
                
                if (dropdown != null)
                    dropdown.onValueChanged.AddListener(OnDropdownChanged);
            }

            if (playOnHover)
            {
                // 需要添加EventTrigger或使用IPointerEnterHandler
                var trigger = GetComponent<UnityEngine.EventSystems.EventTrigger>();
                if (trigger == null)
                    trigger = gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

                var entry = new UnityEngine.EventSystems.EventTrigger.Entry
                {
                    eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
                };
                entry.callback.AddListener((data) => OnHover());
                trigger.triggers.Add(entry);
            }

            if (playOnValueChanged)
            {
                if (slider != null)
                    slider.onValueChanged.AddListener(OnSliderChanged);
                
                if (scrollbar != null)
                    scrollbar.onValueChanged.AddListener(OnScrollbarChanged);
            }
        }

        private void OnClick()
        {
            PlaySFX(clickSFX);
        }

        private void OnHover()
        {
            PlaySFX(hoverSFX);
        }

        private void OnToggleChanged(bool value)
        {
            PlaySFX(value ? confirmSFX : UISFXType.Cancel);
        }

        private void OnSliderChanged(float value)
        {
            // 限制播放频率
            if (Time.time % 0.1f < Time.deltaTime)
                PlaySFX(clickSFX, 0.3f);
        }

        private void OnScrollbarChanged(float value)
        {
            // 限制播放频率
            if (Time.time % 0.1f < Time.deltaTime)
                PlaySFX(clickSFX, 0.3f);
        }

        private void OnDropdownChanged(int value)
        {
            PlaySFX(confirmSFX);
        }

        private void PlaySFX(UISFXType type, float customVolumeScale = 1f)
        {
            if (AudioManager.Instance != null)
            {
                // 这里可以添加音量缩放逻辑
                AudioManager.Instance.PlayUISFX(type);
            }
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(OnClick);
            
            if (toggle != null)
                toggle.onValueChanged.RemoveListener(OnToggleChanged);
            
            if (slider != null)
                slider.onValueChanged.RemoveListener(OnSliderChanged);
            
            if (scrollbar != null)
                scrollbar.onValueChanged.RemoveListener(OnScrollbarChanged);
            
            if (dropdown != null)
                dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
        }
    }

    /// <summary>
    /// 全局UI音效管理器 - 处理菜单和系统级UI音效
    /// </summary>
    public static class UIAudio
    {
        /// <summary>
        /// 播放点击音效
        /// </summary>
        public static void PlayClick()
        {
            AudioManager.Instance?.PlayUISFX(UISFXType.Click);
        }

        /// <summary>
        /// 播放确认音效
        /// </summary>
        public static void PlayConfirm()
        {
            AudioManager.Instance?.PlayUISFX(UISFXType.Confirm);
        }

        /// <summary>
        /// 播放取消音效
        /// </summary>
        public static void PlayCancel()
        {
            AudioManager.Instance?.PlayUISFX(UISFXType.Cancel);
        }

        /// <summary>
        /// 播放警告音效
        /// </summary>
        public static void PlayWarning()
        {
            AudioManager.Instance?.PlayUISFX(UISFXType.Warning);
        }

        /// <summary>
        /// 播放错误音效
        /// </summary>
        public static void PlayError()
        {
            AudioManager.Instance?.PlayUISFX(UISFXType.Error);
        }

        /// <summary>
        /// 播放菜单打开音效
        /// </summary>
        public static void PlayMenuOpen()
        {
            AudioManager.Instance?.PlayUISFX(UISFXType.MenuOpen);
        }

        /// <summary>
        /// 播放菜单关闭音效
        /// </summary>
        public static void PlayMenuClose()
        {
            AudioManager.Instance?.PlayUISFX(UISFXType.MenuClose);
        }

        /// <summary>
        /// 播放解锁音效
        /// </summary>
        public static void PlayUnlock()
        {
            AudioManager.Instance?.PlayUISFX(UISFXType.Unlock);
        }

        /// <summary>
        /// 播放任务完成音效
        /// </summary>
        public static void PlayMissionComplete()
        {
            AudioManager.Instance?.PlayUISFX(UISFXType.MissionComplete);
        }
    }
}
