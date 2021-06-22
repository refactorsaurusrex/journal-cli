using System.IO.Abstractions;
using System.Linq;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsData.Initialize, "JournalSyncSettings")]
    public class InitializeJournalSyncSettingsCmdlet : JournalSyncCmdletBase
    {
        [Parameter(ParameterSetName = "Initialize")]
        public SwitchParameter CreateNewPrivateKey { set; get; }
        
        [Parameter(ParameterSetName = "Initialize")]
        public SwitchParameter CreateNewS3Bucket { get; set; }
        
        [Parameter(ParameterSetName = "Initialize")]
        [Parameter(ParameterSetName = "NewMachine")]
        public string AwsProfileName { get; set; }

        [Parameter(ParameterSetName = "Initialize")]
        [Parameter(ParameterSetName = "NewMachine")]
        [ArgumentCompleter(typeof(AwsRegionCompleter))]
        public string AwsRegion { get; set; }

        [Parameter(ParameterSetName = "NewMachine")]
        public string JournalCliPrivateKey { get; set; }

        [Parameter(ParameterSetName = "NewMachine")]
        public string S3BucketName { get; set; }
        
        [Parameter(ParameterSetName = "ReviewOnly")]
        public SwitchParameter ReviewOnly { get; set; }

        protected override void EndProcessing()
        {
            base.EndProcessing();

            var fileSystem = new FileSystem();
            var store = new FileStore<SyncSettings>(fileSystem);
            var settings = store.Load();
            
            switch (ParameterSetName)
            {
                case "Initialize":
                    InstallSecretModulesIfMissing();
                    SaveNewPrivateKey(settings);
                    SaveAwsProfileName(settings);
                    SaveAwsRegion(settings);
                    var bucketName = ConfigureNewS3Bucket(settings);
                    settings.BucketName = bucketName;
                    store.Save(settings);
                    break;
                case "NewMachine":
                    InstallSecretModulesIfMissing();
                    SaveProvidedPrivateKey();
                    SaveS3BucketName(settings);
                    SaveAwsProfileName(settings);
                    SaveAwsRegion(settings);
                    store.Save(settings);
                    break;
                case "ReviewOnly":
                    // Take no action other than printing status and warnings
                    break;
            }

            if (settings.IsValid())
                WriteHost($"{Checkbox} Sync settings are valid!");
            else
                WriteWarning("One or more sync settings is incorrectly configured. Journal synchronization will not work until settings are valid.");

            WriteObject(settings);
        }

        private void SaveS3BucketName(SyncSettings settings)
        {
            if (string.IsNullOrWhiteSpace(S3BucketName))
                return;

            if (!string.IsNullOrWhiteSpace(settings.BucketName) && 
                !YesOrNo($"You're about to replace the previously saved bucket name '{settings.BucketName}' with '{S3BucketName}'. Are you sure you want to do this?"))
                return;

            settings.BucketName = S3BucketName;
        }

        private void SaveProvidedPrivateKey()
        {
            if (PrivateKeyExists() && 
                !YesOrNo("A private encryption key already exists on this machine. Are you sure you want to replace it?"))
                return;

            ScriptBlock.Create($"Set-Secret -Name '{PrivateKeyName}' -Secret '{JournalCliPrivateKey}' -Vault 'JournalCli'").Invoke();
        }

        private void SaveAwsRegion(SyncSettings settings)
        {
            if (string.IsNullOrWhiteSpace(AwsRegion))
                return;

            if (!string.IsNullOrWhiteSpace(settings.AwsRegion) &&
                !YesOrNo($"The region '{settings.AwsRegion}' has previously been saved. Do you want to overwrite it with '{AwsRegion}'?"))
                return;

            settings.AwsRegion = AwsRegion;
        }

        private void SaveAwsProfileName(SyncSettings settings)
        {
            if (string.IsNullOrWhiteSpace(AwsProfileName))
                return;

            if (!string.IsNullOrWhiteSpace(settings.AwsProfileName) &&
                !YesOrNo($"The region '{settings.AwsProfileName}' has previously been saved. Do you want to overwrite it with '{AwsProfileName}'?"))
                return;

            settings.AwsProfileName = AwsProfileName;
        }

        private string ConfigureNewS3Bucket(SyncSettings settings)
        {
            var key = GetPrivateKey();
            var syncer = new JournalSynchronizer(key, settings);
            return syncer.CreateBucket().Result;
        }

        private void SaveNewPrivateKey(SyncSettings settings)
        {
            if (PrivateKeyExists() && 
                !YesOrNo("A private encryption key already exists on this machine. Are you sure you want to replace it?"))
                return;
            var key = JournalSynchronizer.CreatePrivateKey();
            ScriptBlock.Create($"Set-Secret -Name '{PrivateKeyName}' -Secret '{key}' -Vault 'JournalCli'").Invoke();
        }

        private void InstallSecretModulesIfMissing()
        {
            var secretMgmtModuleIsInstalled = ScriptBlock.Create("get-module -ListAvailable -Name Microsoft.PowerShell.SecretManagement").Invoke().Count != 0;
            if (secretMgmtModuleIsInstalled)
            {
                WriteHost($"{Checkbox} SecretManagement module already installed.", Info);
            }
            else
            {
                WriteHost($"{Checkbox} Installing SecretManagement module...", Info);
                ScriptBlock.Create("Install-Module Microsoft.PowerShell.SecretManagement").Invoke();
            }

            var secretStoreModuleIsInstalled = ScriptBlock.Create("get-module -ListAvailable -Name Microsoft.PowerShell.SecretStore").Invoke().Count != 0;
            if (secretStoreModuleIsInstalled)
            {
                WriteHost($"{Checkbox} SecretStore module already installed.", Info);   
            }
            else
            {
                WriteHost($"{Checkbox} Installing SecretStore module...", Info);
                ScriptBlock.Create("Install-Module Microsoft.PowerShell.SecretStore").Invoke();
            }

            var vaultExists = ScriptBlock.Create("Get-SecretVault | Where-Object { $_.Name -eq 'JournalCli'}").Invoke().Any();
            if (!vaultExists)
            {
                ScriptBlock.Create("Register-SecretVault -Name 'JournalCli' -ModuleName Microsoft.PowerShell.SecretStore").Invoke();
                WriteHost($"{Checkbox} Created 'JournalCli' secret vault.");
            }
            else
            {
                WriteHost($"{Checkbox} 'JournalCli' secret vault already exists.");
            }
        }
    }
}