using UnityEngine;
using System;
using System.Collections.Generic;

namespace SebeJJ.Upgrade
{
    /// <summary>
    /// 材料类型枚举
    /// </summary>
    public enum MaterialType
    {
        Metal,      // 金属
        Crystal,    // 水晶
        Organic,    // 有机
        Energy,     // 能量
        Ancient,    // 古代遗物
        Special     // 特殊
    }
    
    /// <summary>
    /// 材料稀有度枚举
    /// </summary>
    public enum MaterialRarity
    {
        Common,     // 普通
        Uncommon,   // 罕见
        Rare,       // 稀有
        Epic,       // 史诗
        Legendary   // 传说
    }
    
    /// <summary>
    /// 材料数据
    /// </summary>
    [Serializable]
    public class MaterialData
    {
        [Header("基础信息")]
        public string materialId;
        public string materialName;
        public string description;
        public MaterialType materialType;
        public MaterialRarity rarity;
        public Sprite icon;
        
        [Header("堆叠与价值")]
        public int maxStack = 999;
        public int baseValue = 10;          // 基础价值（用于出售）
        
        [Header("获取途径")]
        public List<MaterialSource> sources = new List<MaterialSource>();
        
        [Header("额外信息")]
        public bool canBeCrafted = false;
        public string loreText = "";        // 背景故事
        
        /// <summary>
        /// 获取稀有度颜色
        /// </summary>
        public Color GetRarityColor()
        {
            switch (rarity)
            {
                case MaterialRarity.Common:     return new Color(0.7f, 0.7f, 0.7f);  // 灰色
                case MaterialRarity.Uncommon:   return new Color(0.2f, 0.8f, 0.2f);  // 绿色
                case MaterialRarity.Rare:       return new Color(0.2f, 0.5f, 1f);    // 蓝色
                case MaterialRarity.Epic:       return new Color(0.8f, 0.2f, 0.8f);  // 紫色
                case MaterialRarity.Legendary:  return new Color(1f, 0.6f, 0.1f);    // 橙色
                default: return Color.white;
            }
        }
        
        /// <summary>
        /// 获取稀有度名称
        /// </summary>
        public string GetRarityName()
        {
            switch (rarity)
            {
                case MaterialRarity.Common:     return "普通";
                case MaterialRarity.Uncommon:   return "罕见";
                case MaterialRarity.Rare:       return "稀有";
                case MaterialRarity.Epic:       return "史诗";
                case MaterialRarity.Legendary:  return "传说";
                default: return "未知";
            }
        }
    }
    
    /// <summary>
    /// 材料获取途径
    /// </summary>
    [Serializable]
    public class MaterialSource
    {
        public SourceType sourceType;
        public string sourceId;             // 资源ID或敌人ID
        public float dropChance;            // 掉落概率
        public int minAmount = 1;
        public int maxAmount = 1;
    }
    
    /// <summary>
    /// 获取途径类型
    /// </summary>
    public enum SourceType
    {
        Resource,   // 资源采集
        Enemy,      // 敌人掉落
        Crafting,   // 合成
        Shop,       // 商店购买
        Quest       // 任务奖励
    }
    
    /// <summary>
    /// 合成配方
    /// </summary>
    [Serializable]
    public class CraftingRecipe
    {
        [Header("基础信息")]
        public string recipeId;
        public string recipeName;
        public string description;
        public Sprite icon;
        
        [Header("材料需求")]
        public List<MaterialRequirement> inputs = new List<MaterialRequirement>();
        public List<MaterialRequirement> outputs = new List<MaterialRequirement>();
        
        [Header("合成条件")]
        public int requiredLevel = 1;
        public bool requireWorkbench = false;
        public float craftTime = 0f;        // 0表示即时合成
    }
    
    /// <summary>
    /// 资源转换配置
    /// </summary>
    [Serializable]
    public class ResourceConversion
    {
        public string resourceType;
        public List<ConversionOutput> outputs = new List<ConversionOutput>();
    }
    
    /// <summary>
    /// 转换产出
    /// </summary>
    [Serializable]
    public class ConversionOutput
    {
        public string materialId;
        public float amountMultiplier = 1f;
        public float bonusChance = 0f;      // 额外产出的概率
    }
    
    /// <summary>
    /// 敌人掉落表
    /// </summary>
    [Serializable]
    public class EnemyDropTable
    {
        public string enemyType;
        public List<DropEntry> possibleDrops = new List<DropEntry>();
    }
    
    /// <summary>
    /// 掉落条目
    /// </summary>
    [Serializable]
    public class DropEntry
    {
        public string materialId;
        public float dropChance;
        public int minAmount = 1;
        public int maxAmount = 1;
    }
}