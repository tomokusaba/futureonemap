#!/bin/bash

# Thumbnail generation script for FutureOne navigation images
# Creates optimized thumbnails to reduce loading time
# Includes privacy protection features to prevent individual identification

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
        
        # Generate thumbnail with ImageMagick and privacy protection
        # Using -auto-orient to fix EXIF rotation issues FIRST
        # -resize to maintain aspect ratio and crop to exact dimensions
        # -blur 0x1.5 to apply gentle blur for privacy protection (makes faces unrecognizable)
        # -quality 85 for good compression while maintaining quality
        # -strip to remove metadata and reduce file size
        convert "$image" \
            -auto-orient \
            -resize "${THUMB_WIDTH}x${THUMB_HEIGHT}^" \
            -gravity center \
            -crop "${THUMB_WIDTH}x${THUMB_HEIGHT}+0+0" \
            -blur 0x1.5 \
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

# Privacy protection for full-size images
echo ""
echo "🔒 Applying privacy protection to full-size images..."

# Create privacy-protected versions directory
PRIVACY_DIR="img/privacy_protected"
mkdir -p "$PRIVACY_DIR"

count=0
for image in "$SOURCE_DIR"/*.JPG; do
    if [ -f "$image" ]; then
        filename=$(basename "$image")
        privacy_path="$PRIVACY_DIR/$filename"
        
        count=$((count + 1))
        echo "[$count/$total] Privacy protecting: $filename"
        
        # Apply light blur to full-size images for privacy protection
        # Using lighter blur (0x1) to maintain more detail for navigation
        # while still protecting privacy
        convert "$image" \
            -auto-orient \
            -blur 0x1 \
            -quality 90 \
            -strip \
            "$privacy_path"
    fi
done

echo "✅ Privacy protection complete!"
echo "📁 Privacy-protected images saved in: $PRIVACY_DIR"

# Calculate total space savings
original_total=$(du -sb "$SOURCE_DIR"/*.JPG | awk '{sum += $1} END {print sum}')
thumb_total=$(du -sb "$THUMB_DIR"/*.JPG | awk '{sum += $1} END {print sum}')
total_savings=$(echo "scale=2; (1 - $thumb_total / $original_total) * 100" | bc)

echo "📊 Space savings summary:"
echo "  Original total: $(numfmt --to=iec-i --suffix=B $original_total)"
echo "  Thumbnail total: $(numfmt --to=iec-i --suffix=B $thumb_total)"
echo "  Total reduction: ${total_savings}% smaller"