using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SebeJJ.UI.Animation
{
    /// <summary>
    /// 升级提示动画 - 光效、粒子
    /// </summary>
    public class LevelUpAnimator : MonoBehaviour
    {
        [Header("基础配置")]
        [SerializeField] private RectTransform levelUpPanel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image backgroundGlow;
        
        [Header("等级数字")]
        [SerializeField] private Text levelText;
        [SerializeField] private Text oldLevelText;
        [SerializeField] private Text newLevelText;
        [SerializeField] private float numberCountDuration = 1f;
        
        [Header("光效")]
        [SerializeField] private Image lightRayImage;
        [SerializeField] private float lightRayRotationSpeed = 180f;
        [SerializeField] private float lightRayPulseSpeed = 0.5f;
        [SerializeField] private float lightRayMaxAlpha = 0.6f;
        
        [Header("粒子效果")]
        [SerializeField] private ParticleSystem levelUpParticles;
        [SerializeField] private ParticleSystem sparkleParticles;
        [SerializeField] private float particleEmissionMultiplier = 2f;
        
        [Header("属性提升")]
        [SerializeField] private RectTransform statsContainer;
        [SerializeField] private float statStaggerDelay = 0.1f;
        [SerializeField] private float statSlideDuration = 0.3f;
        [SerializeField] private float statGlowDuration = 0.5f;
        
        [Header("升级环")]
        [SerializeField] private Image levelRingImage;
        [SerializeField] private float ringFillDuration = 1.5f;
        [SerializeField] private float ringRotationSpeed = 100f;
        
        [Header("震动效果")]
        [SerializeField] private float screenShakeDuration = 0.5f;
        [SerializeField] private float screenShakeAmount = 10f;
        
        private Sequence levelUpSequence;
        
        private void Awake()
        {
            if (levelUpPanel == null)
                levelUpPanel = GetComponent<RectTransform>();
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
                
            // 初始隐藏
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }
        
        /// <summary>
        /// 播放升级动画
        /// </summary>
        public void PlayLevelUpAnimation(int oldLevel, int newLevel, System.Action onComplete = null)
        {
            gameObject.SetActive(true);
            
            levelUpSequence?.Kill();
            levelUpSequence = DOTween.Sequence();
            
            // 阶段1：背景淡入
            if (canvasGroup != null)
            {
                levelUpSequence.Append(
                    canvasGroup.DOFade(1f, 0.3f)
                );
            }
            
            // 阶段2：光效旋转
            if (lightRayImage != null)
            {
                lightRayImage.color = new Color(1f, 1f, 1f, 0f);
                
                // 旋转
                lightRayImage.transform.DORotate(
                    new Vector3(0f, 0f, 360f),
                    360f / lightRayRotationSpeed,
                    RotateMode.FastBeyond360
                ).SetEase(Ease.Linear)
                 .SetLoops(-1);
                
                // 脉冲
                levelUpSequence.Join(
                    lightRayImage.DOFade(lightRayMaxAlpha, lightRayPulseSpeed)
                        .SetLoops(-1, LoopType.Yoyo)
                        .SetEase(Ease.InOutSine)
                );
            }
            
            // 阶段3：升级环填充
            if (levelRingImage != null)
            {
                levelRingImage.fillAmount = 0f;
                levelUpSequence.Join(
                    levelRingImage.DOFillAmount(1f, ringFillDuration)
                        .SetEase(Ease.OutQuad)
                );
                
                // 环旋转
                levelRingImage.transform.DORotate(
                    new Vector3(0f, 0f, -360f),
                    360f / ringRotationSpeed,
                    RotateMode.FastBeyond360
                ).SetEase(Ease.Linear)
                 .SetLoops(-1);
            }
            
            // 阶段4：等级数字变化
            if (oldLevelText != null && newLevelText != null)
            {
                oldLevelText.text = oldLevel.ToString();
                newLevelText.text = newLevel.ToString();
                
                oldLevelText.transform.localScale = Vector3.one;
                newLevelText.transform.localScale = Vector3.zero;
                
                // 旧等级缩小消失
                levelUpSequence.Append(
                    oldLevelText.transform.DOScale(Vector3.zero, 0.3f)
                        .SetEase(Ease.InBack)
                        .SetDelay(ringFillDuration * 0.7f)
                );
                
                // 新等级放大出现
                levelUpSequence.Append(
                    newLevelText.transform.DOScale(1.5f, 0.4f)
                        .SetEase(Ease.OutBack)
                );
                
                levelUpSequence.Append(
                    newLevelText.transform.DOScale(1f, 0.2f)
                        .SetEase(Ease.OutQuad)
                );
            }
            
            // 阶段5：屏幕震动
            levelUpSequence.AppendCallback(() =>
            {
                Camera.main.transform.DOShakePosition(
                    screenShakeDuration,
                    screenShakeAmount,
                    20,
                    90f
                );
            });
            
            // 阶段6：粒子爆发
            levelUpSequence.AppendCallback(() => PlayParticles());
            
            // 阶段7：属性提升展示
            levelUpSequence.AppendInterval(0.3f);
            AnimateStats();
            
            // 完成回调
            levelUpSequence.OnComplete(() =>
            {
                onComplete?.Invoke();
                // 自动关闭
                DOVirtual.DelayedCall(2f, Close);
            });
        }
        
        /// <summary>
        /// 播放粒子效果
        /// </summary>
        private void PlayParticles()
        {
            if (levelUpParticles != null)
            {
                var emission = levelUpParticles.emission;
                emission.rateOverTime = emission.rateOverTime.constant * particleEmissionMultiplier;
                levelUpParticles.Play();
            }
            
            if (sparkleParticles != null)
            {
                sparkleParticles.Play();
            }
        }
        
        /// <summary>
        /// 属性提升动画
        /// </summary>
        private void AnimateStats()
        {
            if (statsContainer == null) return;
            
            var statItems = statsContainer.GetComponentsInChildren<RectTransform>();
            
            for (int i = 0; i < statItems.Length; i++)
            {
                var stat = statItems[i];
                if (stat == statsContainer) continue;
                
                float delay = i * statStaggerDelay;
                Vector2 originalPos = stat.anchoredPosition;
                
                DOVirtual.DelayedCall(delay, () =
                {
                    // 滑入
                    stat.anchoredPosition = new Vector2(-100f, originalPos.y);
                    stat.DOAnchorPos(originalPos, statSlideDuration)
                        .SetEase(Ease.OutBack);
                    
                    // 缩放
                    stat.localScale = Vector3.zero;
                    stat.DOScale(Vector3.one, statSlideDuration)
                        .SetEase(Ease.OutBack);
                    
                    // 发光
                    Image statImage = stat.GetComponent<Image>();
                    if (statImage != null)
                    {
                        Color originalColor = statImage.color;
                        statImage.DOColor(Color.white, statGlowDuration * 0.3f)
                            .SetLoops(2, LoopType.Yoyo)
                            .OnComplete(() => statImage.color = originalColor);
                    }
                    
                    // 文本闪烁
                    Text statText = stat.GetComponentInChildren<Text>();
                    if (statText != null)
                    {
                        statText.DOFade(0.3f, statGlowDuration * 0.2f)
                            .SetLoops(3, LoopType.Yoyo);
                    }
                });
            }
        }
        
        /// <summary>
        /// 关闭升级面板
        /// </summary>
        public void Close()
        {
            levelUpSequence?.Kill();
            
            Sequence closeSeq = DOTween.Sequence();
            
            // 停止旋转
            if (lightRayImage != null)
                lightRayImage.transform.DOKill();
            if (levelRingImage != null)
                levelRingImage.transform.DOKill();
            
            // 淡出
            if (canvasGroup != null)
            {
                closeSeq.Append(
                    canvasGroup.DOFade(0f, 0.5f)
                );
            }
            
            // 缩放退出
            closeSeq.Join(
                levelUpPanel.DOScale(Vector3.zero, 0.4f)
                    .SetEase(Ease.InBack)
            );
            
            closeSeq.OnComplete(() =>
            {
                gameObject.SetActive(false);
                levelUpPanel.localScale = Vector3.one;
            });
        }
        
        private void OnDestroy()
        {
            levelUpSequence?.Kill();
        }
    }
}
