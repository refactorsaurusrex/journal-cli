param(
  [string]$Version = '0.0.0'
)

$ErrorActionPreference = 'Stop'
Install-Module WhatsNew
Import-Module WhatsNew

if ($env:APPVEYOR_BUILD_VERSION) {
  $Version = [regex]::match($env:APPVEYOR_BUILD_VERSION,'[0-9]+\.[0-9]+\.[0-9]+').Groups[0].Value
}

if (Test-Path "$PSScriptRoot\publish") {
  Remove-Item -Path "$PSScriptRoot\publish" -Recurse -Force
}

$appName = "JournalCli"
$publishOutputDir = "$PSScriptRoot\publish\$appName"
$proj = Get-ChildItem -Filter "$appName.csproj" -Recurse -Path $PSScriptRoot | Select-Object -First 1 -ExpandProperty FullName
dotnet publish $proj --output $publishOutputDir -c Release

if ($LASTEXITCODE -ne 0) {
  throw "Failed to publish application."
}

Get-ChildItem -Filter "$appName.dll-Help.xml" -Recurse -File -Path "$PSScriptRoot\src" |
  Where-Object { $_.FullName -like "*bin\Release*" } | 
  Select-Object -First 1 | 
  Copy-Item -Destination $publishOutputDir -Force

Remove-Item "$publishOutputDir\*.pdb"

Import-Module "$publishOutputDir\$appName.dll"
$moduleInfo = Get-Module $appName
$cmdletNames = Export-BinaryCmdletNames -ModuleInfo $moduleInfo
$cmdletAliases = Export-BinaryCmdletAliases -ModuleInfo $moduleInfo

$manifestPath = "$publishOutputDir\$appName.psd1"

$newManifestArgs = @{
  Path = $manifestPath
}

$updateManifestArgs = @{
  Path = $manifestPath
  CopyRight = "(c) $((Get-Date).Year) Nick Spreitzer"
  Description = "Index your markdown-based journal with yaml front matter!"
  Guid = 'c45932e7-867b-4894-9226-cf278bdb4e3e'
  Author = 'Nick Spreitzer'
  CompanyName = 'RAWR! Productions'
  ModuleVersion = $Version
  AliasesToExport = $cmdletAliases
  NestedModules = ".\$appName.dll"
  CmdletsToExport = $cmdletNames
  CompatiblePSEditions = @("Desktop","Core")
  HelpInfoUri = "https://github.com/refactorsaurusrex/journal-cli/wiki"
  PowerShellVersion = "6.0"
  PrivateData = @{
    Tags = 'notebook','journal','markdown'
    LicenseUri = 'https://github.com/refactorsaurusrex/journal-cli/blob/master/LICENSE'
    ProjectUri = 'https://github.com/refactorsaurusrex/journal-cli'
  }
}

New-ModuleManifest @newManifestArgs
Update-ModuleManifest @updateManifestArgs
Remove-ModuleManifestComments $manifestPath -NoConfirm