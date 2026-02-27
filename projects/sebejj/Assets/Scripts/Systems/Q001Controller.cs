using UnityEngine;
using System.Collections.Generic;

namespace SebeJJ.Systems
{
    /// <summary>
    /// Q001 新手试潜委托数据
    /// </summary>
    [CreateAssetMenu(fileName = "Q001_Tutorial", menuName = "SebeJJ/Missions/Q001 Tutorial")]
    public class Q001TutorialData : MissionData
    {
        [Header("Q001 特殊设置")]
        public bool isTutorial = true;
        public List<TutorialStep> tutorialSteps = new List<TutorialStep>();
        
        [System.Serializable]
        public class TutorialStep
        {
            public string stepId;
            public string instructionText;
            public TutorialAction requiredAction;
            public float targetValue;
            public string targetId;
            public bool autoAdvance;
            public float displayDuration = 5f;
        }
        
        public enum TutorialAction
        {
            Move,
            Scan,
            Collect,
            ReachDepth,
            Wait,
            ButtonPress
        }
    }
    
    /// <summary>
    /// Q001 运行时控制器
    /// </summary>
    public class Q001Controller : MonoBehaviour
    {
        public static Q001Controller Instance { get; private set; }
        
        [Header("Q001 配置")]
        public Q001TutorialData missionData;
        
        [Header("生成设置")]
        public GameObject tutorialCrystalPrefab;
        public int crystalSpawnCount = 5;
        public float spawnRadius = 8f;
        
        [Header("区域标记")]
        public Transform startArea;
        public Transform practiceArea;
        public Transform targetDepthMarker;
        
        private List<GameObject> spawnedCrystals = new List<GameObject>();
        private int currentStepIndex = 0;
        private bool missionActive = false;
        
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
            // 订阅游戏开始事件
            Utils.GameEvents.OnGameStarted += OnGameStarted;
        }
        
        private void OnDestroy()
        {
            Utils.GameEvents.OnGameStarted -= OnGameStarted;
        }
        
        private void OnGameStarted()
        {
            // 检查是否是新游戏
            StartCoroutine(StartQ001Mission());
        }
        
        private System.Collections.IEnumerator StartQ001Mission()
        {
            yield return new WaitForSeconds(1f);
            
            missionActive = true;
            
            // 生成教学资源
            SpawnTutorialResources();
            
            // 自动接取Q001委托
            var missionManager = MissionManager.Instance;
            if (missionManager != null)
            {
                // 创建Q001委托实例
                var mission = new Mission(missionData);
                missionManager.ActiveMissions.Add(mission);
                
                Debug.Log("[Q001] 新手试潜委托已启动");
                
                // 显示欢迎消息
                ShowWelcomeMessage();
            }
        }
        
        private void SpawnTutorialResources()
        {
            if (tutorialCrystalPrefab == null || practiceArea == null) return;
            
            for (int i = 0; i < crystalSpawnCount; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
                Vector3 spawnPos = practiceArea.position + new Vector3(randomOffset.x, randomOffset.y, 0);
                
                GameObject crystal = Instantiate(tutorialCrystalPrefab, spawnPos, Quaternion.identity);
                
                // 设置水晶为教学专用
                var resource = crystal.GetComponent<Player.CollectibleResource>();
                if (resource != null)
                {
                    resource.resourceId = "crystal_tutorial";
                    resource.resourceName = "教学水晶";
                    resource.requiresScan = true;
                }
                
                spawnedCrystals.Add(crystal);
            }
            
            Debug.Log($"[Q001] 已生成 {crystalSpawnCount} 个教学水晶");
        }
        
        private void ShowWelcomeMessage()
        {
            Core.UIManager.Instance?.ShowNotification(
                "任务开始：新手试潜\n" +
                "目标：学习基础操作，采集3个水晶，下潜至50米"
            );
            
            Utils.AudioManager.Instance?.PlaySFX("mission_start");
        }
        
        /// <summary>
        /// 报告水晶采集
        /// </summary>
        public void ReportCrystalCollected()
        {
            if (!missionActive) return;
            
            // 更新委托进度
            MissionManager.Instance?.UpdateMissionProgress("crystal_tutorial", 1);
            
            // 播放反馈
            Utils.AudioManager.Instance?.PlaySFX("collect_success");
            
            // 检查是否完成采集目标
            CheckCollectionObjective();
        }
        
        /// <summary>
        /// 报告扫描完成
        /// </summary>
        public void ReportScanCompleted()
        {
            if (!missionActive) return;
            
            MissionManager.Instance?.UpdateMissionProgress("scan_tutorial", 1);
        }
        
        /// <summary>
        /// 检查采集目标
        /// </summary>
        private void CheckCollectionObjective()
        {
            var mission = GetQ001Mission();
            if (mission == null) return;
            
            // 检查是否完成所有目标
            if (mission.GetOverallProgress() >= 1f)
            {
                CompleteQ001();
            }
        }
        
        /// <summary>
        /// 完成Q001
        /// </summary>
        private void CompleteQ001()
        {
            missionActive = false;
            
            // 清理生成的水晶
            foreach (var crystal in spawnedCrystals)
            {
                if (crystal != null)
                    Destroy(crystal);
            }
            spawnedCrystals.Clear();
            
            Debug.Log("[Q001] 新手试潜完成！");
        }
        
        /// <summary>
        /// 获取Q001委托实例
        /// </summary>
        private Mission GetQ001Mission()
        {
            var missionManager = MissionManager.Instance;
            if (missionManager == null) return null;
            
            return missionManager.ActiveMissions.Find(m => m.MissionId == "Q001");
        }
        
        /// <summary>
        /// 是否正在进行Q001
        /// </summary>
        public bool IsActive => missionActive;
        
        private void OnDrawGizmos()
        {
            // 绘制练习区域
            if (practiceArea != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(practiceArea.position, spawnRadius);
            }
            
            // 绘制目标深度
            if (targetDepthMarker != null)
            {
                Gizmos.color = Color.yellow;
                Vector3 pos = targetDepthMarker.position;
                Gizmos.DrawLine(pos + Vector3.left * 10, pos + Vector3.right * 10);
                Gizmos.DrawLine(pos + Vector3.up * 2, pos + Vector3.down * 2);
            }
        }
    }
}
