using System.Diagnostics;
using System.IO;
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
            var path = UserSettings.Load().BackupLocation;
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                Process.Start(new ProcessStartInfo(path)
                {
                    UseShellExecute = true
                });
            }
            else
            {
                ThrowTerminatingError("Backup location not found", ErrorCategory.InvalidOperation);
            }
        }
    }
}
