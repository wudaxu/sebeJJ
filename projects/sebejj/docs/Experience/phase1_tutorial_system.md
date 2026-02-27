# Phase 1: 新手引导系统

## 概述

新手引导系统是玩家首次接触游戏时的重要体验环节。本系统采用**渐进式引导**设计，让玩家在实战中自然学习，避免强制教程的枯燥感。

## 设计原则

1. **边做边学** - 在真实游戏场景中教学
2. **渐进解锁** - 不一次性展示所有内容
3. **随时可跳过** - 尊重老玩家
4. **随时可回看** - 帮助面板随时可用

## 引导流程

```
首次进入游戏
    │
    ▼
┌─────────────────┐
│ 开场剧情动画    │ ──► 可选择跳过
│ (30秒)          │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ 基础操作教学    │ ──► WASD移动 + 鼠标瞄准
│ (基地安全区)    │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ 首次下潜引导    │ ──► 引导至第一个资源点
│ (浅海20米)      │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ 采集教学        │ ──► 采集第一个矿石
│                 │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ 返回基地        │ ──► 引导返回
│                 │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ 委托系统解锁    │ ──► 介绍委托板
│                 │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ 机甲配置教学    │ ──► 简单装备更换
│                 │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ 完成首个委托    │ ──► 实战巩固
│                 │
└────────┬────────┘
         │
         ▼
    引导完成，自由游戏
```

## 引导步骤详解

### Step 1: 开场剧情 (可选)

**触发条件**: 首次启动游戏
**内容**: 
- 2147年，地球资源枯竭
- 你是一名自由机甲驾驶员
- 简短的世界观介绍

**实现**:
```csharp
public class OpeningCinematicStep : TutorialStep
{
    public override string StepId => "opening_cinematic";
    public override bool CanSkip => true;
    
    public override void OnEnter()
    {
        // 播放开场动画
        CinematicPlayer.Play("Opening_Cinematic", OnCinematicEnd);
    }
}
```

### Step 2: 基础操作教学

**触发条件**: 完成开场或跳过后
**教学内容**:
| 操作 | 按键 | 说明 |
|------|------|------|
| 移动 | WASD | 机甲前后左右移动 |
| 瞄准 | 鼠标 | 机甲朝向鼠标位置 |
| 冲刺 | Shift | 快速移动，消耗能量 |
| 交互 | E | 与物体/NPC交互 |

**UI提示**:
- 屏幕中央显示操作提示
- 按键高亮动画
- 完成后淡出

### Step 3: 首次下潜引导

**触发条件**: 玩家走近下潜舱
**引导内容**:
1. 高亮显示下潜舱
2. 提示"按E进入下潜舱"
3. 进入后自动选择"新手试炼"委托
4. 引导至20米深度的安全区域

### Step 4: 采集教学

**触发条件**: 首次发现可采集资源
**引导内容**:
1. 高亮显示铜矿石
2. 提示"按住鼠标左键采集"
3. 显示采集进度条
4. 采集完成后提示"资源已存入背包"

### Step 5: 返回基地

**触发条件**: 背包满或玩家选择返回
**引导内容**:
1. 提示"按R启动紧急上浮"
2. 或引导至安全点"按E返回基地"
3. 返回后展示资源结算

### Step 6: 委托系统解锁

**触发条件**: 首次返回基地
**引导内容**:
1. 高亮委托板
2. 介绍委托类型图标
3. 推荐"新手试炼"委托
4. 说明奖励机制

### Step 7: 机甲配置教学

**触发条件**: 进入机甲工坊
**引导内容**:
1. 介绍装备槽位
2. 引导装备第一个钻头
3. 说明属性变化
4. 提示"更好的装备=更高的效率"

### Step 8: 完成首个委托

**触发条件**: 接受任意委托
**引导内容**:
- 不再强制引导，仅在关键点提示
- 委托目标高亮
- 完成时庆祝效果

## 系统解锁提示

随着游戏进度，逐步解锁系统：

| 等级/条件 | 解锁系统 | 提示方式 |
|-----------|----------|----------|
| 初始 | 基础移动、采集 | 新手引导 |
| 完成首个委托 | 委托系统完整功能 | 弹窗提示 |
| 达到10级 | 商店系统 | NPC对话 + 图标高亮 |
| 达到20级 | 模块系统 | 邮件通知 |
| 达到30级 | 深渊区域 | 剧情触发 |
| 首次击败Boss | 高级装备 | 掉落提示 |

## 帮助面板

### 功能

按 **H** 键随时打开帮助面板：

```
┌─────────────────────────────────────┐
│  ? 帮助中心                    [X]  │
├─────────────────────────────────────┤
│ [基础操作] [战斗技巧] [采集指南]     │
│ [委托说明] [机甲配置] [深度指南]     │
├─────────────────────────────────────┤
│                                     │
│  当前选中标签的内容...               │
│                                     │
│  - 操作说明                         │
│  - 图示演示                         │
│  - 快捷键列表                       │
│                                     │
├─────────────────────────────────────┤
│ [重新播放新手引导] [查看视频教程]    │
└─────────────────────────────────────┘
```

### 实现

```csharp
public class HelpPanel : MonoBehaviour
{
    [SerializeField] private TabGroup tabGroup;
    [SerializeField] private Button replayTutorialButton;
    
    private void OnEnable()
    {
        // 暂停游戏（可选）
        Time.timeScale = 0;
    }
    
    public void ReplayTutorial()
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
        TutorialManager.Instance.RestartTutorial();
    }
}
```

## 技术实现

### 核心类

```csharp
// TutorialManager.cs - 引导主管理器
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }
    
    [SerializeField] private TutorialConfig config;
    [SerializeField] private TutorialUI tutorialUI;
    
    private TutorialSaveData saveData;
    private TutorialStep currentStep;
    private Queue<TutorialStep> stepQueue;
    
    public bool IsTutorialActive { get; private set; }
    public bool IsFirstTimePlayer => !saveData.hasCompletedTutorial;
    
    private void Awake()
    {
        Instance = this;
        LoadSaveData();
    }
    
    private void Start()
    {
        if (IsFirstTimePlayer && config.EnableTutorial)
        {
            StartTutorial();
        }
    }
    
    public void StartTutorial()
    {
        IsTutorialActive = true;
        InitializeSteps();
        ProcessNextStep();
    }
    
    private void InitializeSteps()
    {
        stepQueue = new Queue<TutorialStep>();
        
        // 根据配置添加引导步骤
        foreach (var stepData in config.Steps)
        {
            if (!saveData.completedSteps.Contains(stepData.stepId))
            {
                var step = CreateStep(stepData);
                stepQueue.Enqueue(step);
            }
        }
    }
    
    private void ProcessNextStep()
    {
        if (stepQueue.Count == 0)
        {
            CompleteTutorial();
            return;
        }
        
        currentStep = stepQueue.Dequeue();
        currentStep.OnStepCompleted += OnStepCompleted;
        currentStep.OnEnter();
        
        tutorialUI.ShowStep(currentStep);
    }
    
    private void OnStepCompleted(TutorialStep step)
    {
        step.OnStepCompleted -= OnStepCompleted;
        saveData.completedSteps.Add(step.StepId);
        SaveData();
        
        ProcessNextStep();
    }
    
    public void SkipCurrentStep()
    {
        if (currentStep != null && currentStep.CanSkip)
        {
            currentStep.Skip();
        }
    }
    
    public void SkipAllTutorial()
    {
        IsTutorialActive = false;
        saveData.hasCompletedTutorial = true;
        SaveData();
        tutorialUI.HideAll();
    }
    
    private void CompleteTutorial()
    {
        IsTutorialActive = false;
        saveData.hasCompletedTutorial = true;
        SaveData();
        
        // 触发引导完成事件
        EventBus.Publish(new TutorialCompletedEvent());
    }
    
    private void LoadSaveData()
    {
        string json = PlayerPrefs.GetString("TutorialSaveData", "");
        if (string.IsNullOrEmpty(json))
        {
            saveData = new TutorialSaveData();
        }
        else
        {
            saveData = JsonUtility.FromJson<TutorialSaveData>(json);
        }
    }
    
    private void SaveData()
    {
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString("TutorialSaveData", json);
        PlayerPrefs.Save();
    }
}
```

### 引导步骤基类

```csharp
// TutorialStep.cs
public abstract class TutorialStep
{
    public abstract string StepId { get; }
    public abstract string DisplayName { get; }
    public abstract bool CanSkip { get; }
    
    public event System.Action<TutorialStep> OnStepCompleted;
    
    protected TutorialStepData Data { get; private set; }
    
    public void Initialize(TutorialStepData data)
    {
        Data = data;
    }
    
    public abstract void OnEnter();
    public abstract void OnExit();
    
    public virtual void Update() { }
    
    public void Skip()
    {
        OnExit();
        Complete();
    }
    
    protected void Complete()
    {
        OnStepCompleted?.Invoke(this);
    }
}
```

## 配置数据

```csharp
// TutorialConfig.cs (ScriptableObject)
[CreateAssetMenu(fileName = "TutorialConfig", menuName = "SebeJJ/Tutorial Config")]
public class TutorialConfig : ScriptableObject
{
    [Header("基础设置")]
    public bool EnableTutorial = true;
    public bool SkipOnSecondPlay = true;
    public bool PauseGameDuringTutorial = false;
    
    [Header("UI设置")]
    public GameObject TutorialUIPrefab;
    public GameObject HelpPanelPrefab;
    public float HighlightPulseSpeed = 1f;
    
    [Header("引导步骤")]
    public List<TutorialStepData> Steps;
}

[System.Serializable]
public class TutorialStepData
{
    public string stepId;
    public string displayName;
    public TutorialStepType stepType;
    public bool canSkip = true;
    public string description;
    public Sprite illustration;
    public List<string> prerequisites; // 前置步骤
}

public enum TutorialStepType
{
    Cinematic,      // 剧情动画
    Movement,       // 移动教学
    Interaction,    // 交互教学
    Collection,     // 采集教学
    Combat,         // 战斗教学
    UIExplain,      // UI说明
    FreePlay        // 自由练习
}
```

## 存档数据结构

```csharp
// TutorialSaveData.cs
[System.Serializable]
public class TutorialSaveData
{
    public bool hasCompletedTutorial;
    public List<string> completedSteps = new List<string>();
    public Dictionary<string, float> stepProgress = new Dictionary<string, float>();
    public int tutorialVersion = 1; // 用于版本兼容
}
```

## 事件系统

```csharp
// TutorialEvents.cs
public struct TutorialStepStartedEvent
{
    public string StepId;
    public string StepName;
}

public struct TutorialStepCompletedEvent
{
    public string StepId;
}

public struct TutorialCompletedEvent { }

public struct SystemUnlockedEvent
{
    public string SystemId;
    public string SystemName;
    public string Description;
}
```

## 测试要点

1. **首次进入流程** - 验证完整引导流程
2. **跳过功能** - 验证单个步骤跳过和全部跳过
3. **存档恢复** - 验证中断后恢复引导进度
4. **帮助面板** - 验证所有标签内容正确
5. **系统解锁** - 验证各系统解锁时机
6. **多语言** - 验证文本本地化

---

*文档版本: 1.0*
*最后更新: 2026-02-27*
