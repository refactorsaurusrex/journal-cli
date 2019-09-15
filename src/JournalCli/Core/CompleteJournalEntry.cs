using System.Collections.Generic;
using JournalCli.Infrastructure;

namespace JournalCli.Core
{
    public class CompleteJournalEntry : JournalEntry
    {
        public CompleteJournalEntry(IJournalReader journalReader)
            : base(journalReader)
        {
            Headers = journalReader.Headers;
            Body = journalReader.Body;
        }

        public ICollection<string> Headers { get; }

        public string Body { get; }
    }
}