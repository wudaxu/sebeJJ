using UnityEngine;
using System.Collections;
using SebeJJ.Combat;

namespace SebeJJ.Enemies
{
    /// <summary>
    /// 鮟鱇鱼敌人
    /// </summary>
    public class AnglerFish : EnemyBase
    {
        [Header("鮟鱇鱼特性")]
        [SerializeField] private float ambushRange = 6f;
        [SerializeField] private float dashSpeed = 15f;
        [SerializeField] private float dashDuration = 0.3f;
        [SerializeField] private float dashCooldown = 3f;
        [SerializeField] private float disguiseOpacity = 0.3f;
        [SerializeField] private float lureRange = 10f;

        [Header("诱饵灯")]
        [SerializeField] private Transform lureTransform;
        [SerializeField] private Light lureLight;
        [SerializeField] private float lurePulseSpeed = 2f;
        [SerializeField] private float lureBaseIntensity = 1f;

        [Header("伪装")]
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private Collider2D bodyCollider;

        [Header("状态")]
        [SerializeField] private bool isDisguised = true;
        [SerializeField] private bool isDashing = false;

        private float _lastDashTime;
        private Vector2 _dashDirection;

        protected override void Awake()
        {
            base.Awake();
            enemyName = "Angler Fish";
            moveSpeed = 1f; // 平时移动很慢
            detectionRange = 12f;
            attackRange = ambushRange;
            patrolRadius = 5f;
        }

        protected override void Start()
        {
            base.Start();
            
            if (lureTransform == null)
            {
                lureTransform = transform.Find("Lure");
            }

            EnterDisguise();
        }

        protected override void InitializeStates()
        {
            _stateMachine.AddState(new AnglerAmbushState(this));
            _stateMachine.AddState(new AnglerChaseState(this));
            _stateMachine.AddState(new AnglerAttackState(this));
            _stateMachine.AddState(new AnglerDashState(this));

            _stateMachine.ChangeState<AnglerAmbushState>();
        }

        private void Update()
        {
            base.Update();
            UpdateLure();
        }

        /// <summary>
        /// 更新诱饵灯
        /// </summary>
        private void UpdateLure()
        {
            if (lureLight != null)
            {
                float pulse = 0.7f + Mathf.Sin(Time.time * lurePulseSpeed) * 0.3f;
                lureLight.intensity = lureBaseIntensity * pulse;
            }

            // 诱饵灯轻微摆动
            if (lureTransform != null)
            {
                float sway = Mathf.Sin(Time.time * 1.5f) * 5f;
                lureTransform.localRotation = Quaternion.Euler(0, 0, sway);
            }
        }

        /// <summary>
        /// 进入伪装状态
        /// </summary>
        public void EnterDisguise()
        {
            isDisguised = true;
            
            if (bodyRenderer != null)
            {
                Color color = bodyRenderer.color;
                color.a = disguiseOpacity;
                bodyRenderer.color = color;
            }

            // 降低移动速度
            moveSpeed = 1f;
        }

        /// <summary>
        /// 解除伪装
        /// </summary>
        public void ExitDisguise()
        {
            isDisguised = false;
            
            if (bodyRenderer != null)
            {
                Color color = bodyRenderer.color;
                color.a = 1f;
                bodyRenderer.color = color;
            }

            // 恢复正常移动速度
            moveSpeed = 3f;
        }

        /// <summary>
        /// 执行冲刺
        /// </summary>
        public bool TryDash()
        {
            if (isDashing) return false;
            if (Time.time < _lastDashTime + dashCooldown) return false;
            if (target == null) return false;

            float distanceToTarget = Vector2.Distance(transform.position, target.position);
            if (distanceToTarget > ambushRange) return false;

            StartCoroutine(DashCoroutine());
            return true;
        }

        private IEnumerator DashCoroutine()
        {
            isDashing = true;
            _lastDashTime = Time.time;

            // 计算冲刺方向
            _dashDirection = ((Vector2)target.position - (Vector2)transform.position).normalized;

            // 面向目标
            float angle = Mathf.Atan2(_dashDirection.y, _dashDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            // 冲刺
            float timer = 0f;
            while (timer < dashDuration)
            {
                timer += Time.deltaTime;
                _rb.velocity = _dashDirection * dashSpeed;
                
                // 检测碰撞
                CheckDashCollision();
                
                yield return null;
            }

            isDashing = false;
            _rb.velocity = Vector2.zero;
        }

        private void CheckDashCollision()
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, _dashDirection, 1f);
            
            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                // 冲刺命中
                if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
                {
                    DamageInfo damageInfo = new DamageInfo(
                        attackDamage * 2f, // 冲刺伤害加倍
                        DamageType.Physical,
                        _dashDirection,
                        gameObject,
                        false,
                        10f // 强击退
                    );
                    damageable.TakeDamage(damageInfo);
                }

                // 停止冲刺
                StopCoroutine(DashCoroutine());
                isDashing = false;
                _rb.velocity = Vector2.zero;
            }
        }

        protected override void PerformAttack()
        {
            // 尝试冲刺攻击
            if (!TryDash())
            {
                // 普通咬击
                if (target != null)
                {
                    float distance = Vector2.Distance(transform.position, target.position);
                    
                    if (distance <= attackRange)
                    {
                        if (target.TryGetComponent<IDamageable>(out var damageable))
                        {
                            Vector2 direction = ((Vector2)target.position - (Vector2)transform.position).normalized;
                            DamageInfo damageInfo = new DamageInfo(
                                attackDamage,
                                DamageType.Physical,
                                direction,
                                gameObject
                            );
                            damageable.TakeDamage(damageInfo);
                        }
                    }
                }
            }
        }

        protected override void OnDropItems()
        {
            // 掉落生物样本和食物
        }

        public bool IsDisguised => isDisguised;
        public bool IsDashing => isDashing;
        public bool CanDash => Time.time >= _lastDashTime + dashCooldown;
    }

    // 鮟鱇鱼专用状态
    public class AnglerAmbushState : StateBase
    {
        public AnglerAmbushState(EnemyBase enemy) : base(enemy) { }

        public override void Enter()
        {
            if (_enemy is AnglerFish angler)
            {
                angler.EnterDisguise();
            }
        }

        public override void Update()
        {
            if (_enemy.Target != null)
            {
                float distance = Vector2.Distance(_enemy.transform.position, _enemy.Target.position);
                
                if (distance <= _enemy.AttackRange)
                {
                    _enemy.StateMachine.ChangeState<AnglerDashState>();
                }
                else
                {
                    _enemy.StateMachine.ChangeState<AnglerChaseState>();
                }
            }
        }

        public override void FixedUpdate()
        {
            _enemy.StopMoving();
        }
    }

    public class AnglerChaseState : ChaseState
    {
        public AnglerChaseState(EnemyBase enemy) : base(enemy) { }

        public override void Enter()
        {
            if (_enemy is AnglerFish angler)
            {
                angler.ExitDisguise();
            }
        }

        public override void Update()
        {
            base.Update();

            float distance = Vector2.Distance(_enemy.transform.position, _enemy.Target.position);
            
            if (distance <= _enemy.AttackRange)
            {
                _enemy.StateMachine.ChangeState<AnglerAttackState>();
            }
        }
    }

    public class AnglerAttackState : AttackState
    {
        public AnglerAttackState(EnemyBase enemy) : base(enemy) { }

        public override void Update()
        {
            // 尝试冲刺
            if (_enemy is AnglerFish angler && angler.CanDash)
            {
                _enemy.StateMachine.ChangeState<AnglerDashState>();
                return;
            }

            base.Update();
        }
    }

    public class AnglerDashState : StateBase
    {
        public AnglerDashState(EnemyBase enemy) : base(enemy) { }

        public override void Enter()
        {
            if (_enemy is AnglerFish angler)
            {
                angler.ExitDisguise();
                angler.TryDash();
            }
        }

        public override void Update()
        {
            if (_enemy is AnglerFish angler && !angler.IsDashing)
            {
                // 冲刺结束
                if (_enemy.Target != null)
                {
                    float distance = Vector2.Distance(_enemy.transform.position, _enemy.Target.position);
                    
                    if (distance > _enemy.AttackRange * 2f)
                    {
                        _enemy.StateMachine.ChangeState<AnglerAmbushState>();
                    }
                    else
                    {
                        _enemy.StateMachine.ChangeState<AnglerChaseState>();
                    }
                }
                else
                {
                    _enemy.StateMachine.ChangeState<AnglerAmbushState>();
                }
            }
        }
    }
}
