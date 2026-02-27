/**
 * @file BossHealthBar.cs
 * @brief Boss血条UI
 * @description 显示Boss生命值、阶段和状态
 * @author Boss战设计师
 * @date 2026-02-27
 */

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SebeJJ.Boss
{
    /// <summary>
    /// Boss血条UI控制器
    /// </summary>
    public class BossHealthBar : MonoBehaviour
    {
        #region 序列化字段

        [Header("=== UI组件 ===")]
        [SerializeField] private Canvas bossCanvas;
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Slider healthSliderDelayed;
        [SerializeField] private Image healthFillImage;
        [SerializeField] private TextMeshProUGUI bossNameText;
        [SerializeField] private TextMeshProUGUI phaseText;
        [SerializeField] private TextMeshProUGUI healthText;
        
        [Header("=== 阶段颜色 ===")]
        [SerializeField] private Color phase1Color = Color.green;
        [SerializeField] private Color phase2Color = Color.yellow;
        [SerializeField] private Color phase3Color = Color.red;
        [SerializeField] private Color rageColor = new Color(1f, 0.2f, 0.2f);
        
        [Header("=== 阶段图标 ===")]
        [SerializeField] private Image phaseIcon;
        [SerializeField] private Sprite phase1Icon;
        [SerializeField] private Sprite phase2Icon;
        [SerializeField] private Sprite phase3Icon;
        
        [Header("=== 状态图标 ===")]
        [SerializeField] private GameObject defendIcon;
        [SerializeField] private GameObject rageIcon;
        [SerializeField] private GameObject weakPointIcon;
        [SerializeField] private GameObject invincibleIcon;
        
        [Header("=== 动画设置 ===")]
        [SerializeField] private float delayedSliderSpeed = 2f;
        [SerializeField] private float damageFlashDuration = 0.2f;
        [SerializeField] private AnimationCurve damageFlashCurve;
        
        [Header("=== 阶段分隔线 ===")]
        [SerializeField] private RectTransform phaseMarkerContainer;
        [SerializeField] private GameObject phaseMarkerPrefab;
        [SerializeField] private float phase2MarkerPosition = 0.6f;
        [SerializeField] private float phase3MarkerPosition = 0.3f;

        #endregion

        #region 私有字段

        private IronClawBeastBoss _boss;
        private float _targetHealthPercent = 1f;
        private float _currentDelayedPercent = 1f;
        private bool _isFlashing = false;
        private float _flashTimer = 0f;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            // 确保Canvas存在
            if (bossCanvas == null)
            {
                bossCanvas = GetComponent<Canvas>();
            }
            
            // 初始化延迟血条
            if (healthSliderDelayed != null)
            {
                healthSliderDelayed.value = 1f;
            }
            
            // 隐藏所有状态图标
            HideAllStatusIcons();
        }

        private void Update()
        {
            // 更新延迟血条
            if (healthSliderDelayed != null)
            {
                _currentDelayedPercent = Mathf.MoveTowards(
                    _currentDelayedPercent, 
                    _targetHealthPercent, 
                    delayedSliderSpeed * Time.deltaTime);
                healthSliderDelayed.value = _currentDelayedPercent;
            }
            
            // 更新闪烁效果
            if (_isFlashing)
            {
                UpdateFlashEffect();
            }
            
            // 更新状态图标
            UpdateStatusIcons();
        }

        #endregion

        #region 初始化

        public void Initialize(IronClawBeastBoss boss)
        {
            _boss = boss;
            
            if (_boss == null) return;
            
            // 设置Boss名称
            if (bossNameText != null)
            {
                bossNameText.text = "铁钳巨兽";
            }
            
            // 订阅事件
            _boss.OnHealthChanged += OnHealthChanged;
            _boss.OnPhaseChanged += OnPhaseChanged;
            _boss.OnTakeDamage += OnTakeDamage;
            _boss.OnDefeated += OnDefeated;
            _boss.OnWeakPointExposed += OnWeakPointExposed;
            
            // 初始化阶段标记
            CreatePhaseMarkers();
            
            // 初始化显示
            UpdateHealth(_boss.CurrentHealth, _boss.MaxHealth);
            UpdatePhaseDisplay(_boss.CurrentPhase);
        }

        private void OnDestroy()
        {
            if (_boss != null)
            {
                _boss.OnHealthChanged -= OnHealthChanged;
                _boss.OnPhaseChanged -= OnPhaseChanged;
                _boss.OnTakeDamage -= OnTakeDamage;
                _boss.OnDefeated -= OnDefeated;
                _boss.OnWeakPointExposed -= OnWeakPointExposed;
            }
        }

        private void CreatePhaseMarkers()
        {
            if (phaseMarkerContainer == null || phaseMarkerPrefab == null) return;
            
            // 清除现有标记
            foreach (Transform child in phaseMarkerContainer)
            {
                Destroy(child.gameObject);
            }
            
            // 创建第二阶段标记 (60%)
            CreatePhaseMarker(phase2MarkerPosition, "II");
            
            // 创建第三阶段标记 (30%)
            CreatePhaseMarker(phase3MarkerPosition, "III");
        }

        private void CreatePhaseMarker(float position, string label)
        {
            GameObject marker = Instantiate(phaseMarkerPrefab, phaseMarkerContainer);
            RectTransform rect = marker.GetComponent<RectTransform>();
            
            // 设置位置
            rect.anchorMin = new Vector2(position, 0);
            rect.anchorMax = new Vector2(position, 1);
            rect.anchoredPosition = Vector2.zero;
            
            // 设置标签
            TextMeshProUGUI text = marker.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = label;
            }
        }

        #endregion

        #region 更新方法

        public void UpdateHealth(float currentHealth, float maxHealth)
        {
            float percent = currentHealth / maxHealth;
            _targetHealthPercent = percent;
            
            if (healthSlider != null)
            {
                healthSlider.value = percent;
            }
            
            if (healthText != null)
            {
                healthText.text = $"{currentHealth:0}/{maxHealth:0}";
            }
        }

        private void UpdatePhaseDisplay(BossPhase phase)
        {
            if (phaseText == null) return;
            
            switch (phase)
            {
                case BossPhase.Phase1:
                    phaseText.text = "第一阶段";
                    phaseText.color = phase1Color;
                    if (phaseIcon != null && phase1Icon != null)
                        phaseIcon.sprite = phase1Icon;
                    if (healthFillImage != null)
                        healthFillImage.color = phase1Color;
                    break;
                    
                case BossPhase.Phase2:
                    phaseText.text = "第二阶段";
                    phaseText.color = phase2Color;
                    if (phaseIcon != null && phase2Icon != null)
                        phaseIcon.sprite = phase2Icon;
                    if (healthFillImage != null)
                        healthFillImage.color = phase2Color;
                    break;
                    
                case BossPhase.Phase3:
                    phaseText.text = "最终阶段";
                    phaseText.color = phase3Color;
                    if (phaseIcon != null && phase3Icon != null)
                        phaseIcon.sprite = phase3Icon;
                    if (healthFillImage != null)
                        healthFillImage.color = _boss.IsEnraged ? rageColor : phase3Color;
                    break;
                    
                case BossPhase.Defeated:
                    phaseText.text = "已击败";
                    phaseText.color = Color.gray;
                    break;
            }
        }

        private void UpdateStatusIcons()
        {
            if (_boss == null) return;
            
            // 防御图标
            if (defendIcon != null)
                defendIcon.SetActive(_boss.IsDefending);
            
            // 狂暴图标
            if (rageIcon != null)
                rageIcon.SetActive(_boss.IsEnraged);
            
            // 弱点图标
            if (weakPointIcon != null)
                weakPointIcon.SetActive(_boss.IsWeakPointExposed);
            
            // 无敌图标
            if (invincibleIcon != null)
                invincibleIcon.SetActive(_boss.IsInvincible);
        }

        private void HideAllStatusIcons()
        {
            if (defendIcon != null) defendIcon.SetActive(false);
            if (rageIcon != null) rageIcon.SetActive(false);
            if (weakPointIcon != null) weakPointIcon.SetActive(false);
            if (invincibleIcon != null) invincibleIcon.SetActive(false);
        }

        #endregion

        #region 事件处理

        private void OnHealthChanged(float currentHealth)
        {
            UpdateHealth(currentHealth, _boss.MaxHealth);
        }

        private void OnPhaseChanged(BossPhase newPhase)
        {
            UpdatePhaseDisplay(newPhase);
            
            // 阶段转换特效
            PlayPhaseTransitionEffect();
        }

        private void OnTakeDamage(float damage)
        {
            // 触发闪烁效果
            StartDamageFlash();
            
            // 显示伤害数字
            ShowDamageNumber(damage);
        }

        private void OnDefeated()
        {
            UpdatePhaseDisplay(BossPhase.Defeated);
            
            // 播放击败动画
            PlayDefeatAnimation();
        }

        private void OnWeakPointExposed()
        {
            // 弱点暴露提示
            if (weakPointIcon != null)
            {
                // 可以添加闪烁动画
                StartCoroutine(WeakPointFlashCoroutine());
            }
        }

        #endregion

        #region 特效

        private void StartDamageFlash()
        {
            _isFlashing = true;
            _flashTimer = 0f;
            
            if (healthFillImage != null)
            {
                healthFillImage.color = Color.white;
            }
        }

        private void UpdateFlashEffect()
        {
            _flashTimer += Time.deltaTime;
            
            if (_flashTimer >= damageFlashDuration)
            {
                _isFlashing = false;
                // 恢复原始颜色
                UpdatePhaseDisplay(_boss.CurrentPhase);
                return;
            }
            
            // 使用动画曲线控制闪烁
            if (damageFlashCurve != null && damageFlashCurve.length > 0)
            {
                float t = damageFlashCurve.Evaluate(_flashTimer / damageFlashDuration);
                if (healthFillImage != null)
                {
                    healthFillImage.color = Color.Lerp(Color.white, GetCurrentPhaseColor(), t);
                }
            }
        }

        private Color GetCurrentPhaseColor()
        {
            switch (_boss.CurrentPhase)
            {
                case BossPhase.Phase1: return phase1Color;
                case BossPhase.Phase2: return phase2Color;
                case BossPhase.Phase3: return _boss.IsEnraged ? rageColor : phase3Color;
                default: return Color.gray;
            }
        }

        private void PlayPhaseTransitionEffect()
        {
            // 阶段转换时的UI动画
            // 可以添加缩放、颜色变化等效果
            if (phaseText != null)
            {
                StartCoroutine(PhaseTextPulseCoroutine());
            }
        }

        private void PlayDefeatAnimation()
        {
            // 击败时的UI动画
            if (bossCanvas != null)
            {
                // 淡出效果
                StartCoroutine(FadeOutCoroutine());
            }
        }

        private void ShowDamageNumber(float damage)
        {
            // 实例化伤害数字
            // TODO: 实现伤害数字显示
        }

        #endregion

        #region 协程

        private System.Collections.IEnumerator WeakPointFlashCoroutine()
        {
            float flashDuration = 2f;
            float flashInterval = 0.2f;
            float timer = 0f;
            
            while (timer < flashDuration)
            {
                weakPointIcon.SetActive(!weakPointIcon.activeSelf);
                yield return new WaitForSeconds(flashInterval);
                timer += flashInterval;
            }
            
            weakPointIcon.SetActive(_boss.IsWeakPointExposed);
        }

        private System.Collections.IEnumerator PhaseTextPulseCoroutine()
        {
            if (phaseText == null) yield break;
            
            Vector3 originalScale = phaseText.transform.localScale;
            float pulseDuration = 1f;
            float timer = 0f;
            
            while (timer < pulseDuration)
            {
                float scale = 1f + Mathf.Sin(timer * Mathf.PI * 4) * 0.2f;
                phaseText.transform.localScale = originalScale * scale;
                timer += Time.deltaTime;
                yield return null;
            }
            
            phaseText.transform.localScale = originalScale;
        }

        private System.Collections.IEnumerator FadeOutCoroutine()
        {
            CanvasGroup canvasGroup = bossCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = bossCanvas.gameObject.AddComponent<CanvasGroup>();
            }
            
            float fadeDuration = 2f;
            float timer = 0f;
            
            while (timer < fadeDuration)
            {
                canvasGroup.alpha = 1f - (timer / fadeDuration);
                timer += Time.deltaTime;
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
            bossCanvas.gameObject.SetActive(false);
        }

        #endregion
    }
}
