using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using JournalCli.Infrastructure;

namespace JournalCli.Core
{
    public class CompleteJournalEntry : IJournalEntry
    {
        private readonly IJournalReader _reader;

        public CompleteJournalEntry(IJournalReader reader)
        {
            _reader = reader;
            WrapBody(reader.Body);
            Tags = reader.FrontMatter.Tags;
            EntryName = reader.EntryName;
        }

        public string EntryName { get; }
        public ICollection<string> Tags { get; }
        public override string ToString() => EntryName;
        public string Body { get; private set; }
        public IJournalReader GetReader() => _reader;
        private void WrapBody(string body)
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
                    wrapped.AppendLine(line.Wrap());
                }
                else
                {
                    wrapped.AppendLine(line.Wrap());
                    wrapped.AppendLine();
                }
            }

            Body = wrapped.ToString();
        }
    }
}