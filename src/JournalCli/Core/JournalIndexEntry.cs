using System.Collections.Generic;
using JetBrains.Annotations;

namespace JournalCli.Core
{
    [PublicAPI]
    public class JournalIndexEntry<T>
        where T : class, IJournalEntry
    {
        public JournalIndexEntry(string tag, params T[] entries)
        {
            Tag = tag;
            Entries = new List<T>(entries);
        }

        public string Tag { get; }

        public int Count => Entries.Count;

        public ICollection<T> Entries { get; }
    }
}