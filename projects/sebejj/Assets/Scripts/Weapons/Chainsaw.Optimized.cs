using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 链锯武器 - 性能优化版本
    /// 优化点:
    /// 1. 降低伤害检测频率
    /// 2. 复用HashSet避免GC
    /// 3. 缓存Collider结果
    /// 4. 批量处理伤害
    /// </summary>
    public class ChainsawOptimized : WeaponBase
    {
        [Header("链锯设置")]
        [SerializeField] private Transform bladePivot;
        [SerializeField] private Transform bladeTip;
        [SerializeField] private LayerMask enemyLayers;
        [SerializeField] private GameObject bladeEffectPrefab;
        [SerializeField] private GameObject sparkEffectPrefab;
        
        private ChainsawData chainsawData;
        private WeaponFeelController feelController;
        
        // 运行状态
        private bool isSpinning = false;
        private bool isFullySpunUp = false;
        private float currentSpinSpeed = 0f;
        private float continuousAttackTime = 0f;
        
        // 性能优化：复用HashSet
        private static readonly HashSet<GameObject> HitTargetsBuffer = new HashSet<GameObject>(32);
        
        // 性能优化：降低检测频率
        [SerializeField] private float damageCheckInterval = 0.1f;
        private float damageCheckTimer;
        
        // 性能优化：缓存Collider结果
        private Collider2D[] hitBuffer;
        private const int MaxHits = 16;
        
        // 组件缓存
        private ParticleSystem sparkParticles;
        private TrailRenderer bladeTrail;

        private void Awake()
        {
            feelController = GetComponent<WeaponFeelController>();
            hitBuffer = new Collider2D[MaxHits];
        }
        
        public override void Initialize(Transform weaponOwner, Transform weaponFirePoint = null)
        {
            base.Initialize(weaponOwner, weaponFirePoint);
            
            if (weaponData is ChainsawData data)
            {
                chainsawData = data;
            }
        }

        private void Update()
        {
            if (!isAttacking) return;
            
            UpdateBladeRotation();
            
            if (isFullySpunUp)
            {
                continuousAttackTime += Time.deltaTime;
                
                // 间隔性伤害检测
                damageCheckTimer += Time.deltaTime;
                if (damageCheckTimer >= damageCheckInterval)
                {
                    damageCheckTimer = 0f;
                    PerformContinuousDamage();
                }
            }
        }

        protected override void PerformAttack(Vector2 direction)
        {
            if (isAttacking) return;
            StartCoroutine(AttackCoroutine(direction));
        }

        private IEnumerator AttackCoroutine(Vector2 direction)
        {
            OnAttackStarted();
            
            yield return StartCoroutine(SpinUp());
            
            if (!isAttacking) yield break;
            
            isFullySpunUp = true;
            isSpinning = true;
            
            while (isAttacking)
            {
                yield return null;
            }
            
            yield return StartCoroutine(SpinDown());
            
            OnAttackEnded();
        }

        private IEnumerator SpinUp()
        {
            float targetSpeed = chainsawData?.bladeRotationSpeed ?? 720f;
            float spinUpTime = chainsawData?.spinUpTime ?? 0.3f;
            float elapsed = 0f;
            
            PlayAttackSound();
            EnableSparkEffect(true);
            
            while (elapsed < spinUpTime && isAttacking)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / spinUpTime;
                currentSpinSpeed = Mathf.Lerp(0f, targetSpeed, t);
                yield return null;
            }
            
            currentSpinSpeed = targetSpeed;
        }

        private IEnumerator SpinDown()
        {
            float spinDownTime = chainsawData?.spinDownTime ?? 0.5f;
            float elapsed = 0f;
            float startSpeed = currentSpinSpeed;
            
            isFullySpunUp = false;
            
            while (elapsed < spinDownTime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / spinDownTime;
                currentSpinSpeed = Mathf.Lerp(startSpeed, 0f, t);
                yield return null;
            }
            
            currentSpinSpeed = 0f;
            isSpinning = false;
            EnableSparkEffect(false);
        }

        private void UpdateBladeRotation()
        {
            if (bladePivot != null && currentSpinSpeed > 0)
            {
                bladePivot.Rotate(0, 0, currentSpinSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// 执行持续伤害 - 优化版本
        /// </summary>
        private void PerformContinuousDamage()
        {
            if (chainsawData == null) return;
            
            // 清空复用的HashSet
            HitTargetsBuffer.Clear();
            
            Vector2 pivotPos = bladePivot?.position ?? firePoint.position;
            Vector2 tipPos = bladeTip?.position ?? (pivotPos + (Vector2)(firePoint.right * chainsawData.chainsawRange));
            float range = chainsawData.chainsawRange;
            float angle = chainsawData.attackAngle;
            float halfAngle = angle * 0.5f;
            
            // 使用非分配性Overlap
            int hitCount = Physics2D.OverlapCircleNonAlloc(pivotPos, range, hitBuffer, enemyLayers);
            
            Vector2 attackDir = (tipPos - pivotPos).normalized;
            float attackDirX = attackDir.x;
            float attackDirY = attackDir.y;
            
            for (int i = 0; i < hitCount && i < MaxHits; i++)
            {
                var hit = hitBuffer[i];
                if (hit == null) continue;
                
                GameObject target = hit.gameObject;
                
                // 检查是否已处理
                if (HitTargetsBuffer.Contains(target)) continue;
                HitTargetsBuffer.Add(target);
                
                // 检查角度 - 使用点积避免Atan2
                Vector2 toTarget = (Vector2)hit.transform.position - pivotPos;
                float toTargetMag = Mathf.Sqrt(toTarget.x * toTarget.x + toTarget.y * toTarget.y);
                if (toTargetMag < 0.001f) continue;
                
                float dot = (toTarget.x * attackDirX + toTarget.y * attackDirY) / toTargetMag;
                float targetAngle = Mathf.Acos(dot) * Mathf.Rad2Deg;
                
                if (targetAngle > halfAngle) continue;
                
                // 检查距离
                if (toTargetMag > range) continue;
                
                // 应用伤害
                ApplyDamageToTarget(target, hit.transform.position, toTarget / toTargetMag);
            }
            
            // 生成火花特效
            if (HitTargetsBuffer.Count > 0 && Random.value > 0.7f)
            {
                SpawnSparkEffect();
            }
        }

        private void ApplyDamageToTarget(GameObject target, Vector2 hitPosition, Vector2 hitDirection)
        {
            // 计算伤害递增
            float rampMultiplier = 1f + (continuousAttackTime * (chainsawData?.damageRampUp ?? 0.1f));
            float maxMultiplier = chainsawData?.maxDamageMultiplier ?? 2f;
            rampMultiplier = Mathf.Min(rampMultiplier, maxMultiplier);
            
            // 创建伤害
            var damageInfo = CreateDamageInfo(hitPosition, hitDirection);
            damageInfo.BaseDamage = (chainsawData?.damagePerSecond ?? 15f) * damageCheckInterval * rampMultiplier;
            
            if (chainsawData?.ignoreArmor == true)
            {
                damageInfo.ArmorPenetration = 1f;
            }
            
            // 应用伤害
            var damageable = target.GetComponent<IDamageable>();
            bool isKill = false;
            
            if (damageable != null)
            {
                float healthBefore = damageable.CurrentHealth;
                damageable.TakeDamage(damageInfo);
                isKill = damageable.CurrentHealth <= 0 && healthBefore > 0;
            }
            
            feelController?.TriggerHitFeedback(damageInfo.IsCritical, isKill);
            
            var hitReaction = target.GetComponent<SebeJJ.Enemies.EnemyHitReaction>();
            hitReaction?.ProcessHitReaction(damageInfo.BaseDamage, hitDirection, weaponData.knockbackForce);
        }

        private void EnableSparkEffect(bool enable)
        {
            if (sparkParticles == null && sparkEffectPrefab != null)
            {
                var sparkObj = Instantiate(sparkEffectPrefab, bladeTip?.position ?? firePoint.position, Quaternion.identity, transform);
                sparkParticles = sparkObj.GetComponent<ParticleSystem>();
            }
            
            if (sparkParticles != null)
            {
                if (enable)
                    sparkParticles.Play();
                else
                    sparkParticles.Stop();
            }
        }

        private void SpawnSparkEffect()
        {
            if (sparkEffectPrefab != null && bladeTip != null)
            {
                Vector2 sparkPos = bladeTip.position;
                Vector2 randomDir = Random.insideUnitCircle;
                var spark = Instantiate(sparkEffectPrefab, sparkPos, Quaternion.LookRotation(randomDir));
                Destroy(spark, 0.5f);
            }
        }

        public float GetMoveSpeedMultiplier()
        {
            if (!isAttacking || chainsawData == null) return 1f;
            return 1f - chainsawData.moveSpeedPenalty;
        }

        public override void Unequip()
        {
            StopAllCoroutines();
            isSpinning = false;
            isFullySpunUp = false;
            currentSpinSpeed = 0f;
            continuousAttackTime = 0f;
            EnableSparkEffect(false);
            base.Unequip();
        }
    }
}
