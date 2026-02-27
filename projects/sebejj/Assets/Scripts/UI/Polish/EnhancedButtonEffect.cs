using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

namespace SebeJJ.UI.Polish
{
    /// <summary>
    /// 增强按钮效果 - 光晕、缩放、赛博朋克风格悬停效果
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class EnhancedButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("基础组件")]
        [SerializeField] private Button targetButton;
        [SerializeField] private RectTransform buttonRect;
        [SerializeField] private Image buttonImage;
        [SerializeField] private Text buttonText;
        
        [Header("光晕效果")]
        [SerializeField] private Image glowImage;
        [SerializeField] private float glowIntensity = 0.8f;
        [SerializeField] private float glowPulseSpeed = 1f;
        [SerializeField] private Color glowColor = new Color(0.2f, 0.9f, 1f, 1f);
        
        [Header("缩放效果")]
        [SerializeField] private float hoverScale = 1.08f;
        [SerializeField] private float clickScale = 0.92f;
        [SerializeField] private float normalScale = 1f;
        [SerializeField] private float scaleDuration = 0.15f;
        
        [Header("边框效果")]
        [SerializeField] private Image borderImage;
        [SerializeField] private float borderWidth = 3f;
        [SerializeField] private Color borderColor = new Color(0f, 1f, 0.8f, 1f);
        
        [Header("文字效果")]
        [SerializeField] private float textGlowIntensity = 0.5f;
        [SerializeField] private bool animateTextOnHover = true;
        
        [Header("粒子效果")]
        [SerializeField] private bool spawnParticlesOnClick = true;
        [SerializeField] private int particleCount = 8;
        [SerializeField] private float particleSpread = 50f;
        
        [Header("音效")]
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip clickSound;
        
        // 动画序列
        private Sequence glowSequence;
        private Sequence borderSequence;
        private Tweener scaleTweener;
        private Tweener textColorTweener;
        
        // 状态
        private bool isHovered = false;
        private bool isPressed = false;
        private bool isInteractable = true;
        
        private void Awake()
        {
            InitializeComponents();
            SetupInitialState();
        }
        
        private void OnEnable()
        {
            if (targetButton != null)
            {
                targetButton.onClick.AddListener(OnButtonClick);
            }
        }
        
        private void OnDisable()
        {
            if (targetButton != null)
            {
                targetButton.onClick.RemoveListener(OnButtonClick);
            }
            
            KillAllTweens();
        }
        
        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitializeComponents()
        {
            if (targetButton == null) targetButton = GetComponent<Button>();
            if (buttonRect == null) buttonRect = GetComponent<RectTransform>();
            if (buttonImage == null) buttonImage = GetComponent<Image>();
            
            // 创建光晕层
            if (glowImage == null)
            {
                CreateGlowLayer();
            }
            
            // 创建边框层
            if (borderImage == null)
            {
                CreateBorderLayer();
            }
            
            // 获取文字组件
            if (buttonText == null)
            {
                buttonText = GetComponentInChildren<Text>();
            }
        }
        
        /// <summary>
        /// 创建光晕层
        /// </summary>
        private void CreateGlowLayer()
        {
            GameObject glowObj = new GameObject("GlowLayer");
            glowObj.transform.SetParent(transform, false);
            
            glowImage = glowObj.AddComponent<Image>();
            glowImage.sprite = buttonImage.sprite;
            glowImage.type = buttonImage.type;
            glowImage.color = glowColor.WithAlpha(0f);
            
            var glowRect = glowObj.GetComponent<RectTransform>();
            glowRect.anchorMin = Vector2.zero;
            glowRect.anchorMax = Vector2.one;
            glowRect.offsetMin = new Vector2(-5f, -5f);
            glowRect.offsetMax = new Vector2(5f, 5f);
            
            glowObj.transform.SetAsFirstSibling();
        }
        
        /// <summary>
        /// 创建边框层
        /// </summary>
        private void CreateBorderLayer()
        {
            GameObject borderObj = new GameObject("BorderLayer");
            borderObj.transform.SetParent(transform, false);
            
            borderImage = borderObj.AddComponent<Image>();
            borderImage.sprite = CreateBorderSprite();
            borderImage.type = Image.Type.Sliced;
            borderImage.color = borderColor.WithAlpha(0f);
            
            var borderRect = borderObj.GetComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = Vector2.zero;
            borderRect.offsetMax = Vector2.zero;
            
            borderObj.transform.SetAsLastSibling();
        }
        
        /// <summary>
        /// 创建边框精灵
        /// </summary>
        private Sprite CreateBorderSprite()
        {
            // 使用默认边框或从Resources加载
            return Resources.Load<Sprite>("UI/CyberBorder") ?? buttonImage.sprite;
        }
        
        /// <summary>
        /// 设置初始状态
        /// </summary>
        private void SetupInitialState()
        {
            if (buttonRect != null)
            {
                buttonRect.localScale = Vector3.one * normalScale;
            }
            
            if (glowImage != null)
            {
                glowImage.color = glowColor.WithAlpha(0f);
            }
            
            if (borderImage != null)
            {
                borderImage.color = borderColor.WithAlpha(0f);
            }
        }
        
        /// <summary>
        /// 指针进入
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isInteractable || !targetButton.interactable) return;
            
            isHovered = true;
            
            // 缩放动画
            PlayHoverScaleAnimation();
            
            // 光晕动画
            PlayGlowAnimation();
            
            // 边框动画
            PlayBorderAnimation(true);
            
            // 文字效果
            if (animateTextOnHover && buttonText != null)
            {
                AnimateTextOnHover();
            }
            
            // 播放音效
            if (hoverSound != null)
            {
                AudioSource.PlayClipAtPoint(hoverSound, Camera.main.transform.position, 0.5f);
            }
        }
        
        /// <summary>
        /// 指针退出
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            
            // 恢复缩放
            PlayNormalScaleAnimation();
            
            // 停止光晕
            StopGlowAnimation();
            
            // 边框恢复
            PlayBorderAnimation(false);
            
            // 文字恢复
            if (buttonText != null)
            {
                ResetTextAnimation();
            }
        }
        
        /// <summary>
        /// 指针按下
        /// </summary>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isInteractable || !targetButton.interactable) return;
            
            isPressed = true;
            
            // 点击缩放
            PlayClickScaleAnimation();
            
            // 增强光晕
            if (glowImage != null)
            {
                glowImage.DOKill();
                glowImage.DOColor(glowColor.WithAlpha(glowIntensity * 1.5f), 0.1f);
            }
        }
        
        /// <summary>
        /// 指针释放
        /// </summary>
        public void OnPointerUp(PointerEventData eventData)
        {
            isPressed = false;
            
            if (isHovered)
            {
                PlayHoverScaleAnimation();
                PlayGlowAnimation();
            }
            else
            {
                PlayNormalScaleAnimation();
                StopGlowAnimation();
            }
        }
        
        /// <summary>
        /// 按钮点击
        /// </summary>
        private void OnButtonClick()
        {
            // 点击反馈
            PlayClickFeedback();
            
            // 粒子效果
            if (spawnParticlesOnClick)
            {
                SpawnClickParticles();
            }
            
            // 播放音效
            if (clickSound != null)
            {
                AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position, 0.7f);
            }
        }
        
        /// <summary>
        /// 播放悬停缩放动画
        /// </summary>
        private void PlayHoverScaleAnimation()
        {
            scaleTweener?.Kill();
            scaleTweener = buttonRect.DOScale(hoverScale, scaleDuration)
                .SetEase(Ease.OutBack, 0.3f);
        }
        
        /// <summary>
        /// 播放正常缩放动画
        /// </summary>
        private void PlayNormalScaleAnimation()
        {
            scaleTweener?.Kill();
            scaleTweener = buttonRect.DOScale(normalScale, scaleDuration)
                .SetEase(Ease.OutQuad);
        }
        
        /// <summary>
        /// 播放点击缩放动画
        /// </summary>
        private void PlayClickScaleAnimation()
        {
            scaleTweener?.Kill();
            scaleTweener = buttonRect.DOScale(clickScale, scaleDuration * 0.5f)
                .SetEase(Ease.OutQuad);
        }
        
        /// <summary>
        /// 播放光晕动画
        /// </summary>
        private void PlayGlowAnimation()
        {
            if (glowImage == null) return;
            
            glowSequence?.Kill();
            glowSequence = DOTween.Sequence();
            
            // 进入光晕
            glowSequence.Append(glowImage.DOColor(glowColor.WithAlpha(glowIntensity), 0.2f));
            
            // 脉冲效果
            glowSequence.Append(glowImage.DOColor(glowColor.WithAlpha(glowIntensity * 0.6f), 0.5f / glowPulseSpeed)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo));
        }
        
        /// <summary>
        /// 停止光晕动画
        /// </summary>
        private void StopGlowAnimation()
        {
            if (glowImage == null) return;
            
            glowSequence?.Kill();
            glowImage.DOColor(glowColor.WithAlpha(0f), 0.2f);
        }
        
        /// <summary>
        /// 播放边框动画
        /// </summary>
        private void PlayBorderAnimation(bool show)
        {
            if (borderImage == null) return;
            
            borderSequence?.Kill();
            
            if (show)
            {
                borderSequence = DOTween.Sequence();
                borderSequence.Append(borderImage.DOColor(borderColor, 0.15f));
                
                // 闪烁效果
                borderSequence.Append(borderImage.DOColor(borderColor.WithAlpha(0.5f), 0.3f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo));
            }
            else
            {
                borderImage.DOColor(borderColor.WithAlpha(0f), 0.2f);
            }
        }
        
        /// <summary>
        /// 悬停文字动画
        /// </summary>
        private void AnimateTextOnHover()
        {
            if (buttonText == null) return;
            
            textColorTweener?.Kill();
            
            // 颜色变亮
            Color targetColor = buttonText.color.Brighten(0.2f);
            textColorTweener = buttonText.DOColor(targetColor, 0.2f);
            
            // 轻微放大
            buttonText.rectTransform.DOScale(1.05f, 0.2f).SetEase(Ease.OutBack);
        }
        
        /// <summary>
        /// 重置文字动画
        /// </summary>
        private void ResetTextAnimation()
        {
            if (buttonText == null) return;
            
            textColorTweener?.Kill();
            
            // 恢复颜色
            textColorTweener = buttonText.DOColor(buttonText.color.Darken(0.2f), 0.2f);
            
            // 恢复大小
            buttonText.rectTransform.DOScale(1f, 0.2f).SetEase(Ease.OutQuad);
        }
        
        /// <summary>
        /// 播放点击反馈
        /// </summary>
        private void PlayClickFeedback()
        {
            // 冲击波效果
            if (glowImage != null)
            {
                var glowRect = glowImage.rectTransform;
                Vector3 originalScale = glowRect.localScale;
                
                glowRect.localScale = originalScale * 1.2f;
                glowRect.DOScale(originalScale, 0.3f).SetEase(Ease.OutBack);
                
                // 颜色闪烁
                Color originalColor = glowImage.color;
                glowImage.color = Color.white;
                glowImage.DOColor(originalColor, 0.2f);
            }
        }
        
        /// <summary>
        /// 生成点击粒子
        /// </summary>
        private void SpawnClickParticles()
        {
            Vector3 center = buttonRect.position;
            
            for (int i = 0; i < particleCount; i++)
            {
                float angle = (360f / particleCount) * i * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                
                // 使用MainMenuVisualManager创建粒子
                if (MainMenuVisualManager.Instance != null)
                {
                    Vector3 particlePos = center + direction * 20f;
                    MainMenuVisualManager.Instance.SpawnEffectParticle(
                        particlePos, 
                        glowColor.WithAlpha(0.8f),
                        0.05f
                    );
                }
            }
        }
        
        /// <summary>
        /// 停止所有动画
        /// </summary>
        private void KillAllTweens()
        {
            glowSequence?.Kill();
            borderSequence?.Kill();
            scaleTweener?.Kill();
            textColorTweener?.Kill();
            
            if (buttonRect != null)
            {
                buttonRect.DOKill();
            }
            
            if (glowImage != null)
            {
                glowImage.DOKill();
            }
            
            if (borderImage != null)
            {
                borderImage.DOKill();
            }
            
            if (buttonText != null)
            {
                buttonText.DOKill();
                buttonText.rectTransform.DOKill();
            }
        }
        
        /// <summary>
        /// 设置交互状态
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            isInteractable = interactable;
            targetButton.interactable = interactable;
            
            if (!interactable)
            {
                KillAllTweens();
                SetupInitialState();
                
                if (buttonImage != null)
                {
                    buttonImage.color = Color.gray.WithAlpha(0.5f);
                }
            }
        }
    }
    
    /// <summary>
    /// 颜色扩展
    /// </summary>
    public static class ButtonColorExtensions
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
        
        public static Color Darken(this Color color, float amount)
        {
            return new Color(
                Mathf.Max(0f, color.r - amount),
                Mathf.Max(0f, color.g - amount),
                Mathf.Max(0f, color.b - amount),
                color.a
            );
        }
    }
}
