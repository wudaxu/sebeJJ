using UnityEngine;
using System.Collections.Generic;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 近战武器实现 - WP-002
    /// </summary>
    public class MeleeWeapon : WeaponBase
    {
        [Header("近战检测")]
        [SerializeField] private LayerMask hitLayers;
        [SerializeField] private bool showDebugGizmos = false;

        private List<GameObject> hitTargets = new List<GameObject>();
        private WeaponComboSystem comboSystem;
        private WeaponFeelController feelController;

        private void Awake()
        {
            comboSystem = GetComponent<WeaponComboSystem>();
            feelController = GetComponent<WeaponFeelController>();
        }

        protected override void PerformAttack(Vector2 direction)
        {
            if (feelController != null)
            {
                feelController.StartAttack();
                feelController.OnActiveStart += () => StartCoroutine(AttackActiveCoroutine(direction));
                feelController.OnAttackComplete += OnAttackEnded;
            }
            else
            {
                StartCoroutine(AttackCoroutine(direction));
            }
            
            // 记录连招
            comboSystem?.RecordAttack();
        }

        private System.Collections.IEnumerator AttackActiveCoroutine(Vector2 direction)
        {
            hitTargets.Clear();
            
            // 应用连招加成
            float damageMultiplier = comboSystem?.GetDamageMultiplier() ?? 1f;
            float rangeMultiplier = comboSystem?.GetRangeMultiplier() ?? 1f;
            
            // 攻击持续时间内持续检测
            float elapsed = 0f;
            float checkInterval = 0.05f;
            float nextCheck = 0f;
            float activeDuration = weaponData.attackDuration * (comboSystem?.GetSpeedMultiplier() ?? 1f);

            // 播放攻击特效
            SpawnAttackEffect(firePoint.position, direction);

            while (elapsed < activeDuration)
            {
                elapsed += Time.deltaTime;

                if (elapsed >= nextCheck)
                {
                    nextCheck += checkInterval;
                    PerformMeleeHitCheck(direction, damageMultiplier, rangeMultiplier);
                }

                yield return null;
            }
        }

        private System.Collections.IEnumerator AttackCoroutine(Vector2 direction)
        {
            OnAttackStarted();
            hitTargets.Clear();

            // 攻击持续时间内持续检测
            float elapsed = 0f;
            float checkInterval = 0.05f;
            float nextCheck = 0f;

            // 播放攻击特效
            SpawnAttackEffect(firePoint.position, direction);

            while (elapsed < weaponData.attackDuration)
            {
                elapsed += Time.deltaTime;

                if (elapsed >= nextCheck)
                {
                    nextCheck += checkInterval;
                    PerformMeleeHitCheck(direction);
                }

                yield return null;
            }

            OnAttackEnded();
        }

        /// <summary>
        /// 执行近战命中检测
        /// </summary>
        private void PerformMeleeHitCheck(Vector2 direction, float damageMultiplier = 1f, float rangeMultiplier = 1f)
        {
            Vector2 origin = firePoint.position;
            float range = GetCurrentRange() * rangeMultiplier;
            float arc = weaponData.attackArc;

            // 使用扇形检测或圆形检测
            Collider2D[] hits;
            
            if (arc >= 360f)
            {
                // 圆形检测
                hits = Physics2D.OverlapCircleAll(origin, range, hitLayers);
            }
            else
            {
                // 扇形检测
                hits = Physics2D.OverlapCircleAll(origin, range, hitLayers);
            }

            foreach (var hit in hits)
            {
                // 检查角度是否在扇形范围内
                if (arc < 360f)
                {
                    Vector2 toTarget = (hit.transform.position - firePoint.position).normalized;
                    float angle = Vector2.Angle(direction, toTarget);
                    if (angle > arc * 0.5f) continue;
                }

                // 避免重复命中同一目标
                if (hitTargets.Contains(hit.gameObject)) continue;

                // 检查是否有遮挡
                Vector2 hitPos = hit.transform.position;
                RaycastHit2D rayHit = Physics2D.Raycast(origin, (hitPos - origin).normalized, 
                    Vector2.Distance(origin, hitPos), hitLayers);
                
                if (rayHit.collider != null && rayHit.collider != hit)
                    continue;

                // 命中处理
                hitTargets.Add(hit.gameObject);
                OnTargetHit(hit, direction, damageMultiplier);
            }
        }

        /// <summary>
        /// 命中目标处理
        /// </summary>
        private void OnTargetHit(Collider2D target, Vector2 attackDirection, float damageMultiplier = 1f)
        {
            Vector2 hitPosition = target.transform.position;
            Vector2 hitDirection = (hitPosition - (Vector2)firePoint.position).normalized;

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

            // 击退效果
            var rb = target.GetComponent<Rigidbody2D>();
            if (rb != null && weaponData.knockbackForce > 0)
            {
                Vector2 knockback = DamageCalculator.CalculateKnockback(
                    hitDirection, weaponData.knockbackForce, rb.mass);
                rb.AddForce(knockback, ForceMode2D.Impulse);
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
                // 触发命中停顿 - FB-003
                CombatFeedback.Instance?.TriggerHitStop(weaponData.hitStopDuration);
            }
            
            // 触发敌人受击反应
            var hitReaction = target.GetComponent<SebeJJ.Enemies.EnemyHitReaction>();
            hitReaction?.ProcessHitReaction(damageInfo.BaseDamage, hitDirection, weaponData.knockbackForce);
        }

        private void OnDrawGizmosSelected()
        {
            if (!showDebugGizmos || weaponData == null) return;

            Gizmos.color = Color.red;
            
            float range = Application.isPlaying ? GetCurrentRange() : weaponData.attackRange;
            float arc = weaponData.attackArc;

            if (arc >= 360f)
            {
                Gizmos.DrawWireSphere(transform.position, range);
            }
            else
            {
                Vector3 forward = transform.up;
                float halfArc = arc * 0.5f;
                
                Vector3 leftBoundary = Quaternion.Euler(0, 0, -halfArc) * forward * range;
                Vector3 rightBoundary = Quaternion.Euler(0, 0, halfArc) * forward * range;

                Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
                Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
                
                // 绘制弧线
                int segments = 20;
                Vector3 prevPoint = transform.position + leftBoundary;
                for (int i = 1; i <= segments; i++)
                {
                    float t = (float)i / segments;
                    float angle = Mathf.Lerp(-halfArc, halfArc, t);
                    Vector3 point = transform.position + Quaternion.Euler(0, 0, angle) * forward * range;
                    Gizmos.DrawLine(prevPoint, point);
                    prevPoint = point;
                }
            }
        }
    }
}