using UnityEngine;

namespace SebeJJ.Experience.Difficulty
{
    /// <summary>
    /// 敌人强度缩放系统
    /// </summary>
    public class EnemyScalingSystem : MonoBehaviour
    {
        public static EnemyScalingSystem Instance { get; private set; }
        
        [Header("属性曲线")]
        [SerializeField] private AnimationCurve healthCurve = AnimationCurve.EaseInOut(0, 1, 1, 5);
        [SerializeField] private AnimationCurve damageCurve = AnimationCurve.EaseInOut(0, 1, 1, 3);
        [SerializeField] private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 1, 1, 1.3f);
        [SerializeField] private AnimationCurve attackSpeedCurve = AnimationCurve.EaseInOut(0, 1, 1, 1.5f);
        [SerializeField] private AnimationCurve spawnRateCurve = AnimationCurve.Linear(0, 0.5f, 1, 2f);
        
        [Header("精英敌人")]
        [SerializeField] private float eliteChanceBase = 0.05f;
        [SerializeField] private float eliteChanceMax = 0.3f;
        [SerializeField] private AnimationCurve eliteChanceCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Boss配置")]
        [SerializeField] private BossDifficultyConfig[] bossConfigs;
        
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
        /// 计算缩放后的敌人属性
        /// </summary>
        public ScaledEnemyStats CalculateScaledStats(EnemyBaseData enemyData, float depth)
        {
            float normalizedDepth = Mathf.Clamp01(depth / 100f);
            float difficulty = DifficultyManager.Instance.GetDifficultyAtDepth(depth);
            
            return new ScaledEnemyStats
            {
                maxHealth = enemyData.BaseHealth * healthCurve.Evaluate(normalizedDepth) * difficulty,
                damage = enemyData.BaseDamage * damageCurve.Evaluate(normalizedDepth) * difficulty,
                moveSpeed = enemyData.BaseSpeed * speedCurve.Evaluate(normalizedDepth),
                attackSpeed = enemyData.BaseAttackSpeed * attackSpeedCurve.Evaluate(normalizedDepth),
                xpReward = Mathf.RoundToInt(enemyData.BaseXP * (1 + difficulty * 0.5f)),
                creditReward = Mathf.RoundToInt(enemyData.BaseCredits * (1 + difficulty * 0.3f)),
                spawnWeight = enemyData.BaseSpawnWeight * spawnRateCurve.Evaluate(normalizedDepth)
            };
        }
        
        /// <summary>
        /// 获取精英敌人出现概率
        /// </summary>
        public float GetEliteChance(float depth)
        {
            float normalizedDepth = Mathf.Clamp01(depth / 100f);
            float depthFactor = eliteChanceCurve.Evaluate(normalizedDepth);
            
            return Mathf.Lerp(eliteChanceBase, eliteChanceMax, depthFactor);
        }
        
        /// <summary>
        /// 应用精英属性加成
        /// </summary>
        public ScaledEnemyStats ApplyEliteModifiers(ScaledEnemyStats baseStats)
        {
            return new ScaledEnemyStats
            {
                maxHealth = baseStats.maxHealth * 2f,
                damage = baseStats.damage * 1.5f,
                moveSpeed = baseStats.moveSpeed * 1.2f,
                attackSpeed = baseStats.attackSpeed * 1.3f,
                xpReward = Mathf.RoundToInt(baseStats.xpReward * 2f),
                creditReward = Mathf.RoundToInt(baseStats.creditReward * 2f),
                isElite = true
            };
        }
        
        /// <summary>
        /// 获取Boss难度配置
        /// </summary>
        public BossDifficultyConfig GetBossConfig(string bossId)
        {
            foreach (var config in bossConfigs)
            {
                if (config.bossId == bossId)
                    return config;
            }
            return null;
        }
        
        /// <summary>
        /// 获取Boss当前阶段
        /// </summary>
        public BossPhase GetCurrentBossPhase(string bossId, float healthPercent)
        {
            var config = GetBossConfig(bossId);
            if (config == null) return null;
            
            foreach (var phase in config.phases)
            {
                if (healthPercent >= phase.healthThreshold)
                    return phase;
            }
            
            return config.phases[config.phases.Length - 1];
        }
    }
    
    /// <summary>
    /// 敌人基础数据
    /// </summary>
    [System.Serializable]
    public class EnemyBaseData
    {
        public string enemyId;
        public string displayName;
        public float BaseHealth = 100f;
        public float BaseDamage = 10f;
        public float BaseSpeed = 3f;
        public float BaseAttackSpeed = 1f;
        public int BaseXP = 50;
        public int BaseCredits = 20;
        public float BaseSpawnWeight = 1f;
        public int MinSpawnDepth = 0;
        public EnemyType enemyType;
    }
    
    /// <summary>
    /// 缩放后的敌人属性
    /// </summary>
    [System.Serializable]
    public class ScaledEnemyStats
    {
        public float maxHealth;
        public float damage;
        public float moveSpeed;
        public float attackSpeed;
        public int xpReward;
        public int creditReward;
        public float spawnWeight;
        public bool isElite = false;
    }
    
    /// <summary>
    /// 敌人类型
    /// </summary>
    public enum EnemyType
    {
        MechanicalFish,     // 机械鱼
        MechanicalCrab,     // 机械蟹
        MechanicalJellyfish,// 机械水母
        Boss                // Boss
    }
    
    /// <summary>
    /// Boss难度配置
    /// </summary>
    [System.Serializable]
    public class BossDifficultyConfig
    {
        public string bossId;
        public string bossName;
        public BossPhase[] phases;
    }
    
    /// <summary>
    /// Boss阶段
    /// </summary>
    [System.Serializable]
    public class BossPhase
    {
        public string phaseName;
        [Range(0, 1)]
        public float healthThreshold; // 血量百分比阈值
        public float attackCooldownMultiplier = 1f;
        public float damageMultiplier = 1f;
        public float speedMultiplier = 1f;
        public BossAbility[] availableAbilities;
        public bool enraged = false;
    }
    
    /// <summary>
    /// Boss技能
    /// </summary>
    public enum BossAbility
    {
        ClawSwipe,      // 钳击
        BubbleShield,   // 泡泡护盾
        DrillCharge,    // 钻头冲锋
        MineDrop,       // 布雷
        LaserBeam,      // 激光
        SummonMinions   // 召唤小怪
    }
}
