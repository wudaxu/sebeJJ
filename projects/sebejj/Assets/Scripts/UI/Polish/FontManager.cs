using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SebeJJ.UI.Polish
{
    /// <summary>
    /// 字体管理器 - 统一字体管理和动态效果
    /// </summary>
    public class FontManager : MonoBehaviour
    {
        public static FontManager Instance { get; private set; }
        
        [Header("字体资源")]
        [SerializeField] private Font primaryFont;
        [SerializeField] private Font secondaryFont;
        [SerializeField] private Font monospaceFont;
        
        [Header("字体预设")]
        [SerializeField] private FontPreset titlePreset;
        [SerializeField] private FontPreset headerPreset;
        [SerializeField] private FontPreset bodyPreset;
        [SerializeField] private FontPreset captionPreset;
        [SerializeField] private FontPreset codePreset;
        
        [Header("动态效果")]
        [SerializeField] private bool enableTypewriterEffect = true;
        [SerializeField] private float typewriterSpeed = 0.05f;
        
        // 字体缓存
        private System.Collections.Generic.Dictionary<Text, FontPreset> appliedPresets = 
            new System.Collections.Generic.Dictionary<Text, FontPreset>();
        
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
        
        /// <summary>
        /// 应用字体预设
        /// </summary>
        public void ApplyPreset(Text textComponent, FontPresetType presetType)
        {
            FontPreset preset = GetPreset(presetType);
            if (preset == null || textComponent == null) return;
            
            ApplyPresetToText(textComponent, preset);
            appliedPresets[textComponent] = preset;
        }
        
        /// <summary>
        /// 获取预设
        /// </summary>
        private FontPreset GetPreset(FontPresetType type)
        {
            switch (type)
            {
                case FontPresetType.Title:
                    return titlePreset;
                case FontPresetType.Header:
                    return headerPreset;
                case FontPresetType.Body:
                    return bodyPreset;
                case FontPresetType.Caption:
                    return captionPreset;
                case FontPresetType.Code:
                    return codePreset;
                default:
                    return bodyPreset;
            }
        }
        
        /// <summary>
        /// 应用预设到文本
        /// </summary>
        private void ApplyPresetToText(Text textComponent, FontPreset preset)
        {
            textComponent.font = preset.font ?? primaryFont;
            textComponent.fontSize = preset.fontSize;
            textComponent.fontStyle = preset.fontStyle;
            textComponent.color = preset.color;
            textComponent.alignment = preset.alignment;
            textComponent.lineSpacing = preset.lineSpacing;
            
            // 应用材质效果
            if (preset.useOutline)
            {
                ApplyOutlineEffect(textComponent, preset.outlineColor, preset.outlineWidth);
            }
            
            if (preset.useShadow)
            {
                ApplyShadowEffect(textComponent, preset.shadowColor, preset.shadowOffset);
            }
            
            if (preset.useGlow)
            {
                ApplyGlowEffect(textComponent, preset.glowColor, preset.glowIntensity);
            }
        }
        
        /// <summary>
        /// 应用描边效果
        /// </summary>
        private void ApplyOutlineEffect(Text textComponent, Color color, float width)
        {
            Outline outline = textComponent.GetComponent<Outline>();
            if (outline == null)
            {
                outline = textComponent.gameObject.AddComponent<Outline>();
            }
            
            outline.effectColor = color;
            outline.effectDistance = new Vector2(width, width);
        }
        
        /// <summary>
        /// 应用阴影效果
        /// </summary>
        private void ApplyShadowEffect(Text textComponent, Color color, Vector2 offset)
        {
            Shadow shadow = textComponent.GetComponent<Shadow>();
            if (shadow == null)
            {
                shadow = textComponent.gameObject.AddComponent<Shadow>();
            }
            
            shadow.effectColor = color;
            shadow.effectDistance = offset;
        }
        
        /// <summary>
        /// 应用发光效果
        /// </summary>
        private void ApplyGlowEffect(Text textComponent, Color color, float intensity)
        {
            // 使用多个Outline模拟发光
            for (int i = 0; i < 3; i++)
            {
                Outline glow = textComponent.gameObject.AddComponent<Outline>();
                glow.effectColor = color.WithAlpha(intensity / (i + 1));
                glow.effectDistance = new Vector2((i + 1) * 2, (i + 1) * 2);
            }
        }
        
        #region 动态效果
        
        /// <summary>
        /// 打字机效果
        /// </summary>
        public void PlayTypewriterEffect(Text textComponent, string text, float speed = -1f, 
            System.Action onComplete = null)
        {
            if (!enableTypewriterEffect) return;
            
            float charDelay = speed > 0 ? speed : typewriterSpeed;
            textComponent.text = "";
            
            Sequence sequence = DOTween.Sequence();
            
            for (int i = 0; i < text.Length; i++)
            {
                int index = i;
                sequence.AppendCallback(() =>
                {
                    textComponent.text += text[index];
                });
                sequence.AppendInterval(charDelay);
            }
            
            sequence.OnComplete(() => onComplete?.Invoke());
        }
        
        /// <summary>
        /// 文字闪烁效果
        /// </summary>
        public void PlayBlinkEffect(Text textComponent, float duration = 1f, int blinkCount = 3)
        {
            Color originalColor = textComponent.color;
            
            Sequence sequence = DOTween.Sequence();
            
            for (int i = 0; i < blinkCount; i++)
            {
                sequence.Append(textComponent.DOColor(originalColor.WithAlpha(0.3f), duration / (blinkCount * 2)));
                sequence.Append(textComponent.DOColor(originalColor, duration / (blinkCount * 2)));
            }
        }
        
        /// <summary>
        /// 文字渐变效果
        /// </summary>
        public void PlayGradientEffect(Text textComponent, Color startColor, Color endColor, float duration = 2f)
        {
            textComponent.DOColor(endColor, duration)
                .From(startColor)
                .SetEase(Ease.InOutQuad)
                .SetLoops(-1, LoopType.Yoyo);
        }
        
        /// <summary>
        /// 文字脉冲效果
        /// </summary>
        public void PlayPulseEffect(Text textComponent, float minScale = 0.95f, float maxScale = 1.05f, 
            float duration = 1f)
        {
            RectTransform rectTransform = textComponent.rectTransform;
            
            rectTransform.DOScale(maxScale, duration * 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
            
            textComponent.DOColor(textComponent.color.Brighten(0.2f), duration * 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
        
        /// <summary>
        /// 文字抖动效果
        /// </summary>
        public void PlayShakeEffect(Text textComponent, float duration = 0.3f, float intensity = 2f)
        {
            rectTransform = textComponent.rectTransform;
            rectTransform.DOShakeAnchorPos(duration, intensity, 10, 90, false, true);
        }
        
        private RectTransform rectTransform;
        
        /// <summary>
        /// 停止所有效果
        /// </summary>
        public void StopAllEffects(Text textComponent)
        {
            textComponent.DOKill();
            textComponent.rectTransform.DOKill();
        }
        
        #endregion
        
        /// <summary>
        /// 批量应用预设
        /// </summary>
        public void ApplyPresetToAll(Text[] textComponents, FontPresetType presetType)
        {
            foreach (var text in textComponents)
            {
                ApplyPreset(text, presetType);
            }
        }
        
        /// <summary>
        /// 恢复默认设置
        /// </summary>
        public void ResetToDefault(Text textComponent)
        {
            if (appliedPresets.ContainsKey(textComponent))
            {
                ApplyPresetToText(textComponent, bodyPreset);
                appliedPresets.Remove(textComponent);
            }
        }
    }
    
    #region 数据定义
    
    [System.Serializable]
    public class FontPreset
    {
        public Font font;
        public int fontSize = 24;
        public FontStyle fontStyle = FontStyle.Normal;
        public Color color = Color.white;
        public TextAnchor alignment = TextAnchor.MiddleCenter;
        public float lineSpacing = 1f;
        
        [Header("效果")]
        public bool useOutline = false;
        public Color outlineColor = Color.black;
        public float outlineWidth = 1f;
        
        public bool useShadow = false;
        public Color shadowColor = new Color(0, 0, 0, 0.5f);
        public Vector2 shadowOffset = new Vector2(1, -1);
        
        public bool useGlow = false;
        public Color glowColor = new Color(0.2f, 0.9f, 1f, 1f);
        public float glowIntensity = 0.5f;
    }
    
    public enum FontPresetType
    {
        Title,      // 标题
        Header,     // 副标题
        Body,       // 正文
        Caption,    // 说明文字
        Code        // 代码
    }
    
    #endregion
    
    #region 扩展
    
    public static class FontColorExtensions
    {
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
        
        public static Color Brighten(this Color color, float amount)
        {
            return new Color(
                Mathf.Min(1f, color.r + amount),
                Mathf.Min(1f, color.g + amount),
                Mathf.Min(1f, color.b + amount),
                color.a
            );
        }
    }
    
    #endregion
}
