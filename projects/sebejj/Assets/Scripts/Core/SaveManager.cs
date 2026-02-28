using UnityEngine;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SebeJJ.Core
{
    /// <summary>
    /// 存档管理器 - 负责游戏数据的保存和加载 (SV-001~006 完整实现)
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }
        
        [Header("存档设置")]
        public string saveFolderName = "Saves";
        public string fileExtension = ".sav";
        public string backupExtension = ".bak";
        public int maxSaveSlots = 3;
        public int maxBackupCount = 3;
        
        [Header("自动存档")]
        public bool enableAutoSave = true;
        public float autoSaveInterval = 300f; // 5分钟
        public bool autoSaveOnExit = true;
        
        // 存档版本 (用于版本控制)
        public const int SAVE_VERSION = 1;
        
        private string SaveDirectory => Path.Combine(Application.persistentDataPath, saveFolderName);
        private float lastAutoSaveTime;
        private bool isSaving;
        
        // 存档事件
        public event Action OnGameSaved;
        public event Action OnGameLoaded;
        public event Action<string> OnSaveError;
        public event Action<string> OnLoadError;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            EnsureSaveDirectoryExists();
        }
        
        private void Update()
        {
            // 自动存档
            if (enableAutoSave && GameManager.Instance?.CurrentState == GameState.Playing)
            {
                if (Time.time >= lastAutoSaveTime + autoSaveInterval)
                {
                    AutoSave();
                }
            }
        }
        
        private void OnApplicationQuit()
        {
            if (autoSaveOnExit && GameManager.Instance?.CurrentState == GameState.Playing)
            {
                AutoSave();
            }
        }
        
        private void EnsureSaveDirectoryExists()
        {
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);
                Debug.Log($"[SaveManager] 创建存档目录: {SaveDirectory}");
            }
        }
        
        #region 核心存档功能
        
        /// <summary>
        /// 保存游戏 (SV-002: 原子性写入 + 数据校验)
        /// </summary>
        public bool SaveGame(string slotName, bool createBackup = true)
        {
            if (isSaving)
            {
                Debug.LogWarning("[SaveManager] 正在保存中，请稍后再试");
                return false;
            }
            
            isSaving = true;
            
            try
            {
                var saveData = CreateSaveData();
                string json = JsonUtility.ToJson(saveData, true);
                
                // SV-002: 计算校验和
                string checksum = CalculateChecksum(json);
                var wrapper = new SaveDataWrapper
                {
                    version = SAVE_VERSION,
                    checksum = checksum,
                    data = json
                };
                
                string finalJson = JsonUtility.ToJson(wrapper, true);
                string filePath = GetSaveFilePath(slotName);
                string tempPath = filePath + ".tmp";
                
                // SV-002: 原子性写入 (先写临时文件)
                File.WriteAllText(tempPath, finalJson);
                
                // SV-004: 创建备份
                if (createBackup && File.Exists(filePath))
                {
                    CreateBackup(slotName);
                }
                
                // 重命名为正式文件
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                File.Move(tempPath, filePath);
                
                Debug.Log($"[SaveManager] 游戏已保存: {slotName} (版本: {SAVE_VERSION})");
                OnGameSaved?.Invoke();
                
                lastAutoSaveTime = Time.time;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] 保存失败: {e.Message}");
                OnSaveError?.Invoke(e.Message);
                return false;
            }
            finally
            {
                isSaving = false;
            }
        }
        
        /// <summary>
        /// 加载游戏 (SV-001: 版本兼容 + SV-002: 数据校验)
        /// </summary>
        public bool LoadGame(string slotName)
        {
            try
            {
                string filePath = GetSaveFilePath(slotName);
                
                if (!File.Exists(filePath))
                {
                    Debug.LogWarning($"[SaveManager] 存档不存在: {slotName}");
                    return false;
                }
                
                string finalJson = File.ReadAllText(filePath);
                var wrapper = JsonUtility.FromJson<SaveDataWrapper>(finalJson);
                
                // SV-002: 验证校验和
                if (!string.IsNullOrEmpty(wrapper.checksum))
                {
                    string calculatedChecksum = CalculateChecksum(wrapper.data);
                    if (calculatedChecksum != wrapper.checksum)
                    {
                        Debug.LogError("[SaveManager] 存档校验失败，数据可能已损坏");
                        
                        // 尝试从备份恢复
                        if (TryRestoreFromBackup(slotName))
                        {
                            return LoadGame(slotName); // 重新加载
                        }
                        
                        OnLoadError?.Invoke("存档数据已损坏");
                        return false;
                    }
                }
                
                // SV-001: 版本检查
                if (wrapper.version > SAVE_VERSION)
                {
                    Debug.LogError($"[SaveManager] 存档版本({wrapper.version})高于当前版本({SAVE_VERSION})");
                    OnLoadError?.Invoke("存档版本不兼容");
                    return false;
                }
                
                // SV-001: 版本迁移
                var saveData = JsonUtility.FromJson<GameSaveData>(wrapper.data);
                if (wrapper.version < SAVE_VERSION)
                {
                    saveData = MigrateSaveData(saveData, wrapper.version, SAVE_VERSION);
                }
                
                // 加载数据到游戏
                ApplySaveData(saveData);
                
                Debug.Log($"[SaveManager] 游戏已加载: {slotName} (保存时间: {saveData.saveTime})");
                OnGameLoaded?.Invoke();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] 加载失败: {e.Message}");
                OnLoadError?.Invoke(e.Message);
                return false;
            }
        }
        
        /// <summary>
        /// 删除存档
        /// </summary>
        public bool DeleteSave(string slotName)
        {
            try
            {
                string filePath = GetSaveFilePath(slotName);
                
                if (File.Exists(filePath))
                {
                    // 删除存档文件
                    File.Delete(filePath);
                    
                    // 删除备份文件
                    DeleteBackups(slotName);
                    
                    Debug.Log($"[SaveManager] 存档已删除: {slotName}");
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] 删除失败: {e.Message}");
                return false;
            }
        }
        
        #endregion
        
        #region 自动存档
        
        /// <summary>
        /// 自动存档 (SV-003)
        /// </summary>
        public void AutoSave()
        {
            if (isSaving) return;
            
            // 使用特殊槽位进行自动存档
            SaveGame("AutoSave", false);
            Debug.Log("[SaveManager] 自动存档完成");
        }
        
        /// <summary>
        /// 事件触发存档 (SV-003)
        /// </summary>
        public void SaveOnEvent(SaveTriggerEvent triggerEvent)
        {
            switch (triggerEvent)
            {
                case SaveTriggerEvent.MissionComplete:
                case SaveTriggerEvent.DepthMilestone:
                case SaveTriggerEvent.BeforeBoss:
                    AutoSave();
                    break;
            }
        }
        
        #endregion
        
        #region 备份系统
        
        /// <summary>
        /// 创建备份
        /// </summary>
        private void CreateBackup(string slotName)
        {
            string sourcePath = GetSaveFilePath(slotName);
            string backupDir = Path.Combine(SaveDirectory, $"{slotName}_Backups");
            
            if (!Directory.Exists(backupDir))
            {
                Directory.CreateDirectory(backupDir);
            }
            
            // 生成备份文件名
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string backupPath = Path.Combine(backupDir, $"{slotName}_{timestamp}{backupExtension}");
            
            File.Copy(sourcePath, backupPath, true);
            
            // 清理旧备份
            CleanupOldBackups(backupDir);
            
            Debug.Log($"[SaveManager] 创建备份: {backupPath}");
        }
        
        /// <summary>
        /// 清理旧备份
        /// </summary>
        private void CleanupOldBackups(string backupDir)
        {
            var backupFiles = new DirectoryInfo(backupDir).GetFiles("*.bak");
            
            if (backupFiles.Length > maxBackupCount)
            {
                // 按时间排序，删除最旧的
                Array.Sort(backupFiles, (a, b) => a.CreationTime.CompareTo(b.CreationTime));
                
                for (int i = 0; i < backupFiles.Length - maxBackupCount; i++)
                {
                    backupFiles[i].Delete();
                }
            }
        }
        
        /// <summary>
        /// 尝试从备份恢复
        /// </summary>
        private bool TryRestoreFromBackup(string slotName)
        {
            string backupDir = Path.Combine(SaveDirectory, $"{slotName}_Backups");
            
            if (!Directory.Exists(backupDir)) return false;
            
            var backupFiles = new DirectoryInfo(backupDir).GetFiles("*.bak");
            if (backupFiles.Length == 0) return false;
            
            // 获取最新的备份
            Array.Sort(backupFiles, (a, b) => b.CreationTime.CompareTo(a.CreationTime));
            string latestBackup = backupFiles[0].FullName;
            string targetPath = GetSaveFilePath(slotName);
            
            try
            {
                File.Copy(latestBackup, targetPath, true);
                Debug.Log($"[SaveManager] 已从备份恢复: {latestBackup}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] 备份恢复失败: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 删除备份
        /// </summary>
        private void DeleteBackups(string slotName)
        {
            string backupDir = Path.Combine(SaveDirectory, $"{slotName}_Backups");
            
            if (Directory.Exists(backupDir))
            {
                Directory.Delete(backupDir, true);
            }
        }
        
        #endregion
        
        #region 导入/导出 (SV-006)
        
        /// <summary>
        /// 导出存档到指定路径
        /// </summary>
        public bool ExportSave(string slotName, string exportPath)
        {
            try
            {
                string sourcePath = GetSaveFilePath(slotName);
                
                if (!File.Exists(sourcePath))
                {
                    Debug.LogWarning($"[SaveManager] 存档不存在: {slotName}");
                    return false;
                }
                
                File.Copy(sourcePath, exportPath, true);
                Debug.Log($"[SaveManager] 存档已导出: {exportPath}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] 导出失败: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// 从指定路径导入存档
        /// </summary>
        public bool ImportSave(string importPath, string slotName)
        {
            try
            {
                if (!File.Exists(importPath))
                {
                    Debug.LogWarning("[SaveManager] 导入文件不存在");
                    return false;
                }
                
                string targetPath = GetSaveFilePath(slotName);
                
                // 验证文件格式
                string content = File.ReadAllText(importPath);
                var wrapper = JsonUtility.FromJson<SaveDataWrapper>(content);
                
                if (wrapper == null || string.IsNullOrEmpty(wrapper.data))
                {
                    Debug.LogError("[SaveManager] 无效的存档文件格式");
                    return false;
                }
                
                File.Copy(importPath, targetPath, true);
                Debug.Log($"[SaveManager] 存档已导入: {slotName}");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] 导入失败: {e.Message}");
                return false;
            }
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 检查存档是否存在
        /// </summary>
        public bool SaveExists(string slotName)
        {
            return File.Exists(GetSaveFilePath(slotName));
        }
        
        /// <summary>
        /// 获取存档文件路径
        /// </summary>
        private string GetSaveFilePath(string slotName)
        {
            return Path.Combine(SaveDirectory, $"{slotName}{fileExtension}");
        }
        
        /// <summary>
        /// 获取所有存档信息
        /// </summary>
        public SaveInfo[] GetAllSaves()
        {
            var saves = new System.Collections.Generic.List<SaveInfo>();
            
            for (int i = 0; i < maxSaveSlots; i++)
            {
                string slotName = $"SaveSlot{i + 1}";
                var info = GetSaveInfo(slotName);
                if (info != null)
                {
                    saves.Add(info);
                }
            }
            
            // 添加自动存档
            var autoSaveInfo = GetSaveInfo("AutoSave");
            if (autoSaveInfo != null)
            {
                autoSaveInfo.slotName = "AutoSave";
                saves.Add(autoSaveInfo);
            }
            
            return saves.ToArray();
        }
        
        /// <summary>
        /// 获取指定存档信息
        /// </summary>
        public SaveInfo GetSaveInfo(string slotName)
        {
            string filePath = GetSaveFilePath(slotName);
            
            if (!File.Exists(filePath))
            {
                return null;
            }
            
            try
            {
                string finalJson = File.ReadAllText(filePath);
                var wrapper = JsonUtility.FromJson<SaveDataWrapper>(finalJson);
                var saveData = JsonUtility.FromJson<GameSaveData>(wrapper.data);
                
                return new SaveInfo
                {
                    slotName = slotName,
                    saveTime = saveData.saveTime,
                    version = saveData.version,
                    currentDepth = saveData.currentDepth,
                    playTime = saveData.playTime
                };
            }
            catch
            {
                return null;
            }
        }
        
        /// <summary>
        /// 计算校验和 (SV-002)
        /// </summary>
        private string CalculateChecksum(string data)
        {
            using (var md5 = MD5.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                byte[] hash = md5.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
        
        /// <summary>
        /// 创建存档数据
        /// </summary>
        private GameSaveData CreateSaveData()
        {
            return new GameSaveData
            {
                saveTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                version = SAVE_VERSION,
                playTime = Time.time,
                
                // 保存资源数据
                oxygen = GameManager.Instance?.resourceManager?.CurrentOxygen ?? 100f,
                maxOxygen = GameManager.Instance?.resourceManager?.MaxOxygen ?? 100f,
                energy = GameManager.Instance?.resourceManager?.CurrentEnergy ?? 100f,
                maxEnergy = GameManager.Instance?.resourceManager?.MaxEnergy ?? 100f,
                credits = GameManager.Instance?.resourceManager?.Credits ?? 0,
                
                // 保存下潜数据
                currentDepth = GameManager.Instance?.diveManager?.CurrentDepth ?? 0f,
                maxDepthReached = GameManager.Instance?.diveManager?.MaxDepthReached ?? 0f,
                
                // 保存背包数据
                inventoryData = GameManager.Instance?.resourceManager?.GetInventorySaveData(),
                
                // 保存成就数据
                unlockedAchievements = Achievement.AchievementManager.Instance?.GetUnlockedAchievementIds() ?? new System.Collections.Generic.List<string>(),
                achievementProgress = Achievement.AchievementManager.Instance?.GetAchievementProgress() ?? new System.Collections.Generic.Dictionary<string, float>()
            };
        }
        
        /// <summary>
        /// 应用存档数据到游戏
        /// </summary>
        private void ApplySaveData(GameSaveData saveData)
        {
            // 加载资源数据
            if (GameManager.Instance?.resourceManager != null)
            {
                GameManager.Instance.resourceManager.LoadFromSave(saveData);
            }
            
            // 加载下潜数据
            if (GameManager.Instance?.diveManager != null)
            {
                GameManager.Instance.diveManager.LoadFromSave(saveData);
            }
            
            // 加载成就数据
            if (Achievement.AchievementManager.Instance != null)
            {
                Achievement.AchievementManager.Instance.LoadAchievementData(saveData.unlockedAchievements, saveData.achievementProgress);
            }
        }
        
        /// <summary>
        /// 存档数据版本迁移 (SV-001)
        /// </summary>
        private GameSaveData MigrateSaveData(GameSaveData data, int fromVersion, int toVersion)
        {
            Debug.Log($"[SaveManager] 迁移存档: v{fromVersion} -> v{toVersion}");
            
            // 版本迁移逻辑
            if (fromVersion < 1)
            {
                // 从旧版本迁移的特定逻辑
                // 例如: 添加新字段的默认值
            }
            
            return data;
        }
        
        #endregion
    }
    
    /// <summary>
    /// 存档数据包装器 (包含元数据)
    /// </summary>
    [Serializable]
    public class SaveDataWrapper
    {
        public int version;
        public string checksum;
        public string data;
    }
    
    /// <summary>
    /// 游戏存档数据结构
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        public string saveTime;
        public int version;
        public float playTime;
        
        // 资源数据
        public float oxygen;
        public float maxOxygen;
        public float energy;
        public float maxEnergy;
        public int credits;
        
        // 下潜数据
        public float currentDepth;
        public float maxDepthReached;
        
        // 背包数据 (JSON字符串)
        public string inventoryData;
        
        // 成就数据
        public System.Collections.Generic.List<string> unlockedAchievements;
        public System.Collections.Generic.Dictionary<string, float> achievementProgress;
    }
    
    /// <summary>
    /// 存档信息
    /// </summary>
    [Serializable]
    public class SaveInfo
    {
        public string slotName;
        public string saveTime;
        public int version;
        public float currentDepth;
        public float playTime;
    }
    
    /// <summary>
    /// 存档触发事件
    /// </summary>
    public enum SaveTriggerEvent
    {
        Manual,
        Auto,
        MissionComplete,
        DepthMilestone,
        BeforeBoss,
        OnExit
    }
}
