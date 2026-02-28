/**
 * @file IronClawBeastBoss.cs
 * @brief 铁钳巨兽Boss - 三阶段Boss战
 * @description 赛博机甲SebeJJ的最终Boss，具有三阶段战斗系统
 * @author Boss战设计师
 * @date 2026-02-27
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SebeJJ.AI;
using SebeJJ.Utils;

namespace SebeJJ.Boss
{
    /// <summary>
    /// Boss阶段
    /// </summary>
    public enum BossPhase
    {
        Phase1,     // 第一阶段：100%-60%血量
        Phase2,     // 第二阶段：60%-30%血量
        Phase3,     // 第三阶段：30%以下
        Defeated    // 被击败
    }

    /// <summary>
    /// Boss攻击类型
    /// </summary>
    public enum BossAttackType
    {
        ClawCombo,      // 钳击连击
        Charge,         // 冲撞攻击
        Defend,         // 防御姿态
        LaserSweep,     // 激光扫射
        Summon,         // 召唤机械蟹
        Earthquake      // 全屏地震
    }

    /// <summary>
    /// 铁钳巨兽Boss主类
    /// </summary>
    [RequireComponent(typeof(AIStateMachine))]
    [RequireComponent(typeof(AIPerception))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class IronClawBeastBoss : MonoBehaviour, IDamageable
    {
        #region 事件定义

        public event Action<BossPhase> OnPhaseChanged;
        public event Action<float> OnHealthChanged;
        public event Action<float> OnTakeDamage;
        public event Action OnDefeated;
        public event Action<BossAttackType> OnAttackStarted;
        public event Action OnWeakPointExposed;

        #endregion

        #region 序列化字段 - 基础属性

        [Header("=== 基础属性 ===")]
        [SerializeField] private float maxHealth = 5000f;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float rotationSpeed = 180f;
        [SerializeField] private float attackDamage = 50f;
        
        [Header("=== 阶段血量阈值 ===")]
        [SerializeField] private float phase2Threshold = 0.6f;  // 60%
        [SerializeField] private float phase3Threshold = 0.3f;  // 30%
        
        [Header("=== 阶段转换 ===")]
        [SerializeField] private float phaseTransitionDuration = 3f;
        [SerializeField] private float phaseTransitionInvincibleTime = 5f;
        
        #endregion

        #region 序列化字段 - 第一阶段

        [Header("=== 第一阶段：钳击连击 ===")]
        [SerializeField] private int clawComboCount = 3;
        [SerializeField] private float clawComboInterval = 0.5f;
        [SerializeField] private float clawAttackRange = 3f;
        [SerializeField] private float clawAttackAngle = 90f;
        [SerializeField] private float clawDamage = 40f;
        [SerializeField] private bool clawCanBeBlocked = true;
        
        [Header("=== 第一阶段：冲撞攻击 ===")]
        [SerializeField] private float chargeSpeed = 12f;
        [SerializeField] private float chargeDamage = 60f;
        [SerializeField] private float chargeCooldown = 8f;
        [SerializeField] private float chargeWarningTime = 1.5f;
        [SerializeField] private float chargeDistance = 10f;
        [SerializeField] private LayerMask chargeObstacleLayers;
        
        [Header("=== 第一阶段：防御姿态 ===")]
        [SerializeField] private float defendDuration = 4f;
        [SerializeField] private float defendCooldown = 10f;
        [SerializeField] private float defendDamageReduction = 0.5f;
        [SerializeField] private float defendBreakStunDuration = 2f;

        #endregion

        #region 序列化字段 - 第二阶段

        [Header("=== 第二阶段：属性提升 ===")]
        [SerializeField] private float phase2AttackSpeedMultiplier = 1.3f;
        [SerializeField] private float phase2MoveSpeedMultiplier = 1.2f;
        
        [Header("=== 第二阶段：激光扫射 ===")]
        [SerializeField] private float laserSweepDuration = 3f;
        [SerializeField] private float laserSweepDamage = 30f;
        [SerializeField] private float laserSweepCooldown = 12f;
        [SerializeField] private float laserSweepAngle = 120f;
        [SerializeField] private float laserMaxRange = 15f;
        [SerializeField] private float laserRotationSpeed = 60f;
        
        [Header("=== 第二阶段：召唤机械蟹 ===")]
        [SerializeField] private GameObject mechCrabPrefab;
        [SerializeField] private int summonCount = 2;
        [SerializeField] private float summonCooldown = 20f;
        [SerializeField] private Transform[] summonPoints;
        [SerializeField] private float summonDuration = 2f;

        #endregion

        #region 序列化字段 - 第三阶段

        [Header("=== 第三阶段：狂暴模式 ===")]
        [SerializeField] private float phase3AttackMultiplier = 1.5f;
        [SerializeField] private float phase3MoveSpeedMultiplier = 1.3f;
        [SerializeField] private float phase3AttackSpeedMultiplier = 1.5f;
        
        [Header("=== 第三阶段：全屏地震 ===")]
        [SerializeField] private float earthquakeDuration = 4f;
        [SerializeField] private float earthquakeDamage = 100f;
        [SerializeField] private float earthquakeCooldown = 25f;
        [SerializeField] private float earthquakeWarningTime = 2f;
        [SerializeField] private float earthquakeJumpWindow = 0.5f;
        [SerializeField] private float earthquakeWaveInterval = 0.8f;
        
        [Header("=== 第三阶段：背部核心弱点 ===")]
        [SerializeField] private Transform weakPointTransform;
        [SerializeField] private float weakPointDamageMultiplier = 3f;
        [SerializeField] private float weakPointAngle = 45f;
        [SerializeField] private float weakPointExposeDuration = 5f;
        [SerializeField] private float weakPointCooldown = 15f;

        #endregion

        #region 序列化字段 - 组件引用

        [Header("=== 组件引用 ===")]
        [SerializeField] private Animator animator;
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Collider2D bossCollider;
        
        [Header("=== 特效引用 ===")]
        [SerializeField] private ParticleSystem phaseTransitionEffect;
        [SerializeField] private ParticleSystem defendEffect;
        [SerializeField] private ParticleSystem laserEffect;
        [SerializeField] private ParticleSystem earthquakeEffect;
        [SerializeField] private ParticleSystem weakPointEffect;
        [SerializeField] private GameObject shieldVisual;
        [SerializeField] private GameObject rageVisual;
        
        [Header("=== 音频引用 ===")]
        [SerializeField] private AudioClip phaseTransitionSound;
        [SerializeField] private AudioClip clawAttackSound;
        [SerializeField] private AudioClip chargeSound;
        [SerializeField] private AudioClip laserSound;
        [SerializeField] private AudioClip summonSound;
        [SerializeField] private AudioClip earthquakeSound;
        [SerializeField] private AudioClip weakPointSound;
        [SerializeField] private AudioClip defeatSound;

        #endregion

        #region 私有字段

        // 基础状态
        private float _currentHealth;
        private BossPhase _currentPhase = BossPhase.Phase1;
        private bool _isDead = false;
        private bool _isInvincible = false;
        private bool _isInPhaseTransition = false;
        
        // 组件
        private AIStateMachine _stateMachine;
        private AIPerception _perception;
        private BossHealthBar _healthBar;
        
        // 攻击计时器
        private float _lastAttackTime = -999f;
        private float _lastChargeTime = -999f;
        private float _lastDefendTime = -999f;
        private float _lastLaserTime = -999f;
        private float _lastSummonTime = -999f;
        private float _lastEarthquakeTime = -999f;
        private float _lastWeakPointTime = -999f;
        
        // 状态标志
        private bool _isDefending = false;
        private bool _isCharging = false;
        private bool _isUsingLaser = false;
        private bool _isUsingEarthquake = false;
        private bool _isWeakPointExposed = false;
        private bool _isEnraged = false;
        
        // 连击
        private int _currentCombo = 0;
        private float _comboTimer = 0f;
        
        // 召唤的小怪
        private List<GameObject> _summonedCrabs = new List<GameObject>();
        
        // 原始属性（用于阶段恢复）
        private float _baseMoveSpeed;
        private float _baseAttackDamage;
        private float _baseAttackCooldown;

        #endregion

        #region 公共属性

        public float CurrentHealth => _currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthPercent => _currentHealth / maxHealth;
        public BossPhase CurrentPhase => _currentPhase;
        public bool IsDead => _isDead;
        public bool IsInvincible => _isInvincible;
        public bool IsDefending => _isDefending;
        public bool IsEnraged => _isEnraged;
        public bool IsWeakPointExposed => _isWeakPointExposed;
        public Transform CurrentTarget => _perception?.PrimaryTarget?.Target;
        public int CurrentCombo => _currentCombo;
        
        // 攻击可用性
        public bool CanAttack => Time.time >= _lastAttackTime + GetCurrentAttackCooldown();
        public bool CanCharge => Time.time >= _lastChargeTime + chargeCooldown;
        public bool CanDefend => Time.time >= _lastDefendTime + defendCooldown && !_isDefending;
        public bool CanLaser => Time.time >= _lastLaserTime + laserSweepCooldown && _currentPhase >= BossPhase.Phase2;
        public bool CanSummon => Time.time >= _lastSummonTime + summonCooldown && _currentPhase >= BossPhase.Phase2;
        public bool CanEarthquake => Time.time >= _lastEarthquakeTime + earthquakeCooldown && _currentPhase >= BossPhase.Phase3;
        public bool CanExposeWeakPoint => Time.time >= _lastWeakPointTime + weakPointCooldown && _currentPhase >= BossPhase.Phase3;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            // 获取组件
            _stateMachine = GetComponent<AIStateMachine>();
            _perception = GetComponent<AIPerception>();
            
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            if (animator == null) animator = GetComponent<Animator>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (bossCollider == null) bossCollider = GetComponent<Collider2D>();
            
            // 初始化
            _currentHealth = maxHealth;
            _baseMoveSpeed = moveSpeed;
            _baseAttackDamage = attackDamage;
            
            // 查找或创建血条
            InitializeHealthBar();
        }

        private void Start()
        {
            InitializeBossStates();
            
            // 订阅事件
            if (_stateMachine != null)
            {
                _stateMachine.OnStateChanged += HandleStateChanged;
            }
            
            // 广播Boss出现事件
            GameEvents.TriggerNotification("警告：检测到巨型机械生命体！");
        }

        private void Update()
        {
            if (_isDead || _isInPhaseTransition) return;
            
            // 更新连击计时器
            if (_currentCombo > 0)
            {
                _comboTimer += Time.deltaTime;
                if (_comboTimer > 2f)
                {
                    ResetCombo();
                }
            }
            
            // 检查阶段转换
            CheckPhaseTransition();
            
            // 更新朝向
            UpdateFacing();
            
            // 更新血条
            UpdateHealthBar();
        }

        private void OnDestroy()
        {
            if (_stateMachine != null)
            {
                _stateMachine.OnStateChanged -= HandleStateChanged;
            }
        }

        #endregion

        #region 初始化

        private void InitializeHealthBar()
        {
            _healthBar = FindObjectOfType<BossHealthBar>();
            if (_healthBar == null)
            {
                // 创建血条UI
                var healthBarObj = new GameObject("BossHealthBar");
                _healthBar = healthBarObj.AddComponent<BossHealthBar>();
            }
            _healthBar.Initialize(this);
        }

        private void InitializeBossStates()
        {
            // 注册所有Boss状态
            _stateMachine.RegisterState(EnemyState.Idle, new BossIdleState(this));
            _stateMachine.RegisterState(EnemyState.Chase, new BossChaseState(this));
            _stateMachine.RegisterState(EnemyState.Attack, new BossAttackState(this));
            _stateMachine.RegisterState(EnemyState.Defend, new BossDefendState(this));
            _stateMachine.RegisterState(EnemyState.Special, new BossSpecialState(this));
            _stateMachine.RegisterState(EnemyState.Dead, new BossDeadState(this));
        }

        #endregion

        #region 阶段系统

        private void CheckPhaseTransition()
        {
            if (_isInPhaseTransition || _currentPhase == BossPhase.Defeated) return;
            
            float healthPercent = HealthPercent;
            
            // 检查进入第二阶段
            if (_currentPhase == BossPhase.Phase1 && healthPercent <= phase2Threshold)
            {
                StartPhaseTransition(BossPhase.Phase2);
            }
            // 检查进入第三阶段
            else if (_currentPhase == BossPhase.Phase2 && healthPercent <= phase3Threshold)
            {
                StartPhaseTransition(BossPhase.Phase3);
            }
        }

        private void StartPhaseTransition(BossPhase newPhase)
        {
            StartCoroutine(PhaseTransitionCoroutine(newPhase));
        }

        private IEnumerator PhaseTransitionCoroutine(BossPhase newPhase)
        {
            _isInPhaseTransition = true;
            _isInvincible = true;
            
            // 停止当前动作
            _stateMachine.ChangeState(EnemyState.Idle);
            
            // 播放阶段转换动画
            animator?.SetTrigger("PhaseTransition");
            
            // 播放特效
            phaseTransitionEffect?.Play();
            
            // 播放音效
            if (phaseTransitionSound != null)
            {
                AudioManager.Instance?.PlaySFX(phaseTransitionSound);
            }
            
            // 显示阶段提示
            string phaseName = newPhase == BossPhase.Phase2 ? "第二阶段" : "最终阶段";
            GameEvents.TriggerNotification($"铁钳巨兽进入{phaseName}！");
            
            // 等待转换动画
            yield return new WaitForSeconds(phaseTransitionDuration);
            
            // 应用阶段属性
            ApplyPhaseAttributes(newPhase);
            
            _currentPhase = newPhase;
            OnPhaseChanged?.Invoke(newPhase);
            
            // 保持无敌一段时间
            yield return new WaitForSeconds(phaseTransitionInvincibleTime - phaseTransitionDuration);
            
            _isInvincible = false;
            _isInPhaseTransition = false;
            
            // 恢复战斗
            _stateMachine.ChangeState(EnemyState.Chase);
        }

        private void ApplyPhaseAttributes(BossPhase phase)
        {
            switch (phase)
            {
                case BossPhase.Phase2:
                    // 第二阶段：攻击速度+30%，移动速度+20%
                    moveSpeed = _baseMoveSpeed * phase2MoveSpeedMultiplier;
                    // 其他属性通过GetCurrentAttackCooldown等动态计算
                    break;
                    
                case BossPhase.Phase3:
                    // 第三阶段：狂暴模式
                    _isEnraged = true;
                    moveSpeed = _baseMoveSpeed * phase3MoveSpeedMultiplier;
                    attackDamage = _baseAttackDamage * phase3AttackMultiplier;
                    
                    // 激活狂暴视觉
                    if (rageVisual != null) rageVisual.SetActive(true);
                    
                    // 播放狂暴音效
                    GameEvents.TriggerNotification("警告：铁钳巨兽进入狂暴状态！");
                    break;
            }
        }

        private float GetCurrentAttackCooldown()
        {
            float cooldown = 1f; // 基础冷却
            
            switch (_currentPhase)
            {
                case BossPhase.Phase2:
                    cooldown /= phase2AttackSpeedMultiplier;
                    break;
                case BossPhase.Phase3:
                    cooldown /= phase3AttackSpeedMultiplier;
                    break;
            }
            
            return cooldown;
        }

        #endregion

        #region 攻击系统

        /// <summary>
        /// 执行钳击连击
        /// </summary>
        public void PerformClawCombo()
        {
            StartCoroutine(ClawComboCoroutine());
        }

        private IEnumerator ClawComboCoroutine()
        {
            OnAttackStarted?.Invoke(BossAttackType.ClawCombo);
            
            for (int i = 0; i < clawComboCount; i++)
            {
                _currentCombo = i + 1;
                
                // 播放预警
                CombatWarningSystem.Instance?.TriggerWarning(
                    transform, CombatWarningSystem.WarningType.Attack, 0.3f);
                
                // 播放动画
                animator?.SetInteger("ComboCount", _currentCombo);
                animator?.SetTrigger("ClawAttack");
                
                yield return new WaitForSeconds(0.3f);
                
                // 执行攻击
                ExecuteClawAttack(i);
                
                // 播放音效
                if (clawAttackSound != null)
                {
                    AudioManager.Instance?.PlaySFX(clawAttackSound);
                }
                
                yield return new WaitForSeconds(clawComboInterval);
            }
            
            ResetCombo();
            _lastAttackTime = Time.time;
        }

        private void ExecuteClawAttack(int comboIndex)
        {
            // 计算伤害（连击递增）
            float damage = clawDamage * (1 + comboIndex * 0.2f);
            
            // 检测扇形范围内的目标
            Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, clawAttackRange);
            
            foreach (var target in targets)
            {
                if (target.gameObject == gameObject) continue;
                
                Vector3 dirToTarget = (target.transform.position - transform.position).normalized;
                float angle = Vector3.Angle(transform.right, dirToTarget);
                
                if (angle <= clawAttackAngle / 2f)
                {
                    var damageable = target.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        damageable.TakeDamage(damage, transform);
                    }
                }
            }
        }

        /// <summary>
        /// 执行冲撞攻击
        /// </summary>
        public void PerformCharge()
        {
            StartCoroutine(ChargeCoroutine());
        }

        private IEnumerator ChargeCoroutine()
        {
            OnAttackStarted?.Invoke(BossAttackType.Charge);
            _isCharging = true;
            
            // 预警阶段
            CombatWarningSystem.Instance?.TriggerWarning(
                transform, CombatWarningSystem.WarningType.Charge, chargeWarningTime);
            
            // 显示冲撞红线
            ShowChargeWarningLine();
            
            // 蓄力动画
            animator?.SetTrigger("ChargeStart");
            
            yield return new WaitForSeconds(chargeWarningTime);
            
            // 冲撞执行
            animator?.SetTrigger("ChargeExecute");
            
            if (chargeSound != null)
            {
                AudioManager.Instance?.PlaySFX(chargeSound);
            }
            
            Vector3 chargeDirection = transform.right;
            float chargeTimer = 0f;
            float maxChargeTime = chargeDistance / chargeSpeed;
            
            // 冲撞过程中的伤害检测
            while (chargeTimer < maxChargeTime)
            {
                // 移动
                Vector3 newPosition = transform.position + chargeDirection * chargeSpeed * Time.deltaTime;
                rb.MovePosition(newPosition);
                
                // 检测碰撞
                CheckChargeCollision(chargeDirection);
                
                // 检测障碍物
                RaycastHit2D hit = Physics2D.Raycast(
                    transform.position, chargeDirection, 1f, chargeObstacleLayers);
                
                if (hit.collider != null)
                {
                    // 撞到障碍物，停止冲撞
                    OnChargeHitObstacle(hit.point);
                    break;
                }
                
                chargeTimer += Time.deltaTime;
                yield return null;
            }
            
            // 冲撞结束
            animator?.SetTrigger("ChargeEnd");
            _isCharging = false;
            _lastChargeTime = Time.time;
        }

        private void ShowChargeWarningLine()
        {
            // 创建预警红线
            Vector3 startPos = transform.position;
            Vector3 direction = transform.right;
            
            // 使用LineRenderer或Debug绘制
            Debug.DrawLine(startPos, startPos + direction * chargeDistance, Color.red, chargeWarningTime);
            
            // 实例化预警线特效
            BossEffectManager.Instance?.ShowChargeWarningLine(startPos, direction, chargeDistance, chargeWarningTime);
        }

        private void CheckChargeCollision(Vector3 direction)
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                transform.position, direction, 2f);
            
            foreach (var hit in hits)
            {
                if (hit.collider.gameObject == gameObject) continue;
                
                var damageable = hit.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(chargeDamage, transform);
                }
            }
        }

        private void OnChargeHitObstacle(Vector2 hitPoint)
        {
            // 撞到障碍物的处理
            // 可以添加眩晕效果
            
            // 创建撞击特效
            BossEffectManager.Instance?.PlayChargeImpact(hitPoint, -transform.right);
        }

        /// <summary>
        /// 进入防御姿态
        /// </summary>
        public void EnterDefend()
        {
            if (_isDefending) return;
            
            _isDefending = true;
            _lastDefendTime = Time.time;
            
            // 播放特效
            defendEffect?.Play();
            if (shieldVisual != null) shieldVisual.SetActive(true);
            
            // 播放动画
            animator?.SetBool("IsDefending", true);
            animator?.SetTrigger("DefendStart");
            
            // 开始防御协程
            StartCoroutine(DefendCoroutine());
        }

        private IEnumerator DefendCoroutine()
        {
            float defendTimer = 0f;
            
            while (defendTimer < defendDuration && _isDefending)
            {
                defendTimer += Time.deltaTime;
                yield return null;
            }
            
            ExitDefend();
        }

        public void ExitDefend()
        {
            if (!_isDefending) return;
            
            _isDefending = false;
            
            defendEffect?.Stop();
            if (shieldVisual != null) shieldVisual.SetActive(false);
            
            animator?.SetBool("IsDefending", false);
            animator?.SetTrigger("DefendEnd");
        }

        /// <summary>
        /// 打破防御
        /// </summary>
        public void BreakDefend()
        {
            if (!_isDefending) return;
            
            ExitDefend();
            
            // 进入眩晕
            animator?.SetTrigger("DefendBreak");
            
            // 可以添加眩晕协程
            StartCoroutine(DefendBreakStunCoroutine());
        }

        private IEnumerator DefendBreakStunCoroutine()
        {
            _isInvincible = true;
            yield return new WaitForSeconds(defendBreakStunDuration);
            _isInvincible = false;
        }

        #endregion

        #region 第二阶段技能

        /// <summary>
        /// 激光扫射
        /// </summary>
        public void PerformLaserSweep()
        {
            StartCoroutine(LaserSweepCoroutine());
        }

        private IEnumerator LaserSweepCoroutine()
        {
            OnAttackStarted?.Invoke(BossAttackType.LaserSweep);
            _isUsingLaser = true;
            
            // 预警
            CombatWarningSystem.Instance?.TriggerWarning(
                transform, CombatWarningSystem.WarningType.Laser, 1f);
            
            animator?.SetTrigger("LaserStart");
            
            yield return new WaitForSeconds(1f);
            
            // 激活激光
            laserEffect?.Play();
            
            if (laserSound != null)
            {
                AudioManager.Instance?.PlaySFX(laserSound);
            }
            
            float sweepTimer = 0f;
            float startAngle = -laserSweepAngle / 2f;
            float endAngle = laserSweepAngle / 2f;
            float currentAngle = startAngle;
            bool sweepingRight = true;
            
            // 创建激光伤害检测
            while (sweepTimer < laserSweepDuration)
            {
                // 计算激光方向
                float rotationSpeedActual = laserRotationSpeed * (sweepingRight ? 1 : -1);
                currentAngle += rotationSpeedActual * Time.deltaTime;
                
                // 检查是否到达边界
                if (currentAngle >= endAngle)
                {
                    currentAngle = endAngle;
                    sweepingRight = false;
                }
                else if (currentAngle <= startAngle)
                {
                    currentAngle = startAngle;
                    sweepingRight = true;
                }
                
                // 执行激光伤害检测
                CheckLaserDamage(currentAngle);
                
                sweepTimer += Time.deltaTime;
                yield return null;
            }
            
            // 结束激光
            laserEffect?.Stop();
            animator?.SetTrigger("LaserEnd");
            
            _isUsingLaser = false;
            _lastLaserTime = Time.time;
        }

        private void CheckLaserDamage(float angle)
        {
            Vector3 laserDirection = Quaternion.Euler(0, 0, angle) * transform.right;
            
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                transform.position, laserDirection, laserMaxRange);
            
            foreach (var hit in hits)
            {
                if (hit.collider.gameObject == gameObject) continue;
                
                var damageable = hit.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(laserSweepDamage * Time.deltaTime, transform);
                }
            }
            
            // 绘制激光线
            Debug.DrawRay(transform.position, laserDirection * laserMaxRange, Color.red, 0.1f);
        }

        /// <summary>
        /// 召唤机械蟹
        /// </summary>
        public void PerformSummon()
        {
            StartCoroutine(SummonCoroutine());
        }

        private IEnumerator SummonCoroutine()
        {
            OnAttackStarted?.Invoke(BossAttackType.Summon);
            
            animator?.SetTrigger("Summon");
            
            if (summonSound != null)
            {
                AudioManager.Instance?.PlaySFX(summonSound);
            }
            
            yield return new WaitForSeconds(summonDuration);
            
            // 召唤机械蟹
            for (int i = 0; i < summonCount; i++)
            {
                Vector3 spawnPos;
                
                if (summonPoints != null && summonPoints.Length > i)
                {
                    spawnPos = summonPoints[i].position;
                }
                else
                {
                    // 随机位置
                    float angle = i * (360f / summonCount) * Mathf.Deg2Rad;
                    spawnPos = transform.position + new Vector3(
                        Mathf.Cos(angle) * 5f, Mathf.Sin(angle) * 5f, 0);
                }
                
                if (mechCrabPrefab != null)
                {
                    GameObject crab = Instantiate(mechCrabPrefab, spawnPos, Quaternion.identity);
                    _summonedCrabs.Add(crab);
                    
                    // 订阅死亡事件以便清理
                    var enemyBase = crab.GetComponent<EnemyBase>();
                    if (enemyBase != null)
                    {
                        enemyBase.OnDeath += () => _summonedCrabs.Remove(crab);
                    }
                }
            }
            
            GameEvents.TriggerNotification("铁钳巨兽召唤了机械蟹！");
            
            _lastSummonTime = Time.time;
        }

        #endregion

        #region 第三阶段技能

        /// <summary>
        /// 全屏地震
        /// </summary>
        public void PerformEarthquake()
        {
            StartCoroutine(EarthquakeCoroutine());
        }

        private IEnumerator EarthquakeCoroutine()
        {
            OnAttackStarted?.Invoke(BossAttackType.Earthquake);
            _isUsingEarthquake = true;
            
            // 预警阶段
            CombatWarningSystem.Instance?.TriggerWarning(
                transform, CombatWarningSystem.WarningType.Earthquake, earthquakeWarningTime);
            
            GameEvents.TriggerNotification("警告：检测到强烈地震波！准备跳跃躲避！");
            
            animator?.SetTrigger("EarthquakeStart");
            
            yield return new WaitForSeconds(earthquakeWarningTime);
            
            // 地震波阶段
            if (earthquakeSound != null)
            {
                AudioManager.Instance?.PlaySFX(earthquakeSound);
            }
            
            earthquakeEffect?.Play();
            
            float earthquakeTimer = 0f;
            float nextWaveTime = 0f;
            
            while (earthquakeTimer < earthquakeDuration)
            {
                // 生成地震波
                if (earthquakeTimer >= nextWaveTime)
                {
                    SpawnEarthquakeWave();
                    nextWaveTime += earthquakeWaveInterval;
                }
                
                // 屏幕震动效果
                CameraShake.Instance?.Shake(0.3f, 0.1f);
                
                earthquakeTimer += Time.deltaTime;
                yield return null;
            }
            
            earthquakeEffect?.Stop();
            animator?.SetTrigger("EarthquakeEnd");
            
            _isUsingEarthquake = false;
            _lastEarthquakeTime = Time.time;
        }

        private void SpawnEarthquakeWave()
        {
            // 创建地震波伤害区域
            // 检测所有在地面上的目标
            Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, 50f);
            
            foreach (var target in targets)
            {
                if (target.gameObject == gameObject) continue;
                
                // 检查目标是否在地面上（可以通过标签或组件判断）
                var player = target.GetComponent<PlayerController>(); // 假设有玩家控制器
                if (player != null && !player.IsJumping)
                {
                    var damageable = target.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        damageable.TakeDamage(earthquakeDamage, transform);
                    }
                }
            }
            
            // 创建地震波视觉特效
            BossEffectManager.Instance?.PlayEarthquakeWave(transform.position, 15f, 3);
        }

        /// <summary>
        /// 暴露背部核心弱点
        /// </summary>
        public void ExposeWeakPoint()
        {
            StartCoroutine(WeakPointCoroutine());
        }

        private IEnumerator WeakPointCoroutine()
        {
            _isWeakPointExposed = true;
            OnWeakPointExposed?.Invoke();
            
            // 播放特效
            weakPointEffect?.Play();
            
            if (weakPointSound != null)
            {
                AudioManager.Instance?.PlaySFX(weakPointSound);
            }
            
            // 弱点暴露期间，Boss可能无法移动或攻击
            animator?.SetBool("WeakPointExposed", true);
            
            yield return new WaitForSeconds(weakPointExposeDuration);
            
            // 隐藏弱点
            _isWeakPointExposed = false;
            weakPointEffect?.Stop();
            animator?.SetBool("WeakPointExposed", false);
            
            _lastWeakPointTime = Time.time;
        }

        #endregion

        #region 伤害与死亡

        public void TakeDamage(float damage, Transform damageSource = null)
        {
            if (_isDead || _isInvincible) return;
            
            // 检查是否击中弱点
            if (_isWeakPointExposed && damageSource != null)
            {
                Vector3 dirToSource = (damageSource.position - transform.position).normalized;
                float angle = Vector3.Angle(-transform.right, dirToSource); // 背部是右侧的反方向
                
                if (angle <= weakPointAngle / 2f)
                {
                    damage *= weakPointDamageMultiplier;
                    // 弱点受击特效
                    GameEvents.TriggerNotification("弱点打击！");
                }
            }
            
            // 防御减伤
            if (_isDefending)
            {
                damage *= (1f - defendDamageReduction);
            }
            
            _currentHealth -= damage;
            _currentHealth = Mathf.Max(0, _currentHealth);
            
            OnTakeDamage?.Invoke(damage);
            OnHealthChanged?.Invoke(_currentHealth);
            
            // 播放受击动画
            animator?.SetTrigger("Hit");
            
            // 检查死亡
            if (_currentHealth <= 0)
            {
                Die();
            }
            else if (_isDefending)
            {
                // 防御状态下受击可能破防
                if (Random.value < 0.1f)
                {
                    BreakDefend();
                }
            }
        }

        private void Die()
        {
            if (_isDead) return;
            
            _isDead = true;
            _currentPhase = BossPhase.Defeated;
            
            // 播放死亡动画
            animator?.SetTrigger("Death");
            
            // 播放音效
            if (defeatSound != null)
            {
                AudioManager.Instance?.PlaySFX(defeatSound);
            }
            
            // 清理召唤的小怪
            foreach (var crab in _summonedCrabs)
            {
                if (crab != null)
                {
                    Destroy(crab);
                }
            }
            _summonedCrabs.Clear();
            
            OnDefeated?.Invoke();
            
            // 切换到死亡状态
            _stateMachine.ChangeState(EnemyState.Dead);
            
            // 激活胜利传送门
            ActivateVictoryPortal();
        }

        private void ActivateVictoryPortal()
        {
            // 查找或创建胜利传送门
            var portal = FindObjectOfType<VictoryPortal>();
            if (portal != null)
            {
                portal.Activate();
            }
        }

        #endregion

        #region 辅助方法

        private void UpdateFacing()
        {
            if (CurrentTarget != null && !_isCharging && !_isUsingLaser)
            {
                Vector3 direction = CurrentTarget.position - transform.position;
                
                if (direction.x > 0)
                {
                    spriteRenderer.flipX = false;
                }
                else if (direction.x < 0)
                {
                    spriteRenderer.flipX = true;
                }
            }
        }

        private void UpdateHealthBar()
        {
            _healthBar?.UpdateHealth(_currentHealth, maxHealth);
        }

        private void ResetCombo()
        {
            _currentCombo = 0;
            _comboTimer = 0f;
            animator?.SetInteger("ComboCount", 0);
        }

        private void HandleStateChanged(EnemyState from, EnemyState to)
        {
            // 状态改变处理
        }

        public void MoveTo(Vector3 targetPosition, float stopDistance = 0.5f)
        {
            if (_isDead || _isDefending || _isCharging) return;
            
            Vector3 direction = targetPosition - transform.position;
            float distance = direction.magnitude;
            
            if (distance <= stopDistance) return;
            
            direction.Normalize();
            
            Vector3 newPosition = transform.position + direction * moveSpeed * Time.deltaTime;
            rb.MovePosition(newPosition);
            
            animator?.SetFloat("MoveSpeed", 1f);
        }

        public bool IsInAttackRange()
        {
            if (CurrentTarget == null) return false;
            
            float distance = Vector3.Distance(transform.position, CurrentTarget.position);
            return distance <= clawAttackRange;
        }

        #endregion

        #region 调试

        private void OnDrawGizmos()
        {
            // 绘制攻击范围
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, clawAttackRange);
            
            // 绘制冲撞距离
            Gizmos.color = Color.yellow;
            Vector3 chargeEnd = transform.position + transform.right * chargeDistance;
            Gizmos.DrawLine(transform.position, chargeEnd);
            
            // 绘制激光范围
            if (_currentPhase >= BossPhase.Phase2)
            {
                Gizmos.color = Color.cyan;
                Vector3 laserLeft = Quaternion.Euler(0, 0, -laserSweepAngle / 2f) * transform.right * laserMaxRange;
                Vector3 laserRight = Quaternion.Euler(0, 0, laserSweepAngle / 2f) * transform.right * laserMaxRange;
                Gizmos.DrawRay(transform.position, laserLeft);
                Gizmos.DrawRay(transform.position, laserRight);
            }
            
            // 绘制弱点角度
            if (_currentPhase >= BossPhase.Phase3)
            {
                Gizmos.color = Color.magenta;
                Vector3 weakLeft = Quaternion.Euler(0, 0, 180 - weakPointAngle / 2f) * transform.right * 3f;
                Vector3 weakRight = Quaternion.Euler(0, 0, 180 + weakPointAngle / 2f) * transform.right * 3f;
                Gizmos.DrawRay(transform.position, weakLeft);
                Gizmos.DrawRay(transform.position, weakRight);
            }
        }

        #endregion
    }

    /// <summary>
    /// 玩家控制器接口（用于地震检测）
    /// </summary>
    public interface PlayerController
    {
        bool IsJumping { get; }
    }

    /// <summary>
    /// 相机震动（单例）
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        public void Shake(float intensity, float duration)
        {
            // 实现相机震动
            StartCoroutine(ShakeCoroutine(intensity, duration));
        }
        
        private IEnumerator ShakeCoroutine(float intensity, float duration)
        {
            Vector3 originalPosition = transform.localPosition;
            float timer = 0f;
            
            while (timer < duration)
            {
                float x = Random.Range(-1f, 1f) * intensity;
                float y = Random.Range(-1f, 1f) * intensity;
                
                transform.localPosition = originalPosition + new Vector3(x, y, 0);
                
                timer += Time.deltaTime;
                yield return null;
            }
            
            transform.localPosition = originalPosition;
        }
    }
}
