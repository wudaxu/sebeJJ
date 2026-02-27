using UnityEngine;
using System.Collections.Generic;

namespace SebeJJ.Utils
{
    /// <summary>
    /// 特效管理器 - 性能优化版本
    /// 优化点:
    /// 1. 使用对象池复用特效
    /// 2. 静态Gradient避免GC
    /// 3. 缓存组件引用
    /// 4. 批量特效处理
    /// </summary>
    public class EffectManagerOptimized : MonoBehaviour
    {
        public static EffectManagerOptimized Instance { get; private set; }
        
        [Header("特效预制体")]
        public GameObject collectEffectPrefab;
        public GameObject collectCompleteEffectPrefab;
        public GameObject scanEffectPrefab;
        public GameObject boostEffectPrefab;
        public GameObject damageEffectPrefab;
        public GameObject explosionEffectPrefab;
        public GameObject bubbleEffectPrefab;
        public GameObject pressureWarningEffectPrefab;
        
        [Header("UI特效")]
        public GameObject notificationEffectPrefab;
        public GameObject missionCompleteEffectPrefab;
        
        [Header("对象池设置")]
        public int defaultPoolSize = 10;
        public int maxPoolSize = 30;
        public Transform effectContainer;
        
        // 对象池字典
        private Dictionary<string, GameObjectPool> effectPools;
        private Dictionary<string, GameObject> effectPrefabs;
        
        // 静态Gradient避免每帧创建
        private static readonly Gradient CollectGradient = new Gradient();
        private static readonly Gradient DamageGradient = new Gradient();
        private static readonly Gradient DefaultGradient = new Gradient();
        
        // 静态构造函数初始化Gradient
        static EffectManagerOptimized()
        {
            CollectGradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.cyan, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            
            DamageGradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.red, 0f), new GradientColorKey(Color.yellow, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            
            DefaultGradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.gray, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
        }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            InitializePools();
        }
        
        private void InitializePools()
        {
            effectPools = new Dictionary<string, GameObjectPool>();
            effectPrefabs = new Dictionary<string, GameObject>
            {
                { "collect", collectEffectPrefab },
                { "collect_complete", collectCompleteEffectPrefab },
                { "scan", scanEffectPrefab },
                { "boost", boostEffectPrefab },
                { "damage", damageEffectPrefab },
                { "explosion", explosionEffectPrefab },
                { "bubble", bubbleEffectPrefab },
                { "pressure_warning", pressureWarningEffectPrefab },
                { "notification", notificationEffectPrefab },
                { "mission_complete", missionCompleteEffectPrefab }
            };
            
            if (effectContainer == null)
            {
                effectContainer = new GameObject("Effects").transform;
                effectContainer.SetParent(transform);
            }
            
            // 为每个特效创建对象池
            foreach (var kvp in effectPrefabs)
            {
                if (kvp.Value != null)
                {
                    Transform container = new GameObject($"Pool_{kvp.Key}").transform;
                    container.SetParent(effectContainer);
                    
                    var pool = new GameObjectPool(kvp.Value, defaultPoolSize, maxPoolSize, container);
                    effectPools[kvp.Key] = pool;
                }
            }
        }
        
        public void PlayCollectEffect(Vector3 position)
        {
            SpawnEffect("collect", position);
            PlayBubbleEffect(position, 5);
        }
        
        public void PlayCollectCompleteEffect(Vector3 position)
        {
            SpawnEffect("collect_complete", position);
            AudioManager.Instance?.PlaySFX(GetAudioClip("collect_success"));
        }
        
        public void PlayScanEffect(Vector3 position, float range)
        {
            var effect = SpawnEffect("scan", position);
            if (effect != null)
            {
                var particles = effect.GetComponent<ParticleSystem>();
                if (particles != null)
                {
                    var shape = particles.shape;
                    shape.radius = range;
                }
            }
            
            AudioManager.Instance?.PlaySFX(GetAudioClip("scan"));
        }
        
        public void PlayBoostEffect(Vector3 position, Vector3 direction)
        {
            var effect = SpawnEffect("boost", position);
            if (effect != null)
            {
                effect.transform.rotation = Quaternion.LookRotation(direction);
            }
        }
        
        public void PlayDamageEffect(Vector3 position)
        {
            SpawnEffect("damage", position);
            CameraShake?.Invoke(0.3f, 0.2f);
        }
        
        public void PlayExplosionEffect(Vector3 position, float scale = 1f)
        {
            var effect = SpawnEffect("explosion", position);
            if (effect != null)
            {
                effect.transform.localScale = Vector3.one * scale;
            }
            
            AudioManager.Instance?.PlaySFX(GetAudioClip("explosion"));
            CameraShake?.Invoke(0.5f * scale, 0.3f);
        }
        
        public void PlayBubbleEffect(Vector3 position, int count = 1)
        {
            // 批量生成气泡，减少函数调用开销
            if (!effectPools.TryGetValue("bubble", out var pool)) return;
            
            for (int i = 0; i < count; i++)
            {
                Vector3 offset = Random.insideUnitSphere * 0.5f;
                GameObject effect = pool.Get();
                if (effect != null)
                {
                    effect.transform.position = position + offset;
                    // 自动回收
                    StartCoroutine(ReturnToPoolAfterDelay(pool, effect, 2f));
                }
            }
        }
        
        public void PlayPressureWarningEffect(Vector3 position)
        {
            SpawnEffect("pressure_warning", position);
        }
        
        public void PlayMissionCompleteEffect()
        {
            SpawnUIEffect("mission_complete");
            AudioManager.Instance?.PlaySFX(GetAudioClip("mission_complete"));
        }
        
        /// <summary>
        /// 从对象池生成特效
        /// </summary>
        private GameObject SpawnEffect(string effectName, Vector3 position)
        {
            if (!effectPools.TryGetValue(effectName, out var pool))
            {
                return CreateDefaultEffect(effectName, position);
            }
            
            GameObject effect = pool.Get();
            if (effect == null) return null;
            
            effect.transform.position = position;
            effect.transform.rotation = Quaternion.identity;
            
            // 设置自动销毁
            var autoDestroy = effect.GetComponent<EffectAutoDestroyOptimized>();
            if (autoDestroy == null)
            {
                autoDestroy = effect.AddComponent<EffectAutoDestroyOptimized>();
            }
            
            var particles = effect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                float duration = particles.main.duration + particles.main.startLifetime.constantMax;
                autoDestroy.Initialize(pool, duration);
            }
            else
            {
                autoDestroy.Initialize(pool, 2f);
            }
            
            return effect;
        }
        
        private void SpawnUIEffect(string effectName)
        {
            if (!effectPools.TryGetValue(effectName, out var pool)) return;
            
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                GameObject effect = pool.Get();
                if (effect != null)
                {
                    effect.transform.SetParent(canvas.transform);
                    effect.transform.localPosition = Vector3.zero;
                    StartCoroutine(ReturnToPoolAfterDelay(pool, effect, 3f));
                }
            }
        }
        
        /// <summary>
        /// 创建默认特效（当预制体不存在时）
        /// 使用静态Gradient避免GC
        /// </summary>
        private GameObject CreateDefaultEffect(string effectName, Vector3 position)
        {
            GameObject effect = new GameObject($"Effect_{effectName}");
            effect.transform.position = position;
            
            var particles = effect.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.duration = 1f;
            main.startLifetime = 0.5f;
            main.startSize = 0.5f;
            main.startSpeed = 2f;
            main.maxParticles = 20;
            
            var emission = particles.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 10) });
            
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.5f;
            
            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            
            // 使用静态Gradient
            switch (effectName)
            {
                case "collect":
                    colorOverLifetime.color = CollectGradient;
                    break;
                case "damage":
                    colorOverLifetime.color = DamageGradient;
                    break;
                default:
                    colorOverLifetime.color = DefaultGradient;
                    break;
            }
            
            Destroy(effect, 2f);
            
            return effect;
        }
        
        private System.Collections.IEnumerator ReturnToPoolAfterDelay(GameObjectPool pool, GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (pool != null && obj != null)
            {
                pool.Return(obj);
            }
        }
        
        private AudioClip GetAudioClip(string clipName)
        {
            return AudioManager.Instance?.GetClip(clipName);
        }
        
        public static System.Action<float, float> CameraShake;
    }
    
    /// <summary>
    /// 优化的特效自动销毁组件 - 使用对象池
    /// </summary>
    public class EffectAutoDestroyOptimized : MonoBehaviour
    {
        private GameObjectPool pool;
        private float lifetime;
        private float timer;
        private ParticleSystem[] particleSystems;
        private bool isInitialized = false;
        
        public void Initialize(GameObjectPool ownerPool, float lifeTime)
        {
            pool = ownerPool;
            lifetime = lifeTime;
            timer = 0f;
            isInitialized = true;
            
            if (particleSystems == null)
            {
                particleSystems = GetComponentsInChildren<ParticleSystem>();
            }
            
            // 重启所有粒子
            foreach (var ps in particleSystems)
            {
                if (ps != null)
                {
                    ps.Clear();
                    ps.Play();
                }
            }
        }
        
        private void Update()
        {
            if (!isInitialized) return;
            
            timer += Time.deltaTime;
            
            // 检查所有粒子是否已停止
            bool allParticlesStopped = true;
            foreach (var ps in particleSystems)
            {
                if (ps != null && ps.IsAlive())
                {
                    allParticlesStopped = false;
                    break;
                }
            }
            
            if (timer >= lifetime || allParticlesStopped)
            {
                ReturnToPool();
            }
        }
        
        private void ReturnToPool()
        {
            if (pool != null)
            {
                // 停止所有粒子
                foreach (var ps in particleSystems)
                {
                    if (ps != null)
                    {
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    }
                }
                
                pool.Return(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void OnDestroy()
        {
            if (particleSystems != null)
            {
                foreach (var ps in particleSystems)
                {
                    if (ps != null)
                    {
                        ps.Clear();
                    }
                }
            }
        }
    }
}
