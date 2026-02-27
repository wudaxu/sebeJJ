using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SebeJJ.UI.Animation
{
    /// <summary>
    /// 血条动画控制器 - 性能优化版本
    /// 优化点:
    /// 1. DOTween自动回收设置
    /// 2. 缓存Tween引用
    /// 3. 减少颜色更新频率
    /// 4. 批量动画合并
    /// </summary>
    public class HealthBarAnimatorOptimized : MonoBehaviour
    {
        [Header("基础配置")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image fillImage;
        [SerializeField] private Image delayedFillImage;
        [SerializeField] private RectTransform barTransform;
        
        [Header("数值变化动画")]
        [SerializeField] private float valueChangeDuration = 0.3f;
        [SerializeField] private Ease valueChangeEase = Ease.OutQuad;
        [SerializeField] private float delayedFillDelay = 0.2f;
        [SerializeField] private float delayedFillDuration = 0.5f;
        
        [Header("颜色配置")]
        [SerializeField] private Color highHealthColor = new Color(0.2f, 0.9f, 0.3f, 1f);
        [SerializeField] private Color mediumHealthColor = new Color(0.95f, 0.8f, 0.1f, 1f);
        [SerializeField] private Color lowHealthColor = new Color(0.95f, 0.2f, 0.1f, 1f);
        [SerializeField] private float mediumHealthThreshold = 0.6f;
        [SerializeField] private float lowHealthThreshold = 0.3f;
        
        [Header("低血量警告")]
        [SerializeField] private float warningPulseSpeed = 0.5f;
        [SerializeField] private float warningShakeAmount = 2f;
        [SerializeField] private float warningShakeDuration = 0.1f;
        [SerializeField] private Image warningOverlay;
        
        [Header("受伤反馈")]
        [SerializeField] private float damageFlashDuration = 0.15f;
        [SerializeField] private Color damageFlashColor = new Color(1f, 0.2f, 0.2f, 0.5f);
        [SerializeField] private float damageShakeAmount = 5f;
        
        [Header("治疗反馈")]
        [SerializeField] private float healFlashDuration = 0.3f;
        [SerializeField] private Color healFlashColor = new Color(0.2f, 1f, 0.4f, 0.4f);
        
        // Tween缓存
        private Sequence warningSequence;
        private Sequence flashSequence;
        private Tweener valueTween;
        private Tweener delayedTween;
        private Tweener colorTween;
        
        // 状态
        private bool isWarningActive;
        private float currentValue;
        private Color currentTargetColor;
        
        // 颜色更新优化
        private float lastColorUpdateTime;
        private const float ColorUpdateInterval = 0.1f;
        
        private void Awake()
        {
            if (healthSlider == null)
                healthSlider = GetComponent<Slider>();
            if (fillImage == null && healthSlider != null)
                fillImage = healthSlider.fillRect.GetComponent<Image>();
            if (barTransform == null)
                barTransform = GetComponent<RectTransform>();
                
            currentValue = healthSlider?.value ?? 1f;
            currentTargetColor = highHealthColor;
        }
        
        /// <summary>
        /// 设置血量值（带动画）
        /// </summary>
        public void SetHealth(float value, float maxValue = 1f)
        {
            float normalizedValue = Mathf.Clamp01(value / maxValue);
            float previousValue = currentValue;
            currentValue = normalizedValue;
            
            // 停止之前的动画
            KillTweens();
            
            // 主血条动画 - 设置AutoKill(false)以便复用
            if (healthSlider != null)
            {
                valueTween = DOTween.To(() => healthSlider.value, x => healthSlider.value = x, 
                    normalizedValue, valueChangeDuration)
                    .SetEase(valueChangeEase)
                    .SetAutoKill(false)
                    .OnUpdate(() => {
                        // 间隔性更新颜色
                        if (Time.time - lastColorUpdateTime >= ColorUpdateInterval)
                        {
                            lastColorUpdateTime = Time.time;
                            UpdateColor(healthSlider.value);
                        }
                    });
            }
            
            // 延迟血条动画
            if (delayedFillImage != null)
            {
                delayedTween = delayedFillImage.DOFillAmount(normalizedValue, delayedFillDuration)
                    .SetEase(Ease.OutQuad)
                    .SetDelay(delayedFillDelay)
                    .SetAutoKill(false);
            }
            
            // 判断是受伤还是治疗
            if (normalizedValue < previousValue)
            {
                PlayDamageFeedback();
            }
            else if (normalizedValue > previousValue)
            {
                PlayHealFeedback();
            }
            
            // 检查低血量警告
            CheckLowHealthWarning(normalizedValue);
        }
        
        /// <summary>
        /// 立即设置血量（无动画）
        /// </summary>
        public void SetHealthInstant(float value, float maxValue = 1f)
        {
            float normalizedValue = Mathf.Clamp01(value / maxValue);
            currentValue = normalizedValue;
            
            KillTweens();
            
            if (healthSlider != null)
            {
                healthSlider.value = normalizedValue;
                UpdateColor(normalizedValue);
            }
            
            if (delayedFillImage != null)
            {
                delayedFillImage.fillAmount = normalizedValue;
            }
            
            CheckLowHealthWarning(normalizedValue);
        }
        
        private void KillTweens()
        {
            valueTween?.Kill();
            delayedTween?.Kill();
            colorTween?.Kill();
        }
        
        private void UpdateColor(float value)
        {
            if (fillImage == null) return;
            
            Color targetColor;
            if (value <= lowHealthThreshold)
            {
                targetColor = lowHealthColor;
            }
            else if (value <= mediumHealthThreshold)
            {
                targetColor = mediumHealthColor;
            }
            else
            {
                targetColor = highHealthColor;
            }
            
            // 只有当颜色变化时才更新
            if (targetColor != currentTargetColor)
            {
                currentTargetColor = targetColor;
                fillImage.color = targetColor;
            }
        }
        
        private void CheckLowHealthWarning(float value)
        {
            bool shouldWarn = value <= lowHealthThreshold && value > 0f;
            
            if (shouldWarn && !isWarningActive)
            {
                StartWarningAnimation();
            }
            else if (!shouldWarn && isWarningActive)
            {
                StopWarningAnimation();
            }
        }
        
        private void StartWarningAnimation()
        {
            isWarningActive = true;
            warningSequence?.Kill();
            
            // 创建新的Sequence
            warningSequence = DOTween.Sequence();
            
            // 脉冲发光 - 使用SetAutoKill(false)
            if (fillImage != null)
            {
                colorTween = fillImage.DOColor(
                    Color.Lerp(lowHealthColor, Color.white, 0.5f),
                    warningPulseSpeed
                ).SetEase(Ease.InOutSine)
                 .SetLoops(-1, LoopType.Yoyo)
                 .SetAutoKill(false);
                
                warningSequence.Append(colorTween);
            }
            
            // 震动
            warningSequence.Join(
                barTransform.DOShakeAnchorPos(
                    warningShakeDuration,
                    new Vector2(warningShakeAmount, 0f),
                    5,
                    90f,
                    false,
                    true
                ).SetLoops(-1)
                 .SetDelay(warningPulseSpeed * 0.5f)
                 .SetAutoKill(false)
            );
            
            // 警告覆盖层
            if (warningOverlay != null)
            {
                warningOverlay.gameObject.SetActive(true);
                warningSequence.Join(
                    warningOverlay.DOFade(0.3f, warningPulseSpeed)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetAutoKill(false)
                );
            }
        }
        
        private void StopWarningAnimation()
        {
            isWarningActive = false;
            warningSequence?.Kill();
            warningSequence = null;
            
            // 恢复颜色
            if (fillImage != null)
            {
                fillImage.DOColor(lowHealthColor, 0.2f).SetAutoKill(true);
            }
            
            // 停止震动
            barTransform.DOAnchorPos(Vector2.zero, 0.2f).SetAutoKill(true);
            
            // 隐藏警告层
            if (warningOverlay != null)
            {
                warningOverlay.DOFade(0f, 0.2f)
                    .SetAutoKill(true)
                    .OnComplete(() => warningOverlay.gameObject.SetActive(false));
            }
        }
        
        private void PlayDamageFeedback()
        {
            flashSequence?.Kill();
            
            flashSequence = DOTween.Sequence();
            
            // 红色闪烁
            if (fillImage != null)
            {
                Color originalColor = fillImage.color;
                flashSequence.Append(
                    fillImage.DOColor(damageFlashColor, damageFlashDuration * 0.5f).SetAutoKill(true)
                );
                flashSequence.Append(
                    fillImage.DOColor(originalColor, damageFlashDuration * 0.5f).SetAutoKill(true)
                );
            }
            
            // 震动
            flashSequence.Join(
                barTransform.DOShakeAnchorPos(
                    damageFlashDuration,
                    new Vector2(damageShakeAmount, 0f),
                    10,
                    90f
                ).SetAutoKill(true)
            );
        }
        
        private void PlayHealFeedback()
        {
            flashSequence?.Kill();
            
            flashSequence = DOTween.Sequence();
            
            // 绿色闪烁
            if (fillImage != null)
            {
                Color originalColor = fillImage.color;
                flashSequence.Append(
                    fillImage.DOColor(healFlashColor, healFlashDuration * 0.3f).SetAutoKill(true)
                );
                flashSequence.Append(
                    fillImage.DOColor(originalColor, healFlashDuration * 0.7f).SetAutoKill(true)
                );
            }
            
            // 发光效果
            flashSequence.Join(
                barTransform.DOScale(Vector3.one * 1.05f, 0.25f)
                    .SetEase(Ease.OutBack)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetAutoKill(true)
            );
        }
        
        public void PreviewDamage(float damageAmount, float maxValue = 1f)
        {
            if (delayedFillImage == null) return;
            
            float previewValue = Mathf.Clamp01((currentValue * maxValue - damageAmount) / maxValue);
            
            delayedFillImage.DOFillAmount(previewValue, 0.1f).SetAutoKill(true);
            delayedFillImage.color = new Color(0.8f, 0.2f, 0.2f, 0.5f);
        }
        
        public void CancelDamagePreview()
        {
            if (delayedFillImage == null) return;
            
            delayedFillImage.DOFillAmount(currentValue, 0.2f).SetAutoKill(true);
        }
        
        private void OnDestroy()
        {
            // 清理所有Tween
            warningSequence?.Kill();
            flashSequence?.Kill();
            valueTween?.Kill();
            delayedTween?.Kill();
            colorTween?.Kill();
        }
    }
}
