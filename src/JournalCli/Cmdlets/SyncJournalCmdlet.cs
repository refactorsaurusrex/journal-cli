using System;
using System.IO;
using System.Management.Automation;
using System.Reflection;
using System.Security;
using System.Text;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsData.Sync, "Journal")]
    public class SyncJournalCmdlet : JournalCmdletBase
    {
        [Parameter]
        public SwitchParameter RunSetup { get; set; }
        
        protected override void EndProcessing()
        {
            base.EndProcessing();
            
            Initialize();
        }

        private void Initialize()
        {
            const char cb = '\x2705';
            const ConsoleColor question = ConsoleColor.Green;
            const ConsoleColor statement = ConsoleColor.Yellow;
            const ConsoleColor info = ConsoleColor.Cyan;
            
            WriteHeader(new []{"JOURNAL SYNC SETUP"}, info);
            var nl = Environment.NewLine;
            var message = $"Here's what we're going to do. Some steps may not be required if you've previously gone through this process.{nl}{nl}" +
                $"1. Install or upgrade the PowerShell SecretManagement and SecretStore modules.{nl}" +
                $"2. Create a PowerShell secret vault for journal-cli, if it does not already exist.{nl}" +
                $"3. Create or update the private encryption key for your journal and store it in the journal-cli vault.{nl}" +
                $"4. Export your encryption key for safe keeping.{nl}" +
                $"5. Configure a new S3 bucket, if one does not already exist.{nl}" +
                "6. Finally, we'll encrypt each journal entry locally before uploading it to S3. (Note that your local journal will remain unencrypted.)".Wrap(WrapWidth) +
                $"{nl}{nl}For more detailed instructions, please go to https://journalcli.me/sync.{nl}{nl}";
            WriteHost(message, info);
            WriteWarning($"DO NOT LOSE YOUR ENCRYPTION KEY!{nl}");
            var disclaimer = "If you lose your encryption key, you not be able to decrypt your cloud-based journal entries. " +
                "The only solution will be to completely delete your cloud-based journal and re-sync with a new key. (Either that, you can wait " +
                "for quantum computers to become generally available, but that may take a decade or so.) Note that your local journal " +
                $"will remain unencrypted; this process encrypts a copy of each entry before uploading it to S3.{nl}";
            WriteHost(disclaimer.Wrap(WrapWidth), statement);

            if (!YesOrNo("Ready to proceed?", question, Console.BackgroundColor)) return;
            
            // TEST: Create unit test to validate that this file is, in fact, embedded. Otherwise if file is renamed, a bug will be introduced.
            var initializeVault = EmbeddedResource.Get("JournalCli.Scripts.SecretStoreConfiguration.ps1");
            ScriptBlock.Create(initializeVault).Invoke();

            const string keyName = "JournalCliPrivateKey";
            
            var keyInfo = ScriptBlock.Create($"Get-SecretInfo -Name '{keyName}'").Invoke();
            if (keyInfo.Count == 0)
            {
                string key;
                if (YesOrNo("Do you already have a private encryption key? If not, I'll create a new one for you."))
                {
                    key = CollectSecret("Please paste your key here:", question);
                    WriteHost($"Key has been saved to the Secret Store under the name '{keyName}'.", statement);
                }
                else
                {
                    key = JournalSynchronizer.CreatePrivateKey();
                    WriteHost(key);
                    WriteHost($"{nl}This is your private key. It's been safely stored on this machine using the PowerShell Secret Store. " +
                        "Now, please save a copy of this key somewhere else safe, such as a password manager. At the risk of being a broken " +
                        $"record, it's essential that you do not lose this key - even if your current computer suddenly dies.{nl}".Wrap(WrapWidth), statement);
                }
                
                ScriptBlock.Create($"Set-Secret -Name '{keyName}' -Secret '{key}' -Vault 'JournalCli'").Invoke();

                while (!YesOrNo("Have you saved the key?")) { }
            }
            else
            {
                WriteHost($"{cb} A private key for journal-cli already exists on this machine.{nl}", statement);
            }
            
            var createBucket = YesOrNo("Okay, now let's work on your S3 bucket. ")
        }
    }
}
