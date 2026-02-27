using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace SebeJJ.Core
{
    /// <summary>
    /// 配置管理器 - 加载和管理游戏配置 (CS-003: 配置外置化)
    /// </summary>
    public static class ConfigManager
    {
        private static readonly Dictionary<string, object> configs = new Dictionary<string, object>();
        private static readonly Dictionary<string, DateTime> configTimestamps = new Dictionary<string, DateTime>();
        
        /// <summary>
        /// 加载配置文件
        /// </summary>
        public static T Load<T>(string configName) where T : class
        {
            if (configs.TryGetValue(configName, out object config))
            {
                return config as T;
            }
            
            try
            {
                var textAsset = Resources.Load<TextAsset>($"Configs/{configName}");
                if (textAsset == null)
                {
                    Debug.LogError($"[ConfigManager] 配置文件不存在: {configName}");
                    return null;
                }
                
                var result = JsonUtility.FromJson<T>(textAsset.text);
                if (result != null)
                {
                    configs[configName] = result;
                    configTimestamps[configName] = DateTime.Now;
                    Debug.Log($"[ConfigManager] 加载配置: {configName}");
                }
                return result;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ConfigManager] 加载配置失败 {configName}: {e.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// 重新加载配置
        /// </summary>
        public static void Reload<T>(string configName) where T : class
        {
            configs.Remove(configName);
            Load<T>(configName);
        }
        
        /// <summary>
        /// 获取配置（如果不存在则加载）
        /// </summary>
        public static T Get<T>(string configName) where T : class
        {
            return Load<T>(configName);
        }
        
        /// <summary>
        /// 检查配置是否已加载
        /// </summary>
        public static bool IsLoaded(string configName)
        {
            return configs.ContainsKey(configName);
        }
        
        /// <summary>
        /// 获取配置加载时间
        /// </summary>
        public static DateTime? GetLoadTime(string configName)
        {
            if (configTimestamps.TryGetValue(configName, out DateTime time))
            {
                return time;
            }
            return null;
        }
        
        /// <summary>
        /// 清空所有配置缓存
        /// </summary>
        public static void ClearCache()
        {
            configs.Clear();
            configTimestamps.Clear();
            Debug.Log("[ConfigManager] 清空配置缓存");
        }
    }
    
    // 配置数据结构
    [Serializable]
    public class PlayerConfigData
    {
        public string version;
        public PlayerStats playerStats;
        public ConsumptionRates consumptionRates;
        public DepthModifier[] depthModifiers;
        public UpgradeCosts upgradeCosts;
    }
    
    [Serializable]
    public class PlayerStats
    {
        public float baseMoveSpeed;
        public float baseOxygenCapacity;
        public float baseEnergyCapacity;
        public float baseInventoryWeight;
        public float baseScanRange;
        public float baseHealth;
    }
    
    [Serializable]
    public class ConsumptionRates
    {
        public float oxygenDepletionRate;
        public float energyRechargeRate;
        public float energyCostPerMove;
        public float energyCostPerBoost;
        public float energyCostPerScan;
        public float energyCostPerCollect;
    }
    
    [Serializable]
    public class DepthModifier
    {
        public float depth;
        public float oxygenMultiplier;
        public float pressure;
        public int dangerLevel;
    }
    
    [Serializable]
    public class UpgradeCosts
    {
        public UpgradeCost oxygenCapacity;
        public UpgradeCost energyCapacity;
        public UpgradeCost moveSpeed;
        public UpgradeCost scanRange;
        public UpgradeCost inventoryCapacity;
    }
    
    [Serializable]
    public class UpgradeCost
    {
        public int baseCost;
        public float costMultiplier;
        public float valuePerLevel;
    }
    
    [Serializable]
    public class GameSettingsData
    {
        public string version;
        public GameSettings gameSettings;
        public SaveSettings saveSettings;
        public UISettings uiSettings;
        public PerformanceSettings performanceSettings;
    }
    
    [Serializable]
    public class GameSettings
    {
        public int targetFrameRate;
        public int vSyncCount;
        public float masterVolume;
        public float musicVolume;
        public float sfxVolume;
        public float ambientVolume;
    }
    
    [Serializable]
    public class SaveSettings
    {
        public float autoSaveInterval;
        public int maxSaveSlots;
        public int maxBackupCount;
        public bool enableAutoSave;
        public bool autoSaveOnExit;
    }
    
    [Serializable]
    public class UISettings
    {
        public int baseSortingOrder;
        public int panelSortingOrder;
        public int popupSortingOrder;
        public int notificationSortingOrder;
        public float notificationDuration;
        public float tooltipDelay;
    }
    
    [Serializable]
    public class PerformanceSettings
    {
        public int objectPoolInitialSize;
        public int objectPoolMaxSize;
        public string particleQuality;
        public string shadowQuality;
        public string textureQuality;
    }
}
