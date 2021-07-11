using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
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
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = A.Fake<IMarkdownFiles>();
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            journal.CreateNewEntry(new LocalDate(2019, 7, 19), tags, readme);

            fileSystem.GetFile("J:\\Current\\2019\\07 July\\2019.07.19.md").TextContents.Should().Be("---\r\ntags:\r\n  - (untagged)\r\n---\r\n# Friday, July 19, 2019\r\n");
        }

        [Fact]
        public void AddNewJournalContent_ReturnsWarning_IfReadmeAlreadyExists()
        {
            const string rootDirectory = "J:\\Current";
            var entryDate = new LocalDate(2021, 7, 20);
            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(entryDate.Month);
            var currentPath = $@"J:\Current\{entryDate.Year}\{entryDate.Month:00} {monthName}";
            var fileSystem = new MockFileSystem();
            fileSystem.AddDirectory(currentPath);
            var filePath = fileSystem.Path.Combine(currentPath, entryDate.ToJournalEntryFileName());
            fileSystem.AddFile(filePath, new MockFileData(TestEntries.WithTagsAndReadme));
            
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = A.Fake<IMarkdownFiles>();
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            
            journal.AppendEntryContent(entryDate, new[] { "This is a body" }, "# Headering", new[] { "one", "two" }, "1 year", out var warnings);
            warnings.Count().Should().Be(1);
        }
        
        [Fact]
        public void AddNewJournalContent_ReturnsNoWarnings_IfReadmeDoesNotAlreadyExists()
        {
            const string rootDirectory = "J:\\Current";
            var entryDate = new LocalDate(2021, 7, 20);
            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(entryDate.Month);
            var currentPath = $@"J:\Current\{entryDate.Year}\{entryDate.Month:00} {monthName}";
            var fileSystem = new MockFileSystem();
            fileSystem.AddDirectory(currentPath);
            var filePath = fileSystem.Path.Combine(currentPath, entryDate.ToJournalEntryFileName());
            fileSystem.AddFile(filePath, new MockFileData(TestEntries.WithTags1));
            
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = A.Fake<IMarkdownFiles>();
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            
            journal.AppendEntryContent(entryDate, new[] { "This is a body" }, "# Headering", new[] { "one", "two" }, "1 year", out var warnings);
            warnings.Count().Should().Be(1);
        }

        [Fact]
        public void CreateNewEntry_OnlyIncludesTagsInFrontMatter_WhenOnlyTagsAreProvided()
        {
            var fileSystem = new MockFileSystem();
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
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
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = A.Fake<IMarkdownFiles>();
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            journal.CreateNewEntry(new LocalDate(2019, 7, 19), null, "2 years");

            var entryText = fileSystem.GetFile("J:\\Current\\2019\\07 July\\2019.07.19.md").TextContents;
            entryText.Should().Contain("readme: 7/19/2021");
            entryText.Should().NotContain("tags:");
        }

        [Fact]
        public void CreateNewEntry_ThrowsException_IfEntryAlreadyExists()
        {
            var fileSystem = CreateVirtualJournal(2019, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);

            Assert.Throws<JournalEntryAlreadyExistsException>(() => journal.CreateNewEntry(new LocalDate(2019, 1, 1), null, null));
        }

        [Fact]
        public void CreateIndex_IncludesAnyTag_WhenNoRequiredTagsIncluded()
        {
            var fileSystem = CreateVirtualJournal(2017, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
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
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            var requiredTags = new List<string> { "blah", "doh" };
            var index = journal.CreateIndex<MetaJournalEntry>(requiredTags: requiredTags);

            foreach (var entry in index.SelectMany(x => x.Entries))
                entry.Tags.Should().Contain(requiredTags);
        }

        [Fact]
        public void CreateIndex_SkipsEntries_OutsideDateRange()
        {
            var fileSystem = CreateVirtualJournal(2017, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);

            var from = new LocalDate(2019, 1, 1);
            var to = new LocalDate(2019, 1, 10);
            var dateRange = new DateRange(from, to);
            var index = journal.CreateIndex<JournalEntryFile>(dateRange);
            index.SelectMany(x => x.Entries).All(x => x.EntryDate >= from && x.EntryDate <= to).Should().BeTrue();
        }

        [Fact]
        public void RenameTag_ThrowsException_WhenTagDoesNotExist()
        {
            var fileSystem = CreateVirtualJournal(2017, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
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
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
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
            A.CallTo(() => ioFactory.CreateReader(A<string>.Ignored)).ReturnsLazily((string file) => new JournalReader(fileSystem, file, BodyWrapWidth));
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
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
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
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
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
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
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
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
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
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
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
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
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
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);

            Assert.Throws<InvalidOperationException>(() => journal.GetRandomEntry(new List<string>{ "fake" }, TagOperator.Any, null));
        }

        [Fact]
        public void OpenRandomEntry_ThrowsException_WhenNoEntriesFound()
        {
            var fileSystem = new MockFileSystem();
            const string rootDirectory = "J:\\Current";
            fileSystem.AddDirectory(rootDirectory);
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);

            Assert.Throws<InvalidOperationException>(() => journal.GetRandomEntry(new[] { "fake" }, TagOperator.Any, null));
        }

        [Fact]
        public void OpenRandomEntry_OpensEntryWithinDateRange_WhenRangeIsProvided()
        {
            var fileSystem = CreateVirtualJournal(2019, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            var entry = journal.GetRandomEntry(null, TagOperator.Any, new DateRange(new LocalDate(2019, 6,1), new LocalDate(2019, 6, 15)));

            var nameElements = entry.EntryName.Split(new[] { '.' });
            var intElements = nameElements.Select(int.Parse).ToArray();
            Assert.True(intElements[0] == 2019 && intElements[1] == 6 && intElements[2] >= 1 && intElements[2] <= 15);
        }

        [Fact]
        public void OpenRandomEntry_SearchesEntireJournal_WhenNoTagsOrDateRangeUsed()
        {
            var fileSystem = CreateVirtualJournal(2019, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);

            var entry = journal.GetRandomEntry(new string[] { }, TagOperator.Any, null);
            entry.Should().NotBeNull();
        }

        [Fact]
        public void OpenRandomEntry_ThrowsException_WhenNoEntriesAreFound()
        {
            var fileSystem = CreateEmptyJournal();
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var systemProcess = A.Fake<ISystemProcess>();
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);

            Assert.Throws<InvalidOperationException>(() => journal.GetRandomEntry(null, TagOperator.Any, null));
        }

        [Fact]
        public void CreateCompiledEntry1_ThrowsException_WhenEntryExistsAndOverwriteIsFalse()
        {
            var fileSystem = CreateVirtualJournal(2019, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);

            var dateRange = new DateRange("2019-2-12", "2019-3-1");
            journal.CreateCompiledEntry(dateRange, null, TagOperator.Any, false);
            Assert.Throws<JournalEntryAlreadyExistsException>(() => journal.CreateCompiledEntry(dateRange, null, TagOperator.Any, false));
        }

        [Fact]
        public void CreateCompiledEntry1_OverwritesEntry_WhenEntryExistsAndOverwriteIsTrue()
        {
            var fileSystem = CreateVirtualJournal(2019, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);

            var dateRange = new DateRange("2019-2-12", "2019-3-1");
            journal.CreateCompiledEntry(dateRange, tags: null, TagOperator.Any, overwrite: false);
            journal.CreateCompiledEntry(dateRange, tags: null, TagOperator.Any, overwrite: true);
        }

        [Fact]
        public void CreateCompiledEntry1_IncludesAllTags_WhenNoneAreSpecified()
        {
            var fileSystem = CreateVirtualJournal(2019, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);

            var dateRange = new DateRange("2019-2-12", "2019-3-1");
            var filePath = ioFactory.CreateWriter().GetCompiledJournalEntryFilePath(dateRange);
            journal.CreateCompiledEntry(dateRange, tags: null, TagOperator.Any, overwrite: false);

            var allTags = journal.CreateIndex<MetaJournalEntry>().Select(x => x.Tag);
            ioFactory.CreateReader(filePath).FrontMatter.Tags.Should().OnlyContain(t => allTags.Contains(t));
        }

        [Fact]
        public void CreateCompiledEntry1_IncludesAllDates_WhenNoFiltersAreSpecified()
        {
            var fileSystem = CreateVirtualJournal(2016, 2019, onlyValidEntries: true);

            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);

            var dateRange = new DateRange("2016-1-1", "2019-12-28"); // Virtual journal assumes 28 days in a month.
            var filePath = ioFactory.CreateWriter().GetCompiledJournalEntryFilePath(dateRange);
            journal.CreateCompiledEntry(range: null, tags: null, TagOperator.Any, overwrite: false);

            fileSystem.FileExists(fileSystem.Path.Combine(rootDirectory, "Compiled", filePath)).Should().BeTrue();
        }

        [Theory]
        [InlineData("cat", "cow")]
        [InlineData("cat")]
        public void CreateCompiledEntry1_FiltersByTag_WhenTagIsSpecified(params string[] tags)
        {
            var fileSystem = CreateVirtualJournal(2019, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);

            var dateRange = new DateRange("2019-2-12", "2019-6-1");
            var filePath = ioFactory.CreateWriter().GetCompiledJournalEntryFilePath(dateRange);
            journal.CreateCompiledEntry(dateRange, tags: tags, TagOperator.Any, overwrite: false);

            var expectedTags = journal.CreateIndex<MetaJournalEntry>()
                .Where(x => tags.Contains(x.Tag))
                .SelectMany(x => x.Entries)
                .SelectMany(x => x.Tags)
                .Distinct();

            ioFactory.CreateReader(filePath).FrontMatter.Tags.Should().OnlyContain(t => expectedTags.Contains(t));
        }

        [Theory]
        [InlineData("blah", "horse")]
        [InlineData("blah", "doh", "cat")]
        public void CreateCompiledEntry1_FiltersByTag_WhenTagIsSpecifiedAndAllAreRequired(params string[] tags)
        {
            var fileSystem = CreateVirtualJournal(2019, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);

            var dateRange = new DateRange("2019-2-12", "2019-6-1");
            var filePath = ioFactory.CreateWriter().GetCompiledJournalEntryFilePath(dateRange);
            journal.CreateCompiledEntry(dateRange, tags: tags, TagOperator.All, overwrite: false);

            var expectedTags = journal.CreateIndex<MetaJournalEntry>(range: null, requiredTags: tags)
                .SelectMany(x => x.Entries)
                .SelectMany(x => x.Tags)
                .Distinct();

            ioFactory.CreateReader(filePath).FrontMatter.Tags.Should().OnlyContain(t => expectedTags.Contains(t));
        }

        [Fact]
        public void CreateCompiledEntry1_ThrowsException_WhenNoTaggedEntriesFound()
        {
            var fileSystem = CreateVirtualJournal(2019, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);

            var dateRange = new DateRange("2019-2-12", "2019-6-1");

            Assert.Throws<InvalidOperationException>(() =>
                journal.CreateCompiledEntry(dateRange, tags: new[] { "Jose" }, TagOperator.All, overwrite: false)
            );
        }

        [Fact]
        public void CreateCompiledEntry1_ThrowsException_WhenNoEntriesFoundInDateRange()
        {
            var fileSystem = CreateVirtualJournal(2019, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);

            var dateRange = new DateRange("2018-2-12", "2018-6-1");

            Assert.Throws<InvalidOperationException>(() =>
                journal.CreateCompiledEntry(dateRange, tags: null, TagOperator.All, overwrite: false)
            );
        }

        [Theory]
        [MemberData(nameof(EmptyListOfEntries))]
        public void CreateCompiledEntry2_ThrowsException_WhenNoEntriesProvided(ICollection<IJournalEntry> entries)
        {
            var fileSystem = CreateVirtualJournal(2019, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);

            Assert.Throws<ArgumentException>(() =>
                journal.CreateCompiledEntry(entries, false)
            );

            Assert.Throws<ArgumentException>(() =>
                journal.CreateCompiledEntry(null, false)
            );
        }

        [Theory]
        [ClassData(typeof(PipedEntries))]
        public void CreateCompiledEntry2_CreatesEntry_WithProvidedEntries(ICollection<JournalEntryFile> entries)
        {
            var fileSystem = CreateVirtualJournal(2019, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            var expectedTags = entries.SelectMany(x => x.Tags).Distinct();
            var expectedBodies = entries.Select(x => x.Body).Distinct();

            journal.CreateCompiledEntry(entries.Cast<IJournalEntry>().ToList(), false);

            var compiledDirectory = fileSystem.Path.Combine(rootDirectory, "Compiled");
            var file = fileSystem.Directory.GetFiles(compiledDirectory).Single();
            var fileText = fileSystem.File.ReadAllText(file);

            foreach (var body in expectedBodies)
                fileText.Should().Contain(body);

            JournalFrontMatter.FromFilePath(fileSystem, file).Tags.Should().OnlyContain(t => expectedTags.Contains(t));

            A.CallTo(() => systemProcess.Start(A<string>._)).MustHaveHappened();
        }

        [Theory]
        [ClassData(typeof(PipedEntries))]
        public void CreateCompiledEntry2_ThrowsException_WhenEntryExistsAndOverwriteIsFalse(ICollection<JournalEntryFile> entries)
        {
            var fileSystem = CreateVirtualJournal(2019, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);

            journal.CreateCompiledEntry(entries.Cast<IJournalEntry>().ToList(), false);
            Assert.Throws<JournalEntryAlreadyExistsException>(() => journal.CreateCompiledEntry(entries.Cast<IJournalEntry>().ToList(), false));
        }

        [Theory]
        [ClassData(typeof(PipedEntries))]
        public void CreateCompiledEntry2_OverwritesEntry_WhenEntryExistsAndOverwriteIsTrue(ICollection<JournalEntryFile> entries)
        {
            var fileSystem = CreateVirtualJournal(2019, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);

            journal.CreateCompiledEntry(entries.Cast<IJournalEntry>().ToList(), false);
            journal.CreateCompiledEntry(entries.Cast<IJournalEntry>().ToList(), true);
        }

        public static IEnumerable<object[]> EmptyListOfEntries() => new[] { new object[] { new List<JournalEntryFile>().Cast<IJournalEntry>().ToList() } };
    }

    public class PipedEntries : TestBase, IEnumerable<object[]>
    {
        private readonly Journal _journal;

        public PipedEntries()
        {
            var fileSystem = CreateVirtualJournal(2019, 2019);
            const string rootDirectory = "J:\\Current";
            var ioFactory = new JournalReaderWriterFactory(fileSystem, rootDirectory, BodyWrapWidth);
            var systemProcess = A.Fake<ISystemProcess>();
            var markdownFiles = new MarkdownFiles(fileSystem, rootDirectory);
            _journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
        }

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { _journal.CreateIndex<JournalEntryFile>()["blah"].Entries };

        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
