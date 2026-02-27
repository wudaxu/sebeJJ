using UnityEngine;
using SebeJJ.Combat;

namespace SebeJJ.Weapons
{
    /// <summary>
    /// 投射物基类
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Header("基础设置")]
        [SerializeField] protected float damage = 10f;
        [SerializeField] protected float speed = 20f;
        [SerializeField] protected float lifetime = 5f;
        [SerializeField] protected DamageType damageType = DamageType.Physical;

        [Header("碰撞")]
        [SerializeField] protected LayerMask hitLayers;
        [SerializeField] protected bool destroyOnHit = true;
        [SerializeField] protected float knockbackForce = 0f;

        [Header("特效")]
        [SerializeField] protected GameObject hitEffectPrefab;
        [SerializeField] protected AudioClip hitSound;

        // 内部状态
        protected Vector2 _direction;
        protected GameObject _owner;
        protected float _spawnTime;
        protected bool _isCrit;
        protected float _damageMultiplier = 1f;

        protected Rigidbody2D _rb;

        protected virtual void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        protected virtual void Start()
        {
            _spawnTime = Time.time;
        }

        protected virtual void Update()
        {
            // 生命周期检查
            if (Time.time - _spawnTime >= lifetime)
            {
                OnLifetimeExpired();
            }
        }

        protected virtual void FixedUpdate()
        {
            // 移动
            if (_rb != null)
            {
                _rb.velocity = _direction * speed;
            }
        }

        /// <summary>
        /// 初始化投射物
        /// </summary>
        public virtual void Initialize(float damage, Vector2 direction, GameObject owner, 
            float speedMultiplier = 1f, bool isCrit = false)
        {
            this.damage = damage;
            _direction = direction.normalized;
            _owner = owner;
            _isCrit = isCrit;
            _damageMultiplier = speedMultiplier;

            // 设置旋转
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        /// <summary>
        /// 设置伤害
        /// </summary>
        public void SetDamage(float newDamage)
        {
            damage = newDamage;
        }

        /// <summary>
        /// 设置方向
        /// </summary>
        public void SetDirection(Vector2 newDirection)
        {
            _direction = newDirection.normalized;
            float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            // 不击中发射者
            if (other.gameObject == _owner) return;

            // 检查层级
            if ((hitLayers.value & (1 << other.gameObject.layer)) == 0) return;

            // 应用伤害
            OnHit(other);

            // 销毁
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnHit(Collider2D other)
        {
            // 尝试造成伤害
            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                DamageInfo damageInfo = new DamageInfo(
                    damage * _damageMultiplier,
                    damageType,
                    _direction,
                    _owner,
                    _isCrit,
                    knockbackForce
                );
                damageable.TakeDamage(damageInfo);
            }

            // 播放特效
            PlayHitEffect(other.transform.position);
        }

        protected virtual void PlayHitEffect(Vector2 position)
        {
            if (hitEffectPrefab != null)
            {
                Instantiate(hitEffectPrefab, position, Quaternion.identity);
            }

            if (hitSound != null)
            {
                AudioSource.PlayClipAtPoint(hitSound, position);
            }
        }

        protected virtual void OnLifetimeExpired()
        {
            Destroy(gameObject);
        }

        protected virtual void OnDestroy()
        {
            // 清理代码
        }
    }
}
