using System;
using System.Management.Automation;
using JetBrains.Annotations;

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
        public DateTime? To { get; set; }

        [Parameter]
        public string[] Tags { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            var range = GetRangeOrThrow(From, To);
            var journal = OpenJournal();

            journal.OpenRandomEntry(Tags, range);
        }
    }
}