$ErrorActionPreference = 'Stop'

if ($env:APPVEYOR_REPO_TAG -eq 'true') {
  Publish-Module -NuGetApiKey $env:psgallery -Path .\publish\JournalCli
  Write-Host "Package successfully published to the PowerShell Gallery."
} else {
  $options = @{
    Name = 'MyGet'
    SourceLocation = 'https://www.myget.org/F/journal-cli/api/v2'
    PublishLocation = 'https://www.myget.org/F/journal-cli/api/v2/package'
    InstallationPolicy = 'Trusted'
  }
  Register-PSRepository @options
  Publish-Module -NuGetApiKey $env:myget -Path .\publish\JournalCli -Repository MyGet
  Write-Host "Package successfully published to the MyGet pre-release feed."
}