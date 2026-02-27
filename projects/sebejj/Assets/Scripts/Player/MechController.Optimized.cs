using UnityEngine;
using System;

namespace SebeJJ.Player
{
    /// <summary>
    /// 机甲控制器 - 性能优化版本
    /// 优化点:
    /// 1. 缓存频繁访问的引用
    /// 2. 减少每帧的数学运算
    /// 3. 优化物理检测
    /// 4. 添加更新频率控制
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class MechControllerOptimized : MonoBehaviour
    {
        public static MechControllerOptimized Instance { get; private set; }
        
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
        
        [Header("性能设置")]
        [SerializeField] private float scanCheckInterval = 0.1f; // 扫描检测间隔
        [SerializeField] private int maxScanResults = 16; // 最大扫描结果数
        
        // 组件缓存
        private Rigidbody2D rb;
        private Vector2 moveInput;
        private float currentSpeed;
        private bool isScanning;
        private bool isCollecting;
        private float lastScanTime;
        
        // 缓存引用避免GC
        private Core.GameManager cachedGameManager;
        private Core.ResourceManager cachedResourceManager;
        
        // 非分配性扫描结果缓存
        private Collider2D[] scanResults;
        
        // 扫描计时器
        private float scanCheckTimer;
        
        // 事件
        public event Action OnScanPerformed;
        public event Action<CollectibleResource> OnResourceCollected;
        public event Action OnDamageTaken;
        
        // 缓存的数学运算结果
        private float scanAngleHalf;
        private float scanRangeSqr;
        private float collectRangeSqr;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.drag = 2f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            
            // 预分配扫描结果数组
            scanResults = new Collider2D[maxScanResults];
            
            // 缓存频繁计算的数值
            scanAngleHalf = scanAngle * 0.5f;
            scanRangeSqr = scanRange * scanRange;
            collectRangeSqr = collectRange * collectRange;
            
            // 缓存管理器引用
            CacheManagerReferences();
        }
        
        private void Start()
        {
            // 延迟缓存，确保所有管理器已初始化
            if (cachedGameManager == null)
            {
                CacheManagerReferences();
            }
        }
        
        /// <summary>
        /// 缓存管理器引用，避免每帧链式访问
        /// </summary>
        private void CacheManagerReferences()
        {
            cachedGameManager = Core.GameManager.Instance;
            if (cachedGameManager != null)
            {
                cachedResourceManager = cachedGameManager.resourceManager;
            }
        }
        
        private void Update()
        {
            HandleInput();
            HandleRotation();
            
            // 间隔性扫描检测
            scanCheckTimer += Time.deltaTime;
            if (scanCheckTimer >= scanCheckInterval)
            {
                scanCheckTimer = 0f;
                UpdateScanStatus();
            }
        }
        
        private void FixedUpdate()
        {
            HandleMovement();
            PreventWallClipping();
        }
        
        private void HandleInput()
        {
            // 移动输入
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");
            
            // 手动归一化避免ClampMagnitude开销
            float sqrMagnitude = moveInput.x * moveInput.x + moveInput.y * moveInput.y;
            if (sqrMagnitude > 1f)
            {
                float invMagnitude = 1f / Mathf.Sqrt(sqrMagnitude);
                moveInput.x *= invMagnitude;
                moveInput.y *= invMagnitude;
            }
            
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
            float deltaTime = Time.fixedDeltaTime;
            
            // 使用平方比较避免开方
            if (moveInput.sqrMagnitude > 0.01f)
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, moveSpeed, acceleration * deltaTime);
            }
            else
            {
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * deltaTime);
            }
            
            Vector2 targetVelocity = moveInput * currentSpeed;
            rb.velocity = Vector2.MoveTowards(rb.velocity, targetVelocity, acceleration * deltaTime);
            
            // 消耗能源 - 使用缓存引用
            if (moveInput.sqrMagnitude > 0.01f && cachedResourceManager != null)
            {
                cachedResourceManager.ConsumeEnergy(moveSpeed * deltaTime * 0.1f);
            }
        }
        
        private void HandleRotation()
        {
            if (moveInput.sqrMagnitude > 0.01f)
            {
                float targetAngle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg;
                float currentAngle = transform.eulerAngles.z;
                float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
            }
        }
        
        /// <summary>
        /// 间隔性更新扫描状态
        /// </summary>
        private void UpdateScanStatus()
        {
            // 可以在这里添加扫描范围的视觉提示更新
            // 避免每帧更新
        }
        
        private bool CanScan()
        {
            return Time.time >= lastScanTime + scanCooldown && 
                   !isScanning && 
                   !isCollecting;
        }
        
        /// <summary>
        /// 执行扫描 - 使用非分配性Overlap
        /// </summary>
        private void PerformScan()
        {
            isScanning = true;
            lastScanTime = Time.time;
            
            // 消耗能源
            cachedResourceManager?.ConsumeEnergy(5f);
            
            // 使用非分配性OverlapCircleNonAlloc
            int hitCount = Physics2D.OverlapCircleNonAlloc(
                scanOrigin.position, 
                scanRange, 
                scanResults, 
                scannableLayers
            );
            
            Vector2 scanPos = scanOrigin.position;
            Vector2 right = transform.right;
            
            for (int i = 0; i < hitCount && i < maxScanResults; i++)
            {
                var hit = scanResults[i];
                if (hit == null) continue;
                
                var resource = hit.GetComponent<CollectibleResource>();
                if (resource != null)
                {
                    // 检查角度 - 使用点积避免Atan2
                    Vector2 dirToResource = ((Vector2)resource.transform.position - scanPos).normalized;
                    float dotProduct = Vector2.Dot(right, dirToResource);
                    float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;
                    
                    if (angle <= scanAngleHalf)
                    {
                        resource.OnScanned();
                    }
                }
            }
            
            OnScanPerformed?.Invoke();
            
            // 使用协程替代Invoke避免GC
            StartCoroutine(EndScanCoroutine());
        }
        
        private System.Collections.IEnumerator EndScanCoroutine()
        {
            yield return new WaitForSeconds(0.5f);
            isScanning = false;
        }
        
        private void TryCollect()
        {
            // 使用非分配性检测
            int hitCount = Physics2D.OverlapCircleNonAlloc(
                transform.position, 
                collectRange, 
                scanResults, 
                scannableLayers
            );
            
            Vector2 myPos = transform.position;
            
            for (int i = 0; i < hitCount && i < maxScanResults; i++)
            {
                var hit = scanResults[i];
                if (hit == null) continue;
                
                var resource = hit.GetComponent<CollectibleResource>();
                if (resource != null && resource.CanCollect())
                {
                    // 使用平方距离检查
                    float sqrDist = ((Vector2)resource.transform.position - myPos).sqrMagnitude;
                    if (sqrDist <= collectRangeSqr)
                    {
                        StartCollecting(resource);
                        return;
                    }
                }
            }
        }
        
        private void StartCollecting(CollectibleResource resource)
        {
            isCollecting = true;
            cachedResourceManager?.ConsumeEnergy(2f);
            StartCoroutine(FinishCollectingCoroutine());
        }
        
        private System.Collections.IEnumerator FinishCollectingCoroutine()
        {
            yield return new WaitForSeconds(collectDuration);
            isCollecting = false;
        }
        
        public void TakeDamage(float damage)
        {
            OnDamageTaken?.Invoke();
        }
        
        public float GetCurrentSpeed()
        {
            return rb.velocity.magnitude;
        }
        
        public bool IsMoving()
        {
            return rb.velocity.sqrMagnitude > 0.01f;
        }
        
        private bool CheckCollisionAhead()
        {
            if (rb.velocity.sqrMagnitude < 0.01f) return false;
            
            Vector2 direction = rb.velocity.normalized;
            float checkDistance = Mathf.Max(0.5f, rb.velocity.magnitude * Time.fixedDeltaTime * 2f);
            
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, checkDistance, 
                LayerMask.GetMask("Default", "Terrain", "Obstacle"));
            return hit.collider != null;
        }
        
        private void PreventWallClipping()
        {
            if (CheckCollisionAhead())
            {
                rb.velocity *= 0.5f;
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            if (scanOrigin != null)
            {
                Gizmos.DrawWireSphere(scanOrigin.position, scanRange);
                
                Vector3 forward = transform.right;
                Quaternion leftRayRotation = Quaternion.AngleAxis(-scanAngleHalf, Vector3.forward);
                Quaternion rightRayRotation = Quaternion.AngleAxis(scanAngleHalf, Vector3.forward);
                Vector3 leftRayDirection = leftRayRotation * forward * scanRange;
                Vector3 rightRayDirection = rightRayRotation * forward * scanRange;
                
                Gizmos.DrawLine(scanOrigin.position, scanOrigin.position + leftRayDirection);
                Gizmos.DrawLine(scanOrigin.position, scanOrigin.position + rightRayDirection);
            }
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, collectRange);
        }
    }
}
