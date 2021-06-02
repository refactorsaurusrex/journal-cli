using System.Collections.Generic;
using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Core
{
    public class MetaJournalEntry : IJournalEntry
    {
        private readonly IJournalReader _reader;

        public MetaJournalEntry(IJournalReader reader)
        {
            _reader = reader;
            Headers = reader.Headers;
            Tags = reader.FrontMatter.Tags;
            EntryName = reader.EntryName;
            EntryDate = reader.EntryDate;
        }

        public string EntryName { get; }
        public IReadOnlyCollection<string> Tags { get; }
        public LocalDate EntryDate { get; }
        public IReadOnlyCollection<string> Headers { get; }
        public override string ToString() => EntryName;
        public IJournalReader GetReader() => _reader;
    }
}