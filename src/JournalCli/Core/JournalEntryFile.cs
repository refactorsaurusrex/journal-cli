using System;
using System.Collections.Generic;
using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Core
{
    public class JournalEntryFile : JournalEntryBase, IComparable<JournalEntryFile>
    {
        private readonly IJournalReader _reader;

        public JournalEntryFile(IJournalReader reader)
        {
            _reader = reader;
            EntryDate = reader.EntryDate;
            FilePath = reader.FilePath;
            Body = reader.RawBody;
            Tags = reader.FrontMatter.Tags;
            EntryName = reader.EntryName;
        }

        public override LocalDate EntryDate { get; }

        public string FilePath { get; }

        public string Body { get; }

        public override string EntryName { get; }

        public override IReadOnlyCollection<string> Tags { get; }

        public override IJournalReader GetReader() => _reader;

        public int CompareTo(JournalEntryFile other)
        {
            if (ReferenceEquals(this, other)) return 0;
            return ReferenceEquals(null, other) ? 1 : EntryDate.CompareTo(other.EntryDate);
        }
    }
}