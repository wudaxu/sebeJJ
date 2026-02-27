# SebeJJ - 第四周开发计划

## 目标
完成经济系统、升级系统、基地功能，准备发布

---

## 里程碑
- [ ] 商店系统完整
- [ ] 机甲升级系统
- [ ] 基地场景功能
- [ ] 游戏循环闭环
- [ ] 发布准备

---

## 每日任务分解

### Day 22: 经济系统
**目标:** 实现货币和交易基础

#### 任务清单
- [ ] 实现 `Currency` 货币系统
- [ ] 实现 `ShopSystem` 商店管理器
- [ ] 实现商店UI框架
- [ ] 实现买卖逻辑
- [ ] 实现价格浮动系统

#### 货币系统
```csharp
public enum CurrencyType
{
    Credits,        // 信用点 - 基础货币
    DataTokens,     // 数据代币 - 高级货币
    Reputation      // 声望 - 解锁内容
}

public class CurrencySystem : MonoBehaviour
{
    private Dictionary<CurrencyType, int> balances = new();
    
    public void AddCurrency(CurrencyType type, int amount)
    {
        if (!balances.ContainsKey(type))
            balances[type] = 0;
        balances[type] += amount;
        GameEvents.OnCurrencyChanged?.Invoke(type, balances[type]);
    }
    
    public bool CanAfford(CurrencyType type, int amount)
    {
        return balances.GetValueOrDefault(type, 0) >= amount;
    }
    
    public bool SpendCurrency(CurrencyType type, int amount)
    {
        if (!CanAfford(type, amount)) return false;
        balances[type] -= amount;
        GameEvents.OnCurrencyChanged?.Invoke(type, balances[type]);
        return true;
    }
}
```

#### 验收标准
- [ ] 货币增减正常
- [ ] 交易逻辑正确
- [ ] 价格浮动合理
- [ ] UI显示正确

---

### Day 23: 商店与交易
**目标:** 实现完整商店功能

#### 任务清单
- [ ] 实现商品列表UI
- [ ] 实现商品分类筛选
- [ ] 实现商品详情面板
- [ ] 实现库存刷新机制
- [ ] 实现特殊商品 (限时、限量)

#### 商店系统
```csharp
[System.Serializable]
public class ShopItem
{
    public ItemData item;
    public CurrencyType currency;
    public int basePrice;
    public int stock = -1;  // -1 = 无限
    public float priceMultiplier = 1f;
    public bool isLimitedTime;
    public float limitedTimeDuration;
}

public class ShopSystem : MonoBehaviour
{
    public List<ShopItem> availableItems;
    public float refreshInterval = 300f;  // 5分钟
    public float priceFluctuation = 0.2f;
    
    public void BuyItem(ShopItem shopItem)
    {
        int finalPrice = Mathf.RoundToInt(shopItem.basePrice * shopItem.priceMultiplier);
        
        if (CurrencySystem.Instance.SpendCurrency(shopItem.currency, finalPrice))
        {
            Inventory.Instance.AddItem(shopItem.item);
            shopItem.stock--;
            GameEvents.OnItemPurchased?.Invoke(shopItem);
        }
    }
    
    public void SellItem(ItemData item)
    {
        int sellPrice = Mathf.RoundToInt(item.baseValue * 0.6f);
        if (Inventory.Instance.RemoveItem(item))
        {
            CurrencySystem.Instance.AddCurrency(CurrencyType.Credits, sellPrice);
        }
    }
    
    public void RefreshStock()
    {
        // 随机刷新商品
        // 应用价格波动
    }
}
```

#### 验收标准
- [ ] 购买/出售流程顺畅
- [ ] 库存管理正确
- [ ] 价格变化合理
- [ ] 特殊商品机制正常

---

### Day 24: 升级系统 - Part 1
**目标:** 实现机甲升级框架

#### 任务清单
- [ ] 创建 `UpgradeData` ScriptableObject
- [ ] 实现 `UpgradeSystem` 管理器
- [ ] 实现升级树数据结构
- [ ] 实现升级解锁逻辑
- [ ] 实现升级效果应用

#### 升级系统
```csharp
public enum UpgradeType
{
    // 机体升级
    HullReinforcement,      // 船体强化 - +生命值
    EnergyCore,             // 能量核心 - +能量
    OxygenTank,             // 氧气罐 - +氧气
    
    // 机动升级
    Thruster,               // 推进器 - +速度
    Gyroscope,              // 陀螺仪 - +转向
    
    // 采集升级
    MiningLaser,            // 采矿激光 - +采矿效率
    CargoExpansion,         // 货仓扩展 - +容量
    
    // 战斗升级
    WeaponDamage,           // 武器伤害
    FireRate,               // 射速
    EnergyEfficiency,       // 能量效率
    
    // 特殊升级
    PressureResistance,     // 压强抗性 - 可下潜更深
    Sonar,                  // 声纳 - 显示地图
    AutoRepair              // 自动修复
}

[CreateAssetMenu(fileName = "Upgrade", menuName = "SebeJJ/Upgrade")]
public class UpgradeData : ScriptableObject
{
    public string upgradeName;
    public UpgradeType type;
    public string description;
    public Sprite icon;
    public int maxLevel = 5;
    public int baseCost;
    public float costMultiplier = 1.5f;
    public float valuePerLevel;
    public List<UpgradeData> prerequisites;
}

public class UpgradeSystem : MonoBehaviour
{
    private Dictionary<UpgradeData, int> upgradeLevels = new();
    
    public bool CanUpgrade(UpgradeData upgrade)
    {
        // 检查等级上限
        if (GetLevel(upgrade) >= upgrade.maxLevel) return false;
        
        // 检查前置条件
        foreach (var prereq in upgrade.prerequisites)
        {
            if (GetLevel(prereq) < 1) return false;
        }
        
        // 检查货币
        int cost = GetUpgradeCost(upgrade);
        return CurrencySystem.Instance.CanAfford(CurrencyType.Credits, cost);
    }
    
    public void ApplyUpgrade(UpgradeData upgrade)
    {
        if (!CanUpgrade(upgrade)) return;
        
        int cost = GetUpgradeCost(upgrade);
        CurrencySystem.Instance.SpendCurrency(CurrencyType.Credits, cost);
        
        upgradeLevels[upgrade] = GetLevel(upgrade) + 1;
        ApplyUpgradeEffect(upgrade);
    }
}
```

#### 验收标准
- [ ] 升级数据可配置
- [ ] 前置条件检查正确
- [ ] 升级效果生效
- [ ] 等级上限限制

---

### Day 25: 升级系统 - Part 2
**目标:** 实现升级UI和视觉效果

#### 任务清单
- [ ] 实现升级树UI
- [ ] 实现升级节点可视化
- [ ] 实现升级动画效果
- [ ] 实现属性预览
- [ ] 实现升级历史记录

#### 升级UI
```
UpgradeTreePanel
├── CategoryTabs (机体/机动/采集/战斗/特殊)
├── UpgradeTreeViewport
│   ├── UpgradeNode (可点击)
│   ├── ConnectionLines
│   └── LockedOverlay
├── UpgradeDetailsPanel
│   ├── Icon
│   ├── Name
│   ├── Description
│   ├── CurrentLevel
│   ├── NextLevelPreview
│   └── UpgradeButton
└── CurrencyDisplay
```

#### 验收标准
- [ ] 升级树可视化清晰
- [ ] 节点状态显示正确
- [ ] 升级动画流畅
- [ ] 属性预览准确

---

### Day 26: 基地场景
**目标:** 实现基地场景和功能

#### 任务清单
- [ ] 创建 `BaseScene` 场景
- [ ] 实现基地UI主界面
- [ ] 集成商店到基地
- [ ] 集成升级系统到基地
- [ ] 实现任务/公告板
- [ ] 实现存档点

#### 基地功能
```csharp
public class BaseManager : MonoBehaviour
{
    [Header("Base Stations")]
    public ShopStation shopStation;
    public UpgradeStation upgradeStation;
    public MissionBoard missionBoard;
    public SaveStation saveStation;
    
    private void Start()
    {
        // 初始化基地
        GameEvents.OnReturnToBase?.Invoke();
    }
    
    public void PrepareForDive()
    {
        // 保存基地状态
        SaveSystem.Instance.AutoSave();
        
        // 生成新地图
        MapGenerator.Instance.GenerateLevel(PlayerData.Instance.currentDepth);
        
        // 进入探索场景
        GameManager.Instance.ChangeState(GameState.Exploring);
    }
}
```

#### 验收标准
- [ ] 基地场景完整
- [ ] 各功能站可用
- [ ] 场景切换正常
- [ ] 状态保存正确

---

### Day 27: 游戏循环闭环
**目标:** 实现完整的游戏循环

#### 任务清单
- [ ] 实现下潜准备流程
- [ ] 实现探索中状态管理
- [ ] 实现紧急返回机制
- [ ] 实现死亡和惩罚
- [ ] 实现胜利条件
- [ ] 平衡数值调整

#### 游戏循环
```
基地准备 → 下潜探索 → 采集/战斗 → 选择返回/继续 → 返回基地 → 升级/交易 → 重复
```

#### 死亡惩罚
```csharp
public class DeathPenalty : MonoBehaviour
{
    public void ApplyDeathPenalty()
    {
        // 1. 丢失本次收集的资源
        Inventory.Instance.Clear();
        
        // 2. 丢失部分货币
        int creditLoss = CurrencySystem.Instance.GetBalance(CurrencyType.Credits) / 2;
        CurrencySystem.Instance.SpendCurrency(CurrencyType.Credits, creditLoss);
        
        // 3. 机甲损坏，需要维修
        MechController.Instance.SetDamagedState(true);
        
        // 4. 返回基地
        GameManager.Instance.LoadScene("BaseScene");
    }
}
```

#### 验收标准
- [ ] 游戏循环完整
- [ ] 各阶段切换正常
- [ ] 死亡惩罚合理
- [ ] 进度感明显

---

### Day 28: 发布准备
**目标:** 最后优化和发布准备

#### 任务清单
- [ ] 代码清理和优化
- [ ] 性能测试和优化
- [ ] 修复已知Bug
- [ ] 实现主菜单
- [ ] 实现设置菜单
- [ ] 添加教程/引导
- [ ] 构建测试版本

#### 性能检查清单
- [ ] 目标帧率 60 FPS
- [ ] 内存使用 < 500MB
- [ ] 加载时间 < 3秒
- [ ] 无内存泄漏
- [ ] 对象池工作正常

#### 构建设置
```
Platform: PC (Windows/Mac/Linux)
Resolution: 1920x1080 (可缩放)
Quality: High
Compression: LZ4HC
```

#### 验收标准
- [ ] 无明显Bug
- [ ] 性能达标
- [ ] 所有功能可用
- [ ] 可正常构建运行

---

## 最终交付物

### 代码
- [ ] 完整的游戏系统代码
- [ ] 清晰的代码结构
- [ ] 必要的注释

### 场景
- [ ] MainMenuScene
- [ ] BaseScene
- [ ] ExplorationScene

### UI
- [ ] 主菜单
- [ ] HUD
- [ ] 商店UI
- [ ] 升级UI
- [ ] 背包UI
- [ ] 暂停菜单
- [ ] 游戏结束画面

### 配置
- [ ] 武器数据
- [ ] 敌人数据
- [ ] 资源数据
- [ ] 升级数据
- [ ] 平衡数值

---

## 发布检查清单

### 功能完整
- [ ] 核心循环可玩
- [ ] 无阻塞性Bug
- [ ] 存档功能正常
- [ ] 设置可调整

### 用户体验
- [ ] 操作流畅
- [ ] UI清晰
- [ ] 反馈明确
- [ ] 难度合理

### 技术质量
- [ ] 性能达标
- [ ] 内存稳定
- [ ] 无崩溃
- [ ] 构建成功

---

## 后续建议 (发布后)

1. **内容扩展**
   - 更多敌人类型
   - 更多武器
   - 更多升级选项
   - 更多地图变体

2. **功能增强**
   - 成就系统
   - 排行榜
   - 云存档
   - 多语言支持

3. **平台适配**
   - 手柄支持优化
   - 移动端适配
   - 主机版本

---

**计划版本:** 1.0  
**创建日期:** 2026-02-26  
**负责人:** 架构师 Agent
