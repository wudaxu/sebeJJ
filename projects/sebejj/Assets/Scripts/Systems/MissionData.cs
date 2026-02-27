using System;
using System.Collections.Generic;
using UnityEngine;

namespace SebeJJ.Systems
{
    /// <summary>
    /// 委托数据 - 可序列化的委托配置
    /// </summary>
    [CreateAssetMenu(fileName = "NewMission", menuName = "SebeJJ/Mission Data")]
    public class MissionData : ScriptableObject
    {
        [Header("基础信息")]
        public string missionId;
        public string title;
        [TextArea(3, 5)]
        public string description;
        public MissionType type;
        public int difficulty = 1; // 1-5
        
        [Header("目标")]
        public List<MissionObjective> objectives = new List<MissionObjective>();
        
        [Header("限制")]
        public float timeLimit = 0f; // 0 = 无限制
        public float minDepth = 0f;
        public float maxDepth = 1000f;
        
        [Header("奖励")]
        public int rewardCredits = 100;
        public List<RewardItem> rewardItems = new List<RewardItem>();
        public float reputationReward = 10f;
        
        [Header("解锁条件")]
        public int requiredReputation = 0;
        public string requiredMissionId = "";
    }
    
    /// <summary>
    /// 委托目标
    /// </summary>
    [Serializable]
    public class MissionObjective
    {
        public string objectiveId;
        public string description;
        public string targetId; // 目标ID（资源ID、区域ID等）
        public int requiredAmount = 1;
        [HideInInspector]
        public int currentAmount = 0;
        
        public bool IsComplete => currentAmount >= requiredAmount;
        public float Progress => requiredAmount > 0 ? (float)currentAmount / requiredAmount : 0f;
    }
    
    /// <summary>
    /// 奖励物品
    /// </summary>
    [Serializable]
    public class RewardItem
    {
        public string itemId;
        public string itemName;
        public int quantity = 1;
    }
    
    /// <summary>
    /// 运行时委托实例
    /// </summary>
    [Serializable]
    public class Mission
    {
        public string MissionId { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public MissionType Type { get; private set; }
        public int Difficulty { get; private set; }
        public List<MissionObjective> Objectives { get; private set; }
        public float TimeLimit { get; private set; }
        public float MinDepth { get; private set; }
        public float MaxDepth { get; private set; }
        public int RewardCredits { get; private set; }
        public List<RewardItem> RewardItems { get; private set; }
        public float ReputationReward { get; private set; }
        
        public MissionStatus Status { get; set; } = MissionStatus.Available;
        public DateTime AcceptTime { get; set; }
        public DateTime CompleteTime { get; set; }
        public int CurrentProgress { get; set; }
        public int TargetProgress { get; private set; }
        
        public Mission(MissionData data)
        {
            MissionId = data.missionId;
            Title = data.title;
            Description = data.description;
            Type = data.type;
            Difficulty = data.difficulty;
            TimeLimit = data.timeLimit;
            MinDepth = data.minDepth;
            MaxDepth = data.maxDepth;
            RewardCredits = data.rewardCredits;
            ReputationReward = data.reputationReward;
            
            // QF-001修复: 深拷贝目标列表并重置计数器
            Objectives = new List<MissionObjective>();
            if (data.objectives != null)
            {
                foreach (var obj in data.objectives)
                {
                    if (obj != null)
                    {
                        Objectives.Add(new MissionObjective
                        {
                            objectiveId = obj.objectiveId,
                            description = obj.description,
                            targetId = obj.targetId,
                            requiredAmount = obj.requiredAmount,
                            currentAmount = 0 // QF-001修复: 确保重置为0
                        });
                    }
                }
            }
            
            // 深拷贝奖励列表
            RewardItems = new List<RewardItem>();
            if (data.rewardItems != null)
            {
                foreach (var item in data.rewardItems)
                {
                    if (item != null)
                    {
                        RewardItems.Add(new RewardItem
                        {
                            itemId = item.itemId,
                            itemName = item.itemName,
                            quantity = item.quantity
                        });
                    }
                }
            }
            
            // 计算目标进度
            CalculateTargetProgress();
        }
        
        private void CalculateTargetProgress()
        {
            TargetProgress = 0;
            foreach (var obj in Objectives)
            {
                TargetProgress += obj.requiredAmount;
            }
        }
        
        /// <summary>
        /// MS-001修复: 使用游戏内时间而非系统时间
        /// </summary>
        public float AcceptTimeGameTime { get; set; }
        
        /// <summary>
        /// 是否已过期
        /// MS-001修复: 使用Time.time而非DateTime.Now
        /// </summary>
        public bool IsExpired()
        {
            if (TimeLimit <= 0) return false;
            if (Status != MissionStatus.Active) return false;
            
            // MS-001修复: 使用游戏内时间
            return (Time.time - AcceptTimeGameTime) > TimeLimit;
        }
        
        /// <summary>
        /// 获取剩余时间（秒）
        /// MS-001修复: 使用Time.time
        /// </summary>
        public float GetRemainingTime()
        {
            if (TimeLimit <= 0) return -1f;
            if (Status != MissionStatus.Active) return -1f;
            
            float elapsed = Time.time - AcceptTimeGameTime;
            return Mathf.Max(0f, TimeLimit - elapsed);
        }
        
        /// <summary>
        /// 获取总体进度百分比
        /// </summary>
        public float GetOverallProgress()
        {
            if (TargetProgress <= 0) return 0f;
            return Mathf.Clamp01((float)CurrentProgress / TargetProgress);
        }
        
        /// <summary>
        /// 检查是否满足深度要求
        /// </summary>
        public bool IsDepthValid(float currentDepth)
        {
            return currentDepth >= MinDepth && currentDepth <= MaxDepth;
        }
    }
}
