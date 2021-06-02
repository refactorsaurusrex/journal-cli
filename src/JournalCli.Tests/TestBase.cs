using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Tests
{
    public abstract class TestBase
    {
        public const int BodyWrapWidth = 120;

        private static readonly List<string> ValidJournalSamples = new List<string>
        {
            TestEntries.WithTags1,
            TestEntries.WithTags2,
            TestEntries.WithTags3,
            TestEntries.WithTags4,
            TestEntries.WithTags5,
            TestEntries.WithTags6,
            TestEntries.WithTagsAndReadme
        };

        private static readonly List<string> InvalidJournalSamples = new List<string>
        {
            TestEntries.WithoutFrontMatter,
            TestEntries.WithoutTags,
            TestEntries.Empty
        };

        private readonly Random _random = new Random();

        protected VirtualJournal CreateEmptyJournal()
        {
            var fileSystem = new VirtualJournal();
            fileSystem.AddDirectory(@"J:\Current\");
            return fileSystem;
        }

        protected VirtualJournal CreateVirtualJournal(int yearStart, int yearEnd, bool onlyValidEntries = false)
        {
            var samples = onlyValidEntries ? ValidJournalSamples : ValidJournalSamples.Concat(InvalidJournalSamples).ToList();
            var fileSystem = new VirtualJournal();

            for (var year = yearStart; year <= yearEnd; year++)
            {
                foreach (var month in Enumerable.Range(1, 12))
                {
                    var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
                    var currentPath = $@"J:\Current\{year}\{month:00} {monthName}";
                    fileSystem.AddDirectory(currentPath);

                    foreach (var day in Enumerable.Range(1, 28))
                    {
                        var dt = new LocalDate(year, month, day);
                        var filePath = fileSystem.Path.Combine(currentPath, dt.ToJournalEntryFileName());
                        var index = _random.Next(0, samples.Count - 1);

                        var text = samples[index];
                        fileSystem.AddFile(filePath, new MockFileData(text));

                        if (text == TestEntries.WithTagsAndReadme)
                            fileSystem.TotalReadmeEntries++;
                    }
                }
            }

            return fileSystem;
        }
    }
}
