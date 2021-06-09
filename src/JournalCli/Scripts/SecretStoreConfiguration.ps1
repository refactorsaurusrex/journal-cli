# Install-Module Microsoft.PowerShell.SecretManagement, Microsoft.PowerShell.SecretStore
# Update-Module Microsoft.PowerShell.SecretManagement, Microsoft.PowerShell.SecretStore
$cb = "`u{2705}"
Write-Host "$cb PowerShell SecretManagement and SecretStore modules have been installed and updated." -ForegroundColor Green

$vaultName = 'JournalCli'
$exists = Get-SecretVault | Where-Object { $_.Name -eq $vaultName}

if ($exists) {
  Write-Host "$cb $vaultName secret vault has already been registered." -ForegroundColor Green
} else {
  Register-SecretVault -Name $vaultName -ModuleName Microsoft.PowerShell.SecretStore
  Write-Host "$cb The $vaultName secret vault is now registered!" -ForegroundColor Green
}
