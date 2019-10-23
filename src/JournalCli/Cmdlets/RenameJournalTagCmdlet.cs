using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Rename, "JournalTag", ConfirmImpact = ConfirmImpact.High)]
    public class RenameJournalTagCmdlet : JournalCmdletBase
    {
        [Parameter]
        public SwitchParameter DryRun { get; set; }

        [Parameter(Mandatory = true)]
        public string OldName { get; set; }

        [Parameter(Mandatory = true)]
        public string NewName { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            if (!DryRun)
            {
                WriteHeader($"You're about to replace all tags named '{OldName}' with the new name '{NewName}'.", ConsoleColor.Red);
                if (!AreYouSure($"Rename '{OldName}' to '{NewName}'."))
                    return;
            }

            var fileSystem = new FileSystem();
            var systemProcess = new SystemProcess();
            var ioFactory = new JournalReaderWriterFactory(fileSystem, Location);
            var markdownFiles = new MarkdownFiles(fileSystem, Location);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            IEnumerable<string> effectedEntries;

            if (DryRun)
            {
                const string header = "Tags in the following entries would be renamed:";
                WriteHeader(header, ConsoleColor.DarkGreen);

                effectedEntries = journal.RenameTagDryRun(OldName);
            }
            else
            {
                const string header = "Tags in the following entries have been renamed:";
                WriteHeader(header, ConsoleColor.Yellow);

                Commit(GitCommitType.PreRenameTag);
                effectedEntries = journal.RenameTag(OldName, NewName);
                Commit(GitCommitType.PostRenameTag);
            }

            var counter = 1;
            var consoleColor = DryRun ? ConsoleColor.DarkGreen : ConsoleColor.Yellow;
            foreach (var file in effectedEntries)
            {
                WriteHost($"{counter++.ToString().PadLeft(3)}) {file}", consoleColor);
            }
        }
    }
}
