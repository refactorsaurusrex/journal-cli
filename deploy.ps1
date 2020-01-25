$ErrorActionPreference = 'Stop'

if ($env:APPVEYOR_REPO_TAG -eq 'true') {
  Write-Host "Publishing to the PowerShell Gallery..."
  Publish-Module -NuGetApiKey $env:psgallery -Path .\publish\JournalCli
  Write-Host "Package successfully published to the PowerShell Gallery."
} elseif ($env:APPVEYOR_REPO_BRANCH -eq 'master' -and $null -eq $env:APPVEYOR_PULL_REQUEST_NUMBER) {
  $options = @{
    Name = 'MyGet'
    SourceLocation = 'https://www.myget.org/F/journal-cli/api/v2'
    PublishLocation = 'https://www.myget.org/F/journal-cli/api/v2/package'
    InstallationPolicy = 'Trusted'
  }
  Write-Host "Registering the MyGet Gallery..."
  Register-PSRepository @options
  Write-Host "Publishing to the MyGet Gallery..."
  Publish-Module -NuGetApiKey $env:myget -Path .\publish\JournalCli -Repository MyGet
  Write-Host "Package successfully published to the MyGet pre-release feed."
} else {
  Write-Host "Non-master build and no tags pushed. Skipping deployment."
}