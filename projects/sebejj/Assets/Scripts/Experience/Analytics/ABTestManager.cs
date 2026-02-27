using System.Collections.Generic;
using UnityEngine;

namespace SebeJJ.Experience.Analytics
{
    /// <summary>
    /// A/B测试管理器
    /// </summary>
    public class ABTestManager : MonoBehaviour
    {
        public static ABTestManager Instance { get; private set; }
        
        [Header("测试配置")]
        [SerializeField] private List<ABTest> activeTests;
        [SerializeField] private string playerId;
        
        private Dictionary<string, ABTestGroup> playerAssignments = new Dictionary<string, ABTestGroup>();
        private Dictionary<string, ABTestConfig> appliedConfigs = new Dictionary<string, ABTestConfig>();
        
        public System.Action<string, ABTestGroup> OnTestAssigned;
        
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
            // 生成或加载玩家ID
            playerId = LoadOrGeneratePlayerId();
            
            // 分配测试组
            AssignTests();
        }
        
        /// <summary>
        /// 分配测试组
        /// </summary>
        private void AssignTests()
        {
            foreach (var test in activeTests)
            {
                if (!test.IsActive) continue;
                
                // 检查玩家是否符合测试条件
                if (!IsPlayerEligible(test)) continue;
                
                // 分配组
                var group = AssignPlayerToGroup(playerId, test);
                playerAssignments[test.TestId] = group;
                
                // 应用配置
                ApplyTestConfiguration(test, group);
                
                // 记录分配
                LogTestAssignment(test, group);
                
                OnTestAssigned?.Invoke(test.TestId, group);
                
                Debug.Log($"[ABTest] 玩家 {playerId} 被分配到测试 {test.TestName} 的 {group} 组");
            }
        }
        
        /// <summary>
        /// 分配玩家到组
        /// </summary>
        private ABTestGroup AssignPlayerToGroup(string userId, ABTest test)
        {
            // 使用哈希确保同一用户始终在同一组
            int hash = (userId + test.TestId).GetHashCode();
            int groupValue = Mathf.Abs(hash) % 100;
            
            // 根据分配比例决定
            int cumulativePercentage = 0;
            foreach (var group in test.Groups)
            {
                cumulativePercentage += group.AllocationPercentage;
                if (groupValue < cumulativePercentage)
                {
                    return group.Group;
                }
            }
            
            return ABTestGroup.Control; // 默认对照组
        }
        
        /// <summary>
        /// 应用测试配置
        /// </summary>
        private void ApplyTestConfiguration(ABTest test, ABTestGroup group)
        {
            var config = group == ABTestGroup.Control ? test.ControlConfig : test.VariantConfig;
            appliedConfigs[test.TestId] = config;
            config.Apply();
        }
        
        /// <summary>
        /// 检查玩家是否符合测试条件
        /// </summary>
        private bool IsPlayerEligible(ABTest test)
        {
            // 检查新玩家/老玩家要求
            if (test.NewPlayersOnly && !IsNewPlayer()) return false;
            if (test.ExistingPlayersOnly && IsNewPlayer()) return false;
            
            // 检查平台
            // if (test.TargetPlatforms.Count > 0 && !test.TargetPlatforms.Contains(Application.platform))
            //     return false;
            
            // 检查地区
            // if (test.TargetRegions.Count > 0 && !test.TargetRegions.Contains(GetPlayerRegion()))
            //     return false;
            
            return true;
        }
        
        /// </summary>
        /// 获取测试配置
        /// </summary>
        public T GetConfig<T>(string testId) where T : ABTestConfig
        {
            if (appliedConfigs.TryGetValue(testId, out var config))
            {
                return config as T;
            }
            return null;
        }
        
        /// <summary>
        /// 获取玩家所在组
        /// </summary>
        public ABTestGroup GetPlayerGroup(string testId)
        {
            if (playerAssignments.TryGetValue(testId, out var group))
            {
                return group;
            }
            return ABTestGroup.NotAssigned;
        }
        
        /// </summary>
        /// 记录测试指标
        /// </summary>
        public void LogMetric(string testId, string metricName, object value)
        {
            if (!playerAssignments.ContainsKey(testId)) return;
            
            var group = playerAssignments[testId];
            
            // AnalyticsManager.Log("ab_test_metric", new Dictionary<string, object>
            // {
            //     { "test_id", testId },
            //     { "group", group.ToString() },
            //     { "metric", metricName },
            //     { "value", value },
            //     { "player_id", playerId }
            // });
        }
        
        /// <summary>
        /// 记录转化事件
        /// </summary>
        public void LogConversion(string testId, string conversionEvent)
        {
            LogMetric(testId, "conversion", conversionEvent);
        }
        
        /// <summary>
        /// 记录测试分配
        /// </summary>
        private void LogTestAssignment(ABTest test, ABTestGroup group)
        {
            // AnalyticsManager.Log("ab_test_assignment", new Dictionary<string, object>
            // {
            //     { "test_id", test.TestId },
            //     { "test_name", test.TestName },
            //     { "group", group.ToString() },
            //     { "player_id", playerId },
            //     { "timestamp", System.DateTime.UtcNow }
            // });
        }
        
        /// <summary>
        /// 加载或生成玩家ID
        /// </summary>
        private string LoadOrGeneratePlayerId()
        {
            string id = PlayerPrefs.GetString("ABTest_PlayerId", "");
            if (string.IsNullOrEmpty(id))
            {
                id = System.Guid.NewGuid().ToString();
                PlayerPrefs.SetString("ABTest_PlayerId", id);
                PlayerPrefs.Save();
            }
            return id;
        }
        
        /// <summary>
        /// 检查是否为新玩家
        /// </summary>
        private bool IsNewPlayer()
        {
            // 检查首次启动时间
            string firstLaunch = PlayerPrefs.GetString("FirstLaunchDate", "");
            if (string.IsNullOrEmpty(firstLaunch))
            {
                PlayerPrefs.SetString("FirstLaunchDate", System.DateTime.Now.ToString());
                return true;
            }
            
            // 如果首次启动在24小时内，视为新玩家
            if (System.DateTime.TryParse(firstLaunch, out var firstLaunchDate))
            {
                return (System.DateTime.Now - firstLaunchDate).TotalHours < 24;
            }
            
            return false;
        }
        
        /// <summary>
        /// 获取活跃测试列表
        /// </summary>
        public List<ABTest> GetActiveTests()
        {
            return activeTests.FindAll(t => t.IsActive);
        }
        
        /// <summary>
        /// 生成测试报告
        /// </summary>
        public ABTestReport GenerateReport(string testId)
        {
            var test = activeTests.Find(t => t.TestId == testId);
            if (test == null) return null;
            
            return new ABTestReport
            {
                TestId = testId,
                TestName = test.TestName,
                StartDate = test.StartDate,
                EndDate = test.EndDate,
                ControlGroupSize = 0, // 从分析系统获取
                VariantGroupSize = 0,
                Metrics = new Dictionary<string, ABTestMetric>()
            };
        }
    }
    
    /// <summary>
    /// A/B测试定义
    /// </summary>
    [System.Serializable]
    public class ABTest
    {
        public string TestId;
        public string TestName;
        public string Hypothesis;
        public bool IsActive = true;
        
        [Header("目标用户")]
        public bool NewPlayersOnly = false;
        public bool ExistingPlayersOnly = false;
        public List<RuntimePlatform> TargetPlatforms = new List<RuntimePlatform>();
        public List<string> TargetRegions = new List<string>();
        
        [Header("分组")]
        public List<ABTestGroupConfig> Groups = new List<ABTestGroupConfig>();
        
        [Header("配置")]
        public ABTestConfig ControlConfig;
        public ABTestConfig VariantConfig;
        
        [Header("指标")]
        public List<string> TargetMetrics = new List<string>();
        
        [Header("时间")]
        public System.DateTime StartDate;
        public System.DateTime EndDate;
        public int RequiredSampleSize = 1000;
    }
    
    /// </summary>
    /// A/B测试组配置
    /// </summary>
    [System.Serializable]
    public class ABTestGroupConfig
    {
        public ABTestGroup Group;
        [Range(0, 100)]
        public int AllocationPercentage;
    }
    
    /// </summary>
    /// A/B测试组
    /// </summary>
    public enum ABTestGroup
    {
        NotAssigned,
        Control,    // 对照组 (A)
        Variant     // 实验组 (B)
    }
    
    /// <summary>
    /// A/B测试配置基类
    /// </summary>
    public abstract class ABTestConfig : ScriptableObject
    {
        public abstract void Apply();
    }
    
    /// </summary>
    /// 教程长度测试配置
    /// </summary>
    [CreateAssetMenu(fileName = "TutorialLengthConfig", menuName = "SebeJJ/AB Test/Tutorial Length")]
    public class TutorialLengthConfig : ABTestConfig
    {
        public int TutorialStepCount = 8;
        public bool ShowSkipButton = true;
        public float AutoAdvanceDelay = 0f;
        
        public override void Apply()
        {
            // 应用到TutorialManager
            // TutorialManager.Instance.SetConfig(this);
        }
    }
    
    /// </summary>
    /// 死亡惩罚测试配置
    /// </summary>
    [CreateAssetMenu(fileName = "DeathPenaltyConfig", menuName = "SebeJJ/AB Test/Death Penalty")]
    public class DeathPenaltyConfig : ABTestConfig
    {
        [Range(0, 100)]
        public float ResourceLossPercent = 50f;
        [Range(0, 100)]
        public float CreditLossPercent = 10f;
        public bool EnableInsurance = true;
        
        public override void Apply()
        {
            // 应用到DeathPenaltySystem
            // DeathPenaltySystem.Instance.SetConfig(this);
        }
    }
    
    /// </summary>
    /// 奖励频率测试配置
    /// </summary>
    [CreateAssetMenu(fileName = "RewardFrequencyConfig", menuName = "SebeJJ/AB Test/Reward Frequency")]
    public class RewardFrequencyConfig : ABTestConfig
    {
        public bool EnableMilestoneRewards = true;
        public float MilestoneInterval = 600f; // 10分钟
        public bool EnableComboSystem = true;
        
        public override void Apply()
        {
            // 应用到RewardTimingSystem
            // RewardTimingSystem.Instance.SetConfig(this);
        }
    }
    
    /// </summary>
    /// A/B测试报告
    /// </summary>
    public class ABTestReport
    {
        public string TestId;
        public string TestName;
        public System.DateTime StartDate;
        public System.DateTime EndDate;
        public int ControlGroupSize;
        public int VariantGroupSize;
        public Dictionary<string, ABTestMetric> Metrics;
    }
    
    /// </summary>
    /// A/B测试指标
    /// </summary>
    public class ABTestMetric
    {
        public string MetricName;
        public float ControlValue;
        public float VariantValue;
        public float Difference;
        public float PercentageChange;
        public float PValue;
        public bool IsSignificant;
    }
}
