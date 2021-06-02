using System.Collections.Generic;
using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Core
{
    public class ReadmeJournalEntry : IJournalEntry
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
        public string EntryName { get; }
        public IReadOnlyCollection<string> Tags { get; }
        public LocalDate EntryDate { get; }
        public override string ToString() => EntryName;
        public IJournalReader GetReader() => _reader;
    }
}