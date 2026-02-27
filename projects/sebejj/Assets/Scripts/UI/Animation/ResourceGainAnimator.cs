using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace SebeJJ.UI.Animation
{
    /// <summary>
    /// 资源获得动画 - 飞入背包效果
    /// </summary>
    public class ResourceGainAnimator : MonoBehaviour
    {
        [Header("基础配置")]
        [SerializeField] private RectTransform resourceIcon;
        [SerializeField] private Image iconImage;
        [SerializeField] private Text amountText;
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("飞行配置")]
        [SerializeField] private float flyDuration = 0.8f;
        [SerializeField] private float flyHeight = 100f;
        [SerializeField] private Ease flyEase = Ease.InOutQuad;
        [SerializeField] private AnimationCurve flyCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        
        [Header("起始效果")]
        [SerializeField] private float startPopScale = 1.5f;
        [SerializeField] private float startPopDuration = 0.2f;
        [SerializeField] private float startFloatHeight = 30f;
        
        [Header("目标效果")]
        [SerializeField] private float targetShrinkScale = 0.3f;
        [SerializeField] private float targetPulseAmount = 1.2f;
        [SerializeField] private float targetPulseDuration = 0.15f;
        
        [Header("拖尾效果")]
        [SerializeField] private bool useTrail = true;
        [SerializeField] private int trailCount = 5;
        [SerializeField] private float trailDelay = 0.05f;
        [SerializeField] private float trailFadeSpeed = 2f;
        [SerializeField] private float trailScaleMultiplier = 0.8f;
        
        [Header("获得提示")]
        [SerializeField] private Text gainTextPrefab;
        [SerializeField] private float gainTextOffset = 50f;
        [SerializeField] private float gainTextDuration = 1f;
        
        private Sequence flySequence;
        private Vector3 startPosition;
        private Vector3 targetPosition;
        private int resourceAmount;
        
        /// <summary>
        /// 初始化资源飞行动画
        /// </summary>
        public void Initialize(Sprite icon, int amount, Vector3 worldStartPos, RectTransform targetUIElement)
        {
            if (iconImage != null)
                iconImage.sprite = icon;
            
            if (amountText != null)
            {
                amountText.text = "+" + amount;
                amountText.gameObject.SetActive(true);
            }
            
            resourceAmount = amount;
            
            // 转换坐标
            startPosition = WorldToUISpace(worldStartPos);
            targetPosition = targetUIElement.position;
            
            // 设置初始位置
            resourceIcon.position = startPosition;
            resourceIcon.localScale = Vector3.zero;
            
            if (canvasGroup != null)
                canvasGroup.alpha = 1f;
        }
        
        /// <summary>
        /// 播放飞行动画
        /// </summary>
        public void PlayFlyAnimation(System.Action onComplete = null)
        {
            flySequence?.Kill();
            flySequence = DOTween.Sequence();
            
            // 阶段1：起始弹出
            flySequence.Append(
                resourceIcon.DOScale(Vector3.one * startPopScale, startPopDuration)
                    .SetEase(Ease.OutBack)
            );
            
            // 起始上浮
            flySequence.Join(
                resourceIcon.DOMoveY(startPosition.y + startFloatHeight, startPopDuration)
                    .SetEase(Ease.OutQuad)
            );
            
            // 显示数量文本
            if (amountText != null)
            {
                amountText.transform.localScale = Vector3.zero;
                flySequence.Join(
                    amountText.transform.DOScale(Vector3.one, startPopDuration)
                        .SetEase(Ease.OutBack)
                );
            }
            
            // 阶段2：飞行到目标
            flySequence.AppendInterval(0.1f);
            
            // 创建贝塞尔曲线路径
            Vector3 controlPoint = CalculateControlPoint(startPosition, targetPosition);
            Vector3[] path = new Vector3[] { startPosition, controlPoint, targetPosition };
            
            flySequence.Append(
                resourceIcon.DOPath(path, flyDuration, PathType.CatmullRom)
                    .SetEase(flyEase)
            );
            
            // 飞行中旋转
            flySequence.Join(
                resourceIcon.DORotate(new Vector3(0f, 0f, 360f), flyDuration, RotateMode.FastBeyond360)
                    .SetEase(Ease.Linear)
            );
            
            // 飞行中缩放变化
            flySequence.Join(
                resourceIcon.DOScale(Vector3.one, flyDuration * 0.3f)
                    .SetEase(Ease.OutQuad)
            );
            
            flySequence.Append(
                resourceIcon.DOScale(Vector3.one * targetShrinkScale, flyDuration * 0.7f)
                    .SetEase(Ease.InQuad)
            );
            
            // 数量文本淡出
            if (amountText != null)
            {
                flySequence.Join(
                    amountText.DOFade(0f, flyDuration * 0.5f)
                        .SetDelay(flyDuration * 0.3f)
                );
            }
            
            // 阶段3：目标脉冲
            flySequence.AppendCallback(() =>
            {
                // 目标UI脉冲
                PulseTarget();
                
                // 生成获得提示
                SpawnGainText();
            });
            
            // 消失
            flySequence.Append(
                resourceIcon.DOScale(Vector3.zero, 0.1f)
                    .SetEase(Ease.InBack)
            );
            
            if (canvasGroup != null)
            {
                flySequence.Join(
                    canvasGroup.DOFade(0f, 0.1f)
                );
            }
            
            flySequence.OnComplete(() =>
            {
                onComplete?.Invoke();
                Destroy(gameObject);
            });
            
            // 创建拖尾效果
            if (useTrail)
            {
                CreateTrailEffect();
            }
        }
        
        /// <summary>
        /// 计算贝塞尔曲线控制点
        /// </summary>
        private Vector3 CalculateControlPoint(Vector3 start, Vector3 end)
        {
            Vector3 midPoint = (start + end) / 2f;
            float heightOffset = Mathf.Abs(end.y - start.y) * 0.5f + flyHeight;
            
            // 根据方向决定控制点偏移
            return midPoint + Vector3.up * heightOffset;
        }
        
        /// <summary>
        /// 世界坐标转UI坐标
        /// </summary>
        private Vector3 WorldToUISpace(Vector3 worldPos)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            
            // 转换为UI本地坐标
            RectTransform canvasRect = GetComponentInParent<Canvas>()?.GetComponent<RectTransform>();
            if (canvasRect != null)
            {
                Vector2 localPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect, screenPos, null, out localPos);
                return canvasRect.TransformPoint(localPos);
            }
            
            return screenPos;
        }
        
        /// <summary>
        /// 创建拖尾效果
        /// </summary>
        private void CreateTrailEffect()
        {
            for (int i = 0; i < trailCount; i++)
            {
                float delay = i * trailDelay;
                
                DOVirtual.DelayedCall(delay, () =
                {
                    if (this == null) return;
                    
                    GameObject trail = new GameObject($"Trail_{i}");
                    trail.transform.SetParent(transform.parent);
                    
                    Image trailImage = trail.AddComponent<Image>();
                    trailImage.sprite = iconImage?.sprite;
                    trailImage.color = new Color(1f, 1f, 1f, 0.5f);
                    
                    RectTransform trailRect = trail.GetComponent<RectTransform>();
                    trailRect.sizeDelta = resourceIcon.sizeDelta * trailScaleMultiplier;
                    trailRect.position = resourceIcon.position;
                    trailRect.rotation = resourceIcon.rotation;
                    
                    // 拖尾淡出
                    trailImage.DOFade(0f, 1f / trailFadeSpeed)
                        .SetEase(Ease.OutQuad);
                    
                    trailRect.DOScale(Vector3.zero, 1f / trailFadeSpeed)
                        .SetEase(Ease.OutQuad)
                        .OnComplete(() => Destroy(trail));
                });
            }
        }
        
        /// <summary>
        /// 目标UI脉冲
        /// </summary>
        private void PulseTarget()
        {
            // 这里可以通过事件系统通知目标UI播放脉冲动画
            // 简化处理：创建一个临时的脉冲效果
        }
        
        /// <summary>
        /// 生成获得提示文本
        /// </summary>
        private void SpawnGainText()
        {
            if (gainTextPrefab == null) return;
            
            Text gainText = Instantiate(gainTextPrefab, transform.parent);
            gainText.text = "+" + resourceAmount;
            
            RectTransform textRect = gainText.GetComponent<RectTransform>();
            textRect.position = targetPosition + Vector3.up * gainTextOffset;
            
            // 上浮淡出
            Sequence textSeq = DOTween.Sequence();
            
            textSeq.Append(
                textRect.DOMoveY(textRect.position.y + 30f, gainTextDuration)
                    .SetEase(Ease.OutQuad)
            );
            
            textSeq.Join(
                gainText.DOFade(0f, gainTextDuration)
                    .SetEase(Ease.InQuad)
            );
            
            textSeq.OnComplete(() => Destroy(gainText.gameObject));
        }
        
        private void OnDestroy()
        {
            flySequence?.Kill();
        }
    }
}
