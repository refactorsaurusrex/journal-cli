using System;
using System.IO.Abstractions;
using System.Management.Automation;
using JetBrains.Annotations;
using SysIO = System.IO;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Remove, "BackupJournalFiles", ConfirmImpact = ConfirmImpact.High)]
    public class RemoveBackupJournalFilesCmdlet : JournalCmdletBase
    {
        private readonly string _warning = "***** Hey, you! *****\r\n" +
            $"This function will find and PERMANENTLY DELETE all '{Constants.BackupFileExtension}' files found in your journal directory. " +
            "Consider creating a full backup before proceeding, by running 'Backup-Journal'.";

        [Parameter]
        public SwitchParameter DryRun { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (!DryRun)
                WriteHost(_warning, ConsoleColor.Red);

            // TODO: Move ShouldContinue to base class
            if (!DryRun && !ShouldContinue("Do you want to continue?", $"Deleting '{Constants.BackupFileExtension}' files..."))
                return;

            var fileSystem = new FileSystem();
            var backupFiles = fileSystem.Directory.GetFiles(RootDirectory, $"*{Constants.BackupFileExtension}", SysIO.SearchOption.AllDirectories);

            if (DryRun)
            {
                // TODO: Move Header writer to base class
                const string header = "The following files would be deleted:";
                WriteHost(header, ConsoleColor.Cyan);
                WriteHost(new string('=', header.Length), ConsoleColor.Cyan);

                var counter = 1;
                foreach (var backupFile in backupFiles)
                    WriteHost($"{counter++.ToString().PadLeft(3)}: {backupFile}", ConsoleColor.Cyan);
            }
            else
            {
                foreach (var backupFile in backupFiles)
                {
                    fileSystem.File.Delete(backupFile);
                    WriteHost($"Deleted: {backupFile}", ConsoleColor.Red);
                }

                WriteHost($"Done! Deleted {backupFiles.Length} file(s).", ConsoleColor.Cyan);
            }
        }
    }
}