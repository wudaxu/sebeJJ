using UnityEngine;
using System.Collections.Generic;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 弹丸组件
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Header("视觉效果")]
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private ParticleSystem hitEffect;

        private RangedWeapon ownerWeapon;
        private Vector2 direction;
        private float speed;
        private float lifetime;
        private bool isPiercing;
        private int maxPierceCount;
        
        private Rigidbody2D rb;
        private List<GameObject> hitTargets = new List<GameObject>();
        private int currentPierceCount = 0;
        private bool isInitialized = false;

        public void Initialize(RangedWeapon owner, Vector2 dir, float spd, float life, 
            bool piercing, int pierceCount)
        {
            ownerWeapon = owner;
            direction = dir.normalized;
            speed = spd;
            lifetime = life;
            isPiercing = piercing;
            maxPierceCount = pierceCount;
            isInitialized = true;

            // 设置旋转
            transform.up = direction;

            // 启动销毁计时
            Destroy(gameObject, lifetime);
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (!isInitialized) return;

            // 移动弹丸
            if (rb != null)
            {
                rb.linearVelocity = direction * speed;
            }
            else
            {
                transform.position += (Vector3)(direction * speed * Time.fixedDeltaTime);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isInitialized) return;
            if (ownerWeapon == null) return;

            // 避免重复命中
            if (hitTargets.Contains(other.gameObject)) return;

            // 记录命中
            hitTargets.Add(other.gameObject);

            // 计算命中点
            Vector2 hitPosition = other.ClosestPoint(transform.position);
            Vector2 hitDirection = direction;

            // 通知武器处理命中
            ownerWeapon.OnProjectileHit(other.gameObject, hitPosition, hitDirection);

            // 处理穿透
            if (isPiercing && currentPierceCount < maxPierceCount)
            {
                currentPierceCount++;
                // 继续飞行
            }
            else
            {
                DestroyProjectile();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!isInitialized) return;

            // 碰到墙壁等障碍物时销毁
            if (!isPiercing || currentPierceCount >= maxPierceCount)
            {
                DestroyProjectile();
            }
        }

        /// <summary>
        /// 销毁弹丸
        /// </summary>
        private void DestroyProjectile()
        {
            // 播放命中特效
            if (hitEffect != null)
            {
                hitEffect.Play();
                hitEffect.transform.SetParent(null);
                Destroy(hitEffect.gameObject, 1f);
            }

            // 停止拖尾
            if (trailRenderer != null)
            {
                trailRenderer.transform.SetParent(null);
                trailRenderer.autodestruct = true;
            }

            Destroy(gameObject);
        }
    }
}