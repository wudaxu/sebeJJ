using UnityEngine;

namespace SebeJJ.Rendering
{
    /// <summary>
    /// 2000米深度区域渲染优化 - 解决部分显卡渲染问题
    /// </summary>
    public class Depth2000Rendering : MonoBehaviour
    {
        [Header("渲染质量等级")]
        [Tooltip("根据显卡性能自动调整")]
        public bool autoDetectQuality = true;
        
        [Tooltip("强制质量等级(0=低,1=中,2=高)")]
        [Range(0, 2)]
        public int forcedQualityLevel = 2;
        
        [Header("LOD设置")]
        [Tooltip("远景LOD距离")]
        public float farLodDistance = 50f;
        
        [Tooltip("中景LOD距离")]
        public float midLodDistance = 30f;
        
        [Tooltip("近景LOD距离")]
        public float nearLodDistance = 15f;
        
        [Header("特效限制")]
        [Tooltip("最大粒子数量")]
        public int maxParticles = 500;
        
        [Tooltip("同时光源数量")]
        public int maxLights = 4;
        
        [Header("后处理")]
        [Tooltip("启用后处理")]
        public bool enablePostProcessing = true;
        
        [Tooltip("泛光强度")]
        [Range(0f, 2f)]
        public float bloomIntensity = 0.5f;
        
        // 当前质量等级
        public int CurrentQualityLevel { get; private set; } = 2;
        
        // 性能监控
        private float fpsTimer = 0f;
        private int frameCount = 0;
        private float currentFps = 60f;
        private const float FPS_CHECK_INTERVAL = 2f;
        private const float LOW_FPS_THRESHOLD = 30f;
        
        void Start()
        {
            if (autoDetectQuality)
            {
                DetectHardwareCapabilities();
            }
            else
            {
                CurrentQualityLevel = forcedQualityLevel;
            }
            
            ApplyQualitySettings();
        }
        
        void Update()
        {
            // 监控帧率并动态调整
            MonitorPerformance();
        }
        
        /// <summary>
        /// 检测硬件能力
        /// </summary>
    private void DetectHardwareCapabilities()
        {
            // 获取显卡信息
            string gpuName = SystemInfo.graphicsDeviceName.ToLower();
            int vramMB = SystemInfo.graphicsMemorySize;
            int shaderLevel = SystemInfo.graphicsShaderLevel;
            
            Debug.Log($"[Depth2000Rendering] GPU: {gpuName}, VRAM: {vramMB}MB, Shader: {shaderLevel}");
            
            // 根据硬件分级
            if (IsLowEndGPU(gpuName, vramMB))
            {
                CurrentQualityLevel = 0; // 低质量
                Debug.Log("[Depth2000Rendering] 检测到低端显卡，使用低质量设置");
            }
            else if (IsMidRangeGPU(gpuName, vramMB))
            {
                CurrentQualityLevel = 1; // 中质量
                Debug.Log("[Depth2000Rendering] 检测到中端显卡，使用中质量设置");
            }
            else
            {
                CurrentQualityLevel = 2; // 高质量
                Debug.Log("[Depth2000Rendering] 检测到高端显卡，使用高质量设置");
            }
        }
        
        /// <summary>
        /// 判断是否为低端GPU
        /// </summary>
    private bool IsLowEndGPU(string gpuName, int vramMB)
        {
            // 集成显卡
            if (gpuName.Contains("intel") && !gpuName.Contains("arc"))
                return true;
            
            // 低端独显
            string[] lowEndGPUs = { "gt 730", "gt 710", "r5 220", "r7 240", "hd 6450", "hd 5450" };
            foreach (var lowEnd in lowEndGPUs)
            {
                if (gpuName.Contains(lowEnd))
                    return true;
            }
            
            // 显存小于2GB
            if (vramMB < 2048)
                return true;
            
            return false;
        }
        
        /// <summary>
        /// 判断是否为中端GPU
        /// </summary>
    private bool IsMidRangeGPU(string gpuName, int vramMB)
        {
            string[] midRangeGPUs = { 
                "gtx 750", "gtx 760", "gtx 950", "gtx 960", "gtx 1050",
                "rx 460", "rx 560", "rx 570", "r9 270", "r9 280"
            };
            
            foreach (var mid in midRangeGPUs)
            {
                if (gpuName.Contains(mid))
                    return true;
            }
            
            // 显存2-4GB
            if (vramMB >= 2048 && vramMB < 4096)
                return true;
            
            return false;
        }
        
        /// <summary>
        /// 应用质量设置
        /// </summary>
    private void ApplyQualitySettings()
        {
            switch (CurrentQualityLevel)
            {
                case 0: // 低质量
                    ApplyLowQuality();
                    break;
                case 1: // 中质量
                    ApplyMediumQuality();
                    break;
                case 2: // 高质量
                    ApplyHighQuality();
                    break;
            }
        }
        
        /// <summary>
        /// 低质量设置
        /// </summary>
    private void ApplyLowQuality()
        {
            // 降低阴影质量
            QualitySettings.shadows = ShadowQuality.Disable;
            
            // 降低纹理质量
            QualitySettings.masterTextureLimit = 1;
            
            // 降低抗锯齿
            QualitySettings.antiAliasing = 0;
            
            // 降低粒子数量
            maxParticles = 200;
            
            // 限制光源
            maxLights = 2;
            
            // 禁用后处理
            enablePostProcessing = false;
            
            // 降低LOD距离
            farLodDistance = 30f;
            midLodDistance = 20f;
            nearLodDistance = 10f;
            
            Debug.Log("[Depth2000Rendering] 已应用低质量设置");
        }
        
        /// <summary>
        /// 中质量设置
        /// </summary>
    private void ApplyMediumQuality()
        {
            QualitySettings.shadows = ShadowQuality.HardOnly;
            QualitySettings.shadowResolution = ShadowResolution.Low;
            QualitySettings.masterTextureLimit = 0;
            QualitySettings.antiAliasing = 2;
            
            maxParticles = 350;
            maxLights = 3;
            enablePostProcessing = true;
            bloomIntensity = 0.3f;
            
            farLodDistance = 40f;
            midLodDistance = 25f;
            nearLodDistance = 12f;
            
            Debug.Log("[Depth2000Rendering] 已应用中质量设置");
        }
        
        /// <summary>
        /// 高质量设置
        /// </summary>
    private void ApplyHighQuality()
        {
            QualitySettings.shadows = ShadowQuality.All;
            QualitySettings.shadowResolution = ShadowResolution.High;
            QualitySettings.masterTextureLimit = 0;
            QualitySettings.antiAliasing = 4;
            
            maxParticles = 500;
            maxLights = 4;
            enablePostProcessing = true;
            bloomIntensity = 0.5f;
            
            farLodDistance = 50f;
            midLodDistance = 30f;
            nearLodDistance = 15f;
            
            Debug.Log("[Depth2000Rendering] 已应用高质量设置");
        }
        
        /// <summary>
        /// 性能监控
        /// </summary>
    private void MonitorPerformance()
        {
            frameCount++;
            fpsTimer += Time.deltaTime;
            
            if (fpsTimer >= FPS_CHECK_INTERVAL)
            {
                currentFps = frameCount / fpsTimer;
                frameCount = 0;
                fpsTimer = 0f;
                
                // 如果帧率过低且不是最低质量，降低质量
                if (currentFps < LOW_FPS_THRESHOLD && CurrentQualityLevel > 0)
                {
                    Debug.LogWarning($"[Depth2000Rendering] 帧率过低({currentFps:F1} FPS)，降低渲染质量");
                    CurrentQualityLevel--;
                    ApplyQualitySettings();
                }
            }
        }
        
        /// <summary>
        /// 获取当前质量设置
        /// </summary>
    public string GetCurrentQualityInfo()
        {
            string[] qualityNames = { "低", "中", "高" };
            return $"质量等级: {qualityNames[CurrentQualityLevel]}, 当前FPS: {currentFps:F1}";
        }
    }
}
