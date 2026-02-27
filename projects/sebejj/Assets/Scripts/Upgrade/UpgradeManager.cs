using UnityEngine;
using System;
using System.Collections.Generic;

namespace SebeJJ.Upgrade
{
    /// <summary>
    /// 升级管理器 - 单例模式
    /// 负责管理所有升级相关的数据和进度
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        public static UpgradeManager Instance { get; private set; }
        
        [Header("升级配置")]
        public UpgradeDataConfig upgradeConfig;
        
        [Header("当前升级状态")]
        [SerializeField] private MechaUpgradeState mechaUpgradeState = new MechaUpgradeState();
        [SerializeField] private Dictionary<string, WeaponUpgradeState> weaponUpgradeStates = new Dictionary<string, WeaponUpgradeState>();
        
        // 事件
        public event Action<MechaUpgradeType, int> OnMechaUpgraded;
        public event Action<string, WeaponUpgradeType, int> OnWeaponUpgraded;
        public event Action<UpgradeData> OnUpgradeUnlocked;
        public event Action<UpgradeData, List<MaterialRequirement>> OnUpgradePreview;
        public event Action OnUpgradeFailed;
        
        // 属性
        public MechaUpgradeState MechaState => mechaUpgradeState;
        public IReadOnlyDictionary<string, WeaponUpgradeState> WeaponStates => weaponUpgradeStates;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeUpgradeStates();
        }
        
        /// <summary>
        /// 初始化升级状态
        /// </summary>
        private void InitializeUpgradeStates()
        {
            if (upgradeConfig == null)
            {
                Debug.LogError("[UpgradeManager] 升级配置未设置!");
                return;
            }
            
            // 初始化机甲升级状态
            mechaUpgradeState.Initialize();
            
            // 初始化武器升级状态
            foreach (var weaponUpgrade in upgradeConfig.weaponUpgrades)
            {
                if (!weaponUpgradeStates.ContainsKey(weaponUpgrade.weaponId))
                {
                    var state = new WeaponUpgradeState();
                    state.Initialize(weaponUpgrade.weaponId);
                    weaponUpgradeStates[weaponUpgrade.weaponId] = state;
                }
            }
            
            Debug.Log("[UpgradeManager] 升级状态初始化完成");
        }
        
        #region 机甲升级
        
        /// <summary>
        /// 获取机甲升级数据
        /// </summary>
        public MechaUpgradeData GetMechaUpgradeData(MechaUpgradeType type)
        {
            return upgradeConfig?.GetMechaUpgrade(type);
        }
        
        /// <summary>
        /// 获取机甲当前升级等级
        /// </summary>
        public int GetMechaUpgradeLevel(MechaUpgradeType type)
        {
            return mechaUpgradeState.GetLevel(type);
        }
        
        /// <summary>
        /// 检查机甲升级是否可用
        /// </summary>
        public bool CanUpgradeMecha(MechaUpgradeType type)
        {
            var upgradeData = GetMechaUpgradeData(type);
            if (upgradeData == null) return false;
            
            int currentLevel = GetMechaUpgradeLevel(type);
            if (currentLevel >= upgradeData.maxLevel) return false;
            
            // 检查材料需求
            var requirements = upgradeData.GetRequirementsForLevel(currentLevel + 1);
            return MaterialManager.Instance?.HasMaterials(requirements) ?? false;
        }
        
        /// <summary>
        /// 执行机甲升级
        /// </summary>
        public bool UpgradeMecha(MechaUpgradeType type)
        {
            if (!CanUpgradeMecha(type))
            {
                OnUpgradeFailed?.Invoke();
                return false;
            }
            
            var upgradeData = GetMechaUpgradeData(type);
            int currentLevel = GetMechaUpgradeLevel(type);
            int newLevel = currentLevel + 1;
            
            // 消耗材料
            var requirements = upgradeData.GetRequirementsForLevel(newLevel);
            MaterialManager.Instance?.ConsumeMaterials(requirements);
            
            // 升级
            mechaUpgradeState.SetLevel(type, newLevel);
            
            // 应用升级效果
            ApplyMechaUpgrade(type, newLevel);
            
            OnMechaUpgraded?.Invoke(type, newLevel);
            Debug.Log($"[UpgradeManager] 机甲升级成功: {type} -> 等级 {newLevel}");
            
            return true;
        }
        
        /// <summary>
        /// 应用机甲升级效果
        /// </summary>
        private void ApplyMechaUpgrade(MechaUpgradeType type, int level)
        {
            var upgradeData = GetMechaUpgradeData(type);
            if (upgradeData == null) return;
            
            float multiplier = upgradeData.GetMultiplierAtLevel(level);
            
            // 应用到实际游戏数值
            var mecha = Player.MechController.Instance;
            if (mecha == null) return;
            
            switch (type)
            {
                case MechaUpgradeType.Hull:
                    // 船体升级 - 增加耐久
                    // 实际应用通过属性系统
                    break;
                    
                case MechaUpgradeType.Energy:
                    // 能量升级 - 增加能量上限
                    Core.GameManager.Instance?.resourceManager?.SetMaxEnergyMultiplier(multiplier);
                    break;
                    
                case MechaUpgradeType.Speed:
                    // 速度升级 - 增加移动速度
                    mecha.moveSpeed = upgradeData.baseValue * multiplier;
                    break;
                    
                case MechaUpgradeType.Cargo:
                    // 货舱升级 - 增加容量
                    Core.GameManager.Instance?.resourceManager?.SetCargoCapacityMultiplier(multiplier);
                    break;
            }
        }
        
        /// <summary>
        /// 获取机甲升级预览
        /// </summary>
        public UpgradePreview GetMechaUpgradePreview(MechaUpgradeType type)
        {
            var upgradeData = GetMechaUpgradeData(type);
            if (upgradeData == null) return null;
            
            int currentLevel = GetMechaUpgradeLevel(type);
            int nextLevel = currentLevel + 1;
            
            if (nextLevel > upgradeData.maxLevel) return null;
            
            return new UpgradePreview
            {
                upgradeName = upgradeData.upgradeName,
                currentLevel = currentLevel,
                nextLevel = nextLevel,
                maxLevel = upgradeData.maxLevel,
                currentValue = upgradeData.GetValueAtLevel(currentLevel),
                nextValue = upgradeData.GetValueAtLevel(nextLevel),
                requirements = upgradeData.GetRequirementsForLevel(nextLevel),
                description = upgradeData.description,
                icon = upgradeData.icon
            };
        }
        
        #endregion
        
        #region 武器升级
        
        /// <summary>
        /// 获取武器升级数据
        /// </summary>
        public WeaponUpgradeData GetWeaponUpgradeData(string weaponId)
        {
            return upgradeConfig?.GetWeaponUpgrade(weaponId);
        }
        
        /// <summary>
        /// 获取武器当前升级等级
        /// </summary>
        public int GetWeaponUpgradeLevel(string weaponId, WeaponUpgradeType type)
        {
            if (weaponUpgradeStates.TryGetValue(weaponId, out var state))
            {
                return state.GetLevel(type);
            }
            return 0;
        }
        
        /// <summary>
        /// 检查武器升级是否可用
        /// </summary>
        public bool CanUpgradeWeapon(string weaponId, WeaponUpgradeType type)
        {
            var upgradeData = GetWeaponUpgradeData(weaponId);
            if (upgradeData == null) return false;
            
            var weaponUpgrade = upgradeData.GetUpgrade(type);
            if (weaponUpgrade == null) return false;
            
            int currentLevel = GetWeaponUpgradeLevel(weaponId, type);
            if (currentLevel >= weaponUpgrade.maxLevel) return false;
            
            // 检查材料需求
            var requirements = weaponUpgrade.GetRequirementsForLevel(currentLevel + 1);
            return MaterialManager.Instance?.HasMaterials(requirements) ?? false;
        }
        
        /// <summary>
        /// 执行武器升级
        /// </summary>
        public bool UpgradeWeapon(string weaponId, WeaponUpgradeType type)
        {
            if (!CanUpgradeWeapon(weaponId, type))
            {
                OnUpgradeFailed?.Invoke();
                return false;
            }
            
            var upgradeData = GetWeaponUpgradeData(weaponId);
            var weaponUpgrade = upgradeData.GetUpgrade(type);
            
            int currentLevel = GetWeaponUpgradeLevel(weaponId, type);
            int newLevel = currentLevel + 1;
            
            // 消耗材料
            var requirements = weaponUpgrade.GetRequirementsForLevel(newLevel);
            MaterialManager.Instance?.ConsumeMaterials(requirements);
            
            // 升级
            if (weaponUpgradeStates.TryGetValue(weaponId, out var state))
            {
                state.SetLevel(type, newLevel);
            }
            
            // 应用升级效果到实际武器
            ApplyWeaponUpgrade(weaponId, type, newLevel);
            
            OnWeaponUpgraded?.Invoke(weaponId, type, newLevel);
            Debug.Log($"[UpgradeManager] 武器升级成功: {weaponId}.{type} -> 等级 {newLevel}");
            
            return true;
        }
        
        /// <summary>
        /// 应用武器升级效果
        /// </summary>
        private void ApplyWeaponUpgrade(string weaponId, WeaponUpgradeType type, int level)
        {
            var weaponManager = Combat.WeaponManager.Instance;
            if (weaponManager == null) return;
            
            // 找到对应的武器并更新
            // 实际应用通过武器系统的事件监听
        }
        
        /// <summary>
        /// 获取武器升级预览
        /// </summary>
        public UpgradePreview GetWeaponUpgradePreview(string weaponId, WeaponUpgradeType type)
        {
            var upgradeData = GetWeaponUpgradeData(weaponId);
            if (upgradeData == null) return null;
            
            var weaponUpgrade = upgradeData.GetUpgrade(type);
            if (weaponUpgrade == null) return null;
            
            int currentLevel = GetWeaponUpgradeLevel(weaponId, type);
            int nextLevel = currentLevel + 1;
            
            if (nextLevel > weaponUpgrade.maxLevel) return null;
            
            return new UpgradePreview
            {
                upgradeName = weaponUpgrade.upgradeName,
                currentLevel = currentLevel,
                nextLevel = nextLevel,
                maxLevel = weaponUpgrade.maxLevel,
                currentValue = weaponUpgrade.GetValueAtLevel(currentLevel),
                nextValue = weaponUpgrade.GetValueAtLevel(nextLevel),
                requirements = weaponUpgrade.GetRequirementsForLevel(nextLevel),
                description = weaponUpgrade.description,
                icon = weaponUpgrade.icon
            };
        }
        
        #endregion
        
        #region 存档/读档
        
        /// <summary>
        /// 获取存档数据
        /// </summary>
        public UpgradeSaveData GetSaveData()
        {
            return new UpgradeSaveData
            {
                mechaState = mechaUpgradeState,
                weaponStates = new Dictionary<string, WeaponUpgradeState>(weaponUpgradeStates)
            };
        }
        
        /// <summary>
        /// 加载存档数据
        /// </summary>
        public void LoadSaveData(UpgradeSaveData saveData)
        {
            if (saveData == null) return;
            
            if (saveData.mechaState != null)
            {
                mechaUpgradeState = saveData.mechaState;
            }
            
            if (saveData.weaponStates != null)
            {
                weaponUpgradeStates = new Dictionary<string, WeaponUpgradeState>(saveData.weaponStates);
            }
            
            // 重新应用所有升级效果
            ReapplyAllUpgrades();
            
            Debug.Log("[UpgradeManager] 升级数据加载完成");
        }
        
        /// <summary>
        /// 重新应用所有升级效果
        /// </summary>
        private void ReapplyAllUpgrades()
        {
            // 重新应用机甲升级
            foreach (MechaUpgradeType type in Enum.GetValues(typeof(MechaUpgradeType)))
            {
                int level = GetMechaUpgradeLevel(type);
                if (level > 0)
                {
                    ApplyMechaUpgrade(type, level);
                }
            }
        }
        
        #endregion
        
        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
    
    /// <summary>
    /// 升级预览数据
    /// </summary>
    [Serializable]
    public class UpgradePreview
    {
        public string upgradeName;
        public int currentLevel;
        public int nextLevel;
        public int maxLevel;
        public float currentValue;
        public float nextValue;
        public List<MaterialRequirement> requirements;
        public string description;
        public Sprite icon;
        
        public float ValueDifference => nextValue - currentValue;
        public float ValueIncreasePercent => currentValue > 0 ? (ValueDifference / currentValue) * 100 : 0;
    }
}