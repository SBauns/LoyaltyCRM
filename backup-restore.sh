#!/bin/bash

set -euo pipefail

# Configuration
# Ideally, this matches the service name in your docker-compose.yml
CONTAINER_NAME="loyaltycrm-sqlserver-1" 
BACKUP_DIR="$(pwd)/backups"

# Select the latest backup file
BACKUP_FILE=$(ls -t "$BACKUP_DIR"/loyaltycrm_backup_*.tar.gz 2>/dev/null | head -n 1)

if [ -z "$BACKUP_FILE" ] || [ ! -f "$BACKUP_FILE" ]; then
    echo "Error: No backup file found in $BACKUP_DIR"
    exit 1
fi

echo "Selected backup: $BACKUP_FILE"

# 1. CRITICAL CHECK: Ensure the container exists
# We check if the container ID exists, regardless of its state (running/stopped)
if ! docker ps -a --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
    echo "CRITICAL ERROR: Container '$CONTAINER_NAME' does not exist."
    echo "This script is designed to restore an EXISTING container managed by Docker Compose."
    echo "Do not run this script to create a new container. Check your docker-compose.yml."
    exit 1
fi

echo "Container '$CONTAINER_NAME' found."

# 2. Stop the container to release file locks
# We use 'docker stop' which respects the timeout defined in compose or defaults to 10s
echo "Stopping container '$CONTAINER_NAME'..."
docker stop "$CONTAINER_NAME"

# 3. Determine the mount point
# We inspect the existing container to find where the volume is mounted
MOUNT_POINT=$(docker inspect "$CONTAINER_NAME" --format='{{range .Mounts}}{{if eq .Type "volume"}}{{.Destination}}{{end}}{{end}}')

if [ -z "$MOUNT_POINT" ]; then
    echo "Error: Could not determine volume mount point for '$CONTAINER_NAME'."
    echo "Ensure the container has a named volume mounted."
    exit 1
fi

echo "Target mount point: $MOUNT_POINT"

# 4. Extract the backup
# We use a temporary container to extract files into the volume of the stopped container
echo "Extracting backup to $MOUNT_POINT..."
docker run --rm \
    --volumes-from "$CONTAINER_NAME" \
    -v "$BACKUP_DIR":/backup \
    alpine \
    sh -c "cd $MOUNT_POINT && tar xzf /backup/$(basename "$BACKUP_FILE")"

# 5. Fix Permissions (CRITICAL for SQL Server)
# SQL Server runs as user 'mssql' (UID 10001). 
# Extracting as root (default in docker run) leaves files owned by root, causing SQL to crash.
echo "Correcting file ownership to mssql (UID 10001)..."
docker run --rm \
    --volumes-from "$CONTAINER_NAME" \
    alpine \
    chown -R 10001:10001 "$MOUNT_POINT"

# 6. Start the container
echo "Starting container '$CONTAINER_NAME'..."
docker start "$CONTAINER_NAME"

# 7. Wait for SQL Server to be ready
echo "Waiting for SQL Server to initialize..."
maxattempts=10
attempts=0
while [ $attempts -lt $maxattempts ]; do
    if docker logs "$CONTAINER_NAME" 2>&1 | grep -q "SQL Server is now ready for client connections"; then
        echo "SQL Server is ready!"
        break
    fi
    attempts=$((attempts + 1))
    echo "Waiting... (attempt $attempts/$maxattempts)"
    sleep 5
done

# Check if we timed out
if [ $attempts -eq $maxattempts ]; then
    echo "WARNING: Timeout waiting for SQL Server. Checking container status..."
    if ! docker ps --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
        echo "ERROR: Container is not running. Check logs:"
        docker logs "$CONTAINER_NAME" --tail 50
        exit 1
    fi
fi

# Optional: Verify the container is actually running
if docker ps --format '{{.Names}}' | grep -q "^${CONTAINER_NAME}$"; then
    echo "SUCCESS: Restore complete. Container '$CONTAINER_NAME' is running."
else
    echo "WARNING: Container started but may have exited. Check logs with: docker logs $CONTAINER_NAME"
    exit 1
fi