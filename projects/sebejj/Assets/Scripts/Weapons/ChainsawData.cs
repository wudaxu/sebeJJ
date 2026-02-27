using UnityEngine;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 链锯武器数据 - 近战持续伤害武器，无视装甲
    /// </summary>
    [CreateAssetMenu(fileName = "ChainsawData", menuName = "SebeJJ/Weapons/Chainsaw")]
    public class ChainsawData : WeaponData
    {
        [Header("链锯专属")]
        [Tooltip("每秒伤害")]
        public float damagePerSecond = 15f;
        
        [Tooltip("攻击范围(链锯长度)")]
        public float chainsawRange = 2.5f;
        
        [Tooltip("攻击扇形角度")]
        public float attackAngle = 60f;
        
        [Tooltip("锯齿旋转速度")]
        public float bladeRotationSpeed = 720f; // 度/秒
        
        [Tooltip("移动速度惩罚(0-1)")]
        public float moveSpeedPenalty = 0.2f;
        
        [Tooltip("启动延迟")]
        public float spinUpTime = 0.3f;
        
        [Tooltip("停止延迟")]
        public float spinDownTime = 0.5f;
        
        [Tooltip("是否无视装甲")]
        public bool ignoreArmor = true;
        
        [Tooltip("装甲穿透率")]
        public float armorPenetration = 1f;
        
        [Tooltip("每次命中能量消耗")]
        public float energyCostPerSecond = 5f;
        
        [Tooltip("持续攻击时的伤害递增")]
        public float damageRampUp = 0.1f; // 每秒增加10%
        
        [Tooltip("最大伤害倍率")]
        public float maxDamageMultiplier = 2f;

        public ChainsawData()
        {
            weaponName = "链锯";
            description = "高速旋转的链锯，无视装甲造成持续伤害，可在移动中使用";
            weaponType = WeaponType.Melee;
            baseDamage = 15f; // 单次tick伤害
            damageType = DamageType.Kinetic;
            attackRange = 2.5f;
            attackCooldown = 0.1f; // 快速连续攻击
            attackDuration = 0.1f;
            energyCost = 0f; // 持续消耗在每秒
            criticalChance = 0.15f;
            criticalMultiplier = 1.8f;
            canUpgrade = true;
            maxLevel = 5;
            damagePerLevel = 5f;
            knockbackForce = 2f;
        }
    }
}