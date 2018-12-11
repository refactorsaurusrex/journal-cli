using System.Collections.ObjectModel;

namespace JournalCli
{
    public class JournalIndex : KeyedCollection<string, JournalIndexEntry>
    {
        protected override string GetKeyForItem(JournalIndexEntry item) => item.Tag;
    }
}