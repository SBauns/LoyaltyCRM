# Set the path to your project and resources folder dynamically
$projectPath = Split-Path -Parent $MyInvocation.MyCommand.Definition
$resourcesPath = Join-Path $projectPath "Resources"

# Find all Translate("...") usages in the project
$translateStrings = Select-String -Path "$projectPath\**\*.cs" -Pattern 'Translate\("([^"]+)"' | ForEach-Object {
    $_.Matches | ForEach-Object { $_.Groups[1].Value }
} | Sort-Object -Unique

# Get all translation JSON files
$jsonFiles = Get-ChildItem -Path $resourcesPath -Filter *.json

foreach ($jsonFile in $jsonFiles) {
    Write-Host "Processing $($jsonFile.Name)..."
    $json = Get-Content $jsonFile.FullName -Raw | ConvertFrom-Json
    # Convert to hashtable for easier manipulation
    $hash = @{}
    foreach ($prop in $json.PSObject.Properties) {
        $hash[$prop.Name] = $prop.Value
    }

    $updated = $false
    foreach ($str in $translateStrings) {
        if (-not $hash.ContainsKey($str)) {
            $hash[$str] = $str
            $updated = $true
            Write-Host "  Added missing key: $str"
        }
    }

    if ($updated) {
        # Save the updated JSON, preserving formatting
        $hash | ConvertTo-Json -Depth 10 | Set-Content $jsonFile.FullName -Encoding UTF8
        Write-Host "  Updated $($jsonFile.Name)"
    } else {
        Write-Host "  No changes needed."
    }
}
Write-Host "Done."