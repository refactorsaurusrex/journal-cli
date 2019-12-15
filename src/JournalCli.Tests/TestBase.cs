using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Tests
{
    public class TestBase
    {
        private readonly Random _random = new Random();

        protected static List<string> JournalSamples = new List<string>
        {
            System.IO.File.ReadAllText(System.IO.Path.Combine(Environment.CurrentDirectory, "TestData", "EntryWithoutFrontMatter.md")),
            System.IO.File.ReadAllText(System.IO.Path.Combine(Environment.CurrentDirectory, "TestData", "EntryWithTags1.md")),
            System.IO.File.ReadAllText(System.IO.Path.Combine(Environment.CurrentDirectory, "TestData", "EntryWithTags2.md")),
            System.IO.File.ReadAllText(System.IO.Path.Combine(Environment.CurrentDirectory, "TestData", "EntryWithTags3.md")),
            System.IO.File.ReadAllText(System.IO.Path.Combine(Environment.CurrentDirectory, "TestData", "EntryWithTags4.md")),
            System.IO.File.ReadAllText(System.IO.Path.Combine(Environment.CurrentDirectory, "TestData", "EntryWithTags5.md")),
            System.IO.File.ReadAllText(System.IO.Path.Combine(Environment.CurrentDirectory, "TestData", "EntryWithTags6.md")),
            System.IO.File.ReadAllText(System.IO.Path.Combine(Environment.CurrentDirectory, "TestData", "EntryWithoutTags.md")),
            System.IO.File.ReadAllText(System.IO.Path.Combine(Environment.CurrentDirectory, "TestData", "EntryWithTagsAndReadme.md")),
            System.IO.File.ReadAllText(System.IO.Path.Combine(Environment.CurrentDirectory, "TestData", "EmptyEntry.md"))
        };

        protected static int JournalSampleLength => JournalSamples.Count - 1;

        protected static string EntryWithoutFrontMatter => JournalSamples[0];

        protected static string EntryWithTags => JournalSamples[1];

        protected static string EntryWithTagsAndReadme => JournalSamples[8];

        protected static string EmptyEntry => JournalSamples[9];

        protected VirtualJournal CreateEmptyJournal()
        {
            var fileSystem = new VirtualJournal();
            fileSystem.AddDirectory(@"J:\Current\");
            return fileSystem;
        }

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
                        var filePath = fileSystem.Path.Combine(currentPath, dt.ToJournalEntryFileName());
                        var index = _random.Next(0, JournalSampleLength);
                        fileSystem.AddFile(filePath, new MockFileData(JournalSamples[index]));
                        if (index == 8)
                            fileSystem.TotalReadmeEntries++;
                    }
                }
            }

            return fileSystem;
        }
    }
}
