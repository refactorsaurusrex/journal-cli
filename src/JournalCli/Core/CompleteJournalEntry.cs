using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Core
{
    public class CompleteJournalEntry : IJournalEntry, IEquatable<CompleteJournalEntry>
    {
        private readonly IJournalReader _reader;

        public CompleteJournalEntry(IJournalReader reader, int bodyWrap)
        {
            _reader = reader;
            Body = WrapBody(reader.RawBody, bodyWrap);
            Tags = reader.FrontMatter.Tags;
            EntryName = reader.EntryName;
            EntryDate = reader.EntryDate;
            ReadMeDate = reader.FrontMatter.ReadmeDate.HasValue ? 
                reader.FrontMatter.ReadmeDate.Value.ToString() : 
                "None";
        }

        public string ReadMeDate { get; }

        public bool IsReadMe() => ReadMeDate != "None" && !string.IsNullOrWhiteSpace(ReadMeDate);

        public string EntryName { get; }

        public IReadOnlyCollection<string> Tags { get; }

        public LocalDate EntryDate { get; }

        public override string ToString() => EntryName;

        public string Body { get; }

        public IJournalReader GetReader() => _reader;

        private string WrapBody(string body, int bodyWrap)
        {
            var lines = body.Trim().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var wrapped = new StringBuilder();

            foreach (var line in lines)
            {
                if (line.StartsWith("#"))
                {
                    wrapped.AppendLine(line);
                    wrapped.AppendLine();
                }
                else if (!Regex.IsMatch(line, @"^[A-Za-z]"))
                {
                    wrapped.AppendLine(line.Wrap(bodyWrap));
                }
                else
                {
                    wrapped.AppendLine(line.Wrap(bodyWrap));
                    wrapped.AppendLine();
                }
            }

            return wrapped.ToString();
        }

        public bool Equals(CompleteJournalEntry other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return EntryName == other.EntryName && Body == other.Body;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;

            return obj.GetType() == GetType() && Equals((CompleteJournalEntry)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((EntryName != null ? EntryName.GetHashCode() : 0) * 397) ^ (Body != null ? Body.GetHashCode() : 0);
            }
        }

        public static bool operator ==(CompleteJournalEntry left, CompleteJournalEntry right) => Equals(left, right);

        public static bool operator !=(CompleteJournalEntry left, CompleteJournalEntry right) => !Equals(left, right);
    }
}