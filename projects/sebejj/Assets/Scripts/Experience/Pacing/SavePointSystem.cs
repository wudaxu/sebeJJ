using System.Collections.Generic;
using UnityEngine;

namespace SebeJJ.Experience.Pacing
{
    /// <summary>
    /// 存档点系统
    /// </summary>
    public class SavePointSystem : MonoBehaviour
    {
        public static SavePointSystem Instance { get; private set; }
        
        [Header("存档配置")]
        [SerializeField] private float autoSaveInterval = 60f;
        [SerializeField] private bool saveOnDepthChange = true;
        [SerializeField] private float depthChangeThreshold = 10f;
        [SerializeField] private bool saveOnSafeZone = true;
        
        [Header("存档点分布")]
        [SerializeField] private List<SavePointLocation> checkpointLocations;
        
        private SavePointData lastSavePoint;
        private float lastSaveTime = 0f;
        private float lastSaveDepth = 0f;
        private bool isInSafeZone = false;
        
        public SavePointData LastSavePoint => lastSavePoint;
        public bool IsInSafeZone => isInSafeZone;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Update()
        {
            // 自动存档
            if (Time.time - lastSaveTime >= autoSaveInterval)
            {
                CreateAutoSave();
            }
            
            // 深度变化存档
            if (saveOnDepthChange)
            {
                CheckDepthChange();
            }
        }
        
        /// <summary>
        /// 创建存档点
        /// </summary>
        public void CreateSavePoint(SavePointType type, Vector3 position)
        {
            var saveData = new SavePointData
            {
                Type = type,
                Position = position,
                Depth = GetCurrentDepth(),
                Timestamp = System.DateTime.Now,
                PlayerState = CapturePlayerState(),
                Inventory = CaptureInventory(),
                MissionProgress = CaptureMissionProgress()
            };
            
            lastSavePoint = saveData;
            lastSaveTime = Time.time;
            lastSaveDepth = saveData.Depth;
            
            // 写入存档
            WriteSavePoint(saveData);
            
            // 视觉反馈
            if (type == SavePointType.Checkpoint || type == SavePointType.SafeZone)
            {
                ShowSaveIndicator(type);
            }
            
            Debug.Log($"[SavePoint] 存档创建: {type} at {position}");
        }
        
        /// <summary>
        /// 创建自动存档
        /// </summary>
        private void CreateAutoSave()
        {
            // Vector3 playerPos = PlayerController.Instance.transform.position;
            // CreateSavePoint(SavePointType.AutoSave, playerPos);
        }
        
        /// <summary>
        /// 检查深度变化
        /// </summary>
        private void CheckDepthChange()
        {
            float currentDepth = GetCurrentDepth();
            if (Mathf.Abs(currentDepth - lastSaveDepth) >= depthChangeThreshold)
            {
                // Vector3 playerPos = PlayerController.Instance.transform.position;
                // CreateSavePoint(SavePointType.DepthChange, playerPos);
            }
        }
        
        /// <summary>
        /// 进入安全区
        /// </summary>
        public void EnterSafeZone(Vector3 safeZonePosition)
        {
            isInSafeZone = true;
            
            if (saveOnSafeZone)
            {
                CreateSavePoint(SavePointType.SafeZone, safeZonePosition);
            }
            
            // 显示安全区UI
            // UIManager.Instance.ShowSafeZoneIndicator(true);
        }
        
        /// <summary>
        /// 离开安全区
        /// </summary>
        public void ExitSafeZone()
        {
            isInSafeZone = false;
            
            // UIManager.Instance.ShowSafeZoneIndicator(false);
        }
        
        /// <summary>
        /// 写入存档点
        /// </summary>
        private void WriteSavePoint(SavePointData data)
        {
            // SaveManager.Instance.WriteCheckpoint(data);
        }
        
        /// <summary>
        /// 显示存档指示器
        /// </summary>
        private void ShowSaveIndicator(SavePointType type)
        {
            string message = type switch
            {
                SavePointType.Checkpoint => "☑ 检查点已保存",
                SavePointType.SafeZone => "☑ 安全区存档",
                SavePointType.AutoSave => "☑ 自动保存",
                SavePointType.DepthChange => "☑ 进度保存",
                _ => "☑ 已保存"
            };
            
            // UIManager.Instance.ShowToast(message, duration: 2f);
        }
        
        /// <summary>
        /// 获取最近的存档点
        /// </summary>
        public SavePointData GetNearestCheckpoint(Vector3 position)
        {
            // 返回最近的检查点
            // 如果没有，返回基地存档
            return lastSavePoint;
        }
        
        /// <summary>
        /// 获取基地存档
        /// </summary>
        public SavePointData GetBaseSavePoint()
        {
            // return SaveManager.Instance.GetBaseSavePoint();
            return null;
        }
        
        /// <summary>
        /// 捕获玩家状态
        /// </summary>
        private PlayerStateData CapturePlayerState()
        {
            return new PlayerStateData
            {
                // Health = PlayerController.Instance.CurrentHealth,
                // Energy = PlayerController.Instance.CurrentEnergy,
                // Position = PlayerController.Instance.transform.position,
                // Equipment = EquipmentManager.Instance.GetCurrentLoadout()
            };
        }
        
        /// <summary>
        /// 捕获背包状态
        /// </summary>
        private InventoryData CaptureInventory()
        {
            return new InventoryData
            {
                // Items = InventoryManager.Instance.GetAllItems(),
                // Credits = PlayerManager.Instance.Credits
            };
        }
        
        /// <summary>
        /// 捕获委托进度
        /// </summary>
        private MissionProgressData CaptureMissionProgress()
        {
            return new MissionProgressData
            {
                // CurrentMission = MissionManager.Instance.GetCurrentMission(),
                // Objectives = MissionManager.Instance.GetObjectiveProgress()
            };
        }
        
        /// <summary>
        /// 获取当前深度
        /// </summary>
        private float GetCurrentDepth()
        {
            // return DiveManager.Instance.CurrentDepth;
            return 0f;
        }
        
        /// <summary>
        /// 手动存档（玩家触发）
        /// </summary>
        public void ManualSave()
        {
            if (isInSafeZone)
            {
                // Vector3 playerPos = PlayerController.Instance.transform.position;
                // CreateSavePoint(SavePointType.Manual, playerPos);
            }
            else
            {
                // UIManager.Instance.ShowMessage("只能在安全区手动存档");
            }
        }
    }
    
    /// <summary>
    /// 存档点类型
    /// </summary>
    public enum SavePointType
    {
        AutoSave,       // 自动存档
        Checkpoint,     // 检查点
        SafeZone,       // 安全区存档
        DepthChange,    // 深度变化存档
        Manual,         // 手动存档
        BossPre         // Boss战前存档
    }
    
    /// <summary>
    /// 存档点数据
    /// </summary>
    [System.Serializable]
    public class SavePointData
    {
        public SavePointType Type;
        public Vector3 Position;
        public float Depth;
        public System.DateTime Timestamp;
        public PlayerStateData PlayerState;
        public InventoryData Inventory;
        public MissionProgressData MissionProgress;
    }
    
    /// <summary>
    /// 存档点位置配置
    /// </summary>
    [System.Serializable]
    public class SavePointLocation
    {
        public string LocationId;
        public string LocationName;
        public float Depth;
        public Vector3 Position;
        public SavePointType Type;
        public bool IsSafeZone;
    }
    
    [System.Serializable]
    public class PlayerStateData
    {
        public float Health;
        public float Energy;
        public Vector3 Position;
        // public EquipmentLoadout Equipment;
    }
    
    [System.Serializable]
    public class InventoryData
    {
        // public List<InventoryItem> Items;
        public int Credits;
    }
    
    [System.Serializable]
    public class MissionProgressData
    {
        // public MissionData CurrentMission;
        // public Dictionary<string, int> Objectives;
    }
}
