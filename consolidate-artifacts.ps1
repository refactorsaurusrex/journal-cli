param (
  [string][Parameter(Mandatory=$true)]$RootDirectory,
  [string][Parameter(Mandatory=$true)]$PublishDirectory,
  [string[]][Parameter(Mandatory=$true)]$Targets
)

$ErrorActionPreference = 'Stop'

$allArtifacts = @{}
foreach ($target in $Targets) {
  Get-ChildItem -Path "$RootDirectory\$target\publish\" -File | ForEach-Object {
    $hash = Get-FileHash -Path $_.FullName -Algorithm MD5
    $allArtifacts.Add($_.FullName, $hash.Hash)
  }
}

foreach ($item in $allArtifacts.GetEnumerator()) {
  $count = $allArtifacts.Values | Where-Object {$_ -eq $item.Value } | Measure-Object | Select-Object -ExpandProperty Count
  if ($count -eq $Targets.Length) {
    $filename = Split-Path -Path $item.Key -Leaf
    $destinationPath = "$PublishDirectory\$filename"
    if (Test-Path $destinationPath) {
      Remove-Item $item.Key
    } else {
      Move-Item -Path $item.Key -Destination "$PublishDirectory"
    }
  }
}

foreach ($target in $Targets) {
  Move-Item -Path "$RootDirectory\$target\publish\" -Destination "$PublishDirectory\$target"
}