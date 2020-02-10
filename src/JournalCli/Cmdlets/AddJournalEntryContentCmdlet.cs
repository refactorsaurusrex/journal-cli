using System;
using System.Linq;
using System.Management.Automation;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Alias("aje")]
    [Cmdlet(VerbsCommon.Add, "JournalEntryContent")]
    public class AddJournalEntryContentCmdlet : JournalCmdletBase
    {
        [Parameter]
        public DateTime Date { get; set; } = DateTime.Now;

        [Parameter]
        public int DateOffset { get; set; }

        [Parameter]
        public string Header { get; set; }

        [Parameter(Position = 0)]
        public string[] Body { get; set; }

        [Parameter(Position = 1)]
        public string[] Tags { get; set; }

        protected override void RunJournalCommand()
        {
            if (!string.IsNullOrWhiteSpace(Header))
                HeaderValidator.ValidateOrThrow(Header);

            if (!string.IsNullOrWhiteSpace(Header) && (Body == null || !Body.Any()))
                throw new PSArgumentException("Header cannot be used without Body. Please specify a Body and try again.");

            var entryDate = LocalDate.FromDateTime(Date).PlusDays(DateOffset);
            var hour = Now.Time().Hour;

            if (hour >= 0 && hour <= 4)
            {
                var dayPrior = entryDate.Minus(Period.FromDays(1));
                var question = $"Edit entry for '{dayPrior}' or '{entryDate}'?";
                var result = Choice("It's after midnight!", question, 0, dayPrior.DayOfWeek.ToChoiceString(), entryDate.DayOfWeek.ToChoiceString());
                if (result == 0)
                    entryDate = dayPrior;
            }

            Commit(GitCommitType.PreAppendJournalEntry);
            var journal = OpenJournal();
            journal.AppendEntryContent(entryDate, Body, Header, Tags);
            Commit(GitCommitType.PostAppendJournalEntry);
        }
    }
}
