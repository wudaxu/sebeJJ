using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SebeJJ.UI.Animation
{
    /// <summary>
    /// 委托板动画控制器 - 任务列表滚动、选中效果
    /// </summary>
    public class QuestBoardAnimator : MonoBehaviour
    {
        [Header("面板配置")]
        [SerializeField] private RectTransform boardPanel;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform contentTransform;
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("打开/关闭动画")]
        [SerializeField] private float openDuration = 0.5f;
        [SerializeField] private float closeDuration = 0.35f;
        [SerializeField] private Ease openEase = Ease.OutBack;
        [SerializeField] private Ease closeEase = Ease.InBack;
        
        [Header("任务项动画")]
        [SerializeField] private float itemStaggerDelay = 0.05f;
        [SerializeField] private float itemSlideDistance = 100f;
        [SerializeField] private float itemPopDuration = 0.3f;
        
        [Header("滚动效果")]
        [SerializeField] private float scrollSmoothTime = 0.3f;
        [SerializeField] private float scrollVelocityMultiplier = 2f;
        [SerializeField] private float scrollBounceDuration = 0.4f;
        
        [Header("选中效果")]
        [SerializeField] private float selectedScale = 1.05f;
        [SerializeField] private float selectedLift = 10f;
        [SerializeField] private Color selectedGlowColor = new Color(0.2f, 0.9f, 1f, 0.6f);
        [SerializeField] private float selectionPulseDuration = 1f;
        
        [Header("任务状态效果")]
        [SerializeField] private Color availableColor = new Color(0.3f, 0.9f, 0.3f, 1f);
        [SerializeField] private Color inProgressColor = new Color(0.9f, 0.7f, 0.2f, 1f);
        [SerializeField] private Color completedColor = new Color(0.2f, 0.6f, 1f, 1f);
        [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
        
        [Header("刷新动画")]
        [SerializeField] private float refreshRotationDuration = 1f;
        [SerializeField] private float refreshFadeDuration = 0.3f;
        
        private Sequence currentSequence;
        private Vector3 originalScale;
        private QuestItemAnimator selectedItem;
        
        public event Action OnOpenComplete;
        public event Action OnCloseComplete;
        public event Action<QuestItemAnimator> OnItemSelected;
        
        private void Awake()
        {
            if (boardPanel == null)
                boardPanel = GetComponent<RectTransform>();
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
                
            originalScale = boardPanel.localScale;
        }
        
        /// <summary>
        /// 打开委托板
        /// </summary>
        public void Open()
        {
            gameObject.SetActive(true);
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();
            
            // 初始状态
            boardPanel.localScale = Vector3.zero;
            boardPanel.rotation = Quaternion.Euler(0f, 90f, 0f);
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
            
            // 缩放旋转进入
            currentSequence.Append(
                boardPanel.DOScale(originalScale, openDuration)
                    .SetEase(openEase)
            );
            
            currentSequence.Join(
                boardPanel.DORotate(Vector3.zero, openDuration)
                    .SetEase(Ease.OutBack)
            );
            
            if (canvasGroup != null)
            {
                currentSequence.Join(
                    canvasGroup.DOFade(1f, openDuration * 0.7f)
                        .SetDelay(openDuration * 0.3f)
                );
            }
            
            // 任务项依次滑入
            AnimateItemsIn();
            
            currentSequence.OnComplete(() => OnOpenComplete?.Invoke());
        }
        
        /// <summary>
        /// 关闭委托板
        /// </summary>
        public void Close()
        {
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();
            
            // 任务项依次消失
            AnimateItemsOut();
            
            // 面板退出
            currentSequence.Append(
                boardPanel.DOScale(Vector3.zero, closeDuration)
                    .SetEase(closeEase)
            );
            
            currentSequence.Join(
                boardPanel.DORotate(new Vector3(0f, -90f, 0f), closeDuration)
                    .SetEase(Ease.InBack)
            );
            
            if (canvasGroup != null)
            {
                currentSequence.Join(
                    canvasGroup.DOFade(0f, closeDuration * 0.5f)
                );
            }
            
            currentSequence.OnComplete(() =>
            {
                gameObject.SetActive(false);
                OnCloseComplete?.Invoke();
            });
        }
        
        /// <summary>
        /// 任务项进入动画
        /// </summary>
        private void AnimateItemsIn()
        {
            var items = contentTransform.GetComponentsInChildren<QuestItemAnimator>();
            
            for (int i = 0; i < items.Length; i++)
            {
                float delay = openDuration * 0.3f + i * itemStaggerDelay;
                items[i].PlayEnterAnimation(delay, itemSlideDistance, itemPopDuration);
            }
        }
        
        /// <summary>
        /// 任务项退出动画
        /// </summary>
        private void AnimateItemsOut()
        {
            var items = contentTransform.GetComponentsInChildren<QuestItemAnimator>();
            
            for (int i = 0; i < items.Length; i++)
            {
                float delay = i * itemStaggerDelay * 0.5f;
                items[i].PlayExitAnimation(delay, itemPopDuration * 0.5f);
            }
        }
        
        /// <summary>
        /// 选中任务项
        /// </summary>
        public void SelectItem(QuestItemAnimator item)
        {
            if (selectedItem == item) return;
            
            // 取消之前选中
            if (selectedItem != null)
            {
                selectedItem.SetSelected(false);
            }
            
            selectedItem = item;
            
            if (item != null)
            {
                item.SetSelected(true);
                
                // 滚动到选中项
                ScrollToItem(item);
                
                OnItemSelected?.Invoke(item);
            }
        }
        
        /// <summary>
        /// 滚动到指定任务项
        /// </summary>
        public void ScrollToItem(QuestItemAnimator item)
        {
            if (scrollRect == null || contentTransform == null) return;
            
            RectTransform itemRect = item.GetComponent<RectTransform>();
            
            // 计算目标位置
            float contentHeight = contentTransform.rect.height;
            float viewportHeight = ((RectTransform)scrollRect.viewport).rect.height;
            float itemY = Mathf.Abs(itemRect.anchoredPosition.y);
            float itemHeight = itemRect.rect.height;
            
            float targetNormalizedPos = 1f - (itemY / (contentHeight - viewportHeight));
            targetNormalizedPos = Mathf.Clamp01(targetNormalizedPos);
            
            // 平滑滚动
            DOTween.To(
                () => scrollRect.verticalNormalizedPosition,
                x => scrollRect.verticalNormalizedPosition = x,
                targetNormalizedPos,
                scrollSmoothTime
            ).SetEase(Ease.OutQuad);
        }
        
        /// <summary>
        /// 刷新动画
        /// </summary>
        public void PlayRefreshAnimation(Action onComplete = null)
        {
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();
            
            // 淡出旧任务
            var items = contentTransform.GetComponentsInChildren<QuestItemAnimator>();
            foreach (var item in items)
            {
                item.PlayFadeOut(refreshFadeDuration);
            }
            
            // 旋转刷新图标（如果有）
            Transform refreshIcon = transform.Find("RefreshIcon");
            if (refreshIcon != null)
            {
                currentSequence.Append(
                    refreshIcon.DORotate(new Vector3(0f, 0f, -360f), refreshRotationDuration, RotateMode.FastBeyond360)
                        .SetEase(Ease.InOutQuad)
                );
            }
            
            // 延迟后显示新任务
            currentSequence.AppendInterval(refreshRotationDuration * 0.5f);
            
            currentSequence.OnComplete(() =>
            {
                AnimateItemsIn();
                onComplete?.Invoke();
            });
        }
        
        /// <summary>
        /// 任务完成庆祝动画
        /// </summary>
        public void PlayQuestCompleteAnimation(QuestItemAnimator completedItem)
        {
            if (completedItem == null) return;
            
            Sequence celebrateSeq = DOTween.Sequence();
            
            // 选中项放大发光
            celebrateSeq.Append(
                completedItem.transform.DOScale(selectedScale * 1.2f, 0.3f)
                    .SetEase(Ease.OutBack)
            );
            
            // 颜色变为完成色
            celebrateSeq.Join(
                completedItem.PlayColorChange(completedColor, 0.3f)
            );
            
            // 闪烁效果
            celebrateSeq.Append(
                completedItem.PlayFlash(3, 0.1f)
            );
            
            // 上浮消失
            celebrateSeq.Append(
                completedItem.transform.DOAnchorPosY(
                    completedItem.GetComponent<RectTransform>().anchoredPosition.y + 100f, 
                    0.5f
                ).SetEase(Ease.InBack)
            );
            
            celebrateSeq.Join(
                completedItem.GetComponent<CanvasGroup>()?.DOFade(0f, 0.5f) ?? null
            );
        }
        
        /// <summary>
        /// 滚动时的动态效果
        /// </summary>
        private void Update()
        {
            if (scrollRect == null) return;
            
            // 根据滚动速度调整任务项倾斜
            float velocity = Mathf.Abs(scrollRect.velocity.y);
            float tiltAmount = Mathf.Clamp(velocity * 0.001f, 0f, 10f);
            
            var items = contentTransform.GetComponentsInChildren<QuestItemAnimator>();
            foreach (var item in items)
            {
                if (item != selectedItem)
                {
                    item.SetTilt(tiltAmount * Mathf.Sign(scrollRect.velocity.y));
                }
            }
        }
        
        private void OnDestroy()
        {
            currentSequence?.Kill();
        }
    }
}
