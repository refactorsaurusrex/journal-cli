using System.Collections.Generic;
using JournalCli.Infrastructure;

namespace JournalCli.Core
{
    public interface IJournalEntry
    {
        string EntryName { get; }
        ICollection<string> Tags { get; }
        string ToString();
        IJournalReader GetReader();
    }
}