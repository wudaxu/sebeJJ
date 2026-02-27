using UnityEngine;
using System.Collections;
using SebeJJ.Combat;

namespace SebeJJ.Enemies
{
    /// <summary>
    /// 防御炮塔敌人
    /// </summary>
    public class DefenseTurret : EnemyBase
    {
        [Header("炮塔特性")]
        [SerializeField] private float rotationSpeed = 90f;
        [SerializeField] private float aimThreshold = 5f; // 瞄准阈值角度
        [SerializeField] private int burstCount = 3;
        [SerializeField] private float burstInterval = 0.1f;
        [SerializeField] private float reloadTime = 2f;
        [SerializeField] private float projectileSpeed = 15f;

        [Header("炮塔部件")]
        [SerializeField] private Transform turretHead;
        [SerializeField] private Transform[] firePoints;
        [SerializeField] private GameObject projectilePrefab;

        [Header("视觉效果")]
        [SerializeField] private LineRenderer laserSight;
        [SerializeField] private ParticleSystem muzzleFlash;
        [SerializeField] private Light warningLight;

        [Header("状态")]
        [SerializeField] private bool isReloading = false;
        [SerializeField] private int currentBurstCount = 0;

        private int _currentFirePointIndex = 0;

        protected override void Awake()
        {
            base.Awake();
            enemyName = "Defense Turret";
            moveSpeed = 0f; // 炮塔不移动
            detectionRange = 15f;
            attackRange = 12f;
            attackCooldown = 0.5f;
        }

        protected override void Start()
        {
            base.Start();
            InitializeLaserSight();
        }

        protected override void InitializeStates()
        {
            _stateMachine.AddState(new TurretIdleState(this));
            _stateMachine.AddState(new TurretAimState(this));
            _stateMachine.AddState(new TurretAttackState(this));
            _stateMachine.AddState(new TurretReloadState(this));

            _stateMachine.ChangeState<TurretIdleState>();
        }

        private void InitializeLaserSight()
        {
            if (laserSight == null)
            {
                GameObject laserObj = new GameObject("LaserSight");
                laserObj.transform.SetParent(turretHead != null ? turretHead : transform);
                laserSight = laserObj.AddComponent<LineRenderer>();
            }

            laserSight.startWidth = 0.02f;
            laserSight.endWidth = 0.02f;
            laserSight.positionCount = 2;
            laserSight.useWorldSpace = true;
            laserSight.enabled = false;
            laserSight.startColor = Color.red;
            laserSight.endColor = new Color(1, 0, 0, 0);
        }

        private void Update()
        {
            base.Update();
            UpdateLaserSight();
        }

        private void UpdateLaserSight()
        {
            if (laserSight == null) return;

            if (target != null && _stateMachine.CurrentState is TurretAimState or TurretAttackState)
            {
                laserSight.enabled = true;
                Vector2 startPos = GetCurrentFirePoint().position;
                Vector2 targetPos = target.position;
                
                laserSight.SetPosition(0, startPos);
                laserSight.SetPosition(1, targetPos);
            }
            else
            {
                laserSight.enabled = false;
            }
        }

        /// <summary>
        /// 旋转炮塔朝向目标
        /// </summary>
        public bool RotateTowards(Vector2 targetPosition)
        {
            if (turretHead == null) return false;

            Vector2 direction = (targetPosition - (Vector2)turretHead.position).normalized;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            float currentAngle = turretHead.eulerAngles.z;

            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotationSpeed * Time.deltaTime);
            turretHead.rotation = Quaternion.Euler(0, 0, newAngle);

            // 检查是否瞄准
            float angleDiff = Mathf.Abs(Mathf.DeltaAngle(currentAngle, targetAngle));
            return angleDiff < aimThreshold;
        }

        /// <summary>
        /// 获取当前发射点
        /// </summary>
        public Transform GetCurrentFirePoint()
        {
            if (firePoints == null || firePoints.Length == 0)
            {
                return transform;
            }

            return firePoints[_currentFirePointIndex % firePoints.Length];
        }

        /// <summary>
        /// 切换到下一个发射点
        /// </summary>
        public void NextFirePoint()
        {
            _currentFirePointIndex++;
        }

        /// <summary>
        /// 执行攻击
        /// </summary>
        protected override void PerformAttack()
        {
            if (isReloading) return;

            StartCoroutine(BurstFire());
        }

        private IEnumerator BurstFire()
        {
            for (int i = 0; i < burstCount; i++)
            {
                FireProjectile();
                yield return new WaitForSeconds(burstInterval);
            }

            // 开始装填
            StartCoroutine(Reload());
        }

        private void FireProjectile()
        {
            if (projectilePrefab == null) return;

            Transform firePoint = GetCurrentFirePoint();
            Vector2 fireDirection = firePoint.right;

            // 创建投射物
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            
            // 设置投射物
            Projectile proj = projectile.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Initialize(attackDamage, fireDirection, gameObject);
            }
            else
            {
                // 如果没有投射物组件，直接给刚体速度
                if (projectile.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    rb.velocity = fireDirection * projectileSpeed;
                }
            }

            // 特效
            if (muzzleFlash != null)
            {
                muzzleFlash.transform.position = firePoint.position;
                muzzleFlash.Play();
            }

            // 切换到下一个发射点
            NextFirePoint();
        }

        private IEnumerator Reload()
        {
            isReloading = true;
            
            if (warningLight != null)
            {
                warningLight.color = Color.yellow;
            }

            yield return new WaitForSeconds(reloadTime);

            isReloading = false;
            currentBurstCount = 0;

            if (warningLight != null)
            {
                warningLight.color = Color.red;
            }
        }

        protected override void OnDropItems()
        {
            // 掉落废金属和数据碎片
        }

        public bool IsReloading => isReloading;
    }

    // 炮塔专用状态
    public class TurretIdleState : StateBase
    {
        public TurretIdleState(EnemyBase enemy) : base(enemy) { }

        public override void Update()
        {
            if (_enemy.Target != null)
            {
                _enemy.StateMachine.ChangeState<TurretAimState>();
            }
        }

        public override void FixedUpdate()
        {
            // 缓慢旋转扫描
            if (_enemy is DefenseTurret turret && turret.transform != null)
            {
                turret.transform.Rotate(0, 0, 10f * Time.fixedDeltaTime);
            }
        }
    }

    public class TurretAimState : StateBase
    {
        public TurretAimState(EnemyBase enemy) : base(enemy) { }

        public override void Update()
        {
            if (_enemy.Target == null)
            {
                _enemy.StateMachine.ChangeState<TurretIdleState>();
                return;
            }

            if (_enemy is DefenseTurret turret)
            {
                bool isAimed = turret.RotateTowards(_enemy.Target.position);
                
                if (isAimed)
                {
                    _enemy.StateMachine.ChangeState<TurretAttackState>();
                }
            }
        }
    }

    public class TurretAttackState : StateBase
    {
        public TurretAttackState(EnemyBase enemy) : base(enemy) { }

        public override void Enter()
        {
            _enemy.TryAttack();
        }

        public override void Update()
        {
            if (_enemy.Target == null)
            {
                _enemy.StateMachine.ChangeState<TurretIdleState>();
                return;
            }

            if (_enemy is DefenseTurret turret && turret.IsReloading)
            {
                _enemy.StateMachine.ChangeState<TurretReloadState>();
                return;
            }

            // 保持瞄准
            if (_enemy is DefenseTurret t)
            {
                bool stillAimed = t.RotateTowards(_enemy.Target.position);
                
                if (!stillAimed)
                {
                    _enemy.StateMachine.ChangeState<TurretAimState>();
                }
            }
        }
    }

    public class TurretReloadState : StateBase
    {
        public TurretReloadState(EnemyBase enemy) : base(enemy) { }

        public override void Update()
        {
            if (_enemy is DefenseTurret turret && !turret.IsReloading)
            {
                if (_enemy.Target != null)
                {
                    _enemy.StateMachine.ChangeState<TurretAimState>();
                }
                else
                {
                    _enemy.StateMachine.ChangeState<TurretIdleState>();
                }
            }
        }
    }
}
