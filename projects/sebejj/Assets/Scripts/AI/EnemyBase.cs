/** 
 * @file EnemyBase.cs
 * @brief 敌人基类 - 任务E1-001, E2-001, E3-001
 * @description 所有敌人的基础类，提供通用属性和功能
 * @author AI系统架构师
 * @date 2026-02-27
 */

using System;
using UnityEngine;
using SebeJJ.AI;
using SebeJJ.AI.Pathfinding;

namespace SebeJJ.Enemies
{
    /// <summary>
    /// 敌人类型
    /// </summary>
    public enum EnemyType
    {
        MechFish,       // 机械鱼
        MechCrab,       // 机械蟹
        MechJellyfish,  // 机械水母
        Boss            // Boss
    }

    /// <summary>
    /// 敌人属性配置
    /// </summary>
    [System.Serializable]
    public class EnemyStats
    {
        [Header("基础属性")]
        public float maxHealth = 100f;
        public float moveSpeed = 2f;
        public float rotationSpeed = 360f;
        public float attackDamage = 10f;
        public float attackCooldown = 1f;
        public float attackRange = 1.5f;
        
        [Header("AI属性")]
        public float detectionRange = 10f;
        public float loseTargetRange = 15f;
        public float patrolRadius = 5f;
        public float patrolWaitTime = 2f;
        
        [Header("特殊属性")]
        public float defenseReduction = 0f;     // 防御减伤率
        public float critChance = 0.05f;        // 暴击率
        public float critDamage = 1.5f;         // 暴击伤害倍率
    }

    /// <summary>
    /// 敌人基类
    /// </summary>
    [RequireComponent(typeof(AIStateMachine))]
    [RequireComponent(typeof(AIPerception))]
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
        #region 事件定义
        
        /// <summary>
        /// 受到伤害事件
        /// </summary>
        public event Action<float> OnTakeDamage;
        
        /// <summary>
        /// 死亡事件
        /// </summary>
        public event Action OnDeath;
        
        /// <summary>
        /// 攻击事件
        /// </summary>
        public event Action OnAttack;
        
        /// <summary>
        /// 状态改变事件
        /// </summary>
        public event Action<EnemyState> OnStateChanged;
        
        #endregion

        #region 序列化字段
        
        [Header("基础配置")]
        [SerializeField] protected EnemyType enemyType;
        [SerializeField] protected EnemyStats stats;
        [SerializeField] protected bool showDebugInfo = true;
        
        [Header("组件引用")]
        [SerializeField] protected Animator animator;
        [SerializeField] protected Rigidbody2D rb;
        [SerializeField] protected SpriteRenderer spriteRenderer;
        
        [Header("受击设置")]
        [SerializeField] protected float hitStunDuration = 0.15f;    // 受击硬直时间
        [SerializeField] protected float hitKnockbackForce = 5f;     // 受击击退力度
        [SerializeField] protected bool enableHitReaction = true;    // 是否启用受击反应
        
        // 组件引用
        protected EnemyHitReaction hitReaction;
        protected CombatWarningSystem warningSystem;
        
        #endregion

        #region 私有字段
        
        /// <summary>
        /// 当前生命值
        /// </summary>
        protected float _currentHealth;
        
        /// <summary>
        /// 是否已死亡
        /// </summary>
        protected bool _isDead = false;
        
        /// <summary>
        /// 上次攻击时间
        /// </summary>
        protected float _lastAttackTime = -999f;
        
        /// <summary>
        /// 是否处于无敌状态
        /// </summary>
        protected bool _isInvincible = false;
        
        /// <summary>
        /// 无敌结束时间
        /// </summary>
        protected float _invincibleEndTime = 0f;
        
        /// <summary>
        /// 受击反应组件
        /// </summary>
        protected EnemyHitReaction _hitReaction;
        
        #endregion

        #region 公共属性
        
        /// <summary>
        /// AI状态机
        /// </summary>
        public AIStateMachine StateMachine { get; private set; }
        
        /// <summary>
        /// AI感知系统
        /// </summary>
        public AIPerception Perception { get; private set; }
        
        /// <summary>
        /// 敌人类型
        /// </summary>
        public EnemyType Type => enemyType;
        
        /// <summary>
        /// 当前生命值
        /// </summary>
        public float CurrentHealth => _currentHealth;
        
        /// <summary>
        /// 最大生命值
        /// </summary>
        public float MaxHealth => stats.maxHealth;
        
        /// <summary>
        /// 生命值百分比
        /// </summary>
        public float HealthPercent => _currentHealth / stats.maxHealth;
        
        /// <summary>
        /// 是否已死亡
        /// </summary>
        public bool IsDead => _isDead;
        
        /// <summary>
        /// 是否可以攻击
        /// </summary>
        public bool CanAttack => Time.time >= _lastAttackTime + stats.attackCooldown;
        
        /// <summary>
        /// 出生点位置
        /// </summary>
        public Vector3 SpawnPosition => spawnPosition;
        
        /// <summary>
        /// 当前目标
        /// </summary>
        public Transform CurrentTarget => Perception?.PrimaryTarget?.Target;
        
        /// <summary>
        /// 目标位置
        /// </summary>
        public Vector3? TargetPosition => Perception?.PrimaryTarget?.Position;
        
        #endregion

        #region Unity生命周期
        
        protected virtual void Awake()
        {
            // 获取组件
            StateMachine = GetComponent<AIStateMachine>();
            Perception = GetComponent<AIPerception>();
            
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (animator == null) animator = GetComponent<Animator>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            
            // 记录出生点
            spawnPosition = transform.position;
            
            // 初始化生命值
            _currentHealth = stats.maxHealth;
            
            // 订阅事件
            if (StateMachine != null)
            {
                StateMachine.OnStateChanged += HandleStateChanged;
            }
            
            // 初始化受击反应
            if (enableHitReaction)
            {
                _hitReaction = GetComponent<EnemyHitReaction>();
                if (_hitReaction == null)
                {
                    _hitReaction = gameObject.AddComponent<EnemyHitReaction>();
                }
            }
        }
        
        protected virtual void Start()
        {
            InitializeStates();
        }
        
        protected virtual void Update()
        {
            if (_isDead) return;
            
            // 更新无敌状态
            if (_isInvincible && Time.time >= _invincibleEndTime)
            {
                _isInvincible = false;
            }
            
            // 更新朝向
            UpdateFacing();
        }
        
        protected virtual void OnDestroy()
        {
            if (StateMachine != null)
            {
                StateMachine.OnStateChanged -= HandleStateChanged;
            }
        }
        
        #endregion

        #region 状态初始化
        
        /// <summary>
        /// 初始化AI状态 - 子类必须实现
        /// </summary>
        protected abstract void InitializeStates();
        
        #endregion

        #region 移动控制
        
        /// <summary>
        /// 移动到指定位置
        /// </summary>
        /// <param name="targetPosition">目标位置</param>
        /// <param name="stopDistance">停止距离</param>
        /// <returns>是否到达目标</returns>
        public virtual bool MoveTo(Vector3 targetPosition, float stopDistance = 0.1f)
        {
            if (_isDead) return false;
            
            Vector3 direction = targetPosition - transform.position;
            float distance = direction.magnitude;
            
            if (distance <= stopDistance)
            {
                return true;
            }
            
            direction.Normalize();
            
            // 移动
            Vector3 newPosition = transform.position + direction * stats.moveSpeed * Time.deltaTime;
            
            if (rb != null)
            {
                rb.MovePosition(newPosition);
            }
            else
            {
                transform.position = newPosition;
            }
            
            // 更新动画
            SetAnimationFloat("MoveSpeed", 1f);
            
            return false;
        }
        
        /// <summary>
        /// 平滑转向
        /// </summary>
        /// <param name="targetDirection">目标方向</param>
        protected virtual void SmoothRotate(Vector3 targetDirection)
        {
            if (targetDirection == Vector3.zero) return;
            
            float targetAngle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
            float currentAngle = transform.eulerAngles.z;
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, stats.rotationSpeed * Time.deltaTime);
            
            transform.rotation = Quaternion.Euler(0, 0, newAngle);
        }
        
        /// <summary>
        /// 更新朝向
        /// </summary>
        protected virtual void UpdateFacing()
        {
            if (CurrentTarget != null)
            {
                Vector3 direction = CurrentTarget.position - transform.position;
                if (direction.x != 0)
                {
                    // 翻转精灵
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.flipX = direction.x < 0;
                    }
                }
            }
        }
        
        #endregion

        #region 攻击控制
        
        /// <summary>
        /// 执行攻击 - 子类实现具体攻击逻辑
        /// </summary>
        public abstract void PerformAttack();
        
        /// <summary>
        /// 检查是否在攻击范围内
        /// </summary>
        /// <returns>是否在范围内</returns>
        public virtual bool IsInAttackRange()
        {
            if (CurrentTarget == null) return false;
            
            float distance = Vector3.Distance(transform.position, CurrentTarget.position);
            return distance <= stats.attackRange;
        }
        
        /// <summary>
        /// 记录攻击时间
        /// </summary>
        protected void RecordAttack()
        {
            _lastAttackTime = Time.time;
            OnAttack?.Invoke();
        }
        
        #endregion

        #region 伤害与死亡
        
        /// <summary>
        /// 受到伤害
        /// </summary>
        /// <param name="damage">伤害值</param>
        /// <param name="damageSource">伤害来源</param>
        public virtual void TakeDamage(float damage, Transform damageSource = null)
        {
            if (_isDead || _isInvincible) return;
            
            // 应用防御减伤
            if (stats.defenseReduction > 0)
            {
                damage *= (1f - stats.defenseReduction);
            }
            
            _currentHealth -= damage;
            _currentHealth = Mathf.Max(0, _currentHealth);
            
            OnTakeDamage?.Invoke(damage);
            
            // 触发受击动画
            SetAnimationTrigger("Hit");
            
            // 触发受击反应
            if (_hitReaction != null && damageSource != null)
            {
                Vector2 hitDirection = (transform.position - damageSource.position).normalized;
                _hitReaction.ProcessHitReaction(damage, hitDirection, hitKnockbackForce, hitStunDuration);
            }
            
            // 短暂无敌
            StartInvincibility(0.1f);
            
            // 检查死亡
            if (_currentHealth <= 0)
            {
                Die();
            }
            else
            {
                // 感知到攻击者
                if (damageSource != null && Perception != null)
                {
                    Perception.ForceSetTarget(damageSource);
                }
            }
        }
        
        /// <summary>
        /// 开始无敌状态
        /// </summary>
        /// <param name="duration">持续时间</param>
        public void StartInvincibility(float duration)
        {
            _isInvincible = true;
            _invincibleEndTime = Time.time + duration;
        }
        
        /// <summary>
        /// 死亡
        /// </summary>
        protected virtual void Die()
        {
            if (_isDead) return;
            
            _isDead = true;
            _currentHealth = 0;
            
            // 切换到死亡状态
            StateMachine?.ChangeState(EnemyState.Dead, true);
            
            // 触发死亡动画
            SetAnimationTrigger("Death");
            
            OnDeath?.Invoke();
            
            // 延迟销毁
            Destroy(gameObject, 3f);
        }
        
        /// <summary>
        /// 恢复生命值
        /// </summary>
        /// <param name="amount">恢复量</param>
        public virtual void Heal(float amount)
        {
            if (_isDead) return;
            
            _currentHealth += amount;
            _currentHealth = Mathf.Min(_currentHealth, stats.maxHealth);
        }
        
        #endregion

        #region 动画控制
        
        /// <summary>
        /// 设置动画触发器
        /// </summary>
        /// <param name="triggerName">触发器名称</param>
        protected void SetAnimationTrigger(string triggerName)
        {
            animator?.SetTrigger(triggerName);
        }
        
        /// <summary>
        /// 设置动画布尔值
        /// </summary>
        /// <param name="paramName">参数名称</param>
        /// <param name="value">值</param>
        protected void SetAnimationBool(string paramName, bool value)
        {
            animator?.SetBool(paramName, value);
        }
        
        /// <summary>
        /// 设置动画浮点值
        /// </summary>
        /// <param name="paramName">参数名称</param>
        /// <param name="value">值</param>
        protected void SetAnimationFloat(string paramName, float value)
        {
            animator?.SetFloat(paramName, value);
        }
        
        #endregion

        #region 事件处理
        
        /// <summary>
        /// 处理状态改变事件
        /// </summary>
        /// <param name="from">源状态</param>
        /// <param name="to">目标状态</param>
        protected virtual void HandleStateChanged(EnemyState from, EnemyState to)
        {
            OnStateChanged?.Invoke(to);
            
            if (showDebugInfo)
            {
                Debug.Log($"[{enemyType}] 状态切换: {from} -> {to}");
            }
        }
        
        #endregion

        #region 调试
        
        protected virtual void OnDrawGizmos()
        {
            if (!showDebugInfo) return;
            
            // 绘制攻击范围
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, stats.attackRange);
            
            // 绘制检测范围
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, stats.detectionRange);
            
            // 绘制巡逻范围
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(spawnPosition, stats.patrolRadius);
        }
        
        #endregion
    }

    /// <summary>
    /// 可受伤接口
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(float damage, Transform damageSource = null);
        float CurrentHealth { get; }
        float MaxHealth { get; }
        bool IsDead { get; }
    }
}
