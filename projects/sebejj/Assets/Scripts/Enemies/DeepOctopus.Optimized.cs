using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SebeJJ.Enemies
{
    /// <summary>
    /// 深海章鱼 - 性能优化版本
    /// 优化点:
    /// 1. 降低浮空动画更新频率
    /// 2. 缓存三角函数计算
    /// 3. 复用List避免GC
    /// 4. 优化触手动画
    /// </summary>
    public class DeepOctopusOptimized : EnemyBaseOptimized
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
        
        [Header("性能设置")]
        [SerializeField] private float floatUpdateRate = 0.05f; // 20fps更新浮空动画
        
        // AI状态
        private OctopusAIState aiState = OctopusAIState.Floating;
        private float lastTentacleAttackTime = -999f;
        private float lastInkTime = -999f;
        private int inkUsesRemaining;
        private Vector3 floatCenter;
        
        // 性能优化：缓存浮空位置
        private float lastFloatUpdate;
        private Vector3 cachedFloatOffset;
        private float floatTime;
        
        // 性能优化：复用List
        private static readonly List<Transform> TentaclesBuffer = new List<Transform>(16);
        private List<Transform> tentacles = new List<Transform>();
        
        // 组件
        private Rigidbody rb;
        
        private enum OctopusAIState
        {
            Floating,
            TentacleAttack,
            InkEscape,
            Fleeing
        }
        
        protected override void Awake()
        {
            base.Awake();
            
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
            tentacles.Clear();
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
        
        protected override void UpdateAI(ref EnemyContext context)
        {
            floatTime += context.DeltaTime;
            
            switch (aiState)
            {
                case OctopusAIState.Floating:
                    UpdateFloatingOptimized();
                    
                    if (context.IsPlayerInAttackRange && CanTentacleAttack())
                    {
                        TransitionToState(OctopusAIState.TentacleAttack);
                    }
                    else if (ShouldUseInk(context.SqrDistanceToPlayer))
                    {
                        TransitionToState(OctopusAIState.InkEscape);
                    }
                    break;
                    
                case OctopusAIState.TentacleAttack:
                    // 在协程中处理
                    break;
                    
                case OctopusAIState.InkEscape:
                    // 在协程中处理
                    break;
                    
                case OctopusAIState.Fleeing:
                    UpdateFleeing();
                    if (!context.IsPlayerInDetectionRange)
                    {
                        TransitionToState(OctopusAIState.Floating);
                    }
                    break;
            }
        }
        
        /// <summary>
        /// 优化的浮空动画 - 降低更新频率
        /// </summary>
        private void UpdateFloatingOptimized()
        {
            float currentTime = Time.time;
            
            // 只在间隔时间更新计算
            if (currentTime - lastFloatUpdate >= floatUpdateRate)
            {
                lastFloatUpdate = currentTime;
                cachedFloatOffset = CalculateFloatOffset();
            }
            
            // 使用缓存的位置
            Vector3 targetPosition = floatCenter + cachedFloatOffset;
            Vector3 direction = (targetPosition - transform.position).normalized;
            rb.velocity = direction * floatSpeed;
            
            // 缓慢朝向玩家
            if (player != null)
            {
                Vector3 lookDirection = GetDirectionToPlayer();
                lookDirection.y = 0;
                RotateTowards(lookDirection);
            }
        }
        
        /// <summary>
        /// 计算浮空偏移 - 集中数学运算
        /// </summary>
        private Vector3 CalculateFloatOffset()
        {
            float freq1 = floatTime * floatFrequency;
            float freq2 = freq1 * 1.5f;
            float freq3 = freq1 * 0.7f;
            
            return new Vector3(
                Mathf.Sin(freq1) * 0.5f,
                Mathf.Sin(freq2) * floatAmplitude,
                Mathf.Cos(freq3) * 0.5f
            );
        }
        
        private void UpdateFleeing()
        {
            Vector3 fleeDirection = -GetDirectionToPlayer();
            fleeDirection.y = Random.Range(-0.3f, 0.3f);
            fleeDirection.Normalize();
            
            rb.velocity = fleeDirection * (floatSpeed * 2f);
            RotateTowards(fleeDirection);
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
        
        private bool CanTentacleAttack()
        {
            return Time.time >= lastTentacleAttackTime + tentacleAttackCooldown;
        }
        
        private IEnumerator TentacleAttackCoroutine()
        {
            lastTentacleAttackTime = Time.time;
            
            yield return StartCoroutine(TentacleWindup());
            yield return StartCoroutine(TentacleSweep());
            yield return new WaitForSeconds(0.5f);
            
            TransitionToState(OctopusAIState.Floating);
        }
        
        private IEnumerator TentacleWindup()
        {
            float windupTime = 0.5f;
            float timer = 0f;
            
            // 复制到静态缓冲区避免GC
            TentaclesBuffer.Clear();
            TentaclesBuffer.AddRange(tentacles);
            
            while (timer < windupTime)
            {
                timer += Time.deltaTime;
                float t = timer / windupTime;
                
                foreach (var tentacle in TentaclesBuffer)
                {
                    if (tentacle != null)
                    {
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
            
            TentaclesBuffer.Clear();
            TentaclesBuffer.AddRange(tentacles);
            int tentacleCountLocal = TentaclesBuffer.Count;
            
            while (attackTimer < tentacleAttackDuration)
            {
                attackTimer += Time.deltaTime;
                float t = attackTimer / tentacleAttackDuration;
                
                float sweepAngle = Mathf.Sin(t * Mathf.PI) * tentacleSweepAngle;
                
                for (int i = 0; i < tentacleCountLocal; i++)
                {
                    var tentacle = TentaclesBuffer[i];
                    if (tentacle != null)
                    {
                        float baseAngle = (360f / tentacleCount) * i;
                        tentacle.localRotation = Quaternion.Euler(sweepAngle, baseAngle, 0);
                    }
                }
                
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
            TentaclesBuffer.Clear();
            TentaclesBuffer.AddRange(tentacles);
            
            foreach (var tentacle in TentaclesBuffer)
            {
                if (tentacle != null)
                {
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
        
        private bool ShouldUseInk(float sqrDistanceToPlayer)
        {
            if (inkUsesRemaining <= 0) return false;
            if (Time.time < lastInkTime + inkCooldown) return false;
            
            bool lowHealth = currentHealth / maxHealth < 0.4f;
            bool playerTooClose = sqrDistanceToPlayer < inkEscapeRange * inkEscapeRange * 0.25f;
            
            return lowHealth || playerTooClose;
        }
        
        private IEnumerator InkEscapeCoroutine()
        {
            lastInkTime = Time.time;
            inkUsesRemaining--;
            
            ReleaseInkCloud();
            yield return new WaitForSeconds(0.3f);
            
            TransitionToState(OctopusAIState.Fleeing);
        }
        
        private void ReleaseInkCloud()
        {
            Vector3 inkPosition = transform.position - GetDirectionToPlayer() * 3f;
            
            if (inkCloudPrefab != null)
            {
                GameObject inkCloud = Instantiate(inkCloudPrefab, inkPosition, Quaternion.identity);
                
                InkCloud ink = inkCloud.GetComponent<InkCloud>();
                if (ink != null)
                {
                    ink.Initialize(inkCloudDuration, inkBlindDuration, inkCloudSize);
                }
                else
                {
                    inkCloud.transform.localScale = Vector3.one * inkCloudSize;
                    Destroy(inkCloud, inkCloudDuration);
                }
            }
            
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
        
        private void RotateTowards(Vector3 direction)
        {
            if (direction == Vector3.zero) return;
            
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 120f * Time.deltaTime);
        }
        
        public override void PerformAttack()
        {
            // 由AI状态机管理
        }
        
        protected override void OnDeath()
        {
            ReleaseInkCloud();
            
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
    }
}
