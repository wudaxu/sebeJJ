# Phase 3: 游戏节奏优化

## 概述

游戏节奏控制玩家的情绪起伏和体验流畅度。SebeJJ采用**张弛有度**的设计理念，在紧张战斗和放松探索之间找到平衡。

## 设计原则

1. **波浪式节奏** - 紧张-放松交替
2. **玩家主导** - 玩家决定何时战斗何时撤退
3. **正向反馈** - 行动后及时获得奖励
4. **可控风险** - 玩家能预判和选择风险等级

## 战斗与探索节奏

### 节奏模型

```
紧张度
   │
3.0├────╮              ╭────╮         ╭────── Boss战
   │    │  ╭────╮     │    │ ╭────╮  │
2.0├────┼──┤    ├─────┼────┼─┤    ├──┤
   │    │  │战斗│     │战斗│ │战斗│  │
1.0├────┼──┤    ├─────┼────┼─┤    ├──┘
   │    │  ╰────╯     ╰────╯ ╰────╯
0.5├────┤              ╭────╮
   │探索 │   探索       │安全│    探索
0.0┴────┴──────────────┴────┴──────────────► 时间
   准备   下潜    遭遇   休整   遭遇   遭遇   返回
```

### 战斗节奏控制

```csharp
public class CombatPacingController : MonoBehaviour
{
    [Header("战斗节奏参数")]
    public float minCombatDuration = 10f;      // 最短战斗时间
    public float maxCombatDuration = 60f;      // 最长战斗时间
    public float combatCooldown = 15f;         // 战斗间隔
    public float tensionBuildRate = 0.1f;      // 紧张度累积速度
    
    private float currentTension = 0f;         // 当前紧张度 (0-1)
    private float lastCombatTime = 0f;
    private bool inCombat = false;
    
    private void Update()
    {
        UpdateTension();
        ManageEnemySpawning();
    }
    
    private void UpdateTension()
    {
        if (inCombat)
        {
            // 战斗中紧张度维持高位
            currentTension = Mathf.Min(1f, currentTension + Time.deltaTime * 0.5f);
        }
        else
        {
            // 非战斗时紧张度逐渐下降
            currentTension = Mathf.Max(0f, currentTension - Time.deltaTime * tensionBuildRate);
        }
    }
    
    private void ManageEnemySpawning()
    {
        float timeSinceLastCombat = Time.time - lastCombatTime;
        
        // 根据紧张度和时间决定生成率
        float spawnChance = CalculateSpawnChance(timeSinceLastCombat);
        
        if (Random.value < spawnChance * Time.deltaTime)
        {
            SpawnEnemyEncounter();
        }
    }
    
    private float CalculateSpawnChance(float timeSinceLastCombat)
    {
        // 基础生成率随时间增加
        float timeFactor = Mathf.Clamp01(timeSinceLastCombat / combatCooldown);
        
        // 紧张度影响 (紧张度高时降低生成，给玩家喘息)
        float tensionFactor = 1f - currentTension * 0.5f;
        
        // 深度影响
        float depthFactor = GetDepthSpawnMultiplier();
        
        return 0.1f * timeFactor * tensionFactor * depthFactor;
    }
    
    public void OnCombatStart()
    {
        inCombat = true;
        currentTension = 0.7f;
        
        // 播放战斗音乐
        AudioManager.Instance.PlayMusic("Combat", fadeTime: 2f);
    }
    
    public void OnCombatEnd()
    {
        inCombat = false;
        lastCombatTime = Time.time;
        
        // 播放探索音乐
        AudioManager.Instance.PlayMusic("Exploration", fadeTime: 3f);
        
        // 给予短暂的安全时间
        ApplyCombatCooldown();
    }
}
```

### 探索区域设计

| 区域类型 | 功能 | 设计要点 |
|----------|------|----------|
| 安全区 | 无敌人，可休整 | 明显的视觉标识，资源稀少 |
| 低风险区 | 少量弱敌人 | 适合新手练习，基础资源 |
| 中风险区 | 中等敌人密度 | 主要探索区域，平衡设计 |
| 高风险区 | 大量强敌 | 高价值资源，需要准备 |
| Boss区 | Boss战 | 独特地形，机制挑战 |

## 委托之间的缓冲

### 缓冲设计

```csharp
public class MissionBufferSystem : MonoBehaviour
{
    [Header("缓冲配置")]
    public float minBufferTime = 30f;          // 最短缓冲时间
    public float recommendedBufferTime = 60f;   // 推荐缓冲时间
    
    public void OnMissionComplete(MissionResult result)
    {
        // 1. 结算动画 (5-10秒)
        PlayRewardAnimation(result);
        
        // 2. 数据统计展示 (可选)
        ShowMissionStats(result);
        
        // 3. 推荐下一步行动
        SuggestNextAction(result);
        
        // 4. 记录完成时间
        RecordSessionPace(result);
    }
    
    private void SuggestNextAction(MissionResult result)
    {
        var suggestion = AnalyzePlayerState();
        
        switch (suggestion)
        {
            case PlayerState.Tired:
                ShowMessage("您已连续执行了多个委托，建议返回基地休整。");
                HighlightReturnOption();
                break;
            case PlayerState.LowResources:
                ShowMessage("机甲状态良好，但资源背包已满。");
                HighlightReturnOption();
                break;
            case PlayerState.Damaged:
                ShowMessage("机甲受损严重，建议先进行维修。");
                HighlightRepairOption();
                break;
            case PlayerState.Ready:
                ShowMessage("状态良好！是否接受下一个委托？");
                ShowNextMissionRecommendations();
                break;
        }
    }
}
```

### 推荐流程

```
完成委托
    │
    ▼
┌─────────────────┐
│ 奖励结算动画    │ ──► 5-10秒，满足成就感
│                 │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│ 状态检查        │ ──► 自动评估机甲状态
│                 │
└────────┬────────┘
         │
    ┌────┴────┐
    ▼         ▼
 需要休整    状态良好
    │         │
    ▼         ▼
┌────────┐  ┌────────┐
│返回基地│  │推荐委托│
│维修/售卖│  │难度匹配│
└────────┘  └────────┘
```

## 奖励发放时机

### 即时奖励

```csharp
public class RewardTimingSystem : MonoBehaviour
{
    [Header("奖励时机配置")]
    public bool enableImmediateFeedback = true;
    public float resourcePopupDuration = 2f;
    public float comboWindow = 5f;
    
    // 采集奖励
    public void OnResourceCollected(ResourceData resource, int amount)
    {
        // 1. 即时视觉反馈
        ShowCollectionPopup(resource, amount);
        
        // 2. 音效反馈
        AudioManager.Instance.PlaySFX("Resource_Collect", pitch: 1f + amount * 0.05f);
        
        // 3. 连击系统
        UpdateCollectionCombo(resource);
        
        // 4. 进度更新
        UpdateCollectionProgress(resource, amount);
    }
    
    // 战斗奖励
    public void OnEnemyDefeated(Enemy enemy)
    {
        // 1. 即时掉落
        DropLoot(enemy);
        
        // 2. 经验值弹出
        ShowXPPopup(enemy.XPReward);
        
        // 3. 连杀奖励
        UpdateKillStreak();
        
        // 4. 战斗统计
        UpdateCombatStats(enemy);
    }
    
    // 里程碑奖励
    public void OnMilestoneReached(MilestoneType type)
    {
        switch (type)
        {
            case MilestoneType.FirstDive:
                ShowMilestoneCelebration("首次下潜完成！", bonus: 100);
                break;
            case MilestoneType.DepthRecord:
                ShowMilestoneCelebration("新深度记录！", bonus: 500);
                UnlockNewContent();
                break;
            case MilestoneType.CollectionComplete:
                ShowMilestoneCelebration("委托完成！", showStats: true);
                break;
        }
    }
}
```

### 奖励节奏表

| 行为 | 即时反馈 | 短期奖励 | 长期奖励 |
|------|----------|----------|----------|
| 采集资源 | 弹出+音效 | 背包显示 | 出售获得信用点 |
| 击败敌人 | 掉落+经验 | 击杀统计 | 解锁新区域 |
| 发现新区域 | 地图标记 | 探索奖励 | 成就解锁 |
| 完成委托 | 结算动画 | 信用点+经验 | 新委托解锁 |
| 升级机甲 | 属性提升 | 能力增强 | 新装备槽位 |

## 存档点分布

### 存档策略

SebeJJ采用**检查点 + 自动存档**混合策略：

```csharp
public class SavePointSystem : MonoBehaviour
{
    [Header("存档配置")]
    public float autoSaveInterval = 60f;       // 自动存档间隔
    public bool saveOnDepthChange = true;      // 深度变化时存档
    public bool saveOnSafeZone = true;         // 进入安全区存档
    
    public enum SaveType
    {
        AutoSave,       // 自动存档 (背景进行)
        Checkpoint,     // 检查点存档 (下潜前)
        SafeZone,       // 安全区存档
        Manual          // 手动存档
    }
    
    public void CreateSavePoint(SaveType type, Vector3 position)
    {
        var saveData = new SavePointData
        {
            type = type,
            position = position,
            depth = DiveManager.Instance.CurrentDepth,
            timestamp = DateTime.Now,
            playerState = CapturePlayerState(),
            inventory = CaptureInventory(),
            missionProgress = CaptureMissionProgress()
        };
        
        SaveManager.Instance.WriteSavePoint(saveData);
        
        // 视觉反馈
        if (type == SaveType.Checkpoint || type == SaveType.SafeZone)
        {
            ShowSaveIndicator(type);
        }
    }
    
    private void ShowSaveIndicator(SaveType type)
    {
        string message = type switch
        {
            SaveType.Checkpoint => "☑ 已创建检查点",
            SaveType.SafeZone => "☑ 已存档 (安全区)",
            SaveType.AutoSave => "☑ 自动存档",
            _ => "☑ 已存档"
        };
        
        UIManager.Instance.ShowToast(message, duration: 2f);
    }
}
```

### 存档点分布图

```
深度
 ^
100│                                    [Boss前检查点]
   │                                         │
90 │                               [安全区]  │
   │                                  │      │
80 │                        [检查点]  │      │
   │                           │      │      │
70 │                 [安全区]  │      │      │
   │                    │      │      │      │
60 │           [检查点] │      │      │      │
   │              │     │      │      │      │
50 │    [安全区]  │     │      │      │      │
   │       │      │     │      │      │      │
40 │ [检查点]     │     │      │      │      │
   │    │         │     │      │      │      │
30 │    │  [安全区]     │      │      │      │
   │    │     │        │      │      │      │
20 │    │     │  [检查点]     │      │      │
   │    │     │     │        │      │      │
10 │    │     │     │  [安全区]     │      │
   │    │     │     │     │        │      │
0  ├────┴─────┴─────┴─────┴────────┴──────┴──►
  基地 [检查点] [存档] [存档] [存档] [存档] [存档]

图例:
[检查点] - 自动存档点
[安全区] - 可手动存档的安全区域
[Boss前检查点] - Boss战前强制存档
```

### 死亡恢复机制

```csharp
public class DeathRecoverySystem : MonoBehaviour
{
    public void OnPlayerDeath()
    {
        // 1. 确定重生点
        var respawnPoint = DetermineRespawnPoint();
        
        // 2. 应用死亡惩罚
        DeathPenaltySystem.Instance.ApplyDeathPenalty();
        
        // 3. 恢复状态
        RestorePlayerState(respawnPoint);
        
        // 4. 显示死亡报告
        ShowDeathReport();
    }
    
    private SavePointData DetermineRespawnPoint()
    {
        // 优先使用最近的检查点
        var checkpoint = SaveManager.Instance.GetNearestCheckpoint();
        
        // 如果没有检查点，返回基地
        if (checkpoint == null)
        {
            return SaveManager.Instance.GetBaseSavePoint();
        }
        
        return checkpoint;
    }
}
```

## 心流状态维护

### 心流通道模型

```
挑战
 ^
高│    焦虑区              ╭─────────── 心流通道
  │                  ╭────╯
中│             ╭────╯
  │        ╭────╯              ╭──── 无聊区
低│   ╭────╯              ╭────╯
  │╭──╯              ╭────╯
  ├──────────────────────────────────► 技能
    低              中              高
```

### 动态调整实现

```csharp
public class FlowStateManager : MonoBehaviour
{
    [Header("心流维护")]
    public float evaluationInterval = 60f;
    public float targetChallengeLevel = 0.7f; // 目标挑战度
    
    private PlayerState currentState = PlayerState.Neutral;
    
    private void EvaluatePlayerState()
    {
        // 收集指标
        float deathRate = GetRecentDeathRate();
        float completionRate = GetMissionCompletionRate();
        float averageSessionTime = GetAverageSessionTime();
        float playerEngagement = GetPlayerEngagementScore();
        
        // 判断状态
        if (deathRate > 0.6f && completionRate < 0.3f)
        {
            currentState = PlayerState.Frustrated; // 焦虑区
            ApplyDifficultyReduction();
        }
        else if (deathRate < 0.1f && completionRate > 0.9f)
        {
            currentState = PlayerState.Bored; // 无聊区
            ApplyDifficultyIncrease();
        }
        else
        {
            currentState = PlayerState.Flow; // 心流区
            MaintainCurrentDifficulty();
        }
        
        // 记录分析
        AnalyticsManager.Log("flow_state", new Dictionary<string, object>
        {
            { "state", currentState.ToString() },
            { "death_rate", deathRate },
            { "completion_rate", completionRate }
        });
    }
    
    private void ApplyDifficultyReduction()
    {
        // 降低敌人强度
        DifficultyManager.Instance.AdjustEnemyMultiplier(0.9f);
        
        // 增加资源生成
        ResourceManager.Instance.AdjustSpawnRate(1.2f);
        
        // 显示鼓励提示
        UIManager.Instance.ShowHint("别担心，慢慢来，你会越来越强的！");
    }
    
    private void ApplyDifficultyIncrease()
    {
        // 增加精英敌人
        EnemySpawnManager.Instance.IncreaseEliteChance(0.1f);
        
        // 推荐更高难度委托
        MissionManager.Instance.SuggestHigherDifficultyMissions();
        
        // 显示挑战提示
        UIManager.Instance.ShowHint("你看起来很强！要不要试试更高难度的委托？");
    }
}
```

## 技术实现

### PacingManager.cs

```csharp
public class PacingManager : MonoBehaviour
{
    public static PacingManager Instance { get; private set; }
    
    [Header("节奏控制")]
    public float sessionTargetDuration = 1200f; // 20分钟目标时长
    public float combatRatio = 0.4f;            // 战斗占比目标
    public float explorationRatio = 0.5f;       // 探索占比目标
    public float restRatio = 0.1f;              // 休整占比目标
    
    private SessionPaceData currentSession;
    
    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        StartNewSession();
    }
    
    public void StartNewSession()
    {
        currentSession = new SessionPaceData
        {
            startTime = DateTime.Now,
            combatTime = 0f,
            explorationTime = 0f,
            restTime = 0f
        };
    }
    
    private void Update()
    {
        UpdateSessionMetrics();
        AdjustPacingIfNeeded();
    }
    
    private void UpdateSessionMetrics()
    {
        float deltaTime = Time.deltaTime;
        
        if (CombatManager.Instance.IsInCombat)
        {
            currentSession.combatTime += deltaTime;
        }
        else if (PlayerController.Instance.IsMoving)
        {
            currentSession.explorationTime += deltaTime;
        }
        else
        {
            currentSession.restTime += deltaTime;
        }
    }
    
    private void AdjustPacingIfNeeded()
    {
        float totalTime = currentSession.GetTotalTime();
        if (totalTime < 60f) return; // 至少1分钟后调整
        
        float actualCombatRatio = currentSession.combatTime / totalTime;
        float actualExplorationRatio = currentSession.explorationTime / totalTime;
        
        // 战斗太多，增加探索
        if (actualCombatRatio > combatRatio + 0.1f)
        {
            CombatPacingController.Instance.ReduceEncounterRate();
            ResourceManager.Instance.IncreaseResourceVisibility();
        }
        
        // 探索太多，增加战斗
        if (actualExplorationRatio > explorationRatio + 0.1f)
        {
            CombatPacingController.Instance.IncreaseEncounterRate();
        }
    }
    
    public SessionPaceData GetCurrentSessionData()
    {
        return currentSession;
    }
}

[System.Serializable]
public class SessionPaceData
{
    public DateTime startTime;
    public float combatTime;
    public float explorationTime;
    public float restTime;
    public int enemiesDefeated;
    public int resourcesCollected;
    public int missionsCompleted;
    
    public float GetTotalTime()
    {
        return combatTime + explorationTime + restTime;
    }
    
    public float GetPaceScore()
    {
        // 计算节奏分数 (0-1)
        float combatRatio = combatTime / GetTotalTime();
        float explorationRatio = explorationTime / GetTotalTime();
        
        // 理想: 40%战斗, 50%探索, 10%休整
        float combatScore = 1f - Mathf.Abs(combatRatio - 0.4f);
        float explorationScore = 1f - Mathf.Abs(explorationRatio - 0.5f);
        
        return (combatScore + explorationScore) / 2f;
    }
}
```

## 测试与验证

### 节奏测试指标

| 指标 | 目标值 | 测试方法 |
|------|--------|----------|
| 平均会话时长 | 15-25分钟 | 统计10次游戏 |
| 战斗/探索比例 | 40:50:10 | 时间占比分析 |
| 连续死亡挫败率 | < 30% | 玩家问卷 |
| 无聊退出率 | < 10% | 统计早期退出 |
| 心流体验评分 | > 4.0/5 | 玩家反馈 |

### 数据收集

```csharp
public class PacingAnalytics
{
    public void RecordSessionEnd(SessionPaceData data)
    {
        AnalyticsManager.Log("session_end", new Dictionary<string, object>
        {
            { "duration", data.GetTotalTime() },
            { "combat_ratio", data.combatTime / data.GetTotalTime() },
            { "exploration_ratio", data.explorationTime / data.GetTotalTime() },
            { "pace_score", data.GetPaceScore() },
            { "enemies_defeated", data.enemiesDefeated },
            { "resources_collected", data.resourcesCollected },
            { "missions_completed", data.missionsCompleted }
        });
    }
}
```

---

*文档版本: 1.0*
*最后更新: 2026-02-27*
