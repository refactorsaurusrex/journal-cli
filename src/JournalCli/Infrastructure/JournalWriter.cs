using System;
using System.Collections.Generic;
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

        public void Create(IJournalFrontMatter journalFrontMatter, string filePath, LocalDate entryDate)
        {
            using (var fs = _fileSystem.File.CreateText(filePath))
            {
                fs.WriteLine(journalFrontMatter.ToString(asFrontMatter: true));
                fs.WriteLine($"# {entryDate.ToString()}");
                fs.Flush();
            }
        }

        public void RenameTag(IJournalReader journalReader, string oldTag, string newTag, bool createBackup)
        {
            var currentTags = journalReader.FrontMatter.Tags.ToList();
            var oldItemIndex = currentTags.IndexOf(oldTag);
            currentTags[oldItemIndex] = newTag;

            if (createBackup)
            {
                var backupName = $"{journalReader.FilePath}{Constants.BackupFileExtension}";

                var i = 0;
                while (_fileSystem.File.Exists(backupName))
                    backupName = $"{journalReader.FilePath}({i++}){Constants.BackupFileExtension}";

                _fileSystem.File.Copy(journalReader.FilePath, backupName);
            }

            var frontMatter = new JournalFrontMatter(currentTags, journalReader.FrontMatter.Readme);
            var originalEntry = _fileSystem.File.ReadAllText(journalReader.FilePath);
            // TEST: Ensure newEntry is as expected
            var newEntry = frontMatter + originalEntry;
            _fileSystem.File.WriteAllText(journalReader.FilePath, newEntry);
        }

        public string GetJournalEntryFilePath(LocalDate entryDate)
        {
            var year = entryDate.Year.ToString();
            var month = $"{entryDate.Month:00} {entryDate:MMMM}";
            var parent = _fileSystem.Path.Combine(_rootDirectory, year, month);

            if (!_fileSystem.Directory.Exists(parent))
                _fileSystem.Directory.CreateDirectory(parent);

            var fileName = $"{JournalEntry.FileNamePattern.Format(entryDate)}.md";
            var fullPath = _fileSystem.Path.Combine(parent, fileName);

            if (_fileSystem.File.Exists(fullPath))
                throw new InvalidOperationException($"Journal entry already exists: '{fullPath}'");

            return fullPath;
        }
    }
}
