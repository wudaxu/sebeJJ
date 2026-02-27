using UnityEngine;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 伤害类型枚举
    /// </summary>
    public enum DamageType
    {
        Physical,       // 物理伤害
        Energy,         // 能量伤害
        Explosive,      // 爆炸伤害
        Pressure,       // 压强伤害
        Corrosive       // 腐蚀伤害
    }

    /// <summary>
    /// 伤害信息结构体
    /// </summary>
    public struct DamageInfo
    {
        public float amount;           // 伤害数值
        public DamageType type;        // 伤害类型
        public Vector2 direction;      // 伤害方向（用于击退）
        public GameObject source;      // 伤害来源
        public bool isCritical;        // 是否暴击
        public float knockbackForce;   // 击退力度

        public DamageInfo(float amount, DamageType type, Vector2 direction, 
            GameObject source, bool isCritical = false, float knockbackForce = 0f)
        {
            this.amount = amount;
            this.type = type;
            this.direction = direction;
            this.source = source;
            this.isCritical = isCritical;
            this.knockbackForce = knockbackForce;
        }
    }

    /// <summary>
    /// 可受伤接口
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(DamageInfo damageInfo);
        void Heal(float amount);
        bool IsAlive { get; }
        float HealthPercent { get; }
    }

    /// <summary>
    /// 可破坏接口（用于环境物体）
    /// </summary>
    public interface IDestructible
    {
        void OnDestroyed();
        float Durability { get; }
    }

    /// <summary>
    /// 攻击者接口
    /// </summary>
    public interface IAttacker
    {
        GameObject Owner { get; }
        float BaseDamage { get; }
        DamageType DamageType { get; }
    }
}
