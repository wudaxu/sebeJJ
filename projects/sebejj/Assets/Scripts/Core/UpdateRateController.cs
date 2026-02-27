using UnityEngine;

namespace SebeJJ.Core
{
    /// <summary>
    /// 更新频率控制器 - 优化Update循环性能
    /// 允许组件以不同频率更新，减少CPU占用
    /// </summary>
    public class UpdateRateController : MonoBehaviour
    {
        public enum UpdateFrequency
        {
            EveryFrame,     // 每帧 (60fps)
            Every2Frames,   // 每2帧 (30fps)
            Every4Frames,   // 每4帧 (15fps)
            Every10Frames,  // 每10帧 (6fps)
            Every30Frames,  // 每30帧 (2fps)
            CustomInterval  // 自定义时间间隔
        }
        
        [Header("更新设置")]
        [SerializeField] private UpdateFrequency frequency = UpdateFrequency.EveryFrame;
        [SerializeField] private float customInterval = 0.5f;
        
        // 运行时状态
        private int frameCounter;
        private int targetFrameInterval;
        private float lastUpdateTime;
        private float updateInterval;
        
        // 距离摄像机远时的优化
        [Header("距离优化")]
        [SerializeField] private bool enableDistanceCulling = true;
        [SerializeField] private float highFrequencyDistance = 10f;
        [SerializeField] private float mediumFrequencyDistance = 30f;
        [SerializeField] private float lowFrequencyDistance = 50f;
        
        private Transform mainCamera;
        private float sqrDistanceToCamera;
        
        public bool ShouldUpdateThisFrame { get; private set; }
        public float DeltaTime { get; private set; }
        
        private void Awake()
        {
            Initialize();
        }
        
        private void Start()
        {
            if (enableDistanceCulling)
            {
                mainCamera = Camera.main?.transform;
            }
        }
        
        private void Initialize()
        {
            switch (frequency)
            {
                case UpdateFrequency.EveryFrame:
                    targetFrameInterval = 1;
                    updateInterval = 0f;
                    break;
                case UpdateFrequency.Every2Frames:
                    targetFrameInterval = 2;
                    updateInterval = 0f;
                    break;
                case UpdateFrequency.Every4Frames:
                    targetFrameInterval = 4;
                    updateInterval = 0f;
                    break;
                case UpdateFrequency.Every10Frames:
                    targetFrameInterval = 10;
                    updateInterval = 0f;
                    break;
                case UpdateFrequency.Every30Frames:
                    targetFrameInterval = 30;
                    updateInterval = 0f;
                    break;
                case UpdateFrequency.CustomInterval:
                    targetFrameInterval = 0;
                    updateInterval = customInterval;
                    break;
            }
            
            frameCounter = Random.Range(0, targetFrameInterval); // 随机偏移避免同一帧大量更新
        }
        
        /// <summary>
        /// 在Update中调用，返回是否应该执行更新
        /// </summary>
        public bool ShouldUpdate()
        {
            // 基于帧的更新
            if (targetFrameInterval > 0)
            {
                frameCounter++;
                if (frameCounter >= targetFrameInterval)
                {
                    frameCounter = 0;
                    ShouldUpdateThisFrame = true;
                    DeltaTime = Time.deltaTime * targetFrameInterval;
                    return true;
                }
                ShouldUpdateThisFrame = false;
                return false;
            }
            
            // 基于时间的更新
            float currentTime = Time.time;
            if (currentTime - lastUpdateTime >= updateInterval)
            {
                DeltaTime = currentTime - lastUpdateTime;
                lastUpdateTime = currentTime;
                ShouldUpdateThisFrame = true;
                return true;
            }
            
            ShouldUpdateThisFrame = false;
            return false;
        }
        
        /// <summary>
        /// 根据距离摄像机的距离动态调整更新频率
        /// </summary>
        public bool ShouldUpdateWithDistance()
        {
            if (!enableDistanceCulling || mainCamera == null)
            {
                return ShouldUpdate();
            }
            
            // 计算到摄像机的平方距离
            Vector3 toCamera = mainCamera.position - transform.position;
            sqrDistanceToCamera = toCamera.x * toCamera.x + toCamera.y * toCamera.y + toCamera.z * toCamera.z;
            
            // 根据距离调整频率
            UpdateFrequency effectiveFrequency = frequency;
            
            if (sqrDistanceToCamera > lowFrequencyDistance * lowFrequencyDistance)
            {
                // 远距离 - 最低频率
                effectiveFrequency = UpdateFrequency.Every30Frames;
            }
            else if (sqrDistanceToCamera > mediumFrequencyDistance * mediumFrequencyDistance)
            {
                // 中距离 - 低频率
                effectiveFrequency = UpdateFrequency.Every10Frames;
            }
            else if (sqrDistanceToCamera > highFrequencyDistance * highFrequencyDistance)
            {
                // 近距离 - 中频率
                effectiveFrequency = UpdateFrequency.Every4Frames;
            }
            
            // 如果频率改变，重新初始化
            if (effectiveFrequency != frequency)
            {
                UpdateFrequency originalFrequency = frequency;
                frequency = effectiveFrequency;
                Initialize();
                frequency = originalFrequency; // 恢复原始设置
            }
            
            return ShouldUpdate();
        }
        
        /// <summary>
        /// 强制下一次更新
        /// </summary>
        public void ForceNextUpdate()
        {
            frameCounter = targetFrameInterval - 1;
            lastUpdateTime = Time.time - updateInterval;
        }
        
        /// <summary>
        /// 设置自定义更新间隔
        /// </summary>
        public void SetCustomInterval(float interval)
        {
            customInterval = interval;
            if (frequency == UpdateFrequency.CustomInterval)
            {
                Initialize();
            }
        }
        
        /// <summary>
        /// 获取当前到摄像机的距离
        /// </summary>
        public float GetDistanceToCamera()
        {
            return Mathf.Sqrt(sqrDistanceToCamera);
        }
        
        private void OnDrawGizmosSelected()
        {
            if (!enableDistanceCulling) return;
            
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, highFrequencyDistance);
            
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, mediumFrequencyDistance);
            
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, lowFrequencyDistance);
        }
    }
}
