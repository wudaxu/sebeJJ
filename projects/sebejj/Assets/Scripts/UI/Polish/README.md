# SebeJJ UI Polish 系统

赛博机甲SebeJJ的UI美化和动画完善系统

## 目录结构

```
Assets/Scripts/UI/Polish/
├── MainMenuVisualManager.cs      # 主菜单视觉管理器
├── EnhancedButtonEffect.cs       # 增强按钮效果
├── CyberPanelBorder.cs           # 赛博朋克面板边框
├── TransitionManager.cs          # 界面切换过渡管理器
├── PopupAnimator.cs              # 弹窗动画管理器
├── LoadingScreenAnimator.cs      # 加载界面动画
├── InteractionFeedbackManager.cs # 交互反馈管理器
├── FontManager.cs                # 字体管理器
├── UltrawideAdapter.cs           # 超宽屏适配器
├── Resolution4KAdapter.cs        # 4K分辨率适配器
└── MobileTouchOptimizer.cs       # 移动端触控优化
```

## 功能模块

### 1. UI Polish

#### MainMenuVisualManager (主菜单视觉优化)
- 动态背景效果（星云/网格）
- 粒子系统（环境粒子、能量粒子、数据流）
- 角落装饰动画
- 扫描线效果
- 暗角遮罩
- 进入/退出动画

#### EnhancedButtonEffect (按钮悬停效果增强)
- 光晕效果（悬停脉冲）
- 缩放动画（悬停放大、点击收缩）
- 边框高亮
- 文字效果
- 点击粒子
- 音效反馈

#### CyberPanelBorder (面板边框美化)
- 动态边框绘制
- 角标装饰
- 扫描线动画
- 发光脉冲
- 警告闪烁
- 进入动画

#### FontManager (字体统一和优化)
- 字体预设系统（标题/副标题/正文/说明/代码）
- 描边效果
- 阴影效果
- 发光效果
- 打字机效果
- 文字闪烁/渐变/脉冲

### 2. 动画完善

#### TransitionManager (界面切换过渡)
- 淡入淡出
- 滑动过渡（四方向）
- 赛博朋克故障效果
- 数字扫描效果
- 缩放过渡

#### PopupAnimator (弹窗动画)
- 缩放弹出
- 滑动弹出（上下）
- 淡入淡出
- 弹跳效果
- 翻转效果
- 通知提示（带自动隐藏）

#### LoadingScreenAnimator (加载界面)
- 旋转加载器
- 进度条动画
- 数据块进度
- 扫描线效果
- 故障闪烁
- 代码雨效果
- 全息框架动画

### 3. 响应式适配

#### UltrawideAdapter (21:9超宽屏支持)
- 自动检测屏幕比例
- 侧边面板偏移
- 中心内容限制
- 背景适配
- 侧边装饰

#### Resolution4KAdapter (4K分辨率修复)
- 分辨率类型检测
- CanvasScaler自动配置
- 字体自动缩放
- UI元素缩放
- 贴图质量调整

### 4. 交互反馈

#### InteractionFeedbackManager (交互反馈)
- 点击反馈（音效/粒子/震动）
- 错误提示（震动/红色闪烁）
- 成功庆祝（彩纸/光环）
- 警告动画（脉冲/闪烁）
- 摄像机震动

#### MobileTouchOptimizer (移动端触控优化)
- 触控区域优化
- 长按检测
- 滑动手势
- 双击检测
- 触控波纹反馈
- 触觉反馈

## 使用方法

### 主菜单视觉

```csharp
// 显示主菜单
MainMenuVisualManager.Instance.PlayEnterAnimation();

// 隐藏主菜单
MainMenuVisualManager.Instance.PlayExitAnimation(() => {
    // 动画完成回调
});

// 设置主题色
MainMenuVisualManager.Instance.SetThemeColors(
    new Color(0.2f, 0.9f, 1f),  // 主色
    new Color(0.8f, 0.2f, 0.9f), // 副色
    new Color(1f, 0.6f, 0.1f)    // 强调色
);
```

### 按钮效果

```csharp
// 按钮上添加 EnhancedButtonEffect 组件即可自动生效
// 或通过代码配置
var buttonEffect = GetComponent<EnhancedButtonEffect>();
buttonEffect.SetInteractable(false);
```

### 界面过渡

```csharp
// 淡入淡出过渡
TransitionManager.Instance.FadeTransition(
    () => { /* 过渡开始 */ },
    () => { /* 过渡结束 */ },
    0.4f // 持续时间
);

// 滑动过渡
TransitionManager.Instance.SlideTransition(
    () => { },
    () => { },
    SlideDirection.Right,
    0.35f
);

// 故障效果
TransitionManager.Instance.GlitchTransition(
    () => { },
    () => { }
);
```

### 弹窗动画

```csharp
// 显示弹窗
PopupAnimator.Instance.ShowPopup(
    popupRectTransform,
    PopupAnimationType.Scale,
    () => { /* 显示完成 */ }
);

// 隐藏弹窗
PopupAnimator.Instance.HidePopup(
    popupRectTransform,
    PopupAnimationType.Scale,
    () => { /* 隐藏完成 */ }
);

// 显示通知
PopupAnimator.Instance.ShowNotification(
    notificationRectTransform,
    NotificationType.Success,
    3f, // 自动隐藏延迟
    () => { /* 显示完成 */ }
);
```

### 加载界面

```csharp
// 显示加载界面
LoadingScreenAnimator.Instance.ShowLoadingScreen("自定义提示文本");

// 更新进度
LoadingScreenAnimator.Instance.SetProgress(0.5f);

// 更新提示
LoadingScreenAnimator.Instance.UpdateLoadingTip("加载中...");

// 隐藏加载界面
LoadingScreenAnimator.Instance.HideLoadingScreen(() => {
    // 隐藏完成
});
```

### 交互反馈

```csharp
// 点击反馈
InteractionFeedbackManager.Instance.PlayClickFeedback(position, buttonRect);

// 错误提示
InteractionFeedbackManager.Instance.ShowError(
    "错误信息",
    targetRect,
    2f // 持续时间
);

// 成功庆祝
InteractionFeedbackManager.Instance.PlaySuccessCelebration(
    position,
    "任务完成！"
);

// 警告
InteractionFeedbackManager.Instance.ShowWarning(
    "警告信息",
    targetRect,
    3f
);
```

### 字体管理

```csharp
// 应用字体预设
FontManager.Instance.ApplyPreset(textComponent, FontPresetType.Title);

// 打字机效果
FontManager.Instance.PlayTypewriterEffect(
    textComponent,
    "要显示的文本",
    0.05f, // 速度
    () => { /* 完成回调 */ }
);

// 文字脉冲
FontManager.Instance.PlayPulseEffect(textComponent);
```

### 响应式适配

```csharp
// 手动触发适配
UltrawideAdapter.Instance.ApplyUltrawideAdaptation();

// 检查屏幕类型
if (UltrawideAdapter.Instance.IsUltrawide())
{
    // 超宽屏特殊处理
}

// 4K适配
Resolution4KAdapter.Instance.ApplyResolutionAdaptation();

// 获取缩放因子
float scale = Resolution4KAdapter.Instance.GetCurrentScaleFactor();
```

## 配色规范

### 主题色
- **主色 (Primary)**: #33E6FF (赛博青)
- **副色 (Secondary)**: #CC33E6 (霓虹紫)
- **强调色 (Accent)**: #FF991A (能量橙)
- **背景色 (Background)**: #0D0D14 (深空黑)
- **面板色 (Panel)**: #1A1A26 (半透明黑)

### 状态色
- **正常 (Normal)**: #33E64D
- **警告 (Warning)**: #F2CC1A
- **危险 (Danger)**: #F2331A
- **信息 (Info)**: #3399FF
- **成功 (Success)**: #33FF66

## 动画时长规范

| 动画类型 | 时长 | 缓动 |
|---------|------|------|
| 按钮点击 | 0.1s | OutQuad |
| 悬停过渡 | 0.15s | OutBack |
| 面板打开 | 0.4s | OutBack |
| 面板关闭 | 0.3s | InBack |
| 弹窗显示 | 0.3s | OutBack |
| 弹窗隐藏 | 0.2s | InBack |
| 列表项间隔 | 0.03s | - |
| 伤害数字 | 0.8s | OutQuad |

## 安装步骤

1. 确保已安装 DOTween 插件
2. 将 Polish 文件夹复制到 `Assets/Scripts/UI/`
3. 在场景中创建 UIManager 空物体
4. 添加所需的 Manager 组件
5. 配置引用和参数

## 依赖

- Unity 2019.4 或更高版本
- DOTween (HOTween v2)
- TextMeshPro (可选，用于高级字体效果)

## 注意事项

1. 所有 Manager 类都是单例模式，使用 `Instance` 访问
2. 动画使用 DOTween，确保在场景中存在 DOTween 实例
3. 粒子效果需要配置相应的粒子预制体
4. 音效需要配置相应的音频资源
5. 移动端触控优化仅在触控设备上生效
