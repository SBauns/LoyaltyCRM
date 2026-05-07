#!/bin/bash

set -euo pipefail

# 1. Determine script location dynamically
# This ensures the script works regardless of the user or clone path
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# 2. Configuration (Defaults)
CONTAINER_NAME="${LOYALTY_CONTAINER_NAME:-loyaltycrm-sqlserver-1}"
BACKUP_DIR="${LOYALTY_BACKUP_DIR:-${SCRIPT_DIR}/backups}"
RETENTION_DAYS=90
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="loyaltycrm_backup_${TIMESTAMP}.tar.gz"

# 3. Ensure directories exist
mkdir -p "$BACKUP_DIR"

echo "Starting backup for container: $CONTAINER_NAME"
echo "Backup destination: $BACKUP_DIR"

# 4. Determine mount point
MOUNT_POINT=$(docker inspect "$CONTAINER_NAME" --format='{{range .Mounts}}{{if eq .Type "volume"}}{{.Destination}}{{end}}{{end}}')

if [ -z "$MOUNT_POINT" ]; then
    echo "Error: Could not determine volume mount point for $CONTAINER_NAME"
    exit 1
fi

# 5. Perform Backup
docker run --rm \
    --volumes-from "$CONTAINER_NAME" \
    -v "$BACKUP_DIR":/backup \
    alpine \
    tar czf "/backup/$BACKUP_FILE" -C "$MOUNT_POINT" .

# 6. Verify
if docker run --rm -v "$BACKUP_DIR":/backup alpine tar tf "/backup/$BACKUP_FILE" > /dev/null 2>&1; then
    echo "Backup created and verified: $BACKUP_DIR/$BACKUP_FILE"
else
    echo "Error: Backup verification failed."
    rm -f "$BACKUP_DIR/$BACKUP_FILE"
    exit 1
fi

# 7. Cleanup
find "$BACKUP_DIR" -type f -name "loyaltycrm_backup_*.tar.gz" -mtime +${RETENTION_DAYS} -delete
echo "Cleanup complete."