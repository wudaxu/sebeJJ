using UnityEngine;
using System;

namespace SebeJJ.Player
{
    /// <summary>
    /// 机甲控制器 - 处理移动、扫描、采集等核心功能
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class MechController : MonoBehaviour
    {
        public static MechController Instance { get; private set; }
        
        [Header("移动设置")]
        public float moveSpeed = 5f;
        public float acceleration = 10f;
        public float deceleration = 15f;
        public float rotationSpeed = 360f;
        
        [Header("扫描设置")]
        public float scanRange = 10f;
        public float scanAngle = 90f;
        public float scanCooldown = 2f;
        public LayerMask scannableLayers;
        
        [Header("采集设置")]
        public float collectRange = 2f;
        public float collectDuration = 1f;
        
        [Header("组件引用")]
        public Transform mechVisual;
        public Transform scanOrigin;
        
        // 组件
        private Rigidbody2D rb;
        private Vector2 moveInput;
        private float currentSpeed;
        private bool isScanning;
        private bool isCollecting;
        private float lastScanTime;
        
        // 事件
        public event Action OnScanPerformed;
        public event Action<CollectibleResource> OnResourceCollected;
        public event Action OnDamageTaken;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f; // 深海无重力
            rb.drag = 2f; // 水的阻力
            
            // BUG-001 修复: 设置连续碰撞检测防止穿墙
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
        
        private void Update()
        {
            HandleInput();
            HandleRotation();
        }
        
        private void FixedUpdate()
        {
            HandleMovement();
            PreventWallClipping(); // BUG-001 修复: 防穿墙处理
        }
        
        private void HandleInput()
        {
            // 移动输入
            moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            moveInput = Vector2.ClampMagnitude(moveInput, 1f);
            
            // 扫描
            if (Input.GetKeyDown(KeyCode.Space) && CanScan())
            {
                PerformScan();
            }
            
            // 采集
            if (Input.GetKeyDown(KeyCode.E) && !isCollecting)
            {
                TryCollect();
            }
        }
        
        private void HandleMovement()
        {
            if (moveInput.magnitude > 0.1f)
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, moveSpeed, acceleration * Time.fixedDeltaTime);
            }
            else
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.fixedDeltaTime);
            }
            
            Vector2 targetVelocity = moveInput * currentSpeed;
            rb.velocity = Vector2.MoveTowards(rb.velocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            
            // 消耗能源
            if (moveInput.magnitude > 0.1f)
            {
                Core.GameManager.Instance?.resourceManager?.ConsumeEnergy(moveSpeed * Time.fixedDeltaTime * 0.1f);
            }
        }
        
        private void HandleRotation()
        {
            if (moveInput.magnitude > 0.1f)
            {
                float targetAngle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
                float currentAngle = transform.eulerAngles.z;
                float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
            }
        }
        
        /// <summary>
        /// 是否可以扫描
        /// </summary>
        private bool CanScan()
        {
            return Time.time >= lastScanTime + scanCooldown && 
                   !isScanning && 
                   !isCollecting;
        }
        
        /// <summary>
        /// 执行扫描
        /// </summary>
        private void PerformScan()
        {
            isScanning = true;
            lastScanTime = Time.time;
            
            Debug.Log("[MechController] 执行扫描");
            
            // 消耗能源
            Core.GameManager.Instance?.resourceManager?.ConsumeEnergy(5f);
            
            // 检测范围内的资源
            Collider2D[] hits = Physics2D.OverlapCircleAll(scanOrigin.position, scanRange, scannableLayers);
            
            foreach (var hit in hits)
            {
                var resource = hit.GetComponent<CollectibleResource>();
                if (resource != null)
                {
                    // 检查角度
                    Vector2 dirToResource = (resource.transform.position - transform.position).normalized;
                    float angle = Vector2.Angle(transform.right, dirToResource);
                    
                    if (angle <= scanAngle / 2f)
                    {
                        resource.OnScanned();
                    }
                }
            }
            
            OnScanPerformed?.Invoke();
            
            // 扫描动画完成后重置
            Invoke(nameof(EndScan), 0.5f);
        }
        
        private void EndScan()
        {
            isScanning = false;
        }
        
        /// <summary>
        /// 尝试采集
        /// </summary>
        private void TryCollect()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, collectRange, scannableLayers);
            
            foreach (var hit in hits)
            {
                var resource = hit.GetComponent<CollectibleResource>();
                if (resource != null && resource.CanCollect())
                {
                    StartCollecting(resource);
                    return;
                }
            }
        }
        
        /// <summary>
        /// 开始采集
        /// </summary>
        private void StartCollecting(CollectibleResource resource)
        {
            isCollecting = true;
            Debug.Log($"[MechController] 开始采集: {resource.ResourceName}");
            
            // 消耗能源
            Core.GameManager.Instance?.resourceManager?.ConsumeEnergy(2f);
            
            // 延迟完成采集
            Invoke(nameof(FinishCollecting), collectDuration);
        }
        
        private void FinishCollecting()
        {
            isCollecting = false;
            // 实际采集逻辑在采集物上处理
        }
        
        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(float damage)
        {
            Debug.Log($"[MechController] 受到 {damage} 点伤害");
            OnDamageTaken?.Invoke();
        }
        
        /// <summary>
        /// 获取当前速度
        /// </summary>
        public float GetCurrentSpeed()
        {
            return rb.velocity.magnitude;
        }
        
        /// <summary>
        /// 是否在移动
        /// </summary>
        public bool IsMoving()
        {
            return rb.velocity.magnitude > 0.1f;
        }
        
        /// <summary>
        /// 检查前方是否有碰撞体（防穿墙）
        /// </summary>
        private bool CheckCollisionAhead()
        {
            if (rb.velocity.magnitude < 0.1f) return false;
            
            Vector2 direction = rb.velocity.normalized;
            float checkDistance = Mathf.Max(0.5f, rb.velocity.magnitude * Time.fixedDeltaTime * 2f);
            
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, checkDistance, LayerMask.GetMask("Default", "Terrain", "Obstacle"));
            return hit.collider != null;
        }
        
        /// <summary>
        /// 执行防穿墙处理
        /// </summary>
        private void PreventWallClipping()
        {
            if (CheckCollisionAhead())
            {
                // 减速并稍微反弹
                rb.velocity *= 0.5f;
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // 绘制扫描范围
            Gizmos.color = Color.cyan;
            if (scanOrigin != null)
            {
                Gizmos.DrawWireSphere(scanOrigin.position, scanRange);
                
                // 扫描角度
                Vector3 forward = transform.right;
                Quaternion leftRayRotation = Quaternion.AngleAxis(-scanAngle / 2f, Vector3.forward);
                Quaternion rightRayRotation = Quaternion.AngleAxis(scanAngle / 2f, Vector3.forward);
                Vector3 leftRayDirection = leftRayRotation * forward * scanRange;
                Vector3 rightRayDirection = rightRayRotation * forward * scanRange;
                
                Gizmos.DrawLine(scanOrigin.position, scanOrigin.position + leftRayDirection);
                Gizmos.DrawLine(scanOrigin.position, scanOrigin.position + rightRayDirection);
            }
            
            // 绘制采集范围
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, collectRange);
        }
    }
}
