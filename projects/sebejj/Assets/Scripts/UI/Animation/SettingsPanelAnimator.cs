using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SebeJJ.UI.Animation
{
    /// <summary>
    /// 设置面板动画控制器 - 滑块、开关动效
    /// </summary>
    public class SettingsPanelAnimator : MonoBehaviour
    {
        [Header("面板配置")]
        [SerializeField] private RectTransform settingsPanel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform contentPanel;
        
        [Header("打开/关闭动画")]
        [SerializeField] private float openDuration = 0.4f;
        [SerializeField] private float closeDuration = 0.3f;
        [SerializeField] private Ease openEase = Ease.OutBack;
        [SerializeField] private Ease closeEase = Ease.InBack;
        [SerializeField] private float slideInDistance = 200f;
        
        [Header("滑块动画")]
        [SerializeField] private float sliderHandleScale = 1.3f;
        [SerializeField] private float sliderFillDuration = 0.2f;
        [SerializeField] private Color sliderActiveColor = new Color(0.2f, 0.9f, 1f, 1f);
        [SerializeField] private Color sliderInactiveColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        
        [Header("开关动画")]
        [SerializeField] private float toggleSwitchDuration = 0.25f;
        [SerializeField] private float toggleBounceAmount = 1.2f;
        [SerializeField] private Color toggleOnColor = new Color(0.2f, 0.9f, 0.4f, 1f);
        [SerializeField] private Color toggleOffColor = new Color(0.6f, 0.6f, 0.6f, 1f);
        [SerializeField] private float toggleGlowIntensity = 1.5f;
        
        [Header("下拉菜单动画")]
        [SerializeField] private float dropdownExpandDuration = 0.3f;
        [SerializeField] private float dropdownItemStagger = 0.03f;
        [SerializeField] private float dropdownSlideDistance = 30f;
        
        [Header("分类切换")]
        [SerializeField] private float categoryFadeDuration = 0.2f;
        [SerializeField] private float categorySlideDuration = 0.25f;
        
        [Header("保存提示")]
        [SerializeField] private float saveIconScale = 1.5f;
        [SerializeField] private float saveCheckmarkDuration = 0.5f;
        
        private Sequence currentSequence;
        private Vector3 originalScale;
        private Vector2 originalPosition;
        private bool isOpen;
        
        public bool IsOpen => isOpen;
        
        private void Awake()
        {
            if (settingsPanel == null)
                settingsPanel = GetComponent<RectTransform>();
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
                
            originalScale = settingsPanel.localScale;
            originalPosition = settingsPanel.anchoredPosition;
        }
        
        /// <summary>
        /// 打开设置面板
        /// </summary>
        public void Open()
        {
            if (isOpen) return;
            isOpen = true;
            
            gameObject.SetActive(true);
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();
            
            // 初始状态
            settingsPanel.anchoredPosition = new Vector2(originalPosition.x + slideInDistance, originalPosition.y);
            settingsPanel.localScale = Vector3.one * 0.9f;
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
            
            // 滑入
            currentSequence.Append(
                settingsPanel.DOAnchorPos(originalPosition, openDuration)
                    .SetEase(openEase)
            );
            
            // 缩放
            currentSequence.Join(
                settingsPanel.DOScale(originalScale, openDuration)
                    .SetEase(openEase)
            );
            
            // 淡入
            if (canvasGroup != null)
            {
                currentSequence.Join(
                    canvasGroup.DOFade(1f, openDuration * 0.7f)
                        .SetDelay(openDuration * 0.3f)
                );
            }
            
            // 内容项依次显示
            AnimateContentIn();
        }
        
        /// <summary>
        /// 关闭设置面板
        /// </summary>
        public void Close()
        {
            if (!isOpen) return;
            
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();
            
            // 内容项依次消失
            AnimateContentOut();
            
            // 滑出
            currentSequence.Append(
                settingsPanel.DOAnchorPosX(originalPosition.x + slideInDistance, closeDuration)
                    .SetEase(closeEase)
            );
            
            currentSequence.Join(
                settingsPanel.DOScale(Vector3.one * 0.9f, closeDuration)
            );
            
            if (canvasGroup != null)
            {
                currentSequence.Join(
                    canvasGroup.DOFade(0f, closeDuration * 0.5f)
                );
            }
            
            currentSequence.OnComplete(() =>
            {
                isOpen = false;
                gameObject.SetActive(false);
            });
        }
        
        /// <summary>
        /// 切换开关状态
        /// </summary>
        public void AnimateToggle(Toggle toggle, bool isOn)
        {
            RectTransform handle = toggle.transform.Find("Handle")?.GetComponent<RectTransform>();
            Image background = toggle.GetComponent<Image>();
            Image handleImage = handle?.GetComponent<Image>();
            
            if (handle == null) return;
            
            // 计算目标位置
            float handleWidth = handle.rect.width;
            Vector2 targetPos = isOn 
                ? new Vector2(handleWidth * 0.5f, 0f) 
                : new Vector2(-handleWidth * 0.5f, 0f);
            
            Sequence toggleSeq = DOTween.Sequence();
            
            // 滑块移动（带过冲）
            toggleSeq.Append(
                handle.DOAnchorPos(targetPos, toggleSwitchDuration)
                    .SetEase(Ease.OutBack, toggleBounceAmount)
            );
            
            // 背景颜色变化
            if (background != null)
            {
                Color targetColor = isOn ? toggleOnColor : toggleOffColor;
                toggleSeq.Join(
                    background.DOColor(targetColor, toggleSwitchDuration)
                );
            }
            
            // 滑块缩放效果
            toggleSeq.Join(
                handle.DOScale(Vector3.one * sliderHandleScale, toggleSwitchDuration * 0.3f)
                    .SetEase(Ease.OutBack)
                    .SetLoops(2, LoopType.Yoyo)
            );
            
            // 发光效果
            if (isOn && handleImage != null)
            {
                toggleSeq.Join(
                    DOTween.To(() => 1f, x => 
                    {
                        // 这里可以通过Material设置发光
                    }, toggleGlowIntensity, toggleSwitchDuration)
                );
            }
        }
        
        /// <summary>
        /// 滑块值变化动画
        /// </summary>
        public void AnimateSlider(Slider slider, float targetValue)
        {
            RectTransform fillRect = slider.fillRect;
            RectTransform handleRect = slider.handleRect;
            Image fillImage = fillRect?.GetComponent<Image>();
            
            // 数值变化
            DOTween.To(() => slider.value, x => slider.value = x, targetValue, sliderFillDuration)
                .SetEase(Ease.OutQuad);
            
            // 滑块缩放效果
            if (handleRect != null)
            {
                handleRect.DOScale(Vector3.one * sliderHandleScale, sliderFillDuration * 0.5f)
                    .SetEase(Ease.OutBack)
                    .SetLoops(2, LoopType.Yoyo);
            }
            
            // 填充颜色变化
            if (fillImage != null)
            {
                Color targetColor = targetValue > 0 ? sliderActiveColor : sliderInactiveColor;
                fillImage.DOColor(targetColor, sliderFillDuration);
            }
        }
        
        /// <summary>
        /// 滑块拖动中效果
        /// </summary>
        public void OnSliderDrag(Slider slider)
        {
            RectTransform handleRect = slider.handleRect;
            if (handleRect != null)
            {
                // 拖动时放大
                handleRect.DOScale(Vector3.one * sliderHandleScale, 0.1f)
                    .SetEase(Ease.OutBack);
            }
        }
        
        /// <summary>
        /// 滑块拖动结束
        /// </summary>
        public void OnSliderEndDrag(Slider slider)
        {
            RectTransform handleRect = slider.handleRect;
            if (handleRect != null)
            {
                // 恢复大小
                handleRect.DOScale(Vector3.one, 0.15f)
                    .SetEase(Ease.OutQuad);
            }
        }
        
        /// <summary>
        /// 下拉菜单展开动画
        /// </summary>
        public void AnimateDropdownExpand(RectTransform dropdownContent)
        {
            dropdownContent.gameObject.SetActive(true);
            
            // 内容缩放展开
            dropdownContent.localScale = new Vector3(1f, 0f, 1f);
            dropdownContent.DOScaleY(1f, dropdownExpandDuration)
                .SetEase(Ease.OutBack);
            
            // 选项依次滑入
            var items = dropdownContent.GetComponentsInChildren<RectTransform>();
            for (int i = 1; i < items.Length; i++) // 跳过父对象
            {
                var item = items[i];
                Vector2 originalPos = item.anchoredPosition;
                item.anchoredPosition = new Vector2(originalPos.x, originalPos.y - dropdownSlideDistance);
                
                item.DOAnchorPos(originalPos, dropdownExpandDuration * 0.5f)
                    .SetDelay(i * dropdownItemStagger)
                    .SetEase(Ease.OutBack);
            }
        }
        
        /// <summary>
        /// 下拉菜单收起动画
        /// </summary>
        public void AnimateDropdownCollapse(RectTransform dropdownContent)
        {
            dropdownContent.DOScaleY(0f, dropdownExpandDuration * 0.5f)
                .SetEase(Ease.InBack)
                .OnComplete(() => dropdownContent.gameObject.SetActive(false));
        }
        
        /// <summary>
        /// 分类切换动画
        /// </summary>
        public void AnimateCategorySwitch(RectTransform fromContent, RectTransform toContent)
        {
            Sequence switchSeq = DOTween.Sequence();
            
            // 旧内容淡出并左移
            if (fromContent != null)
            {
                CanvasGroup fromGroup = fromContent.GetComponent<CanvasGroup>();
                if (fromGroup != null)
                {
                    switchSeq.Append(
                        fromGroup.DOFade(0f, categoryFadeDuration)
                    );
                }
                
                switchSeq.Join(
                    fromContent.DOAnchorPosX(-50f, categorySlideDuration)
                        .SetEase(Ease.InQuad)
                );
                
                switchSeq.AppendCallback(() => fromContent.gameObject.SetActive(false));
            }
            
            // 新内容激活
            switchSeq.AppendCallback(() =>
            {
                toContent.gameObject.SetActive(true);
                toContent.anchoredPosition = new Vector2(50f, 0f);
                
                CanvasGroup toGroup = toContent.GetComponent<CanvasGroup>();
                if (toGroup != null)
                    toGroup.alpha = 0f;
            });
            
            // 新内容滑入并淡入
            switchSeq.Append(
                toContent.DOAnchorPosX(0f, categorySlideDuration)
                    .SetEase(Ease.OutQuad)
            );
            
            CanvasGroup newGroup = toContent.GetComponent<CanvasGroup>();
            if (newGroup != null)
            {
                switchSeq.Join(
                    newGroup.DOFade(1f, categoryFadeDuration)
                );
            }
        }
        
        /// <summary>
        /// 保存成功动画
        /// </summary>
        public void PlaySaveSuccessAnimation(RectTransform saveIcon)
        {
            Sequence saveSeq = DOTween.Sequence();
            
            // 图标放大
            saveSeq.Append(
                saveIcon.DOScale(Vector3.one * saveIconScale, saveCheckmarkDuration * 0.3f)
                    .SetEase(Ease.OutBack)
            );
            
            // 旋转
            saveSeq.Join(
                saveIcon.DORotate(new Vector3(0f, 0f, 360f), saveCheckmarkDuration, RotateMode.FastBeyond360)
                    .SetEase(Ease.OutBack)
            );
            
            // 恢复
            saveSeq.Append(
                saveIcon.DOScale(Vector3.one, saveCheckmarkDuration * 0.3f)
                    .SetEase(Ease.OutQuad)
            );
        }
        
        /// <summary>
        /// 内容项进入动画
        /// </summary>
        private void AnimateContentIn()
        {
            var rows = contentPanel.GetComponentsInChildren<RectTransform>();
            
            for (int i = 0; i < rows.Length; i++)
            {
                var row = rows[i];
                CanvasGroup rowGroup = row.GetComponent<CanvasGroup>();
                
                if (rowGroup != null)
                {
                    rowGroup.alpha = 0f;
                    row.anchoredPosition = new Vector2(30f, row.anchoredPosition.y);
                    
                    rowGroup.DOFade(1f, openDuration * 0.5f)
                        .SetDelay(openDuration * 0.3f + i * 0.03f);
                    
                    row.DOAnchorPosX(0f, openDuration * 0.5f)
                        .SetDelay(openDuration * 0.3f + i * 0.03f)
                        .SetEase(Ease.OutQuad);
                }
            }
        }
        
        /// <summary>
        /// 内容项退出动画
        /// </summary>
        private void AnimateContentOut()
        {
            var rows = contentPanel.GetComponentsInChildren<RectTransform>();
            
            for (int i = 0; i < rows.Length; i++)
            {
                var row = rows[i];
                CanvasGroup rowGroup = row.GetComponent<CanvasGroup>();
                
                if (rowGroup != null)
                {
                    rowGroup.DOFade(0f, closeDuration * 0.3f)
                        .SetDelay(i * 0.02f);
                }
            }
        }
        
        private void OnDestroy()
        {
            currentSequence?.Kill();
        }
    }
}
