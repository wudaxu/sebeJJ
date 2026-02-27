using UnityEngine;

namespace SebeJJ.ScriptableObjects
{
    /// <summary>
    /// 升级数据 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "UpgradeData", menuName = "SebeJJ/Upgrade Data")]
    public class UpgradeData : ScriptableObject
    {
        [Header("基础信息")]
        public string upgradeId;
        public string upgradeName;
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;
        
        [Header("类型")]
        public UpgradeType upgradeType;
        public int maxLevel = 5;
        
        [Header("成本")]
        public int baseCost = 100;
        public float costMultiplier = 1.5f;
        
        [Header("效果")]
        public float baseValue = 10f;
        public float valuePerLevel = 5f;
        
        [Header("前置条件")]
        public UpgradeData[] prerequisites;
        public int requiredPlayerLevel = 1;
        
        [Header("解锁")]
        public bool startsLocked = false;
        public string unlockCondition;

        /// <summary>
        /// 获取指定等级的成本
        /// </summary>
        public int GetCost(int level)
        {
            return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, level - 1));
        }

        /// <summary>
        /// 获取指定等级的数值
        /// </summary>
        public float GetValue(int level)
        {
            return baseValue + valuePerLevel * (level - 1);
        }
    }

    public enum UpgradeType
    {
        // 基础属性
        Health,
        Energy,
        Oxygen,
        
        // 防御
        Armor,
        PressureResistance,
        CorrosionResistance,
        
        // 机动
        Speed,
        TurnRate,
        Acceleration,
        
        // 采集
        MiningPower,
        CargoCapacity,
        ScanRange,
        
        // 能量
        EnergyRegen,
        EnergyEfficiency,
        
        // 武器
        Damage,
        FireRate,
        Range,
        
        // 特殊
        SonarRange,
        LightIntensity,
        OxygenEfficiency
    }
}