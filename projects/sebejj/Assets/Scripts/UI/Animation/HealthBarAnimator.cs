using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SebeJJ.UI.Animation
{
    /// <summary>
    /// 血条动画控制器 - 平滑过渡、低血量警告
    /// </summary>
    public class HealthBarAnimator : MonoBehaviour
    {
        [Header("基础配置")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image fillImage;
        [SerializeField] private Image delayedFillImage; // 延迟血条（显示伤害预览）
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
        [SerializeField] private float warningGlowIntensity = 1.5f;
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
        [SerializeField] private float healGlowDuration = 0.5f;
        
        private Sequence warningSequence;
        private Sequence flashSequence;
        private Tween valueTween;
        private Tween delayedTween;
        private bool isWarningActive;
        private float currentValue;
        
        private void Awake()
        {
            if (healthSlider == null)
                healthSlider = GetComponent<Slider>();
            if (fillImage == null && healthSlider != null)
                fillImage = healthSlider.fillRect.GetComponent<Image>();
            if (barTransform == null)
                barTransform = GetComponent<RectTransform>();
                
            currentValue = healthSlider?.value ?? 1f;
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
            valueTween?.Kill();
            delayedTween?.Kill();
            
            // 主血条动画
            if (healthSlider != null)
            {
                valueTween = DOTween.To(() => healthSlider.value, x => healthSlider.value = x, 
                    normalizedValue, valueChangeDuration)
                    .SetEase(valueChangeEase)
                    .OnUpdate(() => UpdateColor(healthSlider.value));
            }
            
            // 延迟血条动画（如果存在）
            if (delayedFillImage != null)
            {
                delayedTween = DOVirtual.DelayedCall(delayedFillDelay, () =
                {
                    delayedFillImage.DOFillAmount(normalizedValue, delayedFillDuration)
                        .SetEase(Ease.OutQuad);
                });
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
            
            valueTween?.Kill();
            delayedTween?.Kill();
            
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
        
        /// <summary>
        /// 更新颜色
        /// </summary>
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
            
            fillImage.color = targetColor;
        }
        
        /// <summary>
        /// 检查低血量警告
        /// </summary>
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
        
        /// <summary>
        /// 开始警告动画
        /// </summary>
        private void StartWarningAnimation()
        {
            isWarningActive = true;
            warningSequence?.Kill();
            warningSequence = DOTween.Sequence();
            
            // 脉冲发光
            if (fillImage != null)
            {
                warningSequence.Append(
                    fillImage.DOColor(
                        Color.Lerp(lowHealthColor, Color.white, 0.5f),
                        warningPulseSpeed
                    ).SetEase(Ease.InOutSine)
                     .SetLoops(-1, LoopType.Yoyo)
                );
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
            );
            
            // 警告覆盖层闪烁
            if (warningOverlay != null)
            {
                warningOverlay.gameObject.SetActive(true);
                warningSequence.Join(
                    warningOverlay.DOFade(0.3f, warningPulseSpeed)
                        .SetLoops(-1, LoopType.Yoyo)
                );
            }
        }
        
        /// <summary>
        /// 停止警告动画
        /// </summary>
        private void StopWarningAnimation()
        {
            isWarningActive = false;
            warningSequence?.Kill();
            
            // 恢复颜色
            if (fillImage != null)
            {
                fillImage.DOColor(lowHealthColor, 0.2f);
            }
            
            // 停止震动
            barTransform.DOAnchorPos(Vector2.zero, 0.2f);
            
            // 隐藏警告层
            if (warningOverlay != null)
            {
                warningOverlay.DOFade(0f, 0.2f)
                    .OnComplete(() => warningOverlay.gameObject.SetActive(false));
            }
        }
        
        /// <summary>
        /// 播放受伤反馈
        /// </summary>
        private void PlayDamageFeedback()
        {
            flashSequence?.Kill();
            flashSequence = DOTween.Sequence();
            
            // 红色闪烁
            if (fillImage != null)
            {
                Color originalColor = fillImage.color;
                flashSequence.Append(
                    fillImage.DOColor(damageFlashColor, damageFlashDuration * 0.5f)
                );
                flashSequence.Append(
                    fillImage.DOColor(originalColor, damageFlashDuration * 0.5f)
                );
            }
            
            // 震动
            flashSequence.Join(
                barTransform.DOShakeAnchorPos(
                    damageFlashDuration,
                    new Vector2(damageShakeAmount, 0f),
                    10,
                    90f
                )
            );
        }
        
        /// <summary>
        /// 播放治疗反馈
        /// </summary>
        private void PlayHealFeedback()
        {
            flashSequence?.Kill();
            flashSequence = DOTween.Sequence();
            
            // 绿色闪烁
            if (fillImage != null)
            {
                Color originalColor = fillImage.color;
                flashSequence.Append(
                    fillImage.DOColor(healFlashColor, healFlashDuration * 0.3f)
                );
                flashSequence.Append(
                    fillImage.DOColor(originalColor, healFlashDuration * 0.7f)
                );
            }
            
            // 发光效果
            flashSequence.Join(
                barTransform.DOScale(Vector3.one * 1.05f, healGlowDuration * 0.5f)
                    .SetEase(Ease.OutBack)
                    .SetLoops(2, LoopType.Yoyo)
            );
        }
        
        /// <summary>
        /// 预览伤害（显示延迟血条）
        /// </summary>
        public void PreviewDamage(float damageAmount, float maxValue = 1f)
        {
            if (delayedFillImage == null) return;
            
            float previewValue = Mathf.Clamp01((currentValue * maxValue - damageAmount) / maxValue);
            
            // 延迟血条立即显示预览值
            delayedFillImage.DOFillAmount(previewValue, 0.1f);
            delayedFillImage.color = new Color(0.8f, 0.2f, 0.2f, 0.5f);
        }
        
        /// <summary>
        /// 取消伤害预览
        /// </summary>
        public void CancelDamagePreview()
        {
            if (delayedFillImage == null) return;
            
            delayedFillImage.DOFillAmount(currentValue, 0.2f);
        }
        
        private void OnDestroy()
        {
            warningSequence?.Kill();
            flashSequence?.Kill();
            valueTween?.Kill();
            delayedTween?.Kill();
        }
    }
}
