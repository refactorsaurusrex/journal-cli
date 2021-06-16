using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;
using Org.BouncyCastle.Crypto.Prng;
using YamlDotNet.Core.Tokens;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsData.Sync, "Journal")]
    public class SyncJournalCmdlet : JournalCmdletBase
    {
        private const char Checkbox = '\x2705';
        private const ConsoleColor Question = ConsoleColor.Green;
        private const ConsoleColor Statement = ConsoleColor.Yellow;
        private const ConsoleColor Info = ConsoleColor.Cyan;
        private const string KeyName = "JournalCliPrivateKey";
        // ReSharper disable once InconsistentNaming
        private string nl = Environment.NewLine;

        [Parameter]
        public SwitchParameter RunSetup { get; set; }
        
        protected override void EndProcessing()
        {
            base.EndProcessing();
            WriteWarning("JournalCli sync is in beta and has not been thoroughly tested. " +
                "Please report bugs to https://github.com/refactorsaurusrex/journal-cli/issues.".Wrap(WrapWidth));
            
            // TODO: If syncSettings is incomplete, or if RunSetup is passed in
            RunInterview();
        }

        private void RunInterview()
        {
            var titles = new []
            {
                "JOURNAL SYNC SETUP",
                "This process will configure this machine to securely sync your journal to S3.",
                "For detailed instructions and additional information, please go to https://journalcli.me/sync."
            };
            WriteHeader(titles, Info);

            if (!YesOrNo("Ready to proceed?"))
                return;

            InstallSecretModulesIfMissing();
            CreateUpdateOrValidatePrivateKey();
            ConfigureS3Bucket();
        }

        private void ConfigureS3Bucket()
        {
            var fileSystem = new FileSystem();
            var fileStore = new FileStore<SyncSettings>(fileSystem);
            var settings = fileStore.Load();
            
            if (string.IsNullOrWhiteSpace(settings.AwsProfileName))
            {
                const string message = "Please provide an AWS profile name that can be used to manage your journal-cli S3 bucket. The specific permissions " +
                    "required are documented on https://journalcli.me/sync.";
                WriteHost(message, Statement);
                settings.AwsProfileName = ReadHost();
                fileStore.Save(settings);
            }
            else
            {
                var question = $"The AWS profile name '{settings.AwsProfileName}' has previously been saved. Do you want to continuing it, or " +
                    "change it to something else?";
                var choice = Choice("AWS Profile Name", question, 0, "Keep &existing", "Enter &new");
                if (choice == 1)
                {
                    WriteHost("Enter the new profile name:", Statement);
                    settings.AwsProfileName = ReadHost();
                    fileStore.Save(settings);
                }
            }

            if (string.IsNullOrWhiteSpace(settings.BucketName))
            {
                if (YesOrNo("Do you already have an S3 bucket that was created for journal-cli?"))
                {
                    WriteHost("Enter the bucket name:", Question);
                    settings.BucketName = ReadHost();
                }
            }
            else
            {
                var question = $"The bucket name '{settings.AwsProfileName}' has previously been saved. Do you want to continuing it, or " +
                    "change it to something else?";
                var choice = Choice("Bucket Name", question, 0, $"&Keep {settings.AwsProfileName}", "Enter &different name");
                if (choice == 1)
                {
                    WriteHost("Enter the bucket name. Note that this must be a bucket created by journal-cli, not one you created manually.", Statement);
                    settings.BucketName = ReadHost();
                    fileStore.Save(settings);
                }
            }

            var key = GetPrivateKey();
            var synchronizer = new JournalSynchronizer(key, settings);
            synchronizer.CreateOrVerifyBucket();
        }

        private string GetPrivateKey()
        {
            
        }

        private void CreateUpdateOrValidatePrivateKey()
        {
            var privateKeyExists = ScriptBlock.Create($"Get-SecretInfo -Name '{KeyName}'").Invoke().Count != 0;
            const string choiceTitle = "Private Encryption Key";
            string key;
            
            if (privateKeyExists)
            {
                const string message = "A private encryption key already exists on this machine. Do you want to keep it, or replace it with a different one?";
                var answer = Choice(choiceTitle, message, 0, "Keep &existing", "Create &new", "I have a &key");

                switch (answer)
                {
                    default:
                        throw new NotSupportedException("The selected choice is not currently supported");
                    case 0:
                        return;
                    case 1:
                        if (YesOrNo("Are you absolutely certain you want to replace the existing key with a new one?", Question, Console.BackgroundColor))
                        {
                            key = JournalSynchronizer.CreatePrivateKey();
                            WriteKeyWithWarning(key);
                        }
                        else
                        {
                            throw new SyncSetupAbortedException();
                        }
                            
                        break;
                    case 2:
                        if (YesOrNo("Are you absolutely certain you want to replace the existing key with one you'll provide?"))
                        {
                            key = CollectSecret("Please paste your key here:", Question);
                        }
                        else
                        {
                            throw new SyncSetupAbortedException();
                        }
                        break;
                }
            }
            else
            {
                const string message = "A private encryption key could not be found on this machine. Do you have one already, or do you need a new one?";
                var answer = Choice(choiceTitle, message, 1, "I &already have one", "I &need a new one");
                switch (answer)
                {
                    case 1:
                        key = CollectSecret("Please paste your key here:", Question);
                        break;
                    case 2:
                        key = JournalSynchronizer.CreatePrivateKey();
                        WriteKeyWithWarning(key);
                        break;
                    default:
                        throw new NotSupportedException("The selected choice is not currently supported");
                }
            }
            
            ScriptBlock.Create($"Set-Secret -Name '{KeyName}' -Secret '{key}' -Vault 'JournalCli'").Invoke();
        }

        private void WriteKeyWithWarning(string key)
        {
            WriteHost(key);
            WriteHost($"{nl}This is your private key. It's been safely stored on this machine using the PowerShell Secret Store. " +
                "Now, please save a copy of this key somewhere else safe, such as a password manager. At the risk of being a broken " +
                $"record, it's essential that you do not lose this key - even if your current computer suddenly dies.{nl}".Wrap(WrapWidth), Statement);
        }

        // private bool PrivateKeyExists() => ScriptBlock.Create($"Get-SecretInfo -Name '{KeyName}'").Invoke().Count != 0;

        // private void AbortInterview() =>
        //     ThrowTerminatingError("Journal sync setup process aborted. Re-run the process at any time to complete setup.", ErrorCategory.OperationStopped);

        private void InstallSecretModulesIfMissing()
        {
            var sm = ScriptBlock.Create("get-module -ListAvailable -Name Microsoft.PowerShell.SecretManagement").Invoke().Count != 0;
            if (sm)
            {
                WriteHost($"{Checkbox} SecretManagement module already installed.", Info);
            }
            else
            {
                WriteHost($"{Checkbox} Installing SecretManagement module...", Info);
                ScriptBlock.Create("Install-Module Microsoft.PowerShell.SecretManagement").Invoke();
            }

            var ss = ScriptBlock.Create("get-module -ListAvailable -Name Microsoft.PowerShell.SecretStore").Invoke().Count != 0;
            if (ss)
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
            }
        }

        // private void Initialize()
        // {
        //     // const char cb = '\x2705';
        //     // const ConsoleColor question = ConsoleColor.Green;
        //     // const ConsoleColor statement = ConsoleColor.Yellow;
        //     // const ConsoleColor info = ConsoleColor.Cyan;
        //     
        //     WriteHeader(new []{"JOURNAL SYNC SETUP"}, Info);
        //     var nl = Environment.NewLine;
        //     var message = $"Here's what we're going to do. Some steps may not be required if you've previously gone through this process.{nl}{nl}" +
        //         $"1. Install or upgrade the PowerShell SecretManagement and SecretStore modules.{nl}" +
        //         $"2. Create a PowerShell secret vault for journal-cli, if it does not already exist.{nl}" +
        //         $"3. Create or update the private encryption key for your journal and store it in the journal-cli vault.{nl}" +
        //         $"4. Export your encryption key for safe keeping.{nl}" +
        //         $"5. Configure a new S3 bucket, if one does not already exist.{nl}" +
        //         "6. Finally, we'll encrypt each journal entry locally before uploading it to S3. (Note that your local journal will remain unencrypted.)".Wrap(WrapWidth) +
        //         $"{nl}{nl}For more detailed instructions, please go to https://journalcli.me/sync.{nl}{nl}";
        //     WriteHost(message, Info);
        //     WriteWarning($"DO NOT LOSE YOUR ENCRYPTION KEY!{nl}");
        //     var disclaimer = "If you lose your encryption key, you not be able to decrypt your cloud-based journal entries. " +
        //         "The only solution will be to completely delete your cloud-based journal and re-sync with a new key. (Either that, you can wait " +
        //         "for quantum computers to become generally available, but that may take a decade or so.) Note that your local journal " +
        //         $"will remain unencrypted; this process encrypts a copy of each entry before uploading it to S3.{nl}";
        //     WriteHost(disclaimer.Wrap(WrapWidth), Statement);
        //
        //     if (!YesOrNo("Ready to proceed?", Question, Console.BackgroundColor)) return;
        //     
        //     // TEST: Create unit test to validate that this file is, in fact, embedded. Otherwise if file is renamed, a bug will be introduced.
        //     var initializeVault = EmbeddedResource.Get("JournalCli.Scripts.SecretStoreConfiguration.ps1");
        //     ScriptBlock.Create(initializeVault).Invoke();
        //
        //     var keyInfo = ScriptBlock.Create($"Get-SecretInfo -Name '{KeyName}'").Invoke();
        //     if (keyInfo.Count == 0)
        //     {
        //         string key;
        //         if (YesOrNo("Do you already have a private encryption key? If not, I'll create a new one for you."))
        //         {
        //             key = CollectSecret("Please paste your key here:", Question);
        //             WriteHost($"Key has been saved to the Secret Store under the name '{KeyName}'.", Statement);
        //         }
        //         else
        //         {
        //             key = JournalSynchronizer.CreatePrivateKey();
        //             WriteHost(key);
        //             WriteHost($"{nl}This is your private key. It's been safely stored on this machine using the PowerShell Secret Store. " +
        //                 "Now, please save a copy of this key somewhere else safe, such as a password manager. At the risk of being a broken " +
        //                 $"record, it's essential that you do not lose this key - even if your current computer suddenly dies.{nl}".Wrap(WrapWidth), Statement);
        //         }
        //         
        //         ScriptBlock.Create($"Set-Secret -Name '{KeyName}' -Secret '{key}' -Vault 'JournalCli'").Invoke();
        //
        //         while (!YesOrNo("Have you saved the key?")) { }
        //     }
        //     else
        //     {
        //         WriteHost($"{Checkbox} A private key for journal-cli already exists on this machine.{nl}", Statement);
        //     }
        //     
        //     // var createBucket = YesOrNo("Okay, now let's work on your S3 bucket. ")
        // }
    }
}
