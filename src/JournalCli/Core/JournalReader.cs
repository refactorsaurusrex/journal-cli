using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using JournalCli.Infrastructure;
using NodaTime;
using NodaTime.Text;

namespace JournalCli.Core
{
    internal class JournalReader : IJournalReader
    {
        public JournalReader(IFileSystem fileSystem, string filePath)
        {
            FilePath = filePath;
            var lines = fileSystem.File.ReadAllLines(FilePath).ToList();
            Headers = lines.Where(x => x.StartsWith("#")).ToList();
            var bodyStartIndex = lines.FindLastIndex(s => s == JournalFrontMatter.BlockIndicator) + 1;
            Body = string.Join(Environment.NewLine, lines.Skip(bodyStartIndex));
            FrontMatter = JournalFrontMatter.FromFilePath(fileSystem, filePath);
            EntryName = fileSystem.Path.GetFileNameWithoutExtension(FilePath) ?? throw new InvalidOperationException();
            EntryDate = LocalDatePattern.CreateWithCurrentCulture("yyyy.MM.dd").Parse(EntryName).Value;
        }

        public string Body { get; }

        public IJournalFrontMatter FrontMatter { get; }

        public ICollection<string> Headers { get; }

        public string FilePath { get; }

        public string EntryName { get; }

        public LocalDate EntryDate { get; }
    }
}
