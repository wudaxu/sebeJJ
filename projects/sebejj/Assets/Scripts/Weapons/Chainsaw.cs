using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 链锯武器实现 - 近战持续伤害，无视装甲
    /// </summary>
    public class Chainsaw : WeaponBase
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
        private HashSet<GameObject> hitTargetsThisTick = new HashSet<GameObject>();
        
        // 组件
        private ParticleSystem sparkParticles;
        private TrailRenderer bladeTrail;

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
                Debug.LogError("[Chainsaw] WeaponData未设置!");
                return;
            }
            
            if (weaponData is ChainsawData data)
            {
                chainsawData = data;
            }
            else
            {
                Debug.LogError("[Chainsaw] WeaponData必须是ChainsawData类型!");
            }
        }

        private void Update()
        {
            if (!isAttacking) return;
            
            // 更新锯齿旋转
            UpdateBladeRotation();
            
            // 更新持续攻击
            if (isFullySpunUp)
            {
                continuousAttackTime += Time.deltaTime;
                PerformContinuousDamage();
            }
        }

        protected override void PerformAttack(Vector2 direction)
        {
            if (isAttacking) return; // 已经在攻击中
            
            StartCoroutine(AttackCoroutine(direction));
        }

        private IEnumerator AttackCoroutine(Vector2 direction)
        {
            OnAttackStarted();
            
            // 启动旋转
            yield return StartCoroutine(SpinUp());
            
            if (!isAttacking) yield break;
            
            isFullySpunUp = true;
            isSpinning = true;
            
            // 持续攻击直到按钮释放
            while (isAttacking)
            {
                yield return null;
            }
            
            // 停止旋转
            yield return StartCoroutine(SpinDown());
            
            OnAttackEnded();
        }

        /// <summary>
        /// 启动旋转
        /// </summary>
        private IEnumerator SpinUp()
        {
            float targetSpeed = chainsawData?.bladeRotationSpeed ?? 720f;
            float spinUpTime = chainsawData?.spinUpTime ?? 0.3f;
            float elapsed = 0f;
            
            // 播放启动音效
            PlayAttackSound();
            
            // 启动火花特效
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

        /// <summary>
        /// 停止旋转
        /// </summary>
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
            
            // 停止火花特效
            EnableSparkEffect(false);
        }

        /// <summary>
        /// 更新锯齿旋转
        /// </summary>
        private void UpdateBladeRotation()
        {
            if (bladePivot != null && currentSpinSpeed > 0)
            {
                bladePivot.Rotate(0, 0, currentSpinSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// 执行持续伤害
        /// </summary>
        private void PerformContinuousDamage()
        {
            if (chainsawData == null) return;
            
            hitTargetsThisTick.Clear();
            
            // 获取链锯范围
            Vector2 pivotPos = bladePivot?.position ?? firePoint.position;
            Vector2 tipPos = bladeTip?.position ?? (pivotPos + (Vector2)(firePoint.right * chainsawData.chainsawRange));
            float range = chainsawData.chainsawRange;
            float angle = chainsawData.attackAngle;
            
            // 检测扇形范围内的敌人
            Collider2D[] hits = Physics2D.OverlapCircleAll(pivotPos, range, enemyLayers);
            
            foreach (var hit in hits)
            {
                GameObject target = hit.gameObject;
                
                // 检查角度
                Vector2 toTarget = ((Vector2)hit.transform.position - pivotPos).normalized;
                Vector2 attackDir = (tipPos - pivotPos).normalized;
                float targetAngle = Vector2.Angle(attackDir, toTarget);
                
                if (targetAngle > angle / 2f) continue;
                
                // 检查距离
                float distance = Vector2.Distance(pivotPos, hit.transform.position);
                if (distance > range) continue;
                
                // 避免同一tick重复命中
                if (hitTargetsThisTick.Contains(target)) continue;
                hitTargetsThisTick.Add(target);
                
                // 计算伤害
                ApplyDamageToTarget(target, hit.transform.position, toTarget);
            }
            
            // 生成火花特效
            if (hitTargetsThisTick.Count > 0 && Random.value > 0.7f)
            {
                SpawnSparkEffect();
            }
        }

        /// <summary>
        /// 对目标应用伤害
        /// </summary>
        private void ApplyDamageToTarget(GameObject target, Vector2 hitPosition, Vector2 hitDirection)
        {
            // 计算伤害递增
            float rampMultiplier = 1f + (continuousAttackTime * (chainsawData?.damageRampUp ?? 0.1f));
            float maxMultiplier = chainsawData?.maxDamageMultiplier ?? 2f;
            rampMultiplier = Mathf.Min(rampMultiplier, maxMultiplier);
            
            // 创建伤害
            var damageInfo = CreateDamageInfo(hitPosition, hitDirection);
            damageInfo.BaseDamage = (chainsawData?.damagePerSecond ?? 15f) * Time.deltaTime * rampMultiplier;
            
            // 无视装甲
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
            
            // 触发反馈
            feelController?.TriggerHitFeedback(damageInfo.IsCritical, isKill);
            
            // 触发敌人受击反应
            var hitReaction = target.GetComponent<SebeJJ.Enemies.EnemyHitReaction>();
            hitReaction?.ProcessHitReaction(damageInfo.BaseDamage, hitDirection, weaponData.knockbackForce);
        }

        /// <summary>
        /// 启用/禁用火花特效
        /// </summary>
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

        /// <summary>
        /// 生成火花特效
        /// </summary>
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

        /// <summary>
        /// 获取移动速度惩罚
        /// </summary>
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

        /// <summary>
        /// 在Scene视图中显示攻击范围
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (chainsawData == null) return;
            
            Vector2 pivotPos = bladePivot?.position ?? transform.position;
            float range = chainsawData.chainsawRange;
            float angle = chainsawData.attackAngle;
            
            // 绘制扇形
            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.3f);
            
            Vector2 forward = (bladeTip?.position ?? transform.position) - transform.position;
            if (forward == Vector2.zero) forward = Vector2.right;
            forward = forward.normalized;
            
            float halfAngle = angle / 2f;
            int segments = 20;
            
            for (int i = 0; i <= segments; i++)
            {
                float currentAngle = -halfAngle + (angle * i / segments);
                Vector2 dir = Quaternion.Euler(0, 0, currentAngle) * forward;
                Gizmos.DrawLine(pivotPos, pivotPos + dir * range);
            }
            
            // 绘制弧线
            Gizmos.DrawWireSphere(pivotPos, range);
        }
    }
}