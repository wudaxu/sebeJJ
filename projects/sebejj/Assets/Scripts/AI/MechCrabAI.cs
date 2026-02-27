/**
 * @file MechCrabAI.cs
 * @brief 机械蟹AI - 任务E2-001~004
 * @description 实现机械蟹的巡逻、防御姿态、钳击攻击行为
 * @author AI系统架构师
 * @date 2026-02-27
 */

using UnityEngine;
using SebeJJ.AI;

namespace SebeJJ.Enemies
{
    /// <summary>
    /// 机械蟹AI - 防御型敌人
    /// 行为特点：
    /// - Patrol: 在固定路线上巡逻
    /// - Defend: 受到伤害时进入防御姿态，大幅减伤
    /// - Attack: 钳击攻击，可连击
    /// - Alert: 警戒状态，提高感知范围
    /// </summary>
    public class MechCrabAI : EnemyBase
    {
        #region 序列化字段

        [Header("机械蟹特有配置")]
        [SerializeField] private float patrolWaitTime = 2f;
        [SerializeField] private float defendDuration = 3f;
        [SerializeField] private float defendCooldown = 5f;
        [SerializeField] private float defenseDamageReduction = 0.75f; // 75%减伤
        [SerializeField] private float alertDetectionMultiplier = 1.5f;

        [Header("钳击攻击")]
        [SerializeField] private int maxComboCount = 3;
        [SerializeField] private float comboWindow = 1.5f;
        [SerializeField] private float comboDamageMultiplier = 1.2f;
        [SerializeField] private float clawRange = 2f;
        [SerializeField] private float clawAngle = 60f;
        [SerializeField] private float attackWarningTime = 0.25f;    // 攻击预警时间

        [Header("受击设置")]
        [SerializeField] private float hitStunDuration = 0.2f;       // 受击硬直
        [SerializeField] private float hitKnockbackForce = 4f;       // 受击击退
        [SerializeField] private float defendBreakStun = 0.8f;       // 破防眩晕

        [Header("巡逻路线")]
        [SerializeField] private Vector3[] patrolPoints;
        [SerializeField] private bool patrolLoop = true;

        [Header("特效")]
        [SerializeField] private ParticleSystem defendEffect;
        [SerializeField] private ParticleSystem clawAttackEffect;
        [SerializeField] private GameObject shieldVisual;

        // 组件引用
        private EnemyHitReaction hitReaction;

        #endregion

        #region 私有字段

        /// <summary>
        /// 当前巡逻点索引
        /// </summary>
        private int _currentPatrolIndex = 0;

        /// <summary>
        /// 是否在等待
        /// </summary>
        private bool _isWaiting = false;

        /// <summary>
        /// 等待计时器
        /// </summary>
        private float _waitTimer = 0f;

        /// <summary>
        /// 是否处于防御姿态
        /// </summary>
        private bool _isDefending = false;

        /// <summary>
        /// 防御计时器
        /// </summary>
        private float _defendTimer = 0f;

        /// <summary>
        /// 上次防御时间
        /// </summary>
        private float _lastDefendTime = -999f;

        /// <summary>
        /// 当前连击数
        /// </summary>
        private int _currentCombo = 0;

        /// <summary>
        /// 连击计时器
        /// </summary>
        private float _comboTimer = 0f;

        /// <summary>
        /// 原始防御减伤值
        /// </summary>
        private float _originalDefenseReduction;

        /// <summary>
        /// 是否处于警戒状态
        /// </summary>
        private bool _isAlert = false;

        #endregion

        #region 公共属性

        /// <summary>
        /// 是否处于防御姿态
        /// </summary>
        public bool IsDefending => _isDefending;

        /// <summary>
        /// 是否可以防御
        /// </summary>
        public bool CanDefend => Time.time >= _lastDefendTime + defendCooldown && !_isDefending;

        /// <summary>
        /// 当前连击数
        /// </summary>
        public int CurrentCombo => _currentCombo;

        #endregion

        #region Unity生命周期

        protected override void Awake()
        {
            base.Awake();
            enemyType = EnemyType.MechCrab;
            _originalDefenseReduction = stats.defenseReduction;

            // 如果没有设置巡逻点，使用默认的
            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                patrolPoints = new Vector3[]
                {
                    SpawnPosition + new Vector3(-5f, 0, 0),
                    SpawnPosition + new Vector3(5f, 0, 0)
                };
            }

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

            // 更新防御状态
            if (_isDefending)
            {
                _defendTimer += Time.deltaTime;
                if (_defendTimer >= defendDuration)
                {
                    EndDefend();
                }
            }

            // 更新连击计时器
            if (_currentCombo > 0)
            {
                _comboTimer += Time.deltaTime;
                if (_comboTimer >= comboWindow)
                {
                    ResetCombo();
                }
            }
        }

        #endregion

        #region 状态初始化

        protected override void InitializeStates()
        {
            // 注册状态
            StateMachine.RegisterState(EnemyState.Idle, new MechCrabIdleState(this));
            StateMachine.RegisterState(EnemyState.Patrol, new MechCrabPatrolState(this));
            StateMachine.RegisterState(EnemyState.Alert, new MechCrabAlertState(this));
            StateMachine.RegisterState(EnemyState.Defend, new MechCrabDefendState(this));
            StateMachine.RegisterState(EnemyState.Chase, new MechCrabChaseState(this));
            StateMachine.RegisterState(EnemyState.Attack, new MechCrabAttackState(this));
            StateMachine.RegisterState(EnemyState.Dead, new MechCrabDeadState(this));
        }

        #endregion

        #region 巡逻行为

        /// <summary>
        /// 更新巡逻
        /// </summary>
        public void UpdatePatrol()
        {
            if (_isWaiting)
            {
                _waitTimer += Time.deltaTime;

                if (_waitTimer >= patrolWaitTime)
                {
                    _isWaiting = false;
                    _waitTimer = 0f;
                    MoveToNextPatrolPoint();
                }

                return;
            }

            // 向当前巡逻点移动
            Vector3 targetPoint = patrolPoints[_currentPatrolIndex];
            bool reached = MoveTo(targetPoint, 0.5f);

            if (reached)
            {
                _isWaiting = true;
                _waitTimer = 0f;
            }
        }

        /// <summary>
        /// 移动到下一个巡逻点
        /// </summary>
        private void MoveToNextPatrolPoint()
        {
            _currentPatrolIndex++;

            if (_currentPatrolIndex >= patrolPoints.Length)
            {
                if (patrolLoop)
                {
                    _currentPatrolIndex = 0;
                }
                else
                {
                    // 往返巡逻
                    System.Array.Reverse(patrolPoints);
                    _currentPatrolIndex = 1;
                }
            }
        }

        /// <summary>
        /// 获取最近的巡逻点
        /// </summary>
        public Vector3 GetNearestPatrolPoint()
        {
            Vector3 nearest = patrolPoints[0];
            float nearestDist = Vector3.Distance(transform.position, nearest);

            for (int i = 1; i < patrolPoints.Length; i++)
            {
                float dist = Vector3.Distance(transform.position, patrolPoints[i]);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = patrolPoints[i];
                }
            }

            return nearest;
        }

        #endregion

        #region 防御行为

        /// <summary>
        /// 开始防御
        /// </summary>
        public void StartDefend()
        {
            if (_isDefending || !CanDefend) return;

            _isDefending = true;
            _defendTimer = 0f;
            _lastDefendTime = Time.time;

            // 应用防御减伤
            stats.defenseReduction = defenseDamageReduction;

            // 播放特效
            defendEffect?.Play();
            if (shieldVisual != null) shieldVisual.SetActive(true);

            // 设置动画
            SetAnimationBool("IsDefending", true);
            SetAnimationTrigger("DefendStart");

            // 切换到防御状态
            StateMachine.ChangeState(EnemyState.Defend);
        }

        /// <summary>
        /// 结束防御
        /// </summary>
        public void EndDefend()
        {
            if (!_isDefending) return;

            _isDefending = false;
            _defendTimer = 0f;

            // 恢复原始防御值
            stats.defenseReduction = _originalDefenseReduction;

            // 停止特效
            defendEffect?.Stop();
            if (shieldVisual != null) shieldVisual.SetActive(false);

            // 设置动画
            SetAnimationBool("IsDefending", false);
            SetAnimationTrigger("DefendEnd");

            // 返回之前的状态
            if (Perception.HasTarget)
            {
                StateMachine.ChangeState(EnemyState.Chase);
            }
            else
            {
                StateMachine.ChangeState(EnemyState.Patrol);
            }
        }

        /// <summary>
        /// 打破防御
        /// </summary>
        public void BreakDefend()
        {
            if (!_isDefending) return;
            
            EndDefend();
            
            // 进入眩晕状态
            hitReaction?.ProcessHitReaction(0, Vector2.zero, 0, defendBreakStun, false);
            SetAnimationTrigger("DefendBreak");
        }

        #endregion

        #region 钳击攻击

        /// <summary>
        /// 执行钳击攻击
        /// </summary>
        public override void PerformAttack()
        {
            if (!CanAttack) return;

            // 触发攻击预警
            CombatWarningSystem.Instance?.TriggerWarning(transform, CombatWarningSystem.WarningType.Attack, attackWarningTime);

            RecordAttack();

            // 延迟执行攻击（配合预警）
            Invoke(nameof(ExecuteClawAttackDelayed), attackWarningTime);
        }

        /// <summary>
        /// 延迟执行钳击
        /// </summary>
        private void ExecuteClawAttackDelayed()
        {
            // 增加连击数
            _currentCombo++;
            _comboTimer = 0f;

            // 计算伤害
            float damageMultiplier = 1f + (_currentCombo - 1) * (comboDamageMultiplier - 1f);
            float finalDamage = stats.attackDamage * damageMultiplier;

            // 播放动画
            SetAnimationInteger("ComboCount", _currentCombo);
            SetAnimationTrigger("Attack");

            // 播放特效
            clawAttackEffect?.Play();

            // 检测扇形范围内的目标
            PerformClawAttack(finalDamage);

            // 检查连击上限
            if (_currentCombo >= maxComboCount)
            {
                ResetCombo();
            }
        }

        /// <summary>
        /// 执行钳击
        /// </summary>
        private void PerformClawAttack(float damage)
        {
            // 获取所有可能的目标
            Collider2D[] potentialTargets = Physics2D.OverlapCircleAll(transform.position, clawRange);

            foreach (var target in potentialTargets)
            {
                if (target.transform == transform) continue;

                // 检查角度
                Vector3 directionToTarget = (target.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.right, directionToTarget);

                if (angle <= clawAngle / 2f)
                {
                    // 在攻击范围内
                    var damageable = target.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        damageable.TakeDamage(damage, transform);
                    }
                }
            }
        }

        /// <summary>
        /// 重置连击
        /// </summary>
        public void ResetCombo()
        {
            _currentCombo = 0;
            _comboTimer = 0f;
            SetAnimationInteger("ComboCount", 0);
        }

        #endregion

        #region 警戒行为

        /// <summary>
        /// 进入警戒状态
        /// </summary>
        public void EnterAlert()
        {
            if (_isAlert) return;

            _isAlert = true;

            // 增加感知范围
            if (Perception != null)
            {
                // 通过反射或直接访问修改（实际项目中应使用属性）
            }

            SetAnimationBool("IsAlert", true);
        }

        /// <summary>
        /// 退出警戒状态
        /// </summary>
        public void ExitAlert()
        {
            if (!_isAlert) return;

            _isAlert = false;
            SetAnimationBool("IsAlert", false);
        }

        #endregion

        #region 伤害处理

        public override void TakeDamage(float damage, Transform damageSource = null)
        {
            // 如果不在防御状态且可以防御，有几率进入防御
            if (!_isDefending && CanDefend && Random.value < 0.3f)
            {
                StartDefend();
                return;
            }

            base.TakeDamage(damage, damageSource);
        }

        #endregion

        #region 调试

        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();

            // 绘制巡逻路线
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                Gizmos.color = Color.blue;

                for (int i = 0; i < patrolPoints.Length; i++)
                {
                    Vector3 point = patrolPoints[i];
                    if (i == 0)
                    {
                        Gizmos.DrawSphere(point, 0.3f);
                    }
                    else
                    {
                        Gizmos.DrawLine(patrolPoints[i - 1], point);
                        Gizmos.DrawSphere(point, 0.2f);
                    }
                }

                if (patrolLoop && patrolPoints.Length > 1)
                {
                    Gizmos.DrawLine(patrolPoints[patrolPoints.Length - 1], patrolPoints[0]);
                }
            }

            // 绘制钳击范围
            Gizmos.color = Color.red;
            Vector3 forward = transform.right;
            Vector3 leftBoundary = Quaternion.Euler(0, 0, clawAngle / 2f) * forward * clawRange;
            Vector3 rightBoundary = Quaternion.Euler(0, 0, -clawAngle / 2f) * forward * clawRange;

            Gizmos.DrawRay(transform.position, leftBoundary);
            Gizmos.DrawRay(transform.position, rightBoundary);
        }

        #endregion

        #region 动画辅助

        protected void SetAnimationInteger(string paramName, int value)
        {
            animator?.SetInteger(paramName, value);
        }

        #endregion
    }

    #region 机械蟹状态类

    /// <summary>
    /// 机械蟹待机状态
    /// </summary>
    public class MechCrabIdleState : AIStateBase
    {
        private MechCrabAI _crab;
        private float _timer = 0f;

        public MechCrabIdleState(MechCrabAI crab)
        {
            _crab = crab;
        }

        public override void OnEnter()
        {
            _timer = 0f;
            _crab.SetAnimationBool("IsMoving", false);
        }

        public override void OnUpdate(float deltaTime)
        {
            _timer += deltaTime;

            // 检测玩家
            if (_crab.Perception.HasTarget)
            {
                _crab.EnterAlert();
                ChangeState(EnemyState.Alert);
                return;
            }

            // 待机一段时间后开始巡逻
            if (_timer >= 1f)
            {
                ChangeState(EnemyState.Patrol);
            }
        }

        public override void OnExit()
        {
        }
    }

    /// <summary>
    /// 机械蟹巡逻状态
    /// </summary>
    public class MechCrabPatrolState : AIStateBase
    {
        private MechCrabAI _crab;

        public MechCrabPatrolState(MechCrabAI crab)
        {
            _crab = crab;
        }

        public override void OnEnter()
        {
            _crab.SetAnimationBool("IsMoving", true);
        }

        public override void OnUpdate(float deltaTime)
        {
            // 检测玩家
            if (_crab.Perception.HasTarget)
            {
                _crab.EnterAlert();
                ChangeState(EnemyState.Alert);
                return;
            }

            // 执行巡逻
            _crab.UpdatePatrol();
        }

        public override void OnExit()
        {
            _crab.SetAnimationBool("IsMoving", false);
        }
    }

    /// <summary>
    /// 机械蟹警戒状态
    /// </summary>
    public class MechCrabAlertState : AIStateBase
    {
        private MechCrabAI _crab;
        private float _alertTimer = 0f;
        private float _maxAlertTime = 2f;

        public MechCrabAlertState(MechCrabAI crab)
        {
            _crab = crab;
        }

        public override void OnEnter()
        {
            _alertTimer = 0f;
            _crab.SetAnimationBool("IsAlert", true);
        }

        public override void OnUpdate(float deltaTime)
        {
            _alertTimer += deltaTime;

            // 确认目标后追击
            if (_crab.Perception.HasTarget && _alertTimer >= 0.5f)
            {
                ChangeState(EnemyState.Chase);
                return;
            }

            // 丢失目标后返回巡逻
            if (!_crab.Perception.HasTarget && _alertTimer >= _maxAlertTime)
            {
                _crab.ExitAlert();
                ChangeState(EnemyState.Patrol);
            }
        }

        public override void OnExit()
        {
            _crab.SetAnimationBool("IsAlert", false);
        }
    }

    /// <summary>
    /// 机械蟹防御状态
    /// </summary>
    public class MechCrabDefendState : AIStateBase
    {
        private MechCrabAI _crab;

        public MechCrabDefendState(MechCrabAI crab)
        {
            _crab = crab;
        }

        public override void OnEnter()
        {
            _crab.SetAnimationBool("IsDefending", true);
        }

        public override void OnUpdate(float deltaTime)
        {
            // 防御状态下可以转向面对目标
            if (_crab.CurrentTarget != null)
            {
                Vector3 direction = _crab.CurrentTarget.position - _crab.transform.position;
                direction.Normalize();

                // 缓慢转向
                float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                float currentAngle = _crab.transform.eulerAngles.z;
                float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, 90f * deltaTime);
                _crab.transform.rotation = Quaternion.Euler(0, 0, newAngle);
            }
        }

        public override void OnExit()
        {
            _crab.SetAnimationBool("IsDefending", false);
        }
    }

    /// <summary>
    /// 机械蟹追击状态
    /// </summary>
    public class MechCrabChaseState : AIStateBase
    {
        private MechCrabAI _crab;
        private float _chaseTimer = 0f;
        private float _loseTargetTime = 5f;

        public MechCrabChaseState(MechCrabAI crab)
        {
            _crab = crab;
        }

        public override void OnEnter()
        {
            _chaseTimer = 0f;
            _crab.SetAnimationBool("IsMoving", true);
            _crab.SetAnimationBool("IsChasing", true);
        }

        public override void OnUpdate(float deltaTime)
        {
            // 检查是否可以防御
            if (_crab.CanDefend && Random.value < 0.1f)
            {
                _crab.StartDefend();
                return;
            }

            // 检查目标
            if (_crab.CurrentTarget == null)
            {
                _chaseTimer += deltaTime;

                if (_chaseTimer >= _loseTargetTime)
                {
                    _crab.ExitAlert();
                    ChangeState(EnemyState.Patrol);
                    return;
                }
            }
            else
                       {
                _chaseTimer = 0f;

                // 检查是否在攻击范围内
                if (_crab.IsInAttackRange())
                {
                    ChangeState(EnemyState.Attack);
                    return;
                }

                // 追击目标
                _crab.MoveTo(_crab.CurrentTarget.position);
            }
        }

        public override void OnExit()
        {
            _crab.SetAnimationBool("IsMoving", false);
            _crab.SetAnimationBool("IsChasing", false);
        }
    }

    /// <summary>
    /// 机械蟹攻击状态
    /// </summary>
    public class MechCrabAttackState : AIStateBase
    {
        private MechCrabAI _crab;
        private float _attackTimer = 0f;

        public MechCrabAttackState(MechCrabAI crab)
        {
            _crab = crab;
        }

        public override void OnEnter()
        {
            _attackTimer = 0f;
            _crab.PerformAttack();
        }

        public override void OnUpdate(float deltaTime)
        {
            _attackTimer += deltaTime;

            // 攻击间隔
            if (_attackTimer >= _crab.stats.attackCooldown)
            {
                if (_crab.IsInAttackRange())
                {
                    _crab.PerformAttack();
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
            _crab.ResetCombo();
        }
    }

    /// <summary>
    /// 机械蟹死亡状态
    /// </summary>
    public class MechCrabDeadState : AIStateBase
    {
        private MechCrabAI _crab;
        private float _deadTimer = 0f;

        public MechCrabDeadState(MechCrabAI crab)
        {
            _crab = crab;
        }

        public override void OnEnter()
        {
            _crab.SetAnimationBool("IsDead", true);

            // 禁用碰撞
            var collider = _crab.GetComponent<Collider2D>();
            if (collider != null) collider.enabled = false;

            // 禁用物理
            var rb = _crab.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
            }

            // 关闭护盾
            if (_crab.IsDefending)
            {
                _crab.EndDefend();
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            _deadTimer += deltaTime;

            // 淡出效果
            if (_deadTimer > 2f)
            {
                var sprite = _crab.GetComponent<SpriteRenderer>();
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
