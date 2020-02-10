using System;
using System.IO.Abstractions;
using System.Linq;
using JournalCli.Core;
using NodaTime;

namespace JournalCli.Infrastructure
{
    internal class JournalWriter : IJournalWriter
    {
        private readonly IFileSystem _fileSystem;
        private readonly string _rootDirectory;

        public JournalWriter(IFileSystem fileSystem, string rootDirectory)
        {
            _fileSystem = fileSystem;
            _rootDirectory = rootDirectory;
        }

        public void Create(string filePath, IJournalFrontMatter journalFrontMatter, string content)
        {
            using (var fs = _fileSystem.File.CreateText(filePath))
            {
                fs.Write(journalFrontMatter.ToString(asFrontMatter: true));
                fs.WriteLine(content);
                fs.Flush();
            }
        }

        public void CreateCompiled(IJournalFrontMatter journalFrontMatter, string filePath, string content)
        {
            using (var fs = _fileSystem.File.CreateText(filePath))
            {
                fs.Write(journalFrontMatter.ToString(asFrontMatter: true));
                fs.WriteLine(content);
                fs.Flush();
            }
        }

        public void RenameTag(IJournalReader journalReader, string oldTag, string newTag)
        {
            if (string.IsNullOrWhiteSpace(oldTag) || string.IsNullOrWhiteSpace(newTag))
                throw new ArgumentNullException($"'{nameof(oldTag)}' cannot be null, empty, or whitespace.", nameof(oldTag));

            if (string.IsNullOrWhiteSpace(newTag))
                throw new ArgumentNullException($"'{nameof(newTag)}' cannot be null, empty, or whitespace.", nameof(newTag));

            if (!journalReader.FrontMatter.HasTags())
                throw new InvalidOperationException("No tags exist in this journal.");

            var currentTags = journalReader.FrontMatter.Tags.ToList();
            var oldItemIndex = currentTags.IndexOf(oldTag);

            if (oldItemIndex < 0)
                throw new InvalidOperationException($"The tag '{oldTag}' does not exist.");

            currentTags[oldItemIndex] = newTag;

            var parser = string.IsNullOrEmpty(journalReader.FrontMatter.Readme) ? 
                null : 
                new ReadmeParser(journalReader.FrontMatter.Readme, journalReader.EntryDate);

            var frontMatter = new JournalFrontMatter(currentTags, parser);
            var newEntry = frontMatter.ToString(asFrontMatter: true) + journalReader.RawBody;
            _fileSystem.File.WriteAllText(journalReader.FilePath, newEntry);
        }

        public string GetJournalEntryFilePath(LocalDate entryDate)
        {
            var year = Journal.YearDirectoryPattern.Format(entryDate);
            var month = Journal.MonthDirectoryPattern.Format(entryDate);
            var parent = _fileSystem.Path.Combine(_rootDirectory, year, month);

            if (!_fileSystem.Directory.Exists(parent))
                _fileSystem.Directory.CreateDirectory(parent);

            var fileName = entryDate.ToJournalEntryFileName();
            var fullPath = _fileSystem.Path.Combine(parent, fileName);
            return fullPath;
        }

        public string GetCompiledJournalEntryFilePath(DateRange range)
        {
            var parent = _fileSystem.Path.Combine(_rootDirectory, "Compiled");

            if (!_fileSystem.Directory.Exists(parent))
                _fileSystem.Directory.CreateDirectory(parent);

            var fileName = range.ToJournalEntryFileName();
            var fullPath = _fileSystem.Path.Combine(parent, fileName);
            return fullPath;
        }

        public bool EntryExists(string path) => _fileSystem.File.Exists(path);
    }
}
