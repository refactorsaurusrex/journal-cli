using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Core
{
    internal class JournalReader : IJournalReader
    {
        public JournalReader(IFileSystem fileSystem, string filePath)
        {
            FilePath = filePath;
            var lines = fileSystem.File.ReadAllLines(FilePath).ToList();

            var headers = lines.Where(HeaderValidator.IsValid).ToList();
            var h1 = headers.FirstOrDefault()?.Replace("# ", "");

            if (h1 != null && headers.Count > 1 && DateTime.TryParse(h1, out _))
                Headers = lines.Where(x => x.StartsWith("#")).Skip(1).ToList();
            else
                Headers = lines.Where(x => x.StartsWith("#")).ToList();

            var bodyStartIndex = lines.FindLastIndex(s => s == JournalFrontMatter.BlockIndicator) + 1;
            RawBody = string.Join(Environment.NewLine, lines.Skip(bodyStartIndex));
            FrontMatter = JournalFrontMatter.FromFilePath(fileSystem, filePath);
            EntryName = fileSystem.Path.GetFileNameWithoutExtension(FilePath) ?? throw new InvalidOperationException();

            if (!Journal.IsCompiledEntry(EntryName))
                EntryDate = Journal.FileNamePattern.Parse(EntryName).Value;
        }

        public string RawBody { get; }

        public IJournalFrontMatter FrontMatter { get; }

        public IReadOnlyCollection<string> Headers { get; }

        public string FilePath { get; }

        public string EntryName { get; }

        public LocalDate EntryDate { get; }

        public T ToJournalEntry<T>() where T : class, IJournalEntry
        {
            switch (typeof(T))
            {
                default:
                    throw new NotSupportedException($"Unable to create instance of {typeof(T).Name}.");
                case var t when t == typeof(JournalEntryFile):
                    return new JournalEntryFile(this) as T;
                case var t when t == typeof(MetaJournalEntry):
                    return new MetaJournalEntry(this) as T;
                case var t when t == typeof(ReadmeJournalEntry):
                    return new ReadmeJournalEntry(this) as T;
                case var t when t == typeof(CompleteJournalEntry):
                    return new CompleteJournalEntry(this) as T;
            }
        }
    }
}
