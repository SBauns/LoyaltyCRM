#!/bin/bash

VOLUME_NAME="loyaltycrm_sqlvolume"
BACKUP_DIR="/opt/backups"
TIMESTAMP=$(date +%Y%m%d-%H%M%S)
BACKUP_FILE="${BACKUP_DIR}/${VOLUME_NAME}-${TIMESTAMP}.tar.gz"

# Ensure backup directory exists
mkdir -p "$BACKUP_DIR"

echo "Starting backup of volume '${VOLUME_NAME}'..."

# Create the snapshot
docker run --rm \
  -v "${VOLUME_NAME}:/source:ro" \
  -v "${BACKUP_DIR}:/backup" \
  alpine tar czf "/backup/${VOLUME_NAME}-${TIMESTAMP}.tar.gz" -C /source .

if [ $? -eq 0 ]; then
  echo "SUCCESS: Backup created at ${BACKUP_FILE}"
  ls -lh "$BACKUP_FILE"
else
  echo "ERROR: Backup failed"
  exit 1
fi