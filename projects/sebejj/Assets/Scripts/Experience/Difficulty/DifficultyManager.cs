using System;
using System.Collections.Generic;
using UnityEngine;

namespace SebeJJ.Experience.Difficulty
{
    /// <summary>
    /// 难度管理器 - 控制整体游戏难度曲线
    /// </summary>
    public class DifficultyManager : MonoBehaviour
    {
        public static DifficultyManager Instance { get; private set; }
        
        [Header("难度曲线")]
        [SerializeField] private AnimationCurve healthCurve = AnimationCurve.EaseInOut(0, 1, 1, 4);
        [SerializeField] private AnimationCurve damageCurve = AnimationCurve.EaseInOut(0, 1, 1, 3);
        [SerializeField] private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 1, 1, 1.5f);
        [SerializeField] private AnimationCurve spawnRateCurve = AnimationCurve.EaseInOut(0, 0.5f, 1, 2);
        
        [Header("动态调整")]
        [SerializeField] private bool enableDynamicDifficulty = true;
        [SerializeField] private float evaluationInterval = 300f; // 5分钟评估一次
        [SerializeField] private float difficultyAdjustmentRate = 0.1f;
        
        [Header("深度难度")]
        [SerializeField] private float depthDifficultyExponent = 1.5f;
        [SerializeField] private float maxDepthDifficulty = 4f;
        
        private float currentDifficultyMultiplier = 1f;
        private float playerSkillFactor = 1f;
        private float dynamicAdjustment = 1f;
        
        // 统计数据
        private Queue<bool> recentDeaths = new Queue<bool>();
        private Queue<float> recentCompletionTimes = new Queue<float>();
        private const int STAT_WINDOW_SIZE = 10;
        
        public float CurrentDifficultyMultiplier => currentDifficultyMultiplier;
        public float PlayerSkillFactor => playerSkillFactor;
        
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
            if (enableDynamicDifficulty)
            {
                InvokeRepeating(nameof(EvaluateAndAdjustDifficulty), evaluationInterval, evaluationInterval);
            }
        }
        
        /// <summary>
        /// 获取指定深度的难度系数
        /// </summary>
        public float GetDifficultyAtDepth(float depth)
        {
            float baseDifficulty = CalculateBaseDifficulty(depth);
            return baseDifficulty * playerSkillFactor * dynamicAdjustment;
        }
        
        /// <summary>
        /// 计算基础难度（仅基于深度）
        /// </summary>
        private float CalculateBaseDifficulty(float depth)
        {
            float normalizedDepth = Mathf.Clamp01(depth / 100f);
            
            // 非线性增长
            float depthFactor = Mathf.Pow(normalizedDepth, depthDifficultyExponent) * maxDepthDifficulty;
            
            // 基础难度 + 深度难度
            return 1f + depthFactor;
        }
        
        /// <summary>
        /// 获取深度层
        /// </summary>
        public DepthLayer GetDepthLayer(float depth)
        {
            return depth switch
            {
                <= 30f => DepthLayer.Shallow,
                <= 60f => DepthLayer.Mid,
                <= 90f => DepthLayer.Deep,
                _ => DepthLayer.Abyss
            };
        }
        
        /// <summary>
        /// 获取深度层难度倍率
        /// </summary>
        public float GetLayerDifficultyMultiplier(DepthLayer layer)
        {
            return layer switch
            {
                DepthLayer.Shallow => 0.8f,
                DepthLayer.Mid => 1.2f,
                DepthLayer.Deep => 1.8f,
                DepthLayer.Abyss => 2.5f,
                _ => 1f
            };
        }
        
        /// <summary>
        /// 评估并调整难度
        /// </summary>
        private void EvaluateAndAdjustDifficulty()
        {
            if (!enableDynamicDifficulty) return;
            
            // 计算玩家技能因子
            float newSkillFactor = CalculatePlayerSkillFactor();
            
            // 平滑过渡
            playerSkillFactor = Mathf.Lerp(playerSkillFactor, newSkillFactor, difficultyAdjustmentRate);
            playerSkillFactor = Mathf.Clamp(playerSkillFactor, 0.8f, 1.2f);
            
            // 应用动态调整
            ApplyDynamicAdjustments();
            
            Debug.Log($"[DifficultyManager] 难度调整 - 技能因子: {playerSkillFactor:F2}, 动态调整: {dynamicAdjustment:F2}");
        }
        
        /// <summary>
        /// 计算玩家技能因子
        /// </summary>
        private float CalculatePlayerSkillFactor()
        {
            float skillScore = 1f;
            
            // 死亡率评估
            float deathRate = CalculateDeathRate();
            if (deathRate > 0.7f)
                skillScore -= 0.15f; // 经常死亡，降低难度
            else if (deathRate < 0.2f)
                skillScore += 0.1f; // 很少死亡，增加难度
            
            // 完成时间评估
            float avgCompletionTime = CalculateAverageCompletionTime();
            float expectedTime = GetExpectedCompletionTime();
            if (avgCompletionTime < expectedTime * 0.8f)
                skillScore += 0.05f;
            
            return Mathf.Clamp(skillScore, 0.8f, 1.2f);
        }
        
        /// <summary>
        /// 应用动态调整
        /// </summary>
        private void ApplyDynamicAdjustments()
        {
            // 根据玩家表现调整
            if (playerSkillFactor < 0.9f)
            {
                // 玩家 struggling - 降低难度
                dynamicAdjustment = Mathf.MoveTowards(dynamicAdjustment, 0.9f, 0.05f);
            }
            else if (playerSkillFactor > 1.1f)
            {
                // 玩家太强 - 增加挑战
                dynamicAdjustment = Mathf.MoveTowards(dynamicAdjustment, 1.1f, 0.05f);
            }
            else
            {
                // 恢复正常
                dynamicAdjustment = Mathf.MoveTowards(dynamicAdjustment, 1f, 0.02f);
            }
        }
        
        /// <summary>
        /// 记录死亡
        /// </summary>
        public void RecordDeath(float depth, string cause)
        {
            recentDeaths.Enqueue(true);
            if (recentDeaths.Count > STAT_WINDOW_SIZE)
                recentDeaths.Dequeue();
            
            // 记录分析数据
            // AnalyticsManager.Log("player_death", new Dictionary<string, object>
            // {
            //     { "depth", depth },
            //     { "cause", cause },
            //     { "difficulty", GetDifficultyAtDepth(depth) }
            // });
        }
        
        /// <summary>
        /// 记录成功完成
        /// </summary>
        public void RecordSuccess(float duration, float depth)
        {
            recentDeaths.Enqueue(false);
            if (recentDeaths.Count > STAT_WINDOW_SIZE)
                recentDeaths.Dequeue();
            
            recentCompletionTimes.Enqueue(duration);
            if (recentCompletionTimes.Count > STAT_WINDOW_SIZE)
                recentCompletionTimes.Dequeue();
        }
        
        /// <summary>
        /// 计算死亡率
        /// </summary>
        private float CalculateDeathRate()
        {
            if (recentDeaths.Count == 0) return 0f;
            
            int deathCount = 0;
            foreach (var death in recentDeaths)
            {
                if (death) deathCount++;
            }
            
            return (float)deathCount / recentDeaths.Count;
        }
        
        /// <summary>
        /// 计算平均完成时间
        /// </summary>
        private float CalculateAverageCompletionTime()
        {
            if (recentCompletionTimes.Count == 0) return 0f;
            
            float sum = 0f;
            foreach (var time in recentCompletionTimes)
            {
                sum += time;
            }
            
            return sum / recentCompletionTimes.Count;
        }
        
        /// <summary>
        /// 获取预期完成时间
        /// </summary>
        private float GetExpectedCompletionTime()
        {
            // 基于当前难度的预期时间
            return 600f; // 10分钟基准
        }
        
        /// <summary>
        /// 手动调整难度（用于调试）
        /// </summary>
        [ContextMenu("Increase Difficulty")]
        public void DebugIncreaseDifficulty()
        {
            dynamicAdjustment = Mathf.Min(1.5f, dynamicAdjustment + 0.1f);
            Debug.Log($"[DifficultyManager] 难度增加至: {dynamicAdjustment:F2}");
        }
        
        [ContextMenu("Decrease Difficulty")]
        public void DebugDecreaseDifficulty()
        {
            dynamicAdjustment = Mathf.Max(0.5f, dynamicAdjustment - 0.1f);
            Debug.Log($"[DifficultyManager] 难度降低至: {dynamicAdjustment:F2}");
        }
        
        [ContextMenu("Reset Difficulty")]
        public void DebugResetDifficulty()
        {
            dynamicAdjustment = 1f;
            playerSkillFactor = 1f;
            recentDeaths.Clear();
            recentCompletionTimes.Clear();
            Debug.Log("[DifficultyManager] 难度已重置");
        }
    }
    
    /// <summary>
    /// 深度层枚举
    /// </summary>
    public enum DepthLayer
    {
        Shallow,    // 0-30米
        Mid,        // 30-60米
        Deep,       // 60-90米
        Abyss       // 90-100米
    }
}
