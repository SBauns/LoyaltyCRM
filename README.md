# LoyaltyCRM

# 1. Clone the repository on the server
git clone <your-repo-url>

# 2. Prepare the environment file
cp .env.example .env
nano .env  # Edit this file to insert the REAL SA_PASSWORD and other secrets
chmod 600 .env  # Restrict access to root/user only

# 3. Make the script executable
chmod +x scripts/backup.sh
chmod +x scripts/backup-restore.sh

# 4. Set up the cron job and rotate
crontab -e
sudo nano /etc/logrotate.d/loyaltycrm_backup

Create `/etc/logrotate.d/loyaltycrm_backup`:
```
/var/log/loyaltycrm_backup.log {
    weekly
    rotate 4
    compress
    missingok
    notifempty
}
```
# 0 5 * * * /home/papa/services/LoyaltyCRM/backup.sh >> /home/papa/services/LoyaltyCRM/logs/loyaltycrm_backup.log 2>&1