using UnityEngine;

namespace SebeJJ.Upgrade
{
    /// <summary>
    /// 机甲属性连接器
    /// 连接升级系统与机甲属性
    /// </summary>
    public class MechaAttributeConnector : MonoBehaviour
    {
        public static MechaAttributeConnector Instance { get; private set; }
        
        [Header("机甲控制器")]
        public Player.MechController mechController;
        
        [Header("属性倍率")]
        [SerializeField] private float hullMultiplier = 1f;
        [SerializeField] private float energyMultiplier = 1f;
        [SerializeField] private float speedMultiplier = 1f;
        [SerializeField] private float cargoMultiplier = 1f;
        
        // 基础属性值（用于计算）
        private float baseMaxHealth = 100f;
        private float baseMaxEnergy = 100f;
        private float baseMoveSpeed = 5f;
        private float baseCargoCapacity = 50f;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Start()
        {
            // 获取基础属性
            CacheBaseAttributes();
            
            SubscribeToEvents();
            
            // 应用当前所有升级
            ApplyAllUpgrades();
        }
        
        private void CacheBaseAttributes()
        {
            if (mechController != null)
            {
                baseMoveSpeed = mechController.moveSpeed;
            }
            
            // 从ResourceManager获取基础值
            var resourceManager = Core.GameManager.Instance?.resourceManager;
            if (resourceManager != null)
            {
                // 这里假设ResourceManager有获取基础值的方法
                // baseMaxEnergy = resourceManager.GetBaseMaxEnergy();
                // baseCargoCapacity = resourceManager.GetBaseCargoCapacity();
            }
        }
        
        private void SubscribeToEvents()
        {
            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.OnMechaUpgraded += OnMechaUpgraded;
            }
        }
        
        /// <summary>
        /// 机甲升级回调
        /// </summary>
        private void OnMechaUpgraded(MechaUpgradeType type, int newLevel)
        {
            ApplyMechaUpgrade(type, newLevel);
        }
        
        /// <summary>
        /// 应用机甲升级
        /// </summary>
        private void ApplyMechaUpgrade(MechaUpgradeType type, int level)
        {
            var upgradeData = UpgradeManager.Instance?.GetMechaUpgradeData(type);
            if (upgradeData == null) return;
            
            float multiplier = upgradeData.GetMultiplierAtLevel(level);
            
            switch (type)
            {
                case MechaUpgradeType.Hull:
                    hullMultiplier = multiplier;
                    ApplyHullUpgrade();
                    break;
                    
                case MechaUpgradeType.Energy:
                    energyMultiplier = multiplier;
                    ApplyEnergyUpgrade();
                    break;
                    
                case MechaUpgradeType.Speed:
                    speedMultiplier = multiplier;
                    ApplySpeedUpgrade();
                    break;
                    
                case MechaUpgradeType.Cargo:
                    cargoMultiplier = multiplier;
                    ApplyCargoUpgrade();
                    break;
            }
            
            Debug.Log($"[MechaAttributeConnector] 应用机甲升级: {type} = {multiplier:F2}x");
        }
        
        /// <summary>
        /// 应用船体升级
        /// </summary>
        private void ApplyHullUpgrade()
        {
            // 更新最大生命值
            float newMaxHealth = baseMaxHealth * hullMultiplier;
            
            // 应用到游戏系统
            // 这里应该调用HealthSystem或类似系统的接口
            Debug.Log($"[MechaAttributeConnector] 最大生命值更新: {newMaxHealth:F0}");
        }
        
        /// <summary>
        /// 应用能量升级
        /// </summary>
        private void ApplyEnergyUpgrade()
        {
            float newMaxEnergy = baseMaxEnergy * energyMultiplier;
            
            var resourceManager = Core.GameManager.Instance?.resourceManager;
            if (resourceManager != null)
            {
                // 设置新的能量上限
                // resourceManager.SetMaxEnergy(newMaxEnergy);
            }
            
            Debug.Log($"[MechaAttributeConnector] 最大能量更新: {newMaxEnergy:F0}");
        }
        
        /// <summary>
        /// 应用速度升级
        /// </summary>
        private void ApplySpeedUpgrade()
        {
            float newSpeed = baseMoveSpeed * speedMultiplier;
            
            if (mechController != null)
            {
                mechController.moveSpeed = newSpeed;
            }
            
            Debug.Log($"[MechaAttributeConnector] 移动速度更新: {newSpeed:F2}");
        }
        
        /// <summary>
        /// 应用货舱升级
        /// </summary>
        private void ApplyCargoUpgrade()
        {
            float newCapacity = baseCargoCapacity * cargoMultiplier;
            
            var resourceManager = Core.GameManager.Instance?.resourceManager;
            if (resourceManager != null)
            {
                // 设置新的货舱容量
                // resourceManager.SetCargoCapacity(newCapacity);
            }
            
            Debug.Log($"[MechaAttributeConnector] 货舱容量更新: {newCapacity:F0}");
        }
        
        /// <summary>
        /// 应用所有升级（用于初始化或读档）
        /// BUG-004修复: 修复属性叠加计算
        /// BUG-005修复: 添加属性刷新事件
        /// </summary>
        public void ApplyAllUpgrades()
        {
            if (UpgradeManager.Instance == null) return;
            
            // BUG-004修复: 先重置所有倍率
            hullMultiplier = 1f;
            energyMultiplier = 1f;
            speedMultiplier = 1f;
            cargoMultiplier = 1f;
            
            // BUG-004修复: 重新计算所有装备加成
            RecalculateEquipmentBonuses();
            
            // 应用升级加成
            foreach (MechaUpgradeType type in System.Enum.GetValues(typeof(MechaUpgradeType)))
            {
                int level = UpgradeManager.Instance.GetMechaUpgradeLevel(type);
                if (level > 0)
                {
                    ApplyMechaUpgrade(type, level);
                }
            }
            
            // BUG-005修复: 触发属性刷新事件
            OnAttributesRecalculated?.Invoke();
        }
        
        // BUG-005修复: 属性刷新事件
        public event System.Action OnAttributesRecalculated;
        
        /// <summary>
        /// BUG-004修复: 重新计算装备加成
        /// </summary>
        private void RecalculateEquipmentBonuses()
        {
            // 获取当前装备并重新计算加成
            var equipmentManager = Core.GameManager.Instance?.equipmentManager;
            if (equipmentManager != null)
            {
                var equippedItems = equipmentManager.GetEquippedItems();
                foreach (var equipment in equippedItems)
                {
                    ApplyEquipmentBonus(equipment);
                }
            }
        }
        
        /// <summary>
        /// BUG-004修复: 应用单个装备加成
        /// </summary>
        private void ApplyEquipmentBonus(EquipmentData equipment)
        {
            if (equipment == null) return;
            
            switch (equipment.bonusType)
            {
                case BonusType.Additive:
                    hullMultiplier += equipment.hullBonus;
                    energyMultiplier += equipment.energyBonus;
                    speedMultiplier += equipment.speedBonus;
                    cargoMultiplier += equipment.cargoBonus;
                    break;
                case BonusType.Multiplicative:
                    hullMultiplier *= equipment.hullBonus;
                    energyMultiplier *= equipment.energyBonus;
                    speedMultiplier *= equipment.speedBonus;
                    cargoMultiplier *= equipment.cargoBonus;
                    break;
            }
        }
        
        /// <summary>
        /// BUG-017修复: 与升级系统同步数据
        /// </summary>
        public void SyncWithUpgradeSystem()
        {
            if (UpgradeManager.Instance == null) return;
            
            foreach (MechaUpgradeType type in System.Enum.GetValues(typeof(MechaUpgradeType)))
            {
                int level = UpgradeManager.Instance.GetMechaUpgradeLevel(type);
                ApplyMechaUpgrade(type, level);
            }
            
            // 触发同步完成事件
            OnDataSynced?.Invoke();
        }
        
        // BUG-017修复: 数据同步事件
        public event System.Action OnDataSynced;
        
        /// <summary>
        /// 获取属性倍率
        /// </summary>
        public float GetMultiplier(MechaUpgradeType type)
        {
            switch (type)
            {
                case MechaUpgradeType.Hull: return hullMultiplier;
                case MechaUpgradeType.Energy: return energyMultiplier;
                case MechaUpgradeType.Speed: return speedMultiplier;
                case MechaUpgradeType.Cargo: return cargoMultiplier;
                default: return 1f;
            }
        }
        
        /// <summary>
        /// 获取当前属性值
        /// </summary>
        public float GetCurrentValue(MechaUpgradeType type)
        {
            switch (type)
            {
                case MechaUpgradeType.Hull: return baseMaxHealth * hullMultiplier;
                case MechaUpgradeType.Energy: return baseMaxEnergy * energyMultiplier;
                case MechaUpgradeType.Speed: return baseMoveSpeed * speedMultiplier;
                case MechaUpgradeType.Cargo: return baseCargoCapacity * cargoMultiplier;
                default: return 0f;
            }
        }
        
        /// <summary>
        /// 设置基础属性值
        /// </summary>
        public void SetBaseValue(MechaUpgradeType type, float value)
        {
            switch (type)
            {
                case MechaUpgradeType.Hull:
                    baseMaxHealth = value;
                    ApplyHullUpgrade();
                    break;
                case MechaUpgradeType.Energy:
                    baseMaxEnergy = value;
                    ApplyEnergyUpgrade();
                    break;
                case MechaUpgradeType.Speed:
                    baseMoveSpeed = value;
                    ApplySpeedUpgrade();
                    break;
                case MechaUpgradeType.Cargo:
                    baseCargoCapacity = value;
                    ApplyCargoUpgrade();
                    break;
            }
        }
        
        private void OnDestroy()
        {
            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.OnMechaUpgraded -= OnMechaUpgraded;
            }
            
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
    
    // BUG-004修复: 装备数据类
    public class EquipmentData
    {
        public string equipmentId;
        public string equipmentName;
        public BonusType bonusType;
        public float hullBonus = 0f;
        public float energyBonus = 0f;
        public float speedBonus = 0f;
        public float cargoBonus = 0f;
    }
    
    public enum BonusType
    {
        Additive,       // 加法叠加
        Multiplicative  // 乘法叠加
    }
}