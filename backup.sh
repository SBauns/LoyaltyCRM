#!/bin/bash

set -euo pipefail

CONTAINER_NAME="loyaltycrm-sqlserver-1"
BACKUP_DIR="$(pwd)"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="loyaltycrm_backup_${TIMESTAMP}.tar.gz"

# 1. Determine the mount point dynamically
MOUNT_POINT=$(docker inspect "$CONTAINER_NAME" --format='{{range .Mounts}}{{if eq .Type "volume"}}{{.Destination}}{{end}}{{end}}')

if [ -z "$MOUNT_POINT" ]; then
    echo "Error: Could not determine volume mount point for $CONTAINER_NAME"
    exit 1
fi

echo "Detected mount point: $MOUNT_POINT"

# 2. Perform the backup using alpine for minimal footprint
# We use 'tar czf' for gzip compression
docker run --rm \
    --volumes-from "$CONTAINER_NAME" \
    -v "$BACKUP_DIR":/backup \
    alpine \
    tar czf "/backup/$BACKUP_FILE" -C "$MOUNT_POINT" .

# 3. Verify the archive
if docker run --rm -v "$BACKUP_DIR":/backup alpine tar tf "/backup/$BACKUP_FILE" > /dev/null 2>&1; then
    echo "Backup created and verified: $BACKUP_DIR/$BACKUP_FILE"
else
    echo "Error: Backup verification failed. Removing corrupted file."
    rm -f "$BACKUP_DIR/$BACKUP_FILE"
    exit 1
fi