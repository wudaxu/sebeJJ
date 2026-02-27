using UnityEngine;
using System.Collections;
using SebeJJ.Combat;

namespace SebeJJ.Enemies
{
    /// <summary>
    /// 安保无人机敌人
    /// </summary>
    public class SecurityDrone : EnemyBase
    {
        [Header("无人机特性")]
        [SerializeField] private float hoverHeight = 0.5f;
        [SerializeField] private float hoverSpeed = 2f;
        [SerializeField] private float laserDamage = 8f;
        [SerializeField] private float laserChargeTime = 0.5f;
        [SerializeField] private float laserDuration = 1f;
        [SerializeField] private float alertRange = 15f;

        [Header("巡逻")]
        [SerializeField] private Vector2[] patrolPoints;
        [SerializeField] private int currentPatrolIndex = 0;

        [Header("视觉效果")]
        [SerializeField] private LineRenderer laserRenderer;
        [SerializeField] private Light alertLight;
        [SerializeField] private SpriteRenderer eyeRenderer;
        [SerializeField] private Color patrolEyeColor = Color.green;
        [SerializeField] private Color alertEyeColor = Color.red;
        [SerializeField] private Color attackEyeColor = Color.magenta;

        [Header("警报")]
        [SerializeField] private bool isAlerted = false;
        [SerializeField] private float alertDuration = 10f;

        // 属性
        public float PatrolRadius => patrolRadius;

        private float _hoverOffset;
        private bool _isChargingLaser;
        private bool _isFiringLaser;
        private float _laserTimer;

        protected override void Awake()
        {
            base.Awake();
            enemyName = "Security Drone";
            moveSpeed = 4f;
            detectionRange = 12f;
            attackRange = 8f;
            patrolRadius = 10f;
        }

        protected override void Start()
        {
            base.Start();
            InitializeLaser();
            
            // 生成巡逻点
            GeneratePatrolPoints();
        }

        protected override void InitializeStates()
        {
            _stateMachine.AddState(new DronePatrolState(this));
            _stateMachine.AddState(new DroneChaseState(this));
            _stateMachine.AddState(new DroneAttackState(this));
            _stateMachine.AddState(new DroneAlertState(this));

            _stateMachine.ChangeState<DronePatrolState>();
        }

        private void InitializeLaser()
        {
            if (laserRenderer == null)
            {
                GameObject laserObj = new GameObject("DroneLaser");
                laserObj.transform.SetParent(transform);
                laserRenderer = laserObj.AddComponent<LineRenderer>();
            }

            laserRenderer.startWidth = 0.1f;
            laserRenderer.endWidth = 0.1f;
            laserRenderer.positionCount = 2;
            laserRenderer.useWorldSpace = true;
            laserRenderer.enabled = false;
            laserRenderer.startColor = Color.red;
            laserRenderer.endColor = new Color(1, 0, 0, 0.5f);
        }

        private void GeneratePatrolPoints()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                patrolPoints = new Vector2[4];
                for (int i = 0; i < 4; i++)
                {
                    float angle = i * 90f * Mathf.Deg2Rad;
                    patrolPoints[i] = _spawnPosition + new Vector2(
                        Mathf.Cos(angle) * patrolRadius,
                        Mathf.Sin(angle) * patrolRadius
                    );
                }
            }
        }

        private void Update()
        {
            base.Update();
            UpdateHover();
            UpdateEyeColor();
        }

        private void UpdateHover()
        {
            _hoverOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
            
            if (visualTransform != null)
            {
                visualTransform.localPosition = new Vector3(0, _hoverOffset, 0);
            }
        }

        private void UpdateEyeColor()
        {
            if (eyeRenderer == null) return;

            Color targetColor = patrolEyeColor;

            if (_isFiringLaser)
            {
                targetColor = attackEyeColor;
            }
            else if (isAlerted || target != null)
            {
                targetColor = alertEyeColor;
            }

            eyeRenderer.color = Color.Lerp(eyeRenderer.color, targetColor, 5f * Time.deltaTime);
        }

        /// <summary>
        /// 执行攻击
        /// </summary>
        protected override void PerformAttack()
        {
            if (_isChargingLaser || _isFiringLaser) return;

            StartCoroutine(LaserAttackSequence());
        }

        private IEnumerator LaserAttackSequence()
        {
            _isChargingLaser = true;

            // 充能阶段
            float chargeTimer = 0f;
            while (chargeTimer < laserChargeTime)
            {
                chargeTimer += Time.deltaTime;
                
                // 显示充能效果
                if (eyeRenderer != null)
                {
                    float flash = Mathf.PingPong(Time.time * 10f, 1f);
                    eyeRenderer.color = Color.Lerp(alertEyeColor, attackEyeColor, flash);
                }

                yield return null;
            }

            _isChargingLaser = false;
            _isFiringLaser = true;

            // 发射激光
            float fireTimer = 0f;
            while (fireTimer < laserDuration)
            {
                fireTimer += Time.deltaTime;
                FireLaser();
                yield return null;
            }

            _isFiringLaser = false;
            laserRenderer.enabled = false;
        }

        private void FireLaser()
        {
            if (target == null)
            {
                laserRenderer.enabled = false;
                return;
            }

            Vector2 startPos = transform.position;
            Vector2 targetPos = target.position;

            laserRenderer.SetPosition(0, startPos);
            laserRenderer.SetPosition(1, targetPos);
            laserRenderer.enabled = true;

            // 射线检测
            Vector2 direction = (targetPos - startPos).normalized;
            float distance = Vector2.Distance(startPos, targetPos);
            
            RaycastHit2D hit = Physics2D.Raycast(startPos, direction, distance);
            
            if (hit.collider != null && hit.collider.TryGetComponent<IDamageable>(out var damageable))
            {
                DamageInfo damageInfo = new DamageInfo(
                    laserDamage * Time.deltaTime,
                    DamageType.Energy,
                    direction,
                    gameObject
                );
                damageable.TakeDamage(damageInfo);
            }
        }

        /// <summary>
        /// 触发警报
        /// </summary>
        public void TriggerAlert()
        {
            if (isAlerted) return;

            isAlerted = true;
            
            // 通知附近无人机
            Collider2D[] nearbyDrones = Physics2D.OverlapCircleAll(transform.position, alertRange);
            foreach (var drone in nearbyDrones)
            {
                if (drone.TryGetComponent<SecurityDrone>(out var securityDrone) && securityDrone != this)
                {
                    securityDrone.ReceiveAlert(target);
                }
            }

            StartCoroutine(AlertCooldown());
        }

        /// <summary>
        /// 接收警报
        /// </summary>
        public void ReceiveAlert(Transform alertTarget)
        {
            if (target == null && alertTarget != null)
            {
                SetTarget(alertTarget);
                isAlerted = true;
                StartCoroutine(AlertCooldown());
            }
        }

        private IEnumerator AlertCooldown()
        {
            yield return new WaitForSeconds(alertDuration);
            isAlerted = false;
        }

        protected override void OnDropItems()
        {
            // 掉落数据碎片
        }

        // 获取下一个巡逻点
        public Vector2 GetNextPatrolPoint()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                return _spawnPosition;
            }

            Vector2 point = patrolPoints[currentPatrolIndex];
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            return point;
        }

        public bool IsFiringLaser => _isFiringLaser;
        public bool IsChargingLaser => _isChargingLaser;
    }

    // 无人机专用状态
    public class DronePatrolState : PatrolState
    {
        public DronePatrolState(EnemyBase enemy) : base(enemy) { }
    }

    public class DroneChaseState : ChaseState
    {
        public DroneChaseState(EnemyBase enemy) : base(enemy) { }

        public override void Enter()
        {
            base.Enter();
            
            if (_enemy is SecurityDrone drone)
            {
                drone.TriggerAlert();
            }
        }
    }

    public class DroneAttackState : AttackState
    {
        public DroneAttackState(EnemyBase enemy) : base(enemy) { }
    }

    public class DroneAlertState : StateBase
    {
        public DroneAlertState(EnemyBase enemy) : base(enemy) { }

        public override void Enter() { }

        public override void Update()
        {
            // 检查目标
            if (_enemy.Target != null)
            {
                _enemy.StateMachine.ChangeState<DroneChaseState>();
            }
            else
            {
                _enemy.StateMachine.ChangeState<DronePatrolState>();
            }
        }
    }
}
