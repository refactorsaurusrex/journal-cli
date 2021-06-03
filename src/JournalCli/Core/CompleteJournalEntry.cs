using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Core
{
    public class CompleteJournalEntry : JournalEntryBase
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

        public override string EntryName { get; }

        public override IReadOnlyCollection<string> Tags { get; }

        public override LocalDate EntryDate { get; }

        public string Body { get; }

        public override IJournalReader GetReader() => _reader;

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
    }
}