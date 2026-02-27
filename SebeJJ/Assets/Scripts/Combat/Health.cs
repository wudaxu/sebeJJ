using UnityEngine;

namespace SebeJJ.Combat
{
    /// <summary>
    /// 生命值组件 - 实现IDamageable接口
    /// </summary>
    public class Health : MonoBehaviour, IDamageable
    {
        [Header("生命值")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;

        [Header("防御")]
        [SerializeField] private float armor = 0f;
        [SerializeField] private float resistancePhysical = 0f;
        [SerializeField] private float resistanceEnergy = 0f;
        [SerializeField] private float resistanceExplosive = 0f;
        [SerializeField] private float resistancePressure = 0f;
        [SerializeField] private float resistanceCorrosive = 0f;

        [Header("设置")]
        [SerializeField] private bool destroyOnDeath = true;
        [SerializeField] private float destroyDelay = 2f;
        [SerializeField] private bool invincible = false;

        [Header("事件")]
        public System.Action OnDeath;
        public System.Action OnTakeDamage;
        public System.Action OnHeal;
        public System.Action<float, float> OnHealthChanged; // 当前值, 最大值

        // 属性
        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public float HealthPercent => maxHealth > 0 ? currentHealth / maxHealth : 0f;
        public bool IsAlive => currentHealth > 0f;
        public bool IsDead => currentHealth <= 0f;
        public bool IsInvincible => invincible;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        private void Start()
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(DamageInfo damageInfo)
        {
            if (!IsAlive || invincible) return;

            // 计算实际伤害（考虑护甲和抗性）
            float finalDamage = CalculateDamage(damageInfo);
            finalDamage = Mathf.Max(0, finalDamage);

            // 应用伤害
            currentHealth -= finalDamage;
            currentHealth = Mathf.Max(0, currentHealth);

            // 触发事件
            OnTakeDamage?.Invoke();
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            // 显示伤害数字
            DamageNumberManager.Instance?.ShowDamage(finalDamage, transform.position, damageInfo.isCritical);

            // 应用击退
            if (damageInfo.knockbackForce > 0 && TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.AddForce(damageInfo.direction * damageInfo.knockbackForce, ForceMode2D.Impulse);
            }

            // 检查死亡
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 计算实际伤害
        /// </summary>
        private float CalculateDamage(DamageInfo damageInfo)
        {
            float damage = damageInfo.amount;

            // 护甲减免（物理伤害受护甲影响更大）
            if (damageInfo.type == DamageType.Physical)
            {
                damage *= (1f - armor / (armor + 100f));
            }

            // 类型抗性
            float resistance = damageInfo.type switch
            {
                DamageType.Physical => resistancePhysical,
                DamageType.Energy => resistanceEnergy,
                DamageType.Explosive => resistanceExplosive,
                DamageType.Pressure => resistancePressure,
                DamageType.Corrosive => resistanceCorrosive,
                _ => 0f
            };

            damage *= (1f - resistance / 100f);

            // 暴击加成
            if (damageInfo.isCritical)
            {
                damage *= 1.5f;
            }

            return damage;
        }

        /// <summary>
        /// 治疗
        /// </summary>
        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0) return;

            currentHealth += amount;
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            OnHeal?.Invoke();
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// 直接设置生命值
        /// </summary>
        public void SetHealth(float value)
        {
            currentHealth = Mathf.Clamp(value, 0, maxHealth);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 设置最大生命值
        /// </summary>
        public void SetMaxHealth(float value, bool healToFull = false)
        {
            maxHealth = value;
            if (healToFull)
            {
                currentHealth = maxHealth;
            }
            else
            {
                currentHealth = Mathf.Min(currentHealth, maxHealth);
            }
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        /// <summary>
        /// 死亡处理
        /// </summary>
        private void Die()
        {
            OnDeath?.Invoke();

            if (destroyOnDeath)
            {
                Destroy(gameObject, destroyDelay);
            }
        }

        /// <summary>
        /// 设置无敌状态
        /// </summary>
        public void SetInvincible(bool value)
        {
            invincible = value;
        }

        /// <summary>
        /// 临时无敌
        /// </summary>
        public void SetTemporaryInvincibility(float duration)
        {
            StartCoroutine(TemporaryInvincibilityCoroutine(duration));
        }

        private System.Collections.IEnumerator TemporaryInvincibilityCoroutine(float duration)
        {
            invincible = true;
            yield return new WaitForSeconds(duration);
            invincible = false;
        }

        /// <summary>
        /// 设置抗性
        /// </summary>
        public void SetResistance(DamageType type, float value)
        {
            switch (type)
            {
                case DamageType.Physical: resistancePhysical = value; break;
                case DamageType.Energy: resistanceEnergy = value; break;
                case DamageType.Explosive: resistanceExplosive = value; break;
                case DamageType.Pressure: resistancePressure = value; break;
                case DamageType.Corrosive: resistanceCorrosive = value; break;
            }
        }

        /// <summary>
        /// 重置生命值
        /// </summary>
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}
