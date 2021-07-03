using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.New, "JournalEntry")]
    [Alias("nj")]
    public class NewJournalEntryCmdlet : JournalCmdletBase
    {
        [Parameter]
        [ArgumentCompleter(typeof(TagCompleter))]
        public string[] Tags { get; set; }

        [Parameter]
        [ValidateReadme]
        public string Readme { get; set; }

        [Parameter]
        [NaturalDate(RoundTo.StartOfPeriod)]
        public LocalDate Date { get; set; } = Today.Date();

        protected override void EndProcessing()
        {
            base.EndProcessing();

            var journal = OpenJournal();

            var hour = Now.Time().Hour;
            if (hour >= 0 && hour <= 4)
            {
                var dayPrior = Date.Minus(Period.FromDays(1));
                var question = $"Did you mean to create an entry for '{dayPrior}' or '{Date}'?";
                var result = Choice("It's after midnight!", question, 0, dayPrior.DayOfWeek.ToChoiceString(), Date.DayOfWeek.ToChoiceString());
                if (result == 0)
                    Date = dayPrior;
            }

            try
            {
                journal.CreateNewEntry(Date, Tags, Readme);
                new JournalTagCache().Invalidate();
            }
            catch (JournalEntryAlreadyExistsException e)
            {
                var question = $"An entry for {Date} already exists. Do you want to open it instead?";
                if (YesOrNo(question))
                {
                    SystemProcess.Start(e.EntryFilePath);
                }
            }
        }
    }
}