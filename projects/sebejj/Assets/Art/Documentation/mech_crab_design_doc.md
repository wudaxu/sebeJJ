# 机械蟹 (MechCrab) 美术资源设计说明

## 资源清单

### Idle 动画 (4帧)
- `mech_crab_idle_01.svg` - 基础姿态
- `mech_crab_idle_02.svg` - 身体微抬
- `mech_crab_idle_03.svg` - 回到原位
- `mech_crab_idle_04.svg` - 螯微动

### Move 动画 (6帧)
- `mech_crab_move_01.svg` - 起步
- `mech_crab_move_02.svg` - 迈步中
- `mech_crab_move_03.svg` - 交替迈步
- `mech_crab_move_04.svg` - 继续迈步
- `mech_crab_move_05.svg` - 交替循环
- `mech_crab_move_06.svg` - 循环回位

### Defend 动画 (2帧)
- `mech_crab_defend_01.svg` - 防御姿态
- `mech_crab_defend_02.svg` - 护盾强化

## 设计规范

### 配色方案
- **主色调**: 赛博青 (#00F0FF)
- **身体填充**: 深海黑 (#0D1B2A)
- **装甲板块**: 深灰蓝 (#1A1A2E)
- **核心发光**: 赛博青渐变
- **护盾**: 电光紫 (#8B5CF6)

### 结构特征
1. **六足设计**: 左右各3条机械腿，关节分明
2. **双螯武器**: 可开合的机械钳，防御时护在身前
3. **方形身体**: 圆角矩形主体，四块装甲板
4. **发光眼睛**: 顶部双灯，触须状支撑
5. **能量护盾**: 防御时展开六边形网格护盾

### 动画特点
- **Idle**: 身体轻微起伏，螯缓慢开合
- **Move**: 六足交替移动，身体左右微倾
- **Defend**: 身体降低，双螯护前，护盾展开

## PNG导出规格

| 参数 | 规格 |
|------|------|
| 分辨率 | 256x256 px |
| 格式 | PNG-24 (透明通道) |
| 色彩空间 | sRGB |
| 文件名格式 | mech_crab_[动作]_[帧号].png |

### 导出设置
```
- 抗锯齿: 开启
- 透明背景: 是
- 嵌入色彩配置文件: 否
- 压缩: 无损
```

## 使用建议

### 动画帧率
- Idle: 6 FPS (循环)
- Move: 10 FPS (循环)
- Defend: 8 FPS (可循环或保持)

### Unity导入设置
```
Texture Type: Sprite (2D and UI)
Sprite Mode: Single
Pixels Per Unit: 100
Filter Mode: Point (像素风) / Bilinear (平滑)
Compression: None
```

## 版本记录
- v1.0 - 2026-02-27 - 初始版本
