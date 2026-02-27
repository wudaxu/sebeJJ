using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace SebeJJ.Upgrade.UI
{
    /// <summary>
    /// 升级预览UI
    /// </summary>
    public class UpgradePreviewUI : MonoBehaviour
    {
        [Header("UI组件")]
        public Image iconImage;
        public Text nameText;
        public Text levelText;
        public Text valueChangeText;
        public Text descriptionText;
        public GameObject effectContainer;
        public ParticleSystem upgradeEffect;
        
        [Header("动画设置")]
        public float displayDuration = 2.5f;
        public float fadeInDuration = 0.3f;
        public float fadeOutDuration = 0.3f;
        
        private CanvasGroup canvasGroup;
        private Coroutine hideCoroutine;
        
        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            canvasGroup.alpha = 0;
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 显示升级预览
        /// </summary>
        public void Show(UpgradePreview preview)
        {
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
            }
            
            gameObject.SetActive(true);
            
            // 更新UI
            if (iconImage != null) iconImage.sprite = preview.icon;
            if (nameText != null) nameText.text = preview.upgradeName;
            if (levelText != null) levelText.text = $"等级 {preview.currentLevel} → {preview.nextLevel}";
            if (valueChangeText != null) 
                valueChangeText.text = $"{preview.currentValue:F1} → {preview.nextValue:F1} (+{preview.ValueIncreasePercent:F0}%)";
            if (descriptionText != null) descriptionText.text = preview.description;
            
            // 播放特效
            if (upgradeEffect != null)
            {
                upgradeEffect.Play();
            }
            
            // 淡入
            LeanTween.alphaCanvas(canvasGroup, 1f, fadeInDuration)
                .setEaseOutQuad();
            
            // 延迟隐藏
            hideCoroutine = StartCoroutine(HideAfterDelay());
        }
        
        /// <summary>
        /// 延迟隐藏
        /// </summary>
        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(displayDuration);
            
            // 淡出
            LeanTween.alphaCanvas(canvasGroup, 0f, fadeOutDuration)
                .setEaseInQuad()
                .setOnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
        }
        
        /// <summary>
        /// 立即隐藏
        /// </summary>
        public void Hide()
        {
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
            }
            
            LeanTween.alphaCanvas(canvasGroup, 0f, fadeOutDuration)
                .setEaseInQuad()
                .setOnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
        }
    }
}