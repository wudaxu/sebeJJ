# Week 2 集成测试方案

**版本**: v1.0  
**日期**: 2026-02-26  
**阶段**: Week 2 - 系统集成测试

---

## 1. 集成测试概述

### 1.1 测试目标
验证委托系统、机甲系统、资源系统之间的接口和数据流正确性，确保多系统协同工作时的稳定性和一致性。

### 1.2 测试范围
- **系统间接口**: 委托→机甲、机甲→资源、资源→下潜
- **数据流**: 委托进度更新、资源消耗计算、状态同步
- **场景集成**: 完整委托执行流程

### 1.3 测试策略
| 测试类型 | 方法 | 覆盖范围 |
|----------|------|----------|
| 接口测试 | 自动化 | 所有系统间API |
| 数据流测试 | 自动化+手动 | 关键业务流程 |
| 场景测试 | 手动 | 完整游戏体验 |
| 回归测试 | 自动化 | 每次构建后执行 |

---

## 2. 系统集成架构

```
┌─────────────────────────────────────────────────────────────┐
│                        委托系统 (QuestSystem)                 │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐     │
│  │ 委托管理  │  │ 进度追踪  │  │ 奖励发放  │  │ 状态同步  │     │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘     │
└───────┼─────────────┼─────────────┼─────────────┼───────────┘
        │             │             │             │
        ▼             ▼             ▼             ▼
┌─────────────────────────────────────────────────────────────┐
│                        机甲系统 (MechSystem)                  │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐     │
│  │ 移动控制  │  │ 扫描功能  │  │ 采集功能  │  │ 状态报告  │     │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘     │
└───────┼─────────────┼─────────────┼─────────────┼───────────┘
        │             │             │             │
        ▼             ▼             ▼             ▼
┌─────────────────────────────────────────────────────────────┐
│                        资源系统 (ResourceSystem)              │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐     │
│  │ 氧气管理  │  │ 能源管理  │  │ 背包管理  │  │ 消耗计算  │     │
│  └────┬─────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘     │
└───────┼─────────────┼─────────────┼─────────────┼───────────┘
        │             │             │             │
        ▼             ▼             ▼             ▼
┌─────────────────────────────────────────────────────────────┐
│                        下潜系统 (DiveSystem)                  │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐     │
│  │ 深度管理  │  │ 压力计算  │  │ 危险区域  │  │ 环境效果  │     │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘     │
└─────────────────────────────────────────────────────────────┘
```

---

## 3. 接口测试规范

### 3.1 委托系统 → 机甲系统

#### 接口: IQuestMechInterface
```csharp
public interface IQuestMechInterface
{
    // 委托请求机甲执行采集
    bool RequestCollection(string itemId, int count);
    
    // 委托请求机甲执行扫描
    bool RequestScan(Vector3 position, float radius);
    
    // 委托请求到达指定深度
    bool RequestDepthReached(float targetDepth);
    
    // 获取机甲当前状态
    MechState GetMechState();
}
```

#### 测试用例
| ID | 接口方法 | 测试场景 | 预期结果 |
|----|----------|----------|----------|
| II-001 | RequestCollection | 委托请求采集5个荧光藻 | 机甲执行采集，返回成功 |
| II-002 | RequestCollection | 背包已满时请求采集 | 返回失败，提示背包满 |
| II-003 | RequestScan | 委托请求扫描隐藏物品 | 扫描执行，返回扫描结果 |
| II-004 | RequestDepthReached | 委托请求到达800m | 到达后返回成功 |
| II-005 | GetMechState | 查询机甲状态 | 返回当前位置、深度、资源 |

### 3.2 机甲系统 → 资源系统

#### 接口: IMechResourceInterface
```csharp
public interface IMechResourceInterface
{
    // 消耗能源
    bool ConsumeEnergy(float amount);
    
    // 消耗氧气
    bool ConsumeOxygen(float amount);
    
    // 检查资源是否充足
    bool HasSufficientResources(float energyNeeded, float oxygenNeeded);
    
    // 获取当前资源状态
    ResourceState GetResourceState();
}
```

#### 测试用例
| ID | 接口方法 | 测试场景 | 预期结果 |
|----|----------|----------|----------|
| II-006 | ConsumeEnergy | 移动消耗10能源 | 能源减少10，返回成功 |
| II-007 | ConsumeEnergy | 能源不足时消耗 | 返回失败，不扣除 |
| II-008 | ConsumeOxygen | 在100m深度消耗氧气 | 按深度系数计算消耗 |
| II-009 | HasSufficientResources | 检查采集所需资源 | 正确判断资源充足性 |

### 3.3 资源系统 → 下潜系统

#### 接口: IResourceDiveInterface
```csharp
public interface IResourceDiveInterface
{
    // 获取当前深度消耗系数
    float GetDepthConsumptionMultiplier(float depth);
    
    // 获取压力伤害值
    float GetPressureDamage(float depth, float maxSafeDepth);
    
    // 获取危险区域额外消耗
    float GetDangerZoneExtraConsumption(string zoneType);
}
```

#### 测试用例
| ID | 接口方法 | 测试场景 | 预期结果 |
|----|----------|----------|----------|
| II-010 | GetDepthConsumptionMultiplier | 获取500m深度系数 | 返回1.5（示例值） |
| II-011 | GetPressureDamage | 机甲最大800m，当前1000m | 返回压力伤害值 |
| II-012 | GetDangerZoneExtraConsumption | 热液喷口区 | 返回额外能源消耗 |

---

## 4. 数据流测试

### 4.1 委托执行完整数据流

```
[委托接取] → [机甲移动] → [资源消耗] → [深度检测] → [目标交互] → [进度更新] → [委托完成]
     │            │            │            │            │            │            │
     ▼            ▼            ▼            ▼            ▼            ▼            ▼
  QuestDB    MechCtrl    ResourceMgr   DiveSys     InteractSys   QuestMgr    RewardSys
```

#### 测试场景: Q002 - 收集荧光藻
| 步骤 | 操作 | 数据变化 | 验证点 |
|------|------|----------|--------|
| 1 | 接取委托 | ActiveQuests +1 | 委托状态为"进行中" |
| 2 | 移动到目标 | Position更新 | 追踪标记指向目标 |
| 3 | 开始采集 | Energy -= 5 | 资源正确消耗 |
| 4 | 采集完成 | Inventory +1 | 物品进入背包 |
| 5 | 进度更新 | Progress 1/5 | 委托面板显示更新 |
| 6 | 重复3-5 | Progress 5/5 | 完成所有目标 |
| 7 | 返回提交 | ActiveQuests -1 | 委托移除，奖励发放 |

### 4.2 并发数据流测试

#### 场景: 多委托同时进行
```
委托A: 采集5个荧光藻 (进度: 2/5)
委托B: 扫描3个隐藏点 (进度: 1/3)
委托C: 下潜到500m (进度: 300/500)
```

| 操作 | 委托A | 委托B | 委托C | 系统状态 |
|------|-------|-------|-------|----------|
| 初始状态 | 0/5 | 0/3 | 0/500 | 正常 |
| 采集1个荧光藻 | 1/5 | 0/3 | 0/500 | A更新 |
| 扫描1个点 | 1/5 | 1/3 | 0/500 | B更新 |
| 下潜到150m | 1/5 | 1/3 | 150/500 | C更新 |
| 采集第2个 | 2/5 | 1/3 | 150/500 | A更新 |
| 检查资源 | - | - | - | 消耗正确累计 |

---

## 5. 集成测试用例

### 5.1 委托-机甲集成 (QM-INT)

#### QM-INT-001: 采集委托完整流程
```csharp
[Test]
public IEnumerator QuestCollectionIntegration()
{
    // 1. 准备测试环境
    LoadTestSave(2);
    var initialEnergy = ResourceManager.Instance.Energy;
    
    // 2. 接取采集委托
    var quest = QuestManager.Instance.AcceptQuest("Q002");
    Assert.IsNotNull(quest);
    
    // 3. 移动到采集点
    yield return MoveToPosition(quest.TargetPosition);
    Assert.IsTrue(Vector3.Distance(MechController.Position, quest.TargetPosition) < 5f);
    
    // 4. 执行采集
    yield return MechController.Collect("glow_algae");
    
    // 5. 验证资源消耗
    Assert.IsTrue(ResourceManager.Instance.Energy < initialEnergy);
    
    // 6. 验证委托进度
    Assert.AreEqual(1, quest.CurrentProgress);
    
    // 7. 验证物品进入背包
    Assert.IsTrue(Inventory.Instance.HasItem("glow_algae"));
}
```

#### QM-INT-002: 扫描委托与机甲联动
```csharp
[Test]
public IEnumerator QuestScanIntegration()
{
    LoadTestSave(2);
    var quest = QuestManager.Instance.AcceptQuest("Q004");
    
    // 使用扫描功能
    var scanResults = MechController.Scan();
    
    // 验证委托进度更新
    yield return new WaitForSeconds(0.5f);
    Assert.IsTrue(quest.CurrentProgress > 0);
}
```

### 5.2 资源-下潜集成 (RD-INT)

#### RD-INT-001: 深度对资源消耗的影响
```csharp
[Test]
public IEnumerator DepthResourceConsumption()
{
    // 在100m深度
    DiveSystem.SetDepth(100f);
    var consumption100m = ResourceManager.GetOxygenConsumptionRate();
    
    // 在500m深度
    DiveSystem.SetDepth(500f);
    var consumption500m = ResourceManager.GetOxygenConsumptionRate();
    
    // 验证深度越大消耗越快
    Assert.IsTrue(consumption500m > consumption100m);
}
```

#### RD-INT-002: 压力伤害与资源恢复
```csharp
[Test]
public IEnumerator PressureDamageAndRepair()
{
    // 进入超深区域
    DiveSystem.SetDepth(1000f);
    var mech = MechController.Instance;
    var initialHealth = mech.Health;
    
    // 等待压力伤害
    yield return new WaitForSeconds(2f);
    
    // 验证受到伤害
    Assert.IsTrue(mech.Health < initialHealth);
    
    // 使用修复道具
    Inventory.Instance.UseItem("repair_kit");
    
    // 验证恢复
    Assert.IsTrue(mech.Health > initialHealth - 20);
}
```

### 5.3 三系统联合集成 (TRI-INT)

#### TRI-INT-001: 危险区采集委托
```csharp
[Test]
public IEnumerator DangerZoneCollectionQuest()
{
    LoadTestSave(3);
    var quest = QuestManager.Instance.AcceptQuest("Q009");
    var initialEnergy = ResourceManager.Instance.Energy;
    
    // 进入危险区
    yield return MoveToPosition(DangerZone.Entrance);
    
    // 验证警告显示
    Assert.IsTrue(UIManager.Instance.DangerWarning.active);
    
    // 验证额外能源消耗
    yield return new WaitForSeconds(2f);
    var energyInDanger = initialEnergy - ResourceManager.Instance.Energy;
    
    // 离开危险区
    yield return MoveToPosition(SafeZone.Center);
    
    // 验证消耗停止
    var energyAfterExit = ResourceManager.Instance.Energy;
    yield return new WaitForSeconds(2f);
    Assert.AreEqual(energyAfterExit, ResourceManager.Instance.Energy);
}
```

---

## 6. 测试执行计划

### 6.1 执行顺序
```
Day 1: 接口测试（自动化）
  ├─ 委托-机甲接口
  ├─ 机甲-资源接口
  └─ 资源-下潜接口

Day 2: 数据流测试（自动化+手动）
  ├─ 单委托完整流程
  ├─ 多委托并发流程
  └─ 异常恢复流程

Day 3: 场景集成测试（手动）
  ├─ Q001-Q005 完整体验
  ├─ 边界条件验证
  └─ 长时间稳定性

Day 4: 问题修复与回归
  ├─ 修复发现的问题
  └─ 执行回归测试

Day 5: 集成测试报告
  ├─ 测试结果汇总
  └─ 遗留问题跟踪
```

### 6.2 通过标准
- [ ] 所有接口测试100%通过
- [ ] 数据流测试无数据丢失/错误
- [ ] 场景测试可完成完整游戏流程
- [ ] 无Critical/Major级别缺陷

---

## 7. 自动化测试脚本

### 7.1 集成测试运行器
```csharp
public class IntegrationTestRunner : MonoBehaviour
{
    [Header("测试配置")]
    public bool runInterfaceTests = true;
    public bool runDataFlowTests = true;
    public bool runScenarioTests = true;
    
    [Header("测试场景")]
    public List<QuestTestScenario> scenarios;
    
    public void RunAllIntegrationTests()
    {
        Debug.Log("[IntegrationTest] Starting integration tests...");
        
        if (runInterfaceTests) StartCoroutine(RunInterfaceTests());
        if (runDataFlowTests) StartCoroutine(RunDataFlowTests());
        if (runScenarioTests) StartCoroutine(RunScenarioTests());
    }
    
    private IEnumerator RunInterfaceTests()
    {
        // 执行所有接口测试
        yield return null;
    }
    
    private IEnumerator RunDataFlowTests()
    {
        // 执行数据流测试
        yield return null;
    }
    
    private IEnumerator RunScenarioTests()
    {
        // 执行场景测试
        foreach (var scenario in scenarios)
        {
            yield return RunScenario(scenario);
        }
    }
}
```

### 7.2 CI/CD集成
```yaml
# .github/workflows/integration-test.yml
name: Integration Tests

on: [push, pull_request]

jobs:
  integration-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      
      - name: Run Interface Tests
        run: |
          ./run_tests.sh --category=interface
          
      - name: Run Data Flow Tests
        run: |
          ./run_tests.sh --category=dataflow
          
      - name: Run Scenario Tests
        run: |
          ./run_tests.sh --category=scenario
          
      - name: Generate Report
        run: |
          ./generate_report.sh --output=integration-report.html
```

---

## 8. 问题跟踪模板

### 集成问题报告
```markdown
## 问题编号: INT-XXX

**系统**: 委托系统 ↔ 机甲系统
**严重程度**: [Critical/Major/Minor]
**发现日期**: YYYY-MM-DD

### 问题描述
[详细描述问题现象]

### 重现步骤
1. [步骤1]
2. [步骤2]

### 预期结果
[应该发生什么]

### 实际结果
[实际发生什么]

### 相关日志
```
[错误日志]
```

### 修复状态
- [ ] 已修复
- [ ] 已验证
- [ ] 已关闭
```
