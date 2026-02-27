using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SebeJJ.Enemies
{
    /// <summary>
    /// 深海章鱼 - 触手攻击与墨汁喷射的敌人
    /// 出现深度：80-100米
    /// </summary>
    public class DeepOctopus : EnemyBase
    {
        [Header("深海章鱼特性")]
        [SerializeField] private float floatSpeed = 2f;
        [SerializeField] private float tentacleAttackRange = 8f;
        [SerializeField] private float inkEscapeRange = 15f;
        [SerializeField] private float floatAmplitude = 1f;
        [SerializeField] private float floatFrequency = 0.5f;
        
        [Header("触手攻击")]
        [SerializeField] private int tentacleCount = 8;
        [SerializeField] private float tentacleDamage = 15f;
        [SerializeField] private float tentacleAttackCooldown = 2f;
        [SerializeField] private float tentacleAttackDuration = 1.5f;
        [SerializeField] private float tentacleSweepAngle = 120f;
        
        [Header("墨汁喷射")]
        [SerializeField] private float inkCooldown = 8f;
        [SerializeField] private float inkBlindDuration = 3f;
        [SerializeField] private float inkCloudDuration = 5f;
        [SerializeField] private float inkCloudSize = 10f;
        [SerializeField] private GameObject inkCloudPrefab;
        [SerializeField] private int maxInkUses = 3;
        
        // AI状态
        private OctopusAIState aiState = OctopusAIState.Floating;
        private float lastTentacleAttackTime = -999f;
        private float lastInkTime = -999f;
        private int inkUsesRemaining;
        private Vector3 floatCenter;
        private float floatTime;
        private List<Transform> tentacles = new List<Transform>();
        
        // 组件引用
        private Rigidbody rb;
        
        private enum OctopusAIState
        {
            Floating,       // 漂浮
            TentacleAttack, // 触手攻击
            InkEscape,      // 墨汁逃跑
            Fleeing         // 逃跑中
        }
        
        protected override void Awake()
        {
            base.Awake();
            // 设置深海章鱼属性
            maxHealth = 200f;
            currentHealth = maxHealth;
            attackDamage = tentacleDamage;
            minSpawnDepth = 80f;
            maxSpawnDepth = 100f;
            detectionRange = 20f;
            attackRange = tentacleAttackRange;
            
            inkUsesRemaining = maxInkUses;
            
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.drag = 3f;
                rb.angularDrag = 8f;
            }
        }
        
        protected override void Start()
        {
            base.Start();
            InitializeTentacles();
        }
        
        private void InitializeTentacles()
        {
            // 查找或创建触手
            Transform tentaclesParent = transform.Find("Tentacles");
            if (tentaclesParent != null)
            {
                foreach (Transform tentacle in tentaclesParent)
                {
                    tentacles.Add(tentacle);
                }
            }
        }
        
        protected override void InitializeAI()
        {
            floatCenter = transform.position;
        }
        
        protected override void UpdateAI()
        {
            if (player == null) return;
            
            float distanceToPlayer = GetDistanceToPlayer();
            floatTime += Time.deltaTime;
            
            switch (aiState)
            {
                case OctopusAIState.Floating:
                    UpdateFloating();
                    
                    // 状态转换判断
                    if (distanceToPlayer <= tentacleAttackRange && CanTentacleAttack())
                    {
                        TransitionToState(OctopusAIState.TentacleAttack);
                    }
                    else if (ShouldUseInk(distanceToPlayer))
                    {
                        TransitionToState(OctopusAIState.InkEscape);
                    }
                    break;
                    
                case OctopusAIState.TentacleAttack:
                    // 攻击状态在协程中处理
                    break;
                    
                case OctopusAIState.InkEscape:
                    // 墨汁逃跑在协程中处理
                    break;
                    
                case OctopusAIState.Fleeing:
                    UpdateFleeing();
                    if (distanceToPlayer >= inkEscapeRange * 1.5f)
                    {
                        TransitionToState(OctopusAIState.Floating);
                    }
                    break;
            }
        }
        
        private void TransitionToState(OctopusAIState newState)
        {
            if (aiState == newState) return;
            
            OnExitState(aiState);
            aiState = newState;
            OnEnterState(newState);
        }
        
        private void OnEnterState(OctopusAIState state)
        {
            switch (state)
            {
                case OctopusAIState.Floating:
                    currentState = EnemyState.Idle;
                    floatCenter = transform.position;
                    break;
                    
                case OctopusAIState.TentacleAttack:
                    currentState = EnemyState.Attack;
                    StartCoroutine(TentacleAttackCoroutine());
                    break;
                    
                case OctopusAIState.InkEscape:
                    currentState = EnemyState.Flee;
                    StartCoroutine(InkEscapeCoroutine());
                    break;
                    
                case OctopusAIState.Fleeing:
                    currentState = EnemyState.Flee;
                    break;
            }
        }
        
        private void OnExitState(OctopusAIState state)
        {
            switch (state)
            {
                case OctopusAIState.TentacleAttack:
                    StopAllTentacleAnimations();
                    break;
            }
        }
        
        #region 状态更新
        
        private void UpdateFloating()
        {
            // 漂浮动画 - 上下浮动
            Vector3 floatOffset = new Vector3(
                Mathf.Sin(floatTime * floatFrequency) * 0.5f,
                Mathf.Sin(floatTime * floatFrequency * 1.5f) * floatAmplitude,
                Mathf.Cos(floatTime * floatFrequency * 0.7f) * 0.5f
            );
            
            Vector3 targetPosition = floatCenter + floatOffset;
            Vector3 direction = (targetPosition - transform.position).normalized;
            rb.velocity = direction * floatSpeed;
            
            // 缓慢朝向玩家
            if (player != null)
            {
                Vector3 lookDirection = GetDirectionToPlayer();
                lookDirection.y = 0; // 保持水平
                RotateTowards(lookDirection);
            }
        }
        
        private void UpdateFleeing()
        {
            // 向远离玩家的方向逃跑
            Vector3 fleeDirection = -GetDirectionToPlayer();
            fleeDirection.y = Random.Range(-0.3f, 0.3f); // 添加一些垂直变化
            fleeDirection.Normalize();
            
            rb.velocity = fleeDirection * (floatSpeed * 2f);
            RotateTowards(fleeDirection);
        }
        
        #endregion
        
        #region 触手攻击
        
        private bool CanTentacleAttack()
        {
            return Time.time >= lastTentacleAttackTime + tentacleAttackCooldown;
        }
        
        private IEnumerator TentacleAttackCoroutine()
        {
            lastTentacleAttackTime = Time.time;
            
            // 预警阶段 - 触手收缩准备
            yield return StartCoroutine(TentacleWindup());
            
            // 攻击阶段 - 触手扫击
            yield return StartCoroutine(TentacleSweep());
            
            // 恢复阶段
            yield return new WaitForSeconds(0.5f);
            
            TransitionToState(OctopusAIState.Floating);
        }
        
        private IEnumerator TentacleWindup()
        {
            float windupTime = 0.5f;
            float timer = 0f;
            
            // 触手收缩动画
            while (timer < windupTime)
            {
                timer += Time.deltaTime;
                float t = timer / windupTime;
                
                foreach (var tentacle in tentacles)
                {
                    if (tentacle != null)
                    {
                        // 触手向后收缩
                        tentacle.localRotation = Quaternion.Euler(-30f * t, 0, 0);
                    }
                }
                yield return null;
            }
        }
        
        private IEnumerator TentacleSweep()
        {
            float attackTimer = 0f;
            bool hasDealtDamage = false;
            
            while (attackTimer < tentacleAttackDuration)
            {
                attackTimer += Time.deltaTime;
                float t = attackTimer / tentacleAttackDuration;
                
                // 触手扫击动画
                float sweepAngle = Mathf.Sin(t * Mathf.PI) * tentacleSweepAngle;
                foreach (var tentacle in tentacles)
                {
                    if (tentacle != null)
                    {
                        float baseAngle = (360f / tentacleCount) * tentacles.IndexOf(tentacle);
                        tentacle.localRotation = Quaternion.Euler(sweepAngle, baseAngle, 0);
                    }
                }
                
                // 在攻击中段造成伤害
                if (!hasDealtDamage && t > 0.3f && t < 0.7f)
                {
                    DealTentacleDamage();
                    hasDealtDamage = true;
                }
                
                yield return null;
            }
            
            StopAllTentacleAnimations();
        }
        
        private void DealTentacleDamage()
        {
            // 检测范围内的玩家
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, tentacleAttackRange);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    var damageable = hitCollider.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        damageable.TakeDamage(tentacleDamage);
                    }
                }
            }
        }
        
        private void StopAllTentacleAnimations()
        {
            foreach (var tentacle in tentacles)
            {
                if (tentacle != null)
                {
                    // 重置触手位置
                    StartCoroutine(ResetTentacle(tentacle));
                }
            }
        }
        
        private IEnumerator ResetTentacle(Transform tentacle)
        {
            Quaternion startRot = tentacle.localRotation;
            Quaternion targetRot = Quaternion.identity;
            float timer = 0f;
            float resetTime = 0.5f;
            
            while (timer < resetTime)
            {
                timer += Time.deltaTime;
                tentacle.localRotation = Quaternion.Slerp(startRot, targetRot, timer / resetTime);
                yield return null;
            }
        }
        
        #endregion
        
        #region 墨汁逃跑
        
        private bool ShouldUseInk(float distanceToPlayer)
        {
            if (inkUsesRemaining <= 0) return false;
            if (Time.time < lastInkTime + inkCooldown) return false;
            
            // 生命值低或玩家太近时使用墨汁
            bool lowHealth = currentHealth / maxHealth < 0.4f;
            bool playerTooClose = distanceToPlayer < inkEscapeRange * 0.5f;
            
            return lowHealth || playerTooClose;
        }
        
        private IEnumerator InkEscapeCoroutine()
        {
            lastInkTime = Time.time;
            inkUsesRemaining--;
            
            // 喷射墨汁
            ReleaseInkCloud();
            
            // 播放墨汁特效
            yield return new WaitForSeconds(0.3f);
            
            // 快速逃跑
            TransitionToState(OctopusAIState.Fleeing);
        }
        
        private void ReleaseInkCloud()
        {
            // 在身后创建墨汁云
            Vector3 inkPosition = transform.position - GetDirectionToPlayer() * 3f;
            
            if (inkCloudPrefab != null)
            {
                GameObject inkCloud = Instantiate(inkCloudPrefab, inkPosition, Quaternion.identity);
                
                // 设置墨汁云参数
                InkCloud ink = inkCloud.GetComponent<InkCloud>();
                if (ink != null)
                {
                    ink.Initialize(inkCloudDuration, inkBlindDuration, inkCloudSize);
                }
                else
                {
                    // 如果没有InkCloud组件，简单缩放
                    inkCloud.transform.localScale = Vector3.one * inkCloudSize;
                    Destroy(inkCloud, inkCloudDuration);
                }
            }
            
            // 对范围内的玩家施加致盲效果
            ApplyBlindEffect();
        }
        
        private void ApplyBlindEffect()
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, inkCloudSize);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    var blindable = hitCollider.GetComponent<IBlindable>();
                    if (blindable != null)
                    {
                        blindable.ApplyBlind(inkBlindDuration);
                    }
                }
            }
        }
        
        #endregion
        
        private void RotateTowards(Vector3 direction)
        {
            if (direction == Vector3.zero) return;
            
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 120f * Time.deltaTime);
        }
        
        public override void PerformAttack()
        {
            // 触手攻击由AI状态机管理
        }
        
        protected override void OnDeath()
        {
            // 播放墨汁爆炸效果
            ReleaseInkCloud();
            
            // 触手无力下垂
            foreach (var tentacle in tentacles)
            {
                if (tentacle != null)
                {
                    Rigidbody tentacleRb = tentacle.GetComponent<Rigidbody>();
                    if (tentacleRb == null)
                    {
                        tentacleRb = tentacle.gameObject.AddComponent<Rigidbody>();
                    }
                    tentacleRb.useGravity = true;
                }
            }
            
            rb.useGravity = true;
            Destroy(gameObject, 4f);
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
            
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, tentacleAttackRange);
            
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(transform.position, inkEscapeRange);
        }
    }
    
    /// <summary>
    /// 墨汁云效果组件
    /// </summary>
    public class InkCloud : MonoBehaviour
    {
        private float duration;
        private float blindDuration;
        private float maxSize;
        private float currentSize = 0f;
        private float expandTime = 0.5f;
        private float timer = 0f;
        
        private ParticleSystem particles;
        private Collider cloudCollider;
        
        public void Initialize(float cloudDuration, float blindTime, float size)
        {
            duration = cloudDuration;
            blindDuration = blindTime;
            maxSize = size;
            
            // 创建球形碰撞体
            cloudCollider = gameObject.AddComponent<SphereCollider>();
            ((SphereCollider)cloudCollider).isTrigger = true;
            
            // 获取或创建粒子系统
            particles = GetComponent<ParticleSystem>();
        }
        
        private void Update()
        {
            timer += Time.deltaTime;
            
            // 扩散阶段
            if (timer < expandTime)
            {
                currentSize = Mathf.Lerp(0f, maxSize, timer / expandTime);
                transform.localScale = Vector3.one * currentSize;
                
                if (cloudCollider is SphereCollider sphere)
                {
                    sphere.radius = currentSize * 0.5f;
                }
            }
            // 消散阶段
            else if (timer > duration - 1f)
            {
                float fadeT = (timer - (duration - 1f)) / 1f;
                float fadeSize = Mathf.Lerp(maxSize, 0f, fadeT);
                transform.localScale = Vector3.one * fadeSize;
            }
            
            // 结束销毁
            if (timer >= duration)
            {
                Destroy(gameObject);
            }
        }
        
        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var blindable = other.GetComponent<IBlindable>();
                if (blindable != null && !blindable.IsBlinded)
                {
                    blindable.ApplyBlind(blindDuration);
                }
            }
        }
    }
    
    /// <summary>
    /// 致盲效果接口
    /// </summary>
    public interface IBlindable
    {
        bool IsBlinded { get; }
        void ApplyBlind(float duration);
        void RemoveBlind();
    }
}
