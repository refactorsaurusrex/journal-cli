param(
    [Parameter(Position = 0, Mandatory = $true)][string]$ProjectName
)

$ErrorActionPreference = 'Stop'

Remove-Item "$PSScriptRoot\publish\$ProjectName\" -Recurse -Force -ErrorAction SilentlyContinue
$proj = Get-ChildItem -Filter "$ProjectName.csproj" -Recurse | Select-Object -First 1 -ExpandProperty FullName
dotnet publish $proj -r 'win10-x64' --output "$PSScriptRoot\publish\$ProjectName" --self-contained true
Import-Module "$PSScriptRoot\publish\$ProjectName\$ProjectName.dll"