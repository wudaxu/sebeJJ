using System.Collections.Generic;
using UnityEngine;

namespace SebeJJ.Experience.Analytics
{
    /// <summary>
    /// 玩家旅程追踪器 - 记录玩家在游戏中的完整旅程
    /// </summary>
    public class PlayerJourneyTracker : MonoBehaviour
    {
        public static PlayerJourneyTracker Instance { get; private set; }
        
        [Header("追踪配置")]
        [SerializeField] private bool enableTracking = true;
        [SerializeField] private float checkpointInterval = 300f; // 5分钟记录一次
        
        private PlayerJourneyData journeyData;
        private float sessionStartTime;
        private float lastCheckpointTime;
        private List<JourneyEvent> eventHistory = new List<JourneyEvent>();
        
        public PlayerJourneyData JourneyData => journeyData;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void Start()
        {
            InitializeJourney();
        }
        
        private void Update()
        {
            if (!enableTracking) return;
            
            // 定期检查点
            if (Time.time - lastCheckpointTime >= checkpointInterval)
            {
                RecordCheckpoint();
            }
        }
        
        /// <summary>
        /// 初始化旅程
        /// </summary>
        private void InitializeJourney()
        {
            journeyData = new PlayerJourneyData
            {
                PlayerId = SystemInfo.deviceUniqueIdentifier,
                StartTime = System.DateTime.Now,
                CurrentStage = JourneyStage.Discovery
            };
            
            sessionStartTime = Time.time;
            lastCheckpointTime = Time.time;
            
            // 记录首次启动
            RecordEvent(JourneyEventType.FirstLaunch, "玩家首次启动游戏");
        }
        
        /// <summary>
        /// 记录事件
        /// </summary>
        public void RecordEvent(JourneyEventType eventType, string description, Dictionary<string, object> parameters = null)
        {
            if (!enableTracking) return;
            
            var journeyEvent = new JourneyEvent
            {
                Timestamp = System.DateTime.Now,
                SessionTime = Time.time - sessionStartTime,
                EventType = eventType,
                Description = description,
                Parameters = parameters ?? new Dictionary<string, object>(),
                CurrentStage = journeyData.CurrentStage,
                CurrentDepth = GetCurrentDepth(),
                PlayerLevel = GetPlayerLevel()
            };
            
            eventHistory.Add(journeyEvent);
            journeyData.TotalEvents++;
            
            // 检查阶段转换
            CheckStageTransition(eventType);
            
            // 发送到分析系统
            // AnalyticsManager.Log("journey_event", journeyEvent.ToDictionary());
            
            Debug.Log($"[JourneyTracker] {eventType}: {description}");
        }
        
        /// <summary>
        /// 记录检查点
        /// </summary>
        private void RecordCheckpoint()
        {
            lastCheckpointTime = Time.time;
            
            var checkpoint = new JourneyCheckpoint
            {
                Timestamp = System.DateTime.Now,
                SessionTime = Time.time - sessionStartTime,
                PlayerLevel = GetPlayerLevel(),
                CurrentDepth = GetCurrentDepth(),
                TotalPlayTime = journeyData.TotalPlayTime + (Time.time - sessionStartTime),
                MissionsCompleted = journeyData.MissionsCompleted,
                ResourcesCollected = journeyData.ResourcesCollected,
                EnemiesDefeated = journeyData.EnemiesDefeated,
                DeepestDepth = journeyData.DeepestDepth
            };
            
            journeyData.Checkpoints.Add(checkpoint);
        }
        
        /// <summary>
        /// 检查阶段转换
        /// </summary>
        private void CheckStageTransition(JourneyEventType eventType)
        {
            var oldStage = journeyData.CurrentStage;
            var newStage = oldStage;
            
            switch (eventType)
            {
                case JourneyEventType.TutorialComplete:
                    if (oldStage == JourneyStage.Discovery)
                        newStage = JourneyStage.Onboarding;
                    break;
                    
                case JourneyEventType.FirstMissionComplete:
                    if (oldStage == JourneyStage.Onboarding)
                        newStage = JourneyStage.Engagement;
                    break;
                    
                case JourneyEventType.FifthMissionComplete:
                    if (oldStage == JourneyStage.Engagement)
                        newStage = JourneyStage.Proficiency;
                    break;
                    
                case JourneyEventType.FirstBossDefeated:
                    if (oldStage == JourneyStage.Proficiency)
                        newStage = JourneyStage.Mastery;
                    break;
                    
                case JourneyEventType.AllContentComplete:
                    if (oldStage == JourneyStage.Mastery)
                        newStage = JourneyStage.Champion;
                    break;
            }
            
            if (newStage != oldStage)
            {
                journeyData.CurrentStage = newStage;
                journeyData.StageEntryTimes[newStage] = System.DateTime.Now;
                
                RecordEvent(JourneyEventType.StageTransition, $"进入阶段: {newStage}", 
                    new Dictionary<string, object> { { "previous_stage", oldStage }, { "new_stage", newStage } });
            }
        }
        
        /// <summary>
        /// 记录委托完成
        /// </summary>
        public void RecordMissionComplete(string missionId, float completionTime, int retryCount)
        {
            journeyData.MissionsCompleted++;
            
            RecordEvent(JourneyEventType.MissionComplete, $"完成委托: {missionId}",
                new Dictionary<string, object>
                {
                    { "mission_id", missionId },
                    { "completion_time", completionTime },
                    { "retry_count", retryCount }
                });
            
            // 检查里程碑
            if (journeyData.MissionsCompleted == 1)
                RecordEvent(JourneyEventType.FirstMissionComplete, "首次完成委托");
            else if (journeyData.MissionsCompleted == 5)
                RecordEvent(JourneyEventType.FifthMissionComplete, "完成5个委托");
        }
        
        /// <summary>
        /// 记录深度记录
        /// </summary>
        public void RecordDepthReached(float depth)
        {
            if (depth > journeyData.DeepestDepth)
            {
                journeyData.DeepestDepth = depth;
                
                RecordEvent(JourneyEventType.NewDepthRecord, $"新深度记录: {depth}米", 
                    new Dictionary<string, object> { { "depth", depth } });
            }
        }
        
        /// <summary>
        /// 记录死亡
        /// </summary>
        public void RecordDeath(float depth, string cause, float sessionDuration)
        {
            journeyData.DeathCount++;
            
            RecordEvent(JourneyEventType.PlayerDeath, $"死亡: {cause}",
                new Dictionary<string, object>
                {
                    { "depth", depth },
                    { "cause", cause },
                    { "session_duration", sessionDuration }
                });
        }
        
        /// <summary>
        /// 获取旅程报告
        /// </summary>
        public JourneyReport GenerateJourneyReport()
        {
            return new JourneyReport
            {
                PlayerId = journeyData.PlayerId,
                TotalPlayTime = journeyData.TotalPlayTime,
                CurrentStage = journeyData.CurrentStage,
                DaysSinceStart = (System.DateTime.Now - journeyData.StartTime).Days,
                TotalSessions = journeyData.Checkpoints.Count,
                MissionsCompleted = journeyData.MissionsCompleted,
                DeepestDepth = journeyData.DeepestDepth,
                DeathCount = journeyData.DeathCount,
                StageProgress = CalculateStageProgress(),
                RecommendedContent = GetRecommendedContent(),
                PainPoints = IdentifyPainPoints()
            };
        }
        
        /// <summary>
        /// 计算阶段进度
        /// </summary>
        private float CalculateStageProgress()
        {
            // 基于当前阶段和已完成的里程碑计算进度
            return journeyData.CurrentStage switch
            {
                JourneyStage.Discovery => 0.1f,
                JourneyStage.Onboarding => 0.25f,
                JourneyStage.Engagement => 0.5f,
                JourneyStage.Proficiency => 0.75f,
                JourneyStage.Mastery => 0.9f,
                JourneyStage.Champion => 1f,
                _ => 0f
            };
        }
        
        /// <summary>
        /// 获取推荐内容
        /// </summary>
        private List<string> GetRecommendedContent()
        {
            var recommendations = new List<string>();
            
            switch (journeyData.CurrentStage)
            {
                case JourneyStage.Discovery:
                    recommendations.Add("完成新手引导");
                    break;
                case JourneyStage.Onboarding:
                    recommendations.Add("尝试不同类型的委托");
                    recommendations.Add("探索30米深度区域");
                    break;
                case JourneyStage.Engagement:
                    recommendations.Add("升级机甲装备");
                    recommendations.Add("挑战50米深度");
                    break;
                case JourneyStage.Proficiency:
                    recommendations.Add("挑战Boss战");
                    recommendations.Add("收集稀有资源");
                    break;
                case JourneyStage.Mastery:
                    recommendations.Add("探索深渊区域");
                    recommendations.Add("完成极限委托");
                    break;
            }
            
            return recommendations;
        }
        
        /// <summary>
        /// 识别痛点
        /// </summary>
        private List<JourneyPainPoint> IdentifyPainPoints()
        {
            var painPoints = new List<JourneyPainPoint>();
            
            // 检查频繁死亡
            if (journeyData.DeathCount > journeyData.MissionsCompleted * 0.5f)
            {
                painPoints.Add(new JourneyPainPoint
                {
                    Type = PainPointType.FrequentDeath,
                    Description = "死亡频率过高",
                    Severity = PainPointSeverity.High
                });
            }
            
            // 检查长时间未进展
            // ...
            
            return painPoints;
        }
        
        private float GetCurrentDepth()
        {
            // return DiveManager.Instance.CurrentDepth;
            return 0f;
        }
        
        private int GetPlayerLevel()
        {
            // return PlayerManager.Instance.Level;
            return 1;
        }
    }
    
    /// <summary>
    /// 玩家旅程数据
    /// </summary>
    [System.Serializable]
    public class PlayerJourneyData
    {
        public string PlayerId;
        public System.DateTime StartTime;
        public JourneyStage CurrentStage;
        public Dictionary<JourneyStage, System.DateTime> StageEntryTimes = new Dictionary<JourneyStage, System.DateTime>();
        
        public float TotalPlayTime;
        public int TotalEvents;
        public int MissionsCompleted;
        public int ResourcesCollected;
        public int EnemiesDefeated;
        public float DeepestDepth;
        public int DeathCount;
        
        public List<JourneyCheckpoint> Checkpoints = new List<JourneyCheckpoint>();
    }
    
    /// <summary>
    /// 旅程阶段
    /// </summary>
    public enum JourneyStage
    {
        Discovery,      // 发现期 (0-30min)
        Onboarding,     // 入门期 (30min-2h)
        Engagement,     // 参与期 (2-5h)
        Proficiency,    // 熟练期 (5-10h)
        Mastery,        // 精通期 (10-20h)
        Champion        // 大师期 (20h+)
    }
    
    /// <summary>
    /// 旅程事件类型
    /// </summary>
    public enum JourneyEventType
    {
        FirstLaunch,
        TutorialComplete,
        FirstDive,
        FirstCollection,
        FirstKill,
        FirstMissionComplete,
        FifthMissionComplete,
        FirstBossDefeated,
        MissionComplete,
        NewDepthRecord,
        PlayerDeath,
        StageTransition,
        SystemUnlocked,
        EquipmentUpgraded,
        AllContentComplete
    }
    
    /// <summary>
    /// 旅程事件
    /// </summary>
    public class JourneyEvent
    {
        public System.DateTime Timestamp;
        public float SessionTime;
        public JourneyEventType EventType;
        public string Description;
        public Dictionary<string, object> Parameters;
        public JourneyStage CurrentStage;
        public float CurrentDepth;
        public int PlayerLevel;
    }
    
    /// <summary>
    /// 旅程检查点
    /// </summary>
    public class JourneyCheckpoint
    {
        public System.DateTime Timestamp;
        public float SessionTime;
        public int PlayerLevel;
        public float CurrentDepth;
        public float TotalPlayTime;
        public int MissionsCompleted;
        public int ResourcesCollected;
        public int EnemiesDefeated;
        public float DeepestDepth;
    }
    
    /// <summary>
    /// 旅程报告
    /// </summary>
    public class JourneyReport
    {
        public string PlayerId;
        public float TotalPlayTime;
        public JourneyStage CurrentStage;
        public int DaysSinceStart;
        public int TotalSessions;
        public int MissionsCompleted;
        public float DeepestDepth;
        public int DeathCount;
        public float StageProgress;
        public List<string> RecommendedContent;
        public List<JourneyPainPoint> PainPoints;
    }
    
    /// <summary>
    /// 旅程痛点
    /// </summary>
    public class JourneyPainPoint
    {
        public PainPointType Type;
        public string Description;
        public PainPointSeverity Severity;
    }
    
    public enum PainPointType
    {
        FrequentDeath,
        StuckOnMission,
        ConfusedBySystem,
        Boredom,
        DifficultySpike
    }
    
    public enum PainPointSeverity
    {
        Low,
        Medium,
        High
    }
}
