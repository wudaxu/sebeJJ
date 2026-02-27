# PNG导出规格总览

## 导出工具推荐
- Adobe Illustrator
- Inkscape (免费)
- Figma
- 在线SVG转PNG工具

## 通用导出设置

### 基础设置
```
格式: PNG
色彩模式: RGB
透明度: 启用
抗锯齿: 启用
嵌入ICC配置文件: 否
```

### 压缩设置
```
压缩级别: 无损 (或最高质量)
交错: 无
```

---

## 各资源导出规格

### 1. 角色动画帧

#### 机械鱼 (MechFish)
| 参数 | 值 |
|------|-----|
| 画布尺寸 | 256x256 px |
| 导出尺寸 | 256x256 px |
| DPI | 72 |
| 背景 | 透明 |
| 文件名格式 | `mech_fish_[action]_[frame].png` |

**文件列表:**
- mech_fish_idle_01.png ~ mech_fish_idle_03.png (3帧)
- mech_fish_move_01.png ~ mech_fish_move_06.png (6帧)
- mech_fish_attack_01.png ~ mech_fish_attack_04.png (4帧)

#### 机械蟹 (MechCrab)
| 参数 | 值 |
|------|-----|
| 画布尺寸 | 256x256 px |
| 导出尺寸 | 256x256 px |
| DPI | 72 |
| 背景 | 透明 |
| 文件名格式 | `mech_crab_[action]_[frame].png` |

**文件列表:**
- mech_crab_idle_01.png ~ mech_crab_idle_04.png (4帧)
- mech_crab_move_01.png ~ mech_crab_move_06.png (6帧)
- mech_crab_defend_01.png ~ mech_crab_defend_02.png (2帧)

#### 机械水母 (MechJellyfish) - P1
| 参数 | 值 |
|------|-----|
| 画布尺寸 | 256x256 px |
| 导出尺寸 | 256x256 px |
| DPI | 72 |
| 背景 | 透明 |
| 文件名格式 | `mech_jellyfish_[action]_[frame].png` |

**文件列表:**
- mech_jellyfish_idle_01.png ~ mech_jellyfish_idle_03.png (3帧)

---

### 2. UI状态条 (9-patch)

| 参数 | 值 |
|------|-----|
| 画布尺寸 | 128x32 px |
| 导出尺寸 | 128x32 px |
| DPI | 72 |
| 背景 | 透明 |
| 文件名格式 | `ui_[type]_bar.9.png` |

**9-patch标记:**
```
在PNG周围添加1px透明边框:
- 顶部: 16px开始，96px可拉伸区域标记为黑色
- 左侧: 8px开始，16px可拉伸区域标记为黑色
- 右侧/底部: 内容区域标记 (可选)
```

**文件列表:**
- ui_health_bar.9.png
- ui_energy_bar.9.png
- ui_oxygen_bar.9.png

---

### 3. 战斗特效

| 参数 | 值 |
|------|-----|
| 画布尺寸 | 128x128 px |
| 导出尺寸 | 128x128 px |
| DPI | 72 |
| 背景 | 透明 |
| 文件名格式 | `fx_[name].png` |

**文件列表:**
- fx_hit_spark.png
- fx_explosion_small.png
- fx_shield_impact.png
- fx_drill_spark.png (P1)
- fx_laser_beam.png (P1)

---

### 4. 武器图标

| 参数 | 值 |
|------|-----|
| 画布尺寸 | 64x64 px |
| 导出尺寸 | 64x64 px |
| DPI | 72 |
| 背景 | 透明 |
| 文件名格式 | `weapon_icon_[name].png` |

**文件列表:**
- weapon_icon_drill.png
- weapon_icon_claw.png
- weapon_icon_laser.png

---

## 批量导出脚本 (Inkscape)

```bash
#!/bin/bash
# 批量导出SVG到PNG

INPUT_DIR="./"
OUTPUT_DIR="../Exported/"
DPI=72

for file in $INPUT_DIR/*.svg; do
    filename=$(basename "$file" .svg)
    inkscape "$file" \
        --export-filename="$OUTPUT_DIR/${filename}.png" \
        --export-dpi=$DPI \
        --export-background-opacity=0
done
```

---

## Unity导入设置模板

### 角色精灵
```yaml
TextureType: Sprite (2D and UI)
SpriteMode: Single
PixelsPerUnit: 100
FilterMode: Point (或 Bilinear)
Compression: None
```

### UI元素
```yaml
TextureType: Sprite (2D and UI)
SpriteMode: Single
MeshType: Full Rect
PixelsPerUnit: 100
Border: [根据9-patch设置]
FilterMode: Bilinear
Compression: None
```

### 特效
```yaml
TextureType: Sprite (2D and UI)
SpriteMode: Single
PixelsPerUnit: 100
FilterMode: Bilinear
Compression: None
WrapMode: Clamp
```

---

## 文件组织结构

```
Assets/
└── Art/
    ├── Characters/
    │   ├── MechFish/
    │   │   ├── mech_fish_idle_01.png
    │   │   ├── ...
    │   │   └── mech_fish_attack_04.png
    │   ├── MechCrab/
    │   │   └── ...
    │   └── MechJellyfish/
    │       └── ...
    ├── UI/
    │   ├── ui_health_bar.9.png
    │   ├── ui_energy_bar.9.png
    │   └── ui_oxygen_bar.9.png
    ├── Effects/
    │   ├── fx_hit_spark.png
    │   ├── fx_explosion_small.png
    │   ├── fx_shield_impact.png
    │   ├── fx_drill_spark.png
    │   └── fx_laser_beam.png
    └── Weapons/
        ├── weapon_icon_drill.png
        ├── weapon_icon_claw.png
        └── weapon_icon_laser.png
```

---

*文档版本: 1.0*
*更新日期: 2026-02-27*
