using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SebeJJ.UI.Animation
{
    /// <summary>
    /// 警告提示动画 - 闪烁、震动
    /// </summary>
    public class WarningAlertAnimator : MonoBehaviour
    {
        [Header("基础配置")]
        [SerializeField] private RectTransform alertTransform;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Text alertText;
        
        [Header("闪烁效果")]
        [SerializeField] private float blinkInterval = 0.3f;
        [SerializeField] private float blinkFadeAmount = 0.3f;
        [SerializeField] private Color blinkColor1 = new Color(1f, 0.2f, 0.2f, 1f);
        [SerializeField] private Color blinkColor2 = new Color(1f, 0.5f, 0.2f, 1f);
        
        [Header("震动效果")]
        [SerializeField] private float shakeAmount = 5f;
        [SerializeField] private float shakeDuration = 0.1f;
        [SerializeField] private int shakeVibrato = 10;
        [SerializeField] private bool shakeRandomness = true;
        
        [Header("缩放脉冲")]
        [SerializeField] private float pulseScale = 1.1f;
        [SerializeField] private float pulseDuration = 0.4f;
        
        [Header("边框闪烁")]
        [SerializeField] private Image borderImage;
        [SerializeField] private float borderBlinkSpeed = 0.2f;
        [SerializeField] private Color borderWarningColor = new Color(1f, 0f, 0f, 1f);
        
        [Header("进入/退出")]
        [SerializeField] private float enterDuration = 0.3f;
        [SerializeField] private float exitDuration = 0.2f;
        [SerializeField] private float slideInDistance = 100f;
        
        [Header("严重警告")]
        [SerializeField] private float criticalShakeMultiplier = 2f;
        [SerializeField] private float criticalBlinkSpeed = 0.15f;
        [SerializeField] private Color criticalColor = new Color(1f, 0f, 0f, 1f);
        
        private Sequence alertSequence;
        private Sequence blinkSequence;
        private Sequence shakeSequence;
        private bool isCritical;
        
        private void Awake()
        {
            if (alertTransform == null)
                alertTransform = GetComponent<RectTransform>();
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
        }
        
        /// <summary>
        /// 显示警告
        /// </summary>
        public void ShowWarning(string message, bool critical = false)
        {
            isCritical = critical;
            
            if (alertText != null)
                alertText.text = message;
            
            gameObject.SetActive(true);
            
            // 进入动画
            PlayEnterAnimation();
            
            // 开始警告效果
            if (critical)
            {
                PlayCriticalWarning();
            }
            else
            {
                PlayNormalWarning();
            }
        }
        
        /// <summary>
        /// 隐藏警告
        /// </summary>
        public void HideWarning()
        {
            PlayExitAnimation();
        }
        
        /// <summary>
        /// 进入动画
        /// </summary>
        private void PlayEnterAnimation()
        {
            alertSequence?.Kill();
            alertSequence = DOTween.Sequence();
            
            // 初始状态
            Vector2 originalPos = alertTransform.anchoredPosition;
            alertTransform.anchoredPosition = new Vector2(originalPos.x + slideInDistance, originalPos.y);
            alertTransform.localScale = Vector3.one * 0.8f;
            
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
            
            // 滑入
            alertSequence.Append(
                alertTransform.DOAnchorPos(originalPos, enterDuration)
                    .SetEase(Ease.OutBack)
            );
            
            // 缩放
            alertSequence.Join(
                alertTransform.DOScale(Vector3.one, enterDuration)
                    .SetEase(Ease.OutBack)
            );
            
            // 淡入
            if (canvasGroup != null)
            {
                alertSequence.Join(
                    canvasGroup.DOFade(1f, enterDuration * 0.7f)
                );
            }
        }
        
        /// <summary>
        /// 退出动画
        /// </summary>
        private void PlayExitAnimation()
        {
            alertSequence?.Kill();
            blinkSequence?.Kill();
            shakeSequence?.Kill();
            
            alertSequence = DOTween.Sequence();
            
            // 滑出
            alertSequence.Append(
                alertTransform.DOAnchorPosX(
                    alertTransform.anchoredPosition.x + slideInDistance,
                    exitDuration
                ).SetEase(Ease.InBack)
            );
            
            // 淡出
            if (canvasGroup != null)
            {
                alertSequence.Join(
                    canvasGroup.DOFade(0f, exitDuration)
                );
            }
            
            alertSequence.OnComplete(() => gameObject.SetActive(false));
        }
        
        /// <summary>
        /// 普通警告效果
        /// </summary>
        private void PlayNormalWarning()
        {
            // 闪烁
            blinkSequence = DOTween.Sequence();
            
            if (backgroundImage != null)
            {
                blinkSequence.Append(
                    backgroundImage.DOColor(blinkColor1, blinkInterval)
                        .SetEase(Ease.InOutSine)
                );
                
                blinkSequence.Append(
                    backgroundImage.DOColor(blinkColor2, blinkInterval)
                        .SetEase(Ease.InOutSine)
                );
                
                blinkSequence.SetLoops(-1);
            }
            
            // 轻微震动
            shakeSequence = DOTween.Sequence();
            shakeSequence.AppendInterval(shakeDuration * 2f);
            shakeSequence.Append(
                alertTransform.DOShakeAnchorPos(
                    shakeDuration,
                    new Vector2(shakeAmount * 0.5f, 0f),
                    shakeVibrato / 2,
                    90f,
                    shakeRandomness
                )
            );
            shakeSequence.SetLoops(-1);
            
            // 缩放脉冲
            alertTransform.DOScale(pulseScale, pulseDuration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
        
        /// <summary>
        /// 严重警告效果
        /// </summary>
        private void PlayCriticalWarning()
        {
            // 快速闪烁
            blinkSequence = DOTween.Sequence();
            
            if (backgroundImage != null)
            {
                backgroundImage.color = criticalColor;
                
                blinkSequence.Append(
                    backgroundImage.DOFade(1f, criticalBlinkSpeed)
                );
                
                blinkSequence.Append(
                    backgroundImage.DOFade(blinkFadeAmount, criticalBlinkSpeed)
                );
                
                blinkSequence.SetLoops(-1);
            }
            
            // 边框闪烁
            if (borderImage != null)
            {
                borderImage.color = borderWarningColor;
                borderImage.DOFade(1f, borderBlinkSpeed)
                    .SetLoops(-1, LoopType.Yoyo);
            }
            
            // 强烈震动
            shakeSequence = DOTween.Sequence();
            shakeSequence.Append(
                alertTransform.DOShakeAnchorPos(
                    shakeDuration,
                    new Vector2(shakeAmount * criticalShakeMultiplier, shakeAmount * criticalShakeMultiplier),
                    shakeVibrato,
                    90f,
                    shakeRandomness
                )
            );
            shakeSequence.SetLoops(-1);
            
            // 快速脉冲
            alertTransform.DOScale(pulseScale * 1.1f, pulseDuration * 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
            
            // 图标旋转
            if (iconImage != null)
            {
                iconImage.transform.DORotate(new Vector3(0f, 0f, 15f), pulseDuration * 0.5f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }
            
            // 屏幕震动效果（通过相机）
            Camera.main.transform.DOShakePosition(
                0.5f,
                shakeAmount * 0.3f,
                10,
                90f
            ).SetLoops(-1);
        }
        
        /// <summary>
        /// 更新警告文本
        /// </summary>
        public void UpdateText(string message)
        {
            if (alertText != null)
            {
                alertText.text = message;
                
                // 文本变化时的闪烁
                alertText.DOFade(0.5f, 0.1f)
                    .SetLoops(2, LoopType.Yoyo);
            }
        }
        
        /// <summary>
        /// 设置警告级别
        /// </summary>
        public void SetWarningLevel(float level)
        {
            // 根据级别调整效果强度
            float intensity = Mathf.Clamp01(level);
            
            // 调整闪烁速度
            float speedMultiplier = 1f + intensity;
            
            // 调整震动强度
            float currentShake = shakeAmount * (1f + intensity);
            
            // 调整颜色强度
            if (backgroundImage != null)
            {
                backgroundImage.color = Color.Lerp(blinkColor1, criticalColor, intensity);
            }
        }
        
        private void OnDestroy()
        {
            alertSequence?.Kill();
            blinkSequence?.Kill();
            shakeSequence?.Kill();
            
            // 停止相机震动
            if (Camera.main != null)
                Camera.main.transform.DOKill();
        }
    }
}
