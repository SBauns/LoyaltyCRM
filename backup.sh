#!/bin/bash

set -euo pipefail

# Configuration
CONTAINER_NAME="loyaltycrm-sqlserver-1"
BACKUP_DIR="$(pwd)/backups"
RETENTION_DAYS=90
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="loyaltycrm_backup_${TIMESTAMP}.tar.gz"

# Ensure backup directory exists
mkdir -p "$BACKUP_DIR"

# 1. Determine the mount point dynamically
MOUNT_POINT=$(docker inspect "$CONTAINER_NAME" --format='{{range .Mounts}}{{if eq .Type "volume"}}{{.Destination}}{{end}}{{end}}')

if [ -z "$MOUNT_POINT" ]; then
    echo "Error: Could not determine volume mount point for $CONTAINER_NAME"
    exit 1
fi

echo "Detected mount point: $MOUNT_POINT"

# 2. Perform the backup
# NOTE: For production consistency, consider stopping the container first:
# docker stop "$CONTAINER_NAME"
# ... run backup ...
# docker start "$CONTAINER_NAME"
echo "Creating backup..."
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

# 4. Cleanup Old Backups (Retention Policy)
echo "Cleaning up backups older than $RETENTION_DAYS days..."
# -type f: only files
# -mtime +90: modified time > 90 days ago
# -delete: remove them
find "$BACKUP_DIR" -type f -name "loyaltycrm_backup_*.tar.gz" -mtime +${RETENTION_DAYS} -delete

# Report what was deleted (optional, for logging)
DELETED_COUNT=$(find "$BACKUP_DIR" -type f -name "loyaltycrm_backup_*.tar.gz" -mtime +${RETENTION_DAYS} 2>/dev/null | wc -l)
echo "Cleanup complete. Removed $DELETED_COUNT old backup(s)."

echo "Backup process finished successfully."