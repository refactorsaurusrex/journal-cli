using System.Linq;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Alias("aje")]
    [Cmdlet(VerbsCommon.Add, "JournalContent")]
    public class AddJournalContentCmdlet : JournalCmdletBase
    {
        [Parameter]
        [NaturalDate(RoundTo.StartOfPeriod)]
        public LocalDate Date { get; set; } = Today.Date();

        [Parameter]
        public string Header { get; set; }

        [Parameter(Position = 0)]
        public string[] Body { get; set; }

        [Parameter(Position = 1)]
        public string[] Tags { get; set; }

        protected override void EndProcessing()
        {
            base.EndProcessing();

            if (!string.IsNullOrWhiteSpace(Header))
                HeaderValidator.ValidateOrThrow(Header);

            if (!string.IsNullOrWhiteSpace(Header) && (Body == null || !Body.Any()))
                throw new PSArgumentException("Header cannot be used without Body. Please specify a Body and try again.");

            var hour = Now.Time().Hour;

            if (hour >= 0 && hour <= 4)
            {
                var dayPrior = Date.Minus(Period.FromDays(1));
                var question = $"Edit entry for '{dayPrior}' or '{Date}'?";
                var result = Choice("It's after midnight!", question, 0, dayPrior.DayOfWeek.ToChoiceString(), Date.DayOfWeek.ToChoiceString());
                if (result == 0)
                    Date = dayPrior;
            }

            var journal = OpenJournal();
            journal.AppendEntryContent(Date, Body, Header, Tags);
        }
    }
}
