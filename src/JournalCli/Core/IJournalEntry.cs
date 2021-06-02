using System;
using System.Collections.Generic;
using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Core
{
    public interface IJournalEntry
    {
        string EntryName { get; }
        IReadOnlyCollection<string> Tags { get; }
        LocalDate EntryDate { get; }
        string ToString();
        IJournalReader GetReader();
    }

#warning do we want to use this or not?
    public abstract class JournalEntryBase : IJournalEntry, IEquatable<JournalEntryBase>
    {
        public abstract string EntryName { get; }
        public abstract IReadOnlyCollection<string> Tags { get; }
        public abstract LocalDate EntryDate { get; }
        public abstract IJournalReader GetReader();

        public bool Equals(JournalEntryBase other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return EntryName == other.EntryName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == GetType() && Equals((JournalEntryBase)obj);
        }

        public override int GetHashCode() => EntryName != null ? EntryName.GetHashCode() : 0;

        public static bool operator ==(JournalEntryBase left, JournalEntryBase right) => Equals(left, right);

        public static bool operator !=(JournalEntryBase left, JournalEntryBase right) => !Equals(left, right);
    }
}