using UnityEngine;
using System.Collections.Generic;

namespace SebeJJ.Utils
{
    /// <summary>
    /// 特效管理器 - 管理所有视觉特效
    /// </summary>
    public class EffectManager : MonoBehaviour
    {
        public static EffectManager Instance { get; private set; }
        
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
        public int poolSize = 10;
        public Transform effectContainer;
        
        private Dictionary<string, ObjectPool<GameObject>> effectPools;
        private Dictionary<string, GameObject> effectPrefabs;
        
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
            effectPools = new Dictionary<string, ObjectPool<GameObject>>();
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
            
            // 为每个特效创建对象池
            foreach (var kvp in effectPrefabs)
            {
                if (kvp.Value != null)
                {
                    // 使用简单的对象池实现
                    // 实际项目中可以使用更复杂的对象池系统
                }
            }
        }
        
        /// <summary>
        /// 播放采集特效
        /// </summary>
        public void PlayCollectEffect(Vector3 position)
        {
            SpawnEffect("collect", position);
            
            // 播放气泡效果
            PlayBubbleEffect(position, 5);
        }
        
        /// <summary>
        /// 播放采集完成特效
        /// </summary>
        public void PlayCollectCompleteEffect(Vector3 position)
        {
            SpawnEffect("collect_complete", position);
            
            // 播放音效
            AudioManager.Instance?.PlaySFX(GetAudioClip("collect_success"));
        }
        
        /// <summary>
        /// 播放扫描特效
        /// </summary>
        public void PlayScanEffect(Vector3 position, float range)
        {
            var effect = SpawnEffect("scan", position);
            if (effect != null)
            {
                // 设置扫描范围
                var particles = effect.GetComponent<ParticleSystem>();
                if (particles != null)
                {
                    var shape = particles.shape;
                    shape.radius = range;
                }
            }
            
            AudioManager.Instance?.PlaySFX(GetAudioClip("scan"));
        }
        
        /// <summary>
        /// 播放推进器特效
        /// </summary>
        public void PlayBoostEffect(Vector3 position, Vector3 direction)
        {
            var effect = SpawnEffect("boost", position);
            if (effect != null)
            {
                effect.transform.rotation = Quaternion.LookRotation(direction);
            }
        }
        
        /// <summary>
        /// 播放受伤特效
        /// </summary>
        public void PlayDamageEffect(Vector3 position)
        {
            SpawnEffect("damage", position);
            
            // 屏幕震动
            CameraShake?.Invoke(0.3f, 0.2f);
        }
        
        /// <summary>
        /// 播放爆炸特效
        /// </summary>
        public void PlayExplosionEffect(Vector3 position, float scale = 1f)
        {
            var effect = SpawnEffect("explosion", position);
            if (effect != null)
            {
                effect.transform.localScale = Vector3.one * scale;
            }
            
            AudioManager.Instance?.PlaySFX(GetAudioClip("explosion"));
            
            // 屏幕震动
            CameraShake?.Invoke(0.5f * scale, 0.3f);
        }
        
        /// <summary>
        /// 播放气泡特效
        /// </summary>
        public void PlayBubbleEffect(Vector3 position, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 offset = Random.insideUnitSphere * 0.5f;
                SpawnEffect("bubble", position + offset);
            }
        }
        
        /// <summary>
        /// 播放高压警告特效
        /// </summary>
        public void PlayPressureWarningEffect(Vector3 position)
        {
            SpawnEffect("pressure_warning", position);
        }
        
        /// <summary>
        /// 播放任务完成特效
        /// </summary>
        public void PlayMissionCompleteEffect()
        {
            // UI特效
            SpawnUIEffect("mission_complete");
            
            AudioManager.Instance?.PlaySFX(GetAudioClip("mission_complete"));
        }
        
        /// <summary>
        /// 生成特效 (BUG-011 修复: 改进生命周期管理)
        /// </summary>
        private GameObject SpawnEffect(string effectName, Vector3 position)
        {
            if (!effectPrefabs.TryGetValue(effectName, out GameObject prefab) || prefab == null)
            {
                // 如果预制体不存在，创建简单的默认特效
                return CreateDefaultEffect(effectName, position);
            }
            
            Transform container = effectContainer != null ? effectContainer : transform;
            GameObject effect = Instantiate(prefab, position, Quaternion.identity, container);
            
            // BUG-011 修复: 改进自动销毁逻辑
            var autoDestroy = effect.GetComponent<EffectAutoDestroy>();
            if (autoDestroy == null)
            {
                autoDestroy = effect.AddComponent<EffectAutoDestroy>();
            }
            
            // 根据粒子系统设置销毁时间
            var particles = effect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                float duration = particles.main.duration + particles.main.startLifetime.constantMax;
                autoDestroy.SetLifetime(duration);
            }
            else
            {
                autoDestroy.SetLifetime(2f);
            }
            
            return effect;
        }
        
        /// <summary>
        /// 生成UI特效
        /// </summary>
        private void SpawnUIEffect(string effectName)
        {
            if (!effectPrefabs.TryGetValue(effectName, out GameObject prefab) || prefab == null)
                return;
            
            // 在Canvas上实例化
            var canvas = FindObjectOfType<UnityEngine.Canvas>();
            if (canvas != null)
            {
                Instantiate(prefab, canvas.transform);
            }
        }
        
        /// <summary>
        /// 创建默认特效（当预制体不存在时）
        /// </summary>
        private GameObject CreateDefaultEffect(string effectName, Vector3 position)
        {
            GameObject effect = new GameObject($"Effect_{effectName}");
            effect.transform.position = position;
            
            // 添加简单的粒子系统
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
            
            // 根据特效类型设置颜色
            var colorOverLifetime = particles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            
            Gradient gradient = new Gradient();
            switch (effectName)
            {
                case "collect":
                    gradient.SetKeys(
                        new GradientColorKey[] { new GradientColorKey(Color.cyan, 0f), new GradientColorKey(Color.white, 1f) },
                        new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
                    );
                    break;
                case "damage":
                    gradient.SetKeys(
                        new GradientColorKey[] { new GradientColorKey(Color.red, 0f), new GradientColorKey(Color.yellow, 1f) },
                        new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
                    );
                    break;
                default:
                    gradient.SetKeys(
                        new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.gray, 1f) },
                        new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
                    );
                    break;
            }
            colorOverLifetime.color = gradient;
            
            Destroy(effect, 2f);
            
            return effect;
        }
        
        /// <summary>
        /// 获取音频片段
        /// </summary>
        private AudioClip GetAudioClip(string clipName)
        {
            return AudioManager.Instance?.GetClip(clipName);
        }
        
        // 相机震动事件
        public static System.Action<float, float> CameraShake;
    }
    
    /// <summary>
    /// 特效自动销毁组件 (BUG-011 修复)
    /// </summary>
    public class EffectAutoDestroy : MonoBehaviour
    {
        private float lifetime;
        private float timer;
        private ParticleSystem[] particleSystems;
        
        public void SetLifetime(float time)
        {
            lifetime = time;
            timer = 0f;
        }
        
        private void Awake()
        {
            // 获取所有粒子系统
            particleSystems = GetComponentsInChildren<ParticleSystem>();
        }
        
        private void Update()
        {
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
            
            // 时间到或所有粒子停止时销毁
            if (timer >= lifetime || allParticlesStopped)
            {
                // 停止所有粒子发射
                foreach (var ps in particleSystems)
                {
                    if (ps != null)
                    {
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    }
                }
                
                // 等待粒子完全消失后再销毁
                if (allParticlesStopped || timer >= lifetime + 1f)
                {
                    Destroy(gameObject);
                }
            }
        }
        
        private void OnDestroy()
        {
            // 确保清理所有粒子
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
