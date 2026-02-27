/**
 * @file MechFishAI.cs
 * @brief 机械鱼AI - 任务E1-001~003
 * @description 实现机械鱼的游荡、追击、冲撞攻击行为
 * @author AI系统架构师
 * @date 2026-02-27
 */

using UnityEngine;
using SebeJJ.AI;

namespace SebeJJ.Enemies
{
    /// <summary>
    /// 机械鱼AI - 快速游动的敌人
    /// 行为特点：
    /// - Idle: 原地悬浮，轻微摆动
    /// - Patrol: 在区域内随机游动
    /// - Chase: 发现玩家后快速追击
    /// - Attack: 冲撞攻击
    /// </summary>
    public class MechFishAI : EnemyBase
    {
        #region 序列化字段

        [Header("机械鱼特有配置")]
        [SerializeField] private float wanderRadius = 8f;
        [SerializeField] private float wanderInterval = 3f;
        [SerializeField] private float chargeSpeed = 6f;
        [SerializeField] private float chargeDuration = 0.5f;
        [SerializeField] private float chargeCooldown = 2f;
        [SerializeField] private AnimationCurve chargeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("冲撞攻击")]
        [SerializeField] private float chargeUpTime = 0.5f;
        [SerializeField] private float chargeDistance = 4f;
        [SerializeField] private float chargeDamageMultiplier = 1.5f;
        [SerializeField] private LayerMask damageLayer;
        [SerializeField] private float chargeWarningTime = 0.3f;     // 冲撞预警时间

        [Header("受击设置")]
        [SerializeField] private float hitStunDuration = 0.15f;      // 受击硬直
        [SerializeField] private float hitKnockbackForce = 6f;       // 受击击退

        [Header("特效")]
        [SerializeField] private ParticleSystem chargeEffect;
        [SerializeField] private ParticleSystem swimTrail;

        // 组件引用
        private EnemyHitReaction hitReaction;
        private CombatWarningSystem warningSystem;

        #endregion

        #region 私有字段

        /// <summary>
        /// 游荡计时器
        /// </summary>
        private float _wanderTimer = 0f;

        /// <summary>
        /// 当前游荡目标点
        /// </summary>
        private Vector3 _wanderTarget;

        /// <summary>
        /// 是否正在冲撞
        /// </summary>
        private bool _isCharging = false;

        /// <summary>
        /// 冲撞开始位置
        /// </summary>
        private Vector3 _chargeStartPos;

        /// <summary>
        /// 冲撞方向
        /// </summary>
        private Vector3 _chargeDirection;

        /// <summary>
        /// 冲撞计时器
        /// </summary>
        private float _chargeTimer = 0f;

        /// <summary>
        /// 上次冲撞时间
        /// </summary>
        private float _lastChargeTime = -999f;

        #endregion

        #region Unity生命周期

        protected override void Awake()
        {
            base.Awake();
            enemyType = EnemyType.MechFish;

            // 初始化游荡目标
            _wanderTarget = GetRandomWanderPoint();

            // 获取组件
            hitReaction = GetComponent<EnemyHitReaction>();
            if (hitReaction == null)
            {
                hitReaction = gameObject.AddComponent<EnemyHitReaction>();
            }
        }

        protected override void Update()
        {
            base.Update();

            if (_isDead) return;

            // 更新冲撞
            if (_isCharging)
            {
                UpdateCharge();
            }
        }

        #endregion

        #region 状态初始化

        protected override void InitializeStates()
        {
            // 注册状态
            StateMachine.RegisterState(EnemyState.Idle, new MechFishIdleState(this));
            StateMachine.RegisterState(EnemyState.Patrol, new MechFishPatrolState(this));
            StateMachine.RegisterState(EnemyState.Chase, new MechFishChaseState(this));
            StateMachine.RegisterState(EnemyState.Attack, new MechFishAttackState(this));
            StateMachine.RegisterState(EnemyState.Dead, new MechFishDeadState(this));

            // 注册状态转换条件
            StateMachine.RegisterTransitionCondition((from, to) => {
                // Idle可以转换到Patrol或Chase
                return to == EnemyState.Patrol || to == EnemyState.Chase || Perception.HasTarget;
            }, EnemyState.Patrol, EnemyState.Idle);

            StateMachine.RegisterTransitionCondition((from, to) => {
                // 发现目标时从任何状态切换到Chase
                return Perception.HasTarget;
            }, EnemyState.Chase);
        }

        #endregion

        #region 游荡行为

        /// <summary>
        /// 获取随机游荡点
        /// </summary>
        /// <returns>随机点</returns>
        public Vector3 GetRandomWanderPoint()
        {
            Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
            return SpawnPosition + new Vector3(randomCircle.x, randomCircle.y, 0);
        }

        /// <summary>
        /// 更新游荡
        /// </summary>
        public void UpdateWander()
        {
            _wanderTimer += Time.deltaTime;

            if (_wanderTimer >= wanderInterval)
            {
                _wanderTarget = GetRandomWanderPoint();
                _wanderTimer = 0f;
            }

            // 向游荡目标移动
            MoveTo(_wanderTarget, 0.5f);
        }

        #endregion

        #region 冲撞攻击

        /// <summary>
        /// 是否可以冲撞
        /// </summary>
        public bool CanCharge => Time.time >= _lastChargeTime + chargeCooldown;

        /// <summary>
        /// 开始冲撞
        /// </summary>
        public void StartCharge()
        {
            if (_isCharging || CurrentTarget == null) return;

            _isCharging = true;
            _chargeStartPos = transform.position;
            _chargeDirection = (CurrentTarget.position - transform.position).normalized;
            _chargeTimer = 0f;
            _lastChargeTime = Time.time;

            // 触发预警
            CombatWarningSystem.Instance?.TriggerWarning(transform, CombatWarningSystem.WarningType.Charge, chargeWarningTime);

            // 播放特效
            chargeEffect?.Play();

            // 设置动画
            SetAnimationTrigger("Charge");

            // 短暂蓄力
            Invoke(nameof(ExecuteCharge), chargeUpTime);
        }

        /// <summary>
        /// 执行冲撞
        /// </summary>
        private void ExecuteCharge()
        {
            if (_isDead) return;

            // 检测冲撞路径上的敌人
            RaycastHit2D[] hits = Physics2D.RaycastAll(_chargeStartPos, _chargeDirection, chargeDistance, damageLayer);

            foreach (var hit in hits)
            {
                if (hit.transform != transform)
                {
                    var damageable = hit.transform.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        damageable.TakeDamage(stats.attackDamage * chargeDamageMultiplier, transform);
                    }
                }
            }
        }

        /// <summary>
        /// 更新冲撞
        /// </summary>
        private void UpdateCharge()
        {
            _chargeTimer += Time.deltaTime;
            float progress = _chargeTimer / chargeDuration;

            if (progress >= 1f)
            {
                EndCharge();
                return;
            }

            // 使用动画曲线控制速度
            float curveValue = chargeCurve.Evaluate(progress);
            float speed = Mathf.Lerp(chargeSpeed, stats.moveSpeed, curveValue);

            // 移动
            Vector3 newPosition = transform.position + _chargeDirection * speed * Time.deltaTime;

            if (rb != null)
            {
                rb.MovePosition(newPosition);
            }
            else
            {
                transform.position = newPosition;
            }
        }

        /// <summary>
        /// 结束冲撞
        /// </summary>
        private void EndCharge()
        {
            _isCharging = false;
            chargeEffect?.Stop();

            // 切换回追击状态
            if (!IsDead)
            {
                StateMachine.ChangeState(EnemyState.Chase);
            }
        }

        /// <summary>
        /// 执行普通攻击
        /// </summary>
        public override void PerformAttack()
        {
            if (!CanAttack) return;
            
            // 如果距离较远且可以冲撞，使用冲撞攻击
            if (CurrentTarget != null)
            {
                float distance = Vector3.Distance(transform.position, CurrentTarget.position);
                
                if (distance > stats.attackRange * 1.5f && CanCharge)
                {
                    StartCharge();
                    return;
                }
            }
            
            // 触发攻击预警
            CombatWarningSystem.Instance?.TriggerWarning(transform, CombatWarningSystem.WarningType.Attack, 0.2f);
            
            // 普通攻击
            RecordAttack();
            SetAnimationTrigger("Attack");
            
            // 延迟执行伤害（配合预警）
            Invoke(nameof(ExecuteMeleeAttack), 0.1f);
        }

        /// <summary>
        /// 执行近战攻击伤害
        /// </summary>
        private void ExecuteMeleeAttack()
        {
            // 检测攻击范围内的目标
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, stats.attackRange, damageLayer);
            
            foreach (var hit in hits)
            {
                if (hit.transform != transform)
                {
                    var damageable = hit.transform.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        damageable.TakeDamage(stats.attackDamage, transform);
                    }
                }
            }
        }

        #endregion

        #region 调试

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            // 绘制游荡范围
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(SpawnPosition, wanderRadius);

            // 绘制当前游荡目标
            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(_wanderTarget, 0.3f);
                Gizmos.DrawLine(transform.position, _wanderTarget);
            }

            // 绘制冲撞范围
            if (_isCharging)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(_chargeStartPos, _chargeDirection * chargeDistance);
            }
        }

        #endregion
    }

    #region 机械鱼状态类

    /// <summary>
    /// 机械鱼待机状态
    /// </summary>
    public class MechFishIdleState : AIStateBase
    {
        private MechFishAI _fish;
        private float _idleTimer = 0f;
        private float _idleDuration;

        public MechFishIdleState(MechFishAI fish)
        {
            _fish = fish;
        }

        public override void OnEnter()
        {
            _idleTimer = 0f;
            _idleDuration = Random.Range(1f, 3f);
            _fish.SetAnimationBool("IsMoving", false);
        }

        public override void OnUpdate(float deltaTime)
        {
            _idleTimer += deltaTime;

            // 检测玩家
            if (_fish.Perception.HasTarget)
            {
                ChangeState(EnemyState.Chase);
                return;
            }

            // 待机时间结束，开始游荡
            if (_idleTimer >= _idleDuration)
            {
                ChangeState(EnemyState.Patrol);
            }
        }

        public override void OnExit()
        {
        }
    }

    /// <summary>
    /// 机械鱼游荡状态
    /// </summary>
    public class MechFishPatrolState : AIStateBase
    {
        private MechFishAI _fish;

        public MechFishPatrolState(MechFishAI fish)
        {
            _fish = fish;
        }

        public override void OnEnter()
        {
            _fish.SetAnimationBool("IsMoving", true);
        }

        public override void OnUpdate(float deltaTime)
        {
            // 检测玩家
            if (_fish.Perception.HasTarget)
            {
                ChangeState(EnemyState.Chase);
                return;
            }

            // 执行游荡
            _fish.UpdateWander();
        }

        public override void OnExit()
        {
            _fish.SetAnimationBool("IsMoving", false);
        }
    }

    /// <summary>
    /// 机械鱼追击状态
    /// </summary>
    public class MechFishChaseState : AIStateBase
    {
        private MechFishAI _fish;
        private float _chaseTimer = 0f;
        private float _loseTargetTime = 3f;

        public MechFishChaseState(MechFishAI fish)
        {
            _fish = fish;
        }

        public override void OnEnter()
        {
            _chaseTimer = 0f;
            _fish.SetAnimationBool("IsMoving", true);
            _fish.SetAnimationBool("IsChasing", true);
        }

        public override void OnUpdate(float deltaTime)
        {
            // 检查目标
            if (_fish.CurrentTarget == null)
            {
                _chaseTimer += deltaTime;

                if (_chaseTimer >= _loseTargetTime)
                {
                    // 丢失目标，返回游荡
                    ChangeState(EnemyState.Patrol);
                    return;
                }
            }
            else
            {
                _chaseTimer = 0f;

                // 检查是否在攻击范围内
                if (_fish.IsInAttackRange())
                {
                    ChangeState(EnemyState.Attack);
                    return;
                }

                // 追击目标
                _fish.MoveTo(_fish.CurrentTarget.position);
            }
        }

        public override void OnExit()
        {
            _fish.SetAnimationBool("IsMoving", false);
            _fish.SetAnimationBool("IsChasing", false);
        }
    }

    /// <summary>
    /// 机械鱼攻击状态
    /// </summary>
    public class MechFishAttackState : AIStateBase
    {
        private MechFishAI _fish;
        private float _attackTimer = 0f;

        public MechFishAttackState(MechFishAI fish)
        {
            _fish = fish;
        }

        public override void OnEnter()
        {
            _attackTimer = 0f;
            _fish.PerformAttack();
        }

        public override void OnUpdate(float deltaTime)
        {
            _attackTimer += deltaTime;

            // 攻击后返回追击状态
            if (_attackTimer >= _fish.stats.attackCooldown)
            {
                if (_fish.IsInAttackRange())
                {
                    _fish.PerformAttack();
                    _attackTimer = 0f;
                }
                else
                {
                    ChangeState(EnemyState.Chase);
                }
            }
        }

        public override void OnExit()
        {
        }
    }

    /// <summary>
    /// 机械鱼死亡状态
    /// </summary>
    public class MechFishDeadState : AIStateBase
    {
        private MechFishAI _fish;
        private float _deadTimer = 0f;

        public MechFishDeadState(MechFishAI fish)
        {
            _fish = fish;
        }

        public override void OnEnter()
        {
            _fish.SetAnimationBool("IsDead", true);

            // 禁用碰撞
            var collider = _fish.GetComponent<Collider2D>();
            if (collider != null) collider.enabled = false;

            // 禁用物理
            var rb = _fish.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            _deadTimer += deltaTime;

            // 淡出效果
            if (_deadTimer > 2f)
            {
                var sprite = _fish.GetComponent<SpriteRenderer>();
                if (sprite != null)
                {
                    Color color = sprite.color;
                    color.a = Mathf.Lerp(1f, 0f, (_deadTimer - 2f) / 1f);
                    sprite.color = color;
                }
            }
        }

        public override void OnExit()
        {
        }
    }

    #endregion
}
