using JournalCli.Infrastructure;

namespace JournalCli.Core
{
    public class ReadmeJournalEntry : JournalEntry
    {
        public ReadmeJournalEntry(IJournalReader journalReader)
            : base(journalReader)
        {
            ReadmeDate = journalReader.FrontMatter.Readme;
        }

        public string ReadmeDate { get; }
    }
}