using UnityEngine;
using System;

namespace SebeJJ.Systems
{
    /// <summary>
    /// 下潜管理器 - 管理深度、压力、危险区域
    /// </summary>
    public class DiveManager : MonoBehaviour
    {
        public static DiveManager Instance { get; private set; }
        
        [Header("深度设置")]
        public float surfaceY = 0f;
        public float maxDepth = 1000f;
        public float depthPerUnit = 10f; // 每单位Y坐标对应的深度
        
        [Header("压力设置")]
        public float basePressure = 1f; // 大气压
        public float pressurePer100m = 10f; // 每100米增加的压力
        public float maxSafePressure = 50f; // 最大安全压力
        
        [Header("危险设置")]
        public float dangerZoneStart = 500f;
        public float criticalZoneStart = 800f;
        public float damagePerSecondInDanger = 5f;
        public float damagePerSecondInCritical = 15f;
        
        [Header("视觉效果")]
        public Camera mainCamera;
        public Color surfaceColor = new Color(0.2f, 0.6f, 0.8f);
        public Color deepColor = new Color(0.05f, 0.05f, 0.2f);
        public Color dangerColor = new Color(0.3f, 0f, 0f);
        
        [Header("光照和雾效")]
        public Light mainLight;
        public float surfaceLightIntensity = 1f;
        public float deepLightIntensity = 0.2f;
        public bool enableFog = true;
        public Color surfaceFogColor = new Color(0.4f, 0.7f, 0.9f);
        public Color deepFogColor = new Color(0.02f, 0.02f, 0.1f);
        public float surfaceFogDensity = 0.001f;
        public float deepFogDensity = 0.05f;
        
        // 当前状态
        public float CurrentDepth { get; private set; }
        public float MaxDepthReached { get; private set; }
        public float CurrentPressure { get; private set; }
        public DiveZone CurrentZone { get; private set; } = DiveZone.Surface;
        
        // 组件
        private Transform playerTransform;
        private float lastDamageTime;
        private float lastDepthUpdateTime; // UF-001修复: 添加更新频率控制
        
        // UF-001修复: UI更新间隔
        private const float UPDATE_INTERVAL = 0.05f; // 20次/秒
        
        // 事件
        public event Action<float> OnDepthChanged;
        public event Action<float> OnPressureChanged;
        public event Action<DiveZone> OnZoneChanged;
        public event Action OnMaxDepthReached;
        public event Action OnPressureWarning;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Start()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
            
            // 查找玩家
            var player = Player.MechController.Instance;
            if (player != null)
                playerTransform = player.transform;
        }
        
        private void Update()
        {
            // UF-001修复: 降低更新频率，使用插值显示
            if (Time.time >= lastDepthUpdateTime + UPDATE_INTERVAL)
            {
                UpdateDepth();
                UpdatePressure();
                UpdateZone();
                lastDepthUpdateTime = Time.time;
            }
            
            UpdateVisuals();
            CheckDangers();
        }
        
        /// <summary>
        /// 初始化新游戏
        /// </summary>
        public void InitializeNewGame()
        {
            CurrentDepth = 0f;
            MaxDepthReached = 0f;
            CurrentPressure = basePressure;
            CurrentZone = DiveZone.Surface;
            
            OnDepthChanged?.Invoke(CurrentDepth);
            OnPressureChanged?.Invoke(CurrentPressure);
            OnZoneChanged?.Invoke(CurrentZone);
        }
        
        /// <summary>
        /// 更新深度
        /// </summary>
        private void UpdateDepth()
        {
            if (playerTransform == null)
            {
                var player = Player.MechController.Instance;
                if (player != null)
                    playerTransform = player.transform;
                return;
            }
            
            float newDepth = Mathf.Max(0f, (surfaceY - playerTransform.position.y) * depthPerUnit);
            
            if (Mathf.Abs(newDepth - CurrentDepth) > 0.1f)
            {
                CurrentDepth = newDepth;
                OnDepthChanged?.Invoke(CurrentDepth);
                
                // 更新最大深度
                if (CurrentDepth > MaxDepthReached)
                {
                    MaxDepthReached = CurrentDepth;
                    OnMaxDepthReached?.Invoke();
                }
            }
        }
        
        /// <summary>
        /// 更新压力
        /// </summary>
        private void UpdatePressure()
        {
            float newPressure = basePressure + (CurrentDepth / 100f) * pressurePer100m;
            
            if (Mathf.Abs(newPressure - CurrentPressure) > 0.01f)
            {
                CurrentPressure = newPressure;
                OnPressureChanged?.Invoke(CurrentPressure);
                
                // 高压警告
                if (CurrentPressure > maxSafePressure * 0.8f && 
                    CurrentPressure <= maxSafePressure * 0.8f + 0.1f)
                {
                    OnPressureWarning?.Invoke();
                }
            }
        }
        
        /// <summary>
        /// 更新区域
        /// </summary>
        private void UpdateZone()
        {
            DiveZone newZone;
            
            if (CurrentDepth >= criticalZoneStart)
                newZone = DiveZone.Critical;
            else if (CurrentDepth >= dangerZoneStart)
                newZone = DiveZone.Danger;
            else if (CurrentDepth >= dangerZoneStart * 0.5f)
                newZone = DiveZone.Deep;
            else
                newZone = DiveZone.Surface;
            
            if (newZone != CurrentZone)
            {
                CurrentZone = newZone;
                OnZoneChanged?.Invoke(CurrentZone);
                Debug.Log($"[DiveManager] 进入区域: {CurrentZone}");
            }
        }
        
        /// <summary>
        /// 更新视觉效果
        /// </summary>
        private void UpdateVisuals()
        {
            if (mainCamera == null) return;
            
            // 根据深度调整背景颜色
            Color targetColor;
            float depthRatio = Mathf.Clamp01(CurrentDepth / maxDepth);
            
            if (CurrentZone == DiveZone.Critical)
            {
                targetColor = dangerColor;
            }
            else if (CurrentZone == DiveZone.Danger)
            {
                targetColor = Color.Lerp(deepColor, dangerColor, 
                    (CurrentDepth - dangerZoneStart) / (criticalZoneStart - dangerZoneStart));
            }
            else
            {
                targetColor = Color.Lerp(surfaceColor, deepColor, depthRatio);
            }
            
            mainCamera.backgroundColor = Color.Lerp(mainCamera.backgroundColor, targetColor, Time.deltaTime * 2f);
            
            // 调整光照强度
            if (mainLight != null)
            {
                float targetIntensity = Mathf.Lerp(surfaceLightIntensity, deepLightIntensity, depthRatio);
                mainLight.intensity = Mathf.Lerp(mainLight.intensity, targetIntensity, Time.deltaTime * 2f);
            }
            
            // 调整雾效
            if (enableFog)
            {
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                
                Color targetFogColor = Color.Lerp(surfaceFogColor, deepFogColor, depthRatio);
                RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, targetFogColor, Time.deltaTime * 2f);
                
                float targetFogDensity = Mathf.Lerp(surfaceFogDensity, deepFogDensity, depthRatio);
                RenderSettings.fogDensity = Mathf.Lerp(RenderSettings.fogDensity, targetFogDensity, Time.deltaTime * 2f);
            }
        }
        
        /// <summary>
        /// 检查危险
        /// </summary>
        private void CheckDangers()
        {
            if (Time.time < lastDamageTime + 1f) return;
            
            float damage = 0f;
            
            switch (CurrentZone)
            {
                case DiveZone.Danger:
                    damage = damagePerSecondInDanger;
                    break;
                case DiveZone.Critical:
                    damage = damagePerSecondInCritical;
                    break;
            }
            
            // 压力过高也会造成伤害
            if (CurrentPressure > maxSafePressure)
            {
                damage += (CurrentPressure - maxSafePressure) * 2f;
            }
            
            if (damage > 0)
            {
                var player = Player.MechController.Instance;
                if (player != null)
                {
                    player.TakeDamage(damage);
                    lastDamageTime = Time.time;
                }
            }
        }
        
        /// <summary>
        /// 获取氧气消耗倍率
        /// NB-002修复: 添加上限保护
        /// </summary>
        public float GetOxygenConsumptionMultiplier()
        {
            float multiplier = 1f;
            
            // 深度越深，氧气消耗越快
            multiplier += CurrentDepth / maxDepth * 0.5f;
            
            // 危险区域额外消耗
            if (CurrentZone == DiveZone.Danger)
                multiplier += 0.3f;
            else if (CurrentZone == DiveZone.Critical)
                multiplier += 0.6f;
            
            // 高压增加消耗
            if (CurrentPressure > maxSafePressure)
                multiplier += (CurrentPressure - maxSafePressure) * 0.05f;
            
            // NB-002修复: 添加上限保护，最大3倍
            return Mathf.Min(multiplier, 3f);
        }
        
        /// <summary>
        /// 获取当前区域名称
        /// </summary>
        public string GetZoneName()
        {
            switch (CurrentZone)
            {
                case DiveZone.Surface: return "浅海区";
                case DiveZone.Deep: return "深海区";
                case DiveZone.Danger: return "危险区";
                case DiveZone.Critical: return "极限区";
                default: return "未知区域";
            }
        }
        
        /// <summary>
        /// 从存档加载
        /// </summary>
        public void LoadFromSave(Core.GameSaveData saveData)
        {
            CurrentDepth = saveData.currentDepth;
            MaxDepthReached = saveData.maxDepthReached;
            
            OnDepthChanged?.Invoke(CurrentDepth);
        }
        
        private void OnDrawGizmosSelected()
        {
            // 绘制深度区域
            float surfaceWorldY = surfaceY;
            float dangerWorldY = surfaceY - dangerZoneStart / depthPerUnit;
            float criticalWorldY = surfaceY - criticalZoneStart / depthPerUnit;
            float maxWorldY = surfaceY - maxDepth / depthPerUnit;
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(new Vector3(-50f, surfaceWorldY, 0f), new Vector3(50f, surfaceWorldY, 0f));
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(new Vector3(-50f, dangerWorldY, 0f), new Vector3(50f, dangerWorldY, 0f));
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector3(-50f, criticalWorldY, 0f), new Vector3(50f, criticalWorldY, 0f));
            
            Gizmos.color = Color.black;
            Gizmos.DrawLine(new Vector3(-50f, maxWorldY, 0f), new Vector3(50f, maxWorldY, 0f));
        }
    }
    
    /// <summary>
    /// 下潜区域枚举
    /// </summary>
    public enum DiveZone
    {
        Surface,    // 浅海区 (0-250m)
        Deep,       // 深海区 (250-500m)
        Danger,     // 危险区 (500-800m)
        Critical    // 极限区 (800m+)
    }
}
