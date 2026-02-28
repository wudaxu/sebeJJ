using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SebeJJ.Networking
{
    /// <summary>
    /// 网络对象同步器 - 解决位置/状态同步的延迟波动
    /// </summary>    public class NetworkSync : MonoBehaviour
    {
        [Header("同步配置")]
        [Tooltip("同步间隔(秒)")]
        public float syncInterval = 0.033f; // 30fps
        
        [Tooltip("位置插值速度")]
        public float positionLerpSpeed = 15f;
        
        [Tooltip("旋转插值速度")]
        public float rotationLerpSpeed = 15f;
        
        [Tooltip("使用预测")]
        public bool usePrediction = true;
        
        [Tooltip("预测因子")]
        [Range(0f, 2f)]
        public float predictionFactor = 1.2f;
        
        // 同步状态
        private Vector3 targetPosition;
        private Quaternion targetRotation;
        private Vector3 lastReceivedVelocity;
        private float lastReceiveTime;
        
        // 历史缓冲区(用于插值)
        private Queue<SyncSnapshot> snapshotBuffer = new Queue<SyncSnapshot>();
        private const int BUFFER_SIZE = 6; // 约200ms缓冲
        
        // 组件引用
        private Rigidbody2D rb;
        
        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            targetPosition = transform.position;
            targetRotation = transform.rotation;
        }
        
        void Update()
        {
            ApplyInterpolatedState();
        }
        
        /// <summary>
        /// 应用插值状态 - 平滑处理网络波动
        /// </summary>
        private void ApplyInterpolatedState()
        {
            if (snapshotBuffer.Count == 0) return;
            
            // 获取当前网络推荐的插值延迟
            int interpolationDelay = NetworkManager.Instance?.GetRecommendedInterpolationDelay() ?? 100;
            float delaySeconds = interpolationDelay / 1000f;
            
            // 找到合适的历史快照进行插值
            SyncSnapshot? targetSnapshot = FindSnapshotForInterpolation(delaySeconds);
            
            if (targetSnapshot.HasValue)
            {
                var snapshot = targetSnapshot.Value;
                
                // 平滑插值到目标位置
                transform.position = Vector3.Lerp(transform.position, snapshot.position, 
                    Time.deltaTime * positionLerpSpeed);
                transform.rotation = Quaternion.Lerp(transform.rotation, snapshot.rotation, 
                    Time.deltaTime * rotationLerpSpeed);
                
                // 如果有刚体，更新速度
                if (rb != null)
                {
                    rb.velocity = snapshot.velocity;
                }
            }
        }
        
        /// <summary>
        /// 查找用于插值的快照
        /// </summary>
        private SyncSnapshot? FindSnapshotForInterpolation(float delaySeconds)
        {
            float targetTime = Time.time - delaySeconds;
            
            SyncSnapshot? bestSnapshot = null;
            float bestTimeDiff = float.MaxValue;
            
            foreach (var snapshot in snapshotBuffer)
            {
                float timeDiff = Mathf.Abs(snapshot.timestamp - targetTime);
                if (timeDiff < bestTimeDiff)
                {
                    bestTimeDiff = timeDiff;
                    bestSnapshot = snapshot;
                }
            }
            
            return bestSnapshot;
        }
        
        /// <summary>
        /// 接收网络同步数据
        /// </summary>
        public void ReceiveSyncData(Vector3 position, Quaternion rotation, Vector3 velocity, float serverTime)
        {
            // 计算延迟
            float latency = (Time.time - serverTime) * 1000f;
            
            // 预测补偿
            if (usePrediction)
            {
                float predictionTime = Mathf.Min(latency / 1000f * predictionFactor, 
                    NetworkManager.Instance?.maxPredictionTime ?? 0.5f);
                position += velocity * predictionTime;
            }
            
            // 添加到缓冲区
            snapshotBuffer.Enqueue(new SyncSnapshot
            {
                timestamp = Time.time,
                position = position,
                rotation = rotation,
                velocity = velocity
            });
            
            // 限制缓冲区大小
            while (snapshotBuffer.Count > BUFFER_SIZE)
            {
                snapshotBuffer.Dequeue();
            }
            
            lastReceiveTime = Time.time;
        }
        
        /// <summary>
        /// 检查同步是否超时
        /// </summary>
        public bool IsSyncTimeout()
        {
            return Time.time - lastReceiveTime > 2f; // 2秒无数据视为超时
        }
    }
    
    /// <summary>
    /// 同步快照
    /// </summary>    public struct SyncSnapshot
    {
        public float timestamp;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 velocity;
    }
}
