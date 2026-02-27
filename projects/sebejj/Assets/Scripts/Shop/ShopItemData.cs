using UnityEngine;
using System;
using System.Collections.Generic;

namespace SebeJJ.Shop
{
    /// <summary>
    /// 商品类型枚举
    /// </summary>
    public enum ItemType
    {
        Weapon,         // 武器装备
        MechaPart,      // 机甲部件
        Consumable,     // 消耗品
        ModuleUpgrade   // 模块升级
    }

    /// <summary>
    /// 商品子类型
    /// </summary>
    public enum ItemSubType
    {
        // 武器
        LaserCannon,        // 激光炮
        MissileLauncher,    // 导弹发射器
        PlasmaRifle,        // 等离子步枪
        Railgun,            // 轨道炮
        Flamethrower,       // 火焰喷射器
        EMPBlaster,         // EMP冲击波
        
        // 机甲部件
        Engine,             // 引擎
        Armor,              // 装甲
        Drill,              // 钻头
        
        // 消耗品
        OxygenTank,         // 氧气罐
        EnergyBattery,      // 能量电池
        RepairKit,          // 修理包
        
        // 模块升级
        EfficiencyModule,   // 效率模块
        ReinforcementModule,// 强化模块
        OverclockModule     // 超频模块
    }

    /// <summary>
    /// 商品稀有度
    /// </summary>
    public enum ItemRarity
    {
        Common,     // 普通
        Uncommon,   // 稀有
        Rare,       // 史诗
        Legendary,  // 传说
        Mythic      // 神话
    }

    /// <summary>
    /// 商品数据配置 - ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "NewShopItem", menuName = "SebeJJ/Shop/Shop Item")]
    public class ShopItemData : ScriptableObject
    {
        [Header("基础信息")]
        public string itemId;
        public string itemName;
        public string description;
        public Sprite icon;
        
        [Header("分类")]
        public ItemType itemType;
        public ItemSubType subType;
        public ItemRarity rarity;
        
        [Header("价格与库存")]
        public int basePrice;
        public int maxStock = 99;
        public bool isLimitedStock = false;
        public int limitedStockCount = 0;
        
        [Header("解锁条件")]
        public bool isLockedByDefault = false;
        public int requiredPlayerLevel = 1;
        public string requiredAchievementId = "";
        public ShopItemData[] requiredItems; // 需要拥有这些商品才能解锁
        
        [Header("属性加成")]
        public ItemStats stats;
        
        [Header("特效")]
        public GameObject purchaseEffectPrefab;
        public AudioClip purchaseSound;
        
        /// <summary>
        /// 获取当前价格（考虑折扣）
        /// </summary>
        public int GetCurrentPrice(float discountRate = 0f)
        {
            return Mathf.RoundToInt(basePrice * (1f - discountRate));
        }
        
        /// <summary>
        /// 获取稀有度颜色
        /// </summary>
        public Color GetRarityColor()
        {
            return rarity switch
            {
                ItemRarity.Common => new Color(0.7f, 0.7f, 0.7f),
                ItemRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),
                ItemRarity.Rare => new Color(0.2f, 0.5f, 1f),
                ItemRarity.Legendary => new Color(1f, 0.8f, 0.2f),
                ItemRarity.Mythic => new Color(1f, 0.2f, 0.8f),
                _ => Color.white
            };
        }
    }

    /// <summary>
    /// 商品属性
    /// </summary>
    [Serializable]
    public class ItemStats
    {
        // 通用属性
        public int damageBonus;
        public int defenseBonus;
        public int speedBonus;
        public int energyBonus;
        public int oxygenBonus;
        
        // 武器特有
        public float fireRate;
        public float range;
        public int ammoCapacity;
        
        // 机甲部件特有
        public int durability;
        public float efficiency;
        
        // 消耗品特有
        public int restoreAmount;
        public float effectDuration;
        
        // 模块升级特有
        public float upgradeMultiplier;
    }
}