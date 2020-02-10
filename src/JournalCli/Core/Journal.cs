using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JournalCli.Infrastructure;
using NodaTime;
using NodaTime.Text;

namespace JournalCli.Core
{
    internal class Journal
    {
        public static LocalDatePattern FileNamePattern { get; } = LocalDatePattern.CreateWithCurrentCulture("yyyy.MM.dd");
        public static LocalDatePattern FileNameWithExtensionPattern { get; } = LocalDatePattern.CreateWithCurrentCulture("yyyy.MM.dd'.md'");
        public static LocalDatePattern MonthDirectoryPattern { get; } = LocalDatePattern.CreateWithCurrentCulture("MM MMMM");
        public static LocalDatePattern YearDirectoryPattern { get; } = LocalDatePattern.CreateWithCurrentCulture("yyyy");

        private readonly IJournalReaderWriterFactory _readerWriterFactory;
        private readonly IMarkdownFiles _markdownFiles;
        private readonly ISystemProcess _systemProcess;

        public static Journal Open(IJournalReaderWriterFactory readerWriterFactory, IMarkdownFiles markdownFiles, ISystemProcess systemProcess)
        {
            return new Journal(readerWriterFactory, markdownFiles, systemProcess);
        }

        public static bool IsCompiledEntry(string fileName)
        {
            var regex = @"[0-9]{4}.[0-9]{2}.[0-9]{2}-[0-9]{4}.[0-9]{2}.[0-9]{2}";

            if (fileName.EndsWith(".md"))
                regex += @"\.md";

            return Regex.IsMatch(fileName, regex);
        }

        private Journal(IJournalReaderWriterFactory readerWriterFactory, IMarkdownFiles markdownFiles, ISystemProcess systemProcess)
        {
            _readerWriterFactory = readerWriterFactory;
            _markdownFiles = markdownFiles;
            _systemProcess = systemProcess;
        }

        public void OpenRandomEntry()
        {
            var entries = _markdownFiles.FindAll();

            if (entries.Count == 0)
                throw new InvalidOperationException("I couldn't find any journal entries. Did you pass in the right root directory?");

            var index = new Random().Next(0, entries.Count - 1);
            _systemProcess.Start(entries[index]);
        }

        public void OpenRandomEntry(ICollection<string> tags, DateRange dateRange)
        {
            if ((tags == null || tags.Count == 0) && dateRange == null)
            {
                OpenRandomEntry();
                return;
            }

            var entries = CreateIndex<JournalEntryFile>().SelectMany(x => x.Entries).ToList();

            if (entries.Count == 0)
                throw new InvalidOperationException("I couldn't find any journal entries. Did you pass in the right root directory?");

            if (tags != null && tags.Count > 0)
                entries = entries.Where(x => x.Tags.Any(tags.Contains)).ToList();

            if (dateRange != null)
                entries = entries.Where(x => dateRange.Includes(x.EntryDate)).ToList();

            if (entries.Count == 0)
                throw new InvalidOperationException("No entries were found with any of the tags provided or within the specified date range.");

            var random = new Random();
            var index = random.Next(0, entries.Count - 1);

            _systemProcess.Start(entries.ElementAt(index).FilePath);
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

        public ICollection<string> RenameTagDryRun(string oldName)
        {
            var index = CreateIndex<JournalEntryFile>();
            var journalEntries = index.SingleOrDefault(x => x.Tag == oldName);

            if (journalEntries == null)
                throw new InvalidOperationException($"No entries found with the tag '{oldName}'");

            return journalEntries.Entries.Select(e => e.EntryName).ToList();
        }

        public ICollection<string> RenameTag(string oldName, string newName)
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

        public JournalIndex<T> CreateIndex<T>(DateRange range = null, ICollection<string> requiredTags = null)
            where T : class, IJournalEntry
        {
            var index = new JournalIndex<T>();

            foreach (var file in _markdownFiles.FindAll())
            {
                var reader = _readerWriterFactory.CreateReader(file);
                if (range != null && !range.Includes(reader.EntryDate))
                    continue;

                var entry = reader.ToJournalEntry<T>();
                
                if (entry.Tags == null || entry.Tags.Count == 0)
                    continue;

                if (requiredTags != null && requiredTags.Count > 0 && !entry.Tags.ContainsAll(requiredTags))
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

            if (journalWriter.EntryExists(entryFilePath))
                throw new JournalEntryAlreadyExistsException(entryFilePath);

            var parser = string.IsNullOrWhiteSpace(readme) ? null : new ReadmeParser(readme, entryDate);
            var frontMatter = new JournalFrontMatter(tags, parser);
            var header = $"# {entryDate.ToString()}";
            journalWriter.Create(entryFilePath, frontMatter, header);
            _systemProcess.Start(entryFilePath);
        }

        public void AppendEntryContent(LocalDate entryDate, string[] body, string heading, string[] tags)
        {
            var journalWriter = _readerWriterFactory.CreateWriter();
            var entryFilePath = journalWriter.GetJournalEntryFilePath(entryDate);

            IJournalFrontMatter frontMatter;
            JournalEntryBody journalBody;
            if (journalWriter.EntryExists(entryFilePath))
            {
                var journalReader = _readerWriterFactory.CreateReader(entryFilePath);

                frontMatter = journalReader.FrontMatter;
                frontMatter.AppendTags(tags);
                journalBody = new JournalEntryBody(journalReader.RawBody);
            }
            else
            {
                frontMatter = new JournalFrontMatter(tags);
                journalBody = new JournalEntryBody();
            }

            if (body != null && body.Any())
            {
                if (string.IsNullOrEmpty(heading))
                    journalBody.AddOrAppendToDefaultHeader(entryDate, body);
                else
                    journalBody.AddOrAppendToCustomHeader(heading, body);
            }

            journalWriter.Create(entryFilePath, frontMatter, journalBody.ToString());
        }

        public IEnumerable<string> GetRecentEntries(int limit)
        {
            var files = _markdownFiles.FindAll()
                .Select(x => new JournalEntryFile(_readerWriterFactory.CreateReader(x)))
                .OrderByDescending(x => x.EntryDate)
                .Select(x => x.EntryName);

            return limit >= 1 ? files.Take(limit) : files;
        }

        /// <summary>
        /// Creates a single journal entry comprised of all entries found which match the specified criteria. 
        /// </summary>
        /// <param name="range">The date range to search for entries. Dates are inclusive. Null values assume all entries are desired.</param>
        /// <param name="tags">Filters entries by tag. Null values assumes all tags are desired.</param>
        /// <param name="allTagsRequired">Requires that each matching entry contain all tags specified by the `tags` parameter.</param>
        /// <param name="overwrite">True to overwrite an existing compiled entry with the same name. Otherwise, an exception is thrown.</param>
        public void CreateCompiledEntry(DateRange range, string[] tags, bool allTagsRequired, bool overwrite)
        {
            List<JournalEntryFile> entries;
            var hasTags = tags != null && tags.Length > 0;

            if (hasTags)
            {
                if (allTagsRequired)
                {
                    entries = CreateIndex<JournalEntryFile>(range, tags)
                        .SelectMany(x => x.Entries)
                        .OrderBy(x => x)
                        .Distinct()
                        .ToList();
                }
                else
                {
                    entries = CreateIndex<JournalEntryFile>(range)
                        .Where(x => tags.Contains(x.Tag))
                        .SelectMany(x => x.Entries)
                        .OrderBy(x => x)
                        .Distinct()
                        .ToList();
                }
            }
            else
            {
                entries = CreateIndex<JournalEntryFile>(range)
                    .SelectMany(x => x.Entries)
                    .OrderBy(x => x)
                    .Distinct()
                    .ToList();
            }

            if (entries.Count == 0)
                throw new InvalidOperationException("No journal entries found matching the specified criteria. Please change your criteria and try again.");

            if (range == null)
                range = new DateRange(entries.First().EntryDate, entries.Last().EntryDate);

            var journalWriter = _readerWriterFactory.CreateWriter();
            var entryFilePath = journalWriter.GetCompiledJournalEntryFilePath(range);

            if (journalWriter.EntryExists(entryFilePath) && !overwrite)
                throw new JournalEntryAlreadyExistsException(entryFilePath);

            var aggregatedTags = entries.SelectMany(x => x.Tags).Distinct();
            var content = string.Join(Environment.NewLine, entries.Select(x => x.Body));

            var frontMatter = new JournalFrontMatter(aggregatedTags);
            journalWriter.CreateCompiled(frontMatter, entryFilePath, content);
            _systemProcess.Start(entryFilePath);
        }

        public void CreateCompiledEntry(ICollection<IJournalEntry> entries, bool overwrite)
        {
            if (entries == null || !entries.Any())
                throw new ArgumentException("No entries were provided. At least one entry must be provided.", nameof(entries));

            var convertedEntries = entries.Select(x => new JournalEntryFile(x.GetReader())).OrderBy(x => x.EntryDate).ToList();
            var range = new DateRange(convertedEntries.First().EntryDate, convertedEntries.Last().EntryDate);

            var journalWriter = _readerWriterFactory.CreateWriter();
            var entryFilePath = journalWriter.GetCompiledJournalEntryFilePath(range);

            if (journalWriter.EntryExists(entryFilePath) && !overwrite)
                throw new JournalEntryAlreadyExistsException(entryFilePath);

            var aggregatedTags = convertedEntries.SelectMany(x => x.Tags).Distinct();
            var content = string.Join(Environment.NewLine, convertedEntries.Select(x => x.Body));

            var frontMatter = new JournalFrontMatter(aggregatedTags);
            journalWriter.CreateCompiled(frontMatter, entryFilePath, content);
            _systemProcess.Start(entryFilePath);
        }
    }
}
