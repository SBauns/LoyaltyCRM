Write-Host "=== Configure Development Secrets ==="

# --- PROJECT PATHS ---
$apiProjectPath = Resolve-Path "./LoyaltyCRM.Api/*.csproj" -ErrorAction SilentlyContinue
$infraProjectPath = Resolve-Path "./LoyaltyCRM.Infrastructure/*.csproj" -ErrorAction SilentlyContinue

if (-not $apiProjectPath) {
    Write-Host "❌ Could not find API project at ./LoyaltyCRM.Api" -ForegroundColor Red
    exit 1
}

if (-not $infraProjectPath) {
    Write-Host "❌ Could not find Infrastructure project at ./LoyaltyCRM.Infrastructure" -ForegroundColor Red
    exit 1
}

Write-Host "✔ API project detected: $apiProjectPath"
Write-Host "✔ Infrastructure project detected: $infraProjectPath"

# --- FUNCTION: Ensure UserSecretsId exists ---
function Ensure-UserSecrets {
    param(
        [string]$ProjectPath,
        [string]$Name
    )

    $content = Get-Content $ProjectPath

    if ($content -notmatch "UserSecretsId") {
        Write-Host "⚠️  $Name project missing UserSecretsId. Initializing..."
        dotnet user-secrets init --project (Split-Path $ProjectPath)
    } else {
        Write-Host "✔️  $Name project already has UserSecretsId."
    }
}

# Ensure both projects have user-secrets enabled
Ensure-UserSecrets -ProjectPath $apiProjectPath -Name "API"
Ensure-UserSecrets -ProjectPath $infraProjectPath -Name "Infrastructure"

# --- BASE64 VALIDATION ---
function Test-Base64 {
    param([string]$Value)

    try {
        [Convert]::FromBase64String($Value) | Out-Null
        return $true
    }
    catch {
        return $false
    }
}

# --- PROMPTS ---
do {
    $jwtKey = Read-Host "Enter JWT SecretKey (Base64 encoded)"
    if (-not (Test-Base64 $jwtKey)) {
        Write-Host "❌ Invalid Base64 string. Please try again." -ForegroundColor Red
    }
} until (Test-Base64 $jwtKey)

$adminUser = Read-Host "Enter Admin Username"
$adminPassword = Read-Host "Enter Admin Password"
$employeeUser = Read-Host "Enter Employee Username"
$employeePassword = Read-Host "Enter Employee Password"

$connectionString = Read-Host "Enter DefaultConnection connection string"
$designConnectionString = Read-Host "Enter DesignConnection connection string"

Write-Host "`n=== Writing secrets ==="

# --- WRITE API SECRETS ---
Write-Host "🔐 Writing API secrets..."
dotnet user-secrets set "JwtSettings:SecretKey" "$jwtKey" --project "./LoyaltyCRM.Api"
dotnet user-secrets set "Users:AdminUser" "$adminUser" --project "./LoyaltyCRM.Api"
dotnet user-secrets set "Users:AdminPassword" "$adminPassword" --project "./LoyaltyCRM.Api"
dotnet user-secrets set "Users:EmployeeUser" "$employeeUser" --project "./LoyaltyCRM.Api"
dotnet user-secrets set "Users:EmployeePassword" "$employeePassword" --project "./LoyaltyCRM.Api"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "$connectionString" --project "./LoyaltyCRM.Infrastructure"

# --- WRITE INFRASTRUCTURE SECRET ---
Write-Host "🗄 Writing Infrastructure DB connection string..."
dotnet user-secrets set "ConnectionStrings:DesignConnection" "$designConnectionString" --project "./LoyaltyCRM.Infrastructure"

Write-Host "`n✔️ All secrets have been stored in the correct projects."