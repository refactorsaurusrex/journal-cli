using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    public abstract class JournalSyncCmdletBase : JournalCmdletBase
    {
        protected const string PrivateKeyName = "JournalCliPrivateKey";
        protected string GetPrivateKey() => ScriptBlock.Create($"Get-Secret -Name '{PrivateKeyName}' -Vault 'JournalCli'").Invoke()[0].BaseObject.ToString();
        protected bool PrivateKeyExists() => ScriptBlock.Create($"Get-SecretInfo -Name '{PrivateKeyName}'").Invoke().Count != 0;
    }
    
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

            // TODO: Always print status and warnings here
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

    public class AwsRegionCompleter : IArgumentCompleter
    {
        private readonly List<string> _regions;
        
        public AwsRegionCompleter() => _regions = Amazon.RegionEndpoint.EnumerableAllRegions.Select(x => x.SystemName).ToList();

        public IEnumerable<CompletionResult> CompleteArgument(string commandName, string parameterName, string wordToComplete, CommandAst commandAst, IDictionary fakeBoundParameters)
        {
            return _regions.Where(x => Regex.IsMatch(x, wordToComplete.WildCardsToRegex())).Select(x => new CompletionResult(x));
        }
    }

    [PublicAPI]
    [Cmdlet(VerbsData.Sync, "Journal")]
    public class SyncJournalCmdlet : JournalSyncCmdletBase
    {
        // [Parameter]
        // public SwitchParameter RunSetup { get; set; }
        
        protected override void EndProcessing()
        {
            base.EndProcessing();
            WriteWarning("JournalCli sync is in beta and has not been thoroughly tested. " +
                "Please report bugs to https://github.com/refactorsaurusrex/journal-cli/issues.".Wrap(WrapWidth));
            
            // TODO: If syncSettings is incomplete, or if RunSetup is passed in
            // RunInterview();
        }

        // private void RunInterview()
        // {
        //     var titles = new []
        //     {
        //         "JOURNAL SYNC SETUP",
        //         "This process will configure this machine to securely sync your journal to S3.",
        //         "For detailed instructions and additional information, please go to https://journalcli.me/sync."
        //     };
        //     WriteHeader(titles, Info);
        //
        //     if (!YesOrNo("Ready to proceed?"))
        //         return;
        //
        //     InstallSecretModulesIfMissing();
        //     CreateUpdateOrValidatePrivateKey();
        //     ConfigureS3Bucket();
        // }
        //
        // private void ConfigureS3Bucket()
        // {
        //     var fileSystem = new FileSystem();
        //     var fileStore = new FileStore<SyncSettings>(fileSystem);
        //     var settings = fileStore.Load();
        //     
        //     if (string.IsNullOrWhiteSpace(settings.AwsProfileName))
        //     {
        //         const string message = "Please provide an AWS profile name that can be used to manage your journal-cli S3 bucket. The specific permissions " +
        //             "required are documented on https://journalcli.me/sync.";
        //         WriteHost(message, Statement);
        //         settings.AwsProfileName = ReadHost();
        //         fileStore.Save(settings);
        //     }
        //     else
        //     {
        //         var question = $"The AWS profile name '{settings.AwsProfileName}' has previously been saved. Do you want to continuing it, or " +
        //             "change it to something else?";
        //         var choice = Choice("AWS Profile Name", question, 0, "Keep &existing", "Enter &new");
        //         if (choice == 1)
        //         {
        //             WriteHost("Enter the new profile name:", Statement);
        //             settings.AwsProfileName = ReadHost();
        //             fileStore.Save(settings);
        //         }
        //     }
        //
        //     if (string.IsNullOrWhiteSpace(settings.BucketName))
        //     {
        //         if (YesOrNo("Do you already have an S3 bucket that was created for journal-cli?"))
        //         {
        //             WriteHost("Enter the bucket name:", Question);
        //             settings.BucketName = ReadHost();
        //         }
        //     }
        //     else
        //     {
        //         var question = $"The bucket name '{settings.AwsProfileName}' has previously been saved. Do you want to continuing it, or " +
        //             "change it to something else?";
        //         var choice = Choice("Bucket Name", question, 0, $"&Keep {settings.AwsProfileName}", "Enter &different name");
        //         if (choice == 1)
        //         {
        //             WriteHost("Enter the bucket name. Note that this must be a bucket created by journal-cli, not one you created manually.", Statement);
        //             settings.BucketName = ReadHost();
        //             fileStore.Save(settings);
        //         }
        //     }
        //
        //     var key = GetPrivateKey();
        //     var synchronizer = new JournalSynchronizer(key, settings);
        //     synchronizer.CreateOrVerifyBucket();
        // }
        //
        // private void CreateUpdateOrValidatePrivateKey()
        // {
        //     var privateKeyExists = ScriptBlock.Create($"Get-SecretInfo -Name '{PrivateKeyName}'").Invoke().Count != 0;
        //     const string choiceTitle = "Private Encryption Key";
        //     string key;
        //     
        //     if (privateKeyExists)
        //     {
        //         const string message = "A private encryption key already exists on this machine. Do you want to keep it, or replace it with a different one?";
        //         var answer = Choice(choiceTitle, message, 0, "Keep &existing", "Create &new", "I have a &key");
        //
        //         switch (answer)
        //         {
        //             default:
        //                 throw new NotSupportedException("The selected choice is not currently supported");
        //             case 0:
        //                 return;
        //             case 1:
        //                 if (YesOrNo("Are you absolutely certain you want to replace the existing key with a new one?", Question, Console.BackgroundColor))
        //                 {
        //                     key = JournalSynchronizer.CreatePrivateKey();
        //                     WriteKeyWithWarning(key);
        //                 }
        //                 else
        //                 {
        //                     throw new SyncSetupAbortedException();
        //                 }
        //                     
        //                 break;
        //             case 2:
        //                 if (YesOrNo("Are you absolutely certain you want to replace the existing key with one you'll provide?"))
        //                 {
        //                     key = CollectSecret("Please paste your key here:", Question);
        //                 }
        //                 else
        //                 {
        //                     throw new SyncSetupAbortedException();
        //                 }
        //                 break;
        //         }
        //     }
        //     else
        //     {
        //         const string message = "A private encryption key could not be found on this machine. Do you have one already, or do you need a new one?";
        //         var answer = Choice(choiceTitle, message, 1, "I &already have one", "I &need a new one");
        //         switch (answer)
        //         {
        //             case 1:
        //                 key = CollectSecret("Please paste your key here:", Question);
        //                 break;
        //             case 2:
        //                 key = JournalSynchronizer.CreatePrivateKey();
        //                 WriteKeyWithWarning(key);
        //                 break;
        //             default:
        //                 throw new NotSupportedException("The selected choice is not currently supported");
        //         }
        //     }
        //     
        //     ScriptBlock.Create($"Set-Secret -Name '{PrivateKeyName}' -Secret '{key}' -Vault 'JournalCli'").Invoke();
        // }
        //
        // private void WriteKeyWithWarning(string key)
        // {
        //     WriteHost(key);
        //     WriteHost($"{NL}This is your private key. It's been safely stored on this machine using the PowerShell Secret Store. " +
        //         "Now, please save a copy of this key somewhere else safe, such as a password manager. At the risk of being a broken " +
        //         $"record, it's essential that you do not lose this key - even if your current computer suddenly dies.{NL}".Wrap(WrapWidth), Statement);
        // }
        //
        // private void InstallSecretModulesIfMissing()
        // {
        //     var secretMgmtModuleIsInstalled = ScriptBlock.Create("get-module -ListAvailable -Name Microsoft.PowerShell.SecretManagement").Invoke().Count != 0;
        //     if (secretMgmtModuleIsInstalled)
        //     {
        //         WriteHost($"{Checkbox} SecretManagement module already installed.", Info);
        //     }
        //     else
        //     {
        //         WriteHost($"{Checkbox} Installing SecretManagement module...", Info);
        //         ScriptBlock.Create("Install-Module Microsoft.PowerShell.SecretManagement").Invoke();
        //     }
        //
        //     var secretStoreModuleIsInstalled = ScriptBlock.Create("get-module -ListAvailable -Name Microsoft.PowerShell.SecretStore").Invoke().Count != 0;
        //     if (secretStoreModuleIsInstalled)
        //     {
        //         WriteHost($"{Checkbox} SecretStore module already installed.", Info);   
        //     }
        //     else
        //     {
        //         WriteHost($"{Checkbox} Installing SecretStore module...", Info);
        //         ScriptBlock.Create("Install-Module Microsoft.PowerShell.SecretStore").Invoke();
        //     }
        //
        //     var vaultExists = ScriptBlock.Create("Get-SecretVault | Where-Object { $_.Name -eq 'JournalCli'}").Invoke().Any();
        //     if (!vaultExists)
        //     {
        //         ScriptBlock.Create("Register-SecretVault -Name 'JournalCli' -ModuleName Microsoft.PowerShell.SecretStore").Invoke();
        //         WriteHost($"{Checkbox} Created 'JournalCli' secret vault.");
        //     }
        //     else
        //     {
        //         WriteHost($"{Checkbox} 'JournalCli' secret vault already exists.");
        //     }
        // }
    }
}
