# 机械鱼 (MechFish) 美术资源设计说明

## 资源清单

### Idle 动画 (3帧)
- `mech_fish_idle_01.svg` - 基础姿态
- `mech_fish_idle_02.svg` - 轻微上浮
- `mech_fish_idle_03.svg` - 回到原位

### Move 动画 (6帧)
- `mech_fish_move_01.svg` - 开始游动
- `mech_fish_move_02.svg` - 尾摆中
- `mech_fish_move_03.svg` - 尾摆极左
- `mech_fish_move_04.svg` - 回摆开始
- `mech_fish_move_05.svg` - 右摆
- `mech_fish_move_06.svg` - 循环回位

### Attack 动画 (4帧)
- `mech_fish_attack_01.svg` - 蓄力
- `mech_fish_attack_02.svg` - 蓄力峰值
- `mech_fish_attack_03.svg` - 发射
- `mech_fish_attack_04.svg` - 后摇恢复

## 设计规范

### 配色方案
- **主色调**: 赛博青 (#00F0FF)
- **身体填充**: 深海黑 (#0D1B2A)
- **装甲板块**: 深灰蓝 (#1A1A2E)
- **核心发光**: 赛博青渐变
- **攻击状态**: 霓虹粉 (#FF00A0)

### 结构特征
1. **流线型身体**: 椭圆主体，适合水下移动
2. **分段装甲**: 身体分为6块装甲板，体现机械感
3. **发光核心**: 胸部中央有圆形反应堆
4. **探照灯眼睛**: 头部双灯设计，攻击时变红
5. **推进器**: 尾部发光喷口，移动时加强

### 动画特点
- **Idle**: 轻微上下浮动，尾鳍和侧鳍缓慢摆动
- **Move**: 尾鳍左右摆动，身体轻微压缩伸展，推进器发光增强
- **Attack**: 身体后缩蓄力，眼睛变红，武器充能，发射光束

## PNG导出规格

| 参数 | 规格 |
|------|------|
| 分辨率 | 256x256 px |
| 格式 | PNG-24 (透明通道) |
| 色彩空间 | sRGB |
| 文件名格式 | mech_fish_[动作]_[帧号].png |

### 导出设置
```
- 抗锯齿: 开启
- 透明背景: 是
- 嵌入色彩配置文件: 否
- 压缩: 无损
```

## 使用建议

### 动画帧率
- Idle: 8 FPS (循环)
- Move: 12 FPS (循环)
- Attack: 15 FPS (单次播放)

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
