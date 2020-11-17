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
} elseif ($null -ne $env:APPVEYOR_BUILD_NUMBER) {
  $Version = "0.0.$env:APPVEYOR_BUILD_NUMBER"
} elseif ($Version -eq '') {
  $Version = "0.0.0"
}

Write-Host "Building version '$Version'..."
$appName = "JournalCli"

if (Test-Path "$PSScriptRoot\publish") {
  Remove-Item -Path "$PSScriptRoot\publish" -Recurse -Force
}

New-Item -Path "$PSScriptRoot\publish\$appName" -ItemType Directory | Out-Null
$publishOutputDir = "$PSScriptRoot\publish\$appName"
$proj = Get-ChildItem -Filter "$appName.csproj" -Recurse -Path $PSScriptRoot | Select-Object -First 1 -ExpandProperty FullName

dotnet publish $proj -c Release --self-contained true --output "$publishOutputDir"
if ($LASTEXITCODE -ne 0) {
  throw "Failed to publish application."
}

$targets = 'win-x64','linux-x64','osx-x64','ubuntu.18.04-x64'
foreach ($target in $targets) {
  dotnet publish $proj -c Release --self-contained true -r $target --output "$publishOutputDir\$target"
  if ($LASTEXITCODE -ne 0) {
    throw "Failed to publish application."
  }
}

foreach ($target in $targets) {
  Get-ChildItem -Path "$publishOutputDir\$target" | ForEach-Object {
    if ($_.Name -notlike "*git*") {
      Remove-Item $_
    }
    # if (Test-Path "$publishOutputDir\$($_.Name)") {
    #   Remove-Item $_
    # }
  }
}

# Copy-Item -Path "$PSScriptRoot\src\post-install.ps1" -Destination $publishOutputDir
Get-ChildItem -Path $publishOutputDir -Filter "*.pdb" -Recurse | Remove-Item

# $manifestBuildDir = "$PSScriptRoot\manifestBuild"
# if (Test-Path $manifestBuildDir) {
#   Remove-Item -Path $manifestBuildDir -Force -Recurse 
# }

# dotnet publish $proj -c Release --self-contained true -r 'win-x64' --output $manifestBuildDir

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
  RootModule = "$appName.dll"
  # ScriptsToProcess = @("post-install.ps1")
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

# & "$PSScriptRoot\consolidate-artifacts.ps1" -RootDirectory "$PSScriptRoot\src\$appName\bin\Release\netstandard2.0" -PublishDirectory $publishOutputDir -Targets $targets

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