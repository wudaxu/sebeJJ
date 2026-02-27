# SebeJJ UI 规范文档

## 概述

本文档定义了赛博机甲SebeJJ游戏的UI设计规范和实现标准。

## 1. 视觉风格

### 1.1 赛博朋克主题

SebeJJ采用赛博朋克视觉风格，特点包括：
- **霓虹色彩**: 青色、紫色、橙色的霓虹光效
- **科技感**: 网格、扫描线、数据流等科技元素
- **暗色基调**: 深空黑背景，突出前景元素
- **故障艺术**: 数字故障、闪烁效果

### 1.2 配色方案

#### 主题色
| 名称 | 色值 | 用途 |
|-----|------|-----|
| 赛博青 | `#33E6FF` | 主色调、高亮、交互 |
| 霓虹紫 | `#CC33E6` | 副色调、装饰、特殊状态 |
| 能量橙 | `#FF991A` | 强调色、警告、能量 |
| 深空黑 | `#0D0D14` | 背景色 |
| 面板黑 | `#1A1A26` | 面板背景 |

#### 状态色
| 名称 | 色值 | 用途 |
|-----|------|-----|
| 成功绿 | `#33FF66` | 成功、完成、正向反馈 |
| 警告黄 | `#F2CC1A` | 警告、注意 |
| 错误红 | `#F2331A` | 错误、危险、负面反馈 |
| 信息蓝 | `#3399FF` | 信息、提示 |

#### 血量颜色
| 状态 | 色值 |
|-----|------|
| 高血量 (>60%) | `#33E64D` |
| 中血量 (30-60%) | `#F2CC1A` |
| 低血量 (<30%) | `#F2331A` |

## 2. 字体规范

### 2.1 字体层级

| 层级 | 字体大小 | 字重 | 用途 |
|-----|---------|-----|-----|
| 标题 (Title) | 48-72px | Bold | 主标题、Logo |
| 副标题 (Header) | 32-48px | SemiBold | 页面标题 |
| 正文 (Body) | 18-24px | Regular | 主要内容 |
| 说明 (Caption) | 12-16px | Regular | 辅助说明 |
| 代码 (Code) | 14-18px | Mono | 数据、代码 |

### 2.2 字体效果

- **描边**: 1-2px 黑色描边，用于提高可读性
- **阴影**: 偏移 (1, -1)，模糊 2px
- **发光**: 用于重要文字，强度 0.5-1.0

## 3. 布局规范

### 3.1 安全区域

- 顶部安全距离: 60px (移动端 notch)
- 底部安全距离: 40px
- 侧边安全距离: 20px

### 3.2 响应式断点

| 分辨率类型 | 宽度范围 | 适配策略 |
|-----------|---------|---------|
| 移动端 | < 768px | 竖屏布局，触控优化 |
| 平板 | 768px - 1024px | 自适应网格 |
| 桌面 | 1024px - 1920px | 标准布局 |
| 超宽屏 | 1920px - 2560px | 侧边扩展 |
| 4K | > 2560px | 高清适配 |

### 3.3 超宽屏适配 (21:9)

- 中心内容最大宽度: 1920px
- 侧边面板向内侧偏移: 100px (21:9), 200px (32:9)
- 背景使用填充模式保持完整

## 4. 组件规范

### 4.1 按钮

#### 尺寸
- 最小触控尺寸: 44x44px
- 标准按钮: 160x56px
- 大按钮: 240x72px
- 小按钮: 120x48px

#### 状态
| 状态 | 缩放 | 光晕 | 颜色变化 |
|-----|-----|-----|---------|
| 正常 | 1.0x | 无 | 基础色 |
| 悬停 | 1.08x | 0.8强度 | 亮度+20% |
| 按下 | 0.92x | 1.5强度 | 亮度-10% |
| 禁用 | 1.0x | 无 | 灰度 50% |

#### 动画
- 悬停进入: 0.15s, Ease.OutBack
- 悬停退出: 0.15s, Ease.OutQuad
- 点击反馈: 0.1s, Ease.OutQuad

### 4.2 面板

#### 边框
- 边框宽度: 2-3px
- 边框颜色: 赛博青 `#33E6FF`
- 角标装饰: 4个角落，L形设计

#### 动画
- 进入: 边框绘制动画，角标依次出现
- 空闲: 扫描线、发光脉冲
- 警告: 红色闪烁

### 4.3 弹窗

#### 类型
- **模态弹窗**: 带遮罩，必须响应
- **通知提示**: 侧边滑入，自动消失
- **确认对话框**: 居中显示，需要确认

#### 动画
- 显示: Scale 0→1, 0.3s, OutBack
- 隐藏: Scale 1→0, 0.2s, InBack
- 遮罩: Alpha 0→0.7, 0.2s

### 4.4 进度条

#### 样式
- 高度: 12-24px
- 圆角: 2px
- 背景: 半透明黑
- 填充: 渐变色

#### 动画
- 值变化: 0.3s, Ease.OutQuad
- 延迟填充: 0.5s 延迟

## 5. 动画规范

### 5.1 动画时长

| 类型 | 时长 | 说明 |
|-----|-----|-----|
| 即时反馈 | 0.1s | 按钮点击 |
| 快速过渡 | 0.15-0.2s | 悬停、开关 |
| 标准过渡 | 0.3-0.4s | 面板、弹窗 |
| 慢速过渡 | 0.5-0.8s | 复杂动画 |
| 环境动画 | 1-3s | 脉冲、呼吸 |

### 5.2 缓动函数

| 效果 | 缓动 | 用途 |
|-----|-----|-----|
| 弹入 | OutBack | 面板、弹窗打开 |
| 弹出 | InBack | 面板、弹窗关闭 |
| 滑动 | OutQuad/InQuad | 侧边栏、列表 |
| 淡入淡出 | OutQuad/InQuad | 背景、提示 |
| 弹性 | OutElastic | 强调效果 |
| 弹跳 | OutBounce | 庆祝效果 |

### 5.3 过渡类型

1. **Fade (淡入淡出)**: 通用过渡
2. **Slide (滑动)**: 四方向滑动
3. **Scale (缩放)**: 中心缩放
4. **Glitch (故障)**: 赛博朋克特效
5. **Digital Scan (数字扫描)**: 科技感过渡

## 6. 交互规范

### 6.1 触控优化

#### 触控区域
- 最小触控尺寸: 44x44px (苹果标准)
- 推荐触控尺寸: 48x48px
- 触控间距: 最小 8px

#### 反馈
- 触觉反馈: 支持设备震动
- 视觉反馈: 触控波纹效果
- 音效反馈: 点击音效

### 6.2 手势支持

| 手势 | 操作 | 反馈 |
|-----|-----|-----|
| 点击 | 选择/确认 | 缩放+音效 |
| 长按 | 菜单/详情 | 震动+波纹 |
| 滑动 | 切换/滚动 | 惯性动画 |
| 双击 | 放大/特殊 | 快速反馈 |

### 6.3 错误处理

#### 视觉反馈
- 红色闪烁: 0.15s 间隔，闪烁3次
- 震动效果: 0.3s，强度10
- 错误提示: 弹窗显示，2s自动消失

#### 音效反馈
- 错误音效: 低音调，短促
- 警告音效: 中音调，持续

## 7. 特效规范

### 7.1 粒子效果

#### 点击粒子
- 数量: 6-8个
- 颜色: 赛博青
- 大小: 0.05-0.1
- 生命周期: 0.5s

#### 成功粒子
- 数量: 20+个
- 颜色: 成功绿 + 赛博青
- 效果: 彩纸爆炸
- 生命周期: 2-3s

### 7.2 发光效果

#### 强度等级
| 等级 | 强度 | 用途 |
|-----|-----|-----|
| 正常 | 0.5 | 基础发光 |
| 悬停 | 0.8 | 交互状态 |
| 选中 | 1.0 | 选中状态 |
| 警告 | 1.2 | 警告状态 |
| 最大 | 1.5 | 强调效果 |

### 7.3 扫描线

- 颜色: 霓虹紫，Alpha 0.3
- 速度: 2-3秒/屏
- 宽度: 2-4px
- 方向: 水平或垂直

## 8. 性能规范

### 8.1 动画性能

- 使用 DOTween 对象池
- 限制同时播放的动画数量 (< 50)
- 使用 CanvasGroup 控制整体显隐
- 避免在动画中修改材质属性

### 8.2 粒子性能

- 限制粒子数量 (< 100)
- 使用对象池复用粒子
- 及时销毁过期粒子
- 低画质模式减少粒子

### 8.3 分辨率适配

- 使用 CanvasScaler 统一适配
- 4K分辨率自动缩放字体
- 超宽屏限制中心内容宽度
- 移动端启用触控优化

## 9. 实现检查清单

### 9.1 UI Polish
- [ ] 主菜单动态背景
- [ ] 按钮悬停光晕效果
- [ ] 面板边框动画
- [ ] 字体统一和优化

### 9.2 动画
- [ ] 界面切换过渡
- [ ] 弹窗弹出/关闭
- [ ] 通知提示动画
- [ ] 加载界面动画

### 9.3 响应式
- [ ] 16:9适配
- [ ] 21:9超宽屏
- [ ] 4K分辨率
- [ ] 移动端触控

### 9.4 交互反馈
- [ ] 点击反馈
- [ ] 错误提示
- [ ] 成功庆祝
- [ ] 警告动画

## 10. 文件清单

```
Assets/Scripts/UI/Polish/
├── MainMenuVisualManager.cs      # 主菜单视觉
├── EnhancedButtonEffect.cs       # 按钮效果
├── CyberPanelBorder.cs           # 面板边框
├── TransitionManager.cs          # 过渡管理
├── PopupAnimator.cs              # 弹窗动画
├── LoadingScreenAnimator.cs      # 加载动画
├── InteractionFeedbackManager.cs # 交互反馈
├── FontManager.cs                # 字体管理
├── UltrawideAdapter.cs           # 超宽屏适配
├── Resolution4KAdapter.cs        # 4K适配
├── MobileTouchOptimizer.cs       # 触控优化
└── README.md                     # 使用文档
```

## 附录

### A. 颜色常量代码

```csharp
// 主题色
public static readonly Color PRIMARY = new Color(0.2f, 0.9f, 1f, 1f);
public static readonly Color SECONDARY = new Color(0.8f, 0.2f, 0.9f, 1f);
public static readonly Color ACCENT = new Color(1f, 0.6f, 0.1f, 1f);

// 状态色
public static readonly Color STATE_SUCCESS = new Color(0.2f, 1f, 0.4f, 1f);
public static readonly Color STATE_WARNING = new Color(0.95f, 0.8f, 0.1f, 1f);
public static readonly Color STATE_DANGER = new Color(0.95f, 0.2f, 0.1f, 1f);
```

### B. 动画时长常量

```csharp
public static class AnimationDurations
{
    public const float BUTTON_CLICK = 0.1f;
    public const float HOVER_TRANSITION = 0.15f;
    public const float PANEL_OPEN = 0.4f;
    public const float PANEL_CLOSE = 0.3f;
    public const float POPUP_SHOW = 0.3f;
    public const float POPUP_HIDE = 0.2f;
}
```

### C. 缓动函数常量

```csharp
public static class EasingConstants
{
    public static readonly Ease EASE_IN_POP = Ease.OutBack;
    public static readonly Ease EASE_OUT_POP = Ease.InBack;
    public static readonly Ease EASE_HOVER_IN = Ease.OutBack;
    public static readonly Ease EASE_HOVER_OUT = Ease.OutQuad;
}
```

---

**版本**: 1.0  
**更新日期**: 2026-02-27  
**作者**: UI美化师
