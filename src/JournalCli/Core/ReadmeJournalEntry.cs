using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JournalCli.Infrastructure;
using NodaTime;
using NodaTime.Text;

namespace JournalCli.Core
{
    public class ReadmeJournalEntry : IJournalEntry
    {
        public ReadmeJournalEntry(IJournalReader reader)
        {
            // ReSharper disable once PossibleInvalidOperationException
            ReadmeDate = reader.FrontMatter.ReadmeDate.Value;
            Headers = reader.Headers;
            Tags = reader.FrontMatter.Tags;
            EntryName = reader.EntryName;
        }

        public LocalDate ReadmeDate { get; }
        public ICollection<string> Headers { get; }
        public string EntryName { get; }
        public ICollection<string> Tags { get; }
        public override string ToString() => EntryName;
    }
}