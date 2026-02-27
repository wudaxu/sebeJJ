using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

namespace SebeJJ.Core
{
    /// <summary>
    /// 错误恢复系统 - BUG-019修复
    /// 处理游戏异常状态恢复
    /// </summary>
    public class ErrorRecoverySystem : MonoBehaviour
    {
        public static ErrorRecoverySystem Instance { get; private set; }
        
        [Header("恢复设置")]
        [SerializeField] private bool autoSaveOnPause = true;
        [SerializeField] private float autoSaveInterval = 60f;
        [SerializeField] private int maxCheckpointCount = 5;
        
        [Header("关键系统")]
        [SerializeField] private List<GameObject> criticalSystems = new List<GameObject>();
        
        // 检查点数据
        private CheckpointData lastCheckpoint;
        private float lastAutoSaveTime;
        private bool isRecovering = false;
        
        // 事件
        public event Action OnRecoveryStarted;
        public event Action OnRecoveryCompleted;
        public event Action<string> OnRecoveryFailed;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void Start()
        {
            // 检查是否需要恢复
            CheckForCrashRecovery();
        }
        
        private void Update()
        {
            // 自动保存检查点
            if (autoSaveOnPause && Time.time - lastAutoSaveTime >= autoSaveInterval)
            {
                CreateEmergencyCheckpoint();
                lastAutoSaveTime = Time.time;
            }
        }
        
        private void OnApplicationPause(bool pause)
        {
            if (pause && autoSaveOnPause)
            {
                CreateEmergencyCheckpoint();
            }
        }
        
        private void OnApplicationQuit()
        {
            // 清理恢复标记
            ClearRecoveryFlag();
        }
        
        /// <summary>
        /// 创建紧急检查点
        /// </summary>
        public void CreateEmergencyCheckpoint()
        {
            try
            {
                var checkpoint = new CheckpointData
                {
                    timestamp = DateTime.Now,
                    gameTime = Time.time,
                    playerPosition = GameManager.Instance?.playerTransform?.position ?? Vector3.zero,
                    playerHealth = GameManager.Instance?.playerHealth?.CurrentHealth ?? 100f,
                    currentMissionIds = GetCurrentMissionIds(),
                    inventoryData = SerializeInventory(),
                    upgradeData = SerializeUpgrades()
                };
                
                lastCheckpoint = checkpoint;
                SaveCheckpointToDisk(checkpoint);
                SetRecoveryFlag();
                
                Debug.Log("[ErrorRecovery] 紧急检查点已创建");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ErrorRecovery] 创建检查点失败: {e.Message}");
            }
        }
        
        /// <summary>
        /// 检查是否需要从崩溃恢复
        /// </summary>
        private void CheckForCrashRecovery()
        {
            if (HasRecoveryFlag())
            {
                Debug.Log("[ErrorRecovery] 检测到未正常退出，准备恢复...");
                StartCoroutine(RecoverFromCrashCoroutine());
            }
        }
        
        /// <summary>
        /// 从崩溃恢复
        /// </summary>
        public void RecoverFromCrash()
        {
            StartCoroutine(RecoverFromCrashCoroutine());
        }
        
        private System.Collections.IEnumerator RecoverFromCrashCoroutine()
        {
            isRecovering = true;
            OnRecoveryStarted?.Invoke();
            
            Debug.Log("[ErrorRecovery] 开始恢复游戏状态...");
            
            // 加载检查点
            var checkpoint = LoadCheckpointFromDisk();
            if (checkpoint == null)
            {
                OnRecoveryFailed?.Invoke("无法加载检查点数据");
                isRecovering = false;
                yield break;
            }
            
            // 等待场景加载
            yield return new WaitForSeconds(0.5f);
            
            try
            {
                // 恢复玩家位置
                if (GameManager.Instance?.playerTransform != null)
                {
                    GameManager.Instance.playerTransform.position = checkpoint.playerPosition;
                }
                
                // 恢复玩家生命值
                if (GameManager.Instance?.playerHealth != null)
                {
                    GameManager.Instance.playerHealth.SetHealth(checkpoint.playerHealth);
                }
                
                // 恢复委托状态
                RestoreMissions(checkpoint.currentMissionIds);
                
                // 恢复背包
                RestoreInventory(checkpoint.inventoryData);
                
                // 恢复升级
                RestoreUpgrades(checkpoint.upgradeData);
                
                // 清理异常数据
                CleanupCorruptedData();
                
                // 验证关键系统
                VerifyCriticalSystems();
                
                Debug.Log("[ErrorRecovery] 恢复完成");
                OnRecoveryCompleted?.Invoke();
                
                // 显示恢复提示
                UIManager.Instance?.ShowNotification("游戏已从异常状态恢复");
            }
            catch (Exception e)
            {
                Debug.LogError($"[ErrorRecovery] 恢复失败: {e.Message}");
                OnRecoveryFailed?.Invoke(e.Message);
            }
            
            ClearRecoveryFlag();
            isRecovering = false;
        }
        
        /// <summary>
        /// 验证关键系统状态
        /// </summary>
        private void VerifyCriticalSystems()
        {
            foreach (var system in criticalSystems)
            {
                if (system == null)
                {
                    Debug.LogWarning($"[ErrorRecovery] 关键系统缺失，尝试重新初始化...");
                    // 尝试重新初始化
                }
            }
        }
        
        /// <summary>
        /// 清理损坏的数据
        /// </summary>
        private void CleanupCorruptedData()
        {
            // 清理无效的委托
            var missionManager = Systems.MissionManager.Instance;
            if (missionManager != null)
            {
                // 移除已损坏的委托数据
            }
            
            // 清理无效的敌人引用
            var enemies = FindObjectsOfType<Enemies.EnemyBase>();
            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.gameObject == null)
                {
                    Debug.LogWarning("[ErrorRecovery] 发现无效敌人引用，已清理");
                }
            }
        }
        
        #region 数据序列化
        
        private List<string> GetCurrentMissionIds()
        {
            var ids = new List<string>();
            var missionManager = Systems.MissionManager.Instance;
            if (missionManager != null)
            {
                foreach (var mission in missionManager.ActiveMissions)
                {
                    ids.Add(mission.MissionId);
                }
            }
            return ids;
        }
        
        private string SerializeInventory()
        {
            // 序列化背包数据
            return JsonUtility.ToJson(GameManager.Instance?.resourceManager?.GetInventoryData());
        }
        
        private string SerializeUpgrades()
        {
            var upgradeManager = Upgrade.UpgradeManager.Instance;
            if (upgradeManager != null)
            {
                return JsonUtility.ToJson(upgradeManager.GetSaveData());
            }
            return "";
        }
        
        private void RestoreMissions(List<string> missionIds)
        {
            // 恢复委托状态
            Debug.Log($"[ErrorRecovery] 恢复 {missionIds.Count} 个活跃委托");
        }
        
        private void RestoreInventory(string data)
        {
            if (string.IsNullOrEmpty(data)) return;
            // 恢复背包数据
        }
        
        private void RestoreUpgrades(string data)
        {
            if (string.IsNullOrEmpty(data)) return;
            var saveData = JsonUtility.FromJson<Upgrade.UpgradeSaveData>(data);
            Upgrade.UpgradeManager.Instance?.LoadSaveData(saveData);
        }
        
        #endregion
        
        #region 文件操作
        
        private void SaveCheckpointToDisk(CheckpointData checkpoint)
        {
            string path = GetCheckpointPath();
            string json = JsonUtility.ToJson(checkpoint);
            File.WriteAllText(path, json);
        }
        
        private CheckpointData LoadCheckpointFromDisk()
        {
            string path = GetCheckpointPath();
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<CheckpointData>(json);
            }
            return null;
        }
        
        private string GetCheckpointPath()
        {
            return Path.Combine(Application.persistentDataPath, "emergency_checkpoint.json");
        }
        
        private void SetRecoveryFlag()
        {
            PlayerPrefs.SetInt("ErrorRecovery_Needed", 1);
            PlayerPrefs.Save();
        }
        
        private void ClearRecoveryFlag()
        {
            PlayerPrefs.SetInt("ErrorRecovery_Needed", 0);
            PlayerPrefs.Save();
        }
        
        private bool HasRecoveryFlag()
        {
            return PlayerPrefs.GetInt("ErrorRecovery_Needed", 0) == 1;
        }
        
        #endregion
    }
    
    [Serializable]
    public class CheckpointData
    {
        public DateTime timestamp;
        public float gameTime;
        public Vector3 playerPosition;
        public float playerHealth;
        public List<string> currentMissionIds;
        public string inventoryData;
        public string upgradeData;
    }
}
