using System;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "JournalReadmeEntries", DefaultParameterSetName = "All")]
    [OutputType(typeof(ReadmeJournalEntryCollection))]
    [Alias("Get-ReadmeEntries")]
    public class GetJournalReadmeEntriesCmdlet : JournalCmdletBase
    {
        [Parameter(DontShow = true)]
        public SwitchParameter IncludeFuture { get; set; }

        [Parameter(ParameterSetName = "Range")]
        [ValidateSet("Years", "Months", "Days")]
        public string Period { get; set; }

        [Parameter(ParameterSetName = "Range")]
        public int Duration { get; set; }

        [Parameter(ParameterSetName = "All")]
        public SwitchParameter All { get; set; }

        protected override void RunJournalCommand()
        {
            if (MyInvocation.InvocationName == "Get-ReadmeEntries")
                WriteWarning("'Get-ReadmeEntries' is obsolete and will be removed in a future release. Use 'Get-JournalReadmeEntries' instead.");

            if (ParameterSetName == "Range")
            {
                if (Duration == 0)
                {
                    const string message = "Zero is not a valid value when specifying a range. Try again with a Duration value of 1 or larger.";
                    throw new PSArgumentOutOfRangeException(nameof(Duration), Duration, message);
                }

                if (Duration < 0)
                {
                    const string message = "If that was an attempt to look at FUTURE readme entries, nice try. ;) Although I discourage " +
                                           "looking at readme entries before they've expired (doing so seems to violate the spirit of writing a note " +
                                           "to your future self), I built a special switch for this use case. It's undocumented, but if you include " +
                                           "the -IncludeFuture switch, you'll get what you're after. Have fun!";
                    WriteWarning(message);
                    throw new PSArgumentOutOfRangeException(nameof(Duration), Duration, "Duration must be greater than or equal to 1.");
                }
            }

            var journal = OpenJournal();
            var readMeEntries = journal.GetReadmeEntries(EarliestDate, IncludeFuture);
            WriteObject(readMeEntries, true);
        }

        private NodaTime.LocalDate EarliestDate
        {
            get
            {
                if (All || ParameterSetName == "All")
                    return new NodaTime.LocalDate();

                switch (Period)
                {
                    case "Days":
                        return Today.MinusDays(Duration);
                    case "Months":
                        return Today.MinusMonths(Duration);
                    case "Years":
                        return Today.MinusYears(Duration);
                    default:
                        throw new NotSupportedException();
                }
            }
        }
    }
}