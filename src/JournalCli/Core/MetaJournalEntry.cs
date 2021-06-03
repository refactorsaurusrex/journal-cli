using System.Collections.Generic;
using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Core
{
    public class MetaJournalEntry : JournalEntryBase
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

        public override string EntryName { get; }
        public override IReadOnlyCollection<string> Tags { get; }
        public override LocalDate EntryDate { get; }
        public IReadOnlyCollection<string> Headers { get; }
        public override IJournalReader GetReader() => _reader;
    }
}