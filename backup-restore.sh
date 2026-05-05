#!/bin/bash

BACKUP_FILE="$1"
TARGET_VOLUME="loyaltycrm_sqlvolume"
TEMP_VOLUME="${TARGET_VOLUME}_restored"

if [ -z "$BACKUP_FILE" ]; then
  echo "Usage: $0 <path_to_backup_file>"
  echo "Example: $0 /opt/backups/loyaltycrm_sqlvolume-20260505-120000.tar.gz"
  exit 1
fi

if [ ! -f "$BACKUP_FILE" ]; then
  echo "ERROR: Backup file not found: $BACKUP_FILE"
  exit 1
fi

echo "Preparing to restore from: $BACKUP_FILE"

# 1. Create a new empty volume
echo "Creating temporary volume '${TEMP_VOLUME}'..."
docker volume create "$TEMP_VOLUME"

# 2. Extract the backup into the new volume
echo "Restoring data..."
docker run --rm \
  -v "${TEMP_VOLUME}:/target" \
  -v "$(dirname "$BACKUP_FILE"):/src:ro" \
  alpine tar xzf "/src/$(basename "$BACKUP_FILE")" -C /target

if [ $? -ne 0 ]; then
  echo "ERROR: Restoration failed. Cleaning up."
  docker volume rm "$TEMP_VOLUME"
  exit 1
fi

echo "Restoration complete to temporary volume '${TEMP_VOLUME}'."
echo ""
echo "NEXT STEPS:"
echo "1. Stop your running container using '${TARGET_VOLUME}'."
echo "2. Remove the old volume (optional, but recommended if you want to replace it):"
echo "   docker volume rm ${TARGET_VOLUME}"
echo "3. Rename the restored volume to the original name:"
echo "   docker volume rm ${TARGET_VOLUME} 2>/dev/null; docker tag ${TEMP_VOLUME} ${TARGET_VOLUME} 2>/dev/null || mv ${TEMP_VOLUME} ${TARGET_VOLUME}"
echo "   NOTE: Docker does not have a 'mv' command for volumes. You must manually copy data or recreate the container."
echo ""
echo "SIMPLEST WAY TO FINISH RESTORE:"
echo "1. Stop your container: docker stop <your_container_name>"
echo "2. Remove the old volume: docker volume rm ${TARGET_VOLUME}"
echo "3. Rename the temp volume: docker volume create ${TARGET_VOLUME} && docker run --rm -v ${TEMP_VOLUME}:/_old -v ${TARGET_VOLUME}:/_new alpine cp -r /_old/. /_new/"
echo "4. Start your container again."