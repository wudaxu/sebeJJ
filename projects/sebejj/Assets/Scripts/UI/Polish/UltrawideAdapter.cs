using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SebeJJ.UI.Polish
{
    /// <summary>
    /// 超宽屏适配器 - 21:9和更宽屏幕的UI适配
    /// </summary>
    public class UltrawideAdapter : MonoBehaviour
    {
        public static UltrawideAdapter Instance { get; private set; }
        
        [Header("适配设置")]
        [SerializeField] private float ultrawideThreshold = 2.3f;
        [SerializeField] private float superUltrawideThreshold = 3.0f;
        
        [Header("UI元素")]
        [SerializeField] private RectTransform leftPanel;
        [SerializeField] private RectTransform rightPanel;
        [SerializeField] private RectTransform centerContent;
        [SerializeField] private RectTransform[] sideElements;
        
        [Header("适配参数")]
        [SerializeField] private float ultrawideSideOffset = 100f;
        [SerializeField] private float superUltrawideSideOffset = 200f;
        [SerializeField] private float centerContentMaxWidth = 1920f;
        
        [Header("背景适配")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image[] sideBackgrounds;
        
        // 原始位置缓存
        private Vector2 originalLeftPanelPos;
        private Vector2 originalRightPanelPos;
        private Vector2[] originalSideElementPos;
        
        // 当前状态
        private ScreenType currentScreenType = ScreenType.Standard;
        
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
            CacheOriginalPositions();
            ApplyUltrawideAdaptation();
        }
        
        private void Update()
        {
            // 检测分辨率变化
            CheckResolutionChange();
        }
        
        /// <summary>
        /// 缓存原始位置
        /// </summary>
        private void CacheOriginalPositions()
        {
            if (leftPanel != null)
                originalLeftPanelPos = leftPanel.anchoredPosition;
            
            if (rightPanel != null)
                originalRightPanelPos = rightPanel.anchoredPosition;
            
            if (sideElements != null)
            {
                originalSideElementPos = new Vector2[sideElements.Length];
                for (int i = 0; i < sideElements.Length; i++)
                {
                    if (sideElements[i] != null)
                        originalSideElementPos[i] = sideElements[i].anchoredPosition;
                }
            }
        }
        
        /// <summary>
        /// 检测分辨率变化
        /// </summary>
        private void CheckResolutionChange()
        {
            float aspectRatio = (float)Screen.width / Screen.height;
            ScreenType newScreenType = GetScreenType(aspectRatio);
            
            if (newScreenType != currentScreenType)
            {
                currentScreenType = newScreenType;
                ApplyUltrawideAdaptation();
            }
        }
        
        /// <summary>
        /// 获取屏幕类型
        /// </summary>
        private ScreenType GetScreenType(float aspectRatio)
        {
            if (aspectRatio >= superUltrawideThreshold)
                return ScreenType.SuperUltrawide;
            else if (aspectRatio >= ultrawideThreshold)
                return ScreenType.Ultrawide;
            else
                return ScreenType.Standard;
        }
        
        /// <summary>
        /// 应用超宽屏适配
        /// </summary>
        public void ApplyUltrawideAdaptation()
        {
            float aspectRatio = (float)Screen.width / Screen.height;
            
            switch (currentScreenType)
            {
                case ScreenType.SuperUltrawide:
                    ApplySuperUltrawideLayout();
                    break;
                case ScreenType.Ultrawide:
                    ApplyUltrawideLayout();
                    break;
                default:
                    ApplyStandardLayout();
                    break;
            }
            
            // 调整背景
            AdjustBackground();
            
            // 调整中心内容
            AdjustCenterContent();
        }
        
        /// <summary>
        /// 应用标准布局
        /// </summary>
        private void ApplyStandardLayout()
        {
            if (leftPanel != null)
                leftPanel.anchoredPosition = originalLeftPanelPos;
            
            if (rightPanel != null)
                rightPanel.anchoredPosition = originalRightPanelPos;
            
            ResetSideElements();
        }
        
        /// <summary>
        /// 应用超宽屏布局
        /// </summary>
        private void ApplyUltrawideLayout()
        {
            float offset = ultrawideSideOffset;
            
            if (leftPanel != null)
            {
                Vector2 newPos = originalLeftPanelPos;
                newPos.x += offset;
                leftPanel.DOAnchorPos(newPos, 0.3f).SetEase(Ease.OutQuad);
            }
            
            if (rightPanel != null)
            {
                Vector2 newPos = originalRightPanelPos;
                newPos.x -= offset;
                rightPanel.DOAnchorPos(newPos, 0.3f).SetEase(Ease.OutQuad);
            }
            
            AdjustSideElements(offset);
        }
        
        /// <summary>
        /// 应用超超宽屏布局
        /// </summary>
        private void ApplySuperUltrawideLayout()
        {
            float offset = superUltrawideSideOffset;
            
            if (leftPanel != null)
            {
                Vector2 newPos = originalLeftPanelPos;
                newPos.x += offset;
                leftPanel.DOAnchorPos(newPos, 0.3f).SetEase(Ease.OutQuad);
            }
            
            if (rightPanel != null)
            {
                Vector2 newPos = originalRightPanelPos;
                newPos.x -= offset;
                rightPanel.DOAnchorPos(newPos, 0.3f).SetEase(Ease.OutQuad);
            }
            
            AdjustSideElements(offset);
            
            // 添加额外的侧边装饰
            EnableSideDecorations(true);
        }
        
        /// <summary>
        /// 调整侧边元素
        /// </summary>
        private void AdjustSideElements(float offset)
        {
            if (sideElements == null) return;
            
            for (int i = 0; i < sideElements.Length; i++)
            {
                if (sideElements[i] == null) continue;
                
                Vector2 newPos = originalSideElementPos[i];
                
                // 根据位置决定偏移方向
                if (newPos.x < 0)
                    newPos.x -= offset * 0.5f;
                else if (newPos.x > 0)
                    newPos.x += offset * 0.5f;
                
                sideElements[i].DOAnchorPos(newPos, 0.3f).SetEase(Ease.OutQuad);
            }
        }
        
        /// <summary>
        /// 重置侧边元素
        /// </summary>
        private void ResetSideElements()
        {
            if (sideElements == null || originalSideElementPos == null) return;
            
            for (int i = 0; i < sideElements.Length; i++)
            {
                if (sideElements[i] == null) continue;
                sideElements[i].DOAnchorPos(originalSideElementPos[i], 0.3f).SetEase(Ease.OutQuad);
            }
            
            EnableSideDecorations(false);
        }
        
        /// <summary>
        /// 启用/禁用侧边装饰
        /// </summary>
        private void EnableSideDecorations(bool enable)
        {
            if (sideBackgrounds == null) return;
            
            foreach (var bg in sideBackgrounds)
            {
                if (bg == null) continue;
                
                if (enable)
                {
                    bg.gameObject.SetActive(true);
                    bg.DOFade(0.5f, 0.3f).From(0f);
                }
                else
                {
                    bg.DOFade(0f, 0.3f).OnComplete(() => bg.gameObject.SetActive(false));
                }
            }
        }
        
        /// <summary>
        /// 调整背景
        /// </summary>
        private void AdjustBackground()
        {
            if (backgroundImage == null) return;
            
            float aspectRatio = (float)Screen.width / Screen.height;
            
            // 超宽屏使用不同的背景适配模式
            if (aspectRatio >= ultrawideThreshold)
            {
                // 使用填充模式保持背景完整
                backgroundImage.preserveAspect = true;
            }
            else
            {
                backgroundImage.preserveAspect = false;
            }
        }
        
        /// <summary>
        /// 调整中心内容
        /// </summary>
        private void AdjustCenterContent()
        {
            if (centerContent == null) return;
            
            float screenWidth = Screen.width;
            float maxWidth = centerContentMaxWidth;
            
            // 限制中心内容最大宽度
            if (screenWidth > maxWidth)
            {
                centerContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);
            }
            else
            {
                centerContent.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, screenWidth * 0.9f);
            }
        }
        
        /// <summary>
        /// 获取当前屏幕类型
        /// </summary>
        public ScreenType GetCurrentScreenType()
        {
            return currentScreenType;
        }
        
        /// <summary>
        /// 检查是否为超宽屏
        /// </summary>
        public bool IsUltrawide()
        {
            return currentScreenType == ScreenType.Ultrawide || 
                   currentScreenType == ScreenType.SuperUltrawide;
        }
    }
    
    #region 数据定义
    
    public enum ScreenType
    {
        Standard,       // 标准屏幕 (16:9及以下)
        Ultrawide,      // 超宽屏 (21:9)
        SuperUltrawide  // 超超宽屏 (32:9等)
    }
    
    #endregion
    
    #region 扩展
    
    public static class UltrawideColorExtensions
    {
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
    
    #endregion
}
