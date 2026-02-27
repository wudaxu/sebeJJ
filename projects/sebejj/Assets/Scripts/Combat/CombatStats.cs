using UnityEngine;
using System;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 伤害事件参数
    /// </summary>
    public class DamageEventArgs : EventArgs
    {
        public DamageInfo DamageInfo;
        public float FinalDamage;
        public bool IsKillingBlow;
        public TargetType HitTargetType; // 实际命中的目标类型(护盾/装甲)
    }

    /// <summary>
    /// 可受伤接口
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(DamageInfo damageInfo);
        bool IsAlive { get; }
        float CurrentHealth { get; }
        float MaxHealth { get; }
        event EventHandler<DamageEventArgs> OnDamageTaken;
        event EventHandler OnDeath;
    }

    /// <summary>
    /// 战斗属性组件 - 管理生命值、护盾、装甲
    /// </summary>
    public class CombatStats : MonoBehaviour, IDamageable
    {
        [Header("基础属性")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float maxShield = 50f;
        [SerializeField] private float maxArmor = 30f;
        
        [Header("护盾恢复")]
        [SerializeField] private float shieldRegenRate = 5f;      // 每秒恢复量
        [SerializeField] private float shieldRegenDelay = 3f;     // 受伤后延迟恢复时间
        [SerializeField] private bool shieldRegenEnabled = true;

        [Header("伤害减免")]
        [SerializeField] private float damageReduction = 0f;      // 额外伤害减免(0-1)

        [Header("无敌帧")]
        [SerializeField] private float invincibilityDuration = 0.1f; // 受伤后无敌时间

        // 当前数值
        private float currentHealth;
        private float currentShield;
        private float currentArmor;
        
        // 状态
        private bool isAlive = true;
        private bool isInvincible = false;
        private float lastDamageTime;
        private float lastDamageUnscaledTime; // CB-002修复: 使用非缩放时间
        private float invincibilityEndTime;

        // 事件
        public event EventHandler<DamageEventArgs> OnDamageTaken;
        public event EventHandler OnDeath;
        public event EventHandler OnShieldBroken;
        public event EventHandler<float> OnHealthChanged;
        public event EventHandler<float> OnShieldChanged;
        public event EventHandler<float> OnArmorChanged;

        // 属性
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthPercent => maxHealth > 0 ? currentHealth / maxHealth : 0;
        
        public float CurrentShield => currentShield;
        public float MaxShield => maxShield;
        public float ShieldPercent => maxShield > 0 ? currentShield / maxShield : 0;
        public bool HasShield => currentShield > 0;
        
        public float CurrentArmor => currentArmor;
        public float MaxArmor => maxArmor;
        public float ArmorPercent => maxArmor > 0 ? currentArmor / maxArmor : 0;
        
        public bool IsAlive => isAlive;
        public bool IsInvincible => isInvincible || Time.time < invincibilityEndTime;

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            UpdateShieldRegeneration();
            UpdateInvincibility();
        }

        /// <summary>
        /// 初始化属性
        /// </summary>
        public void Initialize()
        {
            currentHealth = maxHealth;
            currentShield = maxShield;
            currentArmor = maxArmor;
            isAlive = true;
            isInvincible = false;
        }

        /// <summary>
        /// 受到伤害 - 核心伤害处理逻辑
        /// </summary>
        public void TakeDamage(DamageInfo damageInfo)
        {
            if (!isAlive || IsInvincible) return;

            // 设置目标
            damageInfo.Target = gameObject;

            // 确定先打护盾还是直接打装甲
            TargetType targetType = HasShield ? TargetType.Shield : TargetType.Armor;
            
            // 计算最终伤害
            float finalDamage = DamageCalculator.CalculateDamage(
                damageInfo, targetType, currentArmor, currentShield, damageReduction);

            // 应用伤害
            float actualDamage = ApplyDamage(finalDamage, damageInfo);

            // 触发无敌帧
            StartInvincibility();

            // 触发事件
            var eventArgs = new DamageEventArgs
            {
                DamageInfo = damageInfo,
                FinalDamage = actualDamage,
                IsKillingBlow = !isAlive,
                HitTargetType = targetType
            };
            OnDamageTaken?.Invoke(this, eventArgs);

            // 检查死亡
            if (currentHealth <= 0 && isAlive)
            {
                Die();
            }
        }

        /// <summary>
        /// 应用伤害到具体属性
        /// </summary>
        private float ApplyDamage(float damage, DamageInfo damageInfo)
        {
            float remainingDamage = damage;
            float totalDamageApplied = 0;

            // 1. 先消耗护盾
            if (HasShield)
            {
                float shieldDamage = Mathf.Min(currentShield, remainingDamage);
                currentShield -= shieldDamage;
                remainingDamage -= shieldDamage;
                totalDamageApplied += shieldDamage;

                OnShieldChanged?.Invoke(this, currentShield);

                // 护盾破碎
                if (currentShield <= 0 && shieldDamage > 0)
                {
                    OnShieldBroken?.Invoke(this, EventArgs.Empty);
                }
            }

            // 2. 剩余伤害作用于生命值
            if (remainingDamage > 0)
            {
                currentHealth -= remainingDamage;
                totalDamageApplied += remainingDamage;
                OnHealthChanged?.Invoke(this, currentHealth);
            }

            // 3. 处理生命偷取(给予攻击者)
            if (damageInfo.Attacker != null && damageInfo.LifeSteal > 0)
            {
                float healAmount = DamageCalculator.CalculateLifeSteal(totalDamageApplied, damageInfo.LifeSteal);
                var attackerStats = damageInfo.Attacker.GetComponent<CombatStats>();
                attackerStats?.Heal(healAmount);
            }

            lastDamageTime = Time.time;
            lastDamageUnscaledTime = Time.unscaledTime; // CB-002修复: 记录非缩放时间
            return totalDamageApplied;
        }

        /// <summary>
        /// 治疗
        /// </summary>
        public void Heal(float amount)
        {
            if (!isAlive || amount <= 0) return;
            
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            OnHealthChanged?.Invoke(this, currentHealth);
        }

        /// <summary>
        /// 恢复护盾
        /// </summary>
        public void RestoreShield(float amount)
        {
            if (!isAlive || amount <= 0) return;
            
            currentShield = Mathf.Min(currentShield + amount, maxShield);
            OnShieldChanged?.Invoke(this, currentShield);
        }

        /// <summary>
        /// 修复装甲
        /// </summary>
        public void RepairArmor(float amount)
        {
            if (!isAlive || amount <= 0) return;
            
            currentArmor = Mathf.Min(currentArmor + amount, maxArmor);
            OnArmorChanged?.Invoke(this, currentArmor);
        }

        /// <summary>
        /// 设置最大生命值
        /// </summary>
        public void SetMaxHealth(float value)
        {
            maxHealth = value;
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        /// <summary>
        /// 设置最大护盾
        /// </summary>
        public void SetMaxShield(float value)
        {
            maxShield = value;
            currentShield = Mathf.Min(currentShield, maxShield);
        }

        /// <summary>
        /// 设置最大装甲
        /// </summary>
        public void SetMaxArmor(float value)
        {
            maxArmor = value;
            currentArmor = Mathf.Min(currentArmor, maxArmor);
        }

        /// <summary>
        /// 开始无敌帧
        /// </summary>
        private void StartInvincibility()
        {
            invincibilityEndTime = Time.time + invincibilityDuration;
        }

        private void UpdateInvincibility()
        {
            // 无敌状态自动更新
        }

        /// <summary>
        /// 护盾自动恢复
        /// CB-002修复: 使用Time.unscaledTime避免受时间缩放影响
        /// </summary>
        private void UpdateShieldRegeneration()
        {
            if (!shieldRegenEnabled || !isAlive) return;
            if (currentShield >= maxShield) return;
            // CB-002修复: 使用unscaledTime
            if (Time.unscaledTime < lastDamageUnscaledTime + shieldRegenDelay) return;

            float regenAmount = shieldRegenRate * Time.unscaledDeltaTime;
            currentShield = Mathf.Min(currentShield + regenAmount, maxShield);
            OnShieldChanged?.Invoke(this, currentShield);
        }

        /// <summary>
        /// 死亡处理
        /// </summary>
        private void Die()
        {
            isAlive = false;
            currentHealth = 0;
            OnDeath?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// 复活
        /// </summary>
        public void Revive(float healthPercent = 1f)
        {
            isAlive = true;
            currentHealth = maxHealth * healthPercent;
            currentShield = maxShield;
            OnHealthChanged?.Invoke(this, currentHealth);
            OnShieldChanged?.Invoke(this, currentShield);
        }

        /// <summary>
        /// 立即击杀(用于调试)
        /// </summary>
        public void Kill()
        {
            if (!isAlive) return;
            currentHealth = 0;
            OnHealthChanged?.Invoke(this, currentHealth);
            Die();
        }
    }
}