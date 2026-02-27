using UnityEngine;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 武器类型枚举
    /// </summary>
    public enum WeaponType
    {
        Melee,      // 近战武器
        Ranged      // 远程武器
    }

    /// <summary>
    /// 武器配置 ScriptableObject - WP-001
    /// </summary>
    [CreateAssetMenu(fileName = "NewWeapon", menuName = "SebeJJ/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        [Header("基础信息")]
        public string weaponName = "新武器";
        public string description = "";
        public WeaponType weaponType = WeaponType.Melee;
        public Sprite icon;
        public GameObject weaponPrefab;

        [Header("伤害属性")]
        public float baseDamage = 10f;
        public DamageType damageType = DamageType.Kinetic;
        public float criticalChance = 0.05f;      // 暴击率(0-1)
        public float criticalMultiplier = 2.0f;   // 暴击倍率

        [Header("攻击属性")]
        public float attackRange = 2f;            // 攻击范围
        public float attackCooldown = 1f;         // 攻击间隔(秒)
        public float attackDuration = 0.3f;       // 攻击持续时间
        public int attackCount = 1;               // 每次攻击命中次数

        [Header("远程武器专用")]
        public float projectileSpeed = 10f;       // 弹丸速度
        public float projectileLifetime = 3f;     // 弹丸存在时间
        public GameObject projectilePrefab;       // 弹丸预制体
        public bool piercing = false;             // 是否穿透
        public int pierceCount = 0;               // 可穿透数量

        [Header("近战武器专用")]
        public float attackArc = 90f;             // 攻击扇形角度
        public float knockbackForce = 5f;         // 击退力度

        [Header("手感调优")]
        public float attackWindup = 0.15f;        // 攻击前摇
        public float attackRecovery = 0.25f;      // 攻击后摇
        public float hitStopDuration = 0.05f;     // 命中停顿
        public float windupCancelWindow = 0.3f;   // 前摇取消窗口(0-1)
        public float recoveryCancelWindow = 0.5f; // 后摇取消窗口(0-1)

        [Header("消耗与要求")]
        public float energyCost = 0f;             // 每次攻击能量消耗
        public int requiredLevel = 1;             // 需要等级

        [Header("升级配置")]
        public bool canUpgrade = true;
        public int maxLevel = 3;
        public float damagePerLevel = 5f;         // 每级增加伤害
        public float rangePerLevel = 0.2f;        // 每级增加范围
        public float cooldownReductionPerLevel = 0.05f; // 每级减少冷却

        [Header("特效")]
        public GameObject attackEffectPrefab;     // 攻击特效
        public GameObject hitEffectPrefab;        // 命中特效
        public AudioClip attackSound;             // 攻击音效
        public AudioClip hitSound;                // 命中音效

        /// <summary>
        /// 获取指定等级的伤害值
        /// </summary>
        public float GetDamageAtLevel(int level)
        {
            return baseDamage + damagePerLevel * (level - 1);
        }

        /// <summary>
        /// 获取指定等级的攻击范围
        /// </summary>
        public float GetRangeAtLevel(int level)
        {
            return attackRange + rangePerLevel * (level - 1);
        }

        /// <summary>
        /// 获取指定等级的冷却时间
        /// </summary>
        public float GetCooldownAtLevel(int level)
        {
            return attackCooldown * Mathf.Pow(1 - cooldownReductionPerLevel, level - 1);
        }
    }
}