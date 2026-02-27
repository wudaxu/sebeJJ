using System;
using System.IO;
using UnityEngine;
using SebeJJ.Utils;

namespace SebeJJ.Core
{
    /// <summary>
    /// 玩家数据类 - 可序列化的游戏数据
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        // 基础信息
        public string playerName = "Player";
        public int playerLevel = 1;
        public int experience = 0;
        
        // 游戏进度
        public int currentDepth = 0;
        public int maxDepthReached = 0;
        public int currentArea = 0;
        
        // 资源
        public int currency = 0;
        public int[] resourceCounts = new int[Enum.GetValues(typeof(ResourceType)).Length];
        
        // 机甲状态
        public float currentHealth = 100f;
        public float currentEnergy = 100f;
        public float currentOxygen = 100f;
        
        // 机甲配置
        public string equippedMechId = "default_mech";
        public string[] equippedWeaponIds = new string[2];
        public string[] equippedModuleIds = new string[4];
        
        // 解锁内容
        public bool[] unlockedAreas;
        public bool[] unlockedMechs;
        public bool[] completedMissions;
        
        // 统计
        public int totalPlayTime = 0;  // 秒
        public int totalResourcesCollected = 0;
        public int totalEnemiesDefeated = 0;
        public int totalMissionsCompleted = 0;
        public int deathCount = 0;
        
        // 时间戳
        public string lastSaveTime;
        public string gameStartTime;

        public PlayerData()
        {
            equippedWeaponIds[0] = "mining_laser";
            lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            gameStartTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    /// <summary>
    /// 存档管理器 - 处理游戏数据的保存和加载
    /// </summary>
    public class SaveManager : Singleton<SaveManager>
    {
        [Header("Save Settings")]
        [SerializeField] private string saveFileName = "savegame.json";
        [SerializeField] private bool useEncryption = false;
        [SerializeField] private bool autoSaveOnExit = true;

        [Header("Debug")]
        [SerializeField] private bool showDebugLogs = true;

        public PlayerData CurrentData { get; private set; }
        public bool HasData => CurrentData != null;

        private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);
        private string BackupPath => Path.Combine(Application.persistentDataPath, $"{saveFileName}.backup");

        #region Unity Lifecycle

        protected override void OnAwake()
        {
            base.OnAwake();
            Initialize();
        }

        #endregion

        #region Initialization

        private void Initialize()
        {
            Log($"Save path: {SavePath}");
            
            // 尝试加载现有存档
            if (HasSaveData())
            {
                Log("Existing save data found.");
            }
        }

        #endregion

        #region Save Operations

        public void CreateNewSave()
        {
            CurrentData = new PlayerData();
            Log("New save created.");
        }

        public void SaveGame()
        {
            if (CurrentData == null)
            {
                LogWarning("No player data to save.");
                return;
            }

            try
            {
                // 更新存档时间
                CurrentData.lastSaveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                // 创建备份
                CreateBackup();
                
                // 序列化数据
                string json = JsonUtility.ToJson(CurrentData, true);
                
                // 加密（如果需要）
                if (useEncryption)
                {
                    json = EncryptString(json);
                }
                
                // 写入文件
                File.WriteAllText(SavePath, json);
                
                Log("Game saved successfully.");
            }
            catch (Exception e)
            {
                LogError($"Failed to save game: {e.Message}");
            }
        }

        public void AutoSave()
        {
            Log("Auto-saving...");
            SaveGame();
        }

        public void LoadGame()
        {
            if (!HasSaveData())
            {
                LogWarning("No save data found. Creating new save.");
                CreateNewSave();
                return;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                
                // 解密（如果需要）
                if (useEncryption)
                {
                    json = DecryptString(json);
                }
                
                CurrentData = JsonUtility.FromJson<PlayerData>(json);
                
                Log("Game loaded successfully.");
            }
            catch (Exception e)
            {
                LogError($"Failed to load game: {e.Message}");
                
                // 尝试从备份恢复
                TryRestoreFromBackup();
            }
        }

        public bool HasSaveData()
        {
            return File.Exists(SavePath);
        }

        public void DeleteSave()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    File.Delete(SavePath);
                }
                
                if (File.Exists(BackupPath))
                {
                    File.Delete(BackupPath);
                }
                
                CurrentData = null;
                Log("Save data deleted.");
            }
            catch (Exception e)
            {
                LogError($"Failed to delete save: {e.Message}");
            }
        }

        #endregion

        #region Backup

        private void CreateBackup()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    File.Copy(SavePath, BackupPath, true);
                }
            }
            catch (Exception e)
            {
                LogWarning($"Failed to create backup: {e.Message}");
            }
        }

        private void TryRestoreFromBackup()
        {
            try
            {
                if (File.Exists(BackupPath))
                {
                    string json = File.ReadAllText(BackupPath);
                    
                    if (useEncryption)
                    {
                        json = DecryptString(json);
                    }
                    
                    CurrentData = JsonUtility.FromJson<PlayerData>(json);
                    Log("Game restored from backup.");
                }
                else
                {
                    CreateNewSave();
                }
            }
            catch (Exception e)
            {
                LogError($"Failed to restore from backup: {e.Message}");
                CreateNewSave();
            }
        }

        #endregion

        #region Data Operations

        public void UpdatePlayerStats(float health, float energy, float oxygen)
        {
            if (CurrentData == null) return;
            
            CurrentData.currentHealth = health;
            CurrentData.currentEnergy = energy;
            CurrentData.currentOxygen = oxygen;
        }

        public void AddCurrency(int amount)
        {
            if (CurrentData == null) return;
            
            CurrentData.currency += amount;
            GameEvents.OnCurrencyChanged?.Invoke(CurrentData.currency);
        }

        public void AddResource(ResourceType type, int amount)
        {
            if (CurrentData == null || type == ResourceType.None) return;
            
            int index = (int)type;
            if (index >= 0 && index < CurrentData.resourceCounts.Length)
            {
                CurrentData.resourceCounts[index] += amount;
                CurrentData.totalResourcesCollected += amount;
                GameEvents.OnResourceCollected?.Invoke(type, amount);
            }
        }

        public int GetResourceCount(ResourceType type)
        {
            if (CurrentData == null || type == ResourceType.None) return 0;
            
            int index = (int)type;
            if (index >= 0 && index < CurrentData.resourceCounts.Length)
            {
                return CurrentData.resourceCounts[index];
            }
            return 0;
        }

        public void UpdateDepth(int depth)
        {
            if (CurrentData == null) return;
            
            CurrentData.currentDepth = depth;
            if (depth > CurrentData.maxDepthReached)
            {
                CurrentData.maxDepthReached = depth;
            }
        }

        #endregion

        #region Encryption (Simple XOR)

        private string EncryptString(string text)
        {
            // 简单的XOR加密，实际项目应使用更安全的加密方式
            string key = Application.identifier;
            char[] result = new char[text.Length];
            
            for (int i = 0; i < text.Length; i++)
            {
                result[i] = (char)(text[i] ^ key[i % key.Length]);
            }
            
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(new string(result)));
        }

        private string DecryptString(string encryptedText)
        {
            string key = Application.identifier;
            string text = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encryptedText));
            char[] result = new char[text.Length];
            
            for (int i = 0; i < text.Length; i++)
            {
                result[i] = (char)(text[i] ^ key[i % key.Length]);
            }
            
            return new string(result);
        }

        #endregion

        #region Debug

        private void Log(string message)
        {
            if (showDebugLogs)
                Debug.Log($"[SaveManager] {message}");
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[SaveManager] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[SaveManager] {message}");
        }

        #endregion
    }
}
