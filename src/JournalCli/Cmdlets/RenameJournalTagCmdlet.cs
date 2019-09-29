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
    [Cmdlet(VerbsCommon.Rename, "JournalTags", ConfirmImpact = ConfirmImpact.High)]
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

            if (!DryRun && !ShouldContinue("Do you want to continue?", $"Renaming '{OldName}' tags to '{NewName}'..."))
                return;

            var fileSystem = new FileSystem();
            var systemProcess = new SystemProcess();
            var ioFactory = new JournalReaderWriterFactory(fileSystem, RootDirectory);
            var markdownFiles = new MarkdownFiles(fileSystem, RootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            IEnumerable<string> effectedFiles;

            if (DryRun)
            {
                const string header = "Tags in these file(s) would be renamed:";
                WriteHost(header, ConsoleColor.Cyan);
                WriteHost(new string('=', header.Length), ConsoleColor.Cyan);

                effectedFiles = journal.RenameTagDryRun(OldName);
            }
            else
            {
                const string header = "Tags in these file(s) have been renamed:";
                WriteHost(header, ConsoleColor.Red);
                WriteHost(new string('=', header.Length), ConsoleColor.Red);

                Commit(GitCommitType.PreRenameTag);
                effectedFiles = journal.RenameTag(OldName, NewName);
                Commit(GitCommitType.PostRenameTag);
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
