using UnityEngine;
using System;
using System.Collections.Generic;

namespace SebeJJ.Upgrade
{
    /// <summary>
    /// 机甲升级类型枚举
    /// </summary>
    public enum MechaUpgradeType
    {
        Hull,   // 船体升级（耐久+）
        Energy, // 能量升级（能量上限+）
        Speed,  // 速度升级（移动速度+）
        Cargo   // 货舱升级（容量+）
    }
    
    /// <summary>
    /// 武器升级类型枚举
    /// </summary>
    public enum WeaponUpgradeType
    {
        Damage,         // 伤害升级
        Range,          // 射程升级
        AttackSpeed,    // 攻速升级
        EnergyEfficiency // 能量效率升级
    }
    
    /// <summary>
    /// 机甲升级数据配置
    /// </summary>
    [Serializable]
    public class MechaUpgradeData
    {
        [Header("基础信息")]
        public MechaUpgradeType upgradeType;
        public string upgradeName;
        public string description;
        public Sprite icon;
        
        [Header("升级属性")]
        public int maxLevel = 10;
        public float baseValue = 100f;              // 基础值
        public float valueIncrement = 10f;          // 每级增加值
        public float valueMultiplier = 1.1f;        // 每级倍率
        public UpgradeCalculationMode calculationMode = UpgradeCalculationMode.Additive;
        
        [Header("升级需求")]
        public List<LevelRequirement> levelRequirements = new List<LevelRequirement>();
        
        /// <summary>
        /// 获取指定等级的数值
        /// </summary>
        public float GetValueAtLevel(int level)
        {
            if (level <= 0) return baseValue;
            
            switch (calculationMode)
            {
                case UpgradeCalculationMode.Additive:
                    return baseValue + (valueIncrement * (level - 1));
                    
                case UpgradeCalculationMode.Multiplicative:
                    return baseValue * Mathf.Pow(valueMultiplier, level - 1);
                    
                case UpgradeCalculationMode.AdditiveThenMultiplicative:
                    float additive = baseValue + (valueIncrement * (level - 1));
                    return additive * Mathf.Pow(valueMultiplier, (level - 1) * 0.5f);
                    
                default:
                    return baseValue;
            }
        }
        
        /// <summary>
        /// 获取指定等级的倍率
        /// </summary>
        public float GetMultiplierAtLevel(int level)
        {
            return GetValueAtLevel(level) / baseValue;
        }
        
        /// <summary>
        /// 获取指定等级的升级需求
        /// </summary>
        public List<MaterialRequirement> GetRequirementsForLevel(int level)
        {
            var requirement = levelRequirements.Find(r => r.level == level);
            return requirement?.materials ?? new List<MaterialRequirement>();
        }
    }
    
    /// <summary>
    /// 武器升级数据配置
    /// </summary>
    [Serializable]
    public class WeaponUpgradeData
    {
        [Header("基础信息")]
        public string weaponId;
        public string weaponName;
        public Sprite weaponIcon;
        
        [Header("升级配置")]
        public List<WeaponUpgradeEntry> upgrades = new List<WeaponUpgradeEntry>();
        
        /// <summary>
        /// 获取指定类型的升级配置
        /// </summary>
        public WeaponUpgradeEntry GetUpgrade(WeaponUpgradeType type)
        {
            return upgrades.Find(u => u.upgradeType == type);
        }
    }
    
    /// <summary>
    /// 武器单项升级配置
    /// </summary>
    [Serializable]
    public class WeaponUpgradeEntry
    {
        [Header("基础信息")]
        public WeaponUpgradeType upgradeType;
        public string upgradeName;
        public string description;
        public Sprite icon;
        
        [Header("升级属性")]
        public int maxLevel = 5;
        public float baseValue = 1f;
        public float valueIncrement = 0.2f;
        public float valueMultiplier = 1.15f;
        public UpgradeCalculationMode calculationMode = UpgradeCalculationMode.Multiplicative;
        
        [Header("升级需求")]
        public List<LevelRequirement> levelRequirements = new List<LevelRequirement>();
        
        /// <summary>
        /// 获取指定等级的数值
        /// </summary>
        public float GetValueAtLevel(int level)
        {
            if (level <= 0) return baseValue;
            
            switch (calculationMode)
            {
                case UpgradeCalculationMode.Additive:
                    return baseValue + (valueIncrement * (level - 1));
                    
                case UpgradeCalculationMode.Multiplicative:
                    return baseValue * Mathf.Pow(valueMultiplier, level - 1);
                    
                case UpgradeCalculationMode.AdditiveThenMultiplicative:
                    float additive = baseValue + (valueIncrement * (level - 1));
                    return additive * Mathf.Pow(valueMultiplier, (level - 1) * 0.5f);
                    
                default:
                    return baseValue;
            }
        }
        
        /// <summary>
        /// 获取指定等级的升级需求
        /// </summary>
        public List<MaterialRequirement> GetRequirementsForLevel(int level)
        {
            var requirement = levelRequirements.Find(r => r.level == level);
            return requirement?.materials ?? new List<MaterialRequirement>();
        }
    }
    
    /// <summary>
    /// 等级需求配置
    /// </summary>
    [Serializable]
    public class LevelRequirement
    {
        public int level;
        public List<MaterialRequirement> materials = new List<MaterialRequirement>();
    }
    
    /// <summary>
    /// 材料需求
    /// </summary>
    [Serializable]
    public class MaterialRequirement
    {
        public string materialId;
        public int amount;
        
        public MaterialRequirement() { }
        
        public MaterialRequirement(string id, int amt)
        {
            materialId = id;
            amount = amt;
        }
    }
    
    /// <summary>
    /// 升级计算模式
    /// </summary>
    public enum UpgradeCalculationMode
    {
        Additive,                   // 加法模式
        Multiplicative,             // 乘法模式
        AdditiveThenMultiplicative  // 先加后乘
    }
}