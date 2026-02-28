using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 等离子炮武器实现 - 发射穿透性等离子球
    /// </summary>
    public class PlasmaCannon : WeaponBase
    {
        [Header("等离子炮设置")]
        [SerializeField] private Transform muzzlePoint;
        [SerializeField] private LayerMask hitLayers;
        [SerializeField] private GameObject plasmaBallPrefab;
        [SerializeField] private GameObject trailEffectPrefab;
        
        private PlasmaCannonData plasmaData;
        private WeaponFeelController feelController;
        private bool isCharging = false;
        
        private void Awake()
        {
            feelController = GetComponent<WeaponFeelController>();
        }
        
        public override void Initialize(Transform weaponOwner, Transform weaponFirePoint = null)
        {
            base.Initialize(weaponOwner, weaponFirePoint);
            
            // SO-001修复: 添加null检查
            if (weaponData == null)
            {
                Debug.LogError("[PlasmaCannon] WeaponData未设置!");
                return;
            }
            
            // 获取等离子炮专属数据
            if (weaponData is PlasmaCannonData data)
            {
                plasmaData = data;
            }
            else
            {
                Debug.LogError("[PlasmaCannon] WeaponData必须是PlasmaCannonData类型!");
            }
        }

        protected override void PerformAttack(Vector2 direction)
        {
            StartCoroutine(AttackCoroutine(direction));
        }

        private IEnumerator AttackCoroutine(Vector2 direction)
        {
            OnAttackStarted();
            
            // 充能阶段
            isCharging = true;
            float chargeTime = plasmaData?.chargeTime ?? 0.3f;
            
            // 播放充能特效
            SpawnChargeEffect();
            
            yield return new WaitForSeconds(chargeTime);
            
            if (!isAttacking) yield break; // 攻击被取消
            
            isCharging = false;
            
            // 发射等离子球
            FirePlasmaBall(direction);
            
            // 播放发射特效
            SpawnAttackEffect(muzzlePoint?.position ?? firePoint.position, direction);
            
            // 等待后摇
            yield return new WaitForSeconds(weaponData.attackRecovery);
            
            OnAttackEnded();
        }

        /// <summary>
        /// 发射等离子球
        /// </summary>
        private void FirePlasmaBall(Vector2 direction)
        {
            Vector2 spawnPos = muzzlePoint?.position ?? firePoint.position;
            
            // 创建等离子球
            GameObject plasmaBall = null;
            if (plasmaBallPrefab != null)
            {
                plasmaBall = Instantiate(plasmaBallPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                // 使用默认预制体
                plasmaBall = CreateDefaultPlasmaBall(spawnPos);
            }
            
            // 初始化等离子球
            var projectile = plasmaBall.GetComponent<PlasmaProjectile>();
            if (projectile != null)
            {
                projectile.Initialize(this, direction, plasmaData);
            }
            else
            {
                // 回退到简单刚体
                var rb = plasmaBall.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = direction * weaponData.projectileSpeed;
                }
                Destroy(plasmaBall, weaponData.projectileLifetime);
            }
        }

        /// <summary>
        /// 创建默认等离子球
        /// </summary>
        private GameObject CreateDefaultPlasmaBall(Vector2 position)
        {
            var go = new GameObject("PlasmaBall");
            go.transform.position = position;
            
            // 添加碰撞器
            var collider = go.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = plasmaData?.plasmaBallSize ?? 0.5f;
            
            // 添加刚体
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.isKinematic = true;
            
            // 添加等离子弹丸组件
            var projectile = go.AddComponent<PlasmaProjectile>();
            
            // 添加视觉
            var sprite = go.AddComponent<SpriteRenderer>();
            sprite.color = plasmaData?.plasmaColor ?? Color.cyan;
            
            return go;
        }

        /// <summary>
        /// 播放充能特效
        /// </summary>
        private void SpawnChargeEffect()
        {
            // 在枪口位置生成充能光球
            // 实际实现中应该实例化特效预制体
        }

        /// <summary>
        /// 处理等离子球命中
        /// </summary>
        public void OnPlasmaHit(GameObject target, Vector2 hitPosition, Vector2 hitDirection, int pierceCount)
        {
            // 计算穿透衰减后的伤害
            float damageMultiplier = 1f;
            if (plasmaData != null && pierceCount > 0)
            {
                damageMultiplier = Mathf.Pow(plasmaData.pierceDamageFalloff, pierceCount);
            }
            
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
            
            // 触发反馈
            feelController?.TriggerHitFeedback(damageInfo.IsCritical, isKill);
            
            // 触发敌人受击反应
            var hitReaction = target.GetComponent<SebeJJ.Enemies.EnemyHitReaction>();
            hitReaction?.ProcessHitReaction(damageInfo.BaseDamage, hitDirection, weaponData.knockbackForce);
        }

        public override void Unequip()
        {
            // 取消充能
            isCharging = false;
            StopAllCoroutines();
            base.Unequip();
        }
    }
}