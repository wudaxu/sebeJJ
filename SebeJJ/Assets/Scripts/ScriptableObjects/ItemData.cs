using UnityEngine;

namespace SebeJJ.ScriptableObjects
{
    /// <summary>
    /// 物品数据 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "ItemData", menuName = "SebeJJ/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("基础信息")]
        public string itemId;
        public string itemName;
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;
        
        [Header("属性")]
        public ItemType itemType;
        public Rarity rarity;
        public int maxStack = 99;
        public int baseValue = 10;
        public float weight = 1f;
        
        [Header("效果")]
        public bool isConsumable;
        public float healthRestore;
        public float energyRestore;
        public float oxygenRestore;
        
        [Header("装备")]
        public bool isEquipable;
        public EquipmentSlot equipSlot;
        public MechStats statModifiers;
    }

    public enum ItemType
    {
        Resource,
        Consumable,
        Equipment,
        Weapon,
        Module,
        Quest
    }

    public enum Rarity
    {
        Common,     // 普通
        Uncommon,   // 罕见
        Rare,       // 稀有
        Epic,       // 史诗
        Legendary   // 传说
    }

    public enum EquipmentSlot
    {
        Weapon,
        Armor,
        Engine,
        Scanner,
        Utility
    }
}