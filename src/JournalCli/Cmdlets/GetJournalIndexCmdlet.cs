using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "JournalIndex")]
    [OutputType(typeof(JournalIndex<>))]
    [Alias("gji")]
    public class GetJournalIndexCmdlet : JournalCmdletBase
    {
        [Parameter]
        [ArgumentCompleter(typeof(TagCompleter))]
        public string[] Tags { get; set; }

        [Parameter]
        public TagOperator TagOperator { get; set; } = TagOperator.Any;

        [Parameter]
        public SwitchParameter ShowCollateralTags { get; set; }

        [Parameter]
        [ValidateSet("Count", "Name")]
        public string OrderBy { get; set; } = "Count";

        [Parameter]
        public SortOrder Direction { get; set; } = SortOrder.Descending;

        [Parameter]
        [NaturalDate(RoundTo.StartOfPeriod)]
        public LocalDate From { get; set; }

        [Parameter]
        [NaturalDate(RoundTo.EndOfPeriod)]
        public LocalDate To { get; set; } = Today.Date();

        protected override void EndProcessing()
        {
            base.EndProcessing();
            var journal = OpenJournal();
            From = LocalDate.Max(From, journal.FirstEntryDate);
            var dateRange = new DateRange(From, To);

            WriteHostInverted($"Searching entries from {From} to {To}");


            var index = TagOperator == TagOperator.All ?
                journal.CreateIndex<CompleteJournalEntry>(dateRange, requiredTags: Tags) :
                journal.CreateIndex<CompleteJournalEntry>(dateRange, optionalTags: Tags);

            IEnumerable<JournalIndexEntry<CompleteJournalEntry>> filteredIndex;
            switch (OrderBy)
            {
                default:
                    throw new InvalidOperationException();

                case var order when order == "Name" && Direction == SortOrder.Ascending:
                    filteredIndex = index.OrderBy(x => x.Tag);
                    break;

                case var order when order == "Name" && Direction == SortOrder.Descending:
                    filteredIndex = index.OrderByDescending(x => x.Tag);
                    break;

                case var order when order == "Count" && Direction == SortOrder.Descending:
                    filteredIndex = index.OrderByDescending(x => x.Entries.Count);
                    break;

                case var order when order == "Count" && Direction == SortOrder.Ascending:
                    filteredIndex = index.OrderBy(x => x.Entries.Count);
                    break;
            }

            switch (TagOperator)
            {
                case TagOperator.Any when ShowCollateralTags:
                    WriteWarning($"The {nameof(ShowCollateralTags)} parameter is ignored when {nameof(TagOperator)} is {nameof(TagOperator.Any)}.");
                    break;
                case TagOperator.All when !ShowCollateralTags:
                    filteredIndex = filteredIndex.Where(x => Tags.Contains(x.Tag));
                    break;
            }

            WriteObject(filteredIndex, true);
        }
    }
}