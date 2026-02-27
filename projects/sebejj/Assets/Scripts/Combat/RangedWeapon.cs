using UnityEngine;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 远程武器实现 - WP-003
    /// </summary>
    public class RangedWeapon : WeaponBase
    {
        [Header("远程设置")]
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private LayerMask hitLayers;
        [SerializeField] private float hitDetectionRadius = 0.15f;   // 命中判定半径

        private WeaponComboSystem comboSystem;
        private WeaponFeelController feelController;

        private void Awake()
        {
            comboSystem = GetComponent<WeaponComboSystem>();
            feelController = GetComponent<WeaponFeelController>();
        }

        protected override void PerformAttack(Vector2 direction)
        {
            StartCoroutine(AttackCoroutine(direction));
        }

        private System.Collections.IEnumerator AttackCoroutine(Vector2 direction)
        {
            OnAttackStarted();

            // 发射弹丸
            FireProjectile(direction);

            // 播放特效
            SpawnAttackEffect(muzzlePoint?.position ?? firePoint.position, direction);

            // 等待攻击动画完成
            yield return new WaitForSeconds(weaponData.attackDuration);

            OnAttackEnded();
        }

        /// <summary>
        /// 发射弹丸
        /// </summary>
        private void FireProjectile(Vector2 direction)
        {
            if (weaponData.projectilePrefab == null) return;

            Vector2 spawnPos = muzzlePoint?.position ?? firePoint.position;
            
            // 创建弹丸
            var projectile = Instantiate(weaponData.projectilePrefab, spawnPos, Quaternion.identity);
            
            // 初始化弹丸
            var projectileComp = projectile.GetComponent<Projectile>();
            if (projectileComp != null)
            {
                projectileComp.Initialize(
                    this,
                    direction,
                    weaponData.projectileSpeed,
                    weaponData.projectileLifetime,
                    weaponData.piercing,
                    weaponData.pierceCount
                );
            }
            else
            {
                // 如果没有Projectile组件，使用简单的刚体发射
                var rb = projectile.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = direction * weaponData.projectileSpeed;
                }
                Destroy(projectile, weaponData.projectileLifetime);
            }
        }

        /// <summary>
        /// 处理弹丸命中
        /// </summary>
        public void OnProjectileHit(GameObject target, Vector2 hitPosition, Vector2 hitDirection)
        {
            // 应用连招加成
            float damageMultiplier = comboSystem?.GetDamageMultiplier() ?? 1f;
            
            // 创建伤害
            var damageInfo = CreateDamageInfo(hitPosition, hitDirection);
            damageInfo.BaseDamage *= damageMultiplier;

            // 应用伤害
            var damageable = target.GetComponent<IDamageable>();
            bool isKill = false;
            if (damageable != null)
            {
                float healthBefore = damageable.CurrentHealth;
                damageable.TakeDamage(damageInfo);
                isKill = damageable.CurrentHealth <= 0 && healthBefore > 0;
            }

            // 特效
            SpawnHitEffect(hitPosition, hitDirection);
            PlayHitSound(hitPosition);

            // 触发命中反馈
            if (feelController != null)
            {
                feelController.TriggerHitFeedback(damageInfo.IsCritical, isKill);
            }
            else
            {
                CombatFeedback.Instance?.TriggerHitStop(weaponData.hitStopDuration);
            }
            
            // 触发敌人受击反应
            var hitReaction = target.GetComponent<SebeJJ.Enemies.EnemyHitReaction>();
            hitReaction?.ProcessHitReaction(damageInfo.BaseDamage, hitDirection, weaponData.knockbackForce);
        }
    }
}