using System.Collections.Generic;
using JournalCli.Infrastructure;

namespace JournalCli.Core
{
    public class JournalEntryFile : IJournalEntry
    {
        public JournalEntryFile(IJournalReader reader)
        {
            FilePath = reader.FilePath;
            Body = reader.Body;
            Tags = reader.FrontMatter.Tags;
            EntryName = reader.EntryName;
        }

        public string FilePath { get; }
        public string Body { get; }
        public string EntryName { get; }
        public ICollection<string> Tags { get; }
        public override string ToString() => EntryName;
    }
}