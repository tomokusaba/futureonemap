#!/bin/bash

# Thumbnail generation script for FutureOne navigation images
# Creates optimized thumbnails to reduce loading time

echo "🖼️  Generating thumbnails for FutureOne navigation images..."

# Set thumbnail dimensions (matching the display size in CSS)
THUMB_WIDTH=200
THUMB_HEIGHT=150

# Source and destination directories
SOURCE_DIR="img"
THUMB_DIR="img/thumbnails"

# Ensure thumbnail directory exists
mkdir -p "$THUMB_DIR"

# Counter for progress
count=0
total=$(ls -1 "$SOURCE_DIR"/*.JPG 2>/dev/null | wc -l)

# Generate thumbnails for all JPG files
for image in "$SOURCE_DIR"/*.JPG; do
    if [ -f "$image" ]; then
        filename=$(basename "$image")
        thumb_path="$THUMB_DIR/$filename"
        
        count=$((count + 1))
        echo "[$count/$total] Processing: $filename"
        
        # Generate thumbnail with ImageMagick
        # Using -resize to maintain aspect ratio and crop to exact dimensions
        # -quality 85 for good compression while maintaining quality
        # -strip to remove metadata and reduce file size
        convert "$image" \
            -resize "${THUMB_WIDTH}x${THUMB_HEIGHT}^" \
            -gravity center \
            -crop "${THUMB_WIDTH}x${THUMB_HEIGHT}+0+0" \
            -quality 85 \
            -strip \
            "$thumb_path"
        
        # Get file sizes for comparison
        original_size=$(stat -c%s "$image")
        thumb_size=$(stat -c%s "$thumb_path")
        compression_ratio=$(echo "scale=2; $thumb_size * 100 / $original_size" | bc)
        
        echo "  Original: $(numfmt --to=iec-i --suffix=B $original_size)"
        echo "  Thumbnail: $(numfmt --to=iec-i --suffix=B $thumb_size)"
        echo "  Reduction: ${compression_ratio}% of original size"
        echo ""
    fi
done

echo "✅ Thumbnail generation complete!"
echo "📁 Thumbnails saved in: $THUMB_DIR"

# Calculate total space savings
original_total=$(du -sb "$SOURCE_DIR"/*.JPG | awk '{sum += $1} END {print sum}')
thumb_total=$(du -sb "$THUMB_DIR"/*.JPG | awk '{sum += $1} END {print sum}')
total_savings=$(echo "scale=2; (1 - $thumb_total / $original_total) * 100" | bc)

echo "📊 Space savings summary:"
echo "  Original total: $(numfmt --to=iec-i --suffix=B $original_total)"
echo "  Thumbnail total: $(numfmt --to=iec-i --suffix=B $thumb_total)"
echo "  Total reduction: ${total_savings}% smaller"