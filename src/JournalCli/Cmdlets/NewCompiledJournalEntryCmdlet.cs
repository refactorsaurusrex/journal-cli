using System.Linq;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.New, "CompiledJournalEntry", DefaultParameterSetName = "Default")]
    public class NewCompiledJournalEntryCmdlet : JournalCmdletBase
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
        public TagOperator TagOperator { get; set; } = TagOperator.Any;

        [Parameter(Mandatory = true, ParameterSetName = "Entries", ValueFromPipeline = true)]
        public PSObject[] Entries { get; set; }

        public SwitchParameter Force { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var journal = OpenJournal();
            From = LocalDate.Max(From, journal.FirstEntryDate);

            switch (ParameterSetName)
            {
                default:
                    RunDefault(journal);
                    break;
                case "Entries":
                    RunEntries(journal);
                    break;
            }
        }

        private void RunEntries(Journal journal)
        {
            if (Entries.Length < 2)
            {
                const string message = "You cannot create a compiled entry from just one normal entry. Please create an array of at least " + 
                    "two journal entries and try again. If you're attempting to pipe entries into this cmdlet, you might need to use the PowerShell " + 
                    "array operator (,). Refer to the help documentation for examples.";
                ThrowTerminatingError(message, ErrorCategory.InvalidOperation);
            }

            var entries = Entries.Select(x => x.BaseObject).Cast<IJournalEntry>().ToList();
            journal.CreateCompiledEntry(entries, Force);
        }

        private void RunDefault(Journal journal)
        {
            var range = new DateRange(From, To);

            try
            {
                journal.CreateCompiledEntry(range, Tags, TagOperator, Force);
            }
            catch (JournalEntryAlreadyExistsException e)
            {
                const string message = "A compiled journal entry already exists based on the provided parameters. To overwrite it, " +
                    "run again with the -Force switch.";
                throw new JournalEntryAlreadyExistsException(e.EntryFilePath, message);
            }
        }
    }
}
