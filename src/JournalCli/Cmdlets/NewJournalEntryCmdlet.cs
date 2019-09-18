using System.IO.Abstractions;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.New, "JournalEntry")]
    [Alias("nj")]
    public class NewJournalEntryCmdlet : JournalCmdletBase
    {
        [Parameter]
        public int DateOffset { get; set; }

        [Parameter]
        public string[] Tags { get; set; }

        [Parameter]
        public string Readme { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var fileSystem = new FileSystem();
            var ioFactory = new JournalReaderWriterFactory(fileSystem, RootDirectory);
            var markdownFiles = new MarkdownFiles(fileSystem, RootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, new SystemProcess());
            var entryDate = Today.MinusDays(DateOffset);
            journal.CreateNewEntry(entryDate, Tags, Readme);
        }
    }
}