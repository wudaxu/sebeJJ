# Week 2 系统交互流程设计

## 1. 系统交互总览

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              Week 2 系统交互图                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│   ┌──────────────┐      ┌──────────────┐      ┌──────────────┐             │
│   │  MissionSystem│◄────►│  GameManager │◄────►│ResourceSystem│             │
│   └──────┬───────┘      └──────┬───────┘      └──────┬───────┘             │
│          │                     │                     │                      │
│          │    ┌────────────────┼────────────────┐    │                      │
│          │    │                │                │    │                      │
│          ▼    ▼                ▼                ▼    ▼                      │
│   ┌──────────────┐      ┌──────────────┐      ┌──────────────┐             │
│   │   UISystem   │◄────►│   SaveManager│◄────►│  MechSystem  │             │
│   └──────────────┘      └──────────────┘      └──────────────┘             │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 2. 核心交互流程

### 2.1 委托接受流程

```
┌─────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│  玩家   │────►│   UI点击     │────►│MissionSystem │────►│  验证条件    │
│         │     │  接受委托    │     │AcceptMission │     │ (机甲/资源)  │
└─────────┘     └──────────────┘     └──────┬───────┘     └──────┬───────┘
                                            │                    │
                                            ▼                    ▼
                                     ┌──────────────┐     ┌──────────────┐
                                     │  触发事件    │◄────│  条件通过    │
                                     │MissionStarted│     │              │
                                     └──────┬───────┘     └──────────────┘
                                            │
                                            ▼
                                     ┌──────────────┐
                                     │ SaveManager  │
                                     │  自动存档    │
                                     └──────────────┘
```

**代码实现**:
```csharp
public class MissionSystem : MonoBehaviour {
    [SerializeField] private MechSystem _mechSystem;
    [SerializeField] private ResourceSystem _resourceSystem;
    
    public bool AcceptMission(MissionData mission) {
        // 1. 验证机甲状态
        if (!_mechSystem.IsMechReady()) {
            UIManager.ShowMessage("机甲未准备就绪");
            return false;
        }
        
        // 2. 验证资源要求
        if (!HasRequiredResources(mission.requiredResources)) {
            UIManager.ShowMessage("缺少必要资源");
            return false;
        }
        
        // 3. 设置当前委托
        CurrentMission = mission;
        
        // 4. 触发事件
        EventBus.Trigger(new MissionStartedEvent {
            MissionId = mission.missionId,
            TargetDepth = mission.targetDepth
        });
        
        // 5. 自动存档
        SaveManager.AutoSave();
        
        return true;
    }
}
```

---

### 2.2 资源采集流程

```
┌─────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│  玩家   │────►│  接近资源    │────►│  检测交互    │────►│  检查背包    │
│         │     │  按交互键    │     │  按键触发    │     │  容量/重量   │
└─────────┘     └──────────────┘     └──────┬───────┘     └──────┬───────┘
                                            │                    │
                                            ▼                    ▼
                                     ┌──────────────┐     ┌──────────────┐
                                     │  容量不足    │     │  容量充足    │
                                     │  提示UI      │     │              │
                                     └──────────────┘     └──────┬───────┘
                                                                 │
                                                                 ▼
                                                          ┌──────────────┐
                                                          │ResourceSystem│
                                                          │  AddItem()   │
                                                          └──────┬───────┘
                                                                 │
                                                                 ▼
                                                          ┌──────────────┐
                                                          │  触发事件    │
                                                          │ResourceCollected
                                                          └──────┬───────┘
                                                                 │
                                                                 ▼
                                                          ┌──────────────┐
                                                          │MissionSystem │
                                                          │ 更新委托进度 │
                                                          └──────────────┘
```

**代码实现**:
```csharp
public class ResourceSystem : MonoBehaviour {
    [SerializeField] private Inventory _inventory;
    [SerializeField] private MissionSystem _missionSystem;
    
    public CollectResult TryCollectResource(ResourceNode resourceNode) {
        var resource = resourceNode.ResourceData;
        
        // 1. 检查背包容量
        if (!_inventory.CanAddItem(resource, 1)) {
            return CollectResult.InventoryFull;
        }
        
        // 2. 计算采集时间
        float collectTime = CalculateCollectTime(resource);
        
        // 3. 开始采集（可打断）
        StartCoroutine(CollectCoroutine(resourceNode, collectTime));
        
        return CollectResult.Success;
    }
    
    private IEnumerator CollectCoroutine(ResourceNode node, float time) {
        // 显示进度条
        UIManager.ShowProgressBar("采集中...", time);
        
        yield return new WaitForSeconds(time);
        
        // 添加到背包
        _inventory.AddItem(node.ResourceData, 1);
        
        // 触发事件
        EventBus.Trigger(new ResourceCollectedEvent {
            ResourceId = node.ResourceData.resourceId,
            Amount = 1
        });
        
        // 销毁资源节点
        node.Collect();
    }
}
```

---

### 2.3 委托完成流程

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│  目标达成    │────►│MissionSystem │────►│  计算奖励    │────►│  发放奖励    │
│ (采集/探索)  │     │CompleteMission│     │ (信用点/资源)│     │              │
└──────────────┘     └──────┬───────┘     └──────────────┘     └──────┬───────┘
                            │                                         │
                            ▼                                         ▼
                     ┌──────────────┐                         ┌──────────────┐
                     │  触发事件    │                         │  更新玩家    │
                     │MissionCompleted                        │  数据        │
                     └──────┬───────┘                         └──────┬───────┘
                            │                                         │
                            ▼                                         ▼
                     ┌──────────────┐                         ┌──────────────┐
                     │   UISystem   │                         │ SaveManager  │
                     │  显示结算    │                         │  存档        │
                     └──────────────┘                         └──────────────┘
```

**代码实现**:
```csharp
public class MissionSystem : MonoBehaviour {
    [SerializeField] private ResourceSystem _resourceSystem;
    [SerializeField] private PlayerData _playerData;
    
    public void CompleteMission(bool success) {
        if (CurrentMission == null) return;
        
        var mission = CurrentMission;
        
        if (success) {
            // 1. 计算奖励
            var reward = CalculateReward(mission);
            
            // 2. 发放奖励
            _playerData.Credits += reward.credits;
            foreach (var item in reward.items) {
                _resourceSystem.AddToStorage(item);
            }
            _playerData.AddExperience(reward.exp);
            
            // 3. 记录完成
            _playerData.CompletedMissions.Add(mission.missionId);
        }
        
        // 4. 触发事件
        EventBus.Trigger(new MissionCompletedEvent {
            MissionId = mission.missionId,
            Success = success,
            Reward = reward
        });
        
        // 5. 显示结算UI
        UIManager.ShowMissionResult(mission, success, reward);
        
        // 6. 清理当前委托
        CurrentMission = null;
        
        // 7. 存档
        SaveManager.Save();
    }
}
```

---

## 3. 数据流设计

### 3.1 运行时数据流

```
┌─────────────────────────────────────────────────────────────────┐
│                        运行时数据流                              │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐         │
│  │ Scriptable  │───►│   Runtime   │───►│    Save     │         │
│  │   Object    │    │   Instance  │    │    Data     │         │
│  │  (配置数据)  │    │  (运行数据)  │    │  (存档数据)  │         │
│  └─────────────┘    └──────┬──────┘    └─────────────┘         │
│                            │                                    │
│                            ▼                                    │
│                     ┌─────────────┐                             │
│                     │   EventBus  │                             │
│                     │  (事件通知)  │                             │
│                     └──────┬──────┘                             │
│                            │                                    │
│              ┌─────────────┼─────────────┐                      │
│              ▼             ▼             ▼                      │
│       ┌──────────┐  ┌──────────┐  ┌──────────┐                 │
│       │MissionSystem│ │ResourceSystem│ │ UISystem  │                 │
│       └──────────┘  └──────────┘  └──────────┘                 │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### 3.2 存档数据流

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   游戏运行    │────►│ SaveManager  │────►│  JSON文件    │
│              │     │  CollectData │     │  (持久化)    │
└──────────────┘     └──────────────┘     └──────────────┘
                            │
                            ▼
                     ┌──────────────┐
                     │  SaveData    │
                     │  ├─ Player   │
                     │  ├─ Mech     │
                     │  ├─ Inventory│
                     │  ├─ Missions │
                     │  └─ Settings │
                     └──────────────┘
```

---

## 4. 接口定义

### 4.1 MissionSystem 接口

```csharp
public interface IMissionSystem {
    MissionData CurrentMission { get; }
    
    // 委托管理
    bool AcceptMission(string missionId);
    void AbandonMission();
    void CompleteMission(bool success);
    
    // 委托查询
    List<MissionData> GetAvailableMissions();
    List<MissionData> GetCompletedMissions();
    
    // 进度更新
    void UpdateObjective(string objectiveId, int progress);
    
    // 事件
    event Action<MissionData> OnMissionStarted;
    event Action<MissionData, bool> OnMissionCompleted;
}
```

### 4.2 ResourceSystem 接口

```csharp
public interface IResourceSystem {
    Inventory PlayerInventory { get; }
    Inventory Storage { get; }
    
    // 采集
    CollectResult TryCollectResource(ResourceNode node);
    float CalculateCollectTime(ResourceData resource);
    
    // 背包管理
    bool AddToInventory(ResourceData resource, int amount);
    bool RemoveFromInventory(string resourceId, int amount);
    bool TransferToStorage(string resourceId, int amount);
    
    // 查询
    int GetItemCount(string resourceId);
    float GetInventoryWeight();
    float GetRemainingCapacity();
    
    // 事件
    event Action<ResourceData, int> OnItemAdded;
    event Action<ResourceData, int> OnItemRemoved;
}
```

### 4.3 事件定义

```csharp
// 委托事件
public struct MissionStartedEvent : IGameEvent {
    public string MissionId;
    public string Title;
    public int TargetDepth;
}

public struct MissionCompletedEvent : IGameEvent {
    public string MissionId;
    public bool Success;
    public RewardData Reward;
}

public struct ObjectiveUpdatedEvent : IGameEvent {
    public string MissionId;
    public string ObjectiveId;
    public int CurrentProgress;
    public int TargetProgress;
}

// 资源事件
public struct ResourceCollectedEvent : IGameEvent {
    public string ResourceId;
    public string ResourceName;
    public int Amount;
    public float Weight;
}

public struct InventoryChangedEvent : IGameEvent {
    public float CurrentWeight;
    public float MaxCapacity;
    public int ItemCount;
}
```

---

## 5. 状态机设计

### 5.1 委托状态机

```
                    ┌─────────────┐
                    │   Inactive  │
                    └──────┬──────┘
                           │ AcceptMission()
                           ▼
                    ┌─────────────┐
         ┌─────────│   Active    │◄────────┐
         │         └──────┬──────┘         │
         │                │                │
         │ Complete()     │ Abandon()      │ UpdateObjective()
         ▼                ▼                │
  ┌─────────────┐  ┌─────────────┐         │
  │  Completed  │  │  Abandoned  │         │
  └─────────────┘  └─────────────┘         │
                                           │
                                           └─► (循环更新进度)
```

### 5.2 背包状态机

```
┌─────────────┐
│    Empty    │◄─────────────────────────┐
└──────┬──────┘                          │
       │ AddItem()                       │ RemoveAll()
       ▼                                 │
┌─────────────┐     AddItem()     ┌─────────────┐
│  Available  │──────────────────►│    Full     │
└──────┬──────┘                   └──────┬──────┘
       │                                 │
       │ RemoveItem()                    │ RemoveItem()
       ▼                                 ▼
       └─────────────────────────────────┘
```

---

## 6. Week 2 新增组件清单

| 组件 | 类型 | 职责 | 依赖 |
|------|------|------|------|
| MissionManager | System | 委托管理 | MechSystem, ResourceSystem |
| MissionGenerator | System | 委托生成 | - |
| MissionBoardUI | UI | 委托板界面 | MissionManager |
| ResourceManager | System | 资源管理 | Inventory |
| Inventory | Data | 背包数据 | - |
| InventoryUI | UI | 背包界面 | ResourceManager |
| ResourceNode | Entity | 可采集资源 | ResourceData |
| CollectionProgressUI | UI | 采集进度 | ResourceManager |

---

*文档版本: 1.0*
*日期: 2026-02-26*
*阶段: Week 2 架构设计*
