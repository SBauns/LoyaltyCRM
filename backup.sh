#!/bin/bash
set -euo pipefail

# Disable Git Bash path conversion for Docker commands
export MSYS_NO_PATHCONV=1

# Resolve script directory for relative paths
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
ENV_FILE="$SCRIPT_DIR/.env"

# Configuration
DB_CONTAINER="loyaltycrm-sqlserver-1"
DATABASE_NAME="YearcardDb"
BACKUP_DIR="./backups"
RETENTION_DAYS=30

# --- Validation ---

if [ ! -f "$ENV_FILE" ]; then
    echo "ERROR: Environment file not found at $ENV_FILE"
    echo "Create it from .env.example and populate with real values."
    exit 1
fi

source "$ENV_FILE"

if [ -z "${SA_PASSWORD:-}" ]; then
    echo "ERROR: SA_PASSWORD is empty or not set in $ENV_FILE"
    exit 1
fi

if ! docker ps --format '{{.Names}}' | grep -q "^${DB_CONTAINER}$"; then
    echo "ERROR: Container '$DB_CONTAINER' is not running."
    echo "Running containers:"
    docker ps --format '{{.Names}}'
    exit 1
fi

# --- Backup ---

mkdir -p "$BACKUP_DIR"

TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
BACKUP_FILE="$BACKUP_DIR/papascrm_backup_$TIMESTAMP.bak"
CONTAINER_BACKUP_PATH="/var/opt/mssql/data/backup_$TIMESTAMP.bak"

echo "Starting backup at $(date)"
echo "Database: $DATABASE_NAME"
echo "Container: $DB_CONTAINER"
echo "Output: $BACKUP_FILE"

# Execute backup inside the container
docker exec "$DB_CONTAINER" /opt/mssql-tools/bin/sqlcmd \
    -S localhost \
    -U sa \
    -P "${SA_PASSWORD}" \
    -Q "BACKUP DATABASE [${DATABASE_NAME}] TO DISK = '${CONTAINER_BACKUP_PATH}' WITH FORMAT, INIT, NAME = 'Full Backup'"

if [ $? -ne 0 ]; then
    echo "ERROR: Database backup command failed."
    exit 1
fi

# Copy backup out of the container
docker cp "$DB_CONTAINER:${CONTAINER_BACKUP_PATH}" "$BACKUP_FILE"

if [ ! -f "$BACKUP_FILE" ]; then
    echo "ERROR: Backup file was not created at $BACKUP_FILE"
    exit 1
fi

# Remove backup file from inside the container
docker exec "$DB_CONTAINER" rm "${CONTAINER_BACKUP_PATH}"

# Report success
FILE_SIZE=$(du -h "$BACKUP_FILE" | cut -f1)
echo "Backup completed successfully: $BACKUP_FILE ($FILE_SIZE)"

# Rotate old backups
find "$BACKUP_DIR" -name "*.bak" -mtime +"$RETENTION_DAYS" -delete

echo "Old backups rotated (retention: $RETENTION_DAYS days)"