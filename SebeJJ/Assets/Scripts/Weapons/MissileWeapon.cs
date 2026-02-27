using UnityEngine;

namespace SebeJJ.Weapons
{
    /// <summary>
    /// 导弹/鱼雷武器 - 追踪型投射物
    /// </summary>
    public class MissileWeapon : Weapon
    {
        [Header("导弹设置")]
        [SerializeField] private float explosionRadius = 3f;
        [SerializeField] private float explosionForce = 10f;
        [SerializeField] private float turnSpeed = 180f;
        [SerializeField] private float trackingRange = 15f;
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private bool canLockOn = true;

        [Header("爆炸特效")]
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private AudioClip explosionSound;
        [SerializeField] private LayerMask explosionLayers;

        protected override bool Fire(Vector2 direction)
        {
            if (projectilePrefab == null) return false;

            Vector2 spawnPos = GetFirePosition();
            GameObject missile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

            // 设置导弹
            MissileProjectile missileProj = missile.GetComponent<MissileProjectile>();
            if (missileProj != null)
            {
                missileProj.Initialize(
                    damage,
                    direction,
                    projectileSpeed,
                    explosionRadius,
                    explosionForce,
                    turnSpeed,
                    trackingRange,
                    lifetime,
                    explosionPrefab,
                    explosionSound,
                    explosionLayers,
                    gameObject
                );
            }

            PlayMuzzleFlash();
            PlayFireSound();
            ApplyRecoil(direction);

            return true;
        }
    }

    /// <summary>
    /// 导弹投射物
    /// </summary>
    public class MissileProjectile : MonoBehaviour
    {
        private float _damage;
        private Vector2 _direction;
        private float _speed;
        private float _explosionRadius;
        private float _explosionForce;
        private float _turnSpeed;
        private float _trackingRange;
        private float _lifetime;
        private GameObject _explosionPrefab;
        private AudioClip _explosionSound;
        private LayerMask _explosionLayers;
        private GameObject _owner;
        private Transform _target;

        private Rigidbody2D _rb;
        private float _spawnTime;

        public void Initialize(float damage, Vector2 direction, float speed, 
            float explosionRadius, float explosionForce, float turnSpeed, 
            float trackingRange, float lifetime, GameObject explosionPrefab, 
            AudioClip explosionSound, LayerMask explosionLayers, GameObject owner)
        {
            _damage = damage;
            _direction = direction;
            _speed = speed;
            _explosionRadius = explosionRadius;
            _explosionForce = explosionForce;
            _turnSpeed = turnSpeed;
            _trackingRange = trackingRange;
            _lifetime = lifetime;
            _explosionPrefab = explosionPrefab;
            _explosionSound = explosionSound;
            _explosionLayers = explosionLayers;
            _owner = owner;
            _spawnTime = Time.time;

            // 寻找目标
            FindTarget();
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            // 初始速度
            if (_rb != null)
            {
                _rb.velocity = _direction * _speed;
            }
        }

        private void Update()
        {
            // 检查生命周期
            if (Time.time - _spawnTime >= _lifetime)
            {
                Explode();
                return;
            }

            // 追踪目标
            if (_target != null)
            {
                TrackTarget();
            }
        }

        private void FindTarget()
        {
            // 在范围内寻找最近的敌人
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, _trackingRange);
            float closestDistance = float.MaxValue;

            foreach (var collider in colliders)
            {
                if (collider.gameObject == _owner) continue;
                if (collider.CompareTag("Enemy"))
                {
                    float distance = Vector2.Distance(transform.position, collider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        _target = collider.transform;
                    }
                }
            }
        }

        private void TrackTarget()
        {
            if (_rb == null || _target == null) return;

            Vector2 toTarget = (_target.position - transform.position).normalized;
            Vector2 currentVelocity = _rb.velocity;
            
            // 计算目标角度
            float targetAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
            float currentAngle = Mathf.Atan2(currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg;
            
            // 平滑转向
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, _turnSpeed * Time.deltaTime);
            Vector2 newDirection = new Vector2(
                Mathf.Cos(newAngle * Mathf.Deg2Rad),
                Mathf.Sin(newAngle * Mathf.Deg2Rad)
            );

            _rb.velocity = newDirection * _speed;
            
            // 旋转朝向
            transform.rotation = Quaternion.Euler(0, 0, newAngle);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject == _owner) return;
            
            Explode();
        }

        private void Explode()
        {
            // 爆炸特效
            if (_explosionPrefab != null)
            {
                Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
            }

            // 爆炸音效
            if (_explosionSound != null)
            {
                AudioSource.PlayClipAtPoint(_explosionSound, transform.position);
            }

            // 范围伤害
            ApplyExplosionDamage();

            Destroy(gameObject);
        }

        private void ApplyExplosionDamage()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _explosionRadius, _explosionLayers);
            
            foreach (var hit in hits)
            {
                // 计算伤害衰减
                float distance = Vector2.Distance(transform.position, hit.transform.position);
                float damageMultiplier = 1f - (distance / _explosionRadius);
                damageMultiplier = Mathf.Max(0.2f, damageMultiplier);

                // 应用伤害
                if (hit.TryGetComponent<SebeJJ.Combat.IDamageable>(out var damageable))
                {
                    SebeJJ.Combat.DamageInfo damageInfo = new SebeJJ.Combat.DamageInfo(
                        _damage * damageMultiplier,
                        SebeJJ.Combat.DamageType.Explosive,
                        (hit.transform.position - transform.position).normalized,
                        _owner
                    );
                    damageable.TakeDamage(damageInfo);
                }

                // 击退效果
                if (hit.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    Vector2 knockbackDir = (hit.transform.position - transform.position).normalized;
                    rb.AddForce(knockbackDir * _explosionForce * damageMultiplier, ForceMode2D.Impulse);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _explosionRadius);
        }
    }
}
