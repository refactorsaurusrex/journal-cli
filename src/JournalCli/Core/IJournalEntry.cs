using System.Collections.Generic;
using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Core
{
    public interface IJournalEntry
    {
        string EntryName { get; }
        IReadOnlyCollection<string> Tags { get; }
        LocalDate EntryDate { get; }
        string ToString();
        IJournalReader GetReader();
    }
}