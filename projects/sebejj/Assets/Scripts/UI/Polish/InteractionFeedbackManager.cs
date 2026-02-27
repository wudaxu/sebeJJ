using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

namespace SebeJJ.UI.Polish
{
    /// <summary>
    /// 交互反馈管理器 - 点击反馈、错误提示、成功庆祝、警告动画
    /// </summary>
    public class InteractionFeedbackManager : MonoBehaviour
    {
        public static InteractionFeedbackManager Instance { get; private set; }
        
        [Header("点击反馈")]
        [SerializeField] private AudioClip clickSound;
        [SerializeField] private ParticleSystem clickParticles;
        [SerializeField] private float clickShakeIntensity = 0.5f;
        
        [Header("错误提示")]
        [SerializeField] private GameObject errorPopupPrefab;
        [SerializeField] private AudioClip errorSound;
        [SerializeField] private float errorShakeDuration = 0.3f;
        [SerializeField] private float errorShakeIntensity = 10f;
        
        [Header("成功庆祝")]
        [SerializeField] private GameObject celebrationEffectPrefab;
        [SerializeField] private AudioClip successSound;
        [SerializeField] private ParticleSystem confettiParticles;
        
        [Header("警告动画")]
        [SerializeField] private GameObject warningPopupPrefab;
        [SerializeField] private AudioClip warningSound;
        [SerializeField] private float warningPulseSpeed = 2f;
        
        [Header("配色")]
        [SerializeField] private Color successColor = new Color(0.2f, 0.9f, 0.3f, 1f);
        [SerializeField] private Color errorColor = new Color(0.95f, 0.2f, 0.1f, 1f);
        [SerializeField] private Color warningColor = new Color(0.95f, 0.8f, 0.1f, 1f);
        
        // 对象池
        private Queue<GameObject> feedbackPool = new Queue<GameObject>();
        private const int MAX_POOL_SIZE = 10;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        #region 点击反馈
        
        /// <summary>
        /// 播放点击反馈
        /// </summary>
        public void PlayClickFeedback(Vector3 position, RectTransform target = null)
        {
            // 音效
            if (clickSound != null)
            {
                AudioSource.PlayClipAtPoint(clickSound, Camera.main.transform.position, 0.5f);
            }
            
            // 粒子效果
            if (clickParticles != null)
            {
                ParticleSystem particles = Instantiate(clickParticles, position, Quaternion.identity);
                particles.Play();
                Destroy(particles.gameObject, 1f);
            }
            
            // 目标震动
            if (target != null)
            {
                target.DOShakeAnchorPos(0.1f, clickShakeIntensity, 10, 90, false, true);
            }
            
            // 额外效果粒子
            if (MainMenuVisualManager.Instance != null)
            {
                MainMenuVisualManager.Instance.SpawnEffectParticle(
                    position,
                    new Color(0.2f, 0.9f, 1f, 0.8f),
                    0.06f
                );
            }
        }
        
        #endregion
        
        #region 错误提示
        
        /// <summary>
        /// 显示错误提示
        /// </summary>
        public void ShowError(string message, RectTransform target = null, float duration = 2f)
        {
            // 音效
            if (errorSound != null)
            {
                AudioSource.PlayClipAtPoint(errorSound, Camera.main.transform.position, 0.7f);
            }
            
            // 目标震动
            if (target != null)
            {
                PlayErrorShake(target);
            }
            
            // 创建错误弹窗
            GameObject errorPopup = GetPooledFeedback();
            if (errorPopup == null && errorPopupPrefab != null)
            {
                errorPopup = Instantiate(errorPopupPrefab, transform);
            }
            
            if (errorPopup != null)
            {
                SetupErrorPopup(errorPopup, message, duration);
            }
            
            // 红色闪烁效果
            if (target != null)
            {
                FlashRed(target);
            }
        }
        
        /// <summary>
        /// 播放错误震动
        /// </summary>
        public void PlayErrorShake(RectTransform target)
        {
            target.DOShakeAnchorPos(errorShakeDuration, errorShakeIntensity, 20, 90, false, true);
            
            // 缩放抖动
            target.DOPunchScale(Vector3.one * 0.1f, errorShakeDuration, 5, 0.5f);
        }
        
        /// <summary>
        /// 红色闪烁
        /// </summary>
        private void FlashRed(RectTransform target)
        {
            var images = target.GetComponentsInChildren<Image>();
            Dictionary<Image, Color> originalColors = new Dictionary<Image, Color>();
            
            foreach (var img in images)
            {
                originalColors[img] = img.color;
                img.color = errorColor;
            }
            
            DOVirtual.DelayedCall(0.15f, () =
            {
                foreach (var kvp in originalColors)
                {
                    if (kvp.Key != null)
                    {
                        kvp.Key.DOColor(kvp.Value, 0.2f);
                    }
                }
            });
        }
        
        /// <summary>
        /// 设置错误弹窗
        /// </summary>
        private void SetupErrorPopup(GameObject popup, string message, float duration)
        {
            popup.SetActive(true);
            
            // 设置文本
            Text messageText = popup.GetComponentInChildren<Text>();
            if (messageText != null)
            {
                messageText.text = message;
                messageText.color = errorColor;
            }
            
            RectTransform rectTransform = popup.GetComponent<RectTransform>();
            
            // 进入动画
            rectTransform.localScale = Vector3.zero;
            rectTransform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            
            // 自动隐藏
            DOVirtual.DelayedCall(duration, () =
            {
                rectTransform.DOScale(0f, 0.2f).SetEase(Ease.InBack)
                    .OnComplete(() =
                    {
                        popup.SetActive(false);
                        ReturnFeedbackToPool(popup);
                    });
            });
        }
        
        #endregion
        
        #region 成功庆祝
        
        /// <summary>
        /// 播放成功庆祝效果
        /// </summary>
        public void PlaySuccessCelebration(Vector3 position, string message = null)
        {
            // 音效
            if (successSound != null)
            {
                AudioSource.PlayClipAtPoint(successSound, Camera.main.transform.position, 0.8f);
            }
            
            // 彩纸粒子
            if (confettiParticles != null)
            {
                ParticleSystem particles = Instantiate(confettiParticles, position, Quaternion.identity);
                
                // 设置颜色
                var main = particles.main;
                main.startColor = new ParticleSystem.MinMaxGradient(successColor, 
                    new Color(0.2f, 0.9f, 1f, 1f));
                
                particles.Play();
                Destroy(particles.gameObject, 3f);
            }
            
            // 创建庆祝效果
            if (celebrationEffectPrefab != null)
            {
                GameObject effect = Instantiate(celebrationEffectPrefab, position, Quaternion.identity, transform);
                
                // 设置文本
                if (!string.IsNullOrEmpty(message))
                {
                    Text text = effect.GetComponentInChildren<Text>();
                    if (text != null)
                    {
                        text.text = message;
                        text.color = successColor;
                    }
                }
                
                // 动画
                RectTransform rectTransform = effect.GetComponent<RectTransform>();
                rectTransform.localScale = Vector3.zero;
                
                Sequence sequence = DOTween.Sequence();
                sequence.Append(rectTransform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack));
                sequence.Append(rectTransform.DOScale(1f, 0.1f).SetEase(Ease.OutQuad));
                sequence.AppendInterval(1f);
                sequence.Append(rectTransform.DOScale(0f, 0.2f).SetEase(Ease.InBack));
                sequence.OnComplete(() =
                {
                    Destroy(effect);
                });
            }
            
            // 额外效果
            SpawnSuccessParticles(position);
        }
        
        /// <summary>
        /// 生成成功粒子
        /// </summary>
        private void SpawnSuccessParticles(Vector3 position)
        {
            if (MainMenuVisualManager.Instance != null)
            {
                // 环形粒子
                for (int i = 0; i < 8; i++)
                {
                    float angle = i * 45f * Mathf.Deg2Rad;
                    Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * 40f;
                    
                    MainMenuVisualManager.Instance.SpawnEffectParticle(
                        position + offset,
                        successColor,
                        0.1f
                    );
                }
                
                // 中心粒子
                MainMenuVisualManager.Instance.SpawnEffectParticle(
                    position,
                    Color.white,
                    0.15f
                );
            }
        }
        
        #endregion
        
        #region 警告动画
        
        /// <summary>
        /// 显示警告
        /// </summary>
        public void ShowWarning(string message, RectTransform target = null, float duration = 3f)
        {
            // 音效
            if (warningSound != null)
            {
                AudioSource.PlayClipAtPoint(warningSound, Camera.main.transform.position, 0.6f);
            }
            
            // 创建警告弹窗
            GameObject warningPopup = GetPooledFeedback();
            if (warningPopup == null && warningPopupPrefab != null)
            {
                warningPopup = Instantiate(warningPopupPrefab, transform);
            }
            
            if (warningPopup != null)
            {
                SetupWarningPopup(warningPopup, message, duration);
            }
            
            // 目标脉冲效果
            if (target != null)
            {
                PlayWarningPulse(target);
            }
        }
        
        /// <summary>
        /// 播放警告脉冲
        /// </summary>
        public void PlayWarningPulse(RectTransform target)
        {
            var images = target.GetComponentsInChildren<Image>();
            
            Sequence pulseSequence = DOTween.Sequence();
            
            for (int i = 0; i < 6; i++)
            {
                pulseSequence.AppendCallback(() =
                {
                    foreach (var img in images)
                    {
                        img.DOColor(warningColor, 0.2f);
                    }
                });
                pulseSequence.AppendInterval(0.2f);
                pulseSequence.AppendCallback(() =
                {
                    foreach (var img in images)
                    {
                        img.DOColor(Color.white, 0.2f);
                    }
                });
                pulseSequence.AppendInterval(0.2f);
            }
        }
        
        /// <summary>
        /// 设置警告弹窗
        /// </summary>
        private void SetupWarningPopup(GameObject popup, string message, float duration)
        {
            popup.SetActive(true);
            
            // 设置文本
            Text messageText = popup.GetComponentInChildren<Text>();
            if (messageText != null)
            {
                messageText.text = message;
                messageText.color = warningColor;
            }
            
            RectTransform rectTransform = popup.GetComponent<RectTransform>();
            
            // 进入动画
            rectTransform.localScale = Vector3.zero;
            rectTransform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            
            // 脉冲边框
            var borderImages = popup.GetComponentsInChildren<Image>();
            Sequence pulseSequence = DOTween.Sequence();
            
            foreach (var img in borderImages)
            {
                if (img.name.Contains("Border"))
                {
                    pulseSequence.Append(img.DOColor(warningColor, 0.3f));
                    pulseSequence.Append(img.DOColor(warningColor.WithAlpha(0.3f), 0.3f));
                }
            }
            pulseSequence.SetLoops(-1, LoopType.Yoyo);
            
            // 自动隐藏
            DOVirtual.DelayedCall(duration, () =
            {
                pulseSequence.Kill();
                rectTransform.DOScale(0f, 0.2f).SetEase(Ease.InBack)
                    .OnComplete(() =
                    {
                        popup.SetActive(false);
                        ReturnFeedbackToPool(popup);
                    });
            });
        }
        
        #endregion
        
        #region 对象池
        
        private GameObject GetPooledFeedback()
        {
            if (feedbackPool.Count > 0)
            {
                return feedbackPool.Dequeue();
            }
            return null;
        }
        
        private void ReturnFeedbackToPool(GameObject feedback)
        {
            if (feedbackPool.Count < MAX_POOL_SIZE)
            {
                feedbackPool.Enqueue(feedback);
            }
            else
            {
                Destroy(feedback);
            }
        }
        
        #endregion
        
        /// <summary>
        /// 震动摄像机
        /// </summary>
        public void ShakeCamera(float duration = 0.3f, float intensity = 0.5f)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null) return;
            
            mainCamera.transform.DOShakePosition(duration, intensity, 10, 90, false, true);
        }
    }
    
    #region 扩展
    
    public static class FeedbackColorExtensions
    {
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }
    }
    
    #endregion
}
