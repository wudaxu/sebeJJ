using UnityEngine;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 等离子炮武器数据 - 远程能量武器，穿透敌人
    /// </summary>
    [CreateAssetMenu(fileName = "PlasmaCannonData", menuName = "SebeJJ/Weapons/Plasma Cannon")]
    public class PlasmaCannonData : WeaponData
    {
        [Header("等离子炮专属")]
        [Tooltip("等离子球大小")]
        public float plasmaBallSize = 1.0f;
        
        [Tooltip("穿透后伤害衰减比例")]
        public float pierceDamageFalloff = 0.8f;
        
        [Tooltip("等离子轨迹持续时间")]
        public float trailDuration = 0.5f;
        
        [Tooltip("等离子颜色")]
        public Color plasmaColor = new Color(0.2f, 0.6f, 1.0f, 1.0f);
        
        [Tooltip("穿透最大距离")]
        public float maxPierceDistance = 20f;
        
        [Tooltip("充能时间")]
        public float chargeTime = 0.3f;

        public PlasmaCannonData()
        {
            weaponName = "等离子炮";
            description = "发射高能等离子球，穿透路径上所有敌人";
            weaponType = WeaponType.Ranged;
            baseDamage = 60f;
            damageType = DamageType.Energy;
            attackRange = 20f;
            attackCooldown = 1.2f;
            energyCost = 20f;
            piercing = true;
            pierceCount = 999; // 理论上无限穿透，受距离限制
            projectileSpeed = 15f;
            projectileLifetime = 2f;
            criticalChance = 0.1f;
            criticalMultiplier = 1.5f;
            canUpgrade = true;
            maxLevel = 5;
            damagePerLevel = 15f;
            cooldownReductionPerLevel = 0.08f;
        }
    }
}