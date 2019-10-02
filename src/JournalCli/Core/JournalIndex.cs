using System.Collections.ObjectModel;

namespace JournalCli.Core
{
    public class JournalIndex<T> : KeyedCollection<string, JournalIndexEntry<T>>
        where T : class, IJournalEntry
    {
        protected override string GetKeyForItem(JournalIndexEntry<T> item) => item.Tag;
    }
}