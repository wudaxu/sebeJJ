using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SebeJJ.UI.Polish
{
    /// <summary>
    /// 4K分辨率适配器 - 高分辨率屏幕优化
    /// </summary>
    public class Resolution4KAdapter : MonoBehaviour
    {
        public static Resolution4KAdapter Instance { get; private set; }
        
        [Header("分辨率阈值")]
        [SerializeField] private int hdThreshold = 1280;
        [SerializeField] private int fhdThreshold = 1920;
        [SerializeField] private int qhdThreshold = 2560;
        [SerializeField] private int uhdThreshold = 3840;
        
        [Header("Canvas设置")]
        [SerializeField] private CanvasScaler canvasScaler;
        [SerializeField] private float referenceWidth = 1920f;
        [SerializeField] private float referenceHeight = 1080f;
        
        [Header("字体缩放")]
        [SerializeField] private bool autoScaleFonts = true;
        [SerializeField] private float fontScaleFactor = 1f;
        
        [Header("UI元素缩放")]
        [SerializeField] private bool autoScaleUI = true;
        [SerializeField] private RectTransform[] scalableElements;
        
        [Header("贴图质量")]
        [SerializeField] private bool adjustTextureQuality = true;
        
        // 当前分辨率类型
        private ResolutionType currentResolutionType = ResolutionType.FHD;
        private float currentScaleFactor = 1f;
        
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
            if (canvasScaler == null)
            {
                canvasScaler = FindObjectOfType<CanvasScaler>();
            }
            
            ApplyResolutionAdaptation();
        }
        
        private void Update()
        {
            // 检测分辨率变化
            CheckResolutionChange();
        }
        
        /// <summary>
        /// 检测分辨率变化
        /// </summary>
        private void CheckResolutionChange()
        {
            ResolutionType newType = GetResolutionType(Screen.width);
            
            if (newType != currentResolutionType)
            {
                currentResolutionType = newType;
                ApplyResolutionAdaptation();
            }
        }
        
        /// <summary>
        /// 获取分辨率类型
        /// </summary>
        private ResolutionType GetResolutionType(int width)
        {
            if (width >= uhdThreshold)
                return ResolutionType.UHD;
            else if (width >= qhdThreshold)
                return ResolutionType.QHD;
            else if (width >= fhdThreshold)
                return ResolutionType.FHD;
            else if (width >= hdThreshold)
                return ResolutionType.HD;
            else
                return ResolutionType.SD;
        }
        
        /// <summary>
        /// 应用分辨率适配
        /// </summary>
        public void ApplyResolutionAdaptation()
        {
            currentResolutionType = GetResolutionType(Screen.width);
            
            // 计算缩放因子
            CalculateScaleFactor();
            
            // 应用CanvasScaler设置
            ApplyCanvasScalerSettings();
            
            // 缩放字体
            if (autoScaleFonts)
            {
                ScaleFonts();
            }
            
            // 缩放UI元素
            if (autoScaleUI)
            {
                ScaleUIElements();
            }
            
            // 调整贴图质量
            if (adjustTextureQuality)
            {
                AdjustTextureQuality();
            }
            
            Debug.Log($"[Resolution4KAdapter] 当前分辨率: {Screen.width}x{Screen.height}, 类型: {currentResolutionType}, 缩放因子: {currentScaleFactor}");
        }
        
        /// <summary>
        /// 计算缩放因子
        /// </summary>
        private void CalculateScaleFactor()
        {
            float widthScale = Screen.width / referenceWidth;
            float heightScale = Screen.height / referenceHeight;
            
            // 使用宽度作为主要缩放参考
            currentScaleFactor = Mathf.Lerp(widthScale, heightScale, 0.5f);
            
            // 限制缩放范围
            currentScaleFactor = Mathf.Clamp(currentScaleFactor, 0.5f, 3f);
        }
        
        /// <summary>
        /// 应用CanvasScaler设置
        /// </summary>
        private void ApplyCanvasScalerSettings()
        {
            if (canvasScaler == null) return;
            
            switch (currentResolutionType)
            {
                case ResolutionType.UHD:
                    // 4K分辨率使用物理像素模式
                    canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    canvasScaler.referenceResolution = new Vector2(referenceWidth, referenceHeight);
                    canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    canvasScaler.matchWidthOrHeight = 0.5f;
                    break;
                    
                case ResolutionType.QHD:
                    // 2K分辨率
                    canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    canvasScaler.referenceResolution = new Vector2(referenceWidth, referenceHeight);
                    canvasScaler.matchWidthOrHeight = 0.5f;
                    break;
                    
                case ResolutionType.FHD:
                    // 1080p标准
                    canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    canvasScaler.referenceResolution = new Vector2(referenceWidth, referenceHeight);
                    canvasScaler.matchWidthOrHeight = 0.5f;
                    break;
                    
                case ResolutionType.HD:
                case ResolutionType.SD:
                    // 低分辨率使用常量物理大小
                    canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                    canvasScaler.scaleFactor = 1f;
                    break;
            }
        }
        
        /// <summary>
        /// 缩放字体
        /// </summary>
        private void ScaleFonts()
        {
            Text[] allTexts = FindObjectsOfType<Text>();
            
            foreach (var text in allTexts)
            {
                // 保存原始字体大小
                if (!text.gameObject.TryGetComponent<OriginalFontSize>(out var originalSize))
                {
                    originalSize = text.gameObject.AddComponent<OriginalFontSize>();
                    originalSize.size = text.fontSize;
                }
                
                // 应用缩放
                int newSize = Mathf.RoundToInt(originalSize.size * currentScaleFactor * fontScaleFactor);
                text.fontSize = newSize;
            }
        }
        
        /// <summary>
        /// 缩放UI元素
        /// </summary>
        private void ScaleUIElements()
        {
            if (scalableElements == null) return;
            
            foreach (var element in scalableElements)
            {
                if (element == null) continue;
                
                // 保存原始缩放
                if (!element.TryGetComponent<OriginalScale>(out var originalScale))
                {
                    originalScale = element.gameObject.AddComponent<OriginalScale>();
                    originalScale.scale = element.localScale;
                }
                
                // 应用缩放
                element.localScale = originalScale.scale * currentScaleFactor;
            }
        }
        
        /// <summary>
        /// 调整贴图质量
        /// </summary>
        private void AdjustTextureQuality()
        {
            switch (currentResolutionType)
            {
                case ResolutionType.UHD:
                    // 4K使用最高质量
                    QualitySettings.masterTextureLimit = 0;
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
                    break;
                    
                case ResolutionType.QHD:
                    // 2K使用高质量
                    QualitySettings.masterTextureLimit = 0;
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
                    break;
                    
                case ResolutionType.FHD:
                    // 1080p使用标准质量
                    QualitySettings.masterTextureLimit = 0;
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
                    break;
                    
                case ResolutionType.HD:
                case ResolutionType.SD:
                    // 低分辨率可以降低贴图质量以提高性能
                    QualitySettings.masterTextureLimit = 1;
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                    break;
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
        /// 获取当前分辨率类型
        /// </summary>
        public ResolutionType GetCurrentResolutionType()
        {
            return currentResolutionType;
        }
        
        /// <summary>
        /// 检查是否为4K
        /// </summary>
        public bool Is4K()
        {
            return currentResolutionType == ResolutionType.UHD;
        }
        
        /// <summary>
        /// 检查是否为2K或更高
        /// </summary>
        public bool Is2KOrHigher()
        {
            return currentResolutionType == ResolutionType.QHD || 
                   currentResolutionType == ResolutionType.UHD;
        }
    }
    
    #region 数据定义
    
    public enum ResolutionType
    {
        SD,     // 标清 (720p以下)
        HD,     // 高清 (720p)
        FHD,    // 全高清 (1080p)
        QHD,    // 2K (1440p)
        UHD     // 4K (2160p)
    }
    
    // 用于保存原始值的组件
    public class OriginalFontSize : MonoBehaviour
    {
        public int size;
    }
    
    public class OriginalScale : MonoBehaviour
    {
        public Vector3 scale;
    }
    
    #endregion
}
