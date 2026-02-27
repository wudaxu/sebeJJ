using System.Collections.Generic;
using UnityEngine;

namespace SebeJJ.Experience.Analytics
{
    /// <summary>
    /// 痛点检测器 - 自动识别玩家遇到的困难
    /// </summary>
    public class PainPointDetector : MonoBehaviour
    {
        public static PainPointDetector Instance { get; private set; }
        
        [Header("检测配置")]
        [SerializeField] private bool enableDetection = true;
        [SerializeField] private float checkInterval = 30f;
        
        [Header("死亡检测")]
        [SerializeField] private int deathThreshold = 3; // 短时间内死亡次数阈值
        [SerializeField] private float deathWindow = 300f; // 5分钟窗口
        
        [Header("进度检测")]
        [SerializeField] private float stuckThreshold = 600f; // 10分钟无进展视为卡住
        
        [Header("战斗检测")]
        [SerializeField] private float combatDifficultyThreshold = 0.7f; // 战斗难度阈值
        
        private Queue<DeathRecord> recentDeaths = new Queue<DeathRecord>();
        private float lastProgressTime = 0f;
        private string currentMissionId = "";
        private int currentMissionAttempts = 0;
        
        public System.Action<PainPoint> OnPainPointDetected;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Start()
        {
            InvokeRepeating(nameof(CheckPainPoints), checkInterval, checkInterval);
        }
        
        /// <summary>
        /// 记录死亡
        /// </summary>
        public void RecordDeath(float depth, string cause, string missionId)
        {
            recentDeaths.Enqueue(new DeathRecord
            {
                Timestamp = Time.time,
                Depth = depth,
                Cause = cause,
                MissionId = missionId
            });
            
            // 清理过期记录
            while (recentDeaths.Count > 0 && Time.time - recentDeaths.Peek().Timestamp > deathWindow)
            {
                recentDeaths.Dequeue();
            }
            
            // 检查是否频繁死亡
            if (recentDeaths.Count >= deathThreshold)
            {
                DetectFrequentDeathPainPoint();
            }
            
            // 记录任务尝试
            if (!string.IsNullOrEmpty(missionId) && missionId == currentMissionId)
            {
                currentMissionAttempts++;
                
                if (currentMissionAttempts >= 3)
                {
                    DetectMissionStuckPainPoint(missionId);
                }
            }
        }
        
        /// <summary>
        /// 记录进度
        /// </summary>
        public void RecordProgress(ProgressType type, string details)
        {
            lastProgressTime = Time.time;
            
            if (type == ProgressType.MissionStarted)
            {
                currentMissionId = details;
                currentMissionAttempts = 0;
            }
            else if (type == ProgressType.MissionCompleted && details == currentMissionId)
            {
                currentMissionId = "";
                currentMissionAttempts = 0;
            }
        }
        
        /// <summary>
        /// 检查痛点
        /// </summary>
        private void CheckPainPoints()
        {
            if (!enableDetection) return;
            
            CheckStuckPainPoint();
            CheckExplorationPainPoint();
            CheckResourcePainPoint();
        }
        
        /// <summary>
        /// 检测频繁死亡痛点
        /// </summary>
        private void DetectFrequentDeathPainPoint()
        {
            var painPoint = new PainPoint
            {
                Type = PainPointType.FrequentDeath,
                Severity = PainPointSeverity.High,
                Description = $"5分钟内死亡{recentDeaths.Count}次",
                Context = GatherDeathContext(),
                SuggestedAction = "降低当前区域难度或建议升级装备",
                Timestamp = Time.time
            };
            
            ReportPainPoint(painPoint);
        }
        
        /// <summary>
        /// 检测任务卡住痛点
        /// </summary>
        private void DetectMissionStuckPainPoint(string missionId)
        {
            var painPoint = new PainPoint
            {
                Type = PainPointType.MissionStuck,
                Severity = PainPointSeverity.Medium,
                Description = $"委托 {missionId} 尝试{currentMissionAttempts}次未成功",
                Context = new Dictionary<string, object>
                {
                    { "mission_id", missionId },
                    { "attempts", currentMissionAttempts },
                    { "recent_deaths", GetRecentDeathsInMission(missionId) }
                },
                SuggestedAction = "提供委托攻略提示或建议降低难度",
                Timestamp = Time.time
            };
            
            ReportPainPoint(painPoint);
        }
        
        /// <summary>
        /// 检测卡住痛点
        /// </summary>
        private void CheckStuckPainPoint()
        {
            float timeSinceProgress = Time.time - lastProgressTime;
            
            if (timeSinceProgress > stuckThreshold)
            {
                var painPoint = new PainPoint
                {
                    Type = PainPointType.NoProgress,
                    Severity = PainPointSeverity.Medium,
                    Description = $"{timeSinceProgress / 60f:F0}分钟无进展",
                    Context = new Dictionary<string, object>
                    {
                        { "time_since_progress", timeSinceProgress },
                        { "current_mission", currentMissionId }
                    },
                    SuggestedAction = "显示帮助提示或推荐其他活动",
                    Timestamp = Time.time
                };
                
                ReportPainPoint(painPoint);
            }
        }
        
        /// <summary>
        /// 检测探索痛点
        /// </summary>
        private void CheckExplorationPainPoint()
        {
            // 检查玩家是否在同一区域徘徊过久
            // if (PlayerController.Instance.TimeInSameArea > 180f)
            // {
            //     ReportPainPoint(new PainPoint
            //     {
            //         Type = PainPointType.Lost,
            //         Severity = PainPointSeverity.Low,
            //         Description = "在相同区域停留过久",
            //         SuggestedAction = "显示导航提示"
            //     });
            // }
        }
        
        /// <summary>
        /// 检测资源痛点
        /// </summary>
        private void CheckResourcePainPoint()
        {
            // 检查资源是否不足
            // if (InventoryManager.Instance.IsNearlyEmpty && PlayerController.Instance.TimeSinceLastResource > 120f)
            // {
            //     ReportPainPoint(new PainPoint
            //     {
            //         Type = PainPointType.LowResources,
            //         Severity = PainPointSeverity.Low,
            //         Description = "长时间未采集到资源",
            //         SuggestedAction = "高亮显示附近资源"
            //     });
            // }
        }
        
        /// <summary>
        /// 收集死亡上下文
        /// </summary>
        private Dictionary<string, object> GatherDeathContext()
        {
            var context = new Dictionary<string, object>();
            
            var depths = new List<float>();
            var causes = new Dictionary<string, int>();
            
            foreach (var death in recentDeaths)
            {
                depths.Add(death.Depth);
                
                if (!causes.ContainsKey(death.Cause))
                    causes[death.Cause] = 0;
                causes[death.Cause]++;
            }
            
            context["average_depth"] = CalculateAverage(depths);
            context["death_causes"] = causes;
            context["mission_attempts"] = currentMissionAttempts;
            
            return context;
        }
        
        /// </summary>
        /// 获取任务中的最近死亡
        /// </summary>
        private int GetRecentDeathsInMission(string missionId)
        {
            int count = 0;
            foreach (var death in recentDeaths)
            {
                if (death.MissionId == missionId)
                    count++;
            }
            return count;
        }
        
        /// <summary>
        /// 报告痛点
        /// </summary>
        private void ReportPainPoint(PainPoint painPoint)
        {
            OnPainPointDetected?.Invoke(painPoint);
            
            // 发送到分析系统
            // AnalyticsManager.Log("pain_point", painPoint.ToDictionary());
            
            // 应用缓解措施
            ApplyMitigation(painPoint);
            
            Debug.Log($"[PainPointDetector] 检测到痛点: {painPoint.Type} - {painPoint.Description}");
        }
        
        /// </summary>
        /// 应用缓解措施
        /// </summary>
        private void ApplyMitigation(PainPoint painPoint)
        {
            switch (painPoint.Type)
            {
                case PainPointType.FrequentDeath:
                    // 降低难度
                    // DifficultyManager.Instance.DebugDecreaseDifficulty();
                    // 显示帮助提示
                    // UIManager.Instance.ShowHint("检测到您遇到了困难，已适当调整难度");
                    break;
                    
                case PainPointType.MissionStuck:
                    // 显示攻略
                    // UIManager.Instance.ShowMissionHint(currentMissionId);
                    break;
                    
                case PainPointType.NoProgress:
                    // 推荐其他活动
                    // UIManager.Instance.ShowActivityRecommendation();
                    break;
                    
                case PainPointType.Lost:
                    // 显示导航
                    // UIManager.Instance.ShowNavigationArrow();
                    break;
                    
                case PainPointType.LowResources:
                    // 高亮资源
                    // ResourceManager.Instance.HighlightNearbyResources();
                    break;
            }
        }
        
        private float CalculateAverage(List<float> values)
        {
            if (values.Count == 0) return 0f;
            
            float sum = 0f;
            foreach (var v in values) sum += v;
            return sum / values.Count;
        }
    }
    
    /// <summary>
    /// 痛点数据
    /// </summary>
    public class PainPoint
    {
        public PainPointType Type;
        public PainPointSeverity Severity;
        public string Description;
        public Dictionary<string, object> Context;
        public string SuggestedAction;
        public float Timestamp;
    }
    
    /// </summary>
    /// 痛点类型
    /// </summary>
    public enum PainPointType
    {
        FrequentDeath,      // 频繁死亡
        MissionStuck,       // 任务卡住
        NoProgress,         // 无进展
        Lost,               // 迷路
        LowResources,       // 资源不足
        CombatTooHard,      // 战斗太难
        Confused,           // 困惑
        Boredom             // 无聊
    }
    
    /// </summary>
    /// 痛点严重程度
    /// </summary>
    public enum PainPointSeverity
    {
        Low,
        Medium,
        High
    }
    
    /// </summary>
    /// 进度类型
    /// </summary>
    public enum ProgressType
    {
        MissionStarted,
        MissionCompleted,
        ResourceCollected,
        EnemyDefeated,
        DepthReached,
        EquipmentUpgraded
    }
    
    /// </summary>
    /// 死亡记录
    /// </summary>
    public class DeathRecord
    {
        public float Timestamp;
        public float Depth;
        public string Cause;
        public string MissionId;
    }
}
