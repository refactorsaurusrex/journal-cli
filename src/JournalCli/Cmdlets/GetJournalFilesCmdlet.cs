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
    [Cmdlet(VerbsCommon.Get, "JournalFiles")]
    [OutputType(typeof(PSObject[]))]
    public class GetJournalFilesCmdlet : JournalCmdletBase
    {
        [Parameter(ParameterSetName = "Default")]
        [NaturalDate(RoundTo.StartOfPeriod)]
        public LocalDate From { get; set; }

        [Parameter(ParameterSetName = "Default")]
        [NaturalDate(RoundTo.EndOfPeriod)]
        public LocalDate To { get; set; } = Today.Date();

        [Parameter(ParameterSetName = "Default")]
        [ArgumentCompleter(typeof(TagCompleter))]
        public string[] Tags { get; set; }

        [Parameter(ParameterSetName = "Default")]
        public TagOperator TagsOperator { get; set; } = TagOperator.Any;

        [Parameter(ParameterSetName = "Default")]
        public SortOrder Direction { get; set; } = SortOrder.Descending;

        [Parameter(ParameterSetName = "Default")]
        [WildcardInt]
        public int? Limit { get; set; } = 30;
        
        [Parameter(ParameterSetName = "Names", Position = 0, ValueFromPipeline = true)]
        public string[] EntryNames { get; set; }

        protected override void EndProcessing()
        {
            base.EndProcessing();
            var journal = OpenJournal();

            IEnumerable<PSObject> entries;
            if (ParameterSetName == "Names")
            {
                entries = EntryNames.Select(x => PathToPSObject(journal.GetEntryFromName(x).FilePath));
            }
            else
            {
                From = LocalDate.Max(From, journal.FirstEntryDate);
                var dateRange = new DateRange(From, To);
                entries = journal.GetEntries<JournalEntryFile>(Tags, TagsOperator, Direction, dateRange, Limit).Select(x => PathToPSObject(x.FilePath));
            }
            
            WriteObject(entries, true);
        }

        private PSObject PathToPSObject(string path) => InvokeProvider.Item.Get(path).First();
    }
}
