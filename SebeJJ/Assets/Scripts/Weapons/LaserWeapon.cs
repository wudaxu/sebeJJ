using UnityEngine;
using SebeJJ.Combat;

namespace SebeJJ.Weapons
{
    /// <summary>
    /// 激光武器 - 持续伤害型武器
    /// </summary>
    public class LaserWeapon : Weapon
    {
        [Header("激光设置")]
        [SerializeField] private float laserWidth = 0.2f;
        [SerializeField] private float tickRate = 0.1f;  // 伤害间隔
        [SerializeField] private LayerMask hitLayers;
        [SerializeField] private bool penetrate = false;

        [Header("视觉效果")]
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private Gradient laserColor;
        [SerializeField] private ParticleSystem impactParticles;

        // 状态
        private bool _isFiring;
        private float _lastTickTime;
        private Vector2 _currentAimDirection;

        protected override void Awake()
        {
            base.Awake();
            
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
            
            SetupLineRenderer();
        }

        private void SetupLineRenderer()
        {
            lineRenderer.startWidth = laserWidth;
            lineRenderer.endWidth = laserWidth;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.enabled = false;
            
            if (laserColor != null)
            {
                lineRenderer.colorGradient = laserColor;
            }
        }

        private void Update()
        {
            if (_isFiring)
            {
                UpdateLaser();
            }
        }

        public override void UpdateWeapon(Vector2 aimDirection)
        {
            _currentAimDirection = aimDirection;
            
            if (_isFiring)
            {
                TryFire(aimDirection);
            }
        }

        protected override bool Fire(Vector2 direction)
        {
            _isFiring = true;
            _currentAimDirection = direction;
            
            // 消耗能量
            // TODO: 从MechStatus消耗能量
            
            return true;
        }

        public override void StopFiring()
        {
            _isFiring = false;
            lineRenderer.enabled = false;
            
            if (impactParticles != null)
            {
                impactParticles.Stop();
            }
        }

        private void UpdateLaser()
        {
            Vector2 startPos = GetFirePosition();
            Vector2 endPos = startPos + _currentAimDirection * range;

            // 射线检测
            RaycastHit2D hit = Physics2D.Raycast(startPos, _currentAimDirection, range, hitLayers);
            
            if (hit.collider != null)
            {
                endPos = hit.point;
                
                // 造成伤害
                if (Time.time >= _lastTickTime + tickRate)
                {
                    ApplyDamage(hit.collider.gameObject);
                    _lastTickTime = Time.time;
                }

                // 更新特效位置
                if (impactParticles != null)
                {
                    impactParticles.transform.position = hit.point;
                    if (!impactParticles.isPlaying)
                    {
                        impactParticles.Play();
                    }
                }
            }
            else
            {
                if (impactParticles != null && impactParticles.isPlaying)
                {
                    impactParticles.Stop();
                }
            }

            // 更新激光视觉效果
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
            lineRenderer.enabled = true;
        }

        private void ApplyDamage(GameObject target)
        {
            if (target.TryGetComponent<IDamageable>(out var damageable))
            {
                DamageInfo damageInfo = new DamageInfo(
                    damage * tickRate,  // 根据间隔调整伤害
                    DamageType.Energy,
                    _currentAimDirection,
                    gameObject
                );
                damageable.TakeDamage(damageInfo);
            }
        }

        private void OnDisable()
        {
            StopFiring();
        }
    }
}
