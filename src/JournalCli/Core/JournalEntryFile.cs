using System;
using System.Collections.Generic;
using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Core
{
    public class JournalEntryFile : IJournalEntry, IComparable<JournalEntryFile>, IEquatable<JournalEntryFile>
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

        public LocalDate EntryDate { get; }
        public string FilePath { get; }
        public string Body { get; }
        public string EntryName { get; }
        public IReadOnlyCollection<string> Tags { get; }
        public override string ToString() => EntryName;
        public IJournalReader GetReader() => _reader;

        public int CompareTo(JournalEntryFile other)
        {
            if (ReferenceEquals(this, other)) return 0;
            return ReferenceEquals(null, other) ? 1 : EntryDate.CompareTo(other.EntryDate);
        }

        public bool Equals(JournalEntryFile other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return EntryDate.Equals(other.EntryDate) && string.Equals(EntryName, other.EntryName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((JournalEntryFile)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EntryDate.GetHashCode() * 397) ^ EntryName.GetHashCode();
            }
        }
    }
}