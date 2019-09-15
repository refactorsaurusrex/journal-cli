using System.Collections.Generic;
using JournalCli.Infrastructure;

namespace JournalCli.Core
{
    public class JournalEntry
    {
        public JournalEntry(IJournalReader journalReader)
        {
            FilePath = journalReader.FilePath;
            Tags = journalReader.FrontMatter.Tags;
            EntryName = journalReader.EntryName;
        }

        public string FilePath { get; }

        public string EntryName { get; }

        public ICollection<string> Tags { get; }

        public override string ToString() => EntryName;
    }
}