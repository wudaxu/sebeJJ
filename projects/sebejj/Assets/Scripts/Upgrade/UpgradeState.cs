using System;
using System.Collections.Generic;

namespace SebeJJ.Upgrade
{
    /// <summary>
    /// 机甲升级状态
    /// </summary>
    [Serializable]
    public class MechaUpgradeState
    {
        private Dictionary<MechaUpgradeType, int> upgradeLevels = new Dictionary<MechaUpgradeType, int>();
        
        public void Initialize()
        {
            foreach (MechaUpgradeType type in Enum.GetValues(typeof(MechaUpgradeType)))
            {
                if (!upgradeLevels.ContainsKey(type))
                {
                    upgradeLevels[type] = 0;
                }
            }
        }
        
        public int GetLevel(MechaUpgradeType type)
        {
            return upgradeLevels.TryGetValue(type, out int level) ? level : 0;
        }
        
        public void SetLevel(MechaUpgradeType type, int level)
        {
            upgradeLevels[type] = level;
        }
        
        public void Reset()
        {
            upgradeLevels.Clear();
            Initialize();
        }
    }
    
    /// <summary>
    /// 武器升级状态
    /// </summary>
    [Serializable]
    public class WeaponUpgradeState
    {
        public string weaponId;
        private Dictionary<WeaponUpgradeType, int> upgradeLevels = new Dictionary<WeaponUpgradeType, int>();
        
        public void Initialize(string id)
        {
            weaponId = id;
            foreach (WeaponUpgradeType type in Enum.GetValues(typeof(WeaponUpgradeType)))
            {
                if (!upgradeLevels.ContainsKey(type))
                {
                    upgradeLevels[type] = 0;
                }
            }
        }
        
        public int GetLevel(WeaponUpgradeType type)
        {
            return upgradeLevels.TryGetValue(type, out int level) ? level : 0;
        }
        
        public void SetLevel(WeaponUpgradeType type, int level)
        {
            upgradeLevels[type] = level;
        }
        
        public void Reset()
        {
            upgradeLevels.Clear();
            Initialize(weaponId);
        }
    }
    
    /// <summary>
    /// 升级存档数据
    /// </summary>
    [Serializable]
    public class UpgradeSaveData
    {
        public MechaUpgradeState mechaState;
        public Dictionary<string, WeaponUpgradeState> weaponStates;
    }
}