using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 电磁脉冲武器实现 - AOE眩晕机械敌人
    /// </summary>
    public class EMPWeapon : WeaponBase
    {
        [Header("EMP设置")]
        [SerializeField] private Transform emitPoint;
        [SerializeField] private LayerMask enemyLayers;
        [SerializeField] private GameObject empWavePrefab;
        [SerializeField] private GameObject empCenterEffectPrefab;
        
        private EMPData empData;
        private WeaponFeelController feelController;
        
        private void Awake()
        {
            feelController = GetComponent<WeaponFeelController>();
        }
        
        public override void Initialize(Transform weaponOwner, Transform weaponFirePoint = null)
        {
            base.Initialize(weaponOwner, weaponFirePoint);
            
            if (weaponData is EMPData data)
            {
                empData = data;
            }
            else
            {
                Debug.LogError("[EMPWeapon] WeaponData必须是EMPData类型!");
            }
        }

        protected override void PerformAttack(Vector2 direction)
        {
            StartCoroutine(AttackCoroutine(direction));
        }

        private IEnumerator AttackCoroutine(Vector2 direction)
        {
            OnAttackStarted();
            
            // 释放EMP
            ReleaseEMP();
            
            // 播放中心特效
            SpawnCenterEffect();
            
            // 播放EMP波纹特效
            SpawnEMPWave();
            
            // 等待动画完成
            yield return new WaitForSeconds(weaponData.attackDuration);
            
            OnAttackEnded();
        }

        /// <summary>
        /// 释放EMP效果
        /// </summary>
        private void ReleaseEMP()
        {
            Vector2 center = emitPoint?.position ?? firePoint.position;
            float radius = empData?.explosionRadius ?? 8f;
            
            // 检测范围内所有敌人
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, enemyLayers);
            
            HashSet<GameObject> processedTargets = new HashSet<GameObject>();
            
            foreach (var hit in hits)
            {
                GameObject target = hit.gameObject;
                
                // 避免重复处理
                if (processedTargets.Contains(target)) continue;
                processedTargets.Add(target);
                
                // 计算距离
                Vector2 targetPos = hit.transform.position;
                float distance = Vector2.Distance(center, targetPos);
                Vector2 hitDirection = (targetPos - center).normalized;
                
                // 计算伤害(距离衰减)
                float distanceMultiplier = 1f - (distance / radius) * 0.5f; // 边缘50%伤害
                float centerBonus = (distance < radius * 0.3f) ? (empData?.centerDamageMultiplier ?? 1.5f) : 1f;
                
                // 创建伤害
                var damageInfo = CreateDamageInfo(targetPos, hitDirection);
                damageInfo.BaseDamage *= distanceMultiplier * centerBonus;
                
                // 检查是否是机械敌人
                bool isMechanical = IsMechanicalEnemy(target);
                if (isMechanical && empData != null)
                {
                    damageInfo.BaseDamage *= empData.mechanicalBonusDamage;
                    damageInfo.StunDuration = empData.stunDuration;
                }
                
                // 护盾额外伤害
                if (empData?.shieldDamageBonus == true)
                {
                    var shield = target.GetComponent<ShieldSystem>();
                    if (shield != null && shield.CurrentShield > 0)
                    {
                        damageInfo.BaseDamage *= empData.shieldDamageMultiplier;
                    }
                }
                
                // 应用伤害
                ApplyDamage(target, damageInfo, targetPos, hitDirection);
            }
            
            // 触发屏幕震动
            CombatFeedback.Instance?.TriggerScreenShake(0.5f, 0.3f);
        }

        /// <summary>
        /// 检查目标是否为机械敌人
        /// </summary>
        private bool IsMechanicalEnemy(GameObject target)
        {
            // 通过标签或组件判断
            if (target.CompareTag("Mechanical")) return true;
            if (target.CompareTag("Robot")) return true;
            if (target.CompareTag("Drone")) return true;
            
            // 检查是否有机械相关组件
            var enemyAI = target.GetComponent<SebeJJ.AI.EnemyAI>();
            if (enemyAI != null)
            {
                // 可以通过EnemyAI的某种属性判断
                // return enemyAI.EnemyType == EnemyType.Mechanical;
            }
            
            return false;
        }

        /// <summary>
        /// 应用伤害和效果
        /// </summary>
        private void ApplyDamage(GameObject target, DamageInfo damageInfo, Vector2 hitPosition, Vector2 hitDirection)
        {
            var damageable = target.GetComponent<IDamageable>();
            bool isKill = false;
            
            if (damageable != null)
            {
                float healthBefore = damageable.CurrentHealth;
                damageable.TakeDamage(damageInfo);
                isKill = damageable.CurrentHealth <= 0 && healthBefore > 0;
            }
            
            // 应用眩晕
            if (damageInfo.StunDuration > 0)
            {
                ApplyStun(target, damageInfo.StunDuration);
            }
            
            // 特效
            SpawnHitEffect(hitPosition, hitDirection);
            
            // 触发反馈
            feelController?.TriggerHitFeedback(false, isKill);
            
            // 触发敌人受击反应
            var hitReaction = target.GetComponent<SebeJJ.Enemies.EnemyHitReaction>();
            hitReaction?.ProcessHitReaction(damageInfo.BaseDamage, hitDirection, 0); // EMP无击退
        }

        /// <summary>
        /// 应用眩晕效果
        /// </summary>
        private void ApplyStun(GameObject target, float duration)
        {
            var stunnable = target.GetComponent<IStunnable>();
            stunnable?.Stun(duration);
            
            // 或者通过EnemyAI应用眩晕
            var enemyAI = target.GetComponent<SebeJJ.AI.EnemyAI>();
            if (enemyAI != null)
            {
                // enemyAI.ApplyStun(duration);
            }
        }

        /// <summary>
        /// 生成EMP波纹特效
        /// </summary>
        private void SpawnEMPWave()
        {
            if (empWavePrefab != null)
            {
                Vector3 spawnPos = emitPoint?.position ?? firePoint.position;
                var wave = Instantiate(empWavePrefab, spawnPos, Quaternion.identity);
                
                // 初始化波纹
                var empWave = wave.GetComponent<EMPWaveEffect>();
                if (empWave != null && empData != null)
                {
                    empWave.Initialize(empData.explosionRadius, empData.waveExpandSpeed, empData.waveDuration);
                }
            }
        }

        /// <summary>
        /// 生成中心特效
        /// </summary>
        private void SpawnCenterEffect()
        {
            if (empCenterEffectPrefab != null)
            {
                Vector3 spawnPos = emitPoint?.position ?? firePoint.position;
                Instantiate(empCenterEffectPrefab, spawnPos, Quaternion.identity);
            }
            
            SpawnAttackEffect(spawnPos, Vector2.up);
        }

        /// <summary>
        /// 在Scene视图中显示范围
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (empData == null) return;
            
            Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.3f);
            Vector3 center = emitPoint?.position ?? transform.position;
            Gizmos.DrawWireSphere(center, empData.explosionRadius);
            
            Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.1f);
            Gizmos.DrawSphere(center, empData.explosionRadius);
        }
    }

    /// <summary>
    /// 可眩晕接口
    /// </summary>
    public interface IStunnable
    {
        void Stun(float duration);
        void EndStun();
        bool IsStunned { get; }
    }
}