# 机械水母 (MechJellyfish) 美术资源设计说明

## 资源清单

### Idle 动画 (3帧)
- `mech_jellyfish_idle_01.svg` - 基础漂浮
- `mech_jellyfish_idle_02.svg` - 收缩上浮
- `mech_jellyfish_idle_03.svg` - 展开下沉

## 设计规范

### 配色方案
- **主色调**: 电光紫 (#8B5CF6)
- **身体填充**: 深海黑 (#0D1B2A)
- **伞盖纹理**: 半透明紫色线条
- **核心发光**: 电光紫渐变
- **触手**: 深灰蓝渐变

### 结构特征
1. **伞盖主体**: 椭圆形半透明结构
2. **内部纹理**: 同心圆 + 十字分割线
3. **发光节点**: 伞盖边缘6个发光点
4. **触手系统**: 5条机械触手，末端发光
5. **核心**: 中央发光球体

### 动画特点
- **Idle**: 伞盖收缩/展开循环，触手随波摆动
- **节奏**: 收缩(上浮) → 展开(下沉) 循环

## PNG导出规格

| 参数 | 规格 |
|------|------|
| 分辨率 | 256x256 px |
| 格式 | PNG-24 (透明通道) |
| 色彩空间 | sRGB |
| 文件名格式 | mech_jellyfish_[动作]_[帧号].png |

## 使用建议

### 动画帧率
- Idle: 6 FPS (循环)

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
