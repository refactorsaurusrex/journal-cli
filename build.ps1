param(
  [string]$Version = ''
)

$ErrorActionPreference = 'Stop'

if ($env:APPVEYOR_REPO_TAG -eq 'true') {
  $Version = [regex]::match($env:APPVEYOR_REPO_TAG_NAME,'[0-9]+\.[0-9]+\.[0-9]+').Groups[0].Value
  $lastPublishedVersion = [Version]::new((Find-Module -Name JournalCli | Select-Object -ExpandProperty Version))
  if ([Version]::new($Version) -le $lastPublishedVersion) {
    throw "Version must be greater than the last published version, which is 'v$lastPublishedVersion'."
  }
  Write-Host "Last published version: 'v$lastPublishedVersion'. Current version: 'v$Version'"
} elseif ($Version -eq '') {
  $Version = '0.0.0'
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

Remove-Item "$publishOutputDir\*.pdb"

Import-Module "$publishOutputDir\$appName.dll"
$moduleInfo = Get-Module $appName
Install-Module WhatsNew
Import-Module WhatsNew
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

Install-Module platyPS
Import-Module platyPS
$docs = "$PSScriptRoot\docs"
try {
  git clone https://github.com/refactorsaurusrex/journal-cli.wiki.git $docs
  if ($LASTEXITCODE -ne 0) {
    throw "Failed to clone wiki."
  }

  Switch-CodeFenceToYamlFrontMatter -Path $docs -NoConfirm
  New-ExternalHelp -Path $docs -OutputPath $publishOutputDir
} finally {
  if (Test-Path $docs) {
    Remove-Item -Path $docs -Recurse -Force
  }
}