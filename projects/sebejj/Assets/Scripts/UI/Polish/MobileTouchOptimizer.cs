using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

namespace SebeJJ.UI.Polish
{
    /// <summary>
    /// 移动端触控优化 - 触控反馈和手势支持
    /// </summary>
    public class MobileTouchOptimizer : MonoBehaviour
    {
        public static MobileTouchOptimizer Instance { get; private set; }
        
        [Header("触控反馈")]
        [SerializeField] private bool enableHapticFeedback = true;
        [SerializeField] private bool enableVisualFeedback = true;
        [SerializeField] private float touchFeedbackDuration = 0.1f;
        
        [Header("触控区域")]
        [SerializeField] private float minTouchSize = 44f; // 苹果推荐的最小触控尺寸
        [SerializeField] private float touchPadding = 10f;
        
        [Header("长按设置")]
        [SerializeField] private float longPressDuration = 0.5f;
        [SerializeField] private bool enableLongPress = true;
        
        [Header("滑动设置")]
        [SerializeField] private float swipeThreshold = 50f;
        [SerializeField] private float swipeVelocityThreshold = 100f;
        
        [Header("双击设置")]
        [SerializeField] private float doubleClickTime = 0.3f;
        
        [Header("视觉反馈")]
        [SerializeField] private GameObject touchRipplePrefab;
        [SerializeField] private Color rippleColor = new Color(0.2f, 0.9f, 1f, 0.5f);
        
        // 状态
        private Vector2 touchStartPosition;
        private float touchStartTime;
        private bool isTouching = false;
        private bool isLongPress = false;
        private int lastClickTime = 0;
        
        // 事件
        public System.Action<Vector2> OnTouchBegan;
        public System.Action<Vector2> OnTouchEnded;
        public System.Action<Vector2> OnLongPress;
        public System.Action<Vector2, Vector2, float> OnSwipe;
        public System.Action<Vector2> OnDoubleTap;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Update()
        {
            HandleTouchInput();
        }
        
        /// <summary>
        /// 处理触控输入
        /// </summary>
        private void HandleTouchInput()
        {
            // 移动端触控
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                Vector2 touchPosition = touch.position;
                
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        OnTouchBegin(touchPosition);
                        break;
                        
                    case TouchPhase.Ended:
                        OnTouchEnd(touchPosition);
                        break;
                        
                    case TouchPhase.Moved:
                        OnTouchMove(touchPosition);
                        break;
                        
                    case TouchPhase.Canceled:
                        OnTouchCancel();
                        break;
                }
            }
            // 编辑器鼠标模拟
            #if UNITY_EDITOR
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    OnTouchBegin(Input.mousePosition);
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    OnTouchEnd(Input.mousePosition);
                }
                else if (Input.GetMouseButton(0))
                {
                    OnTouchMove(Input.mousePosition);
                }
            }
            #endif
        }
        
        /// <summary>
        /// 触控开始
        /// </summary>
        private void OnTouchBegin(Vector2 position)
        {
            isTouching = true;
            isLongPress = false;
            touchStartPosition = position;
            touchStartTime = Time.time;
            
            OnTouchBegan?.Invoke(position);
            
            // 视觉反馈
            if (enableVisualFeedback)
            {
                SpawnTouchRipple(position);
            }
            
            // 触觉反馈
            if (enableHapticFeedback)
            {
                TriggerHapticFeedback();
            }
            
            // 开始长按检测
            if (enableLongPress)
            {
                Invoke(nameof(TriggerLongPress), longPressDuration);
            }
            
            // 双击检测
            int currentTime = Mathf.RoundToInt(Time.time * 1000);
            if (currentTime - lastClickTime < doubleClickTime * 1000)
            {
                OnDoubleTap?.Invoke(position);
            }
            lastClickTime = currentTime;
        }
        
        /// <summary>
        /// 触控结束
        /// </summary>
        private void OnTouchEnd(Vector2 position)
        {
            if (!isTouching) return;
            
            isTouching = false;
            CancelInvoke(nameof(TriggerLongPress));
            
            OnTouchEnded?.Invoke(position);
            
            // 检测滑动
            Vector2 delta = position - touchStartPosition;
            float duration = Time.time - touchStartTime;
            float velocity = delta.magnitude / duration;
            
            if (delta.magnitude > swipeThreshold && velocity > swipeVelocityThreshold)
            {
                OnSwipe?.Invoke(touchStartPosition, position, velocity);
            }
        }
        
        /// <summary>
        /// 触控移动
        /// </summary>
        private void OnTouchMove(Vector2 position)
        {
            if (!isTouching) return;
            
            // 如果移动距离过大，取消长按
            Vector2 delta = position - touchStartPosition;
            if (delta.magnitude > swipeThreshold * 0.5f)
            {
                CancelInvoke(nameof(TriggerLongPress));
            }
        }
        
        /// <summary>
        /// 触控取消
        /// </summary>
        private void OnTouchCancel()
        {
            isTouching = false;
            CancelInvoke(nameof(TriggerLongPress));
        }
        
        /// <summary>
        /// 触发长按
        /// </summary>
        private void TriggerLongPress()
        {
            if (!isTouching) return;
            
            isLongPress = true;
            OnLongPress?.Invoke(touchStartPosition);
            
            // 触觉反馈
            if (enableHapticFeedback)
            {
                TriggerHapticFeedback();
            }
        }
        
        /// <summary>
        /// 生成触控波纹
        /// </summary>
        private void SpawnTouchRipple(Vector2 position)
        {
            if (touchRipplePrefab == null) return;
            
            GameObject ripple = Instantiate(touchRipplePrefab, transform);
            RectTransform rectTransform = ripple.GetComponent<RectTransform>();
            
            // 设置位置
            rectTransform.position = position;
            rectTransform.localScale = Vector3.zero;
            
            // 设置颜色
            Image image = ripple.GetComponent<Image>();
            if (image != null)
            {
                image.color = rippleColor;
            }
            
            // 动画
            rectTransform.DOScale(2f, 0.5f).SetEase(Ease.OutQuad);
            image?.DOFade(0f, 0.5f).SetEase(Ease.OutQuad)
                .OnComplete(() => Destroy(ripple));
        }
        
        /// <summary>
        /// 触发触觉反馈
        /// </summary>
        private void TriggerHapticFeedback()
        {
            #if UNITY_ANDROID
            Handheld.Vibrate();
            #elif UNITY_IOS
            // iOS使用更精细的触觉反馈API
            Handheld.Vibrate();
            #endif
        }
        
        #region 公共方法
        
        /// <summary>
        /// 优化按钮触控区域
        /// </summary>
        public void OptimizeButtonTouchArea(Button button)
        {
            RectTransform rectTransform = button.GetComponent<RectTransform>();
            if (rectTransform == null) return;
            
            // 确保最小触控尺寸
            Vector2 size = rectTransform.sizeDelta;
            if (size.x < minTouchSize) size.x = minTouchSize;
            if (size.y < minTouchSize) size.y = minTouchSize;
            rectTransform.sizeDelta = size;
            
            // 添加触控反馈
            AddTouchFeedback(button);
        }
        
        /// <summary>
        /// 添加触控反馈
        /// </summary>
        public void AddTouchFeedback(Button button)
        {
            TouchFeedback feedback = button.gameObject.GetComponent<TouchFeedback>();
            if (feedback == null)
            {
                feedback = button.gameObject.AddComponent<TouchFeedback>();
            }
            
            feedback.Initialize(this);
        }
        
        /// <summary>
        /// 启用/禁用触觉反馈
        /// </summary>
        public void SetHapticFeedbackEnabled(bool enabled)
        {
            enableHapticFeedback = enabled;
        }
        
        /// <summary>
        /// 检查是否为触控设备
        /// </summary>
        public bool IsTouchDevice()
        {
            return Application.platform == RuntimePlatform.Android ||
                   Application.platform == RuntimePlatform.IPhonePlayer ||
                   Application.platform == RuntimePlatform.WSAPlayerARM ||
                   Input.touchSupported;
        }
        
        #endregion
    }
    
    /// <summary>
    /// 触控反馈组件
    /// </summary>
    public class TouchFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private MobileTouchOptimizer optimizer;
        private Vector3 originalScale;
        private Button button;
        
        public void Initialize(MobileTouchOptimizer opt)
        {
            optimizer = opt;
            button = GetComponent<Button>();
            originalScale = transform.localScale;
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (button != null && !button.interactable) return;
            
            // 缩放反馈
            transform.DOScale(originalScale * 0.95f, 0.1f).SetEase(Ease.OutQuad);
            
            // 颜色反馈
            var image = GetComponent<Image>();
            if (image != null)
            {
                image.DOColor(image.color.Darken(0.2f), 0.1f);
            }
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            // 恢复缩放
            transform.DOScale(originalScale, 0.15f).SetEase(Ease.OutBack);
            
            // 恢复颜色
            var image = GetComponent<Image>();
            if (image != null)
            {
                image.DOColor(image.color.Brighten(0.2f), 0.15f);
            }
        }
    }
    
    #region 扩展
    
    public static class TouchColorExtensions
    {
        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
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
