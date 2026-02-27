using System.Collections.Generic;
using UnityEngine;

namespace SebeJJ.Experience.Difficulty
{
    /// <summary>
    /// 死亡惩罚系统
    /// </summary>
    public class DeathPenaltySystem : MonoBehaviour
    {
        public static DeathPenaltySystem Instance { get; private set; }
        
        [Header("惩罚配置")]
        [SerializeField] private bool enableDeathPenalty = true;
        [SerializeField] private bool loseXPOnDeath = false;
        [SerializeField] private bool damageEquipmentOnDeath = true;
        
        [Header("资源损失")]
        [SerializeField] private float baseResourceLossPercent = 30f;  // NB-005修复: 从50%降低
        [SerializeField] private float maxResourceLossPercent = 50f;   // NB-005修复: 从80%降低
        
        [Header("信用点损失")]
        [SerializeField] private float baseCreditLossPercent = 5f;     // NB-005修复: 从10%降低
        [SerializeField] private float maxCreditLossPercent = 15f;     // NB-005修复: 从30%降低
        
        [Header("装备损伤")]
        [SerializeField] private float baseEquipmentDamagePercent = 10f; // NB-005修复: 从20%降低
        [SerializeField] private float maxEquipmentDamagePercent = 30f;  // NB-005修复: 从50%降低
        
        [Header("重生延迟")]
        [SerializeField] private float baseRespawnDelay = 3f;
        [SerializeField] private float maxRespawnDelay = 10f;
        
        [Header("保险系统")]
        [SerializeField] private bool enableInsurance = true;
        [SerializeField] private float insuranceCostPercent = 10f; // 委托奖励的百分比
        
        private bool hasInsurance = false;
        private InsuranceType currentInsurance = InsuranceType.None;
        
        public System.Action<DeathReport> OnDeathReportGenerated;
        
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
        /// 应用死亡惩罚
        /// </summary>
        public DeathReport ApplyDeathPenalty(DeathContext context)
        {
            if (!enableDeathPenalty)
            {
                return new DeathReport { NoPenalty = true };
            }
            
            var report = new DeathReport();
            report.DeathDepth = context.Depth;
            report.DeathCause = context.Cause;
            report.SessionDuration = context.SessionDuration;
            
            // 计算惩罚倍率
            float penaltyMultiplier = CalculatePenaltyMultiplier(context.Depth);
            
            // 应用保险减免
            if (hasInsurance)
            {
                penaltyMultiplier *= GetInsuranceMultiplier(currentInsurance);
                report.InsuranceApplied = true;
                report.InsuranceType = currentInsurance;
            }
            
            // 1. 资源损失
            report.ResourceLossPercent = Mathf.Lerp(baseResourceLossPercent, maxResourceLossPercent, penaltyMultiplier);
            report.LostResources = LoseResources(report.ResourceLossPercent);
            report.LostResourceValue = CalculateResourceValue(report.LostResources);
            
            // 2. 信用点损失
            report.CreditLossPercent = Mathf.Lerp(baseCreditLossPercent, maxCreditLossPercent, penaltyMultiplier);
            report.LostCredits = LoseCredits(report.CreditLossPercent);
            
            // 3. 装备损伤
            if (damageEquipmentOnDeath)
            {
                report.EquipmentDamagePercent = Mathf.Lerp(baseEquipmentDamagePercent, maxEquipmentDamagePercent, penaltyMultiplier);
                report.DamagedEquipment = ApplyEquipmentDamage(report.EquipmentDamagePercent);
            }
            
            // 4. 经验值损失
            if (loseXPOnDeath)
            {
                report.XPLossPercent = Mathf.Lerp(5f, 15f, penaltyMultiplier);
                report.LostXP = LoseXP(report.XPLossPercent);
            }
            
            // 5. 重生延迟
            report.RespawnDelay = Mathf.Lerp(baseRespawnDelay, maxRespawnDelay, penaltyMultiplier);
            
            // 6. 生存奖励（如果有）
            report.SurvivalBonusXP = CalculateSurvivalBonus(context);
            report.AchievementProgress = GetAchievementProgress(context);
            
            // 记录死亡
            DifficultyManager.Instance.RecordDeath(context.Depth, context.Cause);
            
            // 触发事件
            OnDeathReportGenerated?.Invoke(report);
            
            // 显示死亡报告
            ShowDeathReport(report);
            
            return report;
        }
        
        /// <summary>
        /// 计算惩罚倍率 (0-1)
        /// </summary>
        private float CalculatePenaltyMultiplier(float depth)
        {
            return Mathf.Clamp01(depth / 100f);
        }
        
        /// <summary>
        /// 获取保险减免倍率
        /// </summary>
        private float GetInsuranceMultiplier(InsuranceType insurance)
        {
            return insurance switch
            {
                InsuranceType.Basic => 0.75f,      // 25%减免
                InsuranceType.Standard => 0.5f,   // 50%减免
                InsuranceType.Premium => 0f,     // 100%减免
                _ => 1f
            };
        }
        
        /// <summary>
        /// 损失资源
        /// </summary>
        private List<LostResource> LoseResources(float lossPercent)
        {
            var lostResources = new List<LostResource>();
            
            // 从背包中移除资源
            // var inventory = InventoryManager.Instance;
            // float totalWeight = inventory.GetTotalWeight();
            // float targetLossWeight = totalWeight * (lossPercent / 100f);
            
            // 模拟实现
            // ...
            
            return lostResources;
        }
        
        /// <summary>
        /// 计算资源价值
        /// </summary>
        private int CalculateResourceValue(List<LostResource> resources)
        {
            int total = 0;
            foreach (var res in resources)
            {
                total += res.Value * res.Amount;
            }
            return total;
        }
        
        /// <summary>
        /// 损失信用点
        /// </summary>
        private int LoseCredits(float lossPercent)
        {
            // int currentCredits = PlayerManager.Instance.Credits;
            // int loss = Mathf.RoundToInt(currentCredits * (lossPercent / 100f));
            // PlayerManager.Instance.AddCredits(-loss);
            // return loss;
            return 0;
        }
        
        /// <summary>
        /// 应用装备损伤
        /// </summary>
        private List<DamagedEquipment> ApplyEquipmentDamage(float damagePercent)
        {
            var damagedEquipment = new List<DamagedEquipment>();
            
            // 随机选择装备进行损伤
            // var equipments = EquipmentManager.Instance.GetEquippedItems();
            // ...
            
            return damagedEquipment;
        }
        
        /// <summary>
        /// 损失经验值
        /// </summary>
        private int LoseXP(float lossPercent)
        {
            // int currentXP = PlayerManager.Instance.CurrentXP;
            // int loss = Mathf.RoundToInt(currentXP * (lossPercent / 100f));
            // PlayerManager.Instance.AddXP(-loss);
            // return loss;
            return 0;
        }
        
        /// <summary>
        /// 计算生存奖励
        /// </summary>
        private int CalculateSurvivalBonus(DeathContext context)
        {
            // 基于存活时间给予奖励
            float survivalTime = context.SessionDuration;
            return Mathf.RoundToInt(survivalTime / 60f * 10f); // 每分钟10XP
        }
        
        /// <summary>
        /// 获取成就进度
        /// </summary>
        private Dictionary<string, int> GetAchievementProgress(DeathContext context)
        {
            var progress = new Dictionary<string, int>();
            
            // 深海探险家成就
            // progress["deep_explorer"] = AchievementManager.Instance.GetProgress("deep_explorer");
            
            return progress;
        }
        
        /// <summary>
        /// 购买保险
        /// </summary>
        public void PurchaseInsurance(InsuranceType type)
        {
            if (!enableInsurance) return;
            
            int cost = CalculateInsuranceCost(type);
            
            // if (PlayerManager.Instance.Credits >= cost)
            // {
            //     PlayerManager.Instance.AddCredits(-cost);
            //     hasInsurance = true;
            //     currentInsurance = type;
            // }
        }
        
        /// <summary>
        /// 计算保险费用
        /// </summary>
        private int CalculateInsuranceCost(InsuranceType type)
        {
            float multiplier = type switch
            {
                InsuranceType.Basic => 0.05f,
                InsuranceType.Standard => 0.1f,
                InsuranceType.Premium => 0.2f,
                _ => 0f
            };
            
            // 基于预期委托奖励计算
            // int expectedReward = MissionManager.Instance.GetCurrentMissionExpectedReward();
            // return Mathf.RoundToInt(expectedReward * multiplier);
            return 0;
        }
        
        /// <summary>
        /// 显示死亡报告
        /// </summary>
        private void ShowDeathReport(DeathReport report)
        {
            // UIManager.Instance.ShowDeathReport(report);
            Debug.Log($"[DeathPenalty] 死亡报告 - 深度: {report.DeathDepth}, 损失资源: {report.LostResourceValue}");
        }
        
        /// <summary>
        /// 清除保险（每次下潜后）
        /// </summary>
        public void ClearInsurance()
        {
            hasInsurance = false;
            currentInsurance = InsuranceType.None;
        }
    }
    
    /// <summary>
    /// 死亡上下文
    /// </summary>
    public struct DeathContext
    {
        public float Depth;
        public string Cause;
        public float SessionDuration;
        public int PlayerLevel;
        public float EquipmentScore;
    }
    
    /// <summary>
    /// 死亡报告
    /// </summary>
    public class DeathReport
    {
        public bool NoPenalty;
        public float DeathDepth;
        public string DeathCause;
        public float SessionDuration;
        
        // 损失
        public float ResourceLossPercent;
        public List<LostResource> LostResources;
        public int LostResourceValue;
        
        public float CreditLossPercent;
        public int LostCredits;
        
        public float EquipmentDamagePercent;
        public List<DamagedEquipment> DamagedEquipment;
        
        public float XPLossPercent;
        public int LostXP;
        
        // 重生
        public float RespawnDelay;
        
        // 保险
        public bool InsuranceApplied;
        public InsuranceType InsuranceType;
        
        // 奖励
        public int SurvivalBonusXP;
        public Dictionary<string, int> AchievementProgress;
    }
    
    /// <summary>
    /// 丢失的资源
    /// </summary>
    public class LostResource
    {
        public string ResourceId;
        public string ResourceName;
        public int Amount;
        public int Value;
    }
    
    /// <summary>
    /// 损伤的装备
    /// </summary>
    public class DamagedEquipment
    {
        public string EquipmentId;
        public string EquipmentName;
        public float DurabilityLoss;
        public float RemainingDurability;
    }
    
    /// <summary>
    /// 保险类型
    /// </summary>
    public enum InsuranceType
    {
        None,       // 无保险
        Basic,      // 基础保险
        Standard,   // 标准保险
        Premium     // 高级保险
    }
}
