using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SebeJJ.UI.Animation
{
    /// <summary>
    /// 背包界面动画控制器 - 处理打开/关闭动画和物品交互效果
    /// </summary>
    public class InventoryAnimator : MonoBehaviour
    {
        [Header("面板配置")]
        [SerializeField] private RectTransform inventoryPanel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform contentPanel;
        
        [Header("打开动画")]
        [SerializeField] private float openDuration = 0.4f;
        [SerializeField] private Ease openEase = Ease.OutBack;
        [SerializeField] private float openScaleStart = 0.8f;
        [SerializeField] private float openFadeDelay = 0.1f;
        
        [Header("关闭动画")]
        [SerializeField] private float closeDuration = 0.3f;
        [SerializeField] private Ease closeEase = Ease.InBack;
        [SerializeField] private float closeScaleEnd = 0.9f;
        
        [Header("物品槽动画")]
        [SerializeField] private float slotStaggerDelay = 0.03f;
        [SerializeField] private float slotPopDuration = 0.2f;
        [SerializeField] private float slotHoverScale = 1.1f;
        
        [Header("拖拽效果")]
        [SerializeField] private float dragScale = 1.15f;
        [SerializeField] private float dragLiftAmount = 10f;
        [SerializeField] private float dragAlpha = 0.8f;
        
        [Header("分类切换")]
        [SerializeField] private float categorySwitchDuration = 0.25f;
        [SerializeField] private float categorySlideDistance = 50f;
        
        private Sequence currentSequence;
        private Vector3 originalScale;
        private bool isOpen;
        
        public bool IsOpen => isOpen;
        
        public event Action OnOpenComplete;
        public event Action OnCloseComplete;
        
        private void Awake()
        {
            if (inventoryPanel == null)
                inventoryPanel = GetComponent<RectTransform>();
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
                
            originalScale = inventoryPanel.localScale;
            
            // 初始状态为关闭
            inventoryPanel.localScale = Vector3.zero;
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }
        
        /// <summary>
        /// 打开背包界面
        /// </summary>
        public void Open()
        {
            if (isOpen) return;
            isOpen = true;
            
            gameObject.SetActive(true);
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();
            
            // 缩放进入
            inventoryPanel.localScale = Vector3.one * openScaleStart;
            currentSequence.Append(
                inventoryPanel.DOScale(originalScale, openDuration)
                    .SetEase(openEase)
            );
            
            // 淡入
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                currentSequence.Join(
                    canvasGroup.DOFade(1f, openDuration - openFadeDelay)
                        .SetDelay(openFadeDelay)
                        .SetEase(Ease.OutQuad)
                );
            }
            
            // 内容面板滑入
            if (contentPanel != null)
            {
                contentPanel.anchoredPosition = new Vector2(0f, -50f);
                currentSequence.Join(
                    contentPanel.DOAnchorPos(Vector2.zero, openDuration * 0.8f)
                        .SetDelay(openDuration * 0.2f)
                        .SetEase(Ease.OutQuad)
                );
            }
            
            // 物品槽依次弹出
            AnimateSlotsIn(currentSequence);
            
            currentSequence.OnComplete(() => OnOpenComplete?.Invoke());
        }
        
        /// <summary>
        /// 关闭背包界面
        /// </summary>
        public void Close()
        {
            if (!isOpen) return;
            
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();
            
            // 物品槽依次消失
            AnimateSlotsOut(currentSequence);
            
            // 缩放退出
            currentSequence.Append(
                inventoryPanel.DOScale(Vector3.one * closeScaleEnd, closeDuration)
                    .SetEase(closeEase)
            );
            
            // 淡出
            if (canvasGroup != null)
            {
                currentSequence.Join(
                    canvasGroup.DOFade(0f, closeDuration)
                        .SetEase(Ease.InQuad)
                );
            }
            
            currentSequence.OnComplete(() =>
            {
                isOpen = false;
                gameObject.SetActive(false);
                OnCloseComplete?.Invoke();
            });
        }
        
        /// <summary>
        /// 切换开关状态
        /// </summary>
        public void Toggle()
        {
            if (isOpen)
                Close();
            else
                Open();
        }
        
        /// <summary>
        /// 物品槽进入动画
        /// </summary>
        private void AnimateSlotsIn(Sequence sequence)
        {
            var slots = GetComponentsInChildren<InventorySlotAnimator>();
            
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                float delay = i * slotStaggerDelay;
                
                sequence.Insert(delay, slot.PlayEnterAnimation(slotPopDuration));
            }
        }
        
        /// <summary>
        /// 物品槽退出动画
        /// </summary>
        private void AnimateSlotsOut(Sequence sequence)
        {
            var slots = GetComponentsInChildren<InventorySlotAnimator>();
            
            for (int i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                float delay = i * slotStaggerDelay * 0.5f;
                
                sequence.Insert(delay, slot.PlayExitAnimation(slotPopDuration * 0.5f));
            }
        }
        
        /// <summary>
        /// 分类切换动画
        /// </summary>
        public void PlayCategorySwitch(int fromCategory, int toCategory)
        {
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();
            
            // 旧分类滑出
            float slideDirection = toCategory > fromCategory ? -1f : 1f;
            
            if (contentPanel != null)
            {
                currentSequence.Append(
                    contentPanel.DOAnchorPosX(slideDirection * categorySlideDistance, categorySwitchDuration * 0.5f)
                        .SetEase(Ease.InQuad)
                );
                
                // 淡出
                if (canvasGroup != null)
                {
                    currentSequence.Join(
                        canvasGroup.DOFade(0.5f, categorySwitchDuration * 0.3f)
                    );
                }
                
                // 重置位置并滑入
                currentSequence.AppendCallback(() =>
                {
                    contentPanel.anchoredPosition = new Vector2(-slideDirection * categorySlideDistance, 0f);
                });
                
                currentSequence.Append(
                    contentPanel.DOAnchorPosX(0f, categorySwitchDuration * 0.5f)
                        .SetEase(Ease.OutQuad)
                );
                
                // 淡入
                if (canvasGroup != null)
                {
                    currentSequence.Join(
                        canvasGroup.DOFade(1f, categorySwitchDuration * 0.3f)
                    );
                }
            }
            
            // 物品槽重新进入
            AnimateSlotsIn(currentSequence);
        }
        
        /// <summary>
        /// 新物品获得动画（从世界飞入背包）
        /// </summary>
        public void PlayItemAcquired(Vector3 worldPosition, Sprite itemIcon, Action onComplete = null)
        {
            // 创建临时飞行物品
            GameObject flyingItem = new GameObject("FlyingItem");
            flyingItem.transform.SetParent(transform.parent);
            
            var image = flyingItem.AddComponent<Image>();
            image.sprite = itemIcon;
            image.SetNativeSize();
            
            RectTransform rt = flyingItem.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(64f, 64f);
            
            // 世界坐标转屏幕坐标
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
            rt.position = screenPos;
            
            // 目标位置（背包中心）
            Vector3 targetPos = inventoryPanel.position;
            
            // 飞行动画
            Sequence flySequence = DOTween.Sequence();
            
            // 起始缩放
            rt.localScale = Vector3.one * 0.5f;
            flySequence.Append(
                rt.DOScale(Vector3.one * 1.2f, 0.3f)
                    .SetEase(Ease.OutBack)
            );
            
            // 飞行路径（贝塞尔曲线）
            Vector3 controlPoint = (screenPos + targetPos) / 2f + Vector3.up * 100f;
            
            flySequence.Append(
                rt.DOPath(new Vector3[] { controlPoint, targetPos }, 0.5f, PathType.CatmullRom)
                    .SetEase(Ease.InQuad)
            );
            
            // 缩小消失
            flySequence.Append(
                rt.DOScale(Vector3.zero, 0.2f)
                    .SetEase(Ease.InBack)
            );
            
            flySequence.OnComplete(() =>
            {
                Destroy(flyingItem);
                onComplete?.Invoke();
            });
        }
        
        private void OnDestroy()
        {
            currentSequence?.Kill();
        }
    }
}
