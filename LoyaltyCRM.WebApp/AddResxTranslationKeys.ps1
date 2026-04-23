param (
    [string]$lang = "da" # Language code, e.g. "da" or "en"
)

$resxFile = "LoyaltyCRM.WebApp/Locales/Resources.$lang.resx"

# Load existing .resx as XML
if (Test-Path $resxFile) {
    [xml]$resxXml = Get-Content $resxFile -Raw
} else {
    # Create a new .resx structure if file doesn't exist
    $resxXml = [xml]@"
<?xml version="1.0" encoding="utf-8"?>
<root>
  <resheader name="resmimetype"><value>text/microsoft-resx</value></resheader>
  <resheader name="version"><value>2.0</value></resheader>
  <resheader name="reader"><value>System.Resources.ResXResourceReader, System.Windows.Forms, ...</value></resheader>
  <resheader name="writer"><value>System.Resources.ResXResourceWriter, System.Windows.Forms, ...</value></resheader>
</root>
"@
}

if (-not $resxXml.root) {
    throw "Invalid .resx structure: missing root node"
}

# Collect all translation keys from .razor files
$searchPaths = @("Components", "Layout", "Pages")
$foundKeys = @{}

foreach ($path in $searchPaths) {
    Get-ChildItem -Path $path -Recurse -Include *.razor | ForEach-Object {
        # @L["Key"] pattern
        $matches1 = Select-String -Path $_.FullName -Pattern 'L\["([^"]+)"\]' -AllMatches
        foreach ($match in $matches1.Matches) {
            $foundKeys[$match.Groups[1].Value] = $true
        }
    }
}

# Get existing keys from .resx
$existingKeys = @{}

@($resxXml.root.data) | ForEach-Object {
    if ($_) {
        $nameAttr = $_.GetAttribute("name")
        if ($nameAttr) {
            $existingKeys[$nameAttr] = $true
        }
    }
}

# Get all existing data nodes sorted by name
$dataNodes = @($resxXml.root.data)

# Add missing keys (alphabetically)
$added = $false

foreach ($key in ($foundKeys.Keys | Sort-Object)) {
    if (-not $existingKeys.ContainsKey($key)) {

        $dataNode = $resxXml.CreateElement("data")
        $dataNode.SetAttribute("name", $key)
        $dataNode.SetAttribute("xml:space", "preserve")

        $valueNode = $resxXml.CreateElement("value")
        $valueNode.InnerText = $key
        $dataNode.AppendChild($valueNode) | Out-Null

        # Always fetch fresh nodes (avoids array issues entirely)
        $currentNodes = @($resxXml.root.data) | Sort-Object name

        $inserted = $false
        foreach ($node in $currentNodes) {
            $nodeName = $node.GetAttribute("name")
            if ($nodeName  -gt $key) {
                $resxXml.root.InsertBefore($dataNode, $node) | Out-Null
                $inserted = $true
                break
            }
        }

        if (-not $inserted) {
            $resxXml.root.AppendChild($dataNode) | Out-Null
        }

        Write-Host "Added key: $key"
        $added = $true
    }
}

# Save if changes were made
if ($added) {
    $resxXml.Save($resxFile)
    Write-Host "Updated $resxFile"
} else {
    Write-Host "No new keys to add."
}