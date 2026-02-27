using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SebeJJ.UI.Animation
{
    /// <summary>
    /// 背包物品槽动画 - 单个物品槽的动画效果
    /// </summary>
    public class InventorySlotAnimator : MonoBehaviour
    {
        [Header("基础配置")]
        [SerializeField] private RectTransform slotTransform;
        [SerializeField] private Image slotImage;
        [SerializeField] private Image itemImage;
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("进入动画")]
        [SerializeField] private float enterScaleStart = 0f;
        [SerializeField] private float enterScaleEnd = 1f;
        [SerializeField] private float enterOvershoot = 1.1f;
        
        [Header("悬停效果")]
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float hoverDuration = 0.15f;
        [SerializeField] private float hoverLift = 5f;
        [SerializeField] private Color hoverGlowColor = new Color(0.3f, 0.9f, 1f, 0.5f);
        
        [Header("选中效果")]
        [SerializeField] private float selectedScale = 1.15f;
        [SerializeField] private float selectedBorderWidth = 3f;
        [SerializeField] private Color selectedBorderColor = new Color(0f, 1f, 0.8f, 1f);
        
        [Header("拖拽效果")]
        [SerializeField] private float dragScale = 1.2f;
        [SerializeField] private float dragLift = 20f;
        [SerializeField] private float dragAlpha = 0.85f;
        
        [Header("稀有度光效")]
        [SerializeField] private Image rarityGlowImage;
        [SerializeField] private Color[] rarityColors = new Color[]
        {
            new Color(0.7f, 0.7f, 0.7f, 0.3f),  // 普通
            new Color(0.2f, 0.8f, 0.2f, 0.4f),  // 稀有
            new Color(0.2f, 0.4f, 1f, 0.5f),    // 史诗
            new Color(0.8f, 0.2f, 0.8f, 0.6f),  // 传说
            new Color(1f, 0.6f, 0.1f, 0.7f)     // 神话
        };
        
        private Vector3 originalScale;
        private Vector2 originalPosition;
        private Color originalColor;
        private Sequence currentSequence;
        private bool isSelected;
        private bool isDragging;
        private int currentRarity;
        
        private void Awake()
        {
            if (slotTransform == null)
                slotTransform = GetComponent<RectTransform>();
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
                
            originalScale = slotTransform.localScale;
            originalPosition = slotTransform.anchoredPosition;
            if (slotImage != null)
                originalColor = slotImage.color;
        }
        
        /// <summary>
        /// 播放进入动画
        /// </summary>
        public Tween PlayEnterAnimation(float duration)
        {
            currentSequence?.Kill();
            
            // 重置状态
            slotTransform.localScale = Vector3.one * enterScaleStart;
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
            
            Sequence seq = DOTween.Sequence();
            
            // 淡入
            if (canvasGroup != null)
            {
                seq.Join(
                    canvasGroup.DOFade(1f, duration * 0.5f)
                );
            }
            
            // 缩放弹出（带过冲）
            seq.Append(
                slotTransform.DOScale(originalScale * enterOvershoot, duration * 0.6f)
                    .SetEase(Ease.OutQuad)
            );
            
            seq.Append(
                slotTransform.DOScale(originalScale, duration * 0.4f)
                    .SetEase(Ease.OutBack)
            );
            
            // 稀有度光效闪烁
            if (rarityGlowImage != null && currentRarity > 0)
            {
                seq.Join(
                    rarityGlowImage.DOFade(rarityColors[currentRarity].a * 1.5f, duration * 0.3f)
                        .SetLoops(2, LoopType.Yoyo)
                );
            }
            
            return seq;
        }
        
        /// <summary>
        /// 播放退出动画
        /// </summary>
        public Tween PlayExitAnimation(float duration)
        {
            currentSequence?.Kill();
            
            Sequence seq = DOTween.Sequence();
            
            seq.Append(
                slotTransform.DOScale(Vector3.zero, duration)
                    .SetEase(Ease.InBack)
            );
            
            if (canvasGroup != null)
            {
                seq.Join(
                    canvasGroup.DOFade(0f, duration)
                );
            }
            
            return seq;
        }
        
        /// <summary>
        /// 悬停进入
        /// </summary>
        public void OnPointerEnter()
        {
            if (isDragging) return;
            
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();
            
            // 放大并上浮
            currentSequence.Append(
                slotTransform.DOScale(originalScale * hoverScale, hoverDuration)
                    .SetEase(Ease.OutBack)
            );
            
            currentSequence.Join(
                slotTransform.DOAnchorPosY(originalPosition.y + hoverLift, hoverDuration)
                    .SetEase(Ease.OutQuad)
            );
            
            // 光效
            if (rarityGlowImage != null)
            {
                currentSequence.Join(
                    rarityGlowImage.DOFade(rarityColors[currentRarity].a * 1.3f, hoverDuration)
                );
            }
        }
        
        /// <summary>
        /// 悬停退出
        /// </summary>
        public void OnPointerExit()
        {
            if (isDragging || isSelected) return;
            
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();
            
            currentSequence.Append(
                slotTransform.DOScale(originalScale, hoverDuration)
                    .SetEase(Ease.OutQuad)
            );
            
            currentSequence.Join(
                slotTransform.DOAnchorPos(originalPosition, hoverDuration)
                    .SetEase(Ease.OutQuad)
            );
            
            if (rarityGlowImage != null)
            {
                currentSequence.Join(
                    rarityGlowImage.DOFade(rarityColors[currentRarity].a, hoverDuration)
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
            currentSequence = DOTween.Sequence();
            
            if (selected)
            {
                // 选中放大
                currentSequence.Append(
                    slotTransform.DOScale(originalScale * selectedScale, 0.15f)
                        .SetEase(Ease.OutBack)
                );
                
                // 边框高亮
                if (slotImage != null)
                {
                    // 这里假设使用Outline组件或自定义Shader
                    // 简化处理：改变颜色
                    currentSequence.Join(
                        slotImage.DOColor(selectedBorderColor, 0.15f)
                    );
                }
            }
            else
            {
                currentSequence.Append(
                    slotTransform.DOScale(originalScale, 0.15f)
                        .SetEase(Ease.OutQuad)
                );
                
                if (slotImage != null)
                {
                    currentSequence.Join(
                        slotImage.DOColor(originalColor, 0.15f)
                    );
                }
            }
        }
        
        /// <summary>
        /// 开始拖拽
        /// </summary>
        public void OnBeginDrag()
        {
            isDragging = true;
            currentSequence?.Kill();
            
            // 放大并提升
            slotTransform.DOScale(originalScale * dragScale, 0.1f)
                .SetEase(Ease.OutBack);
            
            slotTransform.DOAnchorPosY(originalPosition.y + dragLift, 0.1f)
                .SetEase(Ease.OutQuad);
            
            if (canvasGroup != null)
            {
                canvasGroup.DOFade(dragAlpha, 0.1f);
            }
            
            // 添加旋转效果
            slotTransform.DORotate(new Vector3(0f, 0f, 5f), 0.1f)
                .SetEase(Ease.OutQuad);
        }
        
        /// <summary>
        /// 结束拖拽
        /// </summary>
        public void OnEndDrag()
        {
            isDragging = false;
            
            // 恢复状态
            slotTransform.DOScale(isSelected ? originalScale * selectedScale : originalScale, 0.2f)
                .SetEase(Ease.OutBack);
            
            slotTransform.DOAnchorPos(originalPosition, 0.2f)
                .SetEase(Ease.OutBack);
            
            if (canvasGroup != null)
            {
                canvasGroup.DOFade(1f, 0.2f);
            }
            
            slotTransform.DORotate(Vector3.zero, 0.2f)
                .SetEase(Ease.OutBack);
        }
        
        /// <summary>
        /// 设置稀有度
        /// </summary>
        public void SetRarity(int rarity)
        {
            currentRarity = Mathf.Clamp(rarity, 0, rarityColors.Length - 1);
            
            if (rarityGlowImage != null)
            {
                rarityGlowImage.color = rarityColors[currentRarity];
                rarityGlowImage.DOFade(rarityColors[currentRarity].a, 0.3f);
            }
        }
        
        /// <summary>
        /// 播放使用动画
        /// </summary>
        public void PlayUseAnimation()
        {
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();
            
            // 缩小消失
            currentSequence.Append(
                slotTransform.DOScale(Vector3.zero, 0.2f)
                    .SetEase(Ease.InBack)
            );
            
            // 闪烁效果
            if (itemImage != null)
            {
                currentSequence.Join(
                    itemImage.DOFade(0f, 0.1f)
                        .SetLoops(3, LoopType.Yoyo)
                );
            }
            
            // 恢复
            currentSequence.Append(
                slotTransform.DOScale(originalScale, 0.3f)
                    .SetEase(Ease.OutBack)
            );
        }
        
        private void OnDestroy()
        {
            currentSequence?.Kill();
        }
    }
}
