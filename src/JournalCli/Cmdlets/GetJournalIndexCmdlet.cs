using System;
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

        [Parameter]
        public DateTime? From { get; set; }

        [Parameter]
        public DateTime To { get; set; } = DateTime.Now;

        protected override void RunJournalCommand()
        {
            var dateRange = GetRangeOrNull(From, To);
            var journal = OpenJournal();

            if (IncludeBodies)
                WriteResults<CompleteJournalEntry>(journal, dateRange);
            else
                WriteResults<MetaJournalEntry>(journal, dateRange);
        }

        private void WriteResults<T>(Journal journal, DateRange dateRange)
            where T : class, IJournalEntry
        {
            var index = journal.CreateIndex<T>(dateRange);

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