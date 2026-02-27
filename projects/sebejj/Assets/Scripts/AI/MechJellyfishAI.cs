/** 
 * @file MechJellyfishAI.cs
 * @brief 机械水母AI - 任务E3-001~004
 * @description 实现机械水母的漂浮、电击AOE攻击行为
 * @author AI系统架构师
 * @date 2026-02-27
 */

using UnityEngine;
using SebeJJ.AI;
using System.Collections.Generic;

namespace SebeJJ.Enemies
{
    /// <summary>
    /// 机械水母AI - 范围攻击型敌人
    /// 行为特点：
    /// - Idle: 上下漂浮
    /// - Float: 缓慢移动，保持漂浮动画
    /// - Charge: 蓄力准备电击
    /// - Attack: 释放电击AOE
    /// </summary>
    public class MechJellyfishAI : EnemyBase
    {
        #region 序列化字段
        
        [Header("漂浮配置")]
        [SerializeField] private float floatAmplitude = 0.5f;
        [SerializeField] private float floatFrequency = 1f;
        [SerializeField] private float floatSpeed = 1f;
        [SerializeField] private float driftRadius = 5f;
        [SerializeField] private float driftSpeed = 0.5f;
        
        [Header("电击攻击")]
        [SerializeField] private float pulseRadius = 4f;
        [SerializeField] private float pulseDamage = 15f;
        [SerializeField] private float pulseChargeTime = 1.5f;
        [SerializeField] private float pulseCooldown = 4f;
        [SerializeField] private int pulseTicks = 3;
        [SerializeField] private float pulseTickInterval = 0.3f;
        [SerializeField] private AnimationCurve pulseDamageCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("预警配置")]
        [SerializeField] private float warningDuration = 0.5f;
        [SerializeField] private Color warningColor = Color.yellow;
        [SerializeField] private Color pulseColor = Color.cyan;
        
        [Header("特效")]
        [SerializeField] private ParticleSystem floatEffect;
        [SerializeField] private ParticleSystem chargeEffect;
        [SerializeField] private ParticleSystem pulseEffect;
        [SerializeField] private GameObject pulseRangeIndicator;
        
        #endregion

        #region 私有字段
        
        /// <summary>
        /// 漂浮基准高度
        /// </summary>
        private float _baseHeight;
        
        /// <summary>
        /// 漂浮计时器
        /// </summary>
        private float _floatTimer = 0f;
        
        /// <summary>
        /// 漂移目标点
        /// </summary>
        private Vector3 _driftTarget;
        
        /// <summary>
        /// 是否正在蓄力
        /// </summary>
        private bool _isCharging = false;
        
        /// <summary>
        /// 蓄力计时器
        /// </summary>
        private float _chargeTimer = 0f;
        
        /// <summary>
        /// 上次电击时间
        /// </summary>
        private float _lastPulseTime = -999f;
        
        /// <summary>
        /// 当前电击次数
        /// </summary>
        private int _currentPulseTick = 0;
        
        /// <summary>
        /// 电击间隔计时器
        /// </summary>
        private float _pulseTickTimer = 0f;
        
        /// <summary>
        /// 原始颜色
        /// </summary>
        private Color _originalColor;
        
        /// <summary>
        /// 受影响的目标列表
        /// </summary>
        private HashSet<Transform> _affectedTargets = new HashSet<Transform>();
        
        #endregion

        #region 公共属性
        
        /// <summary>
        /// 是否可以释放电击
        /// </summary>
        public bool CanPulse => Time.time >= _lastPulseTime + pulseCooldown;
        
        /// <summary>
        /// 是否正在蓄力
        /// </summary>
        public bool IsCharging => _isCharging;
        
        /// <summary>
        /// 蓄力进度 (0-1)
        /// </summary>
        public float ChargeProgress => _isCharging ? _chargeTimer / pulseChargeTime : 0f;
        
        #endregion

        #region Unity生命周期
        
        protected override void Awake()
        {
            base.Awake();
            enemyType = EnemyType.MechJellyfish;
            
            // 记录基准高度
            _baseHeight = transform.position.y;
            
            // 初始化漂移目标
            _driftTarget = GetRandomDriftPoint();
            
            // 保存原始颜色
            if (spriteRenderer != null)
            {
                _originalColor = spriteRenderer.color;
            }
        }
        
        protected override void Update()
        {
            base.Update();
            
            if (_isDead) return;
            
            // 更新漂浮
            UpdateFloating();
            
            // 更新蓄力
            if (_isCharging)
            {
                UpdateCharging();
            }
        }
        
        #endregion

        #region 状态初始化
        
        protected override void InitializeStates()
        {
            // 注册状态
            StateMachine.RegisterState(EnemyState.Idle, new MechJellyfishIdleState(this));
            StateMachine.RegisterState(EnemyState.Patrol, new MechJellyfishFloatState(this));
            StateMachine.RegisterState(EnemyState.Alert, new MechJellyfishAlertState(this));
            StateMachine.RegisterState(EnemyState.Attack, new MechJellyfishAttackState(this));
            StateMachine.RegisterState(EnemyState.Dead, new MechJellyfishDeadState(this));
        }
        
        #endregion

        #region 漂浮行为
        
        /// <summary>
        /// 更新漂浮
        /// </summary>
        private void UpdateFloating()
        {
            _floatTimer += Time.deltaTime;
            
            // 计算漂浮偏移
            float floatOffset = Mathf.Sin(_floatTimer * floatFrequency * Mathf.PI * 2f) * floatAmplitude;
            
            // 应用漂浮
            Vector3 position = transform.position;
            position.y = _baseHeight + floatOffset;
            transform.position = position;
        }
        
        /// <summary>
        /// 更新漂移
        /// </summary>
        public void UpdateDrift()
        {
            // 向漂移目标缓慢移动
            Vector3 direction = _driftTarget - transform.position;
            direction.y = 0; // 保持水平漂移
            float distance = direction.magnitude;
            
            if (distance < 0.5f)
            {
                // 到达目标，选择新目标
                _driftTarget = GetRandomDriftPoint();
            }
            else
            {
                direction.Normalize();
                Vector3 newPosition = transform.position + direction * driftSpeed * Time.deltaTime;
                
                // 保持基准高度
                _baseHeight = newPosition.y;
                
                if (rb != null)
                {
                    rb.MovePosition(newPosition);
                }
                else
                {
                    transform.position = newPosition;
                }
            }
        }
        
        /// <summary>
        /// 获取随机漂移点
        /// </summary>
        public Vector3 GetRandomDriftPoint()
        {
            Vector2 randomCircle = Random.insideUnitCircle * driftRadius;
            return SpawnPosition + new Vector3(randomCircle.x, randomCircle.y, 0);
        }
        
        #endregion

        #region 电击攻击
        
        /// <summary>
        /// 开始蓄力
        /// </summary>
        public void StartCharge()
        {
            if (_isCharging || !CanPulse) return;
            
            _isCharging = true;
            _chargeTimer = 0f;
            _currentPulseTick = 0;
            _affectedTargets.Clear();
            
            // 播放特效
            chargeEffect?.Play();
            
            // 显示范围指示器
            if (pulseRangeIndicator != null)
            {
                pulseRangeIndicator.SetActive(true);
                pulseRangeIndicator.transform.localScale = Vector3.one * pulseRadius * 2f;
            }
            
            // 改变颜色为预警色
            if (spriteRenderer != null)
            {
                spriteRenderer.color = warningColor;
            }
            
            // 设置动画
            SetAnimationBool("IsCharging", true);
            SetAnimationTrigger("ChargeStart");
        }
        
        /// <summary>
        /// 更新蓄力
        /// </summary>
        private void UpdateCharging()
        {
            _chargeTimer += Time.deltaTime;
            
            // 脉冲闪烁效果
            float flashProgress = _chargeTimer / pulseChargeTime;
            if (spriteRenderer != null)
            {
                Color flashColor = Color.Lerp(warningColor, pulseColor, flashProgress);
                float flashIntensity = 1f + Mathf.Sin(flashProgress * Mathf.PI * 10f) * 0.3f;
                spriteRenderer.color = flashColor * flashIntensity;
            }
            
            // 蓄力完成，释放电击
            if (_chargeTimer >= pulseChargeTime)
            {
                ReleasePulse();
            }
        }
        
        /// <summary>
        /// 释放电击脉冲
        /// </summary>
        private void ReleasePulse()
        {
            // 播放特效
            pulseEffect?.Play();
            
            // 范围伤害
            ApplyPulseDamage();
            
            _currentPulseTick++;
            
            // 检查是否还有剩余脉冲次数
            if (_currentPulseTick < pulseTicks)
            {
                // 继续下一次脉冲
                _chargeTimer = pulseChargeTime - pulseTickInterval;
            }
            else
            {
                // 结束蓄力
                EndCharge();
            }
        }
        
        /// <summary>
        /// 应用电击伤害
        /// </summary>
        private void ApplyPulseDamage()
        {
            // 获取范围内的所有目标
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pulseRadius);
            
            foreach (var hit in hits)
            {
                if (hit.transform == transform) continue;
                
                // 计算距离衰减
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                float damageMultiplier = pulseDamageCurve.Evaluate(1f - distance / pulseRadius);
                float finalDamage = pulseDamage * damageMultiplier;
                
                // 应用伤害
                var damageable = hit.transform.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(finalDamage, transform);
                    
                    // 添加减速效果（可选）
                    ApplySlowEffect(hit.transform);
                }
                
                _affectedTargets.Add(hit.transform);
            }
            
            // 屏幕震动效果
            // CameraShake.Instance?.Shake(0.2f, 0.1f);
        }
        
        /// <summary>
        /// 应用减速效果
        /// </summary>
        private void ApplySlowEffect(Transform target)
        {
            // 可以在这里添加减速debuff
            // 例如：target.GetComponent<PlayerController>()?.ApplySlow(0.5f, 2f);
        }
        
        /// <summary>
        /// 结束蓄力
        /// </summary>
        private void EndCharge()
        {
            _isCharging = false;
            _chargeTimer = 0f;
            _lastPulseTime = Time.time;
            
            // 停止特效
            chargeEffect?.Stop();
            
            // 隐藏范围指示器
            if (pulseRangeIndicator != null)
            {
                pulseRangeIndicator.SetActive(false);
            }
            
            // 恢复颜色
            if (spriteRenderer != null)
            {
                spriteRenderer.color = _originalColor;
            }
            
            // 设置动画
            SetAnimationBool("IsCharging", false);
            SetAnimationTrigger("ChargeEnd");
            
            // 返回漂浮状态
            if (!IsDead)
            {
                StateMachine.ChangeState(EnemyState.Patrol);
            }
        }
        
        /// <summary>
        /// 执行攻击（电击）
        /// </summary>
        public override void PerformAttack()
        {
            StartCharge();
        }
        
        /// <summary>
        /// 检查目标是否在电击范围内
        /// </summary>
        /// <returns>是否在范围内</returns>
        public bool IsTargetInPulseRange()
        {
            if (CurrentTarget == null) return false;
            
            float distance = Vector3.Distance(transform.position, CurrentTarget.position);
            return distance <= pulseRadius;
        }
        
        #endregion

        #region 调试
        
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            
            // 绘制漂移范围
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(SpawnPosition, driftRadius);
            
            // 绘制电击范围
            Gizmos.color = _isCharging ? Color.yellow : Color.cyan;
            Gizmos.DrawWireSphere(transform.position, pulseRadius);
            
            // 绘制当前漂移目标
            if (Application.isPlaying)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(_driftTarget, 0.3f);
                Gizmos.DrawLine(transform.position, _driftTarget);
            }
        }
        
        #endregion
    }

    #region 机械水母状态类

    /// <summary>
    /// 机械水母待机状态
    /// </summary>
    public class MechJellyfishIdleState : AIStateBase
    {
        private MechJellyfishAI _jellyfish;
        private float _timer = 0f;
        
        public MechJellyfishIdleState(MechJellyfishAI jellyfish)
        {
            _jellyfish = jellyfish;
        }
        
        public override void OnEnter()
        {
            _timer = 0f;
            _jellyfish.SetAnimationBool("IsFloating", true);
        }
        
        public override void OnUpdate(float deltaTime)
        {
            _timer += deltaTime;
            
            // 检测玩家
            if (_jellyfish.Perception.HasTarget)
            {
                ChangeState(EnemyState.Alert);
                return;
            }
            
            // 待机一段时间后开始漂浮
            if (_timer >= 2f)
            {
                ChangeState(EnemyState.Patrol);
            }
        }
        
        public override void OnExit()
        {
        }
    }

    /// <summary>
    /// 机械水母漂浮状态
    /// </summary>
    public class MechJellyfishFloatState : AIStateBase
    {
        private MechJellyfishAI _jellyfish;
        
        public MechJellyfishFloatState(MechJellyfishAI jellyfish)
        {
            _jellyfish = jellyfish;
        }
        
        public override void OnEnter()
        {
            _jellyfish.SetAnimationBool("IsFloating", true);
        }
        
        public override void OnUpdate(float deltaTime)
        {
            // 检测玩家
            if (_jellyfish.Perception.HasTarget)
            {
                // 检查目标是否在电击范围内
                if (_jellyfish.IsTargetInPulseRange() && _jellyfish.CanPulse)
                {
                    ChangeState(EnemyState.Attack);
                    return;
                }
                
                ChangeState(EnemyState.Alert);
                return;
            }
            
            // 执行漂移
            _jellyfish.UpdateDrift();
        }
        
        public override void OnExit()
        {
            _jellyfish.SetAnimationBool("IsFloating", false);
        }
    }

    /// <summary>
    /// 机械水母警戒状态
    /// </summary>
    public class MechJellyfishAlertState : AIStateBase
    {
        private MechJellyfishAI _jellyfish;
        private float _alertTimer = 0f;
        private float _maxAlertTime = 3f;
        
        public MechJellyfishAlertState(MechJellyfishAI jellyfish)
        {
            _jellyfish = jellyfish;
        }
        
        public override void OnEnter()
        {
            _alertTimer = 0f;
            _jellyfish.SetAnimationBool("IsAlert", true);
        }
        
        public override void OnUpdate(float deltaTime)
        {
            _alertTimer += deltaTime;
            
            // 检查目标是否在电击范围内且可以释放
            if (_jellyfish.IsTargetInPulseRange() && _jellyfish.CanPulse)
            {
                ChangeState(EnemyState.Attack);
                return;
            }
            
            // 丢失目标后返回漂浮
            if (!_jellyfish.Perception.HasTarget && _alertTimer >= _maxAlertTime)
            {
                ChangeState(EnemyState.Patrol);
                return;
            }
            
            // 缓慢向目标漂移
            if (_jellyfish.CurrentTarget != null)
            {
                Vector3 direction = _jellyfish.CurrentTarget.position - _jellyfish.transform.position;
                direction.Normalize();
                _jellyfish.transform.position += direction * _jellyfish.stats.moveSpeed * 0.5f * deltaTime;
            }
        }
        
        public override void OnExit()
        {
            _jellyfish.SetAnimationBool("IsAlert", false);
        }
    }

    /// <summary>
    /// 机械水母攻击状态
    /// </summary>
    public class MechJellyfishAttackState : AIStateBase
    {
        private MechJellyfishAI _jellyfish;
        
        public MechJellyfishAttackState(MechJellyfishAI jellyfish)
        {
            _jellyfish = jellyfish;
        }
        
        public override void OnEnter()
        {
            _jellyfish.PerformAttack();
        }
        
        public override void OnUpdate(float deltaTime)
        {
            // 蓄力完成后会自动切换状态
            if (!_jellyfish.IsCharging)
            {
                if (_jellyfish.Perception.HasTarget)
                {
                    ChangeState(EnemyState.Alert);
                }
                else
                {
                    ChangeState(EnemyState.Patrol);
                }
            }
        }
        
        public override void OnExit()
        {
        }
    }

    /// <summary>
    /// 机械水母死亡状态
    /// </summary>
    public class MechJellyfishDeadState : AIStateBase
    {
        private MechJellyfishAI _jellyfish;
        private float _deadTimer = 0f;
        private float _sinkSpeed = 0.5f;
        
        public MechJellyfishDeadState(MechJellyfishAI jellyfish)
        {
            _jellyfish = jellyfish;
        }
        
        public override void OnEnter()
        {
            _jellyfish.SetAnimationBool("IsDead", true);
            
            // 禁用碰撞
            var collider = _jellyfish.GetComponent<Collider2D>();
            if (collider != null) collider.enabled = false;
            
            // 禁用物理
            var rb = _jellyfish.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.isKinematic = true;
            }
            
            // 如果正在蓄力，强制结束
            if (_jellyfish.IsCharging)
            {
                // 通过反射或直接调用结束蓄力
            }
        }
        
        public override void OnUpdate(float deltaTime)
        {
            _deadTimer += deltaTime;
            
            // 缓慢下沉
            Vector3 position = _jellyfish.transform.position;
            position.y -= _sinkSpeed * deltaTime;
            _jellyfish.transform.position = position;
            
            // 淡出效果
            if (_deadTimer > 1f)
            {
                var sprite = _jellyfish.GetComponent<SpriteRenderer>();
                if (sprite != null)
                {
                    Color color = sprite.color;
                    color.a = Mathf.Lerp(1f, 0f, (_deadTimer - 1f) / 2f);
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
