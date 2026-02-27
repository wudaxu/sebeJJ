using UnityEngine;
using System.Collections;

namespace SebeJJ.Enemies
{
    /// <summary>
    /// 机械鲨鱼 - 高速游动的冲撞型敌人
    /// 出现深度：60-100米
    /// </summary>
    public class MechShark : EnemyBase
    {
        [Header("机械鲨鱼特性")]
        [SerializeField] private float patrolSpeed = 4f;
        [SerializeField] private float chaseSpeed = 12f;
        [SerializeField] private float chargeSpeed = 20f;
        [SerializeField] private float chargeCooldown = 3f;
        [SerializeField] private float chargeWindupTime = 0.8f;
        [SerializeField] private float chargeDuration = 1.5f;
        [SerializeField] private float patrolRadius = 20f;
        [SerializeField] private float rotationSpeed = 180f;
        
        [Header("冲撞设置")]
        [SerializeField] private float chargeDamageMultiplier = 2f;
        [SerializeField] private float chargeKnockbackForce = 15f;
        [SerializeField] private LayerMask obstacleLayers;
        
        // AI状态
        private SharkAIState aiState = SharkAIState.Patrol;
        private Vector3 patrolCenter;
        private Vector3 patrolTarget;
        private float lastChargeTime = -999f;
        private bool isCharging = false;
        private Vector3 chargeDirection;
        
        // 组件引用
        private Rigidbody rb;
        
        private enum SharkAIState
        {
            Patrol,     // 巡逻
            Alert,      // 警觉（发现玩家）
            Chase,      // 追击
            ChargeWindup, // 冲撞蓄力
            Charge,     // 冲撞中
            Recover     // 冲撞后恢复
        }
        
        protected override void Awake()
        {
            base.Awake();
            // 设置机械鲨鱼属性
            maxHealth = 150f;
            currentHealth = maxHealth;
            attackDamage = 20f;
            minSpawnDepth = 60f;
            maxSpawnDepth = 100f;
            
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.drag = 2f;
                rb.angularDrag = 5f;
            }
        }
        
        protected override void InitializeAI()
        {
            patrolCenter = transform.position;
            SetNewPatrolTarget();
        }
        
        protected override void UpdateAI()
        {
            if (player == null) return;
            
            float distanceToPlayer = GetDistanceToPlayer();
            
            switch (aiState)
            {
                case SharkAIState.Patrol:
                    UpdatePatrol();
                    if (distanceToPlayer <= detectionRange)
                    {
                        TransitionToState(SharkAIState.Alert);
                    }
                    break;
                    
                case SharkAIState.Alert:
                    UpdateAlert();
                    if (distanceToPlayer > detectionRange * 1.2f)
                    {
                        TransitionToState(SharkAIState.Patrol);
                    }
                    else if (distanceToPlayer <= detectionRange * 0.8f)
                    {
                        TransitionToState(SharkAIState.Chase);
                    }
                    break;
                    
                case SharkAIState.Chase:
                    UpdateChase();
                    if (distanceToPlayer > detectionRange)
                    {
                        TransitionToState(SharkAIState.Alert);
                    }
                    else if (CanCharge() && distanceToPlayer <= attackRange * 2f)
                    {
                        TransitionToState(SharkAIState.ChargeWindup);
                    }
                    break;
                    
                case SharkAIState.ChargeWindup:
                    // 蓄力状态在协程中处理
                    break;
                    
                case SharkAIState.Charge:
                    UpdateCharge();
                    break;
                    
                case SharkAIState.Recover:
                    UpdateRecover();
                    if (Time.time >= lastChargeTime + chargeCooldown)
                    {
                        TransitionToState(distanceToPlayer <= detectionRange ? SharkAIState.Chase : SharkAIState.Alert);
                    }
                    break;
            }
        }
        
        private void TransitionToState(SharkAIState newState)
        {
            if (aiState == newState) return;
            
            OnExitState(aiState);
            aiState = newState;
            OnEnterState(newState);
        }
        
        private void OnEnterState(SharkAIState state)
        {
            switch (state)
            {
                case SharkAIState.Alert:
                    currentState = EnemyState.Idle;
                    break;
                    
                case SharkAIState.Chase:
                    currentState = EnemyState.Chase;
                    break;
                    
                case SharkAIState.ChargeWindup:
                    currentState = EnemyState.Attack;
                    StartCoroutine(ChargeWindupCoroutine());
                    break;
                    
                case SharkAIState.Charge:
                    StartCharge();
                    break;
                    
                case SharkAIState.Recover:
                    currentState = EnemyState.Idle;
                    isCharging = false;
                    break;
            }
        }
        
        private void OnExitState(SharkAIState state)
        {
            switch (state)
            {
                case SharkAIState.Charge:
                    rb.velocity *= 0.3f;
                    break;
            }
        }
        
        private void UpdatePatrol()
        {
            if (Vector3.Distance(transform.position, patrolTarget) < 2f)
            {
                SetNewPatrolTarget();
            }
            MoveTowards(patrolTarget, patrolSpeed);
        }
        
        private void UpdateAlert()
        {
            Vector3 directionToPlayer = GetDirectionToPlayer();
            RotateTowards(directionToPlayer);
        }
        
        private void UpdateChase()
        {
            Vector3 directionToPlayer = GetDirectionToPlayer();
            MoveTowards(player.position, chaseSpeed);
            RotateTowards(directionToPlayer);
        }
        
        private void UpdateCharge()
        {
            rb.velocity = chargeDirection * chargeSpeed;
            CheckChargeCollision();
        }
        
        private void UpdateRecover()
        {
            if (player != null)
            {
                Vector3 directionToPlayer = GetDirectionToPlayer();
                RotateTowards(directionToPlayer);
            }
        }
        
        private bool CanCharge()
        {
            return Time.time >= lastChargeTime + chargeCooldown;
        }
        
        private IEnumerator ChargeWindupCoroutine()
        {
            Vector3 backDirection = -GetDirectionToPlayer();
            float timer = 0f;
            
            while (timer < chargeWindupTime)
            {
                timer += Time.deltaTime;
                rb.velocity = backDirection * patrolSpeed * 0.5f;
                yield return null;
            }
            
            TransitionToState(SharkAIState.Charge);
        }
        
        private void StartCharge()
        {
            isCharging = true;
            chargeDirection = GetDirectionToPlayer();
            lastChargeTime = Time.time;
            StartCoroutine(ChargeDurationCoroutine());
        }
        
        private IEnumerator ChargeDurationCoroutine()
        {
            yield return new WaitForSeconds(chargeDuration);
            TransitionToState(SharkAIState.Recover);
        }
        
        private void CheckChargeCollision()
        {
            RaycastHit hit;
            float checkDistance = 3f;
            
            if (Physics.Raycast(transform.position, chargeDirection, out hit, checkDistance, obstacleLayers))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    ApplyChargeDamage(hit.collider.gameObject);
                }
                else
                {
                    // 撞到墙壁或其他障碍物，进入恢复状态
                    TransitionToState(SharkAIState.Recover);
                }
            }
        }
        
        private void ApplyChargeDamage(GameObject target)
        {
            var damageable = target.GetComponent<IDamageable>();
            if (damageable != null)
            {
                float chargeDamage = attackDamage * chargeDamageMultiplier;
                damageable.TakeDamage(chargeDamage);
                
                // 击退效果
                Rigidbody playerRb = target.GetComponent<Rigidbody>();
                if (playerRb != null)
                {
                    playerRb.AddForce(chargeDirection * chargeKnockbackForce, ForceMode.Impulse);
                }
            }
            
            TransitionToState(SharkAIState.Recover);
        }
        
        private void SetNewPatrolTarget()
        {
            Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
            patrolTarget = patrolCenter + new Vector3(randomCircle.x, Random.Range(-5f, 5f), randomCircle.y);
        }
        
        private void MoveTowards(Vector3 target, float speed)
        {
            Vector3 direction = (target - transform.position).normalized;
            rb.velocity = direction * speed;
            RotateTowards(direction);
        }
        
        private void RotateTowards(Vector3 direction)
        {
            if (direction == Vector3.zero) return;
            
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        public override void PerformAttack()
        {
            // 冲撞攻击在Charge状态中处理
        }
        
        protected override void OnDeath()
        {
            // 播放死亡动画，掉落零件
            rb.useGravity = true;
            rb.drag = 0.5f;
            Destroy(gameObject, 3f);
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
            
            if (Application.isPlaying)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, patrolTarget);
                Gizmos.DrawWireSphere(patrolTarget, 1f);
            }
        }
    }
    
    public interface IDamageable
    {
        void TakeDamage(float damage);
    }
}
