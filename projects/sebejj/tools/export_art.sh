#!/bin/bash
# SebeJJ Art Asset Export Script
# SVG to PNG conversion with optimization

set -e

PROJECT_ROOT="/root/.openclaw/workspace/projects/sebejj"
EXPORT_DIR="$PROJECT_ROOT/Assets/Art/Exported"
SOURCE_DIR="$PROJECT_ROOT/Assets/Art"

# Export specifications
CHAR_SIZE=256
FX_SIZE=128
ICON_SIZE=64
UI_SIZE=512

# Ensure export directories exist
mkdir -p "$EXPORT_DIR"/{Characters,UI,FX,Effects,Weapons,Backgrounds,Items,Environment,Animations}

# Function to convert SVG to PNG with specified size
convert_svg() {
    local input="$1"
    local output="$2"
    local size="$3"
    local name=$(basename "$input" .svg)
    
    echo "Converting: $name → ${size}x${size}px"
    
    # Use ImageMagick convert with high quality settings
    convert -background none -density 300 "$input" -resize "${size}x${size}>" -format PNG32 "$output" 2>/dev/null || \
    convert -background none "$input" -resize "${size}x${size}>" -format PNG32 "$output"
    
    # Optimize with pngquant if available
    if command -v pngquant &> /dev/null; then
        pngquant --force --quality=80-95 --output "$output.tmp" "$output" 2>/dev/null && mv "$output.tmp" "$output" || true
    fi
}

# Export Characters (256x256)
echo "=== Exporting Characters ==="
for svg in "$SOURCE_DIR"/Characters/*/*.svg; do
    [ -f "$svg" ] || continue
    name=$(basename "$svg" .svg)
    convert_svg "$svg" "$EXPORT_DIR/Characters/${name}.png" $CHAR_SIZE
done

# Export Mecha Base
echo "=== Exporting Mecha Base ==="
if [ -f "$SOURCE_DIR/Characters/Mecha_Mk1_Base.svg" ]; then
    convert_svg "$SOURCE_DIR/Characters/Mecha_Mk1_Base.svg" "$EXPORT_DIR/Characters/Mecha_Mk1_Base.png" $CHAR_SIZE
fi

# Export Animations
echo "=== Exporting Animations ==="
for svg in "$SOURCE_DIR"/Animations/*.svg "$SOURCE_DIR"/Animations/Frames/*.svg; do
    [ -f "$svg" ] || continue
    name=$(basename "$svg" .svg)
    convert_svg "$svg" "$EXPORT_DIR/Animations/${name}.png" $CHAR_SIZE
done

# Export Effects (128x128 or 256x256)
echo "=== Exporting Effects ==="
for svg in "$SOURCE_DIR"/Effects/*.svg; do
    [ -f "$svg" ] || continue
    name=$(basename "$svg" .svg)
    # Larger effects get 256px, smaller get 128px
    if [[ "$name" == *"explosion"* ]] || [[ "$name" == *"death"* ]] || [[ "$name" == *"shield"* ]]; then
        convert_svg "$svg" "$EXPORT_DIR/Effects/${name}.png" 256
    else
        convert_svg "$svg" "$EXPORT_DIR/Effects/${name}.png" $FX_SIZE
    fi
done

# Export FX
echo "=== Exporting FX ==="
for svg in "$SOURCE_DIR"/FX/*/*.svg; do
    [ -f "$svg" ] || continue
    name=$(basename "$svg" .svg)
    convert_svg "$svg" "$EXPORT_DIR/FX/${name}.png" $FX_SIZE
done

# Export Weapons (64x64 icons)
echo "=== Exporting Weapons ==="
for svg in "$SOURCE_DIR"/Weapons/*.svg; do
    [ -f "$svg" ] || continue
    name=$(basename "$svg" .svg)
    convert_svg "$svg" "$EXPORT_DIR/Weapons/${name}.png" $ICON_SIZE
done

# Export UI elements
echo "=== Exporting UI ==="
for svg in "$SOURCE_DIR"/UI/*.svg "$SOURCE_DIR"/UI/*/*.svg; do
    [ -f "$svg" ] || continue
    name=$(basename "$svg" .svg)
    # UI elements vary in size, use larger canvas
    convert_svg "$svg" "$EXPORT_DIR/UI/${name}.png" $UI_SIZE
done

# Export Backgrounds
echo "=== Exporting Backgrounds ==="
for svg in "$SOURCE_DIR"/Backgrounds/*.svg; do
    [ -f "$svg" ] || continue
    name=$(basename "$svg" .svg)
    # Backgrounds are larger
    convert_svg "$svg" "$EXPORT_DIR/Backgrounds/${name}.png" 1024
done

# Export Items
echo "=== Exporting Items ==="
for svg in "$SOURCE_DIR"/Items/*.svg; do
    [ -f "$svg" ] || continue
    name=$(basename "$svg" .svg)
    convert_svg "$svg" "$EXPORT_DIR/Items/${name}.png" $ICON_SIZE
done

# Export Environment
echo "=== Exporting Environment ==="
for svg in "$SOURCE_DIR"/Environment/*.svg; do
    [ -f "$svg" ] || continue
    name=$(basename "$svg" .svg)
    convert_svg "$svg" "$EXPORT_DIR/Environment/${name}.png" 512
done

echo "=== Export Complete ==="
echo "Files exported to: $EXPORT_DIR"

# Generate statistics
echo ""
echo "=== Export Statistics ==="
find "$EXPORT_DIR" -name "*.png" | wc -l | xargs echo "Total PNG files:"
du -sh "$EXPORT_DIR" | cut -f1 | xargs echo "Total size:"
