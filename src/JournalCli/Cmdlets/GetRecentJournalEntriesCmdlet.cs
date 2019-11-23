using System.IO.Abstractions;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "RecentJournalEntries")]
    public class GetRecentJournalEntriesCmdlet : JournalCmdletBase
    {
        [Parameter]
        public int Limit { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var fileSystem = new FileSystem();
            var readerWriterFactory = new JournalReaderWriterFactory(fileSystem, Location);
            var markdownFiles = new MarkdownFiles(fileSystem, Location);
            var systemProcess = new SystemProcess();
            var journal = Journal.Open(readerWriterFactory, markdownFiles, systemProcess);

            var entries = journal.GetRecentEntries(Limit);
            WriteObject(entries, true);
        }
    }
}
