using UnityEngine;
using System.Collections.Generic;

namespace SebeJJ.Enemies
{
    /// <summary>
    /// 深海电鳗AI - BUG-009修复
    /// 修复连锁伤害范围异常问题
    /// </summary>
    public class DeepEelAI : EnemyBase
    {
        [Header("电鳗特有属性")]
        [SerializeField] private float chainRange = 5f;
        [SerializeField] private int maxChainTargets = 3;
        [SerializeField] private float chainDamageFalloff = 0.7f; // 每次连锁伤害衰减
        [SerializeField] private float chainCooldown = 5f;
        [SerializeField] private float shockDamage = 25f;
        [SerializeField] private LayerMask targetLayer;
        
        [Header("特效")]
        [SerializeField] private ParticleSystem electricEffect;
        [SerializeField] private LineRenderer chainLineRenderer;
        
        private float lastChainTime;
        private List<Transform> chainTargets = new List<Transform>();
        
        protected override void InitializeStates()
        {
            // 初始化电鳗特有的AI状态
            StateMachine.RegisterState(EnemyState.Idle, new EelIdleState(this));
            StateMachine.RegisterState(EnemyState.Chase, new EelChaseState(this));
            StateMachine.RegisterState(EnemyState.Attack, new EelAttackState(this));
        }
        
        /// <summary>
        /// 执行连锁电击攻击 - BUG-009修复
        /// 修复范围计算和伤害衰减
        /// </summary>
        public void PerformChainAttack()
        {
            if (Time.time - lastChainTime < chainCooldown) return;
            
            lastChainTime = Time.time;
            chainTargets.Clear();
            
            // 获取主要目标
            Transform currentTarget = CurrentTarget;
            if (currentTarget == null) return;
            
            // 播放特效
            electricEffect?.Play();
            
            // 执行连锁
            float currentDamage = shockDamage;
            float currentRange = chainRange;
            Transform lastTarget = currentTarget;
            
            // 对主要目标造成伤害
            ApplyShockDamage(currentTarget, currentDamage);
            chainTargets.Add(currentTarget);
            
            // 连锁到其他目标
            for (int i = 1; i < maxChainTargets; i++)
            {
                // 查找范围内的下一个目标
                Transform nextTarget = FindNextChainTarget(lastTarget, currentRange, chainTargets);
                if (nextTarget == null) break;
                
                // 应用伤害衰减
                currentDamage *= chainDamageFalloff;
                currentRange *= 0.8f; // 范围递减
                
                // 造成伤害
                ApplyShockDamage(nextTarget, currentDamage);
                
                // 绘制连锁线
                DrawChainLine(lastTarget, nextTarget);
                
                chainTargets.Add(nextTarget);
                lastTarget = nextTarget;
            }
            
            Debug.Log($"[DeepEelAI] 连锁电击命中 {chainTargets.Count} 个目标");
        }
        
        /// <summary>
        /// 查找下一个连锁目标
        /// </summary>
        private Transform FindNextChainTarget(Transform fromTarget, float range, List<Transform> excludeTargets)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(fromTarget.position, range, targetLayer);
            
            Transform nearestTarget = null;
            float nearestDistance = float.MaxValue;
            
            foreach (var collider in colliders)
            {
                if (excludeTargets.Contains(collider.transform)) continue;
                if (collider.transform == fromTarget) continue;
                
                // 检查是否是有效目标
                var damageable = collider.GetComponent<IDamageable>();
                if (damageable == null || damageable.IsDead) continue;
                
                float distance = Vector2.Distance(fromTarget.position, collider.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestTarget = collider.transform;
                }
            }
            
            return nearestTarget;
        }
        
        /// <summary>
        /// 应用电击伤害
        /// </summary>
        private void ApplyShockDamage(Transform target, float damage)
        {
            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage, transform);
                
                // 添加麻痹效果
                var enemy = target.GetComponent<EnemyBase>();
                if (enemy != null)
                {
                    // 短暂眩晕
                    StartCoroutine(ShockStunCoroutine(enemy, 0.5f));
                }
            }
        }
        
        private System.Collections.IEnumerator ShockStunCoroutine(EnemyBase enemy, float duration)
        {
            // 实现眩晕逻辑
            yield return new WaitForSeconds(duration);
        }
        
        /// <summary>
        /// 绘制连锁线
        /// </summary>
        private void DrawChainLine(Transform from, Transform to)
        {
            if (chainLineRenderer == null) return;
            
            // 设置线条位置
            chainLineRenderer.positionCount = 2;
            chainLineRenderer.SetPosition(0, from.position);
            chainLineRenderer.SetPosition(1, to.position);
            
            // 显示线条
            chainLineRenderer.enabled = true;
            
            // 延迟隐藏
            Invoke(nameof(HideChainLine), 0.2f);
        }
        
        private void HideChainLine()
        {
            if (chainLineRenderer != null)
            {
                chainLineRenderer.enabled = false;
            }
        }
        
        /// <summary>
        /// 执行普通电击攻击
        /// </summary>
        public override void PerformAttack()
        {
            if (CurrentTarget == null) return;
            
            // 播放攻击动画
            SetAnimationTrigger("Attack");
            
            // 造成伤害
            ApplyShockDamage(CurrentTarget, stats.attackDamage);
            
            RecordAttack();
        }
        
        protected override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            
            // 绘制连锁范围
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, chainRange);
            
            // 绘制连锁目标
            if (chainTargets.Count > 0)
            {
                Gizmos.color = Color.yellow;
                for (int i = 0; i < chainTargets.Count - 1; i++)
                {
                    if (chainTargets[i] != null && chainTargets[i + 1] != null)
                    {
                        Gizmos.DrawLine(chainTargets[i].position, chainTargets[i + 1].position);
                    }
                }
            }
        }
    }
    
    #region AI状态
    
    public class EelIdleState : IAIState
    {
        private DeepEelAI eel;
        
        public EelIdleState(DeepEelAI eel)
        {
            this.eel = eel;
        }
        
        public void Enter() { }
        
        public void Update()
        {
            // 检测玩家
            if (eel.CurrentTarget != null)
            {
                eel.StateMachine.ChangeState(EnemyState.Chase);
            }
        }
        
        public void Exit() { }
    }
    
    public class EelChaseState : IAIState
    {
        private DeepEelAI eel;
        
        public EelChaseState(DeepEelAI eel)
        {
            this.eel = eel;
        }
        
        public void Enter() { }
        
        public void Update()
        {
            if (eel.CurrentTarget == null)
            {
                eel.StateMachine.ChangeState(EnemyState.Idle);
                return;
            }
            
            // 追击玩家
            eel.MoveTo(eel.CurrentTarget.position, eel.stats.attackRange);
            
            // 检查攻击范围
            if (eel.IsInAttackRange())
            {
                eel.StateMachine.ChangeState(EnemyState.Attack);
            }
        }
        
        public void Exit() { }
    }
    
    public class EelAttackState : IAIState
    {
        private DeepEelAI eel;
        private float attackTimer;
        
        public EelAttackState(DeepEelAI eel)
        {
            this.eel = eel;
        }
        
        public void Enter()
        {
            attackTimer = 0;
        }
        
        public void Update()
        {
            attackTimer += Time.deltaTime;
            
            // 使用连锁攻击或普通攻击
            if (eel.CanAttack)
            {
                if (Random.value < 0.3f) // 30%概率使用连锁攻击
                {
                    eel.PerformChainAttack();
                }
                else
                {
                    eel.PerformAttack();
                }
            }
            
            // 玩家离开攻击范围则追击
            if (!eel.IsInAttackRange())
            {
                eel.StateMachine.ChangeState(EnemyState.Chase);
            }
        }
        
        public void Exit() { }
    }
    
    #endregion
}
