using System;
using UnityEngine;

namespace SebeJJ.Experience.Pacing
{
    /// <summary>
    /// 节奏管理器 - 控制整体游戏节奏
    /// </summary>
    public class PacingManager : MonoBehaviour
    {
        public static PacingManager Instance { get; private set; }
        
        [Header("节奏目标")]
        [SerializeField] private float targetSessionDuration = 1200f; // 20分钟
        [SerializeField] private float targetCombatRatio = 0.4f;      // 40%战斗
        [SerializeField] private float targetExplorationRatio = 0.5f; // 50%探索
        [SerializeField] private float targetRestRatio = 0.1f;        // 10%休整
        
        [Header("调整参数")]
        [SerializeField] private float adjustmentThreshold = 0.1f;
        [SerializeField] private float adjustmentRate = 0.05f;
        
        private SessionPaceData currentSession;
        private bool isSessionActive = false;
        
        public SessionPaceData CurrentSession => currentSession;
        public bool IsSessionActive => isSessionActive;
        
        public static event Action<SessionPaceData> OnSessionStarted;
        public static event Action<SessionPaceData> OnSessionEnded;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Update()
        {
            if (isSessionActive)
            {
                UpdateSessionMetrics();
            }
        }
        
        /// <summary>
        /// 开始新会话
        /// </summary>
        public void StartNewSession()
        {
            currentSession = new SessionPaceData
            {
                StartTime = DateTime.Now,
                StartDepth = GetCurrentDepth()
            };
            
            isSessionActive = true;
            OnSessionStarted?.Invoke(currentSession);
            
            Debug.Log("[PacingManager] 新会话开始");
        }
        
        /// <summary>
        /// 结束当前会话
        /// </summary>
        public void EndSession()
        {
            if (!isSessionActive) return;
            
            isSessionActive = false;
            currentSession.EndTime = DateTime.Now;
            currentSession.EndDepth = GetCurrentDepth();
            
            // 计算统计数据
            CalculateFinalStats();
            
            // 记录分析
            LogSessionData();
            
            OnSessionEnded?.Invoke(currentSession);
            
            Debug.Log($"[PacingManager] 会话结束 - 时长: {currentSession.TotalTime:F0}秒, 节奏分数: {currentSession.PaceScore:F2}");
        }
        
        /// <summary>
        /// 更新会话指标
        /// </summary>
        private void UpdateSessionMetrics()
        {
            float deltaTime = Time.deltaTime;
            
            // 检测当前状态
            if (IsInCombat())
            {
                currentSession.CombatTime += deltaTime;
            }
            else if (IsExploring())
            {
                currentSession.ExplorationTime += deltaTime;
            }
            else
            {
                currentSession.RestTime += deltaTime;
            }
            
            // 定期调整节奏
            if (currentSession.GetTotalTime() % 60 < deltaTime) // 每分钟检查一次
            {
                AdjustPacingIfNeeded();
            }
        }
        
        /// <summary>
        /// 记录战斗开始
        /// </summary>
        public void RecordCombatStart()
        {
            currentSession?.CombatCount++;
        }
        
        /// <summary>
        /// 记录敌人击败
        /// </summary>
        public void RecordEnemyDefeated(string enemyId)
        {
            if (currentSession != null)
            {
                currentSession.EnemiesDefeated++;
            }
        }
        
        /// <summary>
        /// 记录资源采集
        /// </summary>
        public void RecordResourceCollected(string resourceId, int amount)
        {
            if (currentSession != null)
            {
                currentSession.ResourcesCollected += amount;
            }
        }
        
        /// <summary>
        /// 记录委托完成
        /// </summary>
        public void RecordMissionComplete(string missionId)
        {
            if (currentSession != null)
            {
                currentSession.MissionsCompleted++;
            }
        }
        
        /// <summary>
        /// 调整节奏
        /// </summary>
        private void AdjustPacingIfNeeded()
        {
            float totalTime = currentSession.GetTotalTime();
            if (totalTime < 60f) return; // 至少1分钟后调整
            
            float actualCombatRatio = currentSession.CombatTime / totalTime;
            float actualExplorationRatio = currentSession.ExplorationTime / totalTime;
            
            // 战斗太多，增加探索
            if (actualCombatRatio > targetCombatRatio + adjustmentThreshold)
            {
                ReduceCombatFrequency();
            }
            // 探索太多，增加战斗
            else if (actualExplorationRatio > targetExplorationRatio + adjustmentThreshold)
            {
                IncreaseCombatFrequency();
            }
        }
        
        private void ReduceCombatFrequency()
        {
            // 通知战斗节奏控制器
            // CombatPacingController.Instance.ReduceEncounterRate(adjustmentRate);
            Debug.Log("[PacingManager] 减少战斗频率");
        }
        
        private void IncreaseCombatFrequency()
        {
            // 通知战斗节奏控制器
            // CombatPacingController.Instance.IncreaseEncounterRate(adjustmentRate);
            Debug.Log("[PacingManager] 增加战斗频率");
        }
        
        /// <summary>
        /// 计算最终统计
        /// </summary>
        private void CalculateFinalStats()
        {
            float totalTime = currentSession.GetTotalTime();
            if (totalTime <= 0) return;
            
            currentSession.CombatRatio = currentSession.CombatTime / totalTime;
            currentSession.ExplorationRatio = currentSession.ExplorationTime / totalTime;
            currentSession.RestRatio = currentSession.RestTime / totalTime;
            
            // 计算节奏分数
            currentSession.PaceScore = CalculatePaceScore();
        }
        
        /// <summary>
        /// 计算节奏分数
        /// </summary>
        private float CalculatePaceScore()
        {
            // 理想比例: 40%战斗, 50%探索, 10%休整
            float combatScore = 1f - Mathf.Abs(currentSession.CombatRatio - targetCombatRatio) / targetCombatRatio;
            float explorationScore = 1f - Mathf.Abs(currentSession.ExplorationRatio - targetExplorationRatio) / targetExplorationRatio;
            
            return Mathf.Clamp01((combatScore + explorationScore) / 2f);
        }
        
        /// <summary>
        /// 记录会话数据
        /// </summary>
        private void LogSessionData()
        {
            // AnalyticsManager.Log("session_end", new Dictionary<string, object>
            // {
            //     { "duration", currentSession.TotalTime },
            //     { "combat_ratio", currentSession.CombatRatio },
            //     { "exploration_ratio", currentSession.ExplorationRatio },
            //     { "pace_score", currentSession.PaceScore },
            //     { "enemies_defeated", currentSession.EnemiesDefeated },
            //     { "resources_collected", currentSession.ResourcesCollected },
            //     { "missions_completed", currentSession.MissionsCompleted }
            // });
        }
        
        /// <summary>
        /// 检查是否在战斗中
        /// </summary>
        private bool IsInCombat()
        {
            // return CombatManager.Instance.IsInCombat;
            return false;
        }
        
        /// <summary>
        /// 检查是否在探索
        /// </summary>
        private bool IsExploring()
        {
            // 玩家移动且不在战斗
            // return PlayerController.Instance.IsMoving && !IsInCombat();
            return false;
        }
        
        /// <summary>
        /// 获取当前深度
        /// </summary>
        private float GetCurrentDepth()
        {
            // return DiveManager.Instance.CurrentDepth;
            return 0f;
        }
    }
    
    /// <summary>
    /// 会话节奏数据
    /// </summary>
    [System.Serializable]
    public class SessionPaceData
    {
        public DateTime StartTime;
        public DateTime EndTime;
        public float StartDepth;
        public float EndDepth;
        
        public float CombatTime;
        public float ExplorationTime;
        public float RestTime;
        
        public float CombatRatio;
        public float ExplorationRatio;
        public float RestRatio;
        
        public int CombatCount;
        public int EnemiesDefeated;
        public int ResourcesCollected;
        public int MissionsCompleted;
        
        public float PaceScore;
        
        public float TotalTime
        {
            get
            {
                if (EndTime > StartTime)
                    return (float)(EndTime - StartTime).TotalSeconds;
                return (float)(DateTime.Now - StartTime).TotalSeconds;
            }
        }
        
        public float GetTotalTime()
        {
            return CombatTime + ExplorationTime + RestTime;
        }
    }
}
