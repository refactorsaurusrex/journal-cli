using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using JournalCli.Core;
using NodaTime;

namespace JournalCli.Infrastructure
{
    internal class JournalWriter
    {
        private readonly IFileSystem _fileSystem;

        public JournalWriter(IFileSystem fileSystem) => _fileSystem = fileSystem;

        public void Create(IJournalFrontMatter journalFrontMatter, string filePath, LocalDate entryDate)
        {
            using (var fs = _fileSystem.File.CreateText(filePath))
            {
                fs.WriteLine(journalFrontMatter.ToString(asFrontMatter: true));
                fs.WriteLine($"# {entryDate.ToString()}");
                fs.Flush();
            }
        }

        public IEnumerable<string> RenameTag(Journal journal, string oldTag, string newTag, bool createBackupFiles)
        {
            var index = journal.CreateIndex(false);
            var journalEntries = index.SingleOrDefault(x => x.Tag == oldTag);

            if (journalEntries == null)
                throw new InvalidOperationException($"No entries found with the tag '{oldTag}'");

            var filePaths = new List<string>();
            foreach (var journalEntry in journalEntries.Entries)
            {
                filePaths.Add(journalEntry.FilePath);

                var journalReader = new JournalReader(_fileSystem, journalEntry.FilePath);
                var currentTags = journalReader.FrontMatter.Tags.ToList();
                var oldItemIndex = currentTags.IndexOf(oldTag);
                currentTags[oldItemIndex] = newTag;

                if (createBackupFiles)
                {
                    var backupName = $"{journalEntry.FilePath}{Constants.BackupFileExtension}";

                    var i = 0;
                    while (_fileSystem.File.Exists(backupName))
                        backupName = $"{journalEntry.FilePath}({i++}){Constants.BackupFileExtension}";

                    _fileSystem.File.Copy(journalEntry.FilePath, backupName);
                }

                var frontMatter = new JournalFrontMatter(currentTags, journalReader.FrontMatter.Readme);
                var originalEntry = _fileSystem.File.ReadAllText(journalEntry.FilePath);
                var newEntry = frontMatter + originalEntry;
                _fileSystem.File.WriteAllText(journalEntry.FilePath, newEntry);
            }

            return filePaths;
        }
    }
}
