using UnityEngine;

namespace SebeJJ.Core
{
    /// <summary>
    /// 相机控制器 - 平滑跟随玩家 (BUG-012 修复)
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance { get; private set; }
        
        [Header("目标设置")]
        public Transform target;
        public Vector3 offset = new Vector3(0f, 0f, -10f);
        
        [Header("平滑设置")]
        public float smoothSpeed = 5f;
        public float rotationSmoothSpeed = 3f;
        
        [Header("预测跟随")]
        public bool usePrediction = true;
        public float predictionFactor = 0.3f;
        
        [Header("边界限制")]
        public bool useBounds = false;
        public Vector2 minBounds;
        public Vector2 maxBounds;
        
        [Header("动态调整")]
        public float speedZoomFactor = 0.1f;
        public float minZoom = 8f;
        public float maxZoom = 15f;
        
        private Camera cam;
        private Vector3 currentVelocity;
        private Vector3 targetPosition;
        private float targetZoom;
        private Rigidbody2D targetRigidbody;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            cam = GetComponent<Camera>();
            if (cam == null)
            {
                cam = Camera.main;
            }
        }
        
        private void Start()
        {
            // 自动查找玩家
            if (target == null)
            {
                var player = Player.MechController.Instance;
                if (player != null)
                {
                    target = player.transform;
                    targetRigidbody = player.GetComponent<Rigidbody2D>();
                }
            }
            else
            {
                targetRigidbody = target.GetComponent<Rigidbody2D>();
            }
            
            // 初始化位置
            if (target != null)
            {
                transform.position = target.position + offset;
            }
            
            targetZoom = cam.orthographicSize;
        }
        
        // BUG-012 修复: 使用LateUpdate确保在目标移动后更新
        private void LateUpdate()
        {
            if (target == null) return;
            
            CalculateTargetPosition();
            SmoothFollow();
            ApplyBounds();
            UpdateZoom();
        }
        
        /// <summary>
        /// 计算目标位置（包含预测）
        /// </summary>
        private void CalculateTargetPosition()
        {
            targetPosition = target.position + offset;
            
            // 预测跟随
            if (usePrediction && targetRigidbody != null)
            {
                Vector2 velocity = targetRigidbody.velocity;
                targetPosition += (Vector3)(velocity * predictionFactor);
            }
        }
        
        /// <summary>
        /// 平滑跟随
        /// </summary>
        private void SmoothFollow()
        {
            // 使用SmoothDamp实现平滑跟随
            transform.position = Vector3.SmoothDamp(
                transform.position, 
                targetPosition, 
                ref currentVelocity, 
                1f / smoothSpeed
            );
        }
        
        /// <summary>
        /// 应用边界限制
        /// </summary>
        private void ApplyBounds()
        {
            if (!useBounds) return;
            
            Vector3 pos = transform.position;
            pos.x = Mathf.Clamp(pos.x, minBounds.x, maxBounds.x);
            pos.y = Mathf.Clamp(pos.y, minBounds.y, maxBounds.y);
            transform.position = pos;
        }
        
        /// <summary>
        /// 根据速度动态调整缩放
        /// </summary>
        private void UpdateZoom()
        {
            if (targetRigidbody == null) return;
            
            float speed = targetRigidbody.velocity.magnitude;
            float targetSize = Mathf.Lerp(minZoom, maxZoom, speed * speedZoomFactor / maxZoom);
            
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * 2f);
        }
        
        /// <summary>
        /// 设置跟随目标
        /// </summary>
        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            if (target != null)
            {
                targetRigidbody = target.GetComponent<Rigidbody2D>();
            }
        }
        
        /// <summary>
        /// 瞬间移动到目标位置
        /// </summary>
        public void SnapToTarget()
        {
            if (target == null) return;
            
            transform.position = target.position + offset;
            currentVelocity = Vector3.zero;
        }
        
        /// <summary>
        /// 添加屏幕震动效果
        /// </summary>
        public void Shake(float intensity, float duration)
        {
            StartCoroutine(ShakeCoroutine(intensity, duration));
        }
        
        private System.Collections.IEnumerator ShakeCoroutine(float intensity, float duration)
        {
            Vector3 originalPosition = transform.position;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float currentIntensity = intensity * (1f - elapsed / duration);
                
                Vector3 randomOffset = Random.insideUnitSphere * currentIntensity;
                randomOffset.z = 0f; // 保持Z轴不变
                
                transform.position = originalPosition + randomOffset;
                
                yield return null;
            }
            
            // 恢复正常跟随
        }
        
        /// <summary>
        /// 设置边界
        /// </summary>
        public void SetBounds(Vector2 min, Vector2 max)
        {
            minBounds = min;
            maxBounds = max;
            useBounds = true;
        }
        
        /// <summary>
        /// 清除边界
        /// </summary>
        public void ClearBounds()
        {
            useBounds = false;
        }
        
        private void OnDrawGizmosSelected()
        {
            if (useBounds)
            {
                Gizmos.color = Color.yellow;
                Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2f, (minBounds.y + maxBounds.y) / 2f, 0f);
                Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 1f);
                Gizmos.DrawWireCube(center, size);
            }
        }
    }
}
