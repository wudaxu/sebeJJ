using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SebeJJ.Systems
{
    /// <summary>
    /// 委托连锁系统 - BUG-021修复
    /// 管理任务链和前置条件
    /// </summary>
    public class MissionChainSystem : MonoBehaviour
    {
        public static MissionChainSystem Instance { get; private set; }
        
        [Header("连锁配置")]
        [SerializeField] private List<MissionChain> missionChains = new List<MissionChain>();
        
        [Header("解锁设置")]
        [SerializeField] private bool autoUnlockChainedMissions = true;
        [SerializeField] private float chainRewardMultiplier = 1.2f;
        
        // 追踪数据
        private Dictionary<string, List<string> missionPrerequisites = new Dictionary<string, List<string>();
        private Dictionary<string, List<string> missionUnlocks = new Dictionary<string, List<string>();
        private HashSet<string> completedChainMissions = new HashSet<string>();
        private Dictionary<string, int> chainProgress = new Dictionary<string, int>();
        
        // 事件
        public event Action<Mission> OnChainedMissionUnlocked;
        public event Action<string> OnChainCompleted;
        public event Action<Mission, float> OnChainRewardGranted;
        
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
            InitializeMissionChains();
            SubscribeToMissionEvents();
        }
        
        /// <summary>
        /// 初始化委托连锁关系
        /// </summary>
        private void InitializeMissionChains()
        {
            foreach (var chain in missionChains)
            {
                if (chain == null || chain.missionIds == null) continue;
                
                for (int i = 0; i < chain.missionIds.Count; i++)
                {
                    string missionId = chain.missionIds[i];
                    
                    // 设置前置条件
                    if (i > 0)
                    {
                        string prerequisite = chain.missionIds[i - 1];
                        if (!missionPrerequisites.ContainsKey(missionId))
                        {
                            missionPrerequisites[missionId] = new List<string>();
                        }
                        missionPrerequisites[missionId].Add(prerequisite);
                    }
                    
                    // 设置解锁关系
                    if (i < chain.missionIds.Count - 1)
                    {
                        string unlocks = chain.missionIds[i + 1];
                        if (!missionUnlocks.ContainsKey(missionId))
                        {
                            missionUnlocks[missionId] = new List<string>();
                        }
                        missionUnlocks[missionId].Add(unlocks);
                    }
                }
                
                // 初始化连锁进度
                chainProgress[chain.chainId] = 0;
            }
            
            Debug.Log($"[MissionChainSystem] 初始化了 {missionChains.Count} 个任务链");
        }
        
        /// <summary>
        /// 订阅委托事件
        /// </summary>
        private void SubscribeToMissionEvents()
        {
            var missionManager = MissionManager.Instance;
            if (missionManager != null)
            {
                missionManager.OnMissionCompleted += OnMissionCompleted;
            }
        }
        
        /// <summary>
        /// 检查是否可以开始某个委托
        /// </summary>
        public bool CanStartMission(string missionId)
        {
            // 检查前置条件
            if (missionPrerequisites.TryGetValue(missionId, out var prerequisites))
            {
                foreach (var prereq in prerequisites)
                {
                    if (!completedChainMissions.Contains(prereq))
                    {
                        return false;
                    }
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 获取委托的前置条件
        /// </summary>
        public List<string> GetPrerequisites(string missionId)
        {
            if (missionPrerequisites.TryGetValue(missionId, out var prerequisites))
            {
                return new List<string>(prerequisites);
            }
            return new List<string>();
        }
        
        /// <summary>
        /// 获取委托解锁的后续委托
        /// </summary>
        public List<string> GetUnlockedMissions(string missionId)
        {
            if (missionUnlocks.TryGetValue(missionId, out var unlocks))
            {
                return new List<string>(unlocks);
            }
            return new List<string>();
        }
        
        /// <summary>
        /// 委托完成回调
        /// </summary>
        private void OnMissionCompleted(Mission mission)
        {
            if (mission == null) return;
            
            completedChainMissions.Add(mission.MissionId);
            
            // 解锁后续委托
            if (autoUnlockChainedMissions)
            {
                UnlockChainedMissions(mission.MissionId);
            }
            
            // 检查连锁完成
            CheckChainCompletion(mission.MissionId);
            
            // 发放连锁奖励
            GrantChainRewards(mission);
        }
        
        /// <summary>
        /// 解锁连锁委托
        /// </summary>
        private void UnlockChainedMissions(string completedMissionId)
        {
            if (missionUnlocks.TryGetValue(completedMissionId, out var unlocks))
            {
                foreach (var missionId in unlocks)
                {
                    if (CanStartMission(missionId))
                    {
                        var mission = MissionManager.Instance?.GetMission(missionId);
                        if (mission != null)
                        {
                            mission.Status = MissionStatus.Available;
                            OnChainedMissionUnlocked?.Invoke(mission);
                            Debug.Log($"[MissionChainSystem] 解锁连锁委托: {missionId}");
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 检查连锁完成
        /// </summary>
        private void CheckChainCompletion(string missionId)
        {
            foreach (var chain in missionChains)
            {
                if (chain.missionIds.Contains(missionId))
                {
                    // 更新连锁进度
                    if (!chainProgress.ContainsKey(chain.chainId))
                    {
                        chainProgress[chain.chainId] = 0;
                    }
                    chainProgress[chain.chainId]++;
                    
                    // 检查是否完成整个连锁
                    if (chainProgress[chain.chainId] >= chain.missionIds.Count)
                    {
                        OnChainCompleted?.Invoke(chain.chainId);
                        Debug.Log($"[MissionChainSystem] 任务链完成: {chain.chainName}");
                    }
                    
                    break;
                }
            }
        }
        
        /// <summary>
        /// 发放连锁奖励
        /// </summary>
        private void GrantChainRewards(Mission mission)
        {
            // 检查是否是连锁中的委托
            foreach (var chain in missionChains)
            {
                if (chain.missionIds.Contains(mission.MissionId))
                {
                    int missionIndex = chain.missionIds.IndexOf(mission.MissionId);
                    
                    // 计算连锁奖励倍率
                    float multiplier = 1f + (missionIndex * 0.1f); // 每完成一个，奖励增加10%
                    float bonusReward = mission.RewardCredits * (multiplier - 1f);
                    
                    if (bonusReward > 0)
                    {
                        GameManager.Instance?.resourceManager?.AddCredits(Mathf.RoundToInt(bonusReward));
                        OnChainRewardGranted?.Invoke(mission, bonusReward);
                        Debug.Log($"[MissionChainSystem] 发放连锁奖励: {bonusReward} 信用点");
                    }
                    
                    break;
                }
            }
        }
        
        /// <summary>
        /// 获取连锁进度
        /// </summary>
        public float GetChainProgress(string chainId)
        {
            var chain = missionChains.Find(c => c.chainId == chainId);
            if (chain == null) return 0f;
            
            if (!chainProgress.ContainsKey(chainId))
            {
                chainProgress[chainId] = 0;
            }
            
            return (float)chainProgress[chainId] / chain.missionIds.Count;
        }
        
        /// <summary>
        /// 获取所有可用连锁
        /// </summary>
        public List<MissionChain> GetAvailableChains()
        {
            return missionChains.Where(c => GetChainProgress(c.chainId) < 1f).ToList();
        }
        
        /// <summary>
        /// 重置连锁进度
        /// </summary>
        public void ResetChainProgress(string chainId)
        {
            if (chainProgress.ContainsKey(chainId))
            {
                chainProgress[chainId] = 0;
            }
        }
        
        /// <summary>
        /// 获取存档数据
        /// </summary>
        public MissionChainSaveData GetSaveData()
        {
            return new MissionChainSaveData
            {
                completedMissions = new List<string>(completedChainMissions),
                chainProgress = new Dictionary<string, int>(chainProgress)
            };
        }
        
        /// <summary>
        /// 加载存档数据
        /// </summary>
        public void LoadSaveData(MissionChainSaveData data)
        {
            if (data == null) return;
            
            completedChainMissions = new HashSet<string>(data.completedMissions);
            chainProgress = new Dictionary<string, int>(data.chainProgress);
            
            Debug.Log("[MissionChainSystem] 连锁数据加载完成");
        }
    }
    
    /// <summary>
    /// 委托连锁配置
    /// </summary>
    [Serializable]
    public class MissionChain
    {
        public string chainId;
        public string chainName;
        [TextArea(2, 3)]
        public string description;
        public List<string> missionIds = new List<string>();
        public int finalReward;
        public Sprite chainIcon;
    }
    
    /// <summary>
    /// 连锁存档数据
    /// </summary>
    [Serializable]
    public class MissionChainSaveData
    {
        public List<string> completedMissions;
        public Dictionary<string, int> chainProgress;
    }
}
