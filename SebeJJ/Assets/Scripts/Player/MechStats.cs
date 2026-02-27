using UnityEngine;

namespace SebeJJ.Player
{
    /// <summary>
    /// 机甲属性 - ScriptableObject配置
    /// </summary>
    [CreateAssetMenu(fileName = "MechStats", menuName = "SebeJJ/MechStats")]
    public class MechStats : ScriptableObject
    {
        [Header("基础属性")]
        [Tooltip("最大生命值")]
        public float maxHealth = 100f;
        
        [Tooltip("最大能量值")]
        public float maxEnergy = 100f;
        
        [Tooltip("最大氧气值")]
        public float maxOxygen = 100f;

        [Header("防御属性")]
        [Tooltip("护甲值 (0-100)")]
        [Range(0, 100)]
        public float armor = 10f;
        
        [Tooltip("压强抗性 (米)")]
        public float pressureResistance = 100f;
        
        [Tooltip("腐蚀抗性 (0-100)")]
        [Range(0, 100)]
        public float corrosionResistance = 0f;

        [Header("机动属性")]
        [Tooltip("基础移动速度")]
        public float speed = 5f;
        
        [Tooltip("转向速度")]
        public float turnRate = 180f;
        
        [Tooltip("加速度")]
        public float acceleration = 10f;

        [Header("采集属性")]
        [Tooltip("采矿功率")]
        public float miningPower = 1f;
        
        [Tooltip("货舱容量")]
        public float cargoCapacity = 50f;
        
        [Tooltip("扫描范围")]
        public float scanRange = 10f;

        [Header("能量")]
        [Tooltip("能量恢复速率")]
        public float energyRegenRate = 5f;
        
        [Tooltip("移动能量消耗")]
        public float moveEnergyCost = 0.1f;

        [Header("特殊")]
        [Tooltip("是否允许水下推进")]
        public bool hasThrusters = false;
        
        [Tooltip("是否具备夜视能力")]
        public bool hasNightVision = false;
        
        [Tooltip("声纳范围")]
        public float sonarRange = 0f;

        // 运行时修改器
        private float _speedModifier = 1f;
        private float _armorModifier = 1f;
        private float _miningModifier = 1f;

        /// <summary>
        /// 获取修改后的速度
        /// </summary>
        public float GetModifiedSpeed(float baseSpeed)
        {
            return baseSpeed * _speedModifier;
        }

        /// <summary>
        /// 获取修改后的护甲
        /// </summary>
        public float GetModifiedArmor()
        {
            return armor * _armorModifier;
        }

        /// <summary>
        /// 获取修改后的采矿功率
        /// </summary>
        public float GetModifiedMiningPower()
        {
            return miningPower * _miningModifier;
        }

        /// <summary>
        /// 设置速度修改器
        /// </summary>
        public void SetSpeedModifier(float modifier)
        {
            _speedModifier = Mathf.Max(0.1f, modifier);
        }

        /// <summary>
        /// 设置护甲修改器
        /// </summary>
        public void SetArmorModifier(float modifier)
        {
            _armorModifier = Mathf.Max(0.1f, modifier);
        }

        /// <summary>
        /// 设置采矿修改器
        /// </summary>
        public void SetMiningModifier(float modifier)
        {
            _miningModifier = Mathf.Max(0.1f, modifier);
        }

        /// <summary>
        /// 重置所有修改器
        /// </summary>
        public void ResetModifiers()
        {
            _speedModifier = 1f;
            _armorModifier = 1f;
            _miningModifier = 1f;
        }
    }
}
