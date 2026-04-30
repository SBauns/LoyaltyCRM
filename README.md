# LoyaltyCRM

# 1. Clone the repository on the server
cd /opt
git clone <your-repo-url> papascrm
cd papascrm

# 2. Prepare the environment file
cp .env.example .env
nano .env  # Edit this file to insert the REAL SA_PASSWORD and other secrets
chmod 600 .env  # Restrict access to root/user only

# 3. Verify the container name (do this AFTER starting the containers)
docker compose up -d
docker ps --format '{{.Names}}'
# Note the name of the SQL container (e.g., papascrm_sqlserver_1)

# 4. Update the backup script
# Open /opt/papascrm/scripts/backup.sh
# Update DB_CONTAINER variable to match the name found in step 3
# Update DATABASE_NAME if it differs from the default

# 5. Make the script executable
chmod +x scripts/backup.sh

# 6. Set up the cron job
crontab -e
# Add: 0 3 * * 0 /opt/papascrm/scripts/backup.sh >> /var/log/papascrm_backup.log 2>&1