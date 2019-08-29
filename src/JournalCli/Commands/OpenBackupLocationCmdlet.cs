using System.Diagnostics;
using System.IO.Abstractions;
using System.Management.Automation;
using JetBrains.Annotations;

namespace JournalCli.Commands
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
                Process.Start(new ProcessStartInfo(path)
                {
                    UseShellExecute = true
                });
            }
            else
            {
                throw new PSInvalidOperationException("Backup location not found");
            }
        }
    }
}
