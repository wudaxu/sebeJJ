/** 
 * @file AIPerception.cs
 * @brief AI感知系统 - 任务AI-004~005
 * @description 提供视觉侦测、听觉侦测和记忆系统
 * @author AI系统架构师
 * @date 2026-02-27
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace SebeJJ.AI
{
    /// <summary>
    /// 感知目标信息
    /// </summary>
    public class PerceptionTarget
    {
        /// <summary>
        /// 目标对象
        /// </summary>
        public Transform Target { get; set; }
        
        /// <summary>
        /// 目标位置
        /// </summary>
        public Vector3 Position => Target?.position ?? Vector3.zero;
        
        /// <summary>
        /// 最后已知位置
        /// </summary>
        public Vector3 LastKnownPosition { get; set; }
        
        /// <summary>
        /// 最后感知时间
        /// </summary>
        public float LastPerceptionTime { get; set; }
        
        /// <summary>
        /// 感知类型
        /// </summary>
        public PerceptionType PerceptionType { get; set; }
        
        /// <summary>
        /// 目标距离
        /// </summary>
        public float Distance { get; set; }
        
        /// <summary>
        /// 是否在视野内
        /// </summary>
        public bool IsInSight { get; set; }
        
        /// <summary>
        /// 是否可被记忆
        /// </summary>
        public bool IsMemorized => Time.time - LastPerceptionTime < 10f;
        
        /// <summary>
        /// 记忆置信度 (0-1)
        /// </summary>
        public float MemoryConfidence
        {
            get
            {
                float timeSinceLastSeen = Time.time - LastPerceptionTime;
                return Mathf.Max(0f, 1f - timeSinceLastSeen / 10f);
            }
        }
    }

    /// <summary>
    /// 感知类型
    /// </summary>
    public enum PerceptionType
    {
        None,       // 无感知
        Visual,     // 视觉
        Auditory,   // 听觉
        Memory      // 记忆
    }

    /// <summary>
    /// AI感知系统 - 管理敌人的感知能力
    /// </summary>
    public class AIPerception : MonoBehaviour
    {
        #region 事件定义
        
        /// <summary>
        /// 发现目标事件
        /// </summary>
        public event Action<PerceptionTarget> OnTargetDetected;
        
        /// <summary>
        /// 丢失目标事件
        /// </summary>
        public event Action<PerceptionTarget> OnTargetLost;
        
        /// <summary>
        /// 感知更新事件
        /// </summary>
        public event Action OnPerceptionUpdated;
        
        #endregion

        #region 序列化字段
        
        [Header("视觉配置")]
        [SerializeField] private float viewRadius = 10f;
        [SerializeField] private float viewAngle = 120f;
        [SerializeField] private float eyeHeight = 1f;
        [SerializeField] private LayerMask obstacleMask;
        [SerializeField] private LayerMask targetMask;
        
        [Header("听觉配置")]
        [SerializeField] private float hearingRadius = 15f;
        [SerializeField] private float hearingThreshold = 1f;
        
        [Header("记忆配置")]
        [SerializeField] private float memoryDuration = 5f;
        [SerializeField] private float memoryFadeRate = 0.5f;
        
        [Header("更新配置")]
        [SerializeField] private float perceptionUpdateInterval = 0.2f;
        [SerializeField] private bool enableDebugVisuals = true;
        
        #endregion

        #region 私有字段
        
        /// <summary>
        /// 感知到的目标列表
        /// </summary>
        private Dictionary<Transform, PerceptionTarget> _detectedTargets = new Dictionary<Transform, PerceptionTarget>();
        
        /// <summary>
        /// 记忆中的目标列表
        /// </summary>
        private Dictionary<Transform, PerceptionTarget> _memorizedTargets = new Dictionary<Transform, PerceptionTarget>();
        
        /// <summary>
        /// 当前主要目标
        /// </summary>
        private PerceptionTarget _primaryTarget;
        
        /// <summary>
        /// 更新计时器
        /// </summary>
        private float _updateTimer = 0f;
        
        /// <summary>
        /// 眼睛位置
        /// </summary>
        private Vector3 EyePosition => transform.position + Vector3.up * eyeHeight;
        
        /// <summary>
        /// AF-001修复: 获取深度影响乘数
        /// </summary>
        private float GetDepthMultiplier()
        {
            // 尝试获取DiveManager的深度
            float depth = 0f;
            if (Systems.DiveManager.Instance != null)
            {
                depth = Systems.DiveManager.Instance.CurrentDepth;
            }
            // 深度越大，感知范围越小（能见度降低），最小0.5倍
            return Mathf.Lerp(1f, 0.5f, depth / 1000f);
        }
        
        #endregion

        #region 公共属性
        
        /// <summary>
        /// 视野半径
        /// </summary>
        public float ViewRadius => viewRadius;
        
        /// <summary>
        /// 视野角度
        /// </summary>
        public float ViewAngle => viewAngle;
        
        /// <summary>
        /// 听觉半径
        /// </summary>
        public float HearingRadius => hearingRadius;
        
        /// <summary>
        /// 当前主要目标
        /// </summary>
        public PerceptionTarget PrimaryTarget => _primaryTarget;
        
        /// <summary>
        /// 是否有目标
        /// </summary>
        public bool HasTarget => _primaryTarget != null && _primaryTarget.IsInSight;
        
        /// <summary>
        /// 是否有记忆中的目标
        /// </summary>
        public bool HasMemoryTarget => _primaryTarget != null && _primaryTarget.IsMemorized;
        
        /// <summary>
        /// 检测到的目标数量
        /// </summary>
        public int DetectedTargetCount => _detectedTargets.Count;
        
        /// <summary>
        /// 所有检测到的目标
        /// </summary>
        public IEnumerable<PerceptionTarget> AllDetectedTargets => _detectedTargets.Values;
        
        #endregion

        #region Unity生命周期
        
        private void Update()
        {
            _updateTimer += Time.deltaTime;
            
            if (_updateTimer >= perceptionUpdateInterval)
            {
                UpdatePerception();
                _updateTimer = 0f;
            }
            
            // 更新记忆
            UpdateMemory();
        }
        
        private void OnDrawGizmos()
        {
            if (!enableDebugVisuals) return;
            
            // 绘制视野范围
            DrawViewGizmos();
            
            // 绘制听觉范围
            DrawHearingGizmos();
            
            // 绘制目标信息
            DrawTargetGizmos();
        }
        
        #endregion

        #region 感知更新
        
        /// <summary>
        /// 更新感知
        /// </summary>
        private void UpdatePerception()
        {
            // 视觉检测
            UpdateVisualPerception();
            
            // 听觉检测
            UpdateAuditoryPerception();
            
            // 更新主要目标
            UpdatePrimaryTarget();
            
            OnPerceptionUpdated?.Invoke();
        }
        
        /// <summary>
        /// 更新视觉感知
        /// AF-001修复: 考虑深度影响
        /// </summary>
        private void UpdateVisualPerception()
        {
            // AF-001修复: 应用深度影响
            float effectiveViewRadius = viewRadius * GetDepthMultiplier();
            
            Collider2D[] targetsInViewRadius = Physics2D.OverlapCircleAll(transform.position, effectiveViewRadius, targetMask);
            
            foreach (var targetCollider in targetsInViewRadius)
            {
                Transform target = targetCollider.transform;
                
                if (target == transform) continue;
                
                Vector3 dirToTarget = (target.position - EyePosition).normalized;
                float angleToTarget = Vector3.Angle(transform.right, dirToTarget);
                
                // 检查是否在视野角度内
                if (angleToTarget < viewAngle / 2f)
                {
                    float distanceToTarget = Vector3.Distance(EyePosition, target.position);
                    
                    // 射线检测是否有障碍物
                    RaycastHit2D hit = Physics2D.Raycast(EyePosition, dirToTarget, distanceToTarget, obstacleMask);
                    
                    if (!hit)
                    {
                        // 可以看到目标
                        RegisterTarget(target, PerceptionType.Visual, distanceToTarget);
                    }
                    else
                    {
                        // 目标被遮挡，从检测列表中移除
                        RemoveDetectedTarget(target);
                    }
                }
            }
        }
        
        /// <summary>
        /// 更新听觉感知
        /// </summary>
        private void UpdateAuditoryPerception()
        {
            // 检测发出声音的目标
            Collider2D[] targetsInHearingRadius = Physics2D.OverlapCircleAll(transform.position, hearingRadius, targetMask);
            
            foreach (var targetCollider in targetsInHearingRadius)
            {
                Transform target = targetCollider.transform;
                
                if (target == transform) continue;
                
                // 获取目标的噪音组件（如果有）
                var noiseMaker = target.GetComponent<INoiseMaker>();
                if (noiseMaker != null && noiseMaker.IsMakingNoise)
                {
                    float distanceToTarget = Vector3.Distance(transform.position, target.position);
                    float noiseIntensity = noiseMaker.NoiseIntensity;
                    
                    // 检查是否超过听觉阈值
                    if (noiseIntensity / distanceToTarget > hearingThreshold)
                    {
                        RegisterTarget(target, PerceptionType.Auditory, distanceToTarget);
                    }
                }
            }
        }
        
        /// <summary>
        /// 注册检测到的目标
        /// </summary>
        private void RegisterTarget(Transform target, PerceptionType type, float distance)
        {
            bool isNewTarget = false;
            
            if (!_detectedTargets.TryGetValue(target, out var perceptionTarget))
            {
                perceptionTarget = new PerceptionTarget
                {
                    Target = target,
                    LastKnownPosition = target.position
                };
                _detectedTargets[target] = perceptionTarget;
                isNewTarget = true;
            }
            
            perceptionTarget.LastPerceptionTime = Time.time;
            perceptionTarget.LastKnownPosition = target.position;
            perceptionTarget.PerceptionType = type;
            perceptionTarget.Distance = distance;
            perceptionTarget.IsInSight = true;
            
            // 添加到记忆
            _memorizedTargets[target] = perceptionTarget;
            
            if (isNewTarget)
            {
                OnTargetDetected?.Invoke(perceptionTarget);
            }
        }
        
        /// <summary>
        /// 移除检测到的目标
        /// </summary>
        private void RemoveDetectedTarget(Transform target)
        {
            if (_detectedTargets.TryGetValue(target, out var perceptionTarget))
            {
                perceptionTarget.IsInSight = false;
                _detectedTargets.Remove(target);
                OnTargetLost?.Invoke(perceptionTarget);
            }
        }
        
        /// <summary>
        /// 更新记忆
        /// </summary>
        private void UpdateMemory()
        {
            List<Transform> toRemove = new List<Transform>();
            
            foreach (var kvp in _memorizedTargets)
            {
                var target = kvp.Value;
                
                // 检查记忆是否过期
                if (Time.time - target.LastPerceptionTime > memoryDuration)
                {
                    toRemove.Add(kvp.Key);
                }
            }
            
            foreach (var target in toRemove)
            {
                _memorizedTargets.Remove(target);
            }
        }
        
        /// <summary>
        /// 更新主要目标
        /// </summary>
        private void UpdatePrimaryTarget()
        {
            PerceptionTarget bestTarget = null;
            float bestScore = float.MinValue;
            
            // 从检测到的目标中选择最佳目标
            foreach (var target in _detectedTargets.Values)
            {
                float score = CalculateTargetScore(target);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = target;
                }
            }
            
            // 如果没有检测到的目标，从记忆中选择
            if (bestTarget == null)
            {
                foreach (var target in _memorizedTargets.Values)
                {
                    float score = CalculateTargetScore(target) * target.MemoryConfidence;
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestTarget = target;
                    }
                }
            }
            
            _primaryTarget = bestTarget;
        }
        
        /// <summary>
        /// 计算目标评分
        /// </summary>
        private float CalculateTargetScore(PerceptionTarget target)
        {
            float score = 0f;
            
            // 距离因素（越近越好）
            score += (viewRadius - target.Distance) * 10f;
            
            // 感知类型权重
            switch (target.PerceptionType)
            {
                case PerceptionType.Visual:
                    score += 100f;
                    break;
                case PerceptionType.Auditory:
                    score += 50f;
                    break;
                case PerceptionType.Memory:
                    score += 25f;
                    break;
            }
            
            return score;
        }
        
        #endregion

        #region 公共方法
        
        /// <summary>
        /// 检查目标是否在视野内
        /// </summary>
        /// <param name="target">目标Transform</param>
        /// <returns>是否在视野内</returns>
        public bool IsInSight(Transform target)
        {
            return _detectedTargets.ContainsKey(target) && _detectedTargets[target].IsInSight;
        }
        
        /// <summary>
        /// 获取目标的最后已知位置
        /// </summary>
        /// <param name="target">目标Transform</param>
        /// <returns>最后已知位置</returns>
        public Vector3? GetLastKnownPosition(Transform target)
        {
            if (_memorizedTargets.TryGetValue(target, out var perceptionTarget))
            {
                return perceptionTarget.LastKnownPosition;
            }
            return null;
        }
        
        /// <summary>
        /// 清除所有目标
        /// </summary>
        public void ClearAllTargets()
        {
            _detectedTargets.Clear();
            _memorizedTargets.Clear();
            _primaryTarget = null;
        }
        
        /// <summary>
        /// 强制设置目标
        /// </summary>
        /// <param name="target">目标Transform</param>
        public void ForceSetTarget(Transform target)
        {
            if (target == null) return;
            
            float distance = Vector3.Distance(transform.position, target.position);
            RegisterTarget(target, PerceptionType.Visual, distance);
        }
        
        #endregion

        #region 调试可视化
        
        private void DrawViewGizmos()
        {
            Gizmos.color = Color.yellow;
            
            // 绘制视野扇形
            Vector3 forward = transform.right;
            float halfAngle = viewAngle / 2f;
            
            Vector3 leftBoundary = Quaternion.Euler(0, 0, halfAngle) * forward;
            Vector3 rightBoundary = Quaternion.Euler(0, 0, -halfAngle) * forward;
            
            Gizmos.DrawLine(EyePosition, EyePosition + leftBoundary * viewRadius);
            Gizmos.DrawLine(EyePosition, EyePosition + rightBoundary * viewRadius);
            
            // 绘制视野弧线
            int segments = 20;
            float angleStep = viewAngle / segments;
            Vector3 prevPoint = EyePosition + rightBoundary * viewRadius;
            
            for (int i = 1; i <= segments; i++)
            {
                float angle = -halfAngle + angleStep * i;
                Vector3 direction = Quaternion.Euler(0, 0, angle) * forward;
                Vector3 point = EyePosition + direction * viewRadius;
                Gizmos.DrawLine(prevPoint, point);
                prevPoint = point;
            }
        }
        
        private void DrawHearingGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, hearingRadius);
        }
        
        private void DrawTargetGizmos()
        {
            // 绘制检测到的目标
            foreach (var target in _detectedTargets.Values)
            {
                if (target.IsInSight)
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(EyePosition, target.Position);
                    Gizmos.DrawWireSphere(target.Position, 0.5f);
                }
            }
            
            // 绘制记忆中的目标
            foreach (var target in _memorizedTargets.Values)
            {
                if (!target.IsInSight)
                {
                    Gizmos.color = Color.gray;
                    Gizmos.DrawWireSphere(target.LastKnownPosition, 0.3f);
                }
            }
            
            // 高亮主要目标
            if (_primaryTarget != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(_primaryTarget.Position, 0.8f);
            }
        }
        
        #endregion
    }

    /// <summary>
    /// 噪音制造者接口
    /// </summary>
    public interface INoiseMaker
    {
        /// <summary>
        /// 是否正在制造噪音
        /// </summary>
        bool IsMakingNoise { get; }
        
        /// <summary>
        /// 噪音强度
        /// </summary>
        float NoiseIntensity { get; }
    }
}
