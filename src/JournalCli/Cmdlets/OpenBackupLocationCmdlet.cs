using System.IO.Abstractions;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Open, "BackupLocation")]
    public class OpenBackupLocationCmdlet : CmdletBase
    {
        protected override void ProcessRecord()
        {
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
