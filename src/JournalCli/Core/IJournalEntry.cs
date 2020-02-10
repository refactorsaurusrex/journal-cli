using System.Collections.Generic;
using JournalCli.Infrastructure;

namespace JournalCli.Core
{
    public interface IJournalEntry
    {
        string EntryName { get; }
        IReadOnlyCollection<string> Tags { get; }
        string ToString();
        IJournalReader GetReader();
    }
}