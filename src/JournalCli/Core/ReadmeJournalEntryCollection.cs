using System.Collections;
using System.Collections.Generic;
using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Core
{
    internal class ReadmeJournalEntryCollection : IEnumerable<ReadmeJournalEntry>
    {
        private readonly LocalDate _earliestDate;
        private readonly bool _includeFuture;
        private readonly List<ReadmeJournalEntry> _readMeEntries = new List<ReadmeJournalEntry>();

        /// <summary>
        /// Creates a new readme journal entry collection, with the specified minimum date. Optionally, can include future readme's.
        /// </summary>
        /// <param name="earliestDate">The earliest date to return readme entries. </param>
        /// <param name="includeFuture">True to include future readme entries. False to exclude them.</param>
        public ReadmeJournalEntryCollection(LocalDate earliestDate, bool includeFuture)
        {
            _earliestDate = earliestDate;
            _includeFuture = includeFuture;
        }

        public void Add(IJournalReader reader)
        {
            if (string.IsNullOrEmpty(reader.FrontMatter.Readme))
                return;

            var parser = new ReadmeParser(reader.FrontMatter.Readme, reader.EntryDate);

            if (_includeFuture && parser.ExpirationDate > Today.Date() || // Include readme's which expire after today, if requested.
                parser.ExpirationDate <= Today.Date() && parser.ExpirationDate >= _earliestDate)  // Include readme's between earliestDate and today.
            {
                var readme = new ReadmeJournalEntry(reader);
                _readMeEntries.Add(readme);
            }
        }

        public int Count => _readMeEntries.Count;

        public IEnumerator<ReadmeJournalEntry> GetEnumerator() => _readMeEntries.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
