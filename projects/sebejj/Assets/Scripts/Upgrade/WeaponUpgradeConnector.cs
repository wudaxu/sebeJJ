using UnityEngine;
using System.Collections.Generic;

namespace SebeJJ.Upgrade
{
    /// <summary>
    /// 武器升级连接器
    /// 连接升级系统与武器系统
    /// </summary>
    public class WeaponUpgradeConnector : MonoBehaviour
    {
        public static WeaponUpgradeConnector Instance { get; private set; }
        
        [Header("武器管理器")]
        public Combat.WeaponManager weaponManager;
        
        // 武器升级效果缓存
        private Dictionary<string, WeaponUpgradeEffects> weaponEffects = new Dictionary<string, WeaponUpgradeEffects>();
        
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
            SubscribeToEvents();
        }
        
        private void SubscribeToEvents()
        {
            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.OnWeaponUpgraded += OnWeaponUpgraded;
            }
        }
        
        /// <summary>
        /// 武器升级回调
        /// </summary>
        private void OnWeaponUpgraded(string weaponId, WeaponUpgradeType type, int newLevel)
        {
            ApplyWeaponUpgradeEffect(weaponId, type, newLevel);
        }
        
        /// <summary>
        /// 应用武器升级效果
        /// </summary>
        private void ApplyWeaponUpgradeEffect(string weaponId, WeaponUpgradeType type, int level)
        {
            var upgradeData = UpgradeManager.Instance?.GetWeaponUpgradeData(weaponId);
            if (upgradeData == null) return;
            
            var upgradeEntry = upgradeData.GetUpgrade(type);
            if (upgradeEntry == null) return;
            
            float multiplier = upgradeEntry.GetValueAtLevel(level);
            
            // 获取武器实例
            var weapon = FindWeapon(weaponId);
            if (weapon == null) return;
            
            // 应用升级效果
            switch (type)
            {
                case WeaponUpgradeType.Damage:
                    ApplyDamageUpgrade(weapon, multiplier);
                    break;
                    
                case WeaponUpgradeType.Range:
                    ApplyRangeUpgrade(weapon, multiplier);
                    break;
                    
                case WeaponUpgradeType.AttackSpeed:
                    ApplyAttackSpeedUpgrade(weapon, multiplier);
                    break;
                    
                case WeaponUpgradeType.EnergyEfficiency:
                    ApplyEnergyEfficiencyUpgrade(weapon, multiplier);
                    break;
            }
            
            Debug.Log($"[WeaponUpgradeConnector] 应用武器升级: {weaponId}.{type} = {multiplier:F2}x");
        }
        
        /// <summary>
        /// 查找武器
        /// </summary>
        private Combat.WeaponBase FindWeapon(string weaponId)
        {
            if (weaponManager == null) return null;
            
            // 遍历所有武器槽位查找匹配ID的武器
            for (int i = 0; i < weaponManager.WeaponCount; i++)
            {
                var weapon = weaponManager.GetWeapon(i);
                if (weapon != null && weapon.WeaponData != null)
                {
                    // 使用武器名称作为ID匹配
                    if (weapon.WeaponData.weaponName == weaponId || 
                        weapon.GetType().Name == weaponId)
                    {
                        return weapon;
                    }
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 应用伤害升级
        /// </summary>
        private void ApplyDamageUpgrade(Combat.WeaponBase weapon, float multiplier)
        {
            // 通过武器数据应用伤害加成
            var effects = GetOrCreateEffects(weapon);
            effects.damageMultiplier = multiplier;
            
            // 如果有武器数据，更新基础伤害
            if (weapon.WeaponData != null)
            {
                // 注意：这里应该使用运行时修改的方式，而不是直接修改ScriptableObject
                // 实际项目中可能需要额外的运行时武器状态管理
            }
        }
        
        /// <summary>
        /// 应用射程升级
        /// </summary>
        private void ApplyRangeUpgrade(Combat.WeaponBase weapon, float multiplier)
        {
            var effects = GetOrCreateEffects(weapon);
            effects.rangeMultiplier = multiplier;
        }
        
        /// <summary>
        /// 应用攻速升级
        /// </summary>
        private void ApplyAttackSpeedUpgrade(Combat.WeaponBase weapon, float multiplier)
        {
            var effects = GetOrCreateEffects(weapon);
            effects.attackSpeedMultiplier = multiplier;
        }
        
        /// <summary>
        /// 应用能量效率升级
        /// </summary>
        private void ApplyEnergyEfficiencyUpgrade(Combat.WeaponBase weapon, float multiplier)
        {
            var effects = GetOrCreateEffects(weapon);
            effects.energyEfficiencyMultiplier = multiplier;
        }
        
        /// <summary>
        /// 获取或创建武器效果
        /// </summary>
        private WeaponUpgradeEffects GetOrCreateEffects(Combat.WeaponBase weapon)
        {
            string weaponKey = weapon.GetInstanceID().ToString();
            
            if (!weaponEffects.ContainsKey(weaponKey))
            {
                weaponEffects[weaponKey] = new WeaponUpgradeEffects();
            }
            
            return weaponEffects[weaponKey];
        }
        
        /// <summary>
        /// 获取武器升级效果
        /// </summary>
        public WeaponUpgradeEffects GetWeaponEffects(Combat.WeaponBase weapon)
        {
            string weaponKey = weapon.GetInstanceID().ToString();
            return weaponEffects.TryGetValue(weaponKey, out var effects) ? effects : new WeaponUpgradeEffects();
        }
        
        /// <summary>
        /// 计算最终伤害
        /// </summary>
        public float CalculateFinalDamage(Combat.WeaponBase weapon, float baseDamage)
        {
            var effects = GetWeaponEffects(weapon);
            return baseDamage * effects.damageMultiplier;
        }
        
        /// <summary>
        /// 计算最终射程
        /// </summary>
        public float CalculateFinalRange(Combat.WeaponBase weapon, float baseRange)
        {
            var effects = GetWeaponEffects(weapon);
            return baseRange * effects.rangeMultiplier;
        }
        
        /// <summary>
        /// 计算最终冷却
        /// </summary>
        public float CalculateFinalCooldown(Combat.WeaponBase weapon, float baseCooldown)
        {
            var effects = GetWeaponEffects(weapon);
            return baseCooldown / effects.attackSpeedMultiplier;
        }
        
        /// <summary>
        /// 计算最终能量消耗
        /// </summary>
        public float CalculateFinalEnergyCost(Combat.WeaponBase weapon, float baseCost)
        {
            var effects = GetWeaponEffects(weapon);
            return baseCost / effects.energyEfficiencyMultiplier;
        }
        
        private void OnDestroy()
        {
            if (UpgradeManager.Instance != null)
            {
                UpgradeManager.Instance.OnWeaponUpgraded -= OnWeaponUpgraded;
            }
            
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
    
    /// <summary>
    /// 武器升级效果数据
    /// </summary>
    [System.Serializable]
    public class WeaponUpgradeEffects
    {
        public float damageMultiplier = 1f;
        public float rangeMultiplier = 1f;
        public float attackSpeedMultiplier = 1f;
        public float energyEfficiencyMultiplier = 1f;
        
        public void Reset()
        {
            damageMultiplier = 1f;
            rangeMultiplier = 1f;
            attackSpeedMultiplier = 1f;
            energyEfficiencyMultiplier = 1f;
        }
    }
}