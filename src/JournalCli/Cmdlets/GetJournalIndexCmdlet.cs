using System;
using System.IO.Abstractions;
using System.Linq;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "JournalIndex")]
    [OutputType(typeof(JournalIndex<>))]
    [Alias("gji")]
    public class GetJournalIndexCmdlet : JournalCmdletBase
    {
        [Parameter]
        [ValidateSet("Count", "Name")]
        public string OrderBy { get; set; } = "Count";

        [Parameter]
        [ValidateSet("Ascending", "Descending")]
        public string Direction { get; set; } = "Descending";

        [Parameter]
        public SwitchParameter IncludeBodies { get; set; }

        [Parameter(ParameterSetName = "Range")]
        public DateTime From { get; set; }

        [Parameter(ParameterSetName = "Range")]
        public DateTime To { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var fileSystem = new FileSystem();
            var systemProcess = new SystemProcess();
            var ioFactory = new JournalReaderWriterFactory(fileSystem, Location);
            var markdownFiles = new MarkdownFiles(fileSystem, Location);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);

            if (IncludeBodies)
                WriteResults<CompleteJournalEntry>(journal);
            else
                WriteResults<MetaJournalEntry>(journal);
        }

        private void WriteResults<T>(Journal journal)
            where T : class, IJournalEntry
        {
            var index = journal.CreateIndex<T>();

            switch (OrderBy)
            {
                case var order when order == "Name" && Direction == "Ascending":
                    var ascByName = index.OrderBy(x => x.Tag);
                    WriteObject(ascByName, true);
                    break;

                case var order when order == "Name" && Direction == "Descending":
                    var descByName = index.OrderByDescending(x => x.Tag);
                    WriteObject(descByName, true);
                    break;

                case var order when order == "Count" && Direction == "Descending":
                    var descByCount = index.OrderByDescending(x => x.Entries.Count);
                    WriteObject(descByCount, true);
                    break;

                case var order when order == "Count" && Direction == "Ascending":
                    var ascByCount = index.OrderBy(x => x.Entries.Count);
                    WriteObject(ascByCount, true);
                    break;
            }
        }

    }
}