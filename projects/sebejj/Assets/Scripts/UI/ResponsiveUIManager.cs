using UnityEngine;
using System;
using System.Collections.Generic;

namespace SebeJJ.UI
{
    /// <summary>
    /// 响应式UI管理器 - BUG-014修复
    /// 处理不同分辨率的UI适配
    /// </summary>
    public class ResponsiveUIManager : MonoBehaviour
    {
        public static ResponsiveUIManager Instance { get; private set; }
        
        [Header("Canvas设置")]
        [SerializeField] private CanvasScaler canvasScaler;
        [SerializeField] private Canvas mainCanvas;
        
        [Header("分辨率设置")]
        [SerializeField] private Vector2 referenceResolution = new Vector2(1920, 1080);
        [SerializeField] private float matchWidthOrHeight = 0.5f;
        
        [Header("适配设置")]
        [SerializeField] private bool autoAdjustScale = true;
        [SerializeField] private float minScaleFactor = 0.5f;
        [SerializeField] private float maxScaleFactor = 2f;
        
        [Header("安全区域")]
        [SerializeField] private bool useSafeArea = true;
        [SerializeField] private RectTransform safeAreaPanel;
        
        // 分辨率跟踪
        private int lastScreenWidth;
        private int lastScreenHeight;
        private float currentScaleFactor = 1f;
        
        // 事件
        public event Action OnResolutionChanged;
        public event Action<float> OnScaleFactorChanged;
        
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
            // 获取组件引用
            if (canvasScaler == null)
            {
                canvasScaler = FindObjectOfType<CanvasScaler>();
            }
            if (mainCanvas == null)
            {
                mainCanvas = FindObjectOfType<Canvas>();
            }
            
            // 初始化
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            
            // 应用初始设置
            ApplyResponsiveSettings();
            
            // 应用安全区域
            if (useSafeArea)
            {
                ApplySafeArea();
            }
        }
        
        private void Update()
        {
            // 检测分辨率变化
            if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
            {
                OnScreenResolutionChanged();
            }
        }
        
        /// <summary>
        /// 屏幕分辨率变化处理
        /// </summary>
        private void OnScreenResolutionChanged()
        {
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            
            Debug.Log($"[ResponsiveUI] 分辨率变化: {lastScreenWidth}x{lastScreenHeight}");
            
            ApplyResponsiveSettings();
            
            if (useSafeArea)
            {
                ApplySafeArea();
            }
            
            OnResolutionChanged?.Invoke();
        }
        
        /// <summary>
        /// 应用响应式设置
        /// </summary>
        private void ApplyResponsiveSettings()
        {
            if (canvasScaler == null) return;
            
            // 设置参考分辨率
            canvasScaler.referenceResolution = referenceResolution;
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = matchWidthOrHeight;
            
            if (autoAdjustScale)
            {
                // 计算最佳缩放因子
                float scaleFactor = CalculateOptimalScaleFactor();
                canvasScaler.scaleFactor = scaleFactor;
                
                if (Mathf.Abs(scaleFactor - currentScaleFactor) > 0.01f)
                {
                    currentScaleFactor = scaleFactor;
                    OnScaleFactorChanged?.Invoke(scaleFactor);
                }
            }
            
            // 根据屏幕比例调整UI
            AdjustUIForAspectRatio();
        }
        
        /// <summary>
        /// 计算最佳缩放因子
        /// </summary>
        private float CalculateOptimalScaleFactor()
        {
            float widthScale = (float)Screen.width / referenceResolution.x;
            float heightScale = (float)Screen.height / referenceResolution.y;
            
            // 使用匹配模式计算
            float scale = Mathf.Lerp(widthScale, heightScale, matchWidthOrHeight);
            
            // 限制在最小和最大范围内
            return Mathf.Clamp(scale, minScaleFactor, maxScaleFactor);
        }
        
        /// <summary>
        /// 根据屏幕比例调整UI
        /// </summary>
        private void AdjustUIForAspectRatio()
        {
            float aspectRatio = (float)Screen.width / Screen.height;
            
            // 超宽屏 (21:9 或更宽)
            if (aspectRatio >= 2.3f)
            {
                AdjustForUltrawide();
            }
            // 宽屏 (16:9)
            else if (aspectRatio >= 1.7f)
            {
                AdjustForWidescreen();
            }
            // 标准屏 (4:3, 16:10)
            else if (aspectRatio >= 1.3f)
            {
                AdjustForStandard();
            }
            // 竖屏或特殊比例
            else
            {
                AdjustForPortrait();
            }
        }
        
        /// <summary>
        /// 超宽屏适配
        /// </summary>
        private void AdjustForUltrawide()
        {
            // 调整侧边UI位置
            var sidePanels = FindObjectsOfType<SidePanel>();
            foreach (var panel in sidePanels)
            {
                panel.AdjustForUltrawide();
            }
        }
        
        /// <summary>
        /// 宽屏适配
        /// </summary>
        private void AdjustForWidescreen()
        {
            // 标准16:9适配
            var sidePanels = FindObjectsOfType<SidePanel>();
            foreach (var panel in sidePanels)
            {
                panel.ResetToDefault();
            }
        }
        
        /// <summary>
        /// 标准屏适配
        /// </summary>
        private void AdjustForStandard()
        {
            // 紧凑布局
            var adaptableLayouts = FindObjectsOfType<AdaptableLayout>();
            foreach (var layout in adaptableLayouts)
            {
                layout.SetCompactMode(true);
            }
        }
        
        /// <summary>
        /// 竖屏适配
        /// </summary>
        private void AdjustForPortrait()
        {
            // 竖屏特殊处理
            var adaptableLayouts = FindObjectsOfType<AdaptableLayout>();
            foreach (var layout in adaptableLayouts)
            {
                layout.SetPortraitMode(true);
            }
        }
        
        /// <summary>
        /// 应用安全区域
        /// </summary>
        private void ApplySafeArea()
        {
            if (safeAreaPanel == null) return;
            
            Rect safeArea = Screen.safeArea;
            
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            
            safeAreaPanel.anchorMin = anchorMin;
            safeAreaPanel.anchorMax = anchorMax;
        }
        
        /// <summary>
        /// 手动刷新UI
        /// </summary>
        public void RefreshUI()
        {
            ApplyResponsiveSettings();
            
            if (useSafeArea)
            {
                ApplySafeArea();
            }
        }
        
        /// <summary>
        /// 获取当前缩放因子
        /// </summary>
        public float GetCurrentScaleFactor()
        {
            return currentScaleFactor;
        }
        
        /// <summary>
        /// 设置参考分辨率
        /// </summary>
        public void SetReferenceResolution(Vector2 resolution)
        {
            referenceResolution = resolution;
            ApplyResponsiveSettings();
        }
    }
    
    #region 辅助组件
    
    /// <summary>
    /// 侧边面板适配
    /// </summary>
    public class SidePanel : MonoBehaviour
    {
        private Vector2 defaultAnchorMin;
        private Vector2 defaultAnchorMax;
        
        private void Awake()
        {
            var rectTransform = GetComponent<RectTransform>();
            defaultAnchorMin = rectTransform.anchorMin;
            defaultAnchorMax = rectTransform.anchorMax;
        }
        
        public void AdjustForUltrawide()
        {
            var rectTransform = GetComponent<RectTransform>();
            // 向内侧移动以适应超宽屏
            rectTransform.anchorMin = new Vector2(0.1f, rectTransform.anchorMin.y);
            rectTransform.anchorMax = new Vector2(0.9f, rectTransform.anchorMax.y);
        }
        
        public void ResetToDefault()
        {
            var rectTransform = GetComponent<RectTransform>();
            rectTransform.anchorMin = defaultAnchorMin;
            rectTransform.anchorMax = defaultAnchorMax;
        }
    }
    
    /// <summary>
    /// 自适应布局
    /// </summary>
    public class AdaptableLayout : MonoBehaviour
    {
        private Vector2 defaultSpacing;
        private bool isCompactMode = false;
        private bool isPortraitMode = false;
        
        private void Awake()
        {
            var layoutGroup = GetComponent<UnityEngine.UI.HorizontalOrVerticalLayoutGroup>();
            if (layoutGroup != null)
            {
                defaultSpacing = new Vector2(layoutGroup.spacing, layoutGroup.spacing);
            }
        }
        
        public void SetCompactMode(bool compact)
        {
            if (isCompactMode == compact) return;
            isCompactMode = compact;
            
            var layoutGroup = GetComponent<UnityEngine.UI.HorizontalOrVerticalLayoutGroup>();
            if (layoutGroup != null)
            {
                layoutGroup.spacing = compact ? defaultSpacing.x * 0.7f : defaultSpacing.x;
            }
        }
        
        public void SetPortraitMode(bool portrait)
        {
            if (isPortraitMode == portrait) return;
            isPortraitMode = portrait;
            
            // 竖屏布局调整
        }
    }
    
    #endregion
}
