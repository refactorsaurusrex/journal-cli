using System.IO.Abstractions.TestingHelpers;
using FakeItEasy;
using FluentAssertions;
using JournalCli.Core;
using JournalCli.Infrastructure;
using NodaTime;
using Xunit;

namespace JournalCli.Tests
{
    public class JournalTests
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
    }
}
