using System;
using UnityEngine;

namespace SebeJJ.Data
{
    /// <summary>
    /// 背包物品数据
    /// </summary>
    [Serializable]
    public class InventoryItem
    {
        public string itemId;
        public string itemName;
        public int quantity;
        public float weight;
        public int value;
        public string description;
        public Sprite icon;
    }
    
    /// <summary>
    /// 机甲升级数据
    /// </summary>
    [Serializable]
    public class MechUpgrade
    {
        public string upgradeId;
        public string upgradeName;
        public UpgradeType type;
        public int level = 0;
        public int maxLevel = 5;
        public int baseCost = 100;
        public float valuePerLevel = 10f;
        public string description;
    }
    
    public enum UpgradeType
    {
        OxygenCapacity,     // 氧气容量
        EnergyCapacity,     // 能源容量
        MoveSpeed,          // 移动速度
        ScanRange,          // 扫描范围
        InventoryCapacity,  // 背包容量
        PressureResistance  // 抗压能力
    }
    
    /// <summary>
    /// 游戏配置数据
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "SebeJJ/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("玩家设置")]
        public float baseMoveSpeed = 5f;
        public float baseOxygenCapacity = 100f;
        public float baseEnergyCapacity = 100f;
        public float baseInventoryWeight = 50f;
        public float baseScanRange = 10f;
        
        [Header("经济设置")]
        public int startingCredits = 0;
        public float sellValueMultiplier = 0.7f;
        
        [Header("难度设置")]
        public float oxygenConsumptionMultiplier = 1f;
        public float enemyDamageMultiplier = 1f;
        public float resourceSpawnMultiplier = 1f;
    }
}
