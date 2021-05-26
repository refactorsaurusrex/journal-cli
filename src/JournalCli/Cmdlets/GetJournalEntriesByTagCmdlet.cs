using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "JournalEntriesByTag")]
    [OutputType(typeof(IEnumerable<JournalIndexEntry<MetaJournalEntry>>))]
    [OutputType(typeof(IEnumerable<JournalIndexEntry<CompleteJournalEntry>>))]
    [Alias("gjt")]
    // TODO: Rename to Get-JournalEntries
    public class GetJournalEntriesByTagCmdlet : JournalCmdletBase
    {
        [Parameter(Mandatory = true)]
        public string[] Tags { get; set; }

        [Parameter]
        public SwitchParameter IncludeBodies { get; set; }

        [Parameter]
        public SwitchParameter All { get; set; }

        [Parameter]
        public DateTime? From { get; set; }

        [Parameter]
        public DateTime To { get; set; } = DateTime.Now;

        protected override void EndProcessing()
        {
            base.EndProcessing();

            var dateRange = GetRangeOrNull(From, To);
            var journal = OpenJournal();

            switch (All.ToBool())
            {
                case true when IncludeBodies:
                    WriteAllTagResults<CompleteJournalEntry>(journal, dateRange);
                    break;
                case true when !IncludeBodies:
                    WriteAllTagResults<MetaJournalEntry>(journal, dateRange);
                    break;
                case false when IncludeBodies:
                    WriteAnyTagResults<CompleteJournalEntry>(journal, dateRange);
                    break;
                case false when !IncludeBodies:
                    WriteAnyTagResults<MetaJournalEntry>(journal, dateRange);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void WriteAllTagResults<T>(Journal journal, DateRange dateRange)
            where T : class, IJournalEntry
        {
            var allIndex = journal.CreateIndex<T>(dateRange, Tags).OrderByDescending(x => x.Entries.Count);
            WriteObject(allIndex, true);
        }

        private void WriteAnyTagResults<T>(Journal journal, DateRange dateRange)
            where T : class, IJournalEntry
        {
            var anyIndex = journal.CreateIndex<T>(dateRange);
            var result = anyIndex.Where(x => Tags.Contains(x.Tag)).OrderByDescending(x => x.Entries.Count);
            WriteObject(result, true);
        }
    }
}