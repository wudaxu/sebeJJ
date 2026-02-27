using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SebeJJ.UI.Animation
{
    /// <summary>
    /// 菜单按钮动画控制器 - 处理主菜单按钮的悬停、选中、过渡效果
    /// </summary>
    public class MenuButtonAnimator : MonoBehaviour
    {
        [Header("基础配置")]
        [SerializeField] private RectTransform buttonTransform;
        [SerializeField] private Image buttonImage;
        [SerializeField] private Text buttonText;
        
        [Header("悬停效果")]
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float hoverDuration = 0.2f;
        [SerializeField] private Color hoverColor = new Color(0.2f, 0.8f, 1f, 1f);
        [SerializeField] private Color hoverTextColor = new Color(1f, 1f, 1f, 1f);
        
        [Header("选中效果")]
        [SerializeField] private float selectedScale = 1.15f;
        [SerializeField] private float selectedDuration = 0.15f;
        [SerializeField] private Color selectedColor = new Color(0f, 1f, 0.8f, 1f);
        [SerializeField] private float glowIntensity = 1.5f;
        
        [Header("点击效果")]
        [SerializeField] private float clickScale = 0.95f;
        [SerializeField] private float clickDuration = 0.1f;
        [SerializeField] private AudioClip clickSound;
        
        [Header("过渡效果")]
        [SerializeField] private float transitionOutDuration = 0.3f;
        [SerializeField] private float transitionInDuration = 0.4f;
        
        private Vector3 originalScale;
        private Color originalColor;
        private Color originalTextColor;
        private Sequence currentSequence;
        private bool isSelected;
        
        // 事件
        public event Action OnHoverEnter;
        public event Action OnHoverExit;
        public event Action OnSelected;
        public event Action OnClick;
        
        private void Awake()
        {
            if (buttonTransform == null)
                buttonTransform = GetComponent<RectTransform>();
            if (buttonImage == null)
                buttonImage = GetComponent<Image>();
                
            originalScale = buttonTransform.localScale;
            if (buttonImage != null)
                originalColor = buttonImage.color;
            if (buttonText != null)
                originalTextColor = buttonText.color;
        }
        
        /// <summary>
        /// 鼠标进入悬停
        /// </summary>
        public void OnPointerEnter()
        {
            if (isSelected) return;
            
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();
            
            // 缩放动画
            currentSequence.Append(
                buttonTransform.DOScale(originalScale * hoverScale, hoverDuration)
                    .SetEase(Ease.OutBack)
            );
            
            // 颜色变化
            if (buttonImage != null)
            {
                currentSequence.Join(
                    buttonImage.DOColor(hoverColor, hoverDuration)
                        .SetEase(Ease.OutQuad)
                );
            }
            
            if (buttonText != null)
            {
                currentSequence.Join(
                    buttonText.DOColor(hoverTextColor, hoverDuration)
                        .SetEase(Ease.OutQuad)
                );
            }
            
            // 发光效果
            currentSequence.Join(
                DOTween.To(() => glowIntensity, x => SetGlow(x), glowIntensity, hoverDuration)
            );
            
            OnHoverEnter?.Invoke();
        }
        
        /// <summary>
        /// 鼠标退出悬停
        /// </summary>
        public void OnPointerExit()
        {
            if (isSelected) return;
            
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();
            
            currentSequence.Append(
                buttonTransform.DOScale(originalScale, hoverDuration)
                    .SetEase(Ease.OutQuad)
            );
            
            if (buttonImage != null)
            {
                currentSequence.Join(
                    buttonImage.DOColor(originalColor, hoverDuration)
                        .SetEase(Ease.OutQuad)
                );
            }
            
            if (buttonText != null)
            {
                currentSequence.Join(
                    buttonText.DOColor(originalTextColor, hoverDuration)
                        .SetEase(Ease.OutQuad)
                );
            }
            
            OnHoverExit?.Invoke();
        }
        
        /// <summary>
        /// 选中状态
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();
            
            if (selected)
            {
                // 选中动画
                currentSequence.Append(
                    buttonTransform.DOScale(originalScale * selectedScale, selectedDuration)
                        .SetEase(Ease.OutBack)
                );
                
                if (buttonImage != null)
                {
                    currentSequence.Join(
                        buttonImage.DOColor(selectedColor, selectedDuration)
                            .SetEase(Ease.OutQuad)
                    );
                }
                
                // 脉冲效果
                currentSequence.Append(
                    buttonTransform.DOScale(originalScale * selectedScale * 1.05f, 0.5f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo)
                );
                
                OnSelected?.Invoke();
            }
            else
            {
                // 取消选中
                currentSequence.Append(
                    buttonTransform.DOScale(originalScale, selectedDuration)
                        .SetEase(Ease.OutQuad)
                );
                
                if (buttonImage != null)
                {
                    currentSequence.Join(
                        buttonImage.DOColor(originalColor, selectedDuration)
                            .SetEase(Ease.OutQuad)
                    );
                }
            }
        }
        
        /// <summary>
        /// 点击效果
        /// </summary>
        public void OnPointerClick()
        {
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();
            
            // 按下缩小
            currentSequence.Append(
                buttonTransform.DOScale(originalScale * clickScale, clickDuration * 0.5f)
                    .SetEase(Ease.OutQuad)
            );
            
            // 弹回
            currentSequence.Append(
                buttonTransform.DOScale(
                    isSelected ? originalScale * selectedScale : originalScale * hoverScale, 
                    clickDuration * 0.5f
                ).SetEase(Ease.OutBack)
            );
            
            // 播放音效
            if (clickSound != null)
                AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position);
            
            OnClick?.Invoke();
        }
        
        /// <summary>
        /// 过渡出动画
        /// </summary>
        public void PlayTransitionOut(Action onComplete = null)
        {
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();
            
            currentSequence.Append(
                buttonTransform.DOScale(Vector3.zero, transitionOutDuration)
                    .SetEase(Ease.InBack)
            );
            
            if (buttonImage != null)
            {
                currentSequence.Join(
                    buttonImage.DOFade(0f, transitionOutDuration)
                        .SetEase(Ease.InQuad)
                );
            }
            
            currentSequence.OnComplete(() => onComplete?.Invoke());
        }
        
        /// <summary>
        /// 过渡入动画
        /// </summary>
        public void PlayTransitionIn(Action onComplete = null)
        {
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();
            
            buttonTransform.localScale = Vector3.zero;
            
            currentSequence.Append(
                buttonTransform.DOScale(originalScale, transitionInDuration)
                    .SetEase(Ease.OutBack)
            );
            
            if (buttonImage != null)
            {
                Color c = buttonImage.color;
                c.a = 0f;
                buttonImage.color = c;
                
                currentSequence.Join(
                    buttonImage.DOFade(1f, transitionInDuration)
                        .SetEase(Ease.OutQuad)
                );
            }
            
            currentSequence.OnComplete(() => onComplete?.Invoke());
        }
        
        private void SetGlow(float intensity)
        {
            // 这里可以通过Shader或Material设置发光强度
            // 具体实现取决于使用的渲染管线
        }
        
        private void OnDestroy()
        {
            currentSequence?.Kill();
        }
    }
}
