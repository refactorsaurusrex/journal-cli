using System.Linq;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "JournalFiles")]
    [OutputType(typeof(PSObject[]))]
    public class GetJournalFilesCmdlet : JournalCmdletBase
    {
        [Parameter]
        [NaturalDate(RoundTo.StartOfPeriod)]
        public LocalDate From { get; set; }

        [Parameter]
        [NaturalDate(RoundTo.EndOfPeriod)]
        public LocalDate To { get; set; } = Today.Date();

        [Parameter]
        public string[] Tags { get; set; }

        [Parameter]
        public TagOperator TagsOperator { get; set; } = TagOperator.Any;

        [Parameter]
        public SortOrder Direction { get; set; } = SortOrder.Descending;

        [Parameter]
        [WildcardInt]
        public int? Limit { get; set; } = 30;

        protected override void EndProcessing()
        {
            base.EndProcessing();

            var journal = OpenJournal();
            From = LocalDate.Max(From, journal.FirstEntryDate);
            var dateRange = new DateRange(From, To);

            var entries = journal.GetEntries<JournalEntryFile>(Tags, TagsOperator, Direction, dateRange, Limit).Select(x => PathToPSObject(x.FilePath));
            WriteObject(entries, true);
        }

        private PSObject PathToPSObject(string path) => InvokeProvider.Item.Get(path).First();
    }
}
