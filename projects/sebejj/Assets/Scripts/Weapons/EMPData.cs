using UnityEngine;
using System.Collections.Generic;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 电磁脉冲武器数据 - AOE能量武器，眩晕机械敌人
    /// </summary>
    [CreateAssetMenu(fileName = "EMPData", menuName = "SebeJJ/Weapons/EMP")]
    public class EMPData : WeaponData
    {
        [Header("EMP专属")]
        [Tooltip("爆炸范围半径")]
        public float explosionRadius = 8f;
        
        [Tooltip("眩晕持续时间(秒)")]
        public float stunDuration = 3f;
        
        [Tooltip("对机械敌人额外伤害倍率")]
        public float mechanicalBonusDamage = 2f;
        
        [Tooltip("EMP波纹扩散速度")]
        public float waveExpandSpeed = 10f;
        
        [Tooltip("波纹持续时间")]
        public float waveDuration = 0.8f;
        
        [Tooltip("是否对护盾造成额外伤害")]
        public bool shieldDamageBonus = true;
        
        [Tooltip("护盾伤害倍率")]
        public float shieldDamageMultiplier = 2f;
        
        [Tooltip("EMP中心点伤害倍率")]
        public float centerDamageMultiplier = 1.5f;

        public EMPData()
        {
            weaponName = "电磁脉冲";
            description = "释放电磁冲击波，眩晕范围内的机械敌人并造成能量伤害";
            weaponType = WeaponType.Ranged;
            baseDamage = 30f;
            damageType = DamageType.Energy;
            attackRange = 15f;
            attackCooldown = 8f;
            energyCost = 35f;
            criticalChance = 0f; // EMP不会暴击
            criticalMultiplier = 1f;
            canUpgrade = true;
            maxLevel = 5;
            damagePerLevel = 10f;
            cooldownReductionPerLevel = 0.1f;
        }
    }
}