using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Runtime.Caching;
using JournalCli.Infrastructure;

namespace JournalCli.Core
{
    public class JournalTagCache : IEnumerable<string>
    {
        private readonly CacheItemPolicy _policy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromHours(24) };

        private ICollection<string> _tags;

        public JournalTagCache()
        {
            if (MemoryCache.Default[nameof(JournalTagCache)] is ICollection<string> tags)
            {
                _tags = tags;
                return;
            }

            Invalidate();
        }

        public void Invalidate()
        {
            var fileSystem = new FileSystem();
            var store = new FileStore<UserSettings>(fileSystem);
            var settings = store.Load();
            var readerWriterFactory = new JournalReaderWriterFactory(fileSystem, settings.DefaultJournalRoot, 120);
            var markDownFiles = new MarkdownFiles(fileSystem, settings.DefaultJournalRoot);
            var systemProcess = SystemProcessFactory.Create();
            var journal = Journal.Open(readerWriterFactory, markDownFiles, systemProcess);
            var uniqueTags = journal.GetUniqueTags();

            MemoryCache.Default.Set(nameof(JournalTagCache), uniqueTags, _policy);
            _tags = uniqueTags;
        }

        public IEnumerator<string> GetEnumerator() => _tags.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}