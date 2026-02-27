using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SebeJJ.UI.Animation
{
    /// <summary>
    /// 任务项动画 - 单个委托任务的动画效果
    /// </summary>
    public class QuestItemAnimator : MonoBehaviour
    {
        [Header("基础配置")]
        [SerializeField] private RectTransform itemTransform;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image glowImage;
        
        [Header("状态颜色")]
        [SerializeField] private Color availableColor = new Color(0.2f, 0.8f, 0.3f, 0.2f);
        [SerializeField] private Color inProgressColor = new Color(0.9f, 0.7f, 0.2f, 0.2f);
        [SerializeField] private Color completedColor = new Color(0.2f, 0.6f, 1f, 0.2f);
        [SerializeField] private Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        [SerializeField] private Color urgentColor = new Color(0.9f, 0.2f, 0.2f, 0.3f);
        
        [Header("悬停效果")]
        [SerializeField] private float hoverScale = 1.03f;
        [SerializeField] private float hoverDuration = 0.15f;
        [SerializeField] private float hoverLift = 3f;
        [SerializeField] private Color hoverGlowColor = new Color(0.3f, 0.9f, 1f, 0.4f);
        
        [Header("选中效果")]
        [SerializeField] private float selectedScale = 1.05f;
        [SerializeField] private float selectedBorderWidth = 2f;
        [SerializeField] private Color selectedBorderColor = new Color(0f, 1f, 0.8f, 1f);
        [SerializeField] private float pulseDuration = 1.5f;
        
        [Header("紧急任务效果")]
        [SerializeField] private float urgentPulseSpeed = 0.5f;
        [SerializeField] private float urgentShakeAmount = 2f;
        
        private Vector3 originalScale;
        private Vector2 originalPosition;
        private Color originalColor;
        private Sequence currentSequence;
        private bool isSelected;
        private bool isUrgent;
        private Tween urgentTween;
        
        private void Awake()
        {
            if (itemTransform == null)
                itemTransform = GetComponent<RectTransform>();
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
                
            originalScale = itemTransform.localScale;
            originalPosition = itemTransform.anchoredPosition;
            if (backgroundImage != null)
                originalColor = backgroundImage.color;
        }
        
        /// <summary>
        /// 播放进入动画
        /// </summary>
        public void PlayEnterAnimation(float delay, float slideDistance, float duration)
        {
            // 初始状态
            itemTransform.anchoredPosition = new Vector2(originalPosition.x + slideDistance, originalPosition.y);
            itemTransform.localScale = Vector3.one * 0.9f;
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
            
            // 延迟后播放
            DOVirtual.DelayedCall(delay, () =>
            {
                currentSequence?.Kill();
                currentSequence = DOTween.Sequence();
                
                // 滑入
                currentSequence.Append(
                    itemTransform.DOAnchorPos(originalPosition, duration)
                        .SetEase(Ease.OutBack)
                );
                
                // 缩放
                currentSequence.Join(
                    itemTransform.DOScale(originalScale, duration)
                        .SetEase(Ease.OutBack)
                );
                
                // 淡入
                if (canvasGroup != null)
                {
                    currentSequence.Join(
                        canvasGroup.DOFade(1f, duration * 0.7f)
                    );
                }
                
                // 光效闪烁
                if (glowImage != null)
                {
                    currentSequence.Join(
                        glowImage.DOFade(0.5f, duration * 0.3f)
                            .SetLoops(2, LoopType.Yoyo)
                    );
                }
            });
        }
        
        /// <summary>
        /// 播放退出动画
        /// </summary>
        public void PlayExitAnimation(float delay, float duration)
        {
            DOVirtual.DelayedCall(delay, () =>
            {
                currentSequence?.Kill();
                currentSequence = DOTween.Sequence();
                
                currentSequence.Append(
                    itemTransform.DOAnchorPosX(originalPosition.x - 100f, duration)
                        .SetEase(Ease.InBack)
                );
                
                currentSequence.Join(
                    itemTransform.DOScale(Vector3.one * 0.9f, duration)
                );
                
                if (canvasGroup != null)
                {
                    currentSequence.Join(
                        canvasGroup.DOFade(0f, duration)
                    );
                }
            });
        }
        
        /// <summary>
        /// 悬停进入
        /// </summary>
        public void OnPointerEnter()
        {
            if (isSelected) return;
            
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();
            
            currentSequence.Append(
                itemTransform.DOScale(originalScale * hoverScale, hoverDuration)
                    .SetEase(Ease.OutBack)
            );
            
            currentSequence.Join(
                itemTransform.DOAnchorPosY(originalPosition.y + hoverLift, hoverDuration)
                    .SetEase(Ease.OutQuad)
            );
            
            if (glowImage != null)
            {
                currentSequence.Join(
                    glowImage.DOColor(hoverGlowColor, hoverDuration)
                );
            }
        }
        
        /// <summary>
        /// 悬停退出
        /// </summary>
        public void OnPointerExit()
        {
            if (isSelected) return;
            
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();
            
            currentSequence.Append(
                itemTransform.DOScale(originalScale, hoverDuration)
                    .SetEase(Ease.OutQuad)
            );
            
            currentSequence.Join(
                itemTransform.DOAnchorPos(originalPosition, hoverDuration)
                    .SetEase(Ease.OutQuad)
            );
            
            if (glowImage != null)
            {
                currentSequence.Join(
                    glowImage.DOColor(Color.clear, hoverDuration)
                );
            }
        }
        
        /// <summary>
        /// 设置选中状态
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            currentSequence?.Kill();
            
            if (selected)
            {
                currentSequence = DOTween.Sequence();
                
                // 放大
                currentSequence.Append(
                    itemTransform.DOScale(originalScale * selectedScale, 0.2f)
                        .SetEase(Ease.OutBack)
                );
                
                // 边框发光
                if (backgroundImage != null)
                {
                    currentSequence.Join(
                        backgroundImage.DOColor(selectedBorderColor, 0.2f)
                    );
                }
                
                // 持续脉冲
                currentSequence.Append(
                    itemTransform.DOScale(originalScale * selectedScale * 1.02f, pulseDuration * 0.5f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo)
                );
            }
            else
            {
                currentSequence = DOTween.Sequence();
                
                currentSequence.Append(
                    itemTransform.DOScale(originalScale, 0.2f)
                        .SetEase(Ease.OutQuad)
                );
                
                if (backgroundImage != null)
                {
                    currentSequence.Join(
                        backgroundImage.DOColor(originalColor, 0.2f)
                    );
                }
            }
        }
        
        /// <summary>
        /// 设置任务状态
        /// </summary>
        public void SetQuestStatus(QuestStatus status)
        {
            Color targetColor = originalColor;
            
            switch (status)
            {
                case QuestStatus.Available:
                    targetColor = availableColor;
                    break;
                case QuestStatus.InProgress:
                    targetColor = inProgressColor;
                    break;
                case QuestStatus.Completed:
                    targetColor = completedColor;
                    break;
                case QuestStatus.Locked:
                    targetColor = lockedColor;
                    break;
                case QuestStatus.Urgent:
                    targetColor = urgentColor;
                    SetUrgent(true);
                    break;
            }
            
            if (backgroundImage != null)
            {
                backgroundImage.DOColor(targetColor, 0.3f);
            }
            
            originalColor = targetColor;
        }
        
        /// <summary>
        /// 设置紧急状态
        /// </summary>
        public void SetUrgent(bool urgent)
        {
            isUrgent = urgent;
            urgentTween?.Kill();
            
            if (urgent)
            {
                // 红色脉冲
                if (glowImage != null)
                {
                    glowImage.color = urgentColor;
                    urgentTween = glowImage.DOFade(0.6f, urgentPulseSpeed)
                        .SetLoops(-1, LoopType.Yoyo);
                }
                
                // 轻微震动
                urgentTween = itemTransform.DOShakeAnchorPos(
                    urgentPulseSpeed * 2f,
                    new Vector2(urgentShakeAmount, 0f),
                    10,
                    90f,
                    false,
                    true
                ).SetLoops(-1);
            }
            else
            {
                if (glowImage != null)
                {
                    glowImage.DOFade(0f, 0.2f);
                }
                
                itemTransform.DOAnchorPos(originalPosition, 0.2f);
            }
        }
        
        /// <summary>
        /// 设置倾斜（滚动效果）
        /// </summary>
        public void SetTilt(float angle)
        {
            itemTransform.DORotate(new Vector3(0f, 0f, -angle), 0.1f)
                .SetEase(Ease.OutQuad);
        }
        
        /// <summary>
        /// 播放淡出
        /// </summary>
        public void PlayFadeOut(float duration)
        {
            if (canvasGroup != null)
            {
                canvasGroup.DOFade(0f, duration);
            }
        }
        
        /// <summary>
        /// 播放颜色变化
        /// </summary>
        public Tween PlayColorChange(Color targetColor, float duration)
        {
            if (backgroundImage != null)
            {
                return backgroundImage.DOColor(targetColor, duration);
            }
            return null;
        }
        
        /// <summary>
        /// 播放闪烁
        /// </summary>
        public Tween PlayFlash(int count, float interval)
        {
            if (canvasGroup == null) return null;
            
            return canvasGroup.DOFade(0.3f, interval)
                .SetLoops(count * 2, LoopType.Yoyo);
        }
        
        private void OnDestroy()
        {
            currentSequence?.Kill();
            urgentTween?.Kill();
        }
    }
    
    public enum QuestStatus
    {
        Available,    // 可接取
        InProgress,   // 进行中
        Completed,    // 已完成
        Locked,       // 锁定
        Urgent        // 紧急
    }
}
