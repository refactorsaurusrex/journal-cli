if (-not (Get-ChildItem -Path $PSScriptRoot -Directory)) {
  return
}

if ($IsWindows) {
  $target = "win-x64"
} elseif ($IsMacOS) {
  $target = "osx-x64"
} elseif ($IsLinux) {
  if ($PSVersionTable.OS -like "*ubuntu*") {
    $target = "ubuntu.18.04-x64"
  } else {
    $target = "linux-x64"
  }
}

Move-Item -Path "$PSScriptRoot\$target\*.*" -Destination $module -Force
Get-ChildItem -Path $PSScriptRoot -Directory | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue