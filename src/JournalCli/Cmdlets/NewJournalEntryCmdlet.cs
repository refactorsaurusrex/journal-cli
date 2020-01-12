using System;
using System.Management.Automation;
using JetBrains.Annotations;
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
        public int DateOffset { get; set; }

        [Parameter]
        public string[] Tags { get; set; }

        [Parameter]
        public string Readme { get; set; }

        [Parameter]
        public DateTime? Date { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var journal = OpenJournal();
            var entryDate = Date == null ? Today.PlusDays(DateOffset) : LocalDate.FromDateTime(Date.Value).PlusDays(DateOffset);

            var hour = Now.Time().Hour;
            if (hour >= 0 && hour <= 4)
            {
                var dayPrior = entryDate.Minus(Period.FromDays(1));
                var question = $"Did you mean to create an entry for '{dayPrior}' or '{entryDate}'?";
                var result = Choice("It's after midnight!", question, 0, dayPrior.DayOfWeek.ToChoiceString(), entryDate.DayOfWeek.ToChoiceString());
                if (result == 0)
                    entryDate = dayPrior;
            }

            Commit(GitCommitType.PreNewJournalEntry);

            try
            {
                journal.CreateNewEntry(entryDate, Tags, Readme);
                Commit(GitCommitType.PostNewJournalEntry);
            }
            catch (JournalEntryAlreadyExistsException e)
            {
                var question = $"An entry for {entryDate} already exists. Do you want to open it instead?";
                if (YesOrNo(question))
                {
                    SystemProcess.Start(e.EntryFilePath);
                }
            }
        }
    }
}