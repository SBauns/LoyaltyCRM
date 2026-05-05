# -------------------------------------------------------
# Paths
# -------------------------------------------------------
$projectPath = (Get-Location).Path
$resourcesPath = Join-Path $projectPath "LoyaltyCRM.Api/Localization"

Write-Host "Resources path: $resourcesPath"
Write-Host "Exists: $(Test-Path $resourcesPath)"

Write-Host "Scanning project at: $projectPath"

# -------------------------------------------------------
# 1. Find all translation keys in code
# -------------------------------------------------------

$csFiles = Get-ChildItem -Path $projectPath -Recurse -Filter "*.cs"

$translateKeys = $csFiles | Select-String -Pattern '"(translation\.[a-zA-Z0-9_.-]+)"' -AllMatches |
    ForEach-Object {
        $_.Matches | ForEach-Object { $_.Groups[1].Value }
    } | Sort-Object -Unique

Write-Host "`nFound $($translateKeys.Count) translation keys in code"

# Convert to HashSet for fast lookup
$translateSet = @{}
foreach ($k in $translateKeys) {
    $translateSet[$k] = $true
}

# -------------------------------------------------------
# 2. Process translation JSON files
# -------------------------------------------------------

$jsonFiles = Get-ChildItem -Path $resourcesPath -Filter "Translations.*.json"

foreach ($jsonFile in $jsonFiles) {

    Write-Host "`n========================================"
    Write-Host "Processing $($jsonFile.Name)"
    Write-Host "========================================"

    $jsonContent = Get-Content $jsonFile.FullName -Raw | ConvertFrom-Json

    # Convert JSON to mutable dictionary
    $hash = @{}
    foreach ($prop in $jsonContent.PSObject.Properties) {
        $hash[$prop.Name] = $prop.Value
    }

    # ---------------------------------------------------
    # 3. Add missing keys
    # ---------------------------------------------------

    $updated = $false

    foreach ($key in $translateKeys) {
        if (-not $hash.ContainsKey($key)) {
            $hash[$key] = $key   # fallback = key itself
            $updated = $true
            Write-Host "  [+] Added missing key: $key"
        }
    }

    # ---------------------------------------------------
    # 4. Detect UNUSED translations
    # ---------------------------------------------------

    Write-Host "`nChecking for unused translations..."

    $unusedKeys = @()

    foreach ($existingKey in $hash.Keys) {
        if (-not $translateSet.ContainsKey($existingKey)) {
            $unusedKeys += $existingKey
        }
    }

    if ($unusedKeys.Count -gt 0) {
        Write-Warning "Unused translations found in $($jsonFile.Name):"

        foreach ($u in $unusedKeys) {
            Write-Warning "  [-] $u"
        }
    }
    else {
        Write-Host "  No unused translations found."
    }

    # ---------------------------------------------------
    # 5. Save file if changed
    # ---------------------------------------------------

    if ($updated) {

        # Sort keys alphabetically for readability
        $ordered = [ordered]@{}
        $hash.Keys | Sort-Object | ForEach-Object {
            $ordered[$_] = $hash[$_]
        }

        $ordered |
            ConvertTo-Json -Depth 10 |
            Set-Content $jsonFile.FullName -Encoding UTF8

        Write-Host "`n  ✔ Updated $($jsonFile.Name)"
    }
    else {
        Write-Host "`n  No changes written."
    }
}

Write-Host "`nDONE."