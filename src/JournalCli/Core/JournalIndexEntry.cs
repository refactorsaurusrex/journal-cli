using System.Collections.Generic;
using JetBrains.Annotations;

namespace JournalCli.Core
{
    [PublicAPI]
    public class JournalIndexEntry
    {
        public JournalIndexEntry(string tag, params JournalEntry[] entries)
        {
            Tag = tag;
            Entries = new List<JournalEntry>(entries);
        }

        public string Tag { get; }

        public int Count => Entries.Count;

        public ICollection<JournalEntry> Entries { get; }
    }
}