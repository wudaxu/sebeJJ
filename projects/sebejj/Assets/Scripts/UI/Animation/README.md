# SebeJJ UI动效规范文档

## 目录
1. [动画时长标准](#动画时长标准)
2. [缓动函数规范](#缓动函数规范)
3. [颜色变化规则](#颜色变化规则)
4. [预制体说明](#预制体说明)
5. [使用示例](#使用示例)

---

## 动画时长标准

### 即时反馈 (0.1s - 0.25s)
| 动画类型 | 时长 | 说明 |
|---------|------|------|
| 按钮点击 | 0.1s | 快速收缩反馈 |
| 悬停变化 | 0.15s | 平滑状态切换 |
| 开关切换 | 0.25s | 带过冲效果 |
| 滑块拖动 | 0.2s | 即时响应 |

### 界面过渡 (0.3s - 0.5s)
| 动画类型 | 时长 | 说明 |
|---------|------|------|
| 面板打开 | 0.4s | 弹入效果 |
| 面板关闭 | 0.3s | 加速退出 |
| 菜单切换 | 0.35s | 平滑过渡 |
| 弹窗显示 | 0.3s | 带弹性 |

### 列表动画
| 动画类型 | 时长 | 说明 |
|---------|------|------|
| 项进入间隔 | 0.03s | 依次出现 |
| 项动画时长 | 0.25s | 单个项动画 |
| 滚动平滑 | 0.3s | 惯性滚动 |

### 战斗UI
| 动画类型 | 时长 | 说明 |
|---------|------|------|
| 伤害数字弹出 | 0.15s | 快速过冲 |
| 伤害数字浮动 | 0.8s | 减速上升 |
| 伤害数字消失 | 0.3s | 加速淡出 |
| 血条变化 | 0.3s | 平滑过渡 |
| 护盾破碎 | 0.5s | 裂纹+飞散 |
| 连击变化 | 0.15s | 弹性缩放 |

### 特殊效果
| 动画类型 | 时长 | 说明 |
|---------|------|------|
| 资源飞行 | 0.8s | 贝塞尔曲线 |
| 升级动画 | 1.5s | 光环填充 |
| 警告闪烁 | 0.3s | 循环脉冲 |

---

## 缓动函数规范

### 进入动画
```csharp
// 弹入 - 用于面板、弹窗
Ease.OutBack

// 滑入 - 用于侧边栏
Ease.OutQuad

// 淡入 - 用于背景
Ease.OutQuad

// 缩放进入 - 用于小元素
Ease.OutBack
```

### 退出动画
```csharp
// 弹出消失
Ease.InBack

// 滑出消失
Ease.InQuad

// 淡出消失
Ease.InQuad

// 缩放消失
Ease.InBack
```

### 交互反馈
```csharp
// 按钮按下 - 快速
Ease.OutQuad

// 按钮释放 - 弹性
Ease.OutBack

// 悬停进入
Ease.OutBack

// 悬停退出
Ease.OutQuad

// 选中状态
Ease.OutBack
```

### 特殊效果
```csharp
// 脉冲循环
Ease.InOutSine

// 震动效果
Ease.OutElastic

// 弹跳
Ease.OutBounce

// 战斗伤害弹出
Ease.OutBack

// 战斗伤害消失
Ease.InQuad
```

---

## 颜色变化规则

### 主题色
| 名称 | 颜色值 | 用途 |
|-----|--------|------|
| 主色调 | #33E6FF | 赛博青，按钮、高亮 |
| 次要色 | #CC33E6 | 霓虹紫，特殊元素 |
| 强调色 | #FF991A | 能量橙，警告、重要 |
| 背景色 | #0D0D14 | 深空黑，背景 |
| 面板色 | #1A1A26 | 半透明黑，面板 |

### 状态色
| 状态 | 颜色值 | 用途 |
|-----|--------|------|
| 正常 | #33E64D | 可用、正常 |
| 警告 | #F2CC1A | 注意、中等 |
| 危险 | #F2331A | 错误、严重 |
| 信息 | #3399FF | 提示、信息 |
| 成功 | #33FF66 | 完成、成功 |
| 禁用 | #666666 | 锁定、禁用 |

### 血条颜色
| 血量 | 颜色值 | 说明 |
|-----|--------|------|
| 高 (>60%) | #33E64D | 绿色 |
| 中 (30-60%) | #F2CC1A | 黄色 |
| 低 (<30%) | #F2331A | 红色，触发警告 |

### 连击颜色
| 连击数 | 颜色值 | 说明 |
|-------|--------|------|
| 0-5 | #FFFFFF | 白色 |
| 6-10 | #FFE64D | 黄色 |
| 11-20 | #FF801A | 橙色 |
| 21-30 | #FF3333 | 红色 |
| 31-50 | #CC33CC | 紫色 |
| 50+ | #33CCFF | 青色 |

### 稀有度颜色
| 稀有度 | 颜色值 | 说明 |
|-------|--------|------|
| 普通 | #B3B3B3 | 灰色 |
| 稀有 | #33CC33 | 绿色 |
| 史诗 | #3366FF | 蓝色 |
| 传说 | #CC33CC | 紫色 |
| 神话 | #FF991A | 橙色 |

---

## 预制体说明

### 1. MenuButton
**组件结构:**
```
MenuButton (GameObject)
├── RectTransform
├── Image (按钮背景)
├── Button
├── MenuButtonAnimator
└── Text (按钮文字)
```

**配置参数:**
- `hoverScale`: 1.1 (悬停缩放)
- `hoverDuration`: 0.2s
- `selectedScale`: 1.15 (选中缩放)
- `transitionOutDuration`: 0.3s

### 2. InventoryPanel
**组件结构:**
```
InventoryPanel (GameObject)
├── RectTransform
├── CanvasGroup
├── InventoryAnimator
└── Content (物品槽容器)
    └── Slot (多个)
        ├── InventorySlotAnimator
        ├── Image (槽背景)
        ├── Image (物品图标)
        └── Image (稀有度光效)
```

### 3. DamageNumber
**组件结构:**
```
DamageNumber (GameObject)
├── RectTransform
├── CanvasGroup
├── Text (伤害数值)
├── Outline (描边)
└── DamageNumberAnimator
```

**使用方式:**
```csharp
var damageNumber = Instantiate(damageNumberPrefab);
damageNumber.ShowDamage(100, isCritical: true);
```

### 4. HealthBar
**组件结构:**
```
HealthBar (GameObject)
├── RectTransform
├── Slider
│   ├── Background
│   └── Fill Area
│       └── Fill (Image)
├── Image (延迟血条)
├── Image (警告覆盖层)
└── HealthBarAnimator
```

### 5. ShieldIcon
**组件结构:**
```
ShieldIcon (GameObject)
├── RectTransform
├── Image (护盾图标)
├── Image (发光效果)
├── ShieldBreakAnimator
└── ParticleSystem (破碎粒子)
```

### 6. ComboCounter
**组件结构:**
```
ComboCounter (GameObject)
├── RectTransform
├── CanvasGroup
├── Text (连击数)
├── Text ("COMBO"标签)
├── Image (发光背景)
├── ParticleSystem
└── ComboCounterAnimator
```

### 7. ResourceFlyer
**组件结构:**
```
ResourceFlyer (GameObject)
├── RectTransform
├── CanvasGroup
├── Image (资源图标)
├── Text (数量)
└── ResourceGainAnimator
```

### 8. QuestCompletePanel
**组件结构:**
```
QuestCompletePanel (GameObject)
├── RectTransform
├── CanvasGroup
├── Image (背景遮罩)
├── Text ("完成"标题)
├── RectTransform (星星容器)
│   └── Star (多个Image)
├── RectTransform (奖励容器)
├── ParticleSystem (彩纸)
├── Button (继续按钮)
└── QuestCompleteAnimator
```

### 9. LevelUpPanel
**组件结构:**
```
LevelUpPanel (GameObject)
├── RectTransform
├── CanvasGroup
├── Image (光效射线)
├── Image (升级环)
├── Text (旧等级)
├── Text (新等级)
├── RectTransform (属性容器)
├── ParticleSystem
└── LevelUpAnimator
```

### 10. WarningAlert
**组件结构:**
```
WarningAlert (GameObject)
├── RectTransform
├── CanvasGroup
├── Image (背景)
├── Image (边框)
├── Image (警告图标)
├── Text (警告文字)
└── WarningAlertAnimator
```

---

## 使用示例

### 菜单按钮
```csharp
// 获取动画器
var buttonAnimator = button.GetComponent<MenuButtonAnimator>();

// 设置选中
buttonAnimator.SetSelected(true);

// 播放过渡动画
buttonAnimator.PlayTransitionOut(() => {
    // 切换场景
});
```

### 背包界面
```csharp
// 打开背包
inventoryAnimator.Open();

// 关闭背包
inventoryAnimator.Close();

// 播放获得物品动画
inventoryAnimator.PlayItemAcquired(worldPosition, itemIcon, () => {
    // 添加物品到背包
});
```

### 血条更新
```csharp
// 更新血量
healthBarAnimator.SetHealth(currentHP, maxHP);

// 预览伤害
healthBarAnimator.PreviewDamage(damageAmount, maxHP);

// 取消预览
healthBarAnimator.CancelDamagePreview();
```

### 护盾破碎
```csharp
// 播放破碎动画
shieldAnimator.PlayBreakAnimation();

// 恢复护盾
shieldAnimator.PlayRestoreAnimation();

// 受击但未破碎
shieldAnimator.PlayHitAnimation();
```

### 连击计数
```csharp
// 增加连击
comboAnimator.AddCombo();

// 重置连击
comboAnimator.ResetCombo();

// 获取当前连击数
int combo = comboAnimator.GetCurrentCombo();
```

### 警告提示
```csharp
// 显示普通警告
warningAnimator.ShowWarning("能量不足");

// 显示严重警告
warningAnimator.ShowWarning("护盾即将破碎！", critical: true);

// 隐藏警告
warningAnimator.HideWarning();
```

---

## 依赖项

- **DOTween**: 动画引擎 (必需)
- **Unity UI**: uGUI系统 (必需)
- **TextMeshPro**: 可选，用于更好的文字效果

## 性能建议

1. **对象池**: 伤害数字、资源飞行使用对象池
2. **动画复用**: 使用DOTween的SetAutoKill(false)复用动画
3. **Canvas优化**: 动态UI使用单独Canvas
4. **粒子控制**: 限制同时播放的粒子数量

---

*文档版本: 1.0*
*更新日期: 2026-02-27*
