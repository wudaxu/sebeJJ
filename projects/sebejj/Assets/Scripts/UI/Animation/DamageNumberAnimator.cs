using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SebeJJ.UI.Animation
{
    /// <summary>
    /// 伤害数字动画 - 弹出、浮动、消失效果
    /// </summary>
    public class DamageNumberAnimator : MonoBehaviour
    {
        [Header("基础配置")]
        [SerializeField] private RectTransform numberTransform;
        [SerializeField] private Text damageText;
        [SerializeField] private Outline textOutline;
        [SerializeField] private Shadow textShadow;
        
        [Header("弹出动画")]
        [SerializeField] private float popScale = 1.5f;
        [SerializeField] private float popDuration = 0.15f;
        [SerializeField] private float popOvershoot = 1.2f;
        
        [Header("浮动动画")]
        [SerializeField] private float floatHeight = 80f;
        [SerializeField] private float floatDuration = 0.8f;
        [SerializeField] private float floatWobbleAmount = 15f;
        [SerializeField] private float floatWobbleFrequency = 3f;
        
        [Header("消失动画")]
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private float shrinkScale = 0.5f;
        
        [Header("颜色配置")]
        [SerializeField] private Color normalDamageColor = new Color(1f, 0.9f, 0.7f, 1f);
        [SerializeField] private Color criticalDamageColor = new Color(1f, 0.3f, 0.1f, 1f);
        [SerializeField] private Color healColor = new Color(0.2f, 1f, 0.4f, 1f);
        [SerializeField] private Color shieldDamageColor = new Color(0.3f, 0.7f, 1f, 1f);
        
        [Header("暴击效果")]
        [SerializeField] private float criticalScale = 2f;
        [SerializeField] private float criticalShakeAmount = 5f;
        [SerializeField] private float criticalOutlineWidth = 2f;
        
        [Header("连击效果")]
        [SerializeField] private float comboScaleIncrement = 0.2f;
        [SerializeField] private float comboMaxScale = 2.5f;
        
        private Sequence currentSequence;
        private Vector3 originalScale;
        private Vector2 originalPosition;
        private int currentCombo = 0;
        
        private void Awake()
        {
            if (numberTransform == null)
                numberTransform = GetComponent<RectTransform>();
            if (damageText == null)
                damageText = GetComponent<Text>();
                
            originalScale = numberTransform.localScale;
            originalPosition = numberTransform.anchoredPosition;
        }
        
        /// <summary>
        /// 显示普通伤害
        /// </summary>
        public void ShowDamage(int damage, bool isCritical = false)
        {
            SetupText(damage.ToString(), isCritical ? criticalDamageColor : normalDamageColor);
            PlayAnimation(isCritical);
        }
        
        /// <summary>
        /// 显示治疗
        /// </summary>
        public void ShowHeal(int amount)
        {
            SetupText("+" + amount, healColor);
            PlayAnimation(false, true);
        }
        
        /// <summary>
        /// 显示护盾伤害
        /// </summary>
        public void ShowShieldDamage(int damage)
        {
            SetupText(damage.ToString(), shieldDamageColor);
            PlayAnimation(false);
        }
        
        /// <summary>
        /// 显示连击伤害
        /// </summary>
        public void ShowComboDamage(int damage, int comboCount)
        {
            currentCombo = comboCount;
            string text = damage + " x" + comboCount;
            SetupText(text, criticalDamageColor);
            
            // 连击数越高，初始缩放越大
            float comboScale = Mathf.Min(1f + comboCount * comboScaleIncrement, comboMaxScale);
            
            PlayAnimation(true, false, comboScale);
        }
        
        /// <summary>
        /// 设置文本
        /// </summary>
        private void SetupText(string text, Color color)
        {
            if (damageText != null)
            {
                damageText.text = text;
                damageText.color = color;
            }
            
            // 重置状态
            numberTransform.localScale = Vector3.zero;
            numberTransform.anchoredPosition = originalPosition;
            
            CanvasGroup cg = GetComponent<CanvasGroup>();
            if (cg != null)
                cg.alpha = 1f;
        }
        
        /// </summary>
        /// 播放动画
        /// </summary>
        private void PlayAnimation(bool isCritical, bool isHeal = false, float customScale = 1f)
        {
            currentSequence?.Kill();
            currentSequence = DOTween.Sequence();
            
            float startScale = isCritical ? criticalScale : popScale;
            startScale *= customScale;
            
            // 弹出动画
            currentSequence.Append(
                numberTransform.DOScale(originalScale * startScale * popOvershoot, popDuration * 0.6f)
                    .SetEase(Ease.OutQuad)
            );
            
            currentSequence.Append(
                numberTransform.DOScale(originalScale * startScale, popDuration * 0.4f)
                    .SetEase(Ease.OutBack)
            );
            
            // 暴击震动效果
            if (isCritical)
            {
                currentSequence.Join(
                    numberTransform.DOShakeAnchorPos(
                        popDuration,
                        new Vector2(criticalShakeAmount, criticalShakeAmount),
                        10,
                        90f
                    )
                );
                
                // 描边效果
                if (textOutline != null)
                {
                    textOutline.effectDistance = new Vector2(criticalOutlineWidth, criticalOutlineWidth);
                }
            }
            
            // 浮动动画
            float floatTime = floatDuration;
            
            // 向上浮动
            currentSequence.Append(
                numberTransform.DOAnchorPosY(originalPosition.y + floatHeight, floatTime)
                    .SetEase(Ease.OutQuad)
            );
            
            // 左右摇摆
            currentSequence.Join(
                numberTransform.DOAnchorPosX(
                    originalPosition.x + floatWobbleAmount,
                    floatTime / floatWobbleFrequency
                ).SetEase(Ease.InOutSine)
                 .SetLoops(Mathf.RoundToInt(floatWobbleFrequency), LoopType.Yoyo)
            );
            
            // 旋转（轻微）
            float rotationAmount = isCritical ? 10f : 5f;
            currentSequence.Join(
                numberTransform.DORotate(
                    new Vector3(0f, 0f, rotationAmount),
                    floatTime / 2f
                ).SetEase(Ease.InOutSine)
                 .SetLoops(2, LoopType.Yoyo)
            );
            
            // 消失动画
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                currentSequence.Append(
                    canvasGroup.DOFade(0f, fadeDuration)
                        .SetEase(Ease.InQuad)
                );
            }
            
            currentSequence.Join(
                numberTransform.DOScale(originalScale * shrinkScale, fadeDuration)
                    .SetEase(Ease.InBack)
            );
            
            // 销毁
            currentSequence.OnComplete(() => Destroy(gameObject));
        }
        
        /// <summary>
        /// 从对象池获取时重置
        /// </summary>
        public void ResetFromPool()
        {
            currentSequence?.Kill();
            numberTransform.localScale = originalScale;
            numberTransform.anchoredPosition = originalPosition;
            numberTransform.rotation = Quaternion.identity;
            
            CanvasGroup cg = GetComponent<CanvasGroup>();
            if (cg != null)
                cg.alpha = 1f;
        }
        
        private void OnDestroy()
        {
            currentSequence?.Kill();
        }
    }
}
