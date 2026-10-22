#!/usr/bin/env bash
set -euo pipefail

# --- PROJECT PATHS ---
readonly API_PROJECT_DIRECTORY="./LoyaltyCRM.Api"
readonly INFRASTRUCTURE_PROJECT_DIRECTORY="./LoyaltyCRM.Infrastructure"

FindProjectFile() {
    local projectDirectory="$1"

    find "$projectDirectory" -maxdepth 1 -name "*.csproj" | head -n 1
}

apiProjectFile="$(FindProjectFile "$API_PROJECT_DIRECTORY")"
infrastructureProjectFile="$(FindProjectFile "$INFRASTRUCTURE_PROJECT_DIRECTORY")"

if [[ -z "$apiProjectFile" ]]; then
    echo "ERROR: Could not find API project at $API_PROJECT_DIRECTORY" >&2
    exit 1
fi

if [[ -z "$infrastructureProjectFile" ]]; then
    echo "ERROR: Could not find Infrastructure project at $INFRASTRUCTURE_PROJECT_DIRECTORY" >&2
    exit 1
fi

echo "OK: API project detected: $apiProjectFile"
echo "OK: Infrastructure project detected: $infrastructureProjectFile"

# --- FUNCTION: Ensure UserSecretsId exists ---
EnsureUserSecrets() {
    local projectFile="$1"
    local projectName="$2"
    local projectDirectory

    projectDirectory="$(dirname "$projectFile")"

    if ! grep --quiet "UserSecretsId" "$projectFile"; then
        echo "WARN: $projectName project missing UserSecretsId. Initializing..."
        dotnet user-secrets init --project "$projectDirectory"
    else
        echo "OK: $projectName project already has UserSecretsId."
    fi
}

EnsureUserSecrets "$apiProjectFile" "API"
EnsureUserSecrets "$infrastructureProjectFile" "Infrastructure"

# --- BASE64 VALIDATION ---
TestBase64() {
    local value="$1"

    [[ "$value" =~ ^[A-Za-z0-9+/]+={0,2}$ ]] && \
        echo "$value" | base64 --decode > /dev/null 2>&1
}

# --- PROMPT WITH DEFAULT ---
PromptWithDefault() {
    local promptText="$1"
    local defaultValue="$2"
    local userInput

    read -rp "$promptText [$defaultValue]: " userInput

    echo "${userInput:-$defaultValue}"
}

# --- PROMPTS ---
echo ""
echo "Press Enter to accept the shown default value."

defaultJwtKey="$(openssl rand -base64 64 | tr --delete '\n')"

jwtKeyIsValid="false"
while [[ "$jwtKeyIsValid" == "false" ]]; do
    jwtKey="$(PromptWithDefault "Enter JWT SecretKey (Base64 encoded)" "$defaultJwtKey")"

    if TestBase64 "$jwtKey"; then
        jwtKeyIsValid="true"
    else
        echo "ERROR: Invalid Base64 string. Please try again." >&2
    fi
done

adminUser="$(PromptWithDefault "Enter Admin Username" "admin")"
adminPassword="$(PromptWithDefault "Enter Admin Password" "DevAdmin123!")"
employeeUser="$(PromptWithDefault "Enter Employee Username" "employee")"
employeePassword="$(PromptWithDefault "Enter Employee Password" "DevEmployee123!")"
audience="$(PromptWithDefault "Enter Audience" "https://localhost:5001")"
issuer="$(PromptWithDefault "Enter Issuer" "https://localhost:5001")"

connectionString="$(PromptWithDefault "Enter DefaultConnection connection string" \
    "Server=localhost,1433;Database=LoyaltyCRM;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;Encrypt=True")"

designConnectionString="$(PromptWithDefault "Enter DesignConnection connection string" \
    "Server=localhost,1433;Database=LoyaltyCRM_Design;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;Encrypt=True")"

echo ""
echo "=== Writing secrets ==="

SetSecret() {
    local secretKey="$1"
    local secretValue="$2"
    local projectDirectory="$3"

    dotnet user-secrets set "$secretKey" "$secretValue" --project "$projectDirectory"
}

# --- WRITE API SECRETS ---
echo "Writing API secrets..."
SetSecret "JwtSettings:SecretKey" "$jwtKey" "$API_PROJECT_DIRECTORY"
SetSecret "JwtSettings:Audience" "$audience" "$API_PROJECT_DIRECTORY"
SetSecret "JwtSettings:Issuer" "$issuer" "$API_PROJECT_DIRECTORY"
SetSecret "Users:AdminUser" "$adminUser" "$API_PROJECT_DIRECTORY"
SetSecret "Users:AdminPassword" "$adminPassword" "$API_PROJECT_DIRECTORY"
SetSecret "Users:EmployeeUser" "$employeeUser" "$API_PROJECT_DIRECTORY"
SetSecret "Users:EmployeePassword" "$employeePassword" "$API_PROJECT_DIRECTORY"
SetSecret "ConnectionStrings:DefaultConnection" "$connectionString" "$API_PROJECT_DIRECTORY"

# --- WRITE INFRASTRUCTURE SECRET ---
echo "Writing Infrastructure DB connection string..."
SetSecret "ConnectionStrings:DesignConnection" "$designConnectionString" "$INFRASTRUCTURE_PROJECT_DIRECTORY"

echo ""
echo "All secrets have been stored in the correct projects."