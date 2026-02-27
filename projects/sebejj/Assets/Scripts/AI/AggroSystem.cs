using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SebeJJ.AI
{
    /// <summary>
    /// 仇恨系统 - BUG-008修复
    /// 管理敌人对目标的仇恨值
    /// </summary>
    public class AggroSystem : MonoBehaviour
    {
        [Header("仇恨设置")]
        [SerializeField] private float decayRate = 5f; // 每秒衰减值
        [SerializeField] private float maxAggro = 1000f;
        [SerializeField] private bool useDistanceModifier = true;
        
        // 仇恨表: 目标 -> 仇恨值
        private Dictionary<Transform, float> aggroTable = new Dictionary<Transform, float>();
        
        // 当前最高仇恨目标
        public Transform PrimaryTarget { get; private set; }
        
        // 事件
        public event Action<Transform> OnTargetChanged;
        public event Action<Transform, float> OnAggroAdded;
        
        private void Update()
        {
            // 衰减仇恨值
            DecayAggro();
            
            // 更新主要目标
            UpdatePrimaryTarget();
        }
        
        /// <summary>
        /// 添加仇恨值
        /// </summary>
        public void AddAggro(Transform target, float amount, AggroType type = AggroType.Damage)
        {
            if (target == null) return;
            
            // 根据类型应用倍率
            float multiplier = GetAggroMultiplier(type);
            float finalAmount = amount * multiplier;
            
            // 添加距离修正
            if (useDistanceModifier)
            {
                float distance = Vector3.Distance(transform.position, target.position);
                float distanceModifier = Mathf.Clamp01(1f - (distance / 50f)); // 距离越近仇恨越高
                finalAmount *= (0.5f + 0.5f * distanceModifier);
            }
            
            // 添加到仇恨表
            if (!aggroTable.ContainsKey(target))
            {
                aggroTable[target] = 0;
            }
            
            aggroTable[target] = Mathf.Min(aggroTable[target] + finalAmount, maxAggro);
            
            OnAggroAdded?.Invoke(target, finalAmount);
            
            Debug.Log($"[AggroSystem] {target.name} 获得 {finalAmount} 仇恨值 (类型: {type})");
        }
        
        /// <summary>
        /// 获取仇恨值倍率
        /// </summary>
        private float GetAggroMultiplier(AggroType type)
        {
            return type switch
            {
                AggroType.Damage => 1.0f,
                AggroType.Healing => 0.5f,
                AggroType.Threat => 1.5f,
                AggroType.Proximity => 0.3f,
                AggroType.Sound => 0.2f,
                _ => 1.0f
            };
        }
        
        /// <summary>
        /// 衰减仇恨值
        /// </summary>
        private void DecayAggro()
        {
            var targets = new List<Transform>(aggroTable.Keys);
            
            foreach (var target in targets)
            {
                if (target == null)
                {
                    aggroTable.Remove(target);
                    continue;
                }
                
                // 如果是当前目标，衰减较慢
                float decayMultiplier = (target == PrimaryTarget) ? 0.5f : 1f;
                aggroTable[target] -= decayRate * decayMultiplier * Time.deltaTime;
                
                // 移除过期的仇恨
                if (aggroTable[target] <= 0)
                {
                    aggroTable.Remove(target);
                }
            }
        }
        
        /// <summary>
        /// 更新主要目标
        /// </summary>
        private void UpdatePrimaryTarget()
        {
            Transform newTarget = GetHighestAggroTarget();
            
            if (newTarget != PrimaryTarget)
            {
                PrimaryTarget = newTarget;
                OnTargetChanged?.Invoke(PrimaryTarget);
                
                if (PrimaryTarget != null)
                {
                    Debug.Log($"[AggroSystem] 切换目标为: {PrimaryTarget.name}");
                }
            }
        }
        
        /// <summary>
        /// 获取最高仇恨目标
        /// </summary>
        public Transform GetHighestAggroTarget()
        {
            if (aggroTable.Count == 0) return null;
            
            return aggroTable.OrderByDescending(x => x.Value).FirstOrDefault().Key;
        }
        
        /// <summary>
        /// 获取目标的仇恨值
        /// </summary>
        public float GetAggro(Transform target)
        {
            if (target == null) return 0;
            return aggroTable.TryGetValue(target, out float value) ? value : 0;
        }
        
        /// <summary>
        /// 移除目标的仇恨
        /// </summary>
        public void RemoveTarget(Transform target)
        {
            if (target != null && aggroTable.ContainsKey(target))
            {
                aggroTable.Remove(target);
                
                if (target == PrimaryTarget)
                {
                    UpdatePrimaryTarget();
                }
            }
        }
        
        /// <summary>
        /// 清除所有仇恨
        /// </summary>
        public void ClearAllAggro()
        {
            aggroTable.Clear();
            PrimaryTarget = null;
        }
        
        /// <summary>
        /// 强制设置目标（无视仇恨值）
        /// </summary>
        public void ForceSetTarget(Transform target)
        {
            if (target == null) return;
            
            // 给目标一个很高的仇恨值
            aggroTable[target] = maxAggro;
            PrimaryTarget = target;
            OnTargetChanged?.Invoke(PrimaryTarget);
        }
        
        /// <summary>
        /// 获取所有有仇恨的目标
        /// </summary>
        public List<Transform> GetAllTargets()
        {
            return new List<Transform>(aggroTable.Keys);
        }
        
        /// <summary>
        /// 是否有任何仇恨目标
        /// </summary>
        public bool HasAnyTarget()
        {
            return aggroTable.Count > 0;
        }
        
        private void OnDrawGizmos()
        {
            // 绘制仇恨目标连线
            if (PrimaryTarget != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, PrimaryTarget.position);
            }
            
            // 绘制其他仇恨目标
            Gizmos.color = Color.yellow;
            foreach (var target in aggroTable.Keys)
            {
                if (target != null && target != PrimaryTarget)
                {
                    Gizmos.DrawLine(transform.position, target.position);
                }
            }
        }
    }
    
    /// <summary>
    /// 仇恨类型
    /// </summary>
    public enum AggroType
    {
        Damage,     // 造成伤害
        Healing,    // 治疗
        Threat,     // 威胁技能
        Proximity,  // 靠近
        Sound       // 声音
    }
}
