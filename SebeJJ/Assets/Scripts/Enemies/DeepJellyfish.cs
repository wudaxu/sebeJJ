using UnityEngine;
using System.Collections;
using SebeJJ.Combat;

namespace SebeJJ.Enemies
{
    /// <summary>
    /// 深海水母敌人
    /// </summary>
    public class DeepJellyfish : EnemyBase
    {
        [Header("水母特性")]
        [SerializeField] private float floatAmplitude = 0.5f;
        [SerializeField] private float floatFrequency = 1f;
        [SerializeField] private int tentacleCount = 4;
        [SerializeField] private float tentacleRange = 2f;
        [SerializeField] private float tentacleDamage = 5f;
        [SerializeField] private float glowIntensity = 1f;
        [SerializeField] private Color glowColor = Color.cyan;

        [Header("视觉效果")]
        [SerializeField] private SpriteRenderer glowRenderer;
        [SerializeField] private ParticleSystem glowParticles;
        [SerializeField] private LineRenderer[] tentacleRenderers;

        // 属性
        public float PatrolRadius => patrolRadius;

        private float _floatOffset;
        private Vector2 _basePosition;

        protected override void Awake()
        {
            base.Awake();
            enemyName = "Deep Jellyfish";
            moveSpeed = 2f;
            detectionRange = 8f;
            attackRange = 3f;
            patrolRadius = 8f;
        }

        protected override void Start()
        {
            base.Start();
            _basePosition = transform.position;
            
            // 初始化触手
            InitializeTentacles();
        }

        protected override void InitializeStates()
        {
            _stateMachine.AddState(new PatrolState(this));
            _stateMachine.AddState(new ChaseState(this));
            _stateMachine.AddState(new AttackState(this));
            _stateMachine.AddState(new IdleState(this));

            _stateMachine.ChangeState<PatrolState>();
        }

        private void Update()
        {
            base.Update();
            UpdateFloating();
            UpdateGlow();
        }

        /// <summary>
        /// 初始化触手
        /// </summary>
        private void InitializeTentacles()
        {
            tentacleRenderers = new LineRenderer[tentacleCount];
            
            for (int i = 0; i < tentacleCount; i++)
            {
                GameObject tentacleObj = new GameObject($"Tentacle_{i}");
                tentacleObj.transform.SetParent(transform);
                
                LineRenderer lr = tentacleObj.AddComponent<LineRenderer>();
                lr.startWidth = 0.1f;
                lr.endWidth = 0.05f;
                lr.positionCount = 5;
                lr.useWorldSpace = true;
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.startColor = glowColor;
                lr.endColor = new Color(glowColor.r, glowColor.g, glowColor.b, 0.3f);
                
                tentacleRenderers[i] = lr;
            }
        }

        /// <summary>
        /// 更新漂浮效果
        /// </summary>
        private void UpdateFloating()
        {
            _floatOffset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            
            // 应用漂浮到视觉
            if (visualTransform != null)
            {
                visualTransform.localPosition = new Vector3(0, _floatOffset, 0);
            }

            // 更新触手
            UpdateTentacles();
        }

        /// <summary>
        /// 更新触手动画
        /// </summary>
        private void UpdateTentacles()
        {
            if (tentacleRenderers == null) return;

            for (int i = 0; i < tentacleCount; i++)
            {
                if (tentacleRenderers[i] == null) continue;

                float angle = (360f / tentacleCount * i + Time.time * 30f) * Mathf.Deg2Rad;
                Vector2 basePos = transform.position;
                
                for (int j = 0; j < 5; j++)
                {
                    float t = j / 4f;
                    float wave = Mathf.Sin(Time.time * 3f + i + j * 0.5f) * 0.3f;
                    
                    Vector2 pos = basePos + new Vector2(
                        Mathf.Cos(angle) * tentacleRange * t + wave,
                        Mathf.Sin(angle) * tentacleRange * t - t * 0.5f + _floatOffset
                    );
                    
                    tentacleRenderers[i].SetPosition(j, pos);
                }
            }
        }

        /// <summary>
        /// 更新发光效果
        /// </summary>
        private void UpdateGlow()
        {
            if (glowRenderer != null)
            {
                float pulse = 0.8f + Mathf.Sin(Time.time * 2f) * 0.2f;
                glowRenderer.color = glowColor * glowIntensity * pulse;
            }

            if (glowParticles != null)
            {
                var emission = glowParticles.emission;
                emission.rateOverTime = 5f + Mathf.Sin(Time.time) * 2f;
            }
        }

        /// <summary>
        /// 执行攻击
        /// </summary>
        protected override void PerformAttack()
        {
            // 触手攻击
            if (target != null)
            {
                float distance = Vector2.Distance(transform.position, target.position);
                
                if (distance <= tentacleRange)
                {
                    // 对玩家造成伤害
                    if (target.TryGetComponent<IDamageable>(out var damageable))
                    {
                        DamageInfo damageInfo = new DamageInfo(
                            tentacleDamage,
                            DamageType.Corrosive,
                            (target.position - transform.position).normalized,
                            gameObject
                        );
                        damageable.TakeDamage(damageInfo);
                    }

                    // 播放攻击动画
                    StartCoroutine(TentacleAttackAnimation());
                }
            }
        }

        private IEnumerator TentacleAttackAnimation()
        {
            // 触手攻击动画
            float originalRange = tentacleRange;
            tentacleRange *= 1.5f;
            
            yield return new WaitForSeconds(0.3f);
            
            tentacleRange = originalRange;
        }

        /// <summary>
        /// 受到伤害时发光增强
        /// </summary>
        public override void TakeDamage(DamageInfo damageInfo)
        {
            base.TakeDamage(damageInfo);
            
            // 受伤时闪烁
            StartCoroutine(DamageFlash());
        }

        private IEnumerator DamageFlash()
        {
            if (glowRenderer != null)
            {
                Color originalColor = glowRenderer.color;
                glowRenderer.color = Color.white;
                
                yield return new WaitForSeconds(0.1f);
                
                glowRenderer.color = originalColor;
            }
        }

        protected override void OnDropItems()
        {
            // 掉落生物样本
            // TODO: 实例化掉落物
        }

        protected override void OnDestroy()
        {
            // 清理触手
            if (tentacleRenderers != null)
            {
                foreach (var tentacle in tentacleRenderers)
                {
                    if (tentacle != null)
                    {
                        Destroy(tentacle.gameObject);
                    }
                }
            }
            
            base.OnDestroy();
        }
    }
}
