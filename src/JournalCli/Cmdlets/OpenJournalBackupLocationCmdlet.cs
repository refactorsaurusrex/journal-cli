using System.IO.Abstractions;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Open, "JournalBackupLocation")]
    [Alias("Open-BackupLocation")]
    public class OpenJournalBackupLocationCmdlet : CmdletBase
    {
        protected override void ProcessRecord()
        {
            if (MyInvocation.InvocationName == "Open-BackupLocation")
                WriteWarning("'Open-BackupLocation' is obsolete and will be removed in a future release. Use 'Open-JournalBackupLocation' instead.");

            var fileSystem = new FileSystem();
            var encryptedStore = EncryptedStoreFactory.Create<UserSettings>();
            var path = UserSettings.Load(encryptedStore).BackupLocation;
            if (!string.IsNullOrEmpty(path) && fileSystem.Directory.Exists(path))
            {
                SystemProcess.Start(path);
            }
            else
            {
                throw new PSInvalidOperationException("Backup location not found");
            }
        }
    }
}
