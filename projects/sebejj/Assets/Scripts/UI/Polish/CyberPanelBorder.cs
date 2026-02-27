using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SebeJJ.UI.Polish
{
    /// <summary>
    /// 赛博朋克风格面板边框 - 动态边框、扫描线、角标装饰
    /// </summary>
    public class CyberPanelBorder : MonoBehaviour
    {
        [Header("边框组件")]
        [SerializeField] private Image borderTop;
        [SerializeField] private Image borderBottom;
        [SerializeField] private Image borderLeft;
        [SerializeField] private Image borderRight;
        
        [Header("角标装饰")]
        [SerializeField] private RectTransform cornerTL; // 左上
        [SerializeField] private RectTransform cornerTR; // 右上
        [SerializeField] private RectTransform cornerBL; // 左下
        [SerializeField] private RectTransform cornerBR; // 右下
        
        [Header("扫描线")]
        [SerializeField] private Image scanLineHorizontal;
        [SerializeField] private Image scanLineVertical;
        [SerializeField] private float scanSpeed = 2f;
        
        [Header("发光效果")]
        [SerializeField] private Image glowBorder;
        [SerializeField] private float glowIntensity = 0.6f;
        [SerializeField] private float glowPulseSpeed = 1f;
        
        [Header("配色")]
        [SerializeField] private Color primaryColor = new Color(0.2f, 0.9f, 1f, 1f);
        [SerializeField] private Color secondaryColor = new Color(0.8f, 0.2f, 0.9f, 1f);
        [SerializeField] private float borderAlpha = 0.8f;
        
        [Header("动画设置")]
        [SerializeField] private bool playEntryAnimation = true;
        [SerializeField] private float entryDuration = 0.5f;
        [SerializeField] private float cornerStaggerDelay = 0.1f;
        
        // 动画序列
        private Sequence scanSequence;
        private Sequence glowSequence;
        private Sequence cornerSequence;
        
        private void Start()
        {
            InitializeBorders();
            InitializeCorners();
            InitializeScanLines();
            InitializeGlow();
            
            if (playEntryAnimation)
            {
                PlayEntryAnimation();
            }
            else
            {
                StartAmbientAnimations();
            }
        }
        
        private void OnDestroy()
        {
            KillAllSequences();
        }
        
        /// <summary>
        /// 初始化边框
        /// </summary>
        private void InitializeBorders()
        {
            Color borderColor = primaryColor.WithAlpha(borderAlpha);
            
            if (borderTop != null) borderTop.color = borderColor;
            if (borderBottom != null) borderBottom.color = borderColor;
            if (borderLeft != null) borderLeft.color = borderColor;
            if (borderRight != null) borderRight.color = borderColor;
        }
        
        /// <summary>
        /// 初始化角标
        /// </summary>
        private void InitializeCorners()
        {
            // 设置角标初始状态
            SetCornerState(cornerTL, 0f);
            SetCornerState(cornerTR, 0f);
            SetCornerState(cornerBL, 0f);
            SetCornerState(cornerBR, 0f);
        }
        
        /// <summary>
        /// 设置角标状态
        /// </summary>
        private void SetCornerState(RectTransform corner, float alpha)
        {
            if (corner == null) return;
            
            var images = corner.GetComponentsInChildren<Image>();
            foreach (var img in images)
            {
                img.color = primaryColor.WithAlpha(alpha);
            }
        }
        
        /// <summary>
        /// 初始化扫描线
        /// </summary>
        private void InitializeScanLines()
        {
            if (scanLineHorizontal != null)
            {
                scanLineHorizontal.color = secondaryColor.WithAlpha(0.3f);
                scanLineHorizontal.fillAmount = 0f;
            }
            
            if (scanLineVertical != null)
            {
                scanLineVertical.color = secondaryColor.WithAlpha(0.3f);
                scanLineVertical.fillAmount = 0f;
            }
        }
        
        /// <summary>
        /// 初始化发光效果
        /// </summary>
        private void InitializeGlow()
        {
            if (glowBorder != null)
            {
                glowBorder.color = primaryColor.WithAlpha(0f);
            }
        }
        
        /// <summary>
        /// 播放进入动画
        /// </summary>
        private void PlayEntryAnimation()
        {
            Sequence entrySequence = DOTween.Sequence();
            
            // 边框绘制动画
            entrySequence.Append(DrawBordersAnimation());
            
            // 角标依次出现
            entrySequence.AppendCallback(() => PlayCornerEntryAnimation());
            
            // 完成后启动环境动画
            entrySequence.OnComplete(StartAmbientAnimations);
        }
        
        /// <summary>
        /// 边框绘制动画
        /// </summary>
        private Sequence DrawBordersAnimation()
        {
            Sequence sequence = DOTween.Sequence();
            
            // 顶部边框从左到右
            if (borderTop != null)
            {
                borderTop.fillOrigin = (int)Image.OriginHorizontal.Left;
                borderTop.fillAmount = 0f;
                sequence.Join(borderTop.DOFillAmount(1f, entryDuration * 0.25f).SetEase(Ease.OutQuad));
            }
            
            // 底部边框从右到左
            if (borderBottom != null)
            {
                borderBottom.fillOrigin = (int)Image.OriginHorizontal.Right;
                borderBottom.fillAmount = 0f;
                sequence.Join(borderBottom.DOFillAmount(1f, entryDuration * 0.25f).SetEase(Ease.OutQuad));
            }
            
            // 左边框从上到下
            if (borderLeft != null)
            {
                borderLeft.fillOrigin = (int)Image.OriginVertical.Top;
                borderLeft.fillAmount = 0f;
                sequence.Join(borderLeft.DOFillAmount(1f, entryDuration * 0.25f).SetEase(Ease.OutQuad));
            }
            
            // 右边框从下到上
            if (borderRight != null)
            {
                borderRight.fillOrigin = (int)Image.OriginVertical.Bottom;
                borderRight.fillAmount = 0f;
                sequence.Join(borderRight.DOFillAmount(1f, entryDuration * 0.25f).SetEase(Ease.OutQuad));
            }
            
            return sequence;
        }
        
        /// <summary>
        /// 角标进入动画
        /// </summary>
        private void PlayCornerEntryAnimation()
        {
            cornerSequence = DOTween.Sequence();
            
            RectTransform[] corners = { cornerTL, cornerTR, cornerBL, cornerBR };
            
            for (int i = 0; i < corners.Length; i++)
            {
                if (corners[i] == null) continue;
                
                corners[i].localScale = Vector3.zero;
                
                cornerSequence.Insert(i * cornerStaggerDelay,
                    corners[i].DOScale(1f, 0.3f).SetEase(Ease.OutBack));
                
                // 角标颜色渐变
                var images = corners[i].GetComponentsInChildren<Image>();
                foreach (var img in images)
                {
                    img.color = primaryColor.WithAlpha(0f);
                    cornerSequence.Insert(i * cornerStaggerDelay,
                        img.DOColor(primaryColor.WithAlpha(1f), 0.3f));
                }
            }
        }
        
        /// <summary>
        /// 启动环境动画
        /// </summary>
        private void StartAmbientAnimations()
        {
            StartScanLineAnimation();
            StartGlowPulseAnimation();
            StartCornerIdleAnimation();
        }
        
        /// <summary>
        /// 扫描线动画
        /// </summary>
        private void StartScanLineAnimation()
        {
            scanSequence = DOTween.Sequence();
            
            // 水平扫描线
            if (scanLineHorizontal != null)
            {
                var rectTransform = scanLineHorizontal.rectTransform;
                float panelHeight = ((RectTransform)transform).rect.height;
                
                rectTransform.anchoredPosition = new Vector2(0, panelHeight * 0.5f);
                
                scanSequence.Append(rectTransform.DOAnchorPosY(-panelHeight * 0.5f, 3f / scanSpeed)
                    .SetEase(Ease.Linear));
                scanLineHorizontal.DOFade(0f, 0.1f).SetLoops(2, LoopType.Yoyo);
            }
            
            // 垂直扫描线
            if (scanLineVertical != null)
            {
                var rectTransform = scanLineVertical.rectTransform;
                float panelWidth = ((RectTransform)transform).rect.width;
                
                rectTransform.anchoredPosition = new Vector2(-panelWidth * 0.5f, 0);
                
                scanSequence.Join(rectTransform.DOAnchorPosX(panelWidth * 0.5f, 4f / scanSpeed)
                    .SetEase(Ease.Linear));
                scanLineVertical.DOFade(0f, 0.1f).SetLoops(2, LoopType.Yoyo);
            }
            
            scanSequence.SetLoops(-1, LoopType.Restart);
            scanSequence.SetDelay(1f);
        }
        
        /// <summary>
        /// 发光脉冲动画
        /// </summary>
        private void StartGlowPulseAnimation()
        {
            if (glowBorder == null) return;
            
            glowSequence = DOTween.Sequence();
            
            glowBorder.color = primaryColor.WithAlpha(glowIntensity * 0.5f);
            
            glowSequence.Append(glowBorder.DOColor(primaryColor.WithAlpha(glowIntensity), 1f / glowPulseSpeed)
                .SetEase(Ease.InOutSine));
            glowSequence.Append(glowBorder.DOColor(primaryColor.WithAlpha(glowIntensity * 0.3f), 1f / glowPulseSpeed)
                .SetEase(Ease.InOutSine));
            
            glowSequence.SetLoops(-1, LoopType.Yoyo);
        }
        
        /// <summary>
        /// 角标空闲动画
        /// </summary>
        private void StartCornerIdleAnimation()
        {
            RectTransform[] corners = { cornerTL, cornerTR, cornerBL, cornerBR };
            
            for (int i = 0; i < corners.Length; i++)
            {
                if (corners[i] == null) continue;
                
                // 轻微旋转
                corners[i].DORotate(new Vector3(0, 0, i % 2 == 0 ? 5f : -5f), 2f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
                
                // 轻微缩放
                corners[i].DOScale(1.05f, 1.5f + i * 0.2f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            }
        }
        
        /// <summary>
        /// 高亮边框
        /// </summary>
        public void HighlightBorder(Color highlightColor, float duration = 0.5f)
        {
            Sequence highlightSequence = DOTween.Sequence();
            
            // 边框高亮
            if (borderTop != null)
                highlightSequence.Join(borderTop.DOColor(highlightColor, duration * 0.5f));
            if (borderBottom != null)
                highlightSequence.Join(borderBottom.DOColor(highlightColor, duration * 0.5f));
            if (borderLeft != null)
                highlightSequence.Join(borderLeft.DOColor(highlightColor, duration * 0.5f));
            if (borderRight != null)
                highlightSequence.Join(borderRight.DOColor(highlightColor, duration * 0.5f));
            
            // 恢复
            Color originalColor = primaryColor.WithAlpha(borderAlpha);
            highlightSequence.AppendInterval(duration * 0.5f);
            
            if (borderTop != null)
                highlightSequence.Append(borderTop.DOColor(originalColor, duration * 0.5f));
            if (borderBottom != null)
                highlightSequence.Join(borderBottom.DOColor(originalColor, duration * 0.5f));
            if (borderLeft != null)
                highlightSequence.Join(borderLeft.DOColor(originalColor, duration * 0.5f));
            if (borderRight != null)
                highlightSequence.Join(borderRight.DOColor(originalColor, duration * 0.5f));
        }
        
        /// <summary>
        /// 警告闪烁
        /// </summary>
        public void WarningFlash(float duration = 2f)
        {
            Color warningColor = new Color(0.95f, 0.2f, 0.1f, 1f);
            
            Sequence warningSequence = DOTween.Sequence();
            
            // 快速闪烁
            for (int i = 0; i < 6; i++)
            {
                warningSequence.AppendCallback(() => SetBorderColor(warningColor));
                warningSequence.AppendInterval(0.15f);
                warningSequence.AppendCallback(() => SetBorderColor(primaryColor.WithAlpha(borderAlpha)));
                warningSequence.AppendInterval(0.15f);
            }
            
            warningSequence.OnComplete(() => InitializeBorders());
        }
        
        /// <summary>
        /// 设置边框颜色
        /// </summary>
        private void SetBorderColor(Color color)
        {
            if (borderTop != null) borderTop.color = color;
            if (borderBottom != null) borderBottom.color = color;
            if (borderLeft != null) borderLeft.color = color;
            if (borderRight != null) borderRight.color = color;
        }
        
        /// <summary>
        /// 停止所有序列
        /// </summary>
        private void KillAllSequences()
        {
            scanSequence?.Kill();
            glowSequence?.Kill();
            cornerSequence?.Kill();
        }
        
        /// <summary>
        /// 设置主题色
        /// </summary>
        public void SetThemeColors(Color primary, Color secondary)
        {
            primaryColor = primary;
            secondaryColor = secondary;
            InitializeBorders();
        }
    }
    
    /// <summary>
    /// 颜色扩展
    /// </summary>
    public static class PanelColorExtensions
    {
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
}
