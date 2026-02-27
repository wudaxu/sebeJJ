using UnityEngine;

namespace SebeJJ.Enemies
{
    /// <summary>
    /// 敌人基类 - 性能优化版本
    /// 优化点:
    /// 1. 使用平方距离避免开方运算
    /// 2. 缓存玩家位置和Transform
    /// 3. 添加AI更新频率控制
    /// 4. 使用结构体传递数据
    /// </summary>
    public abstract class EnemyBaseOptimized : MonoBehaviour
    {
        [Header("基础属性")]
        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected float currentHealth;
        [SerializeField] protected float attackDamage = 10f;
        [SerializeField] protected float moveSpeed = 5f;
        [SerializeField] protected float detectionRange = 15f;
        [SerializeField] protected float attackRange = 3f;
        
        [Header("深度设置")]
        [SerializeField] protected float minSpawnDepth = 0f;
        [SerializeField] protected float maxSpawnDepth = 100f;
        
        [Header("性能设置")]
        [SerializeField] protected float aiUpdateInterval = 0.1f; // AI更新间隔（秒）
        [SerializeField] protected float distanceCheckInterval = 0.2f; // 距离检测间隔
        
        // 缓存引用
        protected Transform player;
        protected Vector3 playerPosition;
        protected bool isDead = false;
        protected EnemyState currentState = EnemyState.Idle;
        
        // 性能优化：缓存计算结果
        protected float detectionRangeSqr;
        protected float attackRangeSqr;
        protected float sqrDistanceToPlayer;
        protected bool isPlayerInDetectionRange;
        protected bool isPlayerInAttackRange;
        
        // 计时器
        protected float aiUpdateTimer;
        protected float distanceCheckTimer;
        
        // 事件
        public System.Action OnEnemyDeath;
        public System.Action<float> OnHealthChanged;
        
        public enum EnemyState
        {
            Idle,
            Chase,
            Attack,
            Flee,
            Stunned,
            Dead
        }
        
        // 结构体传递数据避免GC
        public struct EnemyContext
        {
            public Vector3 PlayerPosition;
            public float SqrDistanceToPlayer;
            public bool IsPlayerInDetectionRange;
            public bool IsPlayerInAttackRange;
            public float DeltaTime;
        }
        
        protected virtual void Awake()
        {
            currentHealth = maxHealth;
            
            // 预计算平方距离
            detectionRangeSqr = detectionRange * detectionRange;
            attackRangeSqr = attackRange * attackRange;
        }
        
        protected virtual void Start()
        {
            CachePlayerReference();
            InitializeAI();
        }
        
        /// <summary>
        /// 缓存玩家引用
        /// </summary>
        protected virtual void CachePlayerReference()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerPosition = player.position;
            }
        }
        
        protected virtual void Update()
        {
            if (isDead) return;
            
            float deltaTime = Time.deltaTime;
            
            // 更新玩家位置缓存
            if (player != null)
            {
                playerPosition = player.position;
            }
            
            // 间隔性距离检测
            distanceCheckTimer += deltaTime;
            if (distanceCheckTimer >= distanceCheckInterval)
            {
                distanceCheckTimer = 0f;
                UpdateDistanceCache();
            }
            
            // 间隔性AI更新
            aiUpdateTimer += deltaTime;
            if (aiUpdateTimer >= aiUpdateInterval)
            {
                aiUpdateTimer = 0f;
                
                // 构建上下文并更新AI
                var context = new EnemyContext
                {
                    PlayerPosition = playerPosition,
                    SqrDistanceToPlayer = sqrDistanceToPlayer,
                    IsPlayerInDetectionRange = isPlayerInDetectionRange,
                    IsPlayerInAttackRange = isPlayerInAttackRange,
                    DeltaTime = deltaTime
                };
                
                UpdateAI(ref context);
            }
            
            // 每帧更新视觉和动画（保持流畅）
            UpdateVisuals();
        }
        
        /// <summary>
        /// 更新距离缓存 - 使用平方距离
        /// </summary>
        protected virtual void UpdateDistanceCache()
        {
            if (player == null)
            {
                sqrDistanceToPlayer = float.MaxValue;
                isPlayerInDetectionRange = false;
                isPlayerInAttackRange = false;
                return;
            }
            
            Vector3 toPlayer = playerPosition - transform.position;
            sqrDistanceToPlayer = toPlayer.x * toPlayer.x + toPlayer.y * toPlayer.y + toPlayer.z * toPlayer.z;
            
            isPlayerInDetectionRange = sqrDistanceToPlayer <= detectionRangeSqr;
            isPlayerInAttackRange = sqrDistanceToPlayer <= attackRangeSqr;
        }
        
        protected abstract void InitializeAI();
        
        /// <summary>
        /// 使用结构体传递上下文的AI更新
        /// </summary>
        protected abstract void UpdateAI(ref EnemyContext context);
        
        /// <summary>
        /// 更新视觉和动画 - 每帧调用保持流畅
        /// </summary>
        protected virtual void UpdateVisuals() { }
        
        public virtual void TakeDamage(float damage)
        {
            if (isDead) return;
            
            currentHealth -= damage;
            OnHealthChanged?.Invoke(currentHealth / maxHealth);
            
            OnTakeDamage();
            
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        protected virtual void OnTakeDamage() { }
        
        public abstract void PerformAttack();
        
        protected virtual void Die()
        {
            isDead = true;
            currentState = EnemyState.Dead;
            OnEnemyDeath?.Invoke();
            OnDeath();
        }
        
        protected virtual void OnDeath()
        {
            Destroy(gameObject, 2f);
        }
        
        // 优化后的距离检查方法 - 使用缓存值
        protected bool IsPlayerInDetectionRange()
        {
            return isPlayerInDetectionRange;
        }
        
        protected bool IsPlayerInAttackRange()
        {
            return isPlayerInAttackRange;
        }
        
        /// <summary>
        /// 获取到玩家的方向 - 使用缓存位置
        /// </summary>
        protected Vector3 GetDirectionToPlayer()
        {
            if (player == null) return Vector3.zero;
            Vector3 dir = playerPosition - transform.position;
            float sqrMag = dir.x * dir.x + dir.y * dir.y + dir.z * dir.z;
            if (sqrMag > 0.0001f)
            {
                float invMag = 1f / Mathf.Sqrt(sqrMag);
                return new Vector3(dir.x * invMag, dir.y * invMag, dir.z * invMag);
            }
            return Vector3.zero;
        }
        
        /// <summary>
        /// 获取到玩家的距离 - 使用缓存值
        /// </summary>
        protected float GetDistanceToPlayer()
        {
            return Mathf.Sqrt(sqrDistanceToPlayer);
        }
        
        /// <summary>
        /// 获取到玩家的平方距离
        /// </summary>
        protected float GetSqrDistanceToPlayer()
        {
            return sqrDistanceToPlayer;
        }
        
        public bool IsValidSpawnDepth(float depth)
        {
            return depth >= minSpawnDepth && depth <= maxSpawnDepth;
        }
        
        // Getters
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float AttackDamage => attackDamage;
        public float MoveSpeed => moveSpeed;
        public EnemyState CurrentState => currentState;
        public bool IsDead => isDead;
    }
}
