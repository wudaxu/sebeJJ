using UnityEngine;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 伤害类型枚举
    /// </summary>
    public enum DamageType
    {
        Kinetic,    // 动能 - 对装甲150%，对护盾75%
        Energy,     // 能量 - 对装甲75%，对护盾150%
        Explosive,  // 爆炸 - 对装甲125%，对护盾125%
        Corrosive,  // 腐蚀 - 对生物150%
        True        // 真实伤害 - 无视防御
    }

    /// <summary>
    /// 目标类型枚举
    /// </summary>
    public enum TargetType
    {
        Armor,      // 装甲
        Shield,     // 护盾
        Biological  // 生物
    }

    /// <summary>
    /// 伤害信息数据结构 - DM-001
    /// </summary>
    public struct DamageInfo
    {
        public float BaseDamage;           // 基础伤害
        public DamageType DamageType;      // 伤害类型
        public GameObject Attacker;        // 攻击者
        public GameObject Target;          // 目标
        public Vector2 HitPosition;        // 命中位置
        public Vector2 HitDirection;       // 命中方向
        public bool IsCritical;            // 是否暴击
        public float CriticalMultiplier;   // 暴击倍率
        public float ArmorPenetration;     // 护甲穿透(0-1)
        public float ShieldPenetration;    // 护盾穿透(0-1)
        public float LifeSteal;            // 生命偷取比例
        public float KnockbackForce;       // 击退力度
        public float StunDuration;         // 眩晕时长

        public DamageInfo(float baseDamage, DamageType damageType)
        {
            BaseDamage = baseDamage;
            DamageType = damageType;
            Attacker = null;
            Target = null;
            HitPosition = Vector2.zero;
            HitDirection = Vector2.zero;
            IsCritical = false;
            CriticalMultiplier = 2.0f;
            ArmorPenetration = 0f;
            ShieldPenetration = 0f;
            LifeSteal = 0f;
            KnockbackForce = 0f;
            StunDuration = 0f;
        }
    }
}