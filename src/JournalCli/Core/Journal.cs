using System;
using System.Collections.Generic;
using System.Linq;
using JournalCli.Infrastructure;
using NodaTime;
using NodaTime.Text;

namespace JournalCli.Core
{
    internal class Journal
    {
        public static LocalDatePattern FileNamePattern { get; } = LocalDatePattern.CreateWithCurrentCulture("yyyy.MM.dd");
        public static LocalDatePattern MonthDirectoryPattern { get; } = LocalDatePattern.CreateWithCurrentCulture("MM MMMM");
        public static LocalDatePattern YearDirectoryPattern { get; } = LocalDatePattern.CreateWithCurrentCulture("yyyy");

        private readonly IJournalReaderWriterFactory _readerWriterFactory;
        private readonly IMarkdownFiles _markdownFiles;
        private readonly ISystemProcess _systemProcess;

        public static Journal Open(IJournalReaderWriterFactory readerWriterFactory, IMarkdownFiles markdownFiles, ISystemProcess systemProcess)
        {
            return new Journal(readerWriterFactory, markdownFiles, systemProcess);
        }

        private Journal(IJournalReaderWriterFactory readerWriterFactory, IMarkdownFiles markdownFiles, ISystemProcess systemProcess)
        {
            _readerWriterFactory = readerWriterFactory;
            _markdownFiles = markdownFiles;
            _systemProcess = systemProcess;
        }

        public void OpenRandomEntry(params string[] tags)
        {
            if (tags == null || tags.Length == 0)
            {
                var entries = _markdownFiles.FindAll();

                if (entries.Count == 0)
                    throw new InvalidOperationException("I couldn't find any journal entries. Did you pass in the right root directory?");

                var index = new Random().Next(0, entries.Count - 1);
                _systemProcess.Start(entries[index]);
            }
            else
            {
                var journalIndex = CreateIndex<JournalEntryFile>();
                if (journalIndex.Count == 0)
                    throw new InvalidOperationException("I couldn't find any journal entries. Did you pass in the right root directory?");

                var allTaggedEntries = journalIndex.Where(x => tags.Contains(x.Tag)).ToList();
                if (allTaggedEntries.Count == 0)
                    throw new InvalidOperationException("No entries were found with any of the tags provided.");

                var random = new Random();
                var randomIndex1 = random.Next(0, allTaggedEntries.Count - 1);
                var entries = allTaggedEntries[randomIndex1].Entries;
                var randomIndex2 = random.Next(0, entries.Count - 1);

                _systemProcess.Start(entries.ElementAt(randomIndex2).FilePath);
            }
        }

        public ReadmeJournalEntryCollection GetReadmeEntries(LocalDate earliestDate, bool includeFuture)
        {
            var readmeCollection = new ReadmeJournalEntryCollection(earliestDate, includeFuture);
            foreach (var file in _markdownFiles.FindAll())
            {
                var reader = _readerWriterFactory.CreateReader(file);
                readmeCollection.Add(reader);
            }

            return readmeCollection;
        }

        public IEnumerable<string> RenameTagDryRun(string oldName)
        {
            var index = CreateIndex<JournalEntryFile>();
            var journalEntries = index.SingleOrDefault(x => x.Tag == oldName);

            if (journalEntries == null)
                throw new InvalidOperationException($"No entries found with the tag '{oldName}'");

            return journalEntries.Entries.Select(e => e.EntryName).ToList();
        }

        public IEnumerable<string> RenameTag(string oldName, string newName)
        {
            var index = CreateIndex<JournalEntryFile>();
            var journalEntries = index.SingleOrDefault(x => x.Tag == oldName);

            if (journalEntries == null)
                throw new InvalidOperationException($"No entries found with the tag '{oldName}'");

            var writer = _readerWriterFactory.CreateWriter();
            var entryNames = new List<string>();
            foreach (var journalEntry in journalEntries.Entries)
            {
                entryNames.Add(journalEntry.EntryName);
                var reader = _readerWriterFactory.CreateReader(journalEntry.FilePath);
                writer.RenameTag(reader, oldName, newName);
            }

            return entryNames;
        }

        public JournalIndex<T> CreateIndex<T>()
            where T : class, IJournalEntry
        {
            var index = new JournalIndex<T>();

            foreach (var file in _markdownFiles.FindAll())
            {
                var reader = _readerWriterFactory.CreateReader(file);
                var entry = reader.ToJournalEntry<T>();

                if (entry.Tags == null || entry.Tags.Count == 0)
                    continue;

                foreach (var tag in entry.Tags)
                {
                    if (index.Contains(tag))
                    {
                        index[tag].Entries.Add(entry);
                    }
                    else
                    {
                        var journalIndexEntry = new JournalIndexEntry<T>(tag, entry);
                        index.Add(journalIndexEntry);
                    }
                }
            }

            return index;
        }

        public void CreateNewEntry(LocalDate entryDate, string[] tags, string readme)
        {
            var journalWriter = _readerWriterFactory.CreateWriter();
            var entryFilePath = journalWriter.GetJournalEntryFilePath(entryDate);
            var frontMatter = new JournalFrontMatter(tags, readme, entryDate);
            journalWriter.Create(frontMatter, entryFilePath, entryDate);
            _systemProcess.Start(entryFilePath);
        }
    }
}