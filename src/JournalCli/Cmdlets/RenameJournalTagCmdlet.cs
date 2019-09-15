using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Rename, "JournalTags", ConfirmImpact = ConfirmImpact.High)]
    public class RenameJournalTagCmdlet : JournalCmdletBase
    {
        [Parameter]
        public SwitchParameter DryRun { get; set; }

        [Parameter]
        public SwitchParameter NoBackups { get; set; }

        [Parameter(Mandatory = true)]
        public string OldName { get; set; }

        [Parameter(Mandatory = true)]
        public string NewName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (!DryRun && NoBackups)
            {
                var warning = "***** Hey, you! *****\r\n" +
                    $"This function will rename all '{OldName}' tags to '{NewName}' and you've disabled local backups. " + 
                    "Consider creating a full backup before proceeding, by running 'Backup-Journal'.";
                WriteHost(warning, ConsoleColor.Red);
            }

            if (!DryRun && !ShouldContinue("Do you want to continue?", $"Renaming '{OldName}' tags to '{NewName}'..."))
                return;

            var fileSystem = new FileSystem();
            var systemProcess = new SystemProcess();
            var readerFactory = new JournalReaderFactory(fileSystem);
            var journal = Journal.Open(readerFactory, fileSystem, systemProcess, RootDirectory);
            IEnumerable<string> effectedFiles;

            if (DryRun)
            {
                const string header = "Tags in these file(s) would be renamed:";
                WriteHost(header, ConsoleColor.Cyan);
                WriteHost(new string('=', header.Length), ConsoleColor.Cyan);

                var index = journal.CreateIndex(false);
                var journalEntries = index.SingleOrDefault(x => x.Tag == OldName);

                if (journalEntries == null)
                    throw new InvalidOperationException($"No entries found with the tag '{OldName}'");

                effectedFiles = journalEntries.Entries.Select(e => e.FilePath);
            }
            else
            {
                const string header = "Tags in these file(s) have been renamed:";
                WriteHost(header, ConsoleColor.Red);
                WriteHost(new string('=', header.Length), ConsoleColor.Red);

                var journalWriter = new JournalWriter(fileSystem);
                effectedFiles = journalWriter.RenameTag(journal, OldName, NewName, DryRun);
            }

            var counter = 1;
            var consoleColor = DryRun ? ConsoleColor.Cyan : ConsoleColor.Red;
            foreach (var file in effectedFiles)
            {
                WriteHost($"{counter++.ToString().PadLeft(3)}) {file}", consoleColor);
            }
        }
    }
}
