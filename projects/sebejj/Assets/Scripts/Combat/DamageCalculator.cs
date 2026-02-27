using UnityEngine;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 伤害计算器 - DM-002
    /// 实现伤害计算公式、类型克制、暴击系统
    /// </summary>
    public static class DamageCalculator
    {
        // 伤害类型克制表 [伤害类型, 目标类型] = 伤害倍率
        private static readonly float[,] DamageMultiplierTable = new float[,]
        {
            // Armor, Shield, Biological
            { 1.50f, 0.75f, 1.00f },  // Kinetic
            { 0.75f, 1.50f, 1.00f },  // Energy
            { 1.25f, 1.25f, 1.00f },  // Explosive
            { 1.00f, 1.00f, 1.50f },  // Corrosive
            { 1.00f, 1.00f, 1.00f }   // True
        };

        /// <summary>
        /// 计算最终伤害
        /// CB-001修复: 真实伤害完全无视所有减免
        /// BF-001修复: 添加最小伤害值限制
        /// </summary>
        public static float CalculateDamage(DamageInfo damageInfo, TargetType targetType, 
            float armorValue = 0, float shieldValue = 0, float damageReduction = 0)
        {
            float finalDamage = damageInfo.BaseDamage;

            // 1. 应用类型克制
            finalDamage *= GetTypeMultiplier(damageInfo.DamageType, targetType);

            // 2. 真实伤害跳过所有防御计算和减免
            if (damageInfo.DamageType != DamageType.True)
            {
                // 3. 应用暴击
                if (damageInfo.IsCritical)
                {
                    finalDamage *= damageInfo.CriticalMultiplier;
                }

                // 4. 应用护甲减伤
                if (armorValue > 0 && targetType == TargetType.Armor)
                {
                    float effectiveArmor = armorValue * (1 - damageInfo.ArmorPenetration);
                    finalDamage = ApplyArmorReduction(finalDamage, effectiveArmor);
                }

                // 5. 应用护盾减伤
                if (shieldValue > 0 && targetType == TargetType.Shield)
                {
                    float effectiveShield = shieldValue * (1 - damageInfo.ShieldPenetration);
                    finalDamage = ApplyShieldReduction(finalDamage, effectiveShield);
                }

                // 6. 应用额外伤害减免（真实伤害跳过此步骤）
                finalDamage *= (1 - Mathf.Clamp01(damageReduction));
            }

            // BF-001修复: 确保伤害至少为1
            return Mathf.Max(1f, finalDamage);
        }

        /// <summary>
        /// 获取类型克制倍率
        /// </summary>
        public static float GetTypeMultiplier(DamageType damageType, TargetType targetType)
        {
            int damageIndex = (int)damageType;
            int targetIndex = (int)targetType;
            
            if (damageIndex >= 0 && damageIndex < 5 && targetIndex >= 0 && targetIndex < 3)
            {
                return DamageMultiplierTable[damageIndex, targetIndex];
            }
            
            return 1.0f;
        }

        /// <summary>
        /// 计算暴击 - DM-003
        /// BF-002修复: 限制暴击率不超过100%
        /// </summary>
        public static bool RollCritical(float criticalChance)
        {
            // BF-002修复: 确保暴击率在0-1范围内
            return Random.value < Mathf.Clamp01(criticalChance);
        }

        /// <summary>
        /// 创建带暴击的伤害信息
        /// </summary>
        public static DamageInfo CreateDamageWithCritical(float baseDamage, DamageType damageType, 
            float criticalChance, float criticalMultiplier = 2.0f)
        {
            DamageInfo damage = new DamageInfo(baseDamage, damageType);
            damage.IsCritical = RollCritical(criticalChance);
            damage.CriticalMultiplier = criticalMultiplier;
            return damage;
        }

        /// <summary>
        /// 护甲减伤公式
        /// 公式: 伤害 = 原始伤害 * (100 / (100 + 护甲值))
        /// BC-001修复: 添加护甲值>0检查，防止除零
        /// </summary>
        private static float ApplyArmorReduction(float damage, float armor)
        {
            // 确保分母至少为1，防止除零风险
            float denominator = 100f + Mathf.Max(-99f, armor);
            return damage * (100f / denominator);
        }

        /// <summary>
        /// 护盾减伤公式
        /// 公式: 伤害 = 原始伤害 * (1 - 护盾减免比例)
        /// 护盾减免有上限(最大80%)
        /// </summary>
        private static float ApplyShieldReduction(float damage, float shield)
        {
            float reduction = shield / (shield + 100f);
            reduction = Mathf.Min(reduction, 0.8f); // 最大80%减免
            return damage * (1 - reduction);
        }

        /// <summary>
        /// 计算生命偷取数值
        /// </summary>
        public static float CalculateLifeSteal(float damageDealt, float lifeStealPercent)
        {
            return damageDealt * Mathf.Clamp01(lifeStealPercent);
        }

        /// <summary>
        /// 计算击退力度
        /// CB-004修复: 确保质量至少为0.1f，防止除以0或负数
        /// </summary>
        public static Vector2 CalculateKnockback(Vector2 direction, float baseForce, 
            float targetMass = 1f, float resistance = 0f)
        {
            // CB-004修复: 确保质量至少为0.1f，使用Abs防止负数
            float effectiveForce = baseForce / Mathf.Max(0.1f, Mathf.Abs(targetMass));
            effectiveForce *= (1 - Mathf.Clamp01(resistance));
            return direction.normalized * effectiveForce;
        }
    }
}