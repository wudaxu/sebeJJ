using System.Collections.Generic;
using UnityEngine;

namespace SebeJJ.Experience.Pacing
{
    /// <summary>
    /// 奖励发放时机系统
    /// </summary>
    public class RewardTimingSystem : MonoBehaviour
    {
        public static RewardTimingSystem Instance { get; private set; }
        
        [Header("即时反馈")]
        [SerializeField] private bool enableImmediateFeedback = true;
        [SerializeField] private float resourcePopupDuration = 2f;
        [SerializeField] private float xpPopupDuration = 1.5f;
        
        [Header("连击系统")]
        [SerializeField] private float comboWindow = 5f;
        [SerializeField] private AnimationCurve comboMultiplierCurve;
        
        [Header("里程碑")]
        [SerializeField] private List<MilestoneConfig> milestones;
        
        private int currentResourceCombo = 0;
        private int currentKillCombo = 0;
        private float lastResourceCollectTime = 0f;
        private float lastKillTime = 0f;
        private Dictionary<string, bool> achievedMilestones = new Dictionary<string, bool>();
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        /// <summary>
        /// 资源采集奖励
        /// </summary>
        public void OnResourceCollected(string resourceId, int amount, int value)
        {
            if (!enableImmediateFeedback) return;
            
            // 1. 即时视觉反馈
            ShowCollectionPopup(resourceId, amount, value);
            
            // 2. 音效反馈
            PlayCollectionSFX(amount);
            
            // 3. 连击系统
            UpdateResourceCombo();
            
            // 4. 记录统计
            PacingManager.Instance.RecordResourceCollected(resourceId, amount);
            
            // 5. 检查里程碑
            CheckMilestones();
        }
        
        /// <summary>
        /// 敌人击败奖励
        /// </summary>
        public void OnEnemyDefeated(string enemyId, int xpReward, int creditReward, List<LootItem> loot)
        {
            if (!enableImmediateFeedback) return;
            
            // 1. 即时掉落
            DropLoot(loot);
            
            // 2. 经验值弹出
            ShowXPPopup(xpReward);
            
            // 3. 连杀奖励
            UpdateKillCombo();
            float comboMultiplier = GetComboMultiplier(currentKillCombo);
            int bonusXP = Mathf.RoundToInt(xpReward * (comboMultiplier - 1f));
            
            if (bonusXP > 0)
            {
                ShowBonusXPPopup(bonusXP);
            }
            
            // 4. 记录统计
            PacingManager.Instance.RecordEnemyDefeated(enemyId);
            
            // 5. 检查里程碑
            CheckMilestones();
        }
        
        /// <summary>
        /// 委托完成奖励
        /// </summary>
        public void OnMissionCompleted(MissionReward reward)
        {
            // 1. 结算动画
            PlayRewardAnimation(reward);
            
            // 2. 延迟发放奖励（创造期待感）
            StartCoroutine(DelayedRewardCoroutine(reward));
            
            // 3. 记录统计
            PacingManager.Instance.RecordMissionComplete(reward.MissionId);
            
            // 4. 检查里程碑
            CheckMilestones();
        }
        
        /// <summary>
        /// 里程碑达成
        /// </summary>
        public void OnMilestoneReached(MilestoneType type, string milestoneId)
        {
            if (achievedMilestones.ContainsKey(milestoneId))
                return;
            
            achievedMilestones[milestoneId] = true;
            
            var config = GetMilestoneConfig(type);
            if (config == null) return;
            
            // 显示里程碑庆祝
            ShowMilestoneCelebration(config);
            
            // 发放奖励
            GrantMilestoneRewards(config);
            
            // 解锁内容
            if (!string.IsNullOrEmpty(config.UnlockSystemId))
            {
                // TutorialManager.Instance.UnlockSystem(config.UnlockSystemId, config.UnlockSystemName, config.Description);
            }
        }
        
        /// <summary>
        /// 显示采集弹出
        /// </summary>
        private void ShowCollectionPopup(string resourceId, int amount, int value)
        {
            // UIManager.Instance.ShowResourcePopup(resourceId, amount, value, resourcePopupDuration);
        }
        
        /// <summary>
        /// 播放采集音效
        /// </summary>
        private void PlayCollectionSFX(int amount)
        {
            // 根据采集数量调整音调
            // float pitch = 1f + amount * 0.05f;
            // AudioManager.Instance.PlaySFX("Resource_Collect", pitch: pitch);
        }
        
        /// <summary>
        /// 更新资源连击
        /// </summary>
        private void UpdateResourceCombo()
        {
            float timeSinceLast = Time.time - lastResourceCollectTime;
            
            if (timeSinceLast <= comboWindow)
            {
                currentResourceCombo++;
            }
            else
            {
                currentResourceCombo = 1;
            }
            
            lastResourceCollectTime = Time.time;
            
            // 显示连击
            if (currentResourceCombo > 2)
            {
                // UIManager.Instance.ShowComboText($"采集连击 x{currentResourceCombo}");
            }
        }
        
        /// <summary>
        /// 更新击杀连击
        /// </summary>
        private void UpdateKillCombo()
        {
            float timeSinceLast = Time.time - lastKillTime;
            
            if (timeSinceLast <= comboWindow)
            {
                currentKillCombo++;
            }
            else
            {
                currentKillCombo = 1;
            }
            
            lastKillTime = Time.time;
            
            // 显示连击
            if (currentKillCombo > 2)
            {
                // UIManager.Instance.ShowComboText($"击杀连击 x{currentKillCombo}");
            }
        }
        
        /// <summary>
        /// 获取连击倍率
        /// </summary>
        private float GetComboMultiplier(int combo)
        {
            if (comboMultiplierCurve == null || comboMultiplierCurve.length == 0)
                return 1f + combo * 0.1f;
            
            float normalizedCombo = Mathf.Clamp01(combo / 10f);
            return 1f + comboMultiplierCurve.Evaluate(normalizedCombo);
        }
        
        /// <summary>
        /// 掉落战利品
        /// </summary>
        private void DropLoot(List<LootItem> loot)
        {
            foreach (var item in loot)
            {
                // LootManager.Instance.SpawnLoot(item, dropPosition);
            }
        }
        
        /// <summary>
        /// 显示经验值弹出
        /// </summary>
        private void ShowXPPopup(int xp)
        {
            // UIManager.Instance.ShowXPPopup(xp, xpPopupDuration);
        }
        
        /// <summary>
        /// 显示奖励经验值
        /// </summary>
        private void ShowBonusXPPopup(int bonusXP)
        {
            // UIManager.Instance.ShowBonusXPPopup(bonusXP);
        }
        
        /// <summary>
        /// 播放奖励动画
        /// </summary>
        private void PlayRewardAnimation(MissionReward reward)
        {
            // UIManager.Instance.PlayMissionCompleteAnimation(reward);
        }
        
        /// <summary>
        /// 延迟奖励协程
        /// </summary>
        private System.Collections.IEnumerator DelayedRewardCoroutine(MissionReward reward)
        {
            // 等待动画播放
            yield return new WaitForSeconds(1f);
            
            // 发放信用点
            // PlayerManager.Instance.AddCredits(reward.Credits);
            
            // 发放经验值
            // PlayerManager.Instance.AddXP(reward.XP);
            
            // 发放资源
            foreach (var resource in reward.Resources)
            {
                // InventoryManager.Instance.AddItem(resource.Id, resource.Amount);
            }
            
            // 显示总结
            // UIManager.Instance.ShowRewardSummary(reward);
        }
        
        /// <summary>
        /// 显示里程碑庆祝
        /// </summary>
        private void ShowMilestoneCelebration(MilestoneConfig config)
        {
            // UIManager.Instance.ShowMilestoneCelebration(config.Title, config.Description, config.Icon);
            // AudioManager.Instance.PlaySFX("Milestone_Reached");
        }
        
        /// <summary>
        /// 发放里程碑奖励
        /// </summary>
        private void GrantMilestoneRewards(MilestoneConfig config)
        {
            // PlayerManager.Instance.AddCredits(config.BonusCredits);
            // PlayerManager.Instance.AddXP(config.BonusXP);
        }
        
        /// <summary>
        /// 检查里程碑
        /// </summary>
        private void CheckMilestones()
        {
            // 检查各种里程碑条件
            // CheckFirstDiveMilestone();
            // CheckDepthRecordMilestone();
            // CheckCollectionMilestone();
            // ...
        }
        
        /// <summary>
        /// 获取里程碑配置
        /// </summary>
        private MilestoneConfig GetMilestoneConfig(MilestoneType type)
        {
            foreach (var config in milestones)
            {
                if (config.Type == type)
                    return config;
            }
            return null;
        }
    }
    
    /// <summary>
    /// 任务奖励
    /// </summary>
    public class MissionReward
    {
        public string MissionId;
        public string MissionName;
        public int Credits;
        public int XP;
        public List<ResourceReward> Resources;
        public List<EquipmentReward> Equipment;
    }
    
    public class ResourceReward
    {
        public string Id;
        public int Amount;
    }
    
    public class EquipmentReward
    {
        public string Id;
        public int Durability;
    }
    
    /// <summary>
    /// 战利品
    /// </summary>
    public class LootItem
    {
        public string ItemId;
        public int Amount;
        public float DropChance;
    }
    
    /// <summary>
    /// 里程碑类型
    /// </summary>
    public enum MilestoneType
    {
        FirstDive,          // 首次下潜
        FirstKill,          // 首次击杀
        FirstCollection,    // 首次采集
        FirstMission,       // 首次完成委托
        DepthRecord,        // 新深度记录
        CollectionMilestone,// 采集里程碑
        KillMilestone,      // 击杀里程碑
        MissionMilestone,   // 委托里程碑
        LevelMilestone,     // 等级里程碑
        EquipmentMilestone, // 装备里程碑
        SecretDiscovery     // 秘密发现
    }
    
    /// <summary>
    /// 里程碑配置
    /// </summary>
    [System.Serializable]
    public class MilestoneConfig
    {
        public MilestoneType Type;
        public string Title;
        public string Description;
        public Sprite Icon;
        public int BonusCredits;
        public int BonusXP;
        public string UnlockSystemId;
        public string UnlockSystemName;
    }
}
