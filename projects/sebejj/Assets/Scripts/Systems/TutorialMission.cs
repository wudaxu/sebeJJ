using UnityEngine;
using System.Collections;

namespace SebeJJ.Systems
{
    /// <summary>
    /// 新手试潜委托 - Q001
    /// 教学性质的初始任务
    /// </summary>
    public class TutorialMission : MonoBehaviour
    {
        public static TutorialMission Instance { get; private set; }
        
        [Header("Q001 设置")]
        public string missionId = "Q001";
        public string missionTitle = "新手试潜";
        
        [Header("阶段目标")]
        public float targetDepth = 50f;
        public int targetCrystalCount = 3;
        public float targetScanCount = 2;
        
        [Header("生成点")]
        public Transform crystalSpawnArea;
        public Transform tutorialStartPoint;
        
        [Header("教学UI")]
        public GameObject tutorialPanel;
        public TMPro.TextMeshProUGUI tutorialText;
        
        // 阶段
        private TutorialPhase currentPhase = TutorialPhase.None;
        private int collectedCrystals = 0;
        private int scannedObjects = 0;
        
        public enum TutorialPhase
        {
            None,
            Start,          // 开始教学
            Movement,       // 移动教学
            Scanning,       // 扫描教学
            Collection,     // 采集教学
            DeepDive,       // 下潜教学
            Complete        // 完成
        }
        
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
            // 等待游戏开始
            StartCoroutine(WaitForGameStart());
        }
        
        private IEnumerator WaitForGameStart()
        {
            yield return new WaitUntil(() => Core.GameManager.Instance?.CurrentState == GameState.Playing);
            
            // 延迟开始教学
            yield return new WaitForSeconds(1f);
            
            StartTutorial();
        }
        
        /// <summary>
        /// 开始教学
        /// </summary>
        public void StartTutorial()
        {
            currentPhase = TutorialPhase.Start;
            
            ShowTutorialMessage(
                "欢迎来到深海勘探中心！\n\n" +
                "我是你的AI助手，将指导你完成首次下潜任务。\n\n" +
                "按 [WASD] 或 [方向键] 控制机甲移动。"
            );
            
            // 播放欢迎音效
            Utils.AudioManager.Instance?.PlaySFX("welcome");
            
            // 生成教学用水晶
            SpawnTutorialCrystals();
            
            // 进入下一阶段
            StartCoroutine(AdvancePhaseAfterDelay(5f, TutorialPhase.Movement));
        }
        
        /// <summary>
        /// 生成教学水晶
        /// </summary>
        private void SpawnTutorialCrystals()
        {
            if (crystalSpawnArea == null) return;
            
            // 在生成区域内随机放置水晶
            for (int i = 0; i < targetCrystalCount + 2; i++)
            {
                Vector2 randomPos = (Vector2)crystalSpawnArea.position + Random.insideUnitCircle * 5f;
                
                // 这里应该实例化水晶预制体
                // GameObject crystal = Instantiate(crystalPrefab, randomPos, Quaternion.identity);
                
                Debug.Log($"[TutorialMission] 在 {randomPos} 生成教学水晶");
            }
        }
        
        /// <summary>
        /// 进入下一阶段
        /// </summary>
        private void AdvancePhase(TutorialPhase newPhase)
        {
            currentPhase = newPhase;
            
            switch (newPhase)
            {
                case TutorialPhase.Movement:
                    ShowTutorialMessage(
                        "很好！你已经掌握了基本移动。\n\n" +
                        "现在让我们学习扫描功能。\n" +
                        "按 [空格键] 激活扫描器，可以探测周围的资源。"
                    );
                    break;
                    
                case TutorialPhase.Scanning:
                    ShowTutorialMessage(
                        "扫描器已激活！\n\n" +
                        "被扫描到的资源会高亮显示。\n" +
                        "现在尝试扫描至少 2 个目标。"
                    );
                    break;
                    
                case TutorialPhase.Collection:
                    ShowTutorialMessage(
                        "完美！现在让我们采集资源。\n\n" +
                        "靠近高亮的资源，按 [E] 键采集。\n" +
                        "你需要采集 3 个深海水晶。"
                    );
                    break;
                    
                case TutorialPhase.DeepDive:
                    ShowTutorialMessage(
                        "采集完成！\n\n" +
                        "最后，让我们下潜到 50 米深度。\n" +
                        "注意监控氧气存量！"
                    );
                    break;
                    
                case TutorialPhase.Complete:
                    CompleteTutorial();
                    break;
            }
            
            Utils.AudioManager.Instance?.PlaySFX("phase_advance");
        }
        
        private IEnumerator AdvancePhaseAfterDelay(float delay, TutorialPhase newPhase)
        {
            yield return new WaitForSeconds(delay);
            AdvancePhase(newPhase);
        }
        
        /// <summary>
        /// 显示教学消息
        /// </summary>
        private void ShowTutorialMessage(string message)
        {
            if (tutorialPanel != null)
                tutorialPanel.SetActive(true);
            
            if (tutorialText != null)
            {
                tutorialText.text = message;
                
                // 打字机效果
                StopCoroutine("TypewriterEffect");
                StartCoroutine(TypewriterEffect(message));
            }
            
            Debug.Log($"[Tutorial] {message}");
        }
        
        private IEnumerator TypewriterEffect(string fullText)
        {
            if (tutorialText == null) yield break;
            
            tutorialText.text = "";
            
            foreach (char c in fullText)
            {
                tutorialText.text += c;
                
                if (c != ' ' && c != '\n')
                {
                    yield return new WaitForSeconds(0.02f);
                }
            }
        }
        
        /// <summary>
        /// 完成教学
        /// </summary>
        private void CompleteTutorial()
        {
            ShowTutorialMessage(
                "恭喜！你已完成新手试潜！\n\n" +
                "奖励已发放：\n" +
                "• 500 信用点\n" +
                "• 基础扫描模块\n\n" +
                "现在你可以接取更多委托了！"
            );
            
            // 发放奖励
            Core.GameManager.Instance?.resourceManager?.AddCredits(500);
            
            // 播放完成特效
            Utils.EffectManager.Instance?.PlayMissionCompleteEffect();
            
            // 隐藏教学面板
            StartCoroutine(HideTutorialAfterDelay(5f));
            
            Debug.Log("[TutorialMission] 教学完成！");
        }
        
        private IEnumerator HideTutorialAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (tutorialPanel != null)
                tutorialPanel.SetActive(false);
        }
        
        /// <summary>
        /// 报告扫描
        /// </summary>
        public void ReportScan()
        {
            if (currentPhase != TutorialPhase.Scanning) return;
            
            scannedObjects++;
            
            if (scannedObjects >= targetScanCount)
            {
                AdvancePhase(TutorialPhase.Collection);
            }
            else
            {
                ShowTutorialMessage($"已扫描 {scannedObjects}/{targetScanCount} 个目标，继续扫描！");
            }
        }
        
        /// <summary>
        /// 报告采集
        /// </summary>
        public void ReportCollection(string resourceId)
        {
            if (currentPhase != TutorialPhase.Collection) return;
            
            if (resourceId.Contains("crystal"))
            {
                collectedCrystals++;
                
                ShowTutorialMessage($"已采集 {collectedCrystals}/{targetCrystalCount} 个水晶！");
                
                if (collectedCrystals >= targetCrystalCount)
                {
                    AdvancePhase(TutorialPhase.DeepDive);
                }
            }
        }
        
        /// <summary>
        /// 报告深度
        /// </summary>
        public void ReportDepth(float depth)
        {
            if (currentPhase != TutorialPhase.DeepDive) return;
            
            if (depth >= targetDepth)
            {
                AdvancePhase(TutorialPhase.Complete);
            }
        }
        
        /// <summary>
        /// 跳过教学
        /// </summary>
        public void SkipTutorial()
        {
            currentPhase = TutorialPhase.Complete;
            CompleteTutorial();
        }
        
        private void Update()
        {
            // 检测深度变化
            if (currentPhase == TutorialPhase.DeepDive)
            {
                var diveManager = Core.GameManager.Instance?.diveManager;
                if (diveManager != null)
                {
                    ReportDepth(diveManager.CurrentDepth);
                }
            }
        }
        
        /// <summary>
        /// 获取当前阶段
        /// </summary>
        public TutorialPhase GetCurrentPhase()>
        {
            return currentPhase;
        }
        
        /// <summary>
        /// 是否正在进行教学
        /// </summary>
        public bool IsInTutorial => currentPhase != TutorialPhase.None && currentPhase != TutorialPhase.Complete;
    }
}
