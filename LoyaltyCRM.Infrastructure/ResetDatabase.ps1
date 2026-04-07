# PowerShell script to reset the EF Core database and migrations

# Step 1: Drop the database forcefully
Write-Host "Dropping the database..."
dotnet ef database drop --force

# Step 2: Delete migrations in the Migrations folder
$migrationsFolder = "Migrations"
if (Test-Path $migrationsFolder) {
    Write-Host "Deleting existing migrations..."
    Remove-Item "$migrationsFolder\*" -Recurse -Force
} else {
    Write-Host "No Migrations folder found. Skipping deletion."
}

# Step 3: Add a new migration
Write-Host "Adding initial migration..."
dotnet ef migrations add InitialCreate

# Step 4: Update the database
Write-Host "Updating the database..."
dotnet ef database update

Write-Host "Database reset and migration completed."
