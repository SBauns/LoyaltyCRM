#!/bin/sh
set -e

# Replace placeholder in appsettings.json with environment variable
API_BASE_URL=${API_BASE_URL:-https://localhost:7272}
CONFIG_FILE="/usr/share/nginx/html/appsettings.json"
if [ -f "$CONFIG_FILE" ]; then
    sed -i "s|__API_BASE_URL__|$API_BASE_URL|g" "$CONFIG_FILE"
fi

exec "$@"