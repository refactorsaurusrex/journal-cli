using System;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Open, "RandomJournalEntry")]
    [Alias("orj")]
    public class OpenRandomJournalEntryCmdlet : JournalCmdletBase
    {
        [Parameter]
        public DateTime? From { get; set; }

        [Parameter]
        public DateTime To { get; set; } = DateTime.Now;

        [Parameter(ParameterSetName = "Year", Mandatory = true)]
        public int Year { get; set; }

        [Parameter]
        public string[] Tags { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var range = ParameterSetName == "Year" ? new DateRange(new LocalDate(Year, 1, 1), new LocalDate(Year, 12, 31)) : GetRangeOrNull(From, To);
            var journal = OpenJournal();

            journal.OpenRandomEntry(Tags, range);
        }
    }
}