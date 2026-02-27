using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SebeJJ.Systems
{
    /// <summary>
    /// 委托推荐系统 - BUG-022修复
    /// 基于玩家状态推荐合适的委托
    /// </summary>
    public class MissionRecommender : MonoBehaviour
    {
        public static MissionRecommender Instance { get; private set; }
        
        [Header("推荐设置")]
        [SerializeField] private int defaultRecommendationCount = 3;
        [SerializeField] private float difficultyTolerance = 0.5f; // 难度容差
        [SerializeField] private bool considerPlayerHistory = true;
        [SerializeField] private bool considerCurrentEquipment = true;
        
        [Header("权重设置")]
        [SerializeField] private float difficultyMatchWeight = 0.4f;
        [SerializeField] private float rewardValueWeight = 0.25f;
        [SerializeField] private float playerPreferenceWeight = 0.2f;
        [SerializeField] private float missionTypeWeight = 0.15f;
        
        // 玩家历史数据
        private Dictionary<MissionType, int> missionTypeHistory = new Dictionary<MissionType, int>();
        private Dictionary<string, int> completedMissionIds = new Dictionary<string, int>();
        
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
            SubscribeToMissionEvents();
            LoadPlayerHistory();
        }
        
        private void SubscribeToMissionEvents()
        {
            var missionManager = MissionManager.Instance;
            if (missionManager != null)
            {
                missionManager.OnMissionCompleted += OnMissionCompleted;
            }
        }
        
        /// <summary>
        /// 获取推荐委托
        /// </summary>
        public List<Mission> GetRecommendedMissions(int count = -1)
        {
            if (count < 0) count = defaultRecommendationCount;
            
            var candidates = GetAvailableMissionCandidates();
            if (candidates.Count == 0) return new List<Mission>();
            
            var scoredMissions = new List<ScoredMission>();
            
            foreach (var mission in candidates)
            {
                float score = CalculateMissionScore(mission);
                scoredMissions.Add(new ScoredMission(mission, score));
            }
            
            // 排序并返回前N个
            return scoredMissions
                .OrderByDescending(x => x.score)
                .Take(count)
                .Select(x => x.mission)
                .ToList();
        }
        
        /// <summary>
        /// 计算委托评分
        /// </summary>
        private float CalculateMissionScore(Mission mission)
        {
            float score = 0;
            
            // 1. 难度匹配度 (40%)
            score += CalculateDifficultyMatch(mission) * difficultyMatchWeight;
            
            // 2. 奖励价值 (25%)
            score += CalculateRewardValue(mission) * rewardValueWeight;
            
            // 3. 玩家偏好 (20%)
            if (considerPlayerHistory)
            {
                score += CalculatePlayerPreference(mission) * playerPreferenceWeight;
            }
            
            // 4. 委托类型多样性 (15%)
            score += CalculateTypeDiversity(mission) * missionTypeWeight;
            
            // 5. 装备匹配度
            if (considerCurrentEquipment)
            {
                score += CalculateEquipmentMatch(mission) * 0.1f;
            }
            
            return score;
        }
        
        /// <summary>
        /// 计算难度匹配度
        /// </summary>
        private float CalculateDifficultyMatch(Mission mission)
        {
            int playerLevel = GetPlayerLevel();
            int missionDifficulty = mission.Difficulty;
            
            // 理想难度是玩家等级或略高
            float idealDifficulty = playerLevel;
            float diff = Mathf.Abs(missionDifficulty - idealDifficulty);
            
            // 在容差范围内给高分
            if (diff <= difficultyTolerance)
            {
                return 1f;
            }
            
            // 超出容差递减
            return Mathf.Max(0, 1f - (diff - difficultyTolerance) * 0.3f);
        }
        
        /// <summary>
        /// 计算奖励价值
        /// </summary>
        private float CalculateRewardValue(Mission mission)
        {
            float baseScore = mission.RewardCredits / 1000f; // 归一化
            
            // 考虑时间效率
            float estimatedTime = EstimateMissionTime(mission);
            float timeEfficiency = estimatedTime > 0 ? 1f / estimatedTime : 0.5f;
            
            // 考虑物品奖励
            float itemBonus = mission.RewardItems?.Count > 0 ? 0.2f : 0f;
            
            return Mathf.Min(1f, baseScore * 0.6f + timeEfficiency * 0.3f + itemBonus);
        }
        
        /// <summary>
        /// 计算玩家偏好
        /// </summary>
        private float CalculatePlayerPreference(Mission mission)
        {
            if (missionTypeHistory.Count == 0) return 0.5f;
            
            // 基于历史完成数量计算偏好
            int totalCompleted = missionTypeHistory.Values.Sum();
            if (totalCompleted == 0) return 0.5f;
            
            int typeCompleted = missionTypeHistory.TryGetValue(mission.Type, out int count) ? count : 0;
            
            // 完成越多越偏好，但避免过度偏向
            return 0.3f + (typeCompleted / (float)totalCompleted) * 0.7f;
        }
        
        /// <summary>
        /// 计算类型多样性
        /// </summary>
        private float CalculateTypeDiversity(Mission mission)
        {
            // 如果最近完成的委托类型与此相同，降低评分
            var recentMissions = GetRecentCompletedMissions(5);
            int sameTypeCount = recentMissions.Count(m => m.Type == mission.Type);
            
            return 1f - (sameTypeCount * 0.15f);
        }
        
        /// <summary>
        /// 计算装备匹配度
        /// </summary>
        private float CalculateEquipmentMatch(Mission mission)
        {
            // 根据委托类型检查装备
            float matchScore = 0.5f;
            
            switch (mission.Type)
            {
                case MissionType.Collection:
                    // 检查货舱容量
                    // matchScore = GetCargoCapacityScore();
                    break;
                    
                case MissionType.Elimination:
                    // 检查武器配置
                    // matchScore = GetCombatReadinessScore();
                    break;
                    
                case MissionType.Exploration:
                    // 检查能量和耐久
                    // matchScore = GetExplorationReadinessScore();
                    break;
            }
            
            return matchScore;
        }
        
        /// <summary>
        /// 获取可用委托候选
        /// </summary>
        private List<Mission> GetAvailableMissionCandidates()
        {
            var missionManager = MissionManager.Instance;
            if (missionManager == null) return new List<Mission>();
            
            // 获取所有可用委托
            var candidates = missionManager.AvailableMissions
                .Where(m => m.Status == MissionStatus.Available)
                .Where(m => !completedMissionIds.ContainsKey(m.MissionId))
                .ToList();
            
            return candidates;
        }
        
        /// <summary>
        /// 估计委托完成时间
        /// </summary>
        private float EstimateMissionTime(Mission mission)
        {
            // 基于委托类型和难度估计时间
            float baseTime = mission.Type switch
            {
                MissionType.Collection => 5f,
                MissionType.Elimination => 10f,
                MissionType.Exploration => 8f,
                MissionType.Scan => 3f,
                MissionType.Delivery => 6f,
                _ => 5f
            };
            
            return baseTime * (1 + mission.Difficulty * 0.2f);
        }
        
        /// <summary>
        /// 获取玩家等级
        /// </summary>
        private int GetPlayerLevel()
        {
            // 从游戏管理器获取玩家等级
            // 临时返回一个基于完成委托数的估算值
            int completedCount = completedMissionIds.Count;
            return 1 + (completedCount / 5);
        }
        
        /// <summary>
        /// 委托完成回调
        /// </summary>
        private void OnMissionCompleted(Mission mission)
        {
            if (mission == null) return;
            
            // 更新历史数据
            if (!missionTypeHistory.ContainsKey(mission.Type))
            {
                missionTypeHistory[mission.Type] = 0;
            }
            missionTypeHistory[mission.Type]++;
            
            completedMissionIds[mission.MissionId] = (int)DateTime.Now.Ticks;
            
            // 保存历史
            SavePlayerHistory();
        }
        
        /// <summary>
        /// 获取最近完成的委托
        /// </summary>
        private List<MissionData> GetRecentCompletedMissions(int count)
        {
            // 从存档获取最近完成的委托
            return new List<MissionData>();
        }
        
        /// <summary>
        /// 保存玩家历史
        /// </summary>
        private void SavePlayerHistory()
        {
            // 保存到PlayerPrefs或存档系统
            string historyJson = JsonUtility.ToJson(new MissionHistoryData
            {
                typeHistory = missionTypeHistory,
                completedIds = completedMissionIds
            });
            
            PlayerPrefs.SetString("MissionHistory", historyJson);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// 加载玩家历史
        /// </summary>
        private void LoadPlayerHistory()
        {
            string historyJson = PlayerPrefs.GetString("MissionHistory", "");
            if (!string.IsNullOrEmpty(historyJson))
            {
                var data = JsonUtility.FromJson<MissionHistoryData>(historyJson);
                if (data != null)
                {
                    missionTypeHistory = data.typeHistory ?? new Dictionary<MissionType, int>();
                    completedMissionIds = data.completedIds ?? new Dictionary<string, int>();
                }
            }
        }
        
        /// <summary>
        /// 获取推荐理由
        /// </summary>
        public string GetRecommendationReason(Mission mission)
        {
            var reasons = new List<string>();
            
            int playerLevel = GetPlayerLevel();
            
            if (mission.Difficulty <= playerLevel + 1)
            {
                reasons.Add("难度适中");
            }
            
            if (mission.RewardCredits > 500)
            {
                reasons.Add("奖励丰厚");
            }
            
            if (missionTypeHistory.TryGetValue(mission.Type, out int count) && count > 3)
            {
                reasons.Add("符合您的偏好");
            }
            
            if (reasons.Count == 0)
            {
                return "推荐委托";
            }
            
            return string.Join("，", reasons);
        }
    }
    
    /// <summary>
    /// 带评分的委托
    /// </summary>
    public class ScoredMission
    {
        public Mission mission;
        public float score;
        
        public ScoredMission(Mission mission, float score)
        {
            this.mission = mission;
            this.score = score;
        }
    }
    
    /// <summary>
    /// 委托历史数据
    /// </summary>
    [Serializable]
    public class MissionHistoryData
    {
        public Dictionary<MissionType, int> typeHistory;
        public Dictionary<string, int> completedIds;
    }
}
