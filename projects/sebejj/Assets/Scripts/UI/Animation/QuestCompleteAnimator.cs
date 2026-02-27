using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

namespace SebeJJ.UI.Animation
{
    /// <summary>
    /// 委托完成庆祝动画
    /// </summary>
    public class QuestCompleteAnimator : MonoBehaviour
    {
        [Header("基础配置")]
        [SerializeField] private RectTransform celebratePanel;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image backgroundOverlay;
        
        [Header("标题动画")]
        [SerializeField] private Text completeTitle;
        [SerializeField] private float titleScaleInDuration = 0.5f;
        [SerializeField] private float titlePulseDuration = 0.8f;
        [SerializeField] private float titleShakeAmount = 10f;
        
        [Header("奖励展示")]
        [SerializeField] private RectTransform rewardsContainer;
        [SerializeField] private float rewardStaggerDelay = 0.15f;
        [SerializeField] private float rewardPopDuration = 0.3f;
        [SerializeField] private float rewardFloatHeight = 20f;
        
        [Header("特效")]
        [SerializeField] private ParticleSystem confettiParticles;
        [SerializeField] private ParticleSystem sparkleParticles;
        [SerializeField] private Image glowOverlay;
        [SerializeField] private float glowPulseSpeed = 0.5f;
        
        [Header("星星评价")]
        [SerializeField] private List<Image> starImages = new List<Image>();
        [SerializeField] private float starDelay = 0.3f;
        [SerializeField] private float starPopDuration = 0.4f;
        [SerializeField] private Color starEmptyColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        [SerializeField] private Color starFilledColor = new Color(1f, 0.9f, 0.2f, 1f);
        
        [Header("按钮动画")]
        [SerializeField] private Button continueButton;
        [SerializeField] private float buttonDelay = 1f;
        [SerializeField] private float buttonSlideDuration = 0.3f;
        
        [Header("持续时间")]
        [SerializeField] private float autoCloseDelay = 5f;
        
        private Sequence celebrateSequence;
        
        private void Awake()
        {
            if (celebratePanel == null)
                celebratePanel = GetComponent<RectTransform>();
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
                
            // 初始隐藏
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }
        
        /// <summary>
        /// 播放完成庆祝动画
        /// </summary>
        public void PlayCompleteAnimation(int starRating = 3, List<RewardData> rewards = null)
        {
            gameObject.SetActive(true);
            
            celebrateSequence?.Kill();
            celebrateSequence = DOTween.Sequence();
            
            // 阶段1：背景淡入
            if (backgroundOverlay != null)
            {
                backgroundOverlay.color = new Color(0f, 0f, 0f, 0f);
                celebrateSequence.Append(
                    backgroundOverlay.DOFade(0.7f, 0.3f)
                );
            }
            
            if (canvasGroup != null)
            {
                celebrateSequence.Join(
                    canvasGroup.DOFade(1f, 0.3f)
                );
            }
            
            // 阶段2：标题弹出
            if (completeTitle != null)
            {
                completeTitle.transform.localScale = Vector3.zero;
                celebrateSequence.Append(
                    completeTitle.transform.DOScale(Vector3.one, titleScaleInDuration)
                        .SetEase(Ease.OutBack)
                );
                
                // 标题震动
                celebrateSequence.Join(
                    completeTitle.transform.DOShakeAnchorPos(
                        titleScaleInDuration,
                        new Vector2(titleShakeAmount, titleShakeAmount),
                        20,
                        90f
                    )
                );
                
                // 持续脉冲
                celebrateSequence.Append(
                    completeTitle.transform.DOScale(1.05f, titlePulseDuration * 0.5f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo)
                );
            }
            
            // 阶段3：星星评价
            celebrateSequence.AppendInterval(starDelay);
            AnimateStars(starRating);
            
            // 阶段4：奖励展示
            if (rewards != null && rewards.Count > 0)
            {
                celebrateSequence.AppendInterval(rewardStaggerDelay);
                AnimateRewards(rewards);
            }
            
            // 阶段5：特效
            celebrateSequence.AppendCallback(() => PlayEffects());
            
            // 阶段6：显示按钮
            if (continueButton != null)
            {
                celebrateSequence.AppendInterval(buttonDelay);
                
                RectTransform buttonRect = continueButton.GetComponent<RectTransform>();
                Vector2 originalPos = buttonRect.anchoredPosition;
                buttonRect.anchoredPosition = new Vector2(originalPos.x, originalPos.y - 100f);
                
                celebrateSequence.Append(
                    buttonRect.DOAnchorPos(originalPos, buttonSlideDuration)
                        .SetEase(Ease.OutBack)
                );
                
                CanvasGroup buttonGroup = continueButton.GetComponent<CanvasGroup>();
                if (buttonGroup != null)
                {
                    buttonGroup.alpha = 0f;
                    celebrateSequence.Join(
                        buttonGroup.DOFade(1f, buttonSlideDuration)
                    );
                }
            }
            
            // 自动关闭
            celebrateSequence.AppendInterval(autoCloseDelay);
            celebrateSequence.OnComplete(() => Close());
        }
        
        /// <summary>
        /// 星星动画
        /// </summary>
        private void AnimateStars(int rating)
        {
            for (int i = 0; i < starImages.Count; i++)
            {
                var star = starImages[i];
                if (star == null) continue;
                
                // 初始状态
                star.color = starEmptyColor;
                star.transform.localScale = Vector3.zero;
                
                float delay = i * starDelay;
                
                if (i < rating)
                {
                    // 点亮星星
                    DOVirtual.DelayedCall(delay, () =
                    {
                        star.transform.DOScale(1.3f, starPopDuration * 0.6f)
                            .SetEase(Ease.OutBack);
                        
                        star.transform.DOScale(1f, starPopDuration * 0.4f)
                            .SetEase(Ease.OutQuad)
                            .SetDelay(starPopDuration * 0.6f);
                        
                        star.DOColor(starFilledColor, starPopDuration);
                        
                        // 旋转
                        star.transform.DORotate(new Vector3(0f, 0f, 360f), starPopDuration, RotateMode.FastBeyond360)
                            .SetEase(Ease.OutBack);
                    });
                }
                else
                {
                    // 空星星
                    DOVirtual.DelayedCall(delay, () =
                    {
                        star.transform.DOScale(0.8f, starPopDuration)
                            .SetEase(Ease.OutBack);
                    });
                }
            }
        }
        
        /// <summary>
        /// 奖励动画
        /// </summary>
        private void AnimateRewards(List<RewardData> rewards)
        {
            for (int i = 0; i < rewards.Count; i++)
            {
                var reward = rewards[i];
                float delay = i * rewardStaggerDelay;
                
                // 创建奖励图标
                GameObject rewardObj = CreateRewardIcon(reward);
                rewardObj.transform.SetParent(rewardsContainer);
                
                RectTransform rewardRect = rewardObj.GetComponent<RectTransform>();
                rewardRect.localScale = Vector3.zero;
                
                DOVirtual.DelayedCall(delay, () =
                {
                    // 弹出
                    rewardRect.DOScale(Vector3.one, rewardPopDuration)
                        .SetEase(Ease.OutBack);
                    
                    // 上浮
                    rewardRect.DOAnchorPosY(rewardFloatHeight, rewardPopDuration * 2f)
                        .SetEase(Ease.OutQuad)
                        .SetLoops(2, LoopType.Yoyo);
                    
                    // 发光
                    Image rewardImage = rewardObj.GetComponent<Image>();
                    if (rewardImage != null)
                    {
                        rewardImage.DOFade(1.5f, rewardPopDuration * 0.5f)
                            .SetLoops(2, LoopType.Yoyo);
                    }
                });
            }
        }
        
        /// <summary>
        /// 创建奖励图标
        /// </summary>
        private GameObject CreateRewardIcon(RewardData reward)
        {
            GameObject obj = new GameObject($"Reward_{reward.type}");
            
            Image image = obj.AddComponent<Image>();
            image.sprite = reward.icon;
            
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(80f, 80f);
            
            // 添加数量文本
            if (reward.amount > 0)
            {
                GameObject textObj = new GameObject("Amount");
                textObj.transform.SetParent(obj.transform);
                
                Text text = textObj.AddComponent<Text>();
                text.text = "x" + reward.amount;
                text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                text.fontSize = 24;
                text.color = Color.white;
                text.alignment = TextAnchor.LowerRight;
                
                RectTransform textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.anchoredPosition = Vector2.zero;
                textRect.sizeDelta = Vector2.zero;
            }
            
            return obj;
        }
        
        /// <summary>
        /// 播放特效
        /// </summary>
        private void PlayEffects()
        {
            // 彩纸
            if (confettiParticles != null)
            {
                confettiParticles.Play();
            }
            
            // 闪光
            if (sparkleParticles != null)
            {
                sparkleParticles.Play();
            }
            
            // 发光脉冲
            if (glowOverlay != null)
            {
                glowOverlay.DOFade(0.5f, glowPulseSpeed)
                    .SetLoops(6, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
        }
        
        /// <summary>
        /// 关闭庆祝面板
        /// </summary>
        public void Close()
        {
            celebrateSequence?.Kill();
            
            Sequence closeSeq = DOTween.Sequence();
            
            // 淡出
            if (canvasGroup != null)
            {
                closeSeq.Append(
                    canvasGroup.DOFade(0f, 0.3f)
                );
            }
            
            if (backgroundOverlay != null)
            {
                closeSeq.Join(
                    backgroundOverlay.DOFade(0f, 0.3f)
                );
            }
            
            // 缩放退出
            closeSeq.Join(
                celebratePanel.DOScale(Vector3.zero, 0.3f)
                    .SetEase(Ease.InBack)
            );
            
            closeSeq.OnComplete(() =>
            {
                gameObject.SetActive(false);
                celebratePanel.localScale = Vector3.one;
            });
        }
        
        private void OnDestroy()
        {
            celebrateSequence?.Kill();
        }
    }
    
    /// <summary>
    /// 奖励数据
    /// </summary>
    [System.Serializable]
    public class RewardData
    {
        public string type;
        public int amount;
        public Sprite icon;
    }
}
