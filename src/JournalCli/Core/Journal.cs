using System;
using System.IO.Abstractions;
using System.Linq;
using JournalCli.Infrastructure;
using NodaTime;
using NodaTime.Text;

namespace JournalCli.Core
{
    internal class Journal
    {
        private readonly IJournalReaderFactory _journalReaderFactory;
        private readonly IFileSystem _fileSystem;
        private readonly ISystemProcess _systemProcess;
        private readonly string _rootDirectory;

        public static Journal Open(IJournalReaderFactory readerFactory, IFileSystem fileSystem, ISystemProcess systemProcess, string rootDirectory)
        {
            return new Journal(readerFactory, fileSystem, systemProcess, rootDirectory);
        }

        private Journal(IJournalReaderFactory readerFactory, IFileSystem fileSystem, ISystemProcess systemProcess, string rootDirectory)
        {
            _journalReaderFactory = readerFactory;
            _fileSystem = fileSystem;
            _systemProcess = systemProcess;
            _rootDirectory = rootDirectory;
        }

        public void OpenRandomEntry(string[] tags)
        {
            if (tags == null || tags.Length == 0)
            {
                var entries = MarkdownFiles.FindAll(_fileSystem, _rootDirectory);

                if (entries.Count == 0)
                    throw new InvalidOperationException("I couldn't find any journal entries. Did you pass in the right root directory?");

                var index = new Random().Next(0, entries.Count - 1);
                _systemProcess.Start(entries[index]);
            }
            else
            {
                var journalIndex = CreateIndex(false);
                if (journalIndex.Count == 0)
                    throw new InvalidOperationException("I couldn't find any journal entries. Did you pass in the right root directory?");

                var allTaggedEntries = journalIndex.Where(x => tags.Contains(x.Tag)).ToList();
                var random = new Random();
                var randomIndex1 = random.Next(0, allTaggedEntries.Count - 1);
                var entries = allTaggedEntries[randomIndex1].Entries;
                var randomIndex2 = random.Next(0, entries.Count - 1);

                _systemProcess.Start(entries.ElementAt(randomIndex2).FilePath);
            }
        }

        public ReadmeJournalEntryCollection GetReadmeEntries(LocalDate maxDate, bool includeFuture)
        {
            var readmeCollection = new ReadmeJournalEntryCollection(maxDate, includeFuture);
            foreach (var file in MarkdownFiles.FindAll(_fileSystem, _rootDirectory))
            {
                var reader = _journalReaderFactory.CreateReader(file);
                readmeCollection.Add(reader);
            }

            return readmeCollection;
        }

        public JournalIndex CreateIndex(bool includeHeaders)
        {
            var index = new JournalIndex();

            foreach (var file in MarkdownFiles.FindAll(_fileSystem, _rootDirectory))
            {
                var reader = _journalReaderFactory.CreateReader(file);
                var entry = new JournalEntry(reader);
                foreach (var tag in entry.Tags)
                {
                    if (index.Contains(tag))
                    {
                        index[tag].Entries.Add(entry);
                    }
                    else
                    {
                        var journalIndexEntry = new JournalIndexEntry(tag, entry);
                        index.Add(journalIndexEntry);
                    }
                }
            }

            return index;
        }

        public void CreateNewEntry(LocalDate entryDate, string[] tags, string readme)
        {
            var year = entryDate.Year.ToString();
            var month = $"{entryDate.Month:00} {entryDate:MMMM}";
            var parent = _fileSystem.Path.Combine(_rootDirectory, year, month);

            if (!_fileSystem.Directory.Exists(parent))
                _fileSystem.Directory.CreateDirectory(parent);

            var fileName = JournalEntry.FileNamePattern.Format(entryDate);
            var fullPath = _fileSystem.Path.Combine(parent, fileName);

            if (_fileSystem.File.Exists(fullPath))
                throw new InvalidOperationException($"Journal entry already exists: '{fullPath}'");

            var journalWriter = new JournalWriter(_fileSystem);
            var frontMatter = new JournalFrontMatter(tags, readme);
            journalWriter.Create(frontMatter, fullPath, entryDate);
            _systemProcess.Start(fullPath);
        }
    }
}