using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
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

        private static readonly CacheItemPolicy Policy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromHours(24) };

        public static Journal Open(IJournalReaderWriterFactory readerWriterFactory, IMarkdownFiles markdownFiles, ISystemProcess systemProcess)
        {
            if (!MemoryCache.Default.Contains(nameof(FirstEntryDate)))
            {
                var firstEntryDate = markdownFiles
                    .FindAll(fileNamesOnly: true)
                    .Select(md => FileNameWithExtensionPattern.Parse(md))
                    .Select(x => x.Value)
                    .OrderBy(dt => dt)
                    .First();

                MemoryCache.Default.Set(nameof(FirstEntryDate), firstEntryDate, Policy);
            }

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

        public MetaJournalEntry GetRandomEntry(ICollection<string> tags, TagOperator tagOperator, DateRange dateRange)
        {
            var entries = GetEntries<MetaJournalEntry>(tags, tagOperator, SortOrder.Ascending, dateRange, null).ToList();
            if (entries.Count == 0)
                throw new InvalidOperationException("I couldn't find any journal entries. Did you pass in the right root directory?");
            var index = new Random().Next(0, entries.Count - 1);
            return entries[index];
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

        public JournalIndex<T> CreateIndex<T>(DateRange range = null, ICollection<string> requiredTags = null, ICollection<string> optionalTags = null)
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

                if (optionalTags != null && optionalTags.Count > 0 && !entry.Tags.ContainsAny(optionalTags))
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

            var readmeExpression = new ReadmeParser(readme).ToExpression(entryDate);
            var frontMatter = new JournalFrontMatter(tags, readmeExpression);
            var header = $"# {entryDate}";
            journalWriter.Create(entryFilePath, frontMatter, header);
            _systemProcess.Start(entryFilePath);
        }

        public void AppendEntryContent(LocalDate entryDate, string[] body, string heading, string[] tags, string readme, out IEnumerable<string> warnings)
        {
            var journalWriter = _readerWriterFactory.CreateWriter();
            var entryFilePath = journalWriter.GetJournalEntryFilePath(entryDate);

            IJournalFrontMatter frontMatter;
            JournalEntryBody journalBody;
            var readmeParser = new ReadmeParser(readme);
            if (journalWriter.EntryExists(entryFilePath))
            {
                var journalReader = _readerWriterFactory.CreateReader(entryFilePath);

                frontMatter = journalReader.FrontMatter;
                frontMatter.AppendTags(tags);
                journalBody = new JournalEntryBody(journalReader.RawBody);

                warnings = readmeParser.IsValid
                    ? new List<string>
                    {
                        "This journal entry already has a Readme date applied. Readme dates cannot be overwritten using this method. " +
                        "If you want to edit the Readme date, open the entry and either change it manually, or delete it entirely and run this " +
                        "method again."
                    }
                    : new List<string>();
            }
            else
            {
                warnings = new List<string>();
                var readmeExpression = readmeParser.ToExpression(entryDate);
                frontMatter = new JournalFrontMatter(tags, readmeExpression);
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

        /// <summary>
        /// Creates a single journal entry comprised of all entries found which match the specified criteria. 
        /// </summary>
        /// <param name="range">The date range to search for entries. Dates are inclusive. Null values assume all entries are desired.</param>
        /// <param name="tags">Filters entries by tag. Null values assumes all tags are desired.</param>
        /// <param name="tagOperator">Indicates whether all tags must be matched, or if any tag can be matched.</param>
        /// <param name="overwrite">True to overwrite an existing compiled entry with the same name. Otherwise, an exception is thrown.</param>
        public void CreateCompiledEntry(DateRange range, string[] tags, TagOperator tagOperator, bool overwrite)
        {
            List<JournalEntryFile> entries;
            var hasTags = tags != null && tags.Length > 0;

            if (hasTags)
            {
                if (tagOperator == TagOperator.All)
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

            range ??= new DateRange(entries.First().EntryDate, entries.Last().EntryDate);

            var journalWriter = _readerWriterFactory.CreateWriter();
            var entryFilePath = journalWriter.GetCompiledJournalEntryFilePath(range);

            if (journalWriter.EntryExists(entryFilePath) && !overwrite)
                throw new JournalEntryAlreadyExistsException(entryFilePath);

            var aggregatedTags = entries.SelectMany(x => x.Tags).Distinct();
            var content = string.Join(Environment.NewLine, entries.Select(x => x.Body));

            var frontMatter = new JournalFrontMatter(aggregatedTags, ReadmeExpression.Empty());
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

            var frontMatter = new JournalFrontMatter(aggregatedTags, ReadmeExpression.Empty());
            journalWriter.CreateCompiled(frontMatter, entryFilePath, content);
            _systemProcess.Start(entryFilePath);
        }

        // TODO: Some of these parameters make no sense when you just want all entries. Should they all be optional??
        public IEnumerable<T> GetEntries<T>(ICollection<string> tags, TagOperator tagOperator, SortOrder direction, DateRange dateRange, int? limit)
            where T : class, IJournalEntry
        {
            var entries = _markdownFiles.FindAll()
                .Select(x => _readerWriterFactory.CreateReader(x).ToJournalEntry<T>());

            if (tags != null && tags.Any())
            {
                switch (tagOperator)
                {
                    default:
                        throw new NotSupportedException();
                    case TagOperator.All:
                        entries = entries.Where(x => x.Tags != null && x.Tags.ContainsAll(tags));
                        break;
                    case TagOperator.Any:
                        entries = entries.Where(x => x.Tags != null && x.Tags.ContainsAny(tags));
                        break;
                }
            }

            if (dateRange != null)
            {
                entries = entries.Where(x => dateRange.Includes(x.EntryDate));
            }

            switch (direction)
            {
                case SortOrder.Ascending:
                    entries = entries.OrderBy(x => x.EntryDate);
                    break;
                case SortOrder.Descending:
                    entries = entries.OrderByDescending(x => x.EntryDate);
                    break;
                default:
                    throw new NotSupportedException();
            }

            if (limit.HasValue)
            {
                entries = entries.Take(limit.Value);
            }

            return entries;
        }

        public JournalEntryFile GetEntryFromName(string name)
        {
            var entryDate = name.EndsWith(".md") ? FileNameWithExtensionPattern.Parse(name).Value : FileNamePattern.Parse(name).Value;
            var writer = _readerWriterFactory.CreateWriter();
            var path = writer.GetJournalEntryFilePath(entryDate);
            var reader = _readerWriterFactory.CreateReader(path);
            return new JournalEntryFile(reader);
        }

        public JournalEntryFile GetEntryFromDate(LocalDate date)
        {
            var writer = _readerWriterFactory.CreateWriter();
            var path = writer.GetJournalEntryFilePath(date);
            var reader = _readerWriterFactory.CreateReader(path);
            return new JournalEntryFile(reader);
        }

        public LocalDate FirstEntryDate
        {
            get
            {
                if (MemoryCache.Default[nameof(FirstEntryDate)] is LocalDate localDate)
                    return localDate;

                throw new InvalidOperationException($"{nameof(FirstEntryDate)} has not been set.");
            }
        }

        public ICollection<string> GetUniqueTags()
        {
            return GetEntries<MetaJournalEntry>(null, TagOperator.Any, SortOrder.Ascending, null, null)
                .SelectMany(x => x.Tags)
                .Distinct()
                .OrderBy(x => x)
                .ToList();
        }
    }
}
