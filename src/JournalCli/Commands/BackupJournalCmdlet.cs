using System;
using System.IO;
using System.Management.Automation;
using ICSharpCode.SharpZipLib.Zip;
using JetBrains.Annotations;

namespace JournalCli.Commands
{
    [PublicAPI]
    [Cmdlet(VerbsData.Backup, "Journal")]
    public class BackupJournalCmdlet : JournalCmdletBase
    {
        [Parameter]
        public string BackupLocation { get; set; }

        [Parameter]
        public SwitchParameter SaveLocation { get; set; }

        protected override void ProcessRecord()
        {
            if (string.IsNullOrEmpty(BackupLocation))
            {
                var settings = UserSettings.Load();
                if (string.IsNullOrEmpty(settings.BackupLocation))
                    throw new PSInvalidOperationException("Backup location not provided and no location was previously saved.");

                BackupLocation = settings.BackupLocation;
            }
            else
            {
                if (!Directory.Exists(BackupLocation))
                    Directory.CreateDirectory(BackupLocation);

                BackupLocation = ResolvePath(BackupLocation);
                if (SaveLocation)
                {
                    var settings = UserSettings.Load();
                    settings.BackupLocation = BackupLocation;
                    settings.Save();
                }
            }

            var fileName = $"{DateTime.Now:yyyy.MM.dd.H.mm.ss.FFF}.zip";
            var destinationPath = Path.Combine(BackupLocation, fileName);
            var journalRoot = GetResolvedRootDirectory();

            var zip = new FastZip { CreateEmptyDirectories = true };
            zip.CreateZip(destinationPath, journalRoot, true, null);
        }
    }
}
