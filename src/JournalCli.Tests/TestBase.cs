using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using JournalCli.Core;
using NodaTime;

namespace JournalCli.Tests
{
    public class TestBase
    {
        private readonly Random _random = new Random();

        protected static List<string> JournalSamples = new List<string>
        {
            System.IO.File.ReadAllText(System.IO.Path.Combine(Environment.CurrentDirectory, "TestData", "EntryWithoutFrontMatter.md")),
            System.IO.File.ReadAllText(System.IO.Path.Combine(Environment.CurrentDirectory, "TestData", "EntryWithTags.md")),
            System.IO.File.ReadAllText(System.IO.Path.Combine(Environment.CurrentDirectory, "TestData", "EntryWithTagsAndReadme.md")),
            System.IO.File.ReadAllText(System.IO.Path.Combine(Environment.CurrentDirectory, "TestData", "EmptyEntry.md"))
        };

        internal object CreateVirtualJournal()
        {
            throw new NotImplementedException();
        }

        protected static int JournalSampleLength => JournalSamples.Count - 1;

        protected string GetRandomJournalText() => JournalSamples[_random.Next(0, JournalSampleLength)];

        protected static string EntryWithoutFrontMatter => JournalSamples[0];

        protected static string EntryWithTags => JournalSamples[1];

        protected static string EntryWithTagsAndReadme => JournalSamples[2];

        protected static string EmptyEntry => JournalSamples[3];

        protected VirtualJournal CreateVirtualJournal(int yearStart, int yearEnd)
        {
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
                        var filePath = fileSystem.Path.Combine(currentPath, dt.ToString($"{JournalEntry.FileNamePattern.PatternText}'.md'", CultureInfo.CurrentCulture));
                        var index = _random.Next(0, JournalSampleLength);
                        fileSystem.AddFile(filePath, new MockFileData(JournalSamples[index]));
                        if (index == 2)
                            fileSystem.TotalReadmeEntries++;
                    }
                }
            }

            return fileSystem;
        }
    }
}
