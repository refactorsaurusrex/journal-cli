using System;
using System.IO;
using System.Management.Automation;
using JetBrains.Annotations;

namespace JournalCli.Commands
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Remove, "OldFiles", ConfirmImpact = ConfirmImpact.High)]
    public class RemoveOldFilesCmdlet : JournalCmdletBase
    {
        private const string Warning = "***** Hey, you! *****\r\n" +
            "This function will find and PERMANENTLY delete all '.old' files found in your journal directory. Consider creating a backup " +
            "before proceeding, by running 'Backup-Journal'.";

        [Parameter]
        public SwitchParameter DryRun { get; set; }

        protected override void ProcessRecord()
        {
            if (!DryRun)
                WriteHost(Warning, ConsoleColor.Red);

            if (!DryRun && !ShouldContinue("Do you want to continue?", "Deleting .old files..."))
                return;

            var oldFiles = Directory.GetFiles(GetResolvedRootDirectory(), "*.old", SearchOption.AllDirectories);

            if (DryRun)
            {
                const string header = "The following files would be deleted:";
                WriteHost(header, ConsoleColor.Cyan);
                WriteHost(new string('=', header.Length), ConsoleColor.Cyan);

                var counter = 1;
                foreach (var oldFile in oldFiles)
                    WriteHost($"{counter++.ToString().PadLeft(3)}: {oldFile}", ConsoleColor.Cyan);
            }
            else
            {
                foreach (var oldFile in oldFiles)
                {
                    File.Delete(oldFile);
                    WriteHost($"Deleted: {oldFile}", ConsoleColor.Red);
                }

                WriteHost($"Done! Deleted {oldFiles.Length} file(s).", ConsoleColor.Cyan);
            }
        }
    }
}