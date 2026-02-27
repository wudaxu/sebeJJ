using System;
using System.Collections.Generic;
using UnityEngine;

namespace SebeJJ.Experience.Tutorial
{
    /// <summary>
    /// 引导存档数据
    /// </summary>
    [System.Serializable]
    public class TutorialSaveData
    {
        public bool hasCompletedTutorial;
        public List<string> completedSteps = new List<string>();
        public List<string> unlockedSystems = new List<string>();
        public Dictionary<string, float> stepProgress = new Dictionary<string, float>();
        public int tutorialVersion = 2; // 用于版本兼容
        public DateTime firstPlayTime;
        public DateTime lastPlayTime;
        
        public TutorialSaveData()
        {
            firstPlayTime = DateTime.Now;
            lastPlayTime = DateTime.Now;
        }
    }
    
    /// <summary>
    /// 引导配置 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "TutorialConfig", menuName = "SebeJJ/Tutorial Config")]
    public class TutorialConfig : ScriptableObject
    {
        [Header("基础设置")]
        [Tooltip("是否启用新手引导")]
        public bool EnableTutorial = true;
        
        [Tooltip("二周目是否跳过引导")]
        public bool SkipOnSecondPlay = true;
        
        [Tooltip("引导期间是否暂停游戏")]
        public bool PauseGameDuringTutorial = false;
        
        [Header("UI设置")]
        [Tooltip("引导UI预制体")]
        public GameObject TutorialUIPrefab;
        
        [Tooltip("帮助面板预制体")]
        public GameObject HelpPanelPrefab;
        
        [Tooltip("高亮脉冲速度")]
        public float HighlightPulseSpeed = 1f;
        
        [Tooltip("高亮颜色")]
        public Color HighlightColor = new Color(0, 1, 1, 0.5f);
        
        [Header("引导步骤")]
        [Tooltip("引导步骤列表")]
        public List<TutorialStepData> Steps = new List<TutorialStepData>();
        
        [Header("系统解锁配置")]
        [Tooltip("系统解锁提示列表")]
        public List<SystemUnlockConfig> SystemUnlocks = new List<SystemUnlockConfig>();
        
        private void OnValidate()
        {
            // 确保步骤ID唯一
            var idSet = new HashSet<string>();
            foreach (var step in Steps)
            {
                if (!idSet.Add(step.stepId))
                {
                    Debug.LogWarning($"[TutorialConfig] 发现重复的步骤ID: {step.stepId}");
                }
            }
        }
    }
    
    /// <summary>
    /// 引导步骤数据
    /// </summary>
    [System.Serializable]
    public class TutorialStepData
    {
        [Tooltip("步骤唯一ID")]
        public string stepId;
        
        [Tooltip("步骤显示名称")]
        public string displayName;
        
        [Tooltip("步骤类型")]
        public TutorialStepType stepType;
        
        [Tooltip("是否可以跳过")]
        public bool canSkip = true;
        
        [Tooltip("步骤描述")]
        [TextArea(3, 5)]
        public string description;
        
        [Tooltip("说明图片")]
        public Sprite illustration;
        
        [Tooltip("前置步骤ID列表")]
        public List<string> prerequisites = new List<string>();
        
        [Tooltip("自动进入下一步的延迟时间（秒），0表示手动")]
        public float autoAdvanceDelay = 0;
        
        [Tooltip("步骤目标（如采集数量、击败敌人数量）")]
        public int targetAmount = 1;
    }
    
    /// <summary>
    /// 引导步骤类型
    /// </summary>
    public enum TutorialStepType
    {
        [Tooltip("剧情动画")]
        Cinematic,
        
        [Tooltip("移动教学")]
        Movement,
        
        [Tooltip("交互教学")]
        Interaction,
        
        [Tooltip("采集教学")]
        Collection,
        
        [Tooltip("战斗教学")]
        Combat,
        
        [Tooltip("UI说明")]
        UIExplain,
        
        [Tooltip("自由练习")]
        FreePlay,
        
        [Tooltip("委托系统")]
        MissionSystem,
        
        [Tooltip("机甲配置")]
        MechConfig,
        
        [Tooltip("商店系统")]
        ShopSystem,
        
        [Tooltip("自定义")]
        Custom
    }
    
    /// <summary>
    /// 系统解锁配置
    /// </summary>
    [System.Serializable]
    public class SystemUnlockConfig
    {
        [Tooltip("系统ID")]
        public string systemId;
        
        [Tooltip("系统名称")]
        public string systemName;
        
        [Tooltip("解锁描述")]
        [TextArea(2, 3)]
        public string description;
        
        [Tooltip("解锁图标")]
        public Sprite icon;
        
        [Tooltip("解锁等级要求")]
        public int requiredLevel;
        
        [Tooltip("解锁所需委托ID")]
        public string requiredMissionId;
        
        [Tooltip("解锁所需深度")]
        public float requiredDepth;
    }
}
