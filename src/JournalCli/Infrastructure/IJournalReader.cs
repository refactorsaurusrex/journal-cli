using System.Collections.Generic;
using JournalCli.Core;
using NodaTime;

namespace JournalCli.Infrastructure
{
    public interface IJournalReader
    {
        string RawBody { get; }
        IJournalFrontMatter FrontMatter { get; }
        IReadOnlyCollection<string> Headers { get; }
        string FilePath { get; }
        string EntryName { get; }
        LocalDate EntryDate { get; }
        T ToJournalEntry<T>() where T : class, IJournalEntry;
    }
}