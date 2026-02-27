using System;
using System.Collections.Generic;
using UnityEngine;

namespace SebeJJ.Experience.Tutorial
{
    /// <summary>
    /// 新手引导主管理器 - 控制整个引导流程
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager Instance { get; private set; }
        
        [Header("基础配置")]
        [SerializeField] private TutorialConfig config;
        [SerializeField] private TutorialUI tutorialUI;
        
        [Header("事件")]
        public static event Action<TutorialStep> OnStepStarted;
        public static event Action<TutorialStep> OnStepCompleted;
        public static event Action OnTutorialCompleted;
        public static event Action<string> OnSystemUnlocked;
        
        private TutorialSaveData saveData;
        private TutorialStep currentStep;
        private Queue<TutorialStep> stepQueue;
        private Dictionary<string, SystemUnlockData> unlockedSystems;
        
        public bool IsTutorialActive { get; private set; }
        public bool IsFirstTimePlayer => !saveData.hasCompletedTutorial;
        public TutorialStep CurrentStep => currentStep;
        
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            LoadSaveData();
            unlockedSystems = new Dictionary<string, SystemUnlockData>();
        }
        
        private void Start()
        {
            if (tutorialUI == null)
            {
                tutorialUI = FindObjectOfType<TutorialUI>();
            }
            
            // 检查是否应该启动引导
            if (ShouldStartTutorial())
            {
                StartTutorial();
            }
        }
        
        private bool ShouldStartTutorial()
        {
            if (!config.EnableTutorial) return false;
            if (saveData.hasCompletedTutorial && config.SkipOnSecondPlay) return false;
            return true;
        }
        
        /// <summary>
        /// 开始新手引导
        /// </summary>
        public void StartTutorial()
        {
            if (IsTutorialActive) return;
            
            IsTutorialActive = true;
            InitializeSteps();
            
            if (stepQueue.Count > 0)
            {
                ProcessNextStep();
            }
            else
            {
                CompleteTutorial();
            }
        }
        
        /// <summary>
        /// 重新开始引导（从帮助面板调用）
        /// </summary>
        public void RestartTutorial()
        {
            // 重置存档
            saveData = new TutorialSaveData();
            SaveData();
            
            // 清空队列
            stepQueue?.Clear();
            currentStep = null;
            
            // 重新开始
            StartTutorial();
        }
        
        private void InitializeSteps()
        {
            stepQueue = new Queue<TutorialStep>();
            
            foreach (var stepData in config.Steps)
            {
                // 检查是否已完成
                if (saveData.completedSteps.Contains(stepData.stepId))
                    continue;
                
                // 检查前置条件
                if (!CheckPrerequisites(stepData))
                    continue;
                
                // 创建步骤实例
                var step = CreateStep(stepData);
                if (step != null)
                {
                    stepQueue.Enqueue(step);
                }
            }
        }
        
        private bool CheckPrerequisites(TutorialStepData stepData)
        {
            if (stepData.prerequisites == null || stepData.prerequisites.Count == 0)
                return true;
            
            foreach (var prereq in stepData.prerequisites)
            {
                if (!saveData.completedSteps.Contains(prereq))
                    return false;
            }
            return true;
        }
        
        private TutorialStep CreateStep(TutorialStepData data)
        {
            TutorialStep step = data.stepType switch
            {
                TutorialStepType.Cinematic => new OpeningCinematicStep(),
                TutorialStepType.Movement => new MovementTutorialStep(),
                TutorialStepType.Interaction => new InteractionTutorialStep(),
                TutorialStepType.Collection => new CollectionTutorialStep(),
                TutorialStepType.Combat => new CombatTutorialStep(),
                TutorialStepType.UIExplain => new UIExplainStep(),
                TutorialStepType.FreePlay => new FreePlayStep(),
                _ => null
            };
            
            step?.Initialize(data);
            return step;
        }
        
        private void ProcessNextStep()
        {
            if (stepQueue.Count == 0)
            {
                CompleteTutorial();
                return;
            }
            
            currentStep = stepQueue.Dequeue();
            currentStep.OnStepCompleted += OnStepCompletedHandler;
            currentStep.OnEnter();
            
            tutorialUI?.ShowStep(currentStep);
            OnStepStarted?.Invoke(currentStep);
            
            // 暂停游戏（如果配置允许）
            if (config.PauseGameDuringTutorial && currentStep.ShouldPauseGame)
            {
                Time.timeScale = 0;
            }
        }
        
        private void OnStepCompletedHandler(TutorialStep step)
        {
            step.OnStepCompleted -= OnStepCompletedHandler;
            
            // 记录完成
            if (!saveData.completedSteps.Contains(step.StepId))
            {
                saveData.completedSteps.Add(step.StepId);
            }
            SaveData();
            
            // 恢复游戏
            if (config.PauseGameDuringTutorial)
            {
                Time.timeScale = 1;
            }
            
            OnStepCompleted?.Invoke(step);
            
            // 继续下一步
            ProcessNextStep();
        }
        
        /// <summary>
        /// 跳过当前步骤
        /// </summary>
        public void SkipCurrentStep()
        {
            if (currentStep != null && currentStep.CanSkip)
            {
                currentStep.Skip();
            }
        }
        
        /// <summary>
        /// 跳过所有引导
        /// </summary>
        public void SkipAllTutorial()
        {
            IsTutorialActive = false;
            saveData.hasCompletedTutorial = true;
            SaveData();
            
            tutorialUI?.HideAll();
            
            // 恢复游戏
            Time.timeScale = 1;
        }
        
        private void CompleteTutorial()
        {
            IsTutorialActive = false;
            saveData.hasCompletedTutorial = true;
            SaveData();
            
            tutorialUI?.ShowCompletionCelebration();
            OnTutorialCompleted?.Invoke();
            
            Debug.Log("[TutorialManager] 新手引导完成！");
        }
        
        /// <summary>
        /// 解锁系统提示
        /// </summary>
        public void UnlockSystem(string systemId, string systemName, string description, Sprite icon = null)
        {
            if (unlockedSystems.ContainsKey(systemId))
                return;
            
            var unlockData = new SystemUnlockData
            {
                systemId = systemId,
                systemName = systemName,
                description = description,
                icon = icon,
                unlockTime = DateTime.Now
            };
            
            unlockedSystems.Add(systemId, unlockData);
            
            // 显示解锁提示
            tutorialUI?.ShowSystemUnlock(unlockData);
            OnSystemUnlocked?.Invoke(systemId);
            
            // 记录解锁
            if (!saveData.unlockedSystems.Contains(systemId))
            {
                saveData.unlockedSystems.Add(systemId);
                SaveData();
            }
        }
        
        /// <summary>
        /// 检查系统是否已解锁
        /// </summary>
        public bool IsSystemUnlocked(string systemId)
        {
            return unlockedSystems.ContainsKey(systemId) || saveData.unlockedSystems.Contains(systemId);
        }
        
        /// <summary>
        /// 获取引导进度 (0-1)
        /// </summary>
        public float GetTutorialProgress()
        {
            if (config.Steps.Count == 0) return 1f;
            return (float)saveData.completedSteps.Count / config.Steps.Count;
        }
        
        #region 存档管理
        
        private void LoadSaveData()
        {
            string json = PlayerPrefs.GetString("TutorialSaveData_v2", "");
            if (string.IsNullOrEmpty(json))
            {
                saveData = new TutorialSaveData();
            }
            else
            {
                saveData = JsonUtility.FromJson<TutorialSaveData>(json);
            }
            
            // 恢复已解锁系统
            if (saveData.unlockedSystems != null)
            {
                foreach (var systemId in saveData.unlockedSystems)
                {
                    unlockedSystems[systemId] = new SystemUnlockData { systemId = systemId };
                }
            }
        }
        
        private void SaveData()
        {
            string json = JsonUtility.ToJson(saveData);
            PlayerPrefs.SetString("TutorialSaveData_v2", json);
            PlayerPrefs.Save();
        }
        
        /// <summary>
        /// 重置所有引导数据（调试用）
        /// </summary>
        [ContextMenu("Reset Tutorial Data")]
        public void ResetTutorialData()
        {
            saveData = new TutorialSaveData();
            PlayerPrefs.DeleteKey("TutorialSaveData_v2");
            PlayerPrefs.Save();
            Debug.Log("[TutorialManager] 引导数据已重置");
        }
        
        #endregion
    }
    
    /// <summary>
    /// 系统解锁数据
    /// </summary>
    [System.Serializable]
    public class SystemUnlockData
    {
        public string systemId;
        public string systemName;
        public string description;
        public Sprite icon;
        public DateTime unlockTime;
    }
}
