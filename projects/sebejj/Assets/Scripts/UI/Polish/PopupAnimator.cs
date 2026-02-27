using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

namespace SebeJJ.UI.Polish
{
    /// <summary>
    /// 弹窗动画管理器 - 弹窗弹出/关闭动画、通知提示动画
    /// </summary>
    public class PopupAnimator : MonoBehaviour
    {
        public static PopupAnimator Instance { get; private set; }
        
        [Header("弹窗动画设置")]
        [SerializeField] private float popupShowDuration = 0.3f;
        [SerializeField] private float popupHideDuration = 0.2f;
        [SerializeField] private Ease popupShowEase = Ease.OutBack;
        [SerializeField] private Ease popupHideEase = Ease.InBack;
        
        [Header("背景遮罩")]
        [SerializeField] private Image overlayImage;
        [SerializeField] private float overlayFadeDuration = 0.2f;
        [SerializeField] private Color overlayColor = new Color(0f, 0f, 0f, 0.7f);
        
        [Header("粒子效果")]
        [SerializeField] private bool spawnParticlesOnShow = true;
        [SerializeField] private ParticleSystem popupParticles;
        
        // 活动弹窗队列
        private Queue<PopupData> popupQueue = new Queue<PopupData>();
        private List<PopupInstance> activePopups = new List<PopupInstance>();
        private bool isShowingPopup = false;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Start()
        {
            InitializeOverlay();
        }
        
        /// <summary>
        /// 初始化遮罩
        /// </summary>
        private void InitializeOverlay()
        {
            if (overlayImage != null)
            {
                overlayImage.color = overlayColor.WithAlpha(0f);
                overlayImage.raycastTarget = false;
            }
        }
        
        #region 弹窗显示
        
        /// <summary>
        /// 显示弹窗
        /// </summary>
        public void ShowPopup(RectTransform popupPanel, PopupAnimationType animationType = PopupAnimationType.Scale,
            System.Action onShowComplete = null)
        {
            PopupData data = new PopupData
            {
                panel = popupPanel,
                type = animationType,
                onComplete = onShowComplete,
                isShowing = true
            };
            
            if (isShowingPopup)
            {
                popupQueue.Enqueue(data);
                return;
            }
            
            StartCoroutine(ExecutePopupAnimation(data));
        }
        
        /// <summary>
        /// 隐藏弹窗
        /// </summary>
        public void HidePopup(RectTransform popupPanel, PopupAnimationType animationType = PopupAnimationType.Scale,
            System.Action onHideComplete = null)
        {
            PopupData data = new PopupData
            {
                panel = popupPanel,
                type = animationType,
                onComplete = onHideComplete,
                isShowing = false
            };
            
            StartCoroutine(ExecutePopupAnimation(data));
        }
        
        /// <summary>
        /// 显示通知提示
        /// </summary>
        public void ShowNotification(RectTransform notificationPanel, NotificationType type = NotificationType.Info,
            float autoHideDelay = 3f, System.Action onComplete = null)
        {
            Color notificationColor = GetNotificationColor(type);
            
            // 设置颜色
            var images = notificationPanel.GetComponentsInChildren<Image>();
            foreach (var img in images)
            {
                if (img.name.Contains("Icon") || img.name.Contains("Border"))
                {
                    img.DOColor(notificationColor, 0.2f);
                }
            }
            
            // 从右侧滑入
            notificationPanel.anchoredPosition = new Vector2(Screen.width, 0);
            notificationPanel.DOAnchorPosX(0, popupShowDuration)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    onComplete?.Invoke();
                    
                    // 自动隐藏
                    if (autoHideDelay > 0)
                    {
                        DOVirtual.DelayedCall(autoHideDelay, () =>
                        {
                            HideNotification(notificationPanel);
                        });
                    }
                });
            
            // 播放粒子效果
            if (spawnParticlesOnShow)
            {
                SpawnNotificationParticles(notificationPanel.position, notificationColor);
            }
        }
        
        /// <summary>
        /// 隐藏通知提示
        /// </summary>
        public void HideNotification(RectTransform notificationPanel, System.Action onComplete = null)
        {
            notificationPanel.DOAnchorPosX(Screen.width, popupHideDuration)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    notificationPanel.gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
        }
        
        #endregion
        
        #region 动画执行
        
        private System.Collections.IEnumerator ExecutePopupAnimation(PopupData data)
        {
            isShowingPopup = true;
            
            if (data.isShowing)
            {
                // 显示遮罩
                ShowOverlay();
                yield return new WaitForSeconds(overlayFadeDuration * 0.5f);
                
                // 执行显示动画
                switch (data.type)
                {
                    case PopupAnimationType.Scale:
                        yield return StartCoroutine(AnimateScaleIn(data.panel));
                        break;
                    case PopupAnimationType.SlideDown:
                        yield return StartCoroutine(AnimateSlideDown(data.panel));
                        break;
                    case PopupAnimationType.SlideUp:
                        yield return StartCoroutine(AnimateSlideUp(data.panel));
                        break;
                    case PopupAnimationType.Fade:
                        yield return StartCoroutine(AnimateFadeIn(data.panel));
                        break;
                    case PopupAnimationType.Bounce:
                        yield return StartCoroutine(AnimateBounceIn(data.panel));
                        break;
                    case PopupAnimationType.Flip:
                        yield return StartCoroutine(AnimateFlipIn(data.panel));
                        break;
                }
                
                // 添加粒子效果
                if (spawnParticlesOnShow)
                {
                    SpawnPopupParticles(data.panel.position);
                }
            }
            else
            {
                // 执行隐藏动画
                switch (data.type)
                {
                    case PopupAnimationType.Scale:
                        yield return StartCoroutine(AnimateScaleOut(data.panel));
                        break;
                    case PopupAnimationType.SlideDown:
                    case PopupAnimationType.SlideUp:
                        yield return StartCoroutine(AnimateSlideOut(data.panel));
                        break;
                    case PopupAnimationType.Fade:
                        yield return StartCoroutine(AnimateFadeOut(data.panel));
                        break;
                    case PopupAnimationType.Bounce:
                        yield return StartCoroutine(AnimateBounceOut(data.panel));
                        break;
                    case PopupAnimationType.Flip:
                        yield return StartCoroutine(AnimateFlipOut(data.panel));
                        break;
                }
                
                // 隐藏遮罩
                yield return new WaitForSeconds(popupHideDuration * 0.5f);
                HideOverlay();
            }
            
            data.onComplete?.Invoke();
            isShowingPopup = false;
            
            // 处理队列中的下一个弹窗
            if (popupQueue.Count > 0)
            {
                PopupData nextPopup = popupQueue.Dequeue();
                StartCoroutine(ExecutePopupAnimation(nextPopup));
            }
        }
        
        #endregion
        
        #region 具体动画
        
        private System.Collections.IEnumerator AnimateScaleIn(RectTransform panel)
        {
            panel.localScale = Vector3.zero;
            
            Tween tween = panel.DOScale(1f, popupShowDuration)
                .SetEase(popupShowEase);
            
            yield return tween.WaitForCompletion();
        }
        
        private System.Collections.IEnumerator AnimateScaleOut(RectTransform panel)
        {
            Tween tween = panel.DOScale(0f, popupHideDuration)
                .SetEase(popupHideEase);
            
            yield return tween.WaitForCompletion();
        }
        
        private System.Collections.IEnumerator AnimateSlideDown(RectTransform panel)
        {
            float startY = Screen.height * 0.5f + panel.rect.height * 0.5f;
            panel.anchoredPosition = new Vector2(0, startY);
            
            Tween tween = panel.DOAnchorPosY(0, popupShowDuration)
                .SetEase(popupShowEase);
            
            yield return tween.WaitForCompletion();
        }
        
        private System.Collections.IEnumerator AnimateSlideUp(RectTransform panel)
        {
            float startY = -Screen.height * 0.5f - panel.rect.height * 0.5f;
            panel.anchoredPosition = new Vector2(0, startY);
            
            Tween tween = panel.DOAnchorPosY(0, popupShowDuration)
                .SetEase(popupShowEase);
            
            yield return tween.WaitForCompletion();
        }
        
        private System.Collections.IEnumerator AnimateSlideOut(RectTransform panel)
        {
            Tween tween = panel.DOScale(0.8f, popupHideDuration)
                .SetEase(popupHideEase);
            
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.DOFade(0f, popupHideDuration);
            }
            
            yield return tween.WaitForCompletion();
        }
        
        private System.Collections.IEnumerator AnimateFadeIn(RectTransform panel)
        {
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = panel.gameObject.AddComponent<CanvasGroup>();
            }
            
            canvasGroup.alpha = 0f;
            
            Tween tween = canvasGroup.DOFade(1f, popupShowDuration)
                .SetEase(Ease.OutQuad);
            
            yield return tween.WaitForCompletion();
        }
        
        private System.Collections.IEnumerator AnimateFadeOut(RectTransform panel)
        {
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null) yield break;
            
            Tween tween = canvasGroup.DOFade(0f, popupHideDuration)
                .SetEase(Ease.InQuad);
            
            yield return tween.WaitForCompletion();
        }
        
        private System.Collections.IEnumerator AnimateBounceIn(RectTransform panel)
        {
            panel.localScale = Vector3.zero;
            
            Tween tween = panel.DOScale(1f, popupShowDuration)
                .SetEase(Ease.OutBounce);
            
            yield return tween.WaitForCompletion();
        }
        
        private System.Collections.IEnumerator AnimateBounceOut(RectTransform panel)
        {
            Tween tween = panel.DOScale(0f, popupHideDuration)
                .SetEase(Ease.InBounce);
            
            yield return tween.WaitForCompletion();
        }
        
        private System.Collections.IEnumerator AnimateFlipIn(RectTransform panel)
        {
            panel.localScale = new Vector3(0f, 1f, 1f);
            
            Tween tween = panel.DOScaleX(1f, popupShowDuration)
                .SetEase(popupShowEase);
            
            yield return tween.WaitForCompletion();
        }
        
        private System.Collections.IEnumerator AnimateFlipOut(RectTransform panel)
        {
            Tween tween = panel.DOScaleX(0f, popupHideDuration)
                .SetEase(popupHideEase);
            
            yield return tween.WaitForCompletion();
        }
        
        #endregion
        
        #region 遮罩控制
        
        private void ShowOverlay()
        {
            if (overlayImage == null) return;
            
            overlayImage.raycastTarget = true;
            overlayImage.DOColor(overlayColor, overlayFadeDuration)
                .SetEase(Ease.OutQuad);
        }
        
        private void HideOverlay()
        {
            if (overlayImage == null) return;
            
            overlayImage.DOColor(overlayColor.WithAlpha(0f), overlayFadeDuration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => overlayImage.raycastTarget = false);
        }
        
        #endregion
        
        #region 粒子效果
        
        private void SpawnPopupParticles(Vector3 position)
        {
            if (popupParticles != null)
            {
                ParticleSystem particles = Instantiate(popupParticles, position, Quaternion.identity);
                particles.Play();
                Destroy(particles.gameObject, 2f);
            }
            
            // 使用MainMenuVisualManager创建额外效果
            if (MainMenuVisualManager.Instance != null)
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = i * 60f * Mathf.Deg2Rad;
                    Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * 30f;
                    MainMenuVisualManager.Instance.SpawnEffectParticle(
                        position + offset,
                        new Color(0.2f, 0.9f, 1f, 0.8f),
                        0.08f
                    );
                }
            }
        }
        
        private void SpawnNotificationParticles(Vector3 position, Color color)
        {
            if (MainMenuVisualManager.Instance != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = (i * 90f + 45f) * Mathf.Deg2Rad;
                    Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * 20f;
                    MainMenuVisualManager.Instance.SpawnEffectParticle(
                        position + offset,
                        color,
                        0.05f
                    );
                }
            }
        }
        
        #endregion
        
        #region 辅助方法
        
        private Color GetNotificationColor(NotificationType type)
        {
            switch (type)
            {
                case NotificationType.Success:
                    return new Color(0.2f, 0.9f, 0.3f, 1f);
                case NotificationType.Warning:
                    return new Color(0.95f, 0.8f, 0.1f, 1f);
                case NotificationType.Error:
                    return new Color(0.95f, 0.2f, 0.1f, 1f);
                case NotificationType.Info:
                default:
                    return new Color(0.2f, 0.6f, 1f, 1f);
            }
        }
        
        #endregion
    }
    
    #region 数据定义
    
    public enum PopupAnimationType
    {
        Scale,      // 缩放
        SlideDown,  // 向下滑入
        SlideUp,    // 向上滑入
        Fade,       // 淡入淡出
        Bounce,     // 弹跳
        Flip        // 翻转
    }
    
    public enum NotificationType
    {
        Info,       // 信息
        Success,    // 成功
        Warning,    // 警告
        Error       // 错误
    }
    
    public struct PopupData
    {
        public RectTransform panel;
        public PopupAnimationType type;
        public System.Action onComplete;
        public bool isShowing;
    }
    
    public class PopupInstance
    {
        public RectTransform panel;
        public PopupAnimationType type;
        public bool isActive;
    }
    
    #endregion
    
    #region 扩展
    
    public static class PopupColorExtensions
    {
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
    
    #endregion
}
