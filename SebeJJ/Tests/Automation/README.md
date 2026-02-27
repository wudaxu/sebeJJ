# SebeJJ 自动化测试框架

## 框架概述

本测试框架基于 Unity Test Framework (UTF) 和 NUnit，为 SebeJJ 游戏项目提供全面的自动化测试支持。

---

## 目录结构

```
Tests/
├── Automation/
│   ├── Core/                    # 核心系统测试
│   │   ├── GameManagerTests.cs
│   │   ├── EventSystemTests.cs
│   │   ├── SaveSystemTests.cs
│   │   └── ObjectPoolTests.cs
│   ├── Combat/                  # 战斗系统测试 (Week 2)
│   │   ├── DamageSystemTests.cs
│   │   ├── ProjectileSystemTests.cs
│   │   └── WeaponSystemTests.cs
│   ├── AI/                      # 敌人AI测试 (Week 2)
│   │   ├── StateMachineTests.cs
│   │   ├── EnemyBehaviorTests.cs
│   │   └── EnemyTypeTests.cs
│   ├── Player/                  # 玩家系统测试
│   │   ├── MechControllerTests.cs
│   │   ├── MechStatsTests.cs
│   │   ├── PlayerDataTests.cs
│   │   └── InventoryTests.cs
│   ├── UI/                      # UI系统测试
│   │   ├── UIManagerTests.cs
│   │   └── HUDTests.cs
│   ├── Integration/             # 集成测试
│   │   ├── GameFlowTests.cs
│   │   └── CombatIntegrationTests.cs
│   └── TestHelpers/             # 测试辅助类
│       ├── TestUtils.cs
│       └── MockFactory.cs
└── TestData/                    # 测试数据
    ├── TestFixtures/
    ├── MockAssets/
    ├── TEST_DATA_SPEC.md        # Week 1 测试数据
    └── WEEK2_TEST_DATA.md       # Week 2 测试数据
```

---

## 环境要求

- Unity 2022.3 LTS 或更高版本
- Unity Test Framework 1.3.0+
- NUnit 3.13+

---

## 安装配置

### 1. 安装 Unity Test Framework

通过 Package Manager 安装:
```
Window → Package Manager → Unity Registry → Test Framework
```

### 2. 创建 Tests 程序集

创建 `Tests/Automation/SebeJJ.Tests.asmdef`:
```json
{
    "name": "SebeJJ.Tests",
    "rootNamespace": "SebeJJ.Tests",
    "references": [
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner",
        "SebeJJ.Runtime"
    ],
    "includePlatforms": ["Editor"],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": false,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "versionDefines": [],
    "noEngineReferences": false
}
```

### 3. 配置测试运行器

创建 `Tests/test-runner.json`:
```json
{
    "testMode": "EditMode",
    "includeCategories": [],
    "excludeCategories": ["Slow", "Integration"],
    "retryCount": 1,
    "timeout": 300000
}
```

---

## 测试类型

### 1. 单元测试 (Unit Tests)

测试单个类或方法的功能:

```csharp
[Test]
public void PlayerData_TakeDamage_ReducesHealth()
{
    // Arrange
    var playerData = new PlayerData { health = 100f };
    
    // Act
    playerData.TakeDamage(20f);
    
    // Assert
    Assert.AreEqual(80f, playerData.health);
}
```

### 2. 集成测试 (Integration Tests)

测试多个系统协同工作:

```csharp
[Test]
public void GameFlow_PauseAndSave_GameStatePreserved()
{
    // Arrange
    var gameManager = CreateGameManager();
    var saveSystem = CreateSaveSystem();
    
    // Act
    gameManager.ChangeState(GameState.Playing);
    gameManager.PauseGame();
    saveSystem.SaveGame(CreateTestData(), "test");
    
    // Assert
    Assert.AreEqual(GameState.Paused, gameManager.CurrentState);
    Assert.IsTrue(saveSystem.SaveExists("test"));
}
```

### 3. 场景测试 (Play Mode Tests)

测试运行时行为:

```csharp
[UnityTest]
public IEnumerator MechController_Movement_UpdatesPosition()
{
    // Arrange
    var mech = CreateTestMech();
    var startPos = mech.transform.position;
    
    // Act
    mech.Move(Vector2.right);
    yield return new WaitForSeconds(1f);
    
    // Assert
    Assert.Greater(mech.transform.position.x, startPos.x);
}
```

---

## 运行测试

### 通过 Unity Editor

1. 打开 Test Runner: `Window → General → Test Runner`
2. 选择 Edit Mode 或 Play Mode
3. 点击 Run All 或选择特定测试

### 通过命令行

```bash
# 运行所有 Edit Mode 测试
Unity -runTests -testPlatform editmode -testResults results.xml

# 运行所有 Play Mode 测试
Unity -runTests -testPlatform playmode -testResults results.xml

# 运行特定类别测试
Unity -runTests -testPlatform editmode -testCategory "Core" -testResults results.xml
```

### 通过 CI/CD

```yaml
# .github/workflows/test.yml
name: Run Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Run Unity Tests
        uses: game-ci/unity-test-runner@v3
        env:
          UNITY_LICENSE: ${{ secrets.UNITY_LICENSE }}
        with:
          testMode: editmode
          checkName: Edit Mode Tests
```

---

## 测试辅助工具

### TestUtils 类

```csharp
public static class TestUtils
{
    /// <summary>创建测试用的 GameObject</summary>
    public static GameObject CreateTestObject(string name = "TestObject")
    {
        var go = new GameObject(name);
        _testObjects.Add(go);
        return go;
    }
    
    /// <summary>创建测试用的 MechStats</summary>
    public static MechStats CreateTestMechStats()
    {
        var stats = ScriptableObject.CreateInstance<MechStats>();
        stats.maxHealth = 100f;
        stats.speed = 5f;
        // ...
        return stats;
    }
    
    /// <summary>等待帧数</summary>
    public static IEnumerator WaitFrames(int frames)
    {
        for (int i = 0; i < frames; i++)
            yield return null;
    }
}
```

### MockFactory 类

```csharp
public static class MockFactory
{
    public static PlayerData CreateMockPlayerData()
    {
        return new PlayerData
        {
            playerName = "TestPlayer",
            health = 100f,
            energy = 100f,
            oxygen = 100f,
            // ...
        };
    }
    
    public static Inventory CreateMockInventory()
    {
        var inventory = new Inventory();
        inventory.AddItem(ResourceType.CopperOre, 10);
        inventory.AddItem(ResourceType.ScrapMetal, 5);
        return inventory;
    }
}
```

---

## 测试分类

使用 NUnit 的 Category 属性标记测试:

```csharp
[Test]
[Category("Core")]
[Category("Fast")]
public void GameManager_Singleton_ReturnsSameInstance() { }

[Test]
[Category("Core")]
[Category("Slow")]
public void SaveSystem_LargeData_Performance() { }

[Test]
[Category("Integration")]
public void GameFlow_FullGameLoop() { }
```

### 类别说明

| 类别 | 说明 | 运行时机 |
|------|------|----------|
| Core | 核心系统测试 | 每次提交 |
| Combat | 战斗系统测试 | 每次提交 |
| AI | 敌人AI测试 | 每次提交 |
| Weapon | 武器系统测试 | 每次提交 |
| Player | 玩家系统测试 | 每次提交 |
| UI | UI系统测试 | 每次提交 |
| Integration | 集成测试 | 每日构建 |
| Slow | 耗时测试 | 每日构建 |
| Performance | 性能测试 | 每周构建 |

---

## 代码覆盖率

### 配置代码覆盖率

1. 安装 Code Coverage package
2. 启用覆盖率收集:
   ```
   Window → Analysis → Code Coverage
   ```
3. 运行测试并生成报告

### 覆盖率目标

| 模块 | 目标覆盖率 |
|------|------------|
| Core Systems | >= 80% |
| Combat Systems | >= 75% |
| AI Systems | >= 70% |
| Weapon Systems | >= 75% |
| Player Systems | >= 75% |
| UI Systems | >= 70% |
| Overall | >= 75% |

---

## 最佳实践

### 1. 测试命名规范

```csharp
// 格式: [被测类]_[被测方法]_[预期行为]
public void PlayerData_TakeDamage_ReducesHealth()
public void SaveSystem_LoadGame_ReturnsNullWhenNotExists()
public void EventSystem_MultipleSubscribers_AllFire()
```

### 2. 测试结构 (AAA)

```csharp
[Test]
public void ExampleTest()
{
    // Arrange - 准备测试数据
    var input = 5;
    
    // Act - 执行被测操作
    var result = input * 2;
    
    // Assert - 验证结果
    Assert.AreEqual(10, result);
}
```

### 3. 避免测试间依赖

```csharp
[SetUp]
public void Setup()
{
    // 每个测试前重置状态
    GameEvents.ClearAllEvents();
    _testObjects.Clear();
}

[TearDown]
public void Teardown()
{
    // 每个测试后清理
    foreach (var obj in _testObjects)
        Object.DestroyImmediate(obj);
}
```

### 4. 使用参数化测试

```csharp
[TestCase(100f, 20f, 80f)]
[TestCase(50f, 10f, 40f)]
[TestCase(10f, 15f, 0f)]  // 不会低于0
public void PlayerData_TakeDamage_CalculatesCorrectly(
    float initialHealth, 
    float damage, 
    float expectedHealth)
{
    var playerData = new PlayerData { health = initialHealth };
    playerData.TakeDamage(damage);
    Assert.AreEqual(expectedHealth, playerData.health);
}
```

---

## 故障排除

### 常见问题

1. **测试在 Editor 中通过但在 CI 中失败**
   - 检查平台相关代码
   - 确保使用 `[UnityPlatform]` 标记平台特定测试

2. **异步测试超时**
   - 增加超时时间: `[Timeout(10000)]`
   - 检查是否有无限循环

3. **单例测试失败**
   - 确保在 `[TearDown]` 中清理单例
   - 使用 `[Explicit]` 标记需要单独运行的测试

---

## 扩展阅读

- [Unity Test Framework 文档](https://docs.unity3d.com/Packages/com.unity.test-framework@1.3/manual/index.html)
- [NUnit 文档](https://docs.nunit.org/)
- [测试驱动开发 (TDD)](https://en.wikipedia.org/wiki/Test-driven_development)

---

**文档版本:** 1.1  
**更新日期:** 2026-02-26  
**更新内容:** 添加 Week 2 战斗系统、武器系统、敌人AI测试  
**作者:** 测试工程师 Agent
