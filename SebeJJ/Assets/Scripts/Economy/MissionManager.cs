using System;
using System.Collections.Generic;
using UnityEngine;
using SebeJJ.Utils;
using SebeJJ.Core;

namespace SebeJJ.Economy
{
    /// <summary>
    /// 任务数据类
    /// </summary>
    [Serializable]
    public class MissionData
    {
        public string missionId;
        public string title;
        public string description;
        public MissionType missionType;
        public MissionStatus status;
        
        // 目标
        public ResourceType targetResource;
        public int targetAmount;
        public string targetLocation;
        public string targetEnemy;
        
        // 奖励
        public int currencyReward;
        public int experienceReward;
        public List<string> itemRewards;
        
        // 限制
        public float timeLimit;  // 0 = 无限制
        public int maxDepth;
        
        // 进度
        public int currentAmount;
        public float elapsedTime;
        
        // 元数据
        public string giverNpcId;
        public string unlockRequirement;
        public int requiredPlayerLevel;
        public bool isRepeatable;
    }

    /// <summary>
    /// 任务管理器 - 管理所有任务相关的逻辑
    /// </summary>
    public class MissionManager : Singleton<MissionManager>
    {
        [Header("Settings")]
        [SerializeField] private int maxActiveMissions = 3;
        [SerializeField] private bool autoTrackNewMissions = true;

        [Header("Events")]
        [SerializeField] private bool showDebugLogs = true;

        // 任务列表
        private List<MissionData> _availableMissions = new List<MissionData>();
        private List<MissionData> _activeMissions = new List<MissionData>();
        private List<MissionData> _completedMissions = new List<MissionData>();
        private List<MissionData> _failedMissions = new List<MissionData>();

        // 当前追踪的任务
        private MissionData _trackedMission;

        // 属性
        public IReadOnlyList<MissionData> AvailableMissions => _availableMissions;
        public IReadOnlyList<MissionData> ActiveMissions => _activeMissions;
        public IReadOnlyList<MissionData> CompletedMissions => _completedMissions;
        public MissionData TrackedMission => _trackedMission;
        public bool HasAvailableMission => _availableMissions.Count > 0;
        public bool HasActiveMission => _activeMissions.Count > 0;
        public int ActiveMissionCount => _activeMissions.Count;

        #region Unity Lifecycle

        protected override void OnAwake()
        {
            base.OnAwake();
        }

        private void Start()
        {
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void Update()
        {
            UpdateActiveMissions();
        }

        #endregion

        #region Event Subscription

        private void SubscribeToEvents()
        {
            GameEvents.OnResourceCollected += OnResourceCollected;
        }

        private void UnsubscribeFromEvents()
        {
            GameEvents.OnResourceCollected -= OnResourceCollected;
        }

        #endregion

        #region Mission Management

        /// <summary>
        /// 添加可用任务
        /// </summary>
        public void AddAvailableMission(MissionData mission)
        {
            if (mission == null || _availableMissions.Exists(m => m.missionId == mission.missionId))
                return;

            _availableMissions.Add(mission);
            Log($"Added available mission: {mission.title}");
        }

        /// <summary>
        /// 接受任务
        /// </summary>
        public bool AcceptMission(string missionId)
        {
            if (_activeMissions.Count >= maxActiveMissions)
            {
                LogWarning("Cannot accept mission: maximum active missions reached.");
                return false;
            }

            var mission = _availableMissions.Find(m => m.missionId == missionId);
            if (mission == null)
            {
                LogWarning($"Mission {missionId} not found in available missions.");
                return false;
            }

            mission.status = MissionStatus.InProgress;
            mission.currentAmount = 0;
            mission.elapsedTime = 0;

            _availableMissions.Remove(mission);
            _activeMissions.Add(mission);

            if (autoTrackNewMissions || _trackedMission == null)
            {
                TrackMission(missionId);
            }

            GameEvents.OnMissionStarted?.Invoke(missionId);
            Log($"Mission accepted: {mission.title}");

            return true;
        }

        /// <summary>
        /// 放弃任务
        /// </summary>
        public bool AbandonMission(string missionId)
        {
            var mission = _activeMissions.Find(m => m.missionId == missionId);
            if (mission == null) return false;

            mission.status = MissionStatus.NotStarted;
            _activeMissions.Remove(mission);
            _availableMissions.Add(mission);

            if (_trackedMission == mission)
            {
                _trackedMission = _activeMissions.Count > 0 ? _activeMissions[0] : null;
            }

            Log($"Mission abandoned: {mission.title}");
            return true;
        }

        /// <summary>
        /// 完成任务
        /// </summary>
        public bool CompleteMission(string missionId)
        {
            var mission = _activeMissions.Find(m => m.missionId == missionId);
            if (mission == null) return false;

            mission.status = MissionStatus.Completed;
            _activeMissions.Remove(mission);
            _completedMissions.Add(mission);

            // 发放奖励
            GiveRewards(mission);

            // 更新存档
            SaveManager.Instance.CurrentData?.totalMissionsCompleted++;

            if (_trackedMission == mission)
            {
                _trackedMission = _activeMissions.Count > 0 ? _activeMissions[0] : null;
            }

            GameEvents.OnMissionCompleted?.Invoke(missionId);
            Log($"Mission completed: {mission.title}");

            return true;
        }

        /// <summary>
        /// 任务失败
        /// </summary>
        public bool FailMission(string missionId)
        {
            var mission = _activeMissions.Find(m => m.missionId == missionId);
            if (mission == null) return false;

            mission.status = MissionStatus.Failed;
            _activeMissions.Remove(mission);
            _failedMissions.Add(mission);

            if (mission.isRepeatable)
            {
                mission.status = MissionStatus.NotStarted;
                _availableMissions.Add(mission);
            }

            if (_trackedMission == mission)
            {
                _trackedMission = _activeMissions.Count > 0 ? _activeMissions[0] : null;
            }

            GameEvents.OnMissionFailed?.Invoke(missionId);
            Log($"Mission failed: {mission.title}");

            return true;
        }

        /// <summary>
        /// 追踪任务
        /// </summary>
        public void TrackMission(string missionId)
        {
            var mission = _activeMissions.Find(m => m.missionId == missionId);
            if (mission != null)
            {
                _trackedMission = mission;
                Log($"Now tracking mission: {mission.title}");
            }
        }

        /// <summary>
        /// 获取任务
        /// </summary>
        public MissionData GetMission(string missionId)
        {
            var mission = _activeMissions.Find(m => m.missionId == missionId);
            if (mission != null) return mission;

            mission = _availableMissions.Find(m => m.missionId == missionId);
            if (mission != null) return mission;

            return _completedMissions.Find(m => m.missionId == missionId);
        }

        #endregion

        #region Progress Update

        private void UpdateActiveMissions()
        {
            for (int i = _activeMissions.Count - 1; i >= 0; i--)
            {
                var mission = _activeMissions[i];
                
                // 更新时间
                mission.elapsedTime += Time.deltaTime;
                
                // 检查时间限制
                if (mission.timeLimit > 0 && mission.elapsedTime >= mission.timeLimit)
                {
                    FailMission(mission.missionId);
                    continue;
                }

                // 更新进度事件
                float progress = GetMissionProgress(mission);
                GameEvents.OnMissionProgress?.Invoke(mission.missionId, progress);
            }
        }

        private void OnResourceCollected(ResourceType type, int amount)
        {
            foreach (var mission in _activeMissions)
            {
                if (mission.missionType == MissionType.Collection && 
                    mission.targetResource == type)
                {
                    UpdateMissionProgress(mission.missionId, amount);
                }
            }
        }

        /// <summary>
        /// 更新任务进度
        /// </summary>
        public void UpdateMissionProgress(string missionId, int amount)
        {
            var mission = _activeMissions.Find(m => m.missionId == missionId);
            if (mission == null) return;

            mission.currentAmount += amount;
            
            // 检查完成条件
            if (mission.currentAmount >= mission.targetAmount)
            {
                CompleteMission(missionId);
            }
            else
            {
                float progress = GetMissionProgress(mission);
                GameEvents.OnMissionProgress?.Invoke(missionId, progress);
            }
        }

        /// <summary>
        /// 获取任务进度 (0-1)
        /// </summary>
        public float GetMissionProgress(MissionData mission)
        {
            if (mission == null || mission.targetAmount <= 0) return 0f;
            return Mathf.Clamp01((float)mission.currentAmount / mission.targetAmount);
        }

        /// <summary>
        /// 获取追踪任务的进度
        /// </summary>
        public float GetTrackedMissionProgress()
        {
            return GetMissionProgress(_trackedMission);
        }

        #endregion

        #region Rewards

        private void GiveRewards(MissionData mission)
        {
            if (mission == null) return;

            // 货币奖励
            if (mission.currencyReward > 0)
            {
                SaveManager.Instance?.AddCurrency(mission.currencyReward);
            }

            // 经验奖励
            if (mission.experienceReward > 0)
            {
                // TODO: 实现经验系统
            }

            // 物品奖励
            if (mission.itemRewards != null)
            {
                foreach (var itemId in mission.itemRewards)
                {
                    // TODO: 添加到背包
                    Log($"Reward item: {itemId}");
                }
            }

            GameEvents.OnShowNotification?.Invoke($"任务完成！获得 {mission.currencyReward} 信用点", 3f);
        }

        #endregion

        #region Save/Load

        /// <summary>
        /// 获取所有任务数据用于存档
        /// </summary>
        public List<MissionData> GetAllMissionData()
        {
            var allMissions = new List<MissionData>();
            allMissions.AddRange(_availableMissions);
            allMissions.AddRange(_activeMissions);
            allMissions.AddRange(_completedMissions);
            return allMissions;
        }

        /// <summary>
        /// 从存档加载任务数据
        /// </summary>
        public void LoadMissionData(List<MissionData> missions)
        {
            _availableMissions.Clear();
            _activeMissions.Clear();
            _completedMissions.Clear();

            foreach (var mission in missions)
            {
                switch (mission.status)
                {
                    case MissionStatus.NotStarted:
                        _availableMissions.Add(mission);
                        break;
                    case MissionStatus.InProgress:
                        _activeMissions.Add(mission);
                        break;
                    case MissionStatus.Completed:
                        _completedMissions.Add(mission);
                        break;
                    case MissionStatus.Failed:
                        _failedMissions.Add(mission);
                        break;
                }
            }
        }

        #endregion

        #region Debug

        private void Log(string message)
        {
            if (showDebugLogs)
                Debug.Log($"[MissionManager] {message}");
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[MissionManager] {message}");
        }

        #endregion
    }
}