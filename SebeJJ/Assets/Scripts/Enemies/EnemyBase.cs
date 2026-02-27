using UnityEngine;
using SebeJJ.Combat;

namespace SebeJJ.Enemies
{
    /// <summary>
    /// 敌人基类
    /// </summary>
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
        [Header("基础属性")]
        [SerializeField] protected string enemyName = "Enemy";
        [SerializeField] protected float moveSpeed = 3f;
        [SerializeField] protected float rotationSpeed = 180f;
        [SerializeField] protected float detectionRange = 10f;
        [SerializeField] protected float attackRange = 2f;
        [SerializeField] protected float attackDamage = 10f;
        [SerializeField] protected float attackCooldown = 1f;

        [Header("AI设置")]
        [SerializeField] protected float patrolRadius = 5f;
        [SerializeField] protected float patrolWaitTime = 2f;
        [SerializeField] protected float chaseSpeedMultiplier = 1.5f;
        [SerializeField] protected float loseInterestTime = 5f;

        [Header("引用")]
        [SerializeField] protected Transform target;
        [SerializeField] protected Transform visualTransform;

        // 组件
        protected Health _health;
        protected Rigidbody2D _rb;
        protected StateMachine _stateMachine;

        // 状态
        protected Vector2 _spawnPosition;
        protected float _lastAttackTime;
        protected float _lastSeePlayerTime;
        protected bool _isAttacking;

        // 属性
        public string EnemyName => enemyName;
        public float MoveSpeed => moveSpeed;
        public float DetectionRange => detectionRange;
        public float AttackRange => attackRange;
        public Transform Target => target;
        public Vector2 SpawnPosition => _spawnPosition;
        public bool IsAlive => _health?.IsAlive ?? false;
        public float HealthPercent => _health?.HealthPercent ?? 0f;
        public StateMachine StateMachine => _stateMachine;

        // 事件
        public System.Action OnEnemyDeath;
        public System.Action OnTargetDetected;
        public System.Action OnTargetLost;

        protected virtual void Awake()
        {
            _health = GetComponent<Health>();
            _rb = GetComponent<Rigidbody2D>();
            _stateMachine = new StateMachine();

            _rb.gravityScale = 0f;
            _rb.drag = 2f;
        }

        protected virtual void Start()
        {
            _spawnPosition = transform.position;
            
            // 注册死亡事件
            if (_health != null)
            {
                _health.OnDeath += OnDeath;
            }

            // 初始化状态机
            InitializeStates();
        }

        protected virtual void Update()
        {
            _stateMachine?.Update();
            UpdateTargetDetection();
        }

        protected virtual void FixedUpdate()
        {
            _stateMachine?.FixedUpdate();
        }

        /// <summary>
        /// 初始化状态（子类实现）
        /// </summary>
        protected abstract void InitializeStates();

        /// <summary>
        /// 更新目标检测
        /// </summary>
        protected virtual void UpdateTargetDetection()
        {
            if (target == null)
            {
                // 寻找玩家
                FindPlayer();
            }
            else
            {
                // 检查是否还能看到玩家
                float distanceToTarget = Vector2.Distance(transform.position, target.position);
                
                if (distanceToTarget <= detectionRange)
                {
                    _lastSeePlayerTime = Time.time;
                }
                else if (Time.time - _lastSeePlayerTime > loseInterestTime)
                {
                    LoseTarget();
                }
            }
        }

        /// <summary>
        /// 寻找玩家
        /// </summary>
        protected virtual void FindPlayer()
        {
            // 使用标签查找玩家
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            
            if (player != null)
            {
                float distance = Vector2.Distance(transform.position, player.transform.position);
                
                if (distance <= detectionRange)
                {
                    SetTarget(player.transform);
                }
            }
        }

        /// <summary>
        /// 设置目标
        /// </summary>
        public virtual void SetTarget(Transform newTarget)
        {
            if (target == newTarget) return;

            target = newTarget;
            
            if (target != null)
            {
                _lastSeePlayerTime = Time.time;
                OnTargetDetected?.Invoke();
            }
        }

        /// <summary>
        /// 丢失目标
        /// </summary>
        public virtual void LoseTarget()
        {
            if (target != null)
            {
                target = null;
                OnTargetLost?.Invoke();
            }
        }

        /// <summary>
        /// 移动到指定位置
        /// </summary>
        public virtual void MoveTo(Vector2 position, float speedMultiplier = 1f)
        {
            Vector2 direction = (position - (Vector2)transform.position).normalized;
            float speed = moveSpeed * speedMultiplier;

            _rb.velocity = Vector2.Lerp(_rb.velocity, direction * speed, 5f * Time.fixedDeltaTime);

            // 旋转朝向移动方向
            if (direction != Vector2.zero)
            {
                float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                float currentAngle = transform.eulerAngles.z;
                float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.fixedDeltaTime);
                _rb.rotation = newAngle;
            }
        }

        /// <summary>
        /// 停止移动
        /// </summary>
        public virtual void StopMoving()
        {
            _rb.velocity = Vector2.Lerp(_rb.velocity, Vector2.zero, 5f * Time.fixedDeltaTime);
        }

        /// <summary>
        /// 尝试攻击
        /// </summary>
        public virtual bool TryAttack()
        {
            if (Time.time < _lastAttackTime + attackCooldown) return false;
            if (target == null) return false;

            float distanceToTarget = Vector2.Distance(transform.position, target.position);
            if (distanceToTarget > attackRange) return false;

            PerformAttack();
            _lastAttackTime = Time.time;
            return true;
        }

        /// <summary>
        /// 执行攻击（子类实现）
        /// </summary>
        protected abstract void PerformAttack();

        /// <summary>
        /// 受到伤害
        /// </summary>
        public virtual void TakeDamage(DamageInfo damageInfo)
        {
            _health?.TakeDamage(damageInfo);

            // 受到伤害时可能发现攻击者
            if (damageInfo.source != null && target == null)
            {
                SetTarget(damageInfo.source.transform);
            }
        }

        /// <summary>
        /// 治疗
        /// </summary>
        public virtual void Heal(float amount)
        {
            _health?.Heal(amount);
        }

        /// <summary>
        /// 死亡处理
        /// </summary>
        protected virtual void OnDeath()
        {
            OnEnemyDeath?.Invoke();
            
            // 掉落物品
            OnDropItems();

            // 销毁
            Destroy(gameObject, 2f);
        }

        /// <summary>
        /// 掉落物品（子类实现）
        /// </summary>
        protected virtual void OnDropItems() { }

        /// <summary>
        /// 看向目标
        /// </summary>
        protected virtual void LookAt(Vector2 position)
        {
            Vector2 direction = (position - (Vector2)transform.position).normalized;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, targetAngle);
        }

        protected virtual void OnDrawGizmosSelected()
        {
            // 检测范围
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);

            // 攻击范围
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // 巡逻范围
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(_spawnPosition, patrolRadius);
        }
    }
}
