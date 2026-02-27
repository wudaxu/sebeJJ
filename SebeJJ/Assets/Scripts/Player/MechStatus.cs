using UnityEngine;
using SebeJJ.Core;

namespace SebeJJ.Player
{
    /// <summary>
    /// 机甲状态管理 - 管理生命值、能量、氧气等状态
    /// </summary>
    public class MechStatus : MonoBehaviour, Combat.IDamageable
    {
        [Header("Base Stats")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float maxEnergy = 100f;
        [SerializeField] private float maxOxygen = 100f;

        [Header("Regeneration")]
        [SerializeField] private float energyRegenRate = 5f;
        [SerializeField] private float oxygenRegenRate = 0f;
        [SerializeField] private float healthRegenRate = 0f;

        [Header("Oxygen")]
        [SerializeField] private float oxygenDepletionRate = 1f;
        [SerializeField] private float oxygenDepletionDepth = 50f;
        [SerializeField] private float criticalOxygenLevel = 10f;

        [Header("Pressure")]
        [SerializeField] private float maxPressureResistance = 100f;
        [SerializeField] private float pressureDamageRate = 5f;

        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = false;
        [SerializeField] private bool godMode = false;

        // 当前值
        private float _currentHealth;
        private float _currentEnergy;
        private float _currentOxygen;

        // 状态
        private bool _isDead;
        private bool _isOxygenCritical;
        private MechController _controller;

        // 属性
        public float CurrentHealth => _currentHealth;
        public float CurrentEnergy => _currentEnergy;
        public float CurrentOxygen => _currentOxygen;
        public float MaxHealth => maxHealth;
        public float MaxEnergy => maxEnergy;
        public float MaxOxygen => maxOxygen;
        public bool IsDead => _isDead;
        public bool IsOxygenCritical => _isOxygenCritical;

        // 百分比
        public float HealthPercent => _currentHealth / maxHealth;
        public float EnergyPercent => _currentEnergy / maxEnergy;
        public float OxygenPercent => _currentOxygen / maxOxygen;

        #region Unity Lifecycle

        private void Awake()
        {
            _controller = GetComponent<MechController>();
        }

        private void Start()
        {
            InitializeStats();
        }

        private void Update()
        {
            if (_isDead) return;

            HandleRegeneration();
            HandleOxygen();
            HandlePressure();
            CheckCriticalStates();
        }

        #endregion

        #region Initialization

        private void InitializeStats()
        {
            _currentHealth = maxHealth;
            _currentEnergy = maxEnergy;
            _currentOxygen = maxOxygen;
            _isDead = false;
            _isOxygenCritical = false;

            // 触发初始事件
            GameEvents.OnHealthChanged?.Invoke(_currentHealth, maxHealth);
            GameEvents.OnEnergyChanged?.Invoke(_currentEnergy, maxEnergy);
            GameEvents.OnOxygenChanged?.Invoke(_currentOxygen, maxOxygen);
        }

        public void SetMaxStats(float health, float energy, float oxygen)
        {
            maxHealth = health;
            maxEnergy = energy;
            maxOxygen = oxygen;
            
            // 确保当前值不超过最大值
            _currentHealth = Mathf.Min(_currentHealth, maxHealth);
            _currentEnergy = Mathf.Min(_currentEnergy, maxEnergy);
            _currentOxygen = Mathf.Min(_currentOxygen, maxOxygen);
            
            // 触发更新事件
            GameEvents.OnHealthChanged?.Invoke(_currentHealth, maxHealth);
            GameEvents.OnEnergyChanged?.Invoke(_currentEnergy, maxEnergy);
            GameEvents.OnOxygenChanged?.Invoke(_currentOxygen, maxOxygen);
        }

        #endregion

        #region IDamageable Implementation

        public void TakeDamage(Combat.DamageInfo damageInfo)
        {
            if (_isDead || godMode) return;

            // 根据伤害类型计算实际伤害
            float finalDamage = CalculateDamage(damageInfo.amount, damageInfo.type);
            
            _currentHealth -= finalDamage;
            _currentHealth = Mathf.Max(0, _currentHealth);

            GameEvents.OnHealthChanged?.Invoke(_currentHealth, maxHealth);

            if (showDebugInfo)
            {
                Debug.Log($"[MechStatus] Took {finalDamage} {damageInfo.type} damage. Health: {_currentHealth}/{maxHealth}");
            }

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        public bool IsAlive => !_isDead;
        public float HealthPercent => _currentHealth / maxHealth;

        #endregion

        #region Legacy Damage Methods

        public void TakeDamage(float damage, Core.DamageType damageType)
        {
            Combat.DamageType newType = damageType switch
            {
                Core.DamageType.Physical => Combat.DamageType.Physical,
                Core.DamageType.Energy => Combat.DamageType.Energy,
                Core.DamageType.Explosive => Combat.DamageType.Explosive,
                Core.DamageType.Pressure => Combat.DamageType.Pressure,
                Core.DamageType.Corrosive => Combat.DamageType.Corrosive,
                _ => Combat.DamageType.Physical
            };

            TakeDamage(new Combat.DamageInfo(damage, newType, Vector2.zero, gameObject));
        }

        private float CalculateDamage(float baseDamage, Core.DamageType type)
        {
            float multiplier = 1f;
            
            switch (type)
            {
                case Core.DamageType.Physical:
                    // 护甲减免
                    multiplier = 1f - (GetComponent<MechStats>()?.Armor ?? 0) / 100f;
                    break;
                case Core.DamageType.Pressure:
                    // 压强抗性
                    multiplier = 1f - (GetComponent<MechStats>()?.PressureResistance ?? 0) / 100f;
                    break;
            }
            
            return baseDamage * Mathf.Max(0.1f, multiplier);
        }

        private float CalculateDamage(float baseDamage, Combat.DamageType type)
        {
            float multiplier = 1f;
            
            switch (type)
            {
                case Combat.DamageType.Physical:
                    multiplier = 1f - (GetComponent<MechStats>()?.Armor ?? 0) / 100f;
                    break;
                case Combat.DamageType.Pressure:
                    multiplier = 1f - (GetComponent<MechStats>()?.PressureResistance ?? 0) / 100f;
                    break;
            }
            
            return baseDamage * Mathf.Max(0.1f, multiplier);
        }

        public void Heal(float amount)
        {
            if (_isDead) return;

            _currentHealth += amount;
            _currentHealth = Mathf.Min(_currentHealth, maxHealth);

            GameEvents.OnHealthChanged?.Invoke(_currentHealth, maxHealth);
        }

        public void SetHealth(float health)
        {
            _currentHealth = Mathf.Clamp(health, 0, maxHealth);
            GameEvents.OnHealthChanged?.Invoke(_currentHealth, maxHealth);
            
            if (_currentHealth <= 0 && !_isDead)
            {
                Die();
            }
        }

        #endregion

        #region Energy

        public bool ConsumeEnergy(float amount)
        {
            if (_currentEnergy >= amount)
            {
                _currentEnergy -= amount;
                GameEvents.OnEnergyChanged?.Invoke(_currentEnergy, maxEnergy);
                return true;
            }
            return false;
        }

        public void RestoreEnergy(float amount)
        {
            _currentEnergy += amount;
            _currentEnergy = Mathf.Min(_currentEnergy, maxEnergy);
            GameEvents.OnEnergyChanged?.Invoke(_currentEnergy, maxEnergy);
        }

        public void SetEnergy(float energy)
        {
            _currentEnergy = Mathf.Clamp(energy, 0, maxEnergy);
            GameEvents.OnEnergyChanged?.Invoke(_currentEnergy, maxEnergy);
        }

        #endregion

        #region Oxygen

        public void ConsumeOxygen(float amount)
        {
            _currentOxygen -= amount;
            _currentOxygen = Mathf.Max(0, _currentOxygen);
            GameEvents.OnOxygenChanged?.Invoke(_currentOxygen, maxOxygen);
        }

        public void RestoreOxygen(float amount)
        {
            _currentOxygen += amount;
            _currentOxygen = Mathf.Min(_currentOxygen, maxOxygen);
            GameEvents.OnOxygenChanged?.Invoke(_currentOxygen, maxOxygen);
        }

        public void SetOxygen(float oxygen)
        {
            _currentOxygen = Mathf.Clamp(oxygen, 0, maxOxygen);
            GameEvents.OnOxygenChanged?.Invoke(_currentOxygen, maxOxygen);
        }

        private void HandleOxygen()
        {
            if (_controller == null) return;

            float depth = _controller.CurrentDepth;
            
            // 超过一定深度开始消耗氧气
            if (depth > oxygenDepletionDepth)
            {
                float depletion = oxygenDepletionRate * Time.deltaTime;
                ConsumeOxygen(depletion);
            }
            else
            {
                // 浅水区恢复氧气
                RestoreOxygen(oxygenRegenRate * Time.deltaTime);
            }

            // 氧气耗尽造成伤害
            if (_currentOxygen <= 0)
            {
                TakeDamage(5f * Time.deltaTime, Core.DamageType.Pressure);
            }
        }

        #endregion

        #region Regeneration

        private void HandleRegeneration()
        {
            // 能量自动恢复
            if (_currentEnergy < maxEnergy)
            {
                RestoreEnergy(energyRegenRate * Time.deltaTime);
            }

            // 生命值恢复（如果有）
            if (_currentHealth < maxHealth && healthRegenRate > 0)
            {
                Heal(healthRegenRate * Time.deltaTime);
            }
        }

        #endregion

        #region Pressure

        private void HandlePressure()
        {
            if (_controller == null) return;

            float depth = _controller.CurrentDepth;
            
            // 超过压强抗性承受伤害
            if (depth > maxPressureResistance)
            {
                float excessDepth = depth - maxPressureResistance;
                float damage = excessDepth * pressureDamageRate * Time.deltaTime;
                TakeDamage(damage, Core.DamageType.Pressure);
            }
        }

        #endregion

        #region Critical States

        private void CheckCriticalStates()
        {
            // 氧气临界警告
            if (_currentOxygen < criticalOxygenLevel && !_isOxygenCritical)
            {
                _isOxygenCritical = true;
                GameEvents.OnShowWarning?.Invoke("警告：氧气即将耗尽！");
            }
            else if (_currentOxygen >= criticalOxygenLevel && _isOxygenCritical)
            {
                _isOxygenCritical = false;
            }
        }

        #endregion

        #region Death

        private void Die()
        {
            if (_isDead) return;

            _isDead = true;
            Debug.Log("[MechStatus] Player died!");
            
            GameEvents.OnPlayerDeath?.Invoke();
        }

        public void Revive(float healthPercent = 0.5f)
        {
            _isDead = false;
            _currentHealth = maxHealth * healthPercent;
            _currentEnergy = maxEnergy * 0.5f;
            _currentOxygen = maxOxygen;

            GameEvents.OnPlayerRespawn?.Invoke();
            GameEvents.OnHealthChanged?.Invoke(_currentHealth, maxHealth);
            GameEvents.OnEnergyChanged?.Invoke(_currentEnergy, maxEnergy);
            GameEvents.OnOxygenChanged?.Invoke(_currentOxygen, maxOxygen);
        }

        #endregion

        #region Save/Load

        public void LoadFromSave(float health, float energy, float oxygen)
        {
            _currentHealth = health;
            _currentEnergy = energy;
            _currentOxygen = oxygen;
            _isDead = false;

            GameEvents.OnHealthChanged?.Invoke(_currentHealth, maxHealth);
            GameEvents.OnEnergyChanged?.Invoke(_currentEnergy, maxEnergy);
            GameEvents.OnOxygenChanged?.Invoke(_currentOxygen, maxOxygen);
        }

        #endregion
    }
}
