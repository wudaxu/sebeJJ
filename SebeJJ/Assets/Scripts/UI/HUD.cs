using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SebeJJ.Core;

namespace SebeJJ.UI
{
    /// <summary>
    /// HUD (Heads-Up Display) - 游戏主界面
    /// </summary>
    public class HUD : UIPanel
    {
        [Header("Status Bars")]
        [SerializeField] private Slider healthBar;
        [SerializeField] private Slider energyBar;
        [SerializeField] private Slider oxygenBar;
        [SerializeField] private Image healthFill;
        [SerializeField] private Image energyFill;
        [SerializeField] private Image oxygenFill;

        [Header("Status Text")]
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI energyText;
        [SerializeField] private TextMeshProUGUI oxygenText;

        [Header("Info Display")]
        [SerializeField] private TextMeshProUGUI depthText;
        [SerializeField] private TextMeshProUGUI cargoText;
        [SerializeField] private TextMeshProUGUI currencyText;

        [Header("Warning System")]
        [SerializeField] private GameObject warningPanel;
        [SerializeField] private TextMeshProUGUI warningText;
        [SerializeField] private float warningDuration = 3f;

        [Header("Notification System")]
        [SerializeField] private GameObject notificationPanel;
        [SerializeField] private TextMeshProUGUI notificationText;
        [SerializeField] private float notificationFadeTime = 0.5f;

        [Header("Damage Indicator")]
        [SerializeField] private Image damageIndicator;
        [SerializeField] private float damageIndicatorDuration = 0.5f;

        [Header("Colors")]
        [SerializeField] private Color healthColor = Color.red;
        [SerializeField] private Color energyColor = Color.yellow;
        [SerializeField] private Color oxygenColor = Color.cyan;
        [SerializeField] private Color warningColor = Color.red;
        [SerializeField] private Color lowValueColor = new Color(1f, 0.5f, 0f); // Orange

        private float _warningTimer;
        private float _notificationTimer;
        private float _damageIndicatorTimer;
        private CanvasGroup _notificationCanvasGroup;
        private CanvasGroup _warningCanvasGroup;

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            
            if (notificationPanel != null)
            {
                _notificationCanvasGroup = notificationPanel.GetComponent<CanvasGroup>();
                if (_notificationCanvasGroup == null)
                {
                    _notificationCanvasGroup = notificationPanel.AddComponent<CanvasGroup>();
                }
            }

            if (warningPanel != null)
            {
                _warningCanvasGroup = warningPanel.GetComponent<CanvasGroup>();
                if (_warningCanvasGroup == null)
                {
                    _warningCanvasGroup = warningPanel.AddComponent<CanvasGroup>();
                }
            }

            // 初始隐藏
            if (warningPanel != null) warningPanel.SetActive(false);
            if (notificationPanel != null) notificationPanel.SetActive(false);
            if (damageIndicator != null) damageIndicator.gameObject.SetActive(false);
        }

        private void Start()
        {
            SubscribeToEvents();
            InitializeBars();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            UpdateWarningTimer();
            UpdateNotificationTimer();
            UpdateDamageIndicator();
        }

        #endregion

        #region Initialization

        private void InitializeBars()
        {
            if (healthFill != null) healthFill.color = healthColor;
            if (energyFill != null) energyFill.color = energyColor;
            if (oxygenFill != null) oxygenFill.color = oxygenColor;
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnHealthChanged += OnHealthChanged;
            GameEvents.OnEnergyChanged += OnEnergyChanged;
            GameEvents.OnOxygenChanged += OnOxygenChanged;
            GameEvents.OnDepthChanged += OnDepthChanged;
            GameEvents.OnCurrencyChanged += OnCurrencyChanged;
            GameEvents.OnCargoChanged += OnCargoChanged;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnHealthChanged -= OnHealthChanged;
            GameEvents.OnEnergyChanged -= OnEnergyChanged;
            GameEvents.OnOxygenChanged -= OnOxygenChanged;
            GameEvents.OnDepthChanged -= OnDepthChanged;
            GameEvents.OnCurrencyChanged -= OnCurrencyChanged;
            GameEvents.OnCargoChanged -= OnCargoChanged;
        }

        #endregion

        #region Status Updates

        private void OnHealthChanged(float current, float max)
        {
            UpdateBar(healthBar, healthText, current, max);
            
            // 低生命值警告
            if (current / max < 0.2f)
            {
                ShowWarning("警告：生命值过低！");
                if (healthFill != null) healthFill.color = lowValueColor;
            }
            else
            {
                if (healthFill != null) healthFill.color = healthColor;
            }
        }

        private void OnEnergyChanged(float current, float max)
        {
            UpdateBar(energyBar, energyText, current, max);
            
            if (current / max < 0.2f)
            {
                ShowWarning("警告：能量不足！");
                if (energyFill != null) energyFill.color = lowValueColor;
            }
            else
            {
                if (energyFill != null) energyFill.color = energyColor;
            }
        }

        private void OnOxygenChanged(float current, float max)
        {
            UpdateBar(oxygenBar, oxygenText, current, max);
            
            if (current / max < 0.2f)
            {
                ShowWarning("警告：氧气即将耗尽！");
                if (oxygenFill != null) oxygenFill.color = lowValueColor;
            }
            else
            {
                if (oxygenFill != null) oxygenFill.color = oxygenColor;
            }
        }

        private void UpdateBar(Slider slider, TextMeshProUGUI text, float current, float max)
        {
            if (slider != null)
            {
                slider.maxValue = max;
                slider.value = current;
            }

            if (text != null)
            {
                text.text = $"{Mathf.Ceil(current)}/{max}";
            }
        }

        private void OnDepthChanged(float depth)
        {
            if (depthText != null)
            {
                depthText.text = $"深度: {depth:F1}m";
            }
        }

        private void OnCurrencyChanged(int currency)
        {
            if (currencyText != null)
            {
                currencyText.text = $"${currency:N0}";
            }
        }

        private void OnCargoChanged(int current, int max)
        {
            if (cargoText != null)
            {
                cargoText.text = $"货舱: {current}/{max}";
            }
        }

        #endregion

        #region Warnings

        public void ShowWarning(string message)
        {
            if (warningPanel == null || warningText == null) return;

            warningText.text = message;
            warningPanel.SetActive(true);
            
            if (_warningCanvasGroup != null)
            {
                _warningCanvasGroup.alpha = 1f;
            }
            
            _warningTimer = warningDuration;
        }

        private void UpdateWarningTimer()
        {
            if (_warningTimer > 0)
            {
                _warningTimer -= Time.deltaTime;
                
                if (_warningTimer <= 0)
                {
                    HideWarning();
                }
                else if (_warningTimer < 1f && _warningCanvasGroup != null)
                {
                    // 渐隐效果
                    _warningCanvasGroup.alpha = _warningTimer;
                }
            }
        }

        private void HideWarning()
        {
            if (warningPanel != null)
            {
                warningPanel.SetActive(false);
            }
            _warningTimer = 0;
        }

        #endregion

        #region Notifications

        public void ShowNotification(string message, float duration = 3f)
        {
            if (notificationPanel == null || notificationText == null) return;

            notificationText.text = message;
            notificationPanel.SetActive(true);
            
            if (_notificationCanvasGroup != null)
            {
                _notificationCanvasGroup.alpha = 1f;
            }
            
            _notificationTimer = duration;
        }

        private void UpdateNotificationTimer()
        {
            if (_notificationTimer > 0)
            {
                _notificationTimer -= Time.deltaTime;
                
                if (_notificationTimer <= 0)
                {
                    HideNotification();
                }
                else if (_notificationTimer < notificationFadeTime && _notificationCanvasGroup != null)
                {
                    _notificationCanvasGroup.alpha = _notificationTimer / notificationFadeTime;
                }
            }
        }

        private void HideNotification()
        {
            if (notificationPanel != null)
            {
                notificationPanel.SetActive(false);
            }
            _notificationTimer = 0;
        }

        #endregion

        #region Damage Indicator

        public void ShowDamageIndicator(Vector2 direction)
        {
            if (damageIndicator == null) return;

            // 计算伤害来源方向的角度
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            damageIndicator.rectTransform.rotation = Quaternion.Euler(0, 0, angle - 90);
            
            damageIndicator.gameObject.SetActive(true);
            _damageIndicatorTimer = damageIndicatorDuration;
        }

        private void UpdateDamageIndicator()
        {
            if (_damageIndicatorTimer > 0)
            {
                _damageIndicatorTimer -= Time.deltaTime;
                
                if (_damageIndicatorTimer <= 0)
                {
                    if (damageIndicator != null)
                    {
                        damageIndicator.gameObject.SetActive(false);
                    }
                }
                else
                {
                    // 闪烁效果
                    float alpha = Mathf.PingPong(Time.time * 10, 1f);
                    Color color = damageIndicator.color;
                    color.a = alpha;
                    damageIndicator.color = color;
                }
            }
        }

        #endregion
    }
}
