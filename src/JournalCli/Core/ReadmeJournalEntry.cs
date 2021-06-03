using System.Collections.Generic;
using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Core
{
    public class ReadmeJournalEntry : JournalEntryBase
    {
        private readonly IJournalReader _reader;

        public ReadmeJournalEntry(IJournalReader reader)
        {
            _reader = reader;
            // ReSharper disable once PossibleInvalidOperationException
            ReadmeDate = reader.FrontMatter.ReadmeDate.Value;
            Headers = reader.Headers;
            Tags = reader.FrontMatter.Tags;
            EntryName = reader.EntryName;
            EntryDate = reader.EntryDate;
        }

        public LocalDate ReadmeDate { get; }
        public IReadOnlyCollection<string> Headers { get; }
        public override string EntryName { get; }
        public override IReadOnlyCollection<string> Tags { get; }
        public override LocalDate EntryDate { get; }
        public override IJournalReader GetReader() => _reader;
    }
}