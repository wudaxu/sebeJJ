# 赛博机甲 SebeJJ - Unity 2D 游戏项目

![项目状态](https://img.shields.io/badge/状态-正式发布-green)
![版本](https://img.shields.io/badge/版本-v1.0.0-blue)
![Unity](https://img.shields.io/badge/Unity-2022.3%20LTS-black)

## 🎮 项目概述

《赛博机甲 SebeJJ》是一款**深海资源猎人**题材的2D动作探索游戏。玩家驾驶可高度自定义的赛博机甲，在危机四伏的深海中执行各种委托任务，采集珍稀资源，对抗机械怪物，逐步揭开深海之下的秘密。

### 核心体验
- **游戏类型**: 委托驱动 + 资源采集 + 机甲自定义
- **美术风格**: 赛博朋克 × 深海恐惧
- **技术栈**: Unity 2D
- **目标平台**: PC (Steam) / 移动端

### 核心循环
```
接受委托 → 配置机甲 → 下潜探索 → 采集资源 → 遭遇战斗 → 返回基地 → 升级机甲 → 接受新委托
```

---

## ✨ 功能特性

### 核心系统
| 系统 | 状态 | 描述 |
|------|------|------|
| 🎯 委托系统 | ✅ 已完成 | 15个初始委托，支持采集/歼灭/探索/护送/狩猎类型 |
| 🤖 机甲系统 | ✅ 已完成 | 3种框架，8种装备类型，6种模块 |
| 💎 资源系统 | ✅ 已完成 | 15种资源，4个稀有度层级，背包管理 |
| 🌊 下潜系统 | ✅ 已完成 | 深度管理，环境效果，4个深度层 |
| ⚔️ 战斗系统 | ✅ 已完成 | 武器系统，敌人AI，Boss战 |
| 💾 存档系统 | ✅ 已完成 | 多存档槽位，自动/手动保存 |

### 技术特性
- **单例模式**管理核心系统
- **事件驱动**解耦模块
- **对象池**优化性能
- **数据驱动**配置
- **类型安全的事件总线**

---

## 🚀 安装和运行

### 环境要求
- **Unity**: 2022.3 LTS 或更高版本
- **平台**: Windows 10/11, macOS 12+, Linux
- **内存**: 最低 4GB，推荐 8GB

### 安装步骤

1. **克隆项目**
```bash
git clone <repository-url>
cd sebejj
```

2. **Unity Hub 打开**
   - 打开 Unity Hub
   - 点击 "Open" → 选择项目文件夹
   - 使用 Unity 2022.3 LTS 打开

3. **首次导入**
   - 等待 Unity 导入所有资源
   - 打开 `Assets/Scenes/Boot.unity` 启动场景

4. **运行游戏**
   - 点击 Play 按钮
   - 或构建到目标平台

### 构建设置
```
File → Build Settings
- Platform: PC, Mac & Linux Standalone
- Target Platform: Windows / macOS / Linux
- Architecture: x86_64
```

---

## 📁 项目结构

```
SebeJJ/
├── Assets/
│   ├── Scripts/              # 游戏脚本
│   │   ├── Core/             # 核心管理器
│   │   │   ├── GameManager.cs
│   │   │   ├── UIManager.cs
│   │   │   └── SaveManager.cs
│   │   ├── Systems/          # 游戏系统
│   │   │   ├── MissionManager.cs
│   │   │   ├── ResourceManager.cs
│   │   │   ├── DiveManager.cs
│   │   │   └── CombatManager.cs
│   │   ├── Player/           # 机甲相关
│   │   │   ├── MechController.cs
│   │   │   ├── MechMovement.cs
│   │   │   └── MechCollector.cs
│   │   ├── Enemies/          # 敌人系统
│   │   ├── Weapons/          # 武器系统
│   │   ├── UI/               # UI脚本
│   │   └── Utils/            # 工具类
│   ├── Art/                  # 美术资源
│   │   ├── Sprites/          # 精灵图
│   │   ├── Animations/       # 动画
│   │   ├── Effects/          # 特效
│   │   └── StyleGuide/       # 风格指南
│   ├── Audio/                # 音频资源
│   ├── Prefabs/              # 预制体
│   ├── Scenes/               # 场景
│   │   ├── Boot.unity        # 启动场景
│   │   ├── MainMenu.unity    # 主菜单
│   │   ├── Base.unity        # 基地场景
│   │   └── Dive.unity        # 下潜场景
│   └── Resources/            # 动态加载资源
│       ├── Missions/         # 委托配置
│       ├── Configs/          # 游戏配置
│       └── Data/             # 数据文件
├── docs/                     # 项目文档
├── Tests/                    # 测试相关
├── tools/                    # 工具脚本
└── Packages/                 # Unity包
```

---

## 🎮 操作说明

### 基础操作
| 按键 | 功能 |
|------|------|
| WASD / 方向键 | 移动 |
| Shift | 推进器加速 |
| 空格 | 扫描 |
| Tab | 切换采集目标 |
| E | 采集/确认 |
| ESC | 取消采集/打开菜单 |
| I | 打开背包 |

### 游戏流程
1. 开始新游戏后自动进入 Q001 新手试潜
2. 按照教学提示学习基础操作
3. 完成采集和下潜目标
4. 解锁正式委托系统

---

## 🤝 贡献指南

### 代码规范
- **命名**: PascalCase (类/方法), camelCase (变量), UPPER_SNAKE_CASE (常量)
- **命名空间**: `SebeJJ.Systems`, `SebeJJ.Entities`, `SebeJJ.UI`, `SebeJJ.Utils`
- **注释**: 使用 XML 文档注释
- **架构**: 遵循 SOLID 原则，优先使用事件驱动

### 提交规范
```
feat: 新功能
fix: 修复bug
docs: 文档更新
style: 代码格式调整
refactor: 重构
test: 测试相关
chore: 构建/工具
```

### 开发流程
1. 从 `develop` 分支创建功能分支
2. 完成功能并添加测试
3. 提交 Pull Request
4. 代码审查后合并

---

## 📚 文档索引

| 文档 | 描述 |
|------|------|
| [docs/INDEX.md](docs/INDEX.md) | 完整文档目录 |
| [docs/Beta/BETA_PLAN.md](docs/Beta/BETA_PLAN.md) | Beta开发计划 |
| [docs/Beta/BETA_FEATURES.md](docs/Beta/BETA_FEATURES.md) | Beta功能清单 |
| [docs/GDD.md](docs/GDD.md) | 游戏设计文档 |
| [docs/Architecture.md](docs/Architecture.md) | 项目架构文档 |
| [docs/API.md](docs/API.md) | 代码规范与API |
| [QUICKSTART.md](QUICKSTART.md) | 快速开始指南 |
| [CHANGELOG.md](CHANGELOG.md) | 变更日志 |

---

## 📅 开发计划

| 阶段 | 时间 | 状态 |
|------|------|------|
| MVP (4周) | Week 1-4 | ✅ 已完成 |
| Alpha (6周) | Week 5-10 | ✅ 已完成 |
| Beta (8周) | Week 11-18 | ✅ 已完成 |
| Release (10周) | Week 19-26 | ✅ 已完成 |

---

## 👥 团队

- **程序**: 核心系统开发
- **美术**: 视觉设计与资源制作
- **测试**: 质量保证与测试自动化

---

## 📄 许可证

本项目为内部开发项目，保留所有权利。

---

*最后更新: 2026-02-27*
*版本: v1.0.0*
