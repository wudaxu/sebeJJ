using System;
using System.Collections.Generic;
using UnityEngine;

namespace SebeJJ.Networking
{
    /// <summary>
    /// 联机管理器 - 解决延迟波动问题
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }
        
        [Header("网络配置")]
        [Tooltip("目标帧率")]
        public int targetFrameRate = 60;
        
        [Tooltip("网络更新频率(每秒)")]
        public int networkTickRate = 30;
        
        [Tooltip("延迟平滑系数(越小越平滑但越延迟)")]
        [Range(0.1f, 0.9f)]
        public float latencySmoothFactor = 0.3f;
        
        [Header("延迟补偿")]
        [Tooltip("启用延迟补偿")]
        public bool enableLatencyCompensation = true;
        
        [Tooltip("最大预测时间(秒)")]
        public float maxPredictionTime = 0.5f;
        
        [Tooltip("插值延迟(毫秒)")]
        public int interpolationDelay = 100;
        
        // 网络状态
        public NetworkState CurrentState { get; private set; } = NetworkState.Disconnected;
        
        // 延迟统计
        public float CurrentLatency { get; private set; } = 0f;
        public float AverageLatency { get; private set; } = 0f;
        public float LatencyVariance { get; private set; } = 0f;
        
        // 抖动缓冲
        private Queue<float> latencyHistory = new Queue<float>();
        private const int LATENCY_HISTORY_SIZE = 10;
        private float smoothedLatency = 0f;
        
        // 事件
        public event Action<NetworkState> OnNetworkStateChanged;
        public event Action<float> OnLatencyUpdated;
        
        void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        void Start()
        {
            Application.targetFrameRate = targetFrameRate;
        }
        
        void Update()
        {
            UpdateLatencyStatistics();
        }
        
        /// <summary>
        /// 更新延迟统计 - 平滑处理延迟波动
        /// </summary>
        private void UpdateLatencyStatistics()
        {
            // 模拟获取当前延迟(实际项目中从网络层获取)
            float rawLatency = MeasureCurrentLatency();
            
            // 使用指数移动平均平滑延迟
            smoothedLatency = Mathf.Lerp(smoothedLatency, rawLatency, latencySmoothFactor);
            CurrentLatency = smoothedLatency;
            
            // 维护历史记录
            latencyHistory.Enqueue(rawLatency);
            if (latencyHistory.Count > LATENCY_HISTORY_SIZE)
            {
                latencyHistory.Dequeue();
            }
            
            // 计算平均延迟和方差
            CalculateLatencyStats();
            
            // 触发事件
            OnLatencyUpdated?.Invoke(CurrentLatency);
        }
        
        /// <summary>
        /// 计算延迟统计
        /// </summary>
        private void CalculateLatencyStats()
        {
            if (latencyHistory.Count == 0) return;
            
            float sum = 0f;
            foreach (var lat in latencyHistory)
            {
                sum += lat;
            }
            AverageLatency = sum / latencyHistory.Count;
            
            // 计算方差(抖动程度)
            float varianceSum = 0f;
            foreach (var lat in latencyHistory)
            {
                varianceSum += Mathf.Pow(lat - AverageLatency, 2);
            }
            LatencyVariance = varianceSum / latencyHistory.Count;
        }
        
        /// <summary>
        /// 测量当前延迟
        /// </summary>
        private float MeasureCurrentLatency()
        {
            // TODO: 实际项目中从网络层获取真实延迟
            // 这里返回模拟值用于测试
            return UnityEngine.Random.Range(30f, 150f);
        }
        
        /// <summary>
        /// 获取推荐的插值延迟
        /// </summary>
        public int GetRecommendedInterpolationDelay()
        {
            // 基于当前延迟和方差计算推荐插值延迟
            float recommended = CurrentLatency + (LatencyVariance * 2) + 50;
            return Mathf.Clamp((int)recommended, 50, 500);
        }
        
        /// <summary>
        /// 检查网络状态是否稳定
        /// </summary>
        public bool IsNetworkStable()
        {
            return LatencyVariance < 100f && CurrentLatency < 200f;
        }
        
        /// <summary>
        /// 获取网络质量评级
        /// </summary>
        public NetworkQuality GetNetworkQuality()
        {
            if (CurrentLatency < 50 && LatencyVariance < 20)
                return NetworkQuality.Excellent;
            if (CurrentLatency < 100 && LatencyVariance < 50)
                return NetworkQuality.Good;
            if (CurrentLatency < 200 && LatencyVariance < 100)
                return NetworkQuality.Fair;
            return NetworkQuality.Poor;
        }
        
        /// <summary>
        /// 设置网络状态
        /// </summary>
        public void SetNetworkState(NetworkState state)
        {
            if (CurrentState != state)
            {
                CurrentState = state;
                OnNetworkStateChanged?.Invoke(state);
                Debug.Log($"[NetworkManager] 网络状态变更: {state}");
            }
        }
        
        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
    
    /// <summary>
    /// 网络状态
    /// </summary>
    public enum NetworkState
    {
        Disconnected,
        Connecting,
        Connected,
        Reconnecting,
        Error
    }
    
    /// <summary>
    /// 网络质量
    /// </summary>
    public enum NetworkQuality
    {
        Excellent,  // < 50ms, 低方差
        Good,       // < 100ms, 中方差
        Fair,       // < 200ms, 较高方差
        Poor        // >= 200ms 或高方差
    }
}
