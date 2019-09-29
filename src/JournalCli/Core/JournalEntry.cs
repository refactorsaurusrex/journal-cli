using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using JournalCli.Infrastructure;
using NodaTime.Text;

namespace JournalCli.Core
{
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public class JournalEntry
    {
        public static LocalDatePattern FileNamePattern { get; } = LocalDatePattern.CreateWithCurrentCulture("yyyy.MM.dd");
        public static LocalDatePattern MonthDirectoryPattern { get; } = LocalDatePattern.CreateWithCurrentCulture("MM MMMM");
        public static LocalDatePattern YearDirectoryPattern { get; } = LocalDatePattern.CreateWithCurrentCulture("yyyy");

        public JournalEntry(IJournalReader journalReader)
        {
            FilePath = journalReader.FilePath;
            Tags = journalReader.FrontMatter.Tags;
            EntryName = journalReader.EntryName;
        }

        public string FilePath { get; }

        public string EntryName { get; }

        public ICollection<string> Tags { get; }

        public override string ToString() => EntryName;
    }
}