using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Core
{
    public class ReadmeJournalEntry : JournalEntry
    {
        public ReadmeJournalEntry(IJournalReader journalReader)
            : base(journalReader)
        {
            // ReSharper disable once PossibleInvalidOperationException
            ReadmeDate = journalReader.FrontMatter.ReadmeDate.Value;
        }

        public LocalDate ReadmeDate { get; }
    }
}