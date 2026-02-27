using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SebeJJ.Systems
{
    /// <summary>
    /// 委托管理器 - 管理所有委托的生成、接取、追踪和完成
    /// </summary>
    public class MissionManager : MonoBehaviour
    {
        public static MissionManager Instance { get; private set; }
        
        [Header("委托设置")]
        public int maxActiveMissions = 3;
        public int availableMissionCount = 5;
        public float missionRefreshInterval = 300f; // 5分钟刷新
        
        [Header("委托数据")]
        public List<MissionData> missionDatabase = new List<MissionData>();
        
        // 委托列表
        public List<Mission> AvailableMissions { get; private set; } = new List<Mission>();
        public List<Mission> ActiveMissions { get; private set; } = new List<Mission>();
        public List<Mission> CompletedMissions { get; private set; } = new List<Mission>();
        
        // 事件
        public event Action<Mission> OnMissionAccepted;
        public event Action<Mission> OnMissionCompleted;
        public event Action<Mission> OnMissionFailed;
        public event Action<Mission> OnMissionUpdated;
        public event Action OnAvailableMissionsRefreshed;
        
        private float lastRefreshTime;
        private float lastMissionCheckTime; // BUG-003 修复: 添加检查时间戳
        private HashSet<string> completedMissionIds = new HashSet<string>(); // BUG-003 修复: 防止重复完成
        
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
            GenerateAvailableMissions();
        }
        
        private void Update()
        {
            // 自动刷新可用委托
            if (Time.time >= lastRefreshTime + missionRefreshInterval)
            {
                RefreshAvailableMissions();
            }
            
            // 更新活跃委托
            UpdateActiveMissions();
        }
        
        /// <summary>
        /// 初始化新游戏
        /// </summary>
        public void InitializeNewGame()
        {
            AvailableMissions.Clear();
            ActiveMissions.Clear();
            CompletedMissions.Clear();
            GenerateAvailableMissions();
        }
        
        /// <summary>
        /// SO-002修复: 生成可用委托，添加空值过滤
        /// </summary>
        private void GenerateAvailableMissions()
        {
            AvailableMissions.Clear();
            
            // SO-002修复: 过滤空引用
            var validMissions = missionDatabase.FindAll(m => m != null);
            if (validMissions.Count == 0)
            {
                Debug.LogWarning("[MissionManager] 没有有效的委托数据!");
                return;
            }
            
            // 从数据库中随机选择委托
            var shuffled = validMissions.OrderBy(x => UnityEngine.Random.value).ToList();
            
            for (int i = 0; i < Mathf.Min(availableMissionCount, shuffled.Count); i++)
            {
                var mission = new Mission(shuffled[i]);
                AvailableMissions.Add(mission);
            }
            
            lastRefreshTime = Time.time;
            OnAvailableMissionsRefreshed?.Invoke();
            
            Debug.Log($"[MissionManager] 生成 {AvailableMissions.Count} 个可用委托");
        }
        
        /// <summary>
        /// 刷新可用委托
        /// </summary>
        public void RefreshAvailableMissions()
        {
            GenerateAvailableMissions();
            Debug.Log("[MissionManager] 委托列表已刷新");
        }
        
        /// <summary>
        /// 接取委托
        /// </summary>
        public bool AcceptMission(string missionId)
        {
            if (ActiveMissions.Count >= maxActiveMissions)
            {
                Debug.LogWarning("[MissionManager] 活跃委托数量已达上限");
                return false;
            }
            
            var mission = AvailableMissions.Find(m => m.MissionId == missionId);
            if (mission == null) return false;
            
            mission.Status = MissionStatus.Active;
            // MS-001修复: 使用游戏内时间
            mission.AcceptTimeGameTime = Time.time;
            
            AvailableMissions.Remove(mission);
            ActiveMissions.Add(mission);
            
            OnMissionAccepted?.Invoke(mission);
            Debug.Log($"[MissionManager] 接取委托: {mission.Title}");
            
            return true;
        }
        
        /// <summary>
        /// 更新活跃委托 (BUG-003 修复: 优化更新频率，添加状态锁)
        /// </summary>
        private void UpdateActiveMissions()
        {
            // 每0.5秒检查一次，避免每帧检查
            if (Time.time < lastMissionCheckTime + 0.5f) return;
            lastMissionCheckTime = Time.time;
            
            foreach (var mission in ActiveMissions.ToList())
            {
                // 跳过已完成或失败的委托
                if (mission.Status != MissionStatus.Active) continue;
                
                if (mission.IsExpired())
                {
                    FailMission(mission);
                    continue;
                }
                
                // 检查完成条件
                if (CheckMissionCompletion(mission))
                {
                    CompleteMission(mission);
                }
            }
        }
        
        /// <summary>
        /// 检查委托是否完成
        /// </summary>
        private bool CheckMissionCompletion(Mission mission)
        {
            switch (mission.Type)
            {
                case MissionType.Collection:
                    return CheckCollectionObjective(mission);
                case MissionType.Exploration:
                    return CheckExplorationObjective(mission);
                case MissionType.Scan:
                    return CheckScanObjective(mission);
                default:
                    return false;
            }
        }
        
        private bool CheckCollectionObjective(Mission mission)
        {
            // 检查背包中是否有足够的指定资源
            var inventory = Core.GameManager.Instance?.resourceManager?.Inventory;
            if (inventory == null) return false;
            
            foreach (var objective in mission.Objectives)
            {
                int currentAmount = inventory.GetItemCount(objective.targetId);
                if (currentAmount < objective.requiredAmount)
                    return false;
            }
            return true;
        }
        
        private bool CheckExplorationObjective(Mission mission)
        {
            var diveManager = Core.GameManager.Instance?.diveManager;
            if (diveManager == null) return false;
            
            foreach (var objective in mission.Objectives)
            {
                if (objective.targetId == "depth" && diveManager.CurrentDepth < objective.requiredAmount)
                    return false;
            }
            return true;
        }
        
        private bool CheckScanObjective(Mission mission)
        {
            // 扫描目标逻辑
            return mission.CurrentProgress >= mission.TargetProgress;
        }
        
        /// <summary>
        /// 完成委托 (QF-002修复: 添加幂等性检查, MS-003修复: 添加物品奖励发放)
        /// </summary>
        private void CompleteMission(Mission mission)
        {
            // QF-002修复: 幂等性检查，防止重复完成
            if (mission.Status == MissionStatus.Completed) return;
            if (completedMissionIds.Contains(mission.MissionId)) return;
            
            mission.Status = MissionStatus.Completed;
            
            completedMissionIds.Add(mission.MissionId); // 记录已完成ID
            ActiveMissions.Remove(mission);
            CompletedMissions.Add(mission);
            
            // 发放信用点奖励
            Core.GameManager.Instance?.resourceManager?.AddCredits(mission.RewardCredits);
            
            // MS-003修复: 发放物品奖励
            if (mission.RewardItems != null && mission.RewardItems.Count > 0)
            {
                foreach (var rewardItem in mission.RewardItems)
                {
                    if (rewardItem != null && !string.IsNullOrEmpty(rewardItem.itemId))
                    {
                        // 创建库存物品并添加
                        var item = new Data.InventoryItem
                        {
                            itemId = rewardItem.itemId,
                            itemName = rewardItem.itemName,
                            quantity = rewardItem.quantity
                        };
                        Core.GameManager.Instance?.resourceManager?.AddToInventory(item);
                        Debug.Log($"[MissionManager] 发放奖励物品: {rewardItem.itemName} x{rewardItem.quantity}");
                    }
                }
            }
            
            OnMissionCompleted?.Invoke(mission);
            Core.UIManager.Instance?.ShowMissionComplete(mission.Title, mission.RewardCredits);
            
            Debug.Log($"[MissionManager] 委托完成: {mission.Title}");
        }
        
        /// <summary>
        /// 委托失败
        /// </summary>
        private void FailMission(Mission mission)
        {
            mission.Status = MissionStatus.Failed;
            
            ActiveMissions.Remove(mission);
            
            OnMissionFailed?.Invoke(mission);
            Debug.Log($"[MissionManager] 委托失败: {mission.Title}");
        }
        
        /// <summary>
        /// 更新委托进度
        /// </summary>
        public void UpdateMissionProgress(string targetId, int amount = 1)
        {
            foreach (var mission in ActiveMissions)
            {
                var objective = mission.Objectives.Find(o => o.targetId == targetId);
                if (objective != null)
                {
                    objective.currentAmount = Mathf.Min(objective.currentAmount + amount, objective.requiredAmount);
                    mission.CurrentProgress = CalculateTotalProgress(mission);
                    OnMissionUpdated?.Invoke(mission);
                }
            }
        }
        
        private int CalculateTotalProgress(Mission mission)
        {
            if (mission.Objectives.Count == 0) return 0;
            
            int totalProgress = 0;
            foreach (var objective in mission.Objectives)
            {
                totalProgress += objective.currentAmount;
            }
            return totalProgress;
        }
        
        /// <summary>
        /// 获取活跃委托数量
        /// </summary>
        public int GetActiveMissionCount()
        {
            return ActiveMissions.Count;
        }
        
        /// <summary>
        /// 获取已完成委托数量
        /// </summary>
        public int GetCompletedMissionCount()
        {
            return CompletedMissions.Count;
        }
        
        /// <summary>
        /// 是否有指定类型的活跃委托
        /// </summary>
        public bool HasActiveMissionOfType(MissionType type)
        {
            return ActiveMissions.Exists(m => m.Type == type);
        }
        
        /// <summary>
        /// 获取指定ID的委托
        /// </summary>
        public Mission GetMission(string missionId)
        {
            var mission = ActiveMissions.Find(m => m.MissionId == missionId);
            if (mission != null) return mission;
            
            mission = AvailableMissions.Find(m => m.MissionId == missionId);
            if (mission != null) return mission;
            
            return CompletedMissions.Find(m => m.MissionId == missionId);
        }
    }
    
    /// <summary>
    /// 委托状态
    /// </summary>
    public enum MissionStatus
    {
        Available,  // 可用
        Active,     // 进行中
        Completed,  // 已完成
        Failed      // 失败
    }
    
    /// <summary>
    /// 委托类型
    /// </summary>
    public enum MissionType
    {
        Collection, // 采集
        Exploration,// 探索
        Scan,       // 扫描
        Delivery,   // 运送
        Elimination // 清除
    }
}
