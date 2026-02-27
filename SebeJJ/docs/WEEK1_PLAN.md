# SebeJJ - 第一周开发计划

## 目标
建立项目基础框架，完成核心系统搭建和基础玩法原型

---

## 里程碑
- [ ] 项目初始化完成
- [ ] 核心系统框架搭建
- [ ] 玩家机甲基础移动实现
- [ ] 基础场景和摄像机系统
- [ ] 简单UI框架

---

## 每日任务分解

### Day 1: 项目初始化
**目标:** 建立Unity项目结构和基础配置

#### 任务清单
- [ ] 创建Unity 2022.3 LTS项目
- [ ] 配置URP (Universal Render Pipeline)
- [ ] 设置项目文件夹结构
- [ ] 配置Git版本控制
- [ ] 安装必要Package:
  - Input System
  - Cinemachine
  - TextMeshPro
  - 2D Lighting

#### 交付物
```
Assets/
├── _Project/
│   ├── Scripts/
│   ├── Prefabs/
│   ├── Scenes/
│   ├── ScriptableObjects/
│   ├── Audio/
│   ├── Sprites/
│   └── Animations/
```

#### 验收标准
- [ ] 项目能正常打开无报错
- [ ] Git仓库初始化完成
- [ ] 文件夹结构符合架构文档

---

### Day 2: 核心系统框架
**目标:** 实现游戏管理器和事件系统

#### 任务清单
- [ ] 实现单例基类 `Singleton<T>`
- [ ] 实现 `GameManager` 游戏状态管理
- [ ] 实现 `GameEvents` 全局事件系统
- [ ] 实现 `SaveSystem` 存档系统框架
- [ ] 实现 `ObjectPool` 对象池基类

#### 核心代码
```csharp
// GameManager.cs - 需要实现
public class GameManager : Singleton<GameManager>
{
    public GameState CurrentState { get; private set; }
    public void ChangeState(GameState newState);
    public void PauseGame();
    public void ResumeGame();
}

// GameEvents.cs - 需要实现
public static class GameEvents
{
    public static Action OnGameStart;
    public static Action OnGamePause;
    public static Action OnGameResume;
    public static Action OnGameOver;
}
```

#### 验收标准
- [ ] 游戏状态切换正常
- [ ] 事件订阅/发布工作正常
- [ ] 存档/读档功能可用

---

### Day 3: 玩家机甲系统 - Part 1
**目标:** 实现机甲基础移动和物理

#### 任务清单
- [ ] 创建 `MechController` 脚本
- [ ] 实现2D移动控制 (WASD/摇杆)
- [ ] 实现旋转控制
- [ ] 添加浮力物理模拟
- [ ] 实现深度系统 (Y轴映射到深度)
- [ ] 配置Input System输入映射

#### 技术要点
```csharp
[RequireComponent(typeof(Rigidbody2D))]
public class MechController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 180f;
    public float buoyancy = 2f;
    
    [Header("Depth")]
    public float currentDepth;
    
    private Rigidbody2D _rb;
    private Vector2 _movementInput;
}
```

#### 验收标准
- [ ] 机甲能响应输入移动
- [ ] 旋转平滑自然
- [ ] 浮力效果真实感
- [ ] 深度显示正确

---

### Day 4: 玩家机甲系统 - Part 2
**目标:** 实现机甲属性和状态管理

#### 任务清单
- [ ] 创建 `MechStats` ScriptableObject
- [ ] 实现 `PlayerData` 数据类
- [ ] 实现生命值/能量/氧气系统
- [ ] 实现属性修改器系统
- [ ] 创建机甲预制体

#### 属性设计
```csharp
[CreateAssetMenu(fileName = "MechStats", menuName = "SebeJJ/MechStats")]
public class MechStats : ScriptableObject
{
    public float maxHealth = 100f;
    public float maxEnergy = 100f;
    public float maxOxygen = 100f;
    public float armor = 10f;
    public float pressureResistance = 100f;
    public float speed = 5f;
}
```

#### 验收标准
- [ ] 属性SO可配置
- [ ] 生命值变化事件触发
- [ ] 能量消耗/恢复正常
- [ ] 氧气随深度消耗

---

### Day 5: 场景与摄像机系统
**目标:** 搭建基础场景和摄像机跟随

#### 任务清单
- [ ] 创建主场景 `MainScene`
- [ ] 配置2D光照系统
- [ ] 设置Cinemachine摄像机
- [ ] 实现摄像机边界限制
- [ ] 创建基础环境背景
- [ ] 添加深海视觉效果 (暗角、雾效)

#### Cinemachine配置
```
- 创建Cinemachine Virtual Camera
- 设置Follow目标为Player
- 配置Dead Zone和Soft Zone
- 添加轻微相机抖动效果
```

#### 验收标准
- [ ] 摄像机平滑跟随玩家
- [ ] 边界限制生效
- [ ] 深海氛围视觉效果
- [ ] 2D光照正常工作

---

### Day 6: UI框架 - Part 1
**目标:** 实现基础UI系统

#### 任务清单
- [ ] 创建 `UIManager` 单例
- [ ] 实现UI面板基类 `UIPanel`
- [ ] 实现 `HUD` 主界面
- [ ] 实现状态条 (Health/Energy/Oxygen)
- [ ] 实现深度显示
- [ ] 实现警告提示系统

#### UI结构
```
Canvas (Screen Space - Overlay)
├── HUD
│   ├── HealthBar
│   ├── EnergyBar
│   ├── OxygenBar
│   ├── DepthText
│   └── WarningPanel
├── PauseMenu (初始隐藏)
└── GameOverScreen (初始隐藏)
```

#### 验收标准
- [ ] UI响应状态变化
- [ ] 状态条动画平滑
- [ ] 警告提示可正常显示/隐藏
- [ ] UI缩放适应不同分辨率

---

### Day 7: 整合测试与优化
**目标:** 整合第一周内容，修复问题

#### 任务清单
- [ ] 整合所有系统
- [ ] 进行基础玩法测试
- [ ] 修复发现的Bug
- [ ] 性能初步优化
- [ ] 编写单元测试
- [ ] 更新文档

#### 测试清单
- [ ] 游戏启动流程
- [ ] 暂停/恢复功能
- [ ] 存档/读档功能
- [ ] 玩家移动流畅度
- [ ] UI响应性
- [ ] 内存泄漏检查

#### 交付物
- [ ] 可运行的基础原型
- [ ] 第一周总结文档
- [ ] 已知问题列表

---

## 技术依赖

| 依赖项 | 用途 | 优先级 |
|--------|------|--------|
| Unity 2022.3 LTS | 游戏引擎 | 必须 |
| URP | 2D渲染 | 必须 |
| Input System | 输入处理 | 必须 |
| Cinemachine | 摄像机控制 | 必须 |
| TextMeshPro | 文本渲染 | 必须 |

---

## 风险与应对

| 风险 | 可能性 | 应对措施 |
|------|--------|----------|
| URP配置问题 | 中 | 提前测试，准备备选方案 |
| 物理手感不佳 | 中 | 多调参，参考同类游戏 |
| 进度延迟 | 低 | 每日站会，及时调整 |

---

## 成功标准

- [ ] 玩家可以控制机甲在场景中移动
- [ ] 基础UI显示正常
- [ ] 游戏可以暂停/恢复
- [ ] 存档功能可用
- [ ] 无明显Bug

---

**计划版本:** 1.0  
**创建日期:** 2026-02-26  
**负责人:** 架构师 Agent
