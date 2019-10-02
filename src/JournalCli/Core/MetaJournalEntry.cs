using System.Collections.Generic;
using JournalCli.Infrastructure;

namespace JournalCli.Core
{
    public class MetaJournalEntry : IJournalEntry
    {
        public MetaJournalEntry(IJournalReader reader)
        {
            Headers = reader.Headers;
            Tags = reader.FrontMatter.Tags;
            EntryName = reader.EntryName;
        }

        public string EntryName { get; }
        public ICollection<string> Tags { get; }
        public ICollection<string> Headers { get; }
        public override string ToString() => EntryName;
    }
}