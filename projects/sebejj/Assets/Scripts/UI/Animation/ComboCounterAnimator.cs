using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SebeJJ.UI.Animation
{
    /// <summary>
    /// 连击计数动画 - 缩放、颜色变化
    /// </summary>
    public class ComboCounterAnimator : MonoBehaviour
    {
        [Header("基础配置")]
        [SerializeField] private RectTransform counterTransform;
        [SerializeField] private Text comboText;
        [SerializeField] private Text comboLabel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image glowImage;
        
        [Header("缩放动画")]
        [SerializeField] private float baseScale = 1f;
        [SerializeField] private float maxScale = 2f;
        [SerializeField] private float scalePerCombo = 0.1f;
        [SerializeField] private float scalePopDuration = 0.15f;
        [SerializeField] private float scaleBounceAmount = 1.3f;
        
        [Header("颜色变化")]
        [SerializeField] private Color[] comboColors = new Color[]
        {
            new Color(1f, 1f, 1f, 1f),        // 0-5: 白色
            new Color(1f, 0.9f, 0.3f, 1f),    // 6-10: 黄色
            new Color(1f, 0.5f, 0.1f, 1f),    // 11-20: 橙色
            new Color(1f, 0.2f, 0.2f, 1f),    // 21-30: 红色
            new Color(0.8f, 0.2f, 0.8f, 1f),  // 31-50: 紫色
            new Color(0.2f, 0.8f, 1f, 1f)     // 50+: 青色
        };
        [SerializeField] private int[] comboThresholds = new int[] { 0, 6, 11, 21, 31, 50 };
        [SerializeField] private float colorChangeDuration = 0.2f;
        
        [Header("连击维持")]
        [SerializeField] private float comboTimeout = 3f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private float pulseSpeed = 0.3f;
        
        [Header("特殊效果")]
        [SerializeField] private float shakeAmount = 5f;
        [SerializeField] private float rotationAmount = 10f;
        [SerializeField] private ParticleSystem comboParticles;
        
        private Sequence currentSequence;
        private Sequence timeoutSequence;
        private int currentCombo = 0;
        private bool isVisible = false;
        
        private void Awake()
        {
            if (counterTransform == null)
                counterTransform = GetComponent<RectTransform>();
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            if (comboText == null)
                comboText = GetComponentInChildren<Text>();
                
            // 初始隐藏
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }
        
        /// <summary>
        /// 增加连击数
        /// </summary>
        public void AddCombo(int amount = 1)
        {
            currentCombo += amount;
            
            if (!isVisible)
            {
                Show();
            }
            
            UpdateDisplay();
            PlayComboAnimation();
            ResetTimeout();
        }
        
        /// <summary>
        /// 重置连击
        /// </summary>
        public void ResetCombo()
        {
            if (currentCombo == 0) return;
            
            // 播放断开动画
            PlayBreakAnimation();
            
            currentCombo = 0;
        }
        
        /// <summary>
        /// 显示连击计数器
        /// </summary>
        private void Show()
        {
            isVisible = true;
            
            if (canvasGroup != null)
            {
                canvasGroup.DOFade(1f, 0.2f);
            }
            
            // 缩放进入
            counterTransform.localScale = Vector3.zero;
            counterTransform.DOScale(baseScale, 0.3f)
                .SetEase(Ease.OutBack);
        }
        
        /// <summary>
        /// 隐藏连击计数器
        /// </summary>
        private void Hide()
        {
            isVisible = false;
            
            if (canvasGroup != null)
            {
                canvasGroup.DOFade(0f, fadeOutDuration)
                    .OnComplete(() =>
                    {
                        currentCombo = 0;
                        counterTransform.localScale = Vector3.one * baseScale;
                    });
            }
        }
        
        /// <summary>
        /// 更新显示
        /// </summary>
        private void UpdateDisplay()
        {
            if (comboText != null)
            {
                comboText.text = currentCombo.ToString();
            }
            
            // 更新颜色
            Color targetColor = GetComboColor();
            if (comboText != null)
            {
                comboText.DOColor(targetColor, colorChangeDuration);
            }
            
            if (glowImage != null)
            {
                glowImage.DOColor(targetColor * 0.5f, colorChangeDuration);
            }
        }
        
        /// <summary>
        /// 播放连击动画
        /// </summary>
        private void PlayComboAnimation()
        {
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();
            
            // 计算目标缩放
            float targetScale = Mathf.Min(baseScale + currentCombo * scalePerCombo, maxScale);
            
            // 弹跳放大
            currentSequence.Append(
                counterTransform.DOScale(targetScale * scaleBounceAmount, scalePopDuration * 0.6f)
                    .SetEase(Ease.OutQuad)
            );
            
            currentSequence.Append(
                counterTransform.DOScale(targetScale, scalePopDuration * 0.4f)
                    .SetEase(Ease.OutBack)
            );
            
            // 高连击震动
            if (currentCombo >= 10)
            {
                currentSequence.Join(
                    counterTransform.DOShakeAnchorPos(
                        scalePopDuration,
                        new Vector2(shakeAmount * (currentCombo / 10f), 0f),
                        10,
                        90f
                    )
                );
            }
            
            // 旋转效果
            if (currentCombo >= 20)
            {
                currentSequence.Join(
                    counterTransform.DORotate(
                        new Vector3(0f, 0f, rotationAmount),
                        scalePopDuration * 0.5f
                    ).SetEase(Ease.OutBack)
                     .SetLoops(2, LoopType.Yoyo)
                );
            }
            
            // 发光脉冲
            if (glowImage != null)
            {
                currentSequence.Join(
                    glowImage.DOFade(0.8f, pulseSpeed)
                        .SetLoops(2, LoopType.Yoyo)
                );
            }
            
            // 粒子效果
            if (comboParticles != null && currentCombo % 10 == 0)
            {
                comboParticles.Play();
            }
        }
        
        /// <summary>
        /// 播放连击断开动画
        /// </summary>
        private void PlayBreakAnimation()
        {
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();
            
            // 缩小并淡出
            currentSequence.Append(
                counterTransform.DOScale(Vector3.zero, fadeOutDuration)
                    .SetEase(Ease.InBack)
            );
            
            if (canvasGroup != null)
            {
                currentSequence.Join(
                    canvasGroup.DOFade(0f, fadeOutDuration)
                );
            }
            
            // 颜色变灰
            if (comboText != null)
            {
                currentSequence.Join(
                    comboText.DOColor(Color.gray, fadeOutDuration)
                );
            }
            
            currentSequence.OnComplete(() =>
            {
                isVisible = false;
                currentCombo = 0;
            });
        }
        
        /// <summary>
        /// 重置超时计时
        /// </summary>
        private void ResetTimeout()
        {
            timeoutSequence?.Kill();
            
            // 超时后重置连击
            timeoutSequence = DOTween.Sequence();
            timeoutSequence.AppendInterval(comboTimeout);
            timeoutSequence.OnComplete(() => ResetCombo());
        }
        
        /// <summary>
        /// 获取连击颜色
        /// </summary>
        private Color GetComboColor()
        {
            for (int i = comboThresholds.Length - 1; i >= 0; i--)
            {
                if (currentCombo >= comboThresholds[i])
                {
                    return comboColors[Mathf.Min(i, comboColors.Length - 1)];
                }
            }
            return comboColors[0];
        }
        
        /// <summary>
        /// 获取当前连击数
        /// </summary>
        public int GetCurrentCombo()
        {
            return currentCombo;
        }
        
        private void OnDestroy()
        {
            currentSequence?.Kill();
            timeoutSequence?.Kill();
        }
    }
}
