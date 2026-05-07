#!/bin/bash

set -euo pipefail

# Configuration
CONTAINER_NAME="loyaltycrm-sqlserver-1"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
BACKUP_DIR="${SCRIPT_DIR}/backups"

# 1. Handle Input Argument
# If an argument is provided, use it. Otherwise, find the latest file.
if [ $# -gt 0 ]; then
    BACKUP_FILE="$1"
    # Validate the file exists
    if [ ! -f "$BACKUP_DIR/$BACKUP_FILE" ]; then
        echo "Error: Backup file '$BACKUP_FILE' not found in $BACKUP_DIR"
        exit 1
    fi
    echo "Restoring from specified file: $BACKUP_FILE"
else
    BACKUP_FILE=$(ls -t "$BACKUP_DIR"/loyaltycrm_backup_*.tar.gz 2>/dev/null | head -n 1)
    if [ -z "$BACKUP_FILE" ] || [ ! -f "$BACKUP_FILE" ]; then
        echo "Error: No backup files found in $BACKUP_DIR"
        exit 1
    fi
    echo "No file specified. Defaulting to latest: $(basename "$BACKUP_FILE")"
fi

# 2. CRITICAL CHECK: Ensure the container exists
if ! docker ps -a --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
    echo "CRITICAL ERROR: Container '$CONTAINER_NAME' does not exist."
    echo "This script is designed to restore an EXISTING container managed by Docker Compose."
    exit 1
fi

echo "Container '$CONTAINER_NAME' found."

# 3. Stop the container
echo "Stopping container '$CONTAINER_NAME'..."
docker stop "$CONTAINER_NAME"

# 4. Determine mount point
MOUNT_POINT=$(docker inspect "$CONTAINER_NAME" --format='{{range .Mounts}}{{if eq .Type "volume"}}{{.Destination}}{{end}}{{end}}')

if [ -z "$MOUNT_POINT" ]; then
    echo "Error: Could not determine volume mount point for '$CONTAINER_NAME'."
    exit 1
fi

echo "Target mount point: $MOUNT_POINT"

# 5. Extract the backup
echo "Extracting backup to $MOUNT_POINT..."
docker run --rm \
    --volumes-from "$CONTAINER_NAME" \
    -v "$BACKUP_DIR":/backup \
    alpine \
    sh -c "cd $MOUNT_POINT && tar xzf /backup/$(basename "$BACKUP_FILE")"

# 6. Fix Permissions
echo "Correcting file ownership to mssql (UID 10001)..."
docker run --rm \
    --volumes-from "$CONTAINER_NAME" \
    alpine \
    chown -R 10001:10001 "$MOUNT_POINT"

# 7. Start the container
echo "Starting container '$CONTAINER_NAME'..."
docker start "$CONTAINER_NAME"

# 8. Wait for SQL Server to be ready
echo "Waiting for SQL Server to initialize..."
sleep 15

# 9. Verify
if docker ps --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
    echo "SUCCESS: Restore complete from '$(basename "$BACKUP_FILE")'. Container '$CONTAINER_NAME' is running."
else
    echo "WARNING: Container started but may have exited. Check logs: docker logs $CONTAINER_NAME"
    exit 1
fi