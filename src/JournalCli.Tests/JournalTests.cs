using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using JournalCli.Core;
using JournalCli.Infrastructure;
using NodaTime;
using Xunit;

namespace JournalCli.Tests
{
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public class JournalTests : TestBase
    {
        [Fact]
        public void CreateNewEntry_CreatesFileName_ThatMatchEntryDate()
        {
            var fileSystem = new MockFileSystem();
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = A.Fake<IMarkdownFiles>();
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            journal.CreateNewEntry(new LocalDate(2019, 7, 19), null, "");

            fileSystem.AllFiles.Should().OnlyContain(x => x == "J:\\Current\\2019\\07 July\\2019.07.19.md");
        }

        [Theory]
        [InlineData(new string[] { }, "")]
        [InlineData(new string[] { }, null)]
        [InlineData(null, "")]
        [InlineData(null, null)]
        public void CreateNewEntry_AddsEmptyFrontMatter_WhenNoTagsOrReadmeAreProvided(string[] tags, string readme)
        {
            var fileSystem = new MockFileSystem();
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = A.Fake<IMarkdownFiles>();
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            journal.CreateNewEntry(new LocalDate(2019, 7, 19), tags, readme);

            fileSystem.GetFile("J:\\Current\\2019\\07 July\\2019.07.19.md").TextContents.Should().Be("---\r\n\r\n---\r\n# Friday, July 19, 2019\r\n");
        }

        [Fact]
        public void CreateNewEntry_OnlyIncludesTagsInFrontMatter_WhenOnlyTagsAreProvided()
        {
            var fileSystem = new MockFileSystem();
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = A.Fake<IMarkdownFiles>();
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            journal.CreateNewEntry(new LocalDate(2019, 7, 19), new List<string>{ "horse", "Dog", "panda" }.ToArray(), null);

            const string tags = "tags:\r\n  - Dog\r\n  - horse\r\n  - panda";
            var entryText = fileSystem.GetFile("J:\\Current\\2019\\07 July\\2019.07.19.md").TextContents;
            entryText.Should().Contain(tags);
            entryText.Should().NotContain("readme:");
        }

        [Fact]
        public void CreateNewEntry_OnlyIncludesReadmeInFrontMatter_WhenOnlyReadmeIsProvided()
        {
            var fileSystem = new MockFileSystem();
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = A.Fake<IMarkdownFiles>();
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            journal.CreateNewEntry(new LocalDate(2019, 7, 19), null, "2 years");

            var entryText = fileSystem.GetFile("J:\\Current\\2019\\07 July\\2019.07.19.md").TextContents;
            entryText.Should().Contain("readme: 2 years");
            entryText.Should().NotContain("tags:");
        }

        [Fact]
        public void CreateIndex_IncludesAnyTag_WhenNoRequiredTagsIncluded()
        {
            var fileSystem = CreateVirtualJournal(2017, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            var index = journal.CreateIndex<MetaJournalEntry>();

            var expectedTags = new List<string> { "baby", "blah", "carrot", "cat", "cow", "dog", "doh", "forrest", "horse", "hungry", "pig", "tree" };
            var actualTags = index.Select(x => x.Tag).ToList();

            actualTags.Count.Should().Be(actualTags.Distinct().Count());
            actualTags.Should().OnlyContain(tag => expectedTags.Contains(tag));
        }

        [Fact]
        public void CreateIndex_IncludesAllTags_WhenRequiredTagsIncluded()
        {
            var fileSystem = CreateVirtualJournal(2017, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            var requiredTags = new List<string> { "blah", "doh" };
            var index = journal.CreateIndex<MetaJournalEntry>(requiredTags: requiredTags);

            foreach (var entry in index.SelectMany(x => x.Entries))
                entry.Tags.Should().Contain(requiredTags);
        }

        [Fact]
        public void RenameTag_ThrowsException_WhenTagDoesNotExist()
        {
            var fileSystem = CreateVirtualJournal(2017, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            Assert.Throws<InvalidOperationException>(() => journal.RenameTag("superman", "megaman"));
        }

        [Fact]
        public void RenameTag_ChangesTagName_WhenOldTagExists()
        {
            var fileSystem = CreateVirtualJournal(2017, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            var originalIndex = journal.CreateIndex<MetaJournalEntry>();

            var blahCount = originalIndex["blah"].Count;
            var renamedFiles = journal.RenameTag("blah", "landscapes");

            var index = journal.CreateIndex<MetaJournalEntry>();
            var landscapeCount = index["landscapes"].Count;
            landscapeCount.Should().Be(blahCount);
            renamedFiles.Count.Should().Be(blahCount);
            index.Select(x => x.Tag).Should().NotContain("blah");
        }

        [Fact]
        public void RenameTagDryRun_DoesNotInvokeJournalWriter_Ever()
        {
            var fileSystem = CreateVirtualJournal(2017, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = A.Fake<IJournalReaderWriterFactory>();
            var writer = A.Fake<IJournalWriter>();
            A.CallTo(() => ioFactory.CreateWriter()).Returns(writer);
            A.CallTo(() => ioFactory.CreateReader(A<string>.Ignored)).ReturnsLazily((string file) => new JournalReader(fileSystem, file));
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);

            journal.RenameTagDryRun("blah");
            A.CallTo(() => writer.RenameTag(A<IJournalReader>._, A<string>._, A<string>._)).MustNotHaveHappened();
        }

        [Fact]
        public void RenameTagDryRun_ReturnsListOfEffectedFiles_Always()
        {
            var fileSystem = CreateVirtualJournal(2017, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            var index = journal.CreateIndex<MetaJournalEntry>();

            var blahCount = index["blah"].Count;
            journal.RenameTagDryRun("blah").Should().HaveCount(blahCount);
        }

        [Fact]
        public void RenameTagDryRun_ThrowsException_WhenTagDoesExist()
        {
            var fileSystem = CreateVirtualJournal(2017, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            Assert.Throws<InvalidOperationException>(() => journal.RenameTagDryRun("superman"));
        }

        [Fact]
        public void GetReadmeEntries_FiltersOutEntriesOlderThanEarliestDate()
        {
            var earliestDate = new LocalDate(2009, 4, 25);
            var fileSystem = CreateVirtualJournal(2005, 2010);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            var readmes = journal.GetReadmeEntries(earliestDate, false);

            var count = readmes.Count(r => r.ReadmeDate <= earliestDate);
            count.Should().Be(0);
        }

        [Fact]
        public void GetReadmeEntries_ReturnsAllEntries_WhenMaxDateIsDefaultValue()
        {
            var earliestDate = new LocalDate();
            var fileSystem = CreateVirtualJournal(2005, 2010);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            var readmes = journal.GetReadmeEntries(earliestDate, false);

            // Since this journal only has entries up to 2010, and the max expiration date is 5 years,
            // by "now" (e.g. the current date) they should have all expired. Therefore, the "include future" option
            // is effectively irrelevant in this scenario.
            readmes.Count.Should().Be(fileSystem.TotalReadmeEntries);
        }

        [Fact]
        public void GetReadmeEntries_ExcludesFutureEntries_WhenSpecified()
        {
            var earliestDate = new LocalDate();
            var fileSystem = CreateVirtualJournal(2030, 2030);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            var readmes = journal.GetReadmeEntries(earliestDate, false);

            // Because the journal only includes entries from 2030, and all readmes have relative dates specified (5 years),
            // all readme entries should have a future expiration. Note that in 2035, this test will need to be updated. ;)
            readmes.Count.Should().Be(0);
        }

        [Fact]
        public void GetReadmeEntries_IncludesFutureEntries_WhenSpecified()
        {
            var earliestDate = new LocalDate();
            var fileSystem = CreateVirtualJournal(2030, 2031);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            var readmes = journal.GetReadmeEntries(earliestDate, true);

            // Since the earliest entries in this journal are dated 2030, and the min expiration date is 5 years,
            // none of them will have expired before 2035. Therefore, all readmes in this collection should be included.
            // This test will also have to be updated in 2035. Fuck, I'll be 57 years old!
            readmes.Count.Should().Be(fileSystem.TotalReadmeEntries);
        }

        [Fact]
        public void OpenRandomEntry_ThrowsException_WhenNoTaggedEntriesFound()
        {
            var fileSystem = CreateVirtualJournal(2020, 2023);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);

            Assert.Throws<InvalidOperationException>(() => journal.OpenRandomEntry("fake"));
        }

        [Fact]
        public void OpenRandomEntry_ThrowsException_WhenNoEntriesFound()
        {
            var fileSystem = new MockFileSystem();
            const string rootDirectory = "J:\\Current";
            fileSystem.AddDirectory(rootDirectory);
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);

            Assert.Throws<InvalidOperationException>(() => journal.OpenRandomEntry("fake"));
        }

        [Fact]
        public void OpenRandomEntry_OpensRandomEntry_WhenOneExists()
        {
            var fileSystem = CreateVirtualJournal(2019, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            journal.OpenRandomEntry();
            A.CallTo(() => systemProcess.Start(A<string>._)).MustHaveHappened();
        }

        [Fact]
        public void OpenRandomEntry_OpensRandomEntry_ByTag()
        {
            var fileSystem = CreateVirtualJournal(2019, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            journal.OpenRandomEntry("blah");
            A.CallTo(() => systemProcess.Start(A<string>._)).MustHaveHappened();
        }
    }
}
