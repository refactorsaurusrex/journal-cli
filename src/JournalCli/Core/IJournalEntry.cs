using System.Collections.Generic;

namespace JournalCli.Core
{
    public interface IJournalEntry
    {
        string EntryName { get; }
        ICollection<string> Tags { get; }
        string ToString();
    }
}