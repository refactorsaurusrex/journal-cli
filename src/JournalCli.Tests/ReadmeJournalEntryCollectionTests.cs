using System.Linq;
using FluentAssertions;
using JournalCli.Core;
using NodaTime;
using Xunit;

namespace JournalCli.Tests
{
    public class ReadmeJournalEntryCollectionTests : TestBase
    {
        [Fact]
        public void Add_IgnoresEntries_WhenReadmeDateIsBeforeEarliestDate()
        {
            var fileSystem = CreateVirtualJournal(2010, 2016);
            var earliestDate = new LocalDate(2016, 1, 1);
            var readmeCollection = new ReadmeJournalEntryCollection(earliestDate, false);

            foreach (var filePath in fileSystem.AllFiles)
            {
                var reader = new JournalReader(fileSystem, filePath);
                readmeCollection.Add(reader);
            }

            var earliestReadme = readmeCollection.OrderBy(x => x.ReadmeDate).First();
            earliestReadme.ReadmeDate.Should().BeGreaterOrEqualTo(earliestDate);
        }

        [Fact]
        public void Add_IgnoresFutureEntries_WhenFutureEntriesAreExcluded()
        {
            var fileSystem = CreateVirtualJournal(2019, 2020);
            var earliestDate = new LocalDate();
            var readmeCollection = new ReadmeJournalEntryCollection(earliestDate, false);

            foreach (var filePath in fileSystem.AllFiles)
            {
                var reader = new JournalReader(fileSystem, filePath);
                readmeCollection.Add(reader);
            }

            readmeCollection.Should().BeEmpty();
        }

        [Fact]
        public void Add_IncludesEntries_WhenWithinAllConstraints()
        {
            var fileSystem = CreateVirtualJournal(2019, 2020);
            var earliestDate = new LocalDate();
            var readmeCollection = new ReadmeJournalEntryCollection(earliestDate, true);

            foreach (var filePath in fileSystem.AllFiles)
            {
                var reader = new JournalReader(fileSystem, filePath);
                readmeCollection.Add(reader);
            }

            readmeCollection.Should().HaveCount(fileSystem.TotalReadmeEntries);
        }
    }
}
