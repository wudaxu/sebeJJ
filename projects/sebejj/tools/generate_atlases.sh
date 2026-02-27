#!/bin/bash
# Generate Sprite Atlas for SebeJJ

set -e

PROJECT_ROOT="/root/.openclaw/workspace/projects/sebejj"
EXPORT_DIR="$PROJECT_ROOT/Assets/Art/Exported"
ATLAS_DIR="$EXPORT_DIR/Atlases"

mkdir -p "$ATLAS_DIR"

echo "=== Generating Sprite Atlases ==="

# Function to create atlas from directory
create_atlas() {
    local src_dir="$1"
    local atlas_name="$2"
    local max_size="$3"
    
    if [ ! -d "$src_dir" ] || [ -z "$(ls -A "$src_dir" 2>/dev/null)" ]; then
        echo "Skipping $atlas_name (no files)"
        return
    fi
    
    echo "Creating atlas: $atlas_name"
    
    # Use ImageMagick montage to create sprite atlas
    montage "$src_dir"/*.png \
        -mode concatenate \
        -tile x1 \
        -background none \
        -format PNG32 \
        "$ATLAS_DIR/${atlas_name}_Horizontal.png" 2>/dev/null || \
    convert "$src_dir"/*.png -append -background none "$ATLAS_DIR/${atlas_name}_Vertical.png" 2>/dev/null || \
    echo "  Warning: Could not create atlas for $atlas_name"
}

# Create atlases for animation frames
create_atlas "$EXPORT_DIR/Characters" "Characters_Atlas" 2048
create_atlas "$EXPORT_DIR/Animations" "Animations_Atlas" 2048
create_atlas "$EXPORT_DIR/Effects" "Effects_Atlas" 1024
create_atlas "$EXPORT_DIR/FX" "FX_Atlas" 1024
create_atlas "$EXPORT_DIR/Weapons" "Weapons_Atlas" 256
create_atlas "$EXPORT_DIR/Items" "Items_Atlas" 256

echo ""
echo "=== Atlas Generation Complete ==="
ls -lh "$ATLAS_DIR"/*.png 2>/dev/null | awk '{print $9, $5}' || echo "No atlases generated"
