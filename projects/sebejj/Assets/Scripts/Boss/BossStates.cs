/**
 * @file BossStates.cs
 * @brief Boss AI状态类
 * @description 铁钳巨兽的所有AI状态实现
 * @author Boss战设计师
 * @date 2026-02-27
 */

using UnityEngine;
using SebeJJ.AI;

namespace SebeJJ.Boss
{
    #region Boss状态基类

    /// <summary>
    /// Boss状态基类
    /// </summary>
    public abstract class BossStateBase : AIStateBase
    {
        protected IronClawBeastBoss Boss { get; private set; }
        
        public BossStateBase(IronClawBeastBoss boss)
        {
            Boss = boss;
        }
        
        /// <summary>
        /// 获取当前阶段对应的攻击选择权重
        /// </summary>
        protected virtual BossAttackType SelectAttack()
        {
            return BossAttackType.ClawCombo;
        }
        
        /// <summary>
        /// 检查是否应该转换阶段
        /// </summary>
        protected bool ShouldPhaseTransition()
        {
            return Boss.IsInvincible; // 阶段转换时无敌
        }
    }

    #endregion

    #region 待机状态

    /// <summary>
    /// Boss待机状态
    /// </summary>
    public class BossIdleState : BossStateBase
    {
        private float _timer = 0f;
        private float _maxIdleTime = 1f;
        
        public BossIdleState(IronClawBeastBoss boss) : base(boss) { }
        
        public override void OnEnter()
        {
            _timer = 0f;
            Boss.GetComponent<Animator>()?.SetBool("IsMoving", false);
        }
        
        public override void OnUpdate(float deltaTime)
        {
            _timer += deltaTime;
            
            // 阶段转换期间保持待机
            if (ShouldPhaseTransition()) return;
            
            // 检测目标
            if (Boss.CurrentTarget != null)
            {
                ChangeState(EnemyState.Chase);
                return;
            }
            
            // 待机时间结束后进入巡逻（如果有）
            if (_timer >= _maxIdleTime)
            {
                // Boss通常不会真正"巡逻"，而是保持待机或追击
                _timer = 0f;
            }
        }
        
        public override void OnExit()
        {
        }
    }

    #endregion

    #region 追击状态

    /// <summary>
    /// Boss追击状态
    /// </summary>
    public class BossChaseState : BossStateBase
    {
        private float _chaseTimer = 0f;
        private float _attackDecisionTimer = 0f;
        private float _attackDecisionInterval = 0.5f;
        private float _loseTargetTime = 5f;
        
        public BossChaseState(IronClawBeastBoss boss) : base(boss) { }
        
        public override void OnEnter()
        {
            _chaseTimer = 0f;
            _attackDecisionTimer = 0f;
            Boss.GetComponent<Animator>()?.SetBool("IsMoving", true);
            Boss.GetComponent<Animator>()?.SetBool("IsChasing", true);
        }
        
        public override void OnUpdate(float deltaTime)
        {
            // 阶段转换检查
            if (ShouldPhaseTransition())
            {
                ChangeState(EnemyState.Idle);
                return;
            }
            
            // 检查目标
            if (Boss.CurrentTarget == null)
            {
                _chaseTimer += deltaTime;
                
                if (_chaseTimer >= _loseTargetTime)
                {
                    ChangeState(EnemyState.Idle);
                    return;
                }
            }
            else
            {
                _chaseTimer = 0f;
                
                // 检查是否在攻击范围内
                if (Boss.IsInAttackRange())
                {
                    ChangeState(EnemyState.Attack);
                    return;
                }
                
                // 追击目标
                Boss.MoveTo(Boss.CurrentTarget.position);
                
                // 定期决定使用什么攻击
                _attackDecisionTimer += deltaTime;
                if (_attackDecisionTimer >= _attackDecisionInterval)
                {
                    _attackDecisionTimer = 0f;
                    
                    // 根据距离和阶段决定攻击
                    TrySelectSpecialAttack();
                }
            }
        }
        
        /// <summary>
        /// 尝试选择特殊攻击
        /// </summary>
        private void TrySelectSpecialAttack()
        {
            float distanceToTarget = Vector3.Distance(
                Boss.transform.position, Boss.CurrentTarget.position);
            
            // 第三阶段：优先使用地震
            if (Boss.CurrentPhase == BossPhase.Phase3 && Boss.CanEarthquake)
            {
                if (Random.value < 0.3f)
                {
                    ChangeState(EnemyState.Special);
                    return;
                }
            }
            
            // 第二阶段：激光和召唤
            if (Boss.CurrentPhase >= BossPhase.Phase2)
            {
                // 远距离使用激光
                if (distanceToTarget > 8f && Boss.CanLaser && Random.value < 0.4f)
                {
                    ChangeState(EnemyState.Special);
                    return;
                }
                
                // 召唤小怪
                if (Boss.CanSummon && Random.value < 0.2f)
                {
                    ChangeState(EnemyState.Special);
                    return;
                }
            }
            
            // 第一阶段：冲撞
            if (distanceToTarget > 5f && Boss.CanCharge && Random.value < 0.3f)
            {
                ChangeState(EnemyState.Special);
                return;
            }
            
            // 防御姿态
            if (Boss.CanDefend && Boss.HealthPercent < 0.5f && Random.value < 0.15f)
            {
                ChangeState(EnemyState.Defend);
                return;
            }
        }
        
        public override void OnExit()
        {
            Boss.GetComponent<Animator>()?.SetBool("IsMoving", false);
            Boss.GetComponent<Animator>()?.SetBool("IsChasing", false);
        }
    }

    #endregion

    #region 攻击状态

    /// <summary>
    /// Boss攻击状态
    /// </summary>
    public class BossAttackState : BossStateBase
    {
        private float _attackTimer = 0f;
        private bool _isAttacking = false;
        
        public BossAttackState(IronClawBeastBoss boss) : base(boss) { }
        
        public override void OnEnter()
        {
            _attackTimer = 0f;
            _isAttacking = false;
            
            // 执行攻击
            PerformAttack();
        }
        
        public override void OnUpdate(float deltaTime)
        {
            // 阶段转换检查
            if (ShouldPhaseTransition())
            {
                ChangeState(EnemyState.Idle);
                return;
            }
            
            _attackTimer += deltaTime;
            
            // 等待攻击结束
            if (_isAttacking) return;
            
            // 攻击结束后决定下一步
            if (Boss.CurrentTarget == null)
            {
                ChangeState(EnemyState.Idle);
                return;
            }
            
            // 检查是否还在攻击范围内
            if (!Boss.IsInAttackRange())
            {
                ChangeState(EnemyState.Chase);
                return;
            }
            
            // 继续攻击或选择其他行动
            if (Boss.CanAttack)
            {
                PerformAttack();
            }
            else
            {
                // 等待冷却
                float waitTime = 0.5f;
                if (_attackTimer >= waitTime)
                {
                    // 可以防御或继续等待
                    if (Boss.CanDefend && Random.value < 0.3f)
                    {
                        ChangeState(EnemyState.Defend);
                    }
                }
            }
        }
        
        private void PerformAttack()
        {
            _isAttacking = true;
            
            // 根据阶段选择攻击方式
            switch (Boss.CurrentPhase)
            {
                case BossPhase.Phase1:
                    // 第一阶段：主要使用钳击连击
                    Boss.PerformClawCombo();
                    break;
                    
                case BossPhase.Phase2:
                    // 第二阶段：钳击或根据情况选择
                    if (Random.value < 0.7f)
                    {
                        Boss.PerformClawCombo();
                    }
                    else if (Boss.CanCharge)
                    {
                        Boss.PerformCharge();
                    }
                    break;
                    
                case BossPhase.Phase3:
                    // 第三阶段：狂暴攻击
                    if (Boss.IsWeakPointExposed)
                    {
                        // 弱点暴露时尝试恢复
                        // 这里可以添加恢复逻辑
                    }
                    Boss.PerformClawCombo();
                    break;
            }
            
            // 攻击动画结束后
            Boss.StartCoroutine(AttackEndCoroutine());
        }
        
        private System.Collections.IEnumerator AttackEndCoroutine()
        {
            // 等待攻击动画（根据实际动画长度调整）
            yield return new WaitForSeconds(1.5f);
            _isAttacking = false;
        }
        
        public override void OnExit()
        {
            _isAttacking = false;
        }
    }

    #endregion

    #region 防御状态

    /// <summary>
    /// Boss防御状态
    /// </summary>
    public class BossDefendState : BossStateBase
    {
        private float _defendTimer = 0f;
        private bool _isDefending = false;
        
        public BossDefendState(IronClawBeastBoss boss) : base(boss) { }
        
        public override void OnEnter()
        {
            _defendTimer = 0f;
            _isDefending = true;
            Boss.EnterDefend();
        }
        
        public override void OnUpdate(float deltaTime)
        {
            // 阶段转换检查
            if (ShouldPhaseTransition())
            {
                Boss.ExitDefend();
                ChangeState(EnemyState.Idle);
                return;
            }
            
            _defendTimer += deltaTime;
            
            // 防御期间可以缓慢转向面对目标
            if (Boss.CurrentTarget != null)
            {
                FaceTarget();
            }
            
            // 检查防御是否结束
            if (!Boss.IsDefending)
            {
                _isDefending = false;
                
                // 决定下一步行动
                if (Boss.CurrentTarget != null)
                {
                    if (Boss.IsInAttackRange())
                    {
                        ChangeState(EnemyState.Attack);
                    }
                    else
                    {
                        ChangeState(EnemyState.Chase);
                    }
                }
                else
                {
                    ChangeState(EnemyState.Idle);
                }
            }
        }
        
        private void FaceTarget()
        {
            Vector3 direction = Boss.CurrentTarget.position - Boss.transform.position;
            direction.Normalize();
            
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float currentAngle = Boss.transform.eulerAngles.z;
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, 90f * Time.deltaTime);
            Boss.transform.rotation = Quaternion.Euler(0, 0, newAngle);
        }
        
        public override void OnExit()
        {
            if (_isDefending)
            {
                Boss.ExitDefend();
            }
        }
    }

    #endregion

    #region 特殊技能状态

    /// <summary>
    /// Boss特殊技能状态
    /// </summary>
    public class BossSpecialState : BossStateBase
    {
        private bool _isExecuting = false;
        private BossAttackType _selectedAttack;
        
        public BossSpecialState(IronClawBeastBoss boss) : base(boss) { }
        
        public override void OnEnter()
        {
            _isExecuting = false;
            _selectedAttack = SelectSpecialAttack();
            
            ExecuteSpecialAttack();
        }
        
        public override void OnUpdate(float deltaTime)
        {
            // 阶段转换检查
            if (ShouldPhaseTransition())
            {
                ChangeState(EnemyState.Idle);
                return;
            }
            
            // 等待技能执行完成
            if (_isExecuting) return;
            
            // 技能结束后决定下一步
            if (Boss.CurrentTarget == null)
            {
                ChangeState(EnemyState.Idle);
                return;
            }
            
            if (Boss.IsInAttackRange())
            {
                ChangeState(EnemyState.Attack);
            }
            else
            {
                ChangeState(EnemyState.Chase);
            }
        }
        
        private BossAttackType SelectSpecialAttack()
        {
            float distanceToTarget = Boss.CurrentTarget != null ? 
                Vector3.Distance(Boss.transform.position, Boss.CurrentTarget.position) : 0f;
            
            // 根据阶段和距离选择技能
            switch (Boss.CurrentPhase)
            {
                case BossPhase.Phase1:
                    // 第一阶段：冲撞
                    if (Boss.CanCharge)
                        return BossAttackType.Charge;
                    break;
                    
                case BossPhase.Phase2:
                    // 第二阶段：激光、召唤、冲撞
                    if (distanceToTarget > 8f && Boss.CanLaser)
                        return BossAttackType.LaserSweep;
                    if (Boss.CanSummon)
                        return BossAttackType.Summon;
                    if (Boss.CanCharge)
                        return BossAttackType.Charge;
                    break;
                    
                case BossPhase.Phase3:
                    // 第三阶段：地震、激光、召唤
                    if (Boss.CanEarthquake && Random.value < 0.4f)
                        return BossAttackType.Earthquake;
                    if (Boss.CanLaser && distanceToTarget > 6f)
                        return BossAttackType.LaserSweep;
                    if (Boss.CanSummon)
                        return BossAttackType.Summon;
                    if (Boss.CanCharge)
                        return BossAttackType.Charge;
                    break;
            }
            
            // 默认返回冲撞
            return BossAttackType.Charge;
        }
        
        private void ExecuteSpecialAttack()
        {
            _isExecuting = true;
            
            switch (_selectedAttack)
            {
                case BossAttackType.Charge:
                    Boss.PerformCharge();
                    Boss.StartCoroutine(WaitForAction(2f));
                    break;
                    
                case BossAttackType.LaserSweep:
                    Boss.PerformLaserSweep();
                    Boss.StartCoroutine(WaitForAction(4f));
                    break;
                    
                case BossAttackType.Summon:
                    Boss.PerformSummon();
                    Boss.StartCoroutine(WaitForAction(3f));
                    break;
                    
                case BossAttackType.Earthquake:
                    Boss.PerformEarthquake();
                    Boss.StartCoroutine(WaitForAction(5f));
                    break;
                    
                default:
                    _isExecuting = false;
                    break;
            }
        }
        
        private System.Collections.IEnumerator WaitForAction(float duration)
        {
            yield return new WaitForSeconds(duration);
            _isExecuting = false;
        }
        
        public override void OnExit()
        {
            _isExecuting = false;
        }
    }

    #endregion

    #region 死亡状态

    /// <summary>
    /// Boss死亡状态
    /// </summary>
    public class BossDeadState : BossStateBase
    {
        private float _deadTimer = 0f;
        private bool _deathProcessed = false;
        
        public BossDeadState(IronClawBeastBoss boss) : base(boss) { }
        
        public override void OnEnter()
        {
            _deadTimer = 0f;
            _deathProcessed = false;
            
            // 播放死亡动画
            Boss.GetComponent<Animator>()?.SetTrigger("Death");
            Boss.GetComponent<Animator>()?.SetBool("IsDead", true);
            
            // 禁用碰撞
            var collider = Boss.GetComponent<Collider2D>();
            if (collider != null) collider.enabled = false;
            
            // 停止物理
            var rb = Boss.GetComponent<Rigidbody2D>();
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
            if (_deadTimer > 2f && !_deathProcessed)
            {
                _deathProcessed = true;
                Boss.StartCoroutine(FadeOutCoroutine());
            }
        }
        
        private System.Collections.IEnumerator FadeOutCoroutine()
        {
            var sprite = Boss.GetComponent<SpriteRenderer>();
            if (sprite == null) yield break;
            
            float fadeDuration = 3f;
            float timer = 0f;
            Color originalColor = sprite.color;
            
            while (timer < fadeDuration)
            {
                float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                sprite.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                timer += Time.deltaTime;
                yield return null;
            }
            
            // 销毁Boss对象
            Destroy(Boss.gameObject);
        }
        
        public override void OnExit()
        {
        }
    }

    #endregion
}
