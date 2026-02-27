using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

namespace SebeJJ.UI.Polish
{
    /// <summary>
    /// 界面切换过渡管理器 - 管理所有UI界面的切换动画
    /// </summary>
    public class TransitionManager : MonoBehaviour
    {
        public static TransitionManager Instance { get; private set; }
        
        [Header("过渡组件")]
        [SerializeField] private Image fadeOverlay;
        [SerializeField] private RectTransform transitionPanel;
        [SerializeField] private CanvasGroup transitionCanvasGroup;
        
        [Header("过渡效果")]
        [SerializeField] private TransitionType defaultTransition = TransitionType.Fade;
        [SerializeField] private float defaultDuration = 0.4f;
        
        [Header("赛博朋克特效")]
        [SerializeField] private Image glitchOverlay;
        [SerializeField] private Image scanLineOverlay;
        [SerializeField] private ParticleSystem transitionParticles;
        
        [Header("配色")]
        [SerializeField] private Color primaryColor = new Color(0.2f, 0.9f, 1f, 1f);
        [SerializeField] private Color secondaryColor = new Color(0.8f, 0.2f, 0.9f, 1f);
        
        // 状态
        private bool isTransitioning = false;
        private Stack<TransitionData> transitionStack = new Stack<TransitionData>();
        
        // 动画序列
        private Sequence currentSequence;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void Start()
        {
            InitializeTransitionElements();
        }
        
        /// <summary>
        /// 初始化过渡元素
        /// </summary>
        private void InitializeTransitionElements()
        {
            if (fadeOverlay != null)
            {
                fadeOverlay.color = Color.black.WithAlpha(0f);
                fadeOverlay.raycastTarget = false;
            }
            
            if (glitchOverlay != null)
            {
                glitchOverlay.color = primaryColor.WithAlpha(0f);
                glitchOverlay.raycastTarget = false;
            }
            
            if (scanLineOverlay != null)
            {
                scanLineOverlay.color = secondaryColor.WithAlpha(0f);
            }
            
            if (transitionCanvasGroup != null)
            {
                transitionCanvasGroup.alpha = 0f;
                transitionCanvasGroup.blocksRaycasts = false;
            }
        }
        
        #region 公共过渡方法
        
        /// <summary>
        /// 执行界面过渡
        /// </summary>
        public void Transition(System.Action onTransitionStart, System.Action onTransitionEnd, 
            TransitionType? type = null, float? duration = null)
        {
            if (isTransitioning) return;
            
            TransitionData data = new TransitionData
            {
                type = type ?? defaultTransition,
                duration = duration ?? defaultDuration,
                onStart = onTransitionStart,
                onEnd = onTransitionEnd
            };
            
            StartCoroutine(ExecuteTransition(data));
        }
        
        /// <summary>
        /// 淡入淡出过渡
        /// </summary>
        public void FadeTransition(System.Action onTransitionStart, System.Action onTransitionEnd, float? duration = null)
        {
            Transition(onTransitionStart, onTransitionEnd, TransitionType.Fade, duration);
        }
        
        /// <summary>
        /// 滑动过渡
        /// </summary>
        public void SlideTransition(System.Action onTransitionStart, System.Action onTransitionEnd, 
            SlideDirection direction = SlideDirection.Right, float? duration = null)
        {
            TransitionData data = new TransitionData
            {
                type = TransitionType.Slide,
                duration = duration ?? defaultDuration,
                slideDirection = direction,
                onStart = onTransitionStart,
                onEnd = onTransitionEnd
            };
            
            StartCoroutine(ExecuteTransition(data));
        }
        
        /// <summary>
        /// 赛博朋克故障过渡
        /// </summary>
        public void GlitchTransition(System.Action onTransitionStart, System.Action onTransitionEnd, float? duration = null)
        {
            Transition(onTransitionStart, onTransitionEnd, TransitionType.Glitch, duration);
        }
        
        /// <summary>
        /// 数字扫描过渡
        /// </summary>
        public void DigitalScanTransition(System.Action onTransitionStart, System.Action onTransitionEnd, float? duration = null)
        {
            Transition(onTransitionStart, onTransitionEnd, TransitionType.DigitalScan, duration);
        }
        
        #endregion
        
        #region 过渡执行
        
        private System.Collections.IEnumerator ExecuteTransition(TransitionData data)
        {
            isTransitioning = true;
            
            // 开始过渡 - 退出当前界面
            yield return StartCoroutine(PlayExitTransition(data));
            
            // 执行切换
            data.onStart?.Invoke();
            yield return new WaitForSeconds(0.05f);
            
            // 结束过渡 - 进入新界面
            yield return StartCoroutine(PlayEnterTransition(data));
            
            data.onEnd?.Invoke();
            isTransitioning = false;
        }
        
        private System.Collections.IEnumerator PlayExitTransition(TransitionData data)
        {
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();
            
            switch (data.type)
            {
                case TransitionType.Fade:
                    SetupFadeExit(currentSequence, data);
                    break;
                case TransitionType.Slide:
                    SetupSlideExit(currentSequence, data);
                    break;
                case TransitionType.Glitch:
                    SetupGlitchExit(currentSequence, data);
                    break;
                case TransitionType.DigitalScan:
                    SetupDigitalScanExit(currentSequence, data);
                    break;
                case TransitionType.Zoom:
                    SetupZoomExit(currentSequence, data);
                    break;
            }
            
            currentSequence.Play();
            yield return new WaitForSeconds(data.duration);
        }
        
        private System.Collections.IEnumerator PlayEnterTransition(TransitionData data)
        {
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();
            
            switch (data.type)
            {
                case TransitionType.Fade:
                    SetupFadeEnter(currentSequence, data);
                    break;
                case TransitionType.Slide:
                    SetupSlideEnter(currentSequence, data);
                    break;
                case TransitionType.Glitch:
                    SetupGlitchEnter(currentSequence, data);
                    break;
                case TransitionType.DigitalScan:
                    SetupDigitalScanEnter(currentSequence, data);
                    break;
                case TransitionType.Zoom:
                    SetupZoomEnter(currentSequence, data);
                    break;
            }
            
            currentSequence.Play();
            yield return new WaitForSeconds(data.duration);
        }
        
        #endregion
        
        #region 淡入淡出效果
        
        private void SetupFadeExit(Sequence sequence, TransitionData data)
        {
            if (fadeOverlay != null)
            {
                fadeOverlay.raycastTarget = true;
                fadeOverlay.color = Color.black.WithAlpha(0f);
                sequence.Append(fadeOverlay.DOColor(Color.black.WithAlpha(1f), data.duration)
                    .SetEase(Ease.InQuad));
            }
        }
        
        private void SetupFadeEnter(Sequence sequence, TransitionData data)
        {
            if (fadeOverlay != null)
            {
                sequence.Append(fadeOverlay.DOColor(Color.black.WithAlpha(0f), data.duration)
                    .SetEase(Ease.OutQuad));
                sequence.OnComplete(() => fadeOverlay.raycastTarget = false);
            }
        }
        
        #endregion
        
        #region 滑动效果
        
        private void SetupSlideExit(Sequence sequence, TransitionData data)
        {
            if (transitionPanel == null) return;
            
            Vector2 exitPosition = GetSlidePosition(data.slideDirection, true);
            sequence.Append(transitionPanel.DOAnchorPos(exitPosition, data.duration)
                .SetEase(Ease.InQuad));
        }
        
        private void SetupSlideEnter(Sequence sequence, TransitionData data)
        {
            if (transitionPanel == null) return;
            
            Vector2 startPosition = GetSlidePosition(data.slideDirection, false);
            Vector2 endPosition = Vector2.zero;
            
            transitionPanel.anchoredPosition = startPosition;
            sequence.Append(transitionPanel.DOAnchorPos(endPosition, data.duration)
                .SetEase(Ease.OutBack));
        }
        
        private Vector2 GetSlidePosition(SlideDirection direction, bool exit)
        {
            float multiplier = exit ? 1f : -1f;
            float screenWidth = Screen.width;
            float screenHeight = Screen.height;
            
            switch (direction)
            {
                case SlideDirection.Left:
                    return new Vector2(-screenWidth * multiplier, 0);
                case SlideDirection.Right:
                    return new Vector2(screenWidth * multiplier, 0);
                case SlideDirection.Up:
                    return new Vector2(0, screenHeight * multiplier);
                case SlideDirection.Down:
                    return new Vector2(0, -screenHeight * multiplier);
                default:
                    return Vector2.zero;
            }
        }
        
        #endregion
        
        #region 故障效果
        
        private void SetupGlitchExit(Sequence sequence, TransitionData data)
        {
            if (glitchOverlay == null) return;
            
            glitchOverlay.raycastTarget = true;
            
            // 快速闪烁
            for (int i = 0; i < 5; i++)
            {
                sequence.Append(glitchOverlay.DOColor(primaryColor.WithAlpha(Random.value * 0.5f), 0.05f));
                sequence.AppendCallback(() => ApplyGlitchOffset());
            }
            
            // 最终淡出
            sequence.Append(glitchOverlay.DOColor(primaryColor.WithAlpha(0.8f), data.duration * 0.3f));
        }
        
        private void SetupGlitchEnter(Sequence sequence, TransitionData data)
        {
            if (glitchOverlay == null) return;
            
            glitchOverlay.color = primaryColor.WithAlpha(0.8f);
            
            // 快速闪烁恢复
            for (int i = 0; i < 5; i++)
            {
                sequence.Append(glitchOverlay.DOColor(primaryColor.WithAlpha(Random.value * 0.3f), 0.05f));
                sequence.AppendCallback(() => ApplyGlitchOffset());
            }
            
            // 完全消失
            sequence.Append(glitchOverlay.DOColor(primaryColor.WithAlpha(0f), data.duration * 0.3f));
            sequence.OnComplete(() => glitchOverlay.raycastTarget = false);
        }
        
        private void ApplyGlitchOffset()
        {
            if (transitionPanel == null) return;
            
            float offsetX = Random.Range(-10f, 10f);
            transitionPanel.anchoredPosition = new Vector2(offsetX, 0);
        }
        
        #endregion
        
        #region 数字扫描效果
        
        private void SetupDigitalScanExit(Sequence sequence, TransitionData data)
        {
            if (scanLineOverlay == null) return;
            
            scanLineOverlay.raycastTarget = true;
            
            float screenHeight = Screen.height;
            var rectTransform = scanLineOverlay.rectTransform;
            
            rectTransform.anchoredPosition = new Vector2(0, screenHeight * 0.5f);
            rectTransform.sizeDelta = new Vector2(Screen.width, 10f);
            
            // 扫描线从上到下
            sequence.Append(rectTransform.DOAnchorPosY(-screenHeight * 0.5f, data.duration * 0.7f)
                .SetEase(Ease.InOutQuad));
            
            // 淡出
            sequence.Join(scanLineOverlay.DOColor(secondaryColor.WithAlpha(0.6f), data.duration * 0.3f));
            
            // 同时淡出界面
            if (transitionCanvasGroup != null)
            {
                sequence.Join(transitionCanvasGroup.DOFade(0f, data.duration * 0.5f));
            }
        }
        
        private void SetupDigitalScanEnter(Sequence sequence, TransitionData data)
        {
            if (scanLineOverlay == null) return;
            
            float screenHeight = Screen.height;
            var rectTransform = scanLineOverlay.rectTransform;
            
            // 重置界面透明度
            if (transitionCanvasGroup != null)
            {
                transitionCanvasGroup.alpha = 0f;
            }
            
            rectTransform.anchoredPosition = new Vector2(0, -screenHeight * 0.5f);
            
            // 扫描线从下到上
            sequence.Append(rectTransform.DOAnchorPosY(screenHeight * 0.5f, data.duration * 0.7f)
                .SetEase(Ease.InOutQuad));
            
            // 同时淡入界面
            if (transitionCanvasGroup != null)
            {
                sequence.Join(transitionCanvasGroup.DOFade(1f, data.duration * 0.5f));
            }
            
            // 扫描线消失
            sequence.Append(scanLineOverlay.DOColor(secondaryColor.WithAlpha(0f), data.duration * 0.3f));
            sequence.OnComplete(() => scanLineOverlay.raycastTarget = false);
        }
        
        #endregion
        
        #region 缩放效果
        
        private void SetupZoomExit(Sequence sequence, TransitionData data)
        {
            if (transitionPanel == null) return;
            
            sequence.Append(transitionPanel.DOScale(1.2f, data.duration * 0.5f)
                .SetEase(Ease.OutQuad));
            sequence.Join(transitionPanel.GetComponent<CanvasGroup>()?.DOFade(0f, data.duration * 0.5f));
        }
        
        private void SetupZoomEnter(Sequence sequence, TransitionData data)
        {
            if (transitionPanel == null) return;
            
            transitionPanel.localScale = Vector3.one * 0.8f;
            
            var canvasGroup = transitionPanel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                sequence.Join(canvasGroup.DOFade(1f, data.duration));
            }
            
            sequence.Append(transitionPanel.DOScale(1f, data.duration)
                .SetEase(Ease.OutBack));
        }
        
        #endregion
        
        /// <summary>
        /// 检查是否正在过渡
        /// </summary>
        public bool IsTransitioning()
        {
            return isTransitioning;
        }
        
        /// <summary>
        /// 强制停止过渡
        /// </summary>
        public void ForceStopTransition()
        {
            currentSequence?.Kill();
            isTransitioning = false;
            InitializeTransitionElements();
        }
        
        private void OnDestroy()
        {
            currentSequence?.Kill();
        }
    }
    
    #region 数据定义
    
    public enum TransitionType
    {
        Fade,           // 淡入淡出
        Slide,          // 滑动
        Glitch,         // 故障效果
        DigitalScan,    // 数字扫描
        Zoom,           // 缩放
        Custom          // 自定义
    }
    
    public enum SlideDirection
    {
        Left,
        Right,
        Up,
        Down
    }
    
    public struct TransitionData
    {
        public TransitionType type;
        public float duration;
        public SlideDirection slideDirection;
        public System.Action onStart;
        public System.Action onEnd;
    }
    
    #endregion
    
    #region 扩展
    
    public static class TransitionColorExtensions
    {
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
    
    #endregion
}
