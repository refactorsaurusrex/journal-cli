using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Text;
using FluentAssertions;
using JournalCli.Core;
using JournalCli.Infrastructure;
using Xunit;

namespace JournalCli.Tests
{
    public class JournalReaderTests
    {
        [Fact]
        public void This_CanParseJournalFiles_WhenValid()
        {
            var fileSystem = new MockFileSystem();
            var filePath = "J:\\JournalRoot\\2019\\03 March\\2019.01.01.md";
            var entryText = System.IO.File.ReadAllText(System.IO.Path.Combine(Environment.CurrentDirectory, "TestData", "SampleJournalEntry.md"));
            fileSystem.AddFile(filePath, new MockFileData(entryText, Encoding.UTF8));
            IJournalReader reader = new JournalReader(fileSystem, filePath);

            reader.Body.Length.Should().Be(129);
            reader.EntryDate.Should().Be(new NodaTime.LocalDate(2019, 1, 1));
            reader.EntryName.Should().Be("2019.01.01");
            reader.FilePath.Should().Be(filePath);
            reader.FrontMatter.Tags.Should().OnlyContain(s => new List<string> { "blah", "doh" }.Contains(s));
            reader.FrontMatter.Readme.Should().Be("5 years");
            reader.Headers.Should().HaveCount(3);
        }
    }
}
