using System.Collections.ObjectModel;

namespace JournalCli.Core
{
    public class JournalIndex : KeyedCollection<string, JournalIndexEntry>
    {
        protected override string GetKeyForItem(JournalIndexEntry item) => item.Tag;
    }
}