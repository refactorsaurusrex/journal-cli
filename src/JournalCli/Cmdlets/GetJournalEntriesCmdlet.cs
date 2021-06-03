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
    [Cmdlet(VerbsCommon.Get, "JournalEntries", DefaultParameterSetName = "Default")]
    [OutputType(typeof(IEnumerable<MetaJournalEntry>))]
    [OutputType(typeof(MetaJournalEntry))]
    [OutputType(typeof(ReadmeJournalEntryCollection))]
    [Alias("gje")]
    public class GetJournalEntriesCmdlet : JournalCmdletBase
    {
        [Parameter(ParameterSetName = "Default")]
        [Parameter(ParameterSetName = "ReadMe")]
        [ArgumentCompleter(typeof(TagCompleter))]
        public string[] Tags { get; set; }

        [Parameter(ParameterSetName = "Default")]
        [Parameter(ParameterSetName = "ReadMe")]
        public TagOperator TagOperator { get; set; } = TagOperator.Any;

        [Parameter(ParameterSetName = "Default")]
        [Parameter(ParameterSetName = "ReadMe")]
        public SortOrder Direction { get; set; } = SortOrder.Descending;

        [Parameter(ParameterSetName = "Default")]
        [Parameter(ParameterSetName = "ReadMe")]
        [NaturalDate(RoundTo.StartOfPeriod)]
        public LocalDate From { get; set; }

        [Parameter(ParameterSetName = "Default")]
        [Parameter(ParameterSetName = "ReadMe")]
        [NaturalDate(RoundTo.EndOfPeriod)]
        public LocalDate To { get; set; } = Today.Date();

        [Parameter(ParameterSetName = "Default")]
        [Parameter(ParameterSetName = "ReadMe")]
        [WildcardInt]
        public int? Limit { get; set; } = 30;

        [Parameter(ParameterSetName = "ReadMe")]
        public SwitchParameter ShowReadMeEntries { get; set; }

        [Parameter(ParameterSetName = "ReadMe")]
        public SwitchParameter IncludeFutureReadMeEntries { get; set; }

        [Parameter(ParameterSetName = "Random")]
        public SwitchParameter RandomEntry { get; set; }

        protected override void EndProcessing()
        {
            base.EndProcessing();
            var journal = OpenJournal();
            From = LocalDate.Max(From, journal.FirstEntryDate);
            var dateRange = new DateRange(From, To);

            if (RandomEntry)
            {
                WriteHostInverted($"Random entry selected from {From} to {To}");
                var random = journal.GetRandomEntry(Tags, TagOperator, dateRange);
                WriteObject(random);
                return;
            }

            if (ShowReadMeEntries)
            {
                if (IncludeFutureReadMeEntries)
                {
                    if (!YesOrNo("You really want to open your future time capsules?", ConsoleColor.Red, Console.BackgroundColor))
                        IncludeFutureReadMeEntries = false;
                }

                WriteHostInverted($"Getting the last {Limit} ReadMe entries from {From} to {To}");
                var readMeEntries = journal.GetReadmeEntries(From, IncludeFutureReadMeEntries);
                if (Limit.HasValue)
                    WriteObject(readMeEntries.Take(Limit.Value));
                else
                    WriteObject(readMeEntries, true);

                return;
            }

            if (IncludeFutureReadMeEntries)
                WriteWarning($"'{nameof(IncludeFutureReadMeEntries)}' is only valid when the '{nameof(ShowReadMeEntries)}' parameter is set.");

            WriteHostInverted($"Getting the last {Limit} entries from {From} to {To}");

            var entries = journal.GetEntries<MetaJournalEntry>(Tags, TagOperator, Direction, dateRange, Limit);
            WriteObject(entries, true);
        }
    }
}