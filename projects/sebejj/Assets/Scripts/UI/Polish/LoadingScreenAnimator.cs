using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

namespace SebeJJ.UI.Polish
{
    /// <summary>
    /// 加载界面动画管理器 - 赛博朋克风格加载动画
    /// </summary>
    public class LoadingScreenAnimator : MonoBehaviour
    {
        public static LoadingScreenAnimator Instance { get; private set; }
        
        [Header("加载界面组件")]
        [SerializeField] private CanvasGroup loadingCanvasGroup;
        [SerializeField] private RectTransform loadingPanel;
        [SerializeField] private Image backgroundImage;
        
        [Header("进度显示")]
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Text progressText;
        [SerializeField] private Text loadingTipText;
        
        [Header("动画元素")]
        [SerializeField] private RectTransform spinnerTransform;
        [SerializeField] private RectTransform[] dataBlocks;
        [SerializeField] private Image scanLineImage;
        [SerializeField] private ParticleSystem loadingParticles;
        
        [Header("赛博朋克特效")]
        [SerializeField] private Image glitchOverlay;
        [SerializeField] private Text codeRainText;
        [SerializeField] private RectTransform hologramFrame;
        
        [Header("动画设置")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.3f;
        [SerializeField] private float spinnerSpeed = 360f;
        [SerializeField] private float scanSpeed = 2f;
        
        [Header("配色")]
        [SerializeField] private Color primaryColor = new Color(0.2f, 0.9f, 1f, 1f);
        [SerializeField] private Color secondaryColor = new Color(0.8f, 0.2f, 0.9f, 1f);
        
        // 状态
        private bool isLoading = false;
        private float currentProgress = 0f;
        private Sequence loadingSequence;
        
        // 提示文本
        private string[] loadingTips = new string[]
        {
            "正在初始化神经链接...",
            "加载战斗数据...",
            "同步机甲系统...",
            "校准武器模块...",
            "建立安全连接...",
            "优化渲染管线...",
            "加载资源包...",
            "准备战场环境..."
        };
        
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
            InitializeLoadingScreen();
        }
        
        private void Update()
        {
            if (isLoading)
            {
                UpdateLoadingAnimation();
            }
        }
        
        /// <summary>
        /// 初始化加载界面
        /// </summary>
        private void InitializeLoadingScreen()
        {
            if (loadingCanvasGroup != null)
            {
                loadingCanvasGroup.alpha = 0f;
                loadingCanvasGroup.blocksRaycasts = false;
            }
            
            if (progressSlider != null)
            {
                progressSlider.value = 0f;
            }
            
            if (glitchOverlay != null)
            {
                glitchOverlay.color = primaryColor.WithAlpha(0f);
            }
            
            // 初始化数据块
            InitializeDataBlocks();
        }
        
        /// <summary>
        /// 初始化数据块动画
        /// </summary>
        private void InitializeDataBlocks()
        {
            if (dataBlocks == null || dataBlocks.Length == 0) return;
            
            for (int i = 0; i < dataBlocks.Length; i++)
            {
                if (dataBlocks[i] == null) continue;
                
                var image = dataBlocks[i].GetComponent<Image>();
                if (image != null)
                {
                    image.color = primaryColor.WithAlpha(0.3f);
                }
                
                dataBlocks[i].localScale = Vector3.one;
            }
        }
        
        #region 加载控制
        
        /// <summary>
        /// 显示加载界面
        /// </summary>
        public void ShowLoadingScreen(string customTip = null)
        {
            isLoading = true;
            currentProgress = 0f;
            
            // 设置提示文本
            if (loadingTipText != null)
            {
                string tip = customTip ?? loadingTips[Random.Range(0, loadingTips.Length)];
                loadingTipText.text = tip;
            }
            
            // 淡入
            if (loadingCanvasGroup != null)
            {
                loadingCanvasGroup.blocksRaycasts = true;
                loadingCanvasGroup.DOFade(1f, fadeInDuration)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() => StartLoadingAnimations());
            }
            
            // 启动粒子
            if (loadingParticles != null)
            {
                loadingParticles.Play();
            }
        }
        
        /// <summary>
        /// 隐藏加载界面
        /// </summary>
        public void HideLoadingScreen(System.Action onComplete = null)
        {
            // 先完成进度
            SetProgress(1f);
            
            DOVirtual.DelayedCall(0.3f, () =>
            {
                isLoading = false;
                
                // 停止动画
                StopLoadingAnimations();
                
                // 淡出
                if (loadingCanvasGroup != null)
                {
                    loadingCanvasGroup.DOFade(0f, fadeOutDuration)
                        .SetEase(Ease.InQuad)
                        .OnComplete(() =>
                        {
                            loadingCanvasGroup.blocksRaycasts = false;
                            onComplete?.Invoke();
                        });
                }
                
                // 停止粒子
                if (loadingParticles != null)
                {
                    loadingParticles.Stop();
                }
            });
        }
        
        /// <summary>
        /// 设置加载进度
        /// </summary>
        public void SetProgress(float progress)
        {
            currentProgress = Mathf.Clamp01(progress);
            
            if (progressSlider != null)
            {
                progressSlider.DOValue(currentProgress, 0.2f)
                    .SetEase(Ease.OutQuad);
            }
            
            if (progressText != null)
            {
                progressText.text = $"{Mathf.RoundToInt(currentProgress * 100)}%";
            }
            
            // 更新数据块
            UpdateDataBlocks(currentProgress);
        }
        
        /// <summary>
        /// 更新加载提示
        /// </summary>
        public void UpdateLoadingTip(string tip)
        {
            if (loadingTipText != null)
            {
                // 淡出旧文本
                loadingTipText.DOFade(0f, 0.15f)
                    .OnComplete(() =>
                    {
                        loadingTipText.text = tip;
                        loadingTipText.DOFade(1f, 0.15f);
                    });
            }
        }
        
        #endregion
        
        #region 动画控制
        
        /// <summary>
        /// 启动加载动画
        /// </summary>
        private void StartLoadingAnimations()
        {
            // 旋转动画
            if (spinnerTransform != null)
            {
                spinnerTransform.DORotate(new Vector3(0, 0, -360), 2f, RotateMode.LocalAxisAdd)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Restart);
            }
            
            // 扫描线动画
            if (scanLineImage != null)
            {
                float panelHeight = loadingPanel.rect.height;
                var rectTransform = scanLineImage.rectTransform;
                
                rectTransform.anchoredPosition = new Vector2(0, panelHeight * 0.5f);
                rectTransform.DOAnchorPosY(-panelHeight * 0.5f, 3f / scanSpeed)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Restart);
            }
            
            // 故障效果
            StartGlitchEffect();
            
            // 全息框架动画
            if (hologramFrame != null)
            {
                hologramFrame.DOScale(1.02f, 1f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
                
                hologramFrame.DOAnchorPosY(5f, 1.5f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }
            
            // 代码雨效果
            if (codeRainText != null)
            {
                StartCodeRainEffect();
            }
        }
        
        /// <summary>
        /// 停止加载动画
        /// </summary>
        private void StopLoadingAnimations()
        {
            if (spinnerTransform != null)
            {
                spinnerTransform.DOKill();
            }
            
            if (scanLineImage != null)
            {
                scanLineImage.rectTransform.DOKill();
            }
            
            if (hologramFrame != null)
            {
                hologramFrame.DOKill();
            }
            
            loadingSequence?.Kill();
        }
        
        /// <summary>
        /// 更新加载动画
        /// </summary>
        private void UpdateLoadingAnimation()
        {
            // 旋转spinner
            if (spinnerTransform != null)
            {
                spinnerTransform.Rotate(0, 0, -spinnerSpeed * Time.deltaTime);
            }
        }
        
        /// <summary>
        /// 更新数据块
        /// </summary>
        private void UpdateDataBlocks(float progress)
        {
            if (dataBlocks == null || dataBlocks.Length == 0) return;
            
            int activeBlocks = Mathf.FloorToInt(progress * dataBlocks.Length);
            
            for (int i = 0; i < dataBlocks.Length; i++)
            {
                if (dataBlocks[i] == null) continue;
                
                var image = dataBlocks[i].GetComponent<Image>();
                if (image == null) continue;
                
                if (i < activeBlocks)
                {
                    // 已激活
                    image.DOColor(primaryColor, 0.2f);
                    dataBlocks[i].DOScale(1.1f, 0.1f)
                        .SetEase(Ease.OutBack)
                        .OnComplete(() => dataBlocks[i].DOScale(1f, 0.1f));
                }
                else
                {
                    // 未激活
                    image.color = primaryColor.WithAlpha(0.3f);
                }
            }
        }
        
        #endregion
        
        #region 特效
        
        /// <summary>
        /// 启动故障效果
        /// </summary>
        private void StartGlitchEffect()
        {
            if (glitchOverlay == null) return;
            
            loadingSequence = DOTween.Sequence();
            
            // 随机故障闪烁
            for (int i = 0; i < 100; i++)
            {
                float delay = Random.Range(0.1f, 2f);
                float duration = Random.Range(0.02f, 0.1f);
                float alpha = Random.Range(0.1f, 0.4f);
                
                loadingSequence.AppendInterval(delay);
                loadingSequence.Append(glitchOverlay.DOColor(primaryColor.WithAlpha(alpha), duration));
                loadingSequence.Append(glitchOverlay.DOColor(primaryColor.WithAlpha(0f), duration));
            }
            
            loadingSequence.SetLoops(-1);
        }
        
        /// <summary>
        /// 启动代码雨效果
        /// </summary>
        private void StartCodeRainEffect()
        {
            string[] codeSnippets = new string[]
            {
                "0x4F2A", "0x8B3C", "0x1D9E", "0x7F4A",
                "LOAD", "SYNC", "INIT", "EXEC",
                "1010", "0101", "1100", "0011"
            };
            
            // 定期更新代码文本
            DOVirtual.DelayedCall(0.2f, () =>
            {
                if (!isLoading || codeRainText == null) return;
                
                string code = codeSnippets[Random.Range(0, codeSnippets.Length)];
                codeRainText.text = code;
                codeRainText.color = secondaryColor.WithAlpha(Random.Range(0.3f, 0.8f));
                
            }).SetLoops(-1);
        }
        
        #endregion
        
        /// <summary>
        /// 检查是否正在加载
        /// </summary>
        public bool IsLoading()
        {
            return isLoading;
        }
        
        /// <summary>
        /// 获取当前进度
        /// </summary>
        public float GetCurrentProgress()
        {
            return currentProgress;
        }
        
        private void OnDestroy()
        {
            loadingSequence?.Kill();
        }
    }
    
    #region 扩展
    
    public static class LoadingColorExtensions
    {
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
    
    #endregion
}
