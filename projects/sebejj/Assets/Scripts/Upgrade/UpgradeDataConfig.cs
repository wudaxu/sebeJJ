using UnityEngine;
using System.Collections.Generic;

namespace SebeJJ.Upgrade
{
    /// <summary>
    /// 升级数据配置 ScriptableObject
    /// 用于在Unity编辑器中配置所有升级数据
    /// </summary>
    [CreateAssetMenu(fileName = "UpgradeDataConfig", menuName = "SebeJJ/Upgrade Data Config")]
    public class UpgradeDataConfig : ScriptableObject
    {
        [Header("机甲升级配置")]
        public List<MechaUpgradeData> mechaUpgrades = new List<MechaUpgradeData>();
        
        [Header("武器升级配置")]
        public List<WeaponUpgradeData> weaponUpgrades = new List<WeaponUpgradeData>();
        
        [Header("默认配置")]
        public bool useDefaultConfig = true;
        
        private void OnEnable()
        {
            if (useDefaultConfig && mechaUpgrades.Count == 0)
            {
                InitializeDefaultConfig();
            }
        }
        
        /// <summary>
        /// 获取机甲升级数据
        /// </summary>
        public MechaUpgradeData GetMechaUpgrade(MechaUpgradeType type)
        {
            return mechaUpgrades.Find(u => u.upgradeType == type);
        }
        
        /// <summary>
        /// 获取武器升级数据
        /// </summary>
        public WeaponUpgradeData GetWeaponUpgrade(string weaponId)
        {
            return weaponUpgrades.Find(u => u.weaponId == weaponId);
        }
        
        /// <summary>
        /// 初始化默认配置
        /// </summary>
        private void InitializeDefaultConfig()
        {
            // 船体升级配置
            var hullUpgrade = new MechaUpgradeData
            {
                upgradeType = MechaUpgradeType.Hull,
                upgradeName = "船体强化",
                description = "增加机甲耐久上限，提升生存能力",
                maxLevel = 10,
                baseValue = 100f,
                valueIncrement = 20f,
                valueMultiplier = 1.1f,
                calculationMode = UpgradeCalculationMode.AdditiveThenMultiplicative,
                levelRequirements = new List<LevelRequirement>()
            };
            GenerateDefaultRequirements(hullUpgrade.levelRequirements, hullUpgrade.maxLevel);
            mechaUpgrades.Add(hullUpgrade);
            
            // 能量升级配置
            var energyUpgrade = new MechaUpgradeData
            {
                upgradeType = MechaUpgradeType.Energy,
                upgradeName = "能量核心",
                description = "增加能量上限，支持更长时间的探索和战斗",
                maxLevel = 10,
                baseValue = 100f,
                valueIncrement = 15f,
                valueMultiplier = 1.08f,
                calculationMode = UpgradeCalculationMode.AdditiveThenMultiplicative,
                levelRequirements = new List<LevelRequirement>()
            };
            GenerateDefaultRequirements(energyUpgrade.levelRequirements, energyUpgrade.maxLevel);
            mechaUpgrades.Add(energyUpgrade);
            
            // 速度升级配置
            var speedUpgrade = new MechaUpgradeData
            {
                upgradeType = MechaUpgradeType.Speed,
                upgradeName = "推进系统",
                description = "提升移动速度，增强机动性",
                maxLevel = 10,
                baseValue = 5f,
                valueIncrement = 0.5f,
                valueMultiplier = 1.05f,
                calculationMode = UpgradeCalculationMode.Additive,
                levelRequirements = new List<LevelRequirement>()
            };
            GenerateDefaultRequirements(speedUpgrade.levelRequirements, speedUpgrade.maxLevel);
            mechaUpgrades.Add(speedUpgrade);
            
            // 货舱升级配置
            var cargoUpgrade = new MechaUpgradeData
            {
                upgradeType = MechaUpgradeType.Cargo,
                upgradeName = "货舱扩容",
                description = "增加资源携带容量，提高采集效率",
                maxLevel = 8,
                baseValue = 50f,
                valueIncrement = 10f,
                valueMultiplier = 1.1f,
                calculationMode = UpgradeCalculationMode.AdditiveThenMultiplicative,
                levelRequirements = new List<LevelRequirement>()
            };
            GenerateDefaultRequirements(cargoUpgrade.levelRequirements, cargoUpgrade.maxLevel);
            mechaUpgrades.Add(cargoUpgrade);
            
            Debug.Log("[UpgradeDataConfig] 已初始化默认配置");
        }
        
        /// <summary>
        /// 生成默认升级需求
        /// </summary>
        private void GenerateDefaultRequirements(List<LevelRequirement> requirements, int maxLevel)
        {
            for (int i = 1; i <= maxLevel; i++)
            {
                var req = new LevelRequirement
                {
                    level = i,
                    materials = new List<MaterialRequirement>()
                };
                
                // 基础材料需求
                req.materials.Add(new MaterialRequirement("scrap_metal", i * 5));
                
                // 高级材料需求（5级以后）
                if (i >= 5)
                {
                    req.materials.Add(new MaterialRequirement("energy_crystal", (i - 4) * 2));
                }
                
                // 顶级材料需求（8级以后）
                if (i >= 8)
                {
                    req.materials.Add(new MaterialRequirement("ancient_core", i - 7));
                }
                
                requirements.Add(req);
            }
        }
        
        /// <summary>
        /// 验证配置
        /// </summary>
        public void ValidateConfig()
        {
            // 检查机甲升级配置
            foreach (var upgrade in mechaUpgrades)
            {
                if (upgrade.levelRequirements.Count < upgrade.maxLevel)
                {
                    Debug.LogWarning($"[UpgradeDataConfig] {upgrade.upgradeName} 的需求配置不完整!");
                }
            }
            
            // 检查武器升级配置
            foreach (var weapon in weaponUpgrades)
            {
                foreach (var upgrade in weapon.upgrades)
                {
                    if (upgrade.levelRequirements.Count < upgrade.maxLevel)
                    {
                        Debug.LogWarning($"[UpgradeDataConfig] {weapon.weaponName}.{upgrade.upgradeName} 的需求配置不完整!");
                    }
                }
            }
        }
    }
}