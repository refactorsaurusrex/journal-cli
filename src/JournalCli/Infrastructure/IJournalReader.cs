using System.Collections.Generic;
using JournalCli.Core;
using NodaTime;

namespace JournalCli.Infrastructure
{
    public interface IJournalReader
    {
        string Body { get; }
        IJournalFrontMatter FrontMatter { get; }
        ICollection<string> Headers { get; }
        string FilePath { get; }
        string EntryName { get; }
        LocalDate EntryDate { get; }
        T ToJournalEntry<T>() where T : class, IJournalEntry;
    }
}