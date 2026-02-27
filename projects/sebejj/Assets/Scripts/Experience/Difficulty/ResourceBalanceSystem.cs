using System.Collections.Generic;
using UnityEngine;

namespace SebeJJ.Experience.Difficulty
{
    /// <summary>
    /// 资源价值平衡系统
    /// </summary>
    public class ResourceBalanceSystem : MonoBehaviour
    {
        public static ResourceBalanceSystem Instance { get; private set; }
        
        [Header("稀有度倍率")]
        [SerializeField] private float commonMultiplier = 1f;
        [SerializeField] private float uncommonMultiplier = 1.5f;
        [SerializeField] private float rareMultiplier = 2.5f;
        [SerializeField] private float epicMultiplier = 5f;
        [SerializeField] private float legendaryMultiplier = 10f;
        
        [Header("深度加成")]
        [SerializeField] private float depthBonusExponent = 1.2f;
        [SerializeField] private float maxDepthBonus = 3f;
        
        [Header("风险调整")]
        [SerializeField] private bool enableRiskAdjustment = true;
        [SerializeField] private float riskAdjustmentFactor = 0.5f;
        
        [Header("市场波动")]
        [SerializeField] private bool enableMarketFluctuation = true;
        [SerializeField] private float fluctuationRange = 0.2f;
        
        private Dictionary<string, float> marketFactors = new Dictionary<string, float>();
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Start()
        {
            // 初始化市场波动
            if (enableMarketFluctuation)
            {
                InitializeMarketFactors();
            }
        }
        
        /// <summary>
        /// 计算资源价值
        /// </summary>
        public int CalculateResourceValue(ResourceData resource, float depth)
        {
            // 基础价值
            int baseValue = resource.BaseValue;
            
            // 深度加成
            float depthBonus = CalculateDepthBonus(depth);
            
            // 稀有度倍率
            float rarityMultiplier = GetRarityMultiplier(resource.Rarity);
            
            // 风险调整
            float riskMultiplier = enableRiskAdjustment ? CalculateRiskMultiplier(depth) : 1f;
            
            // 市场波动
            float marketFactor = enableMarketFluctuation ? GetMarketFactor(resource.ResourceId) : 1f;
            
            // 计算最终价值
            float finalValue = baseValue * depthBonus * rarityMultiplier * riskMultiplier * marketFactor;
            
            return Mathf.RoundToInt(finalValue);
        }
        
        /// <summary>
        /// 计算深度加成
        /// </summary>
        private float CalculateDepthBonus(float depth)
        {
            float normalizedDepth = Mathf.Clamp01(depth / 100f);
            return 1f + Mathf.Pow(normalizedDepth, depthBonusExponent) * (maxDepthBonus - 1f);
        }
        
        /// <summary>
        /// 获取稀有度倍率
        /// </summary>
        private float GetRarityMultiplier(ResourceRarity rarity)
        {
            return rarity switch
            {
                ResourceRarity.Common => commonMultiplier,
                ResourceRarity.Uncommon => uncommonMultiplier,
                ResourceRarity.Rare => rareMultiplier,
                ResourceRarity.Epic => epicMultiplier,
                ResourceRarity.Legendary => legendaryMultiplier,
                _ => 1f
            };
        }
        
        /// <summary>
        /// 计算风险调整倍率
        /// </summary>
        private float CalculateRiskMultiplier(float depth)
        {
            // 获取该深度历史死亡率
            float deathRate = GetDeathRateAtDepth(depth);
            
            // 死亡率高 = 更高的价值补偿
            return 1f + deathRate * riskAdjustmentFactor;
        }
        
        /// <summary>
        /// 获取市场波动因子
        /// </summary>
        private float GetMarketFactor(string resourceId)
        {
            if (!marketFactors.ContainsKey(resourceId))
            {
                marketFactors[resourceId] = Random.Range(1f - fluctuationRange, 1f + fluctuationRange);
            }
            
            return marketFactors[resourceId];
        }
        
        /// <summary>
        /// 更新市场波动（定期调用）
        /// </summary>
        public void UpdateMarketFluctuation()
        {
            if (!enableMarketFluctuation) return;
            
            var keys = new List<string>(marketFactors.Keys);
            foreach (var key in keys)
            {
                // 随机游走
                float change = Random.Range(-0.05f, 0.05f);
                marketFactors[key] = Mathf.Clamp(marketFactors[key] + change, 1f - fluctuationRange, 1f + fluctuationRange);
            }
        }
        
        /// <summary>
        /// 获取指定深度的死亡率
        /// </summary>
        private float GetDeathRateAtDepth(float depth)
        {
            // 从分析系统获取
            // return AnalyticsManager.Instance.GetDeathRateAtDepth(depth);
            
            // 模拟数据
            DepthLayer layer = DifficultyManager.Instance.GetDepthLayer(depth);
            return layer switch
            {
                DepthLayer.Shallow => 0.05f,
                DepthLayer.Mid => 0.15f,
                DepthLayer.Deep => 0.30f,
                DepthLayer.Abyss => 0.50f,
                _ => 0.1f
            };
        }
        
        /// <summary>
        /// 获取资源风险等级
        /// </summary>
        public int GetResourceRiskLevel(ResourceData resource)
        {
            return resource.MinDepth switch
            {
                < 30 => 1,
                < 60 => 2,
                < 90 => 3,
                _ => 4
            };
        }
        
        /// <summary>
        /// 初始化市场因子
        /// </summary>
        private void InitializeMarketFactors()
        {
            // 可以从存档加载或生成新的
            marketFactors.Clear();
        }
    }
    
    /// <summary>
    /// 资源数据
    /// </summary>
    [System.Serializable]
    public class ResourceData
    {
        public string ResourceId;
        public string DisplayName;
        public int BaseValue;
        public ResourceRarity Rarity;
        public int MinDepth;
        public float Weight;
        public Sprite Icon;
    }
    
    /// <summary>
    /// 资源稀有度
    /// </summary>
    public enum ResourceRarity
    {
        Common,     // 普通
        Uncommon,   // 罕见
        Rare,       // 稀有
        Epic,       // 史诗
        Legendary   // 传说
    }
}
