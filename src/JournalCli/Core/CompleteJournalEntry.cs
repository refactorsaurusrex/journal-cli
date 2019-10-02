﻿using System.Collections.Generic;
using JournalCli.Infrastructure;

namespace JournalCli.Core
{
    public class CompleteJournalEntry : IJournalEntry
    {
        public CompleteJournalEntry(IJournalReader reader)
        {
            Body = reader.Body.Trim();
            Tags = reader.FrontMatter.Tags;
            EntryName = reader.EntryName;
        }

        public string EntryName { get; }
        public ICollection<string> Tags { get; }
        public override string ToString() => EntryName;
        public string Body { get; }
    }
}