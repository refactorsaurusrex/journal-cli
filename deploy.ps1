$ErrorActionPreference = 'Stop'

if ($env:APPVEYOR_REPO_TAG -ne 'true') {
  Write-Host "Skipping deployment. No tags pushed."
  return
}

Publish-Module -NuGetApiKey $env:psgallery -Path .\publish\JournalCli