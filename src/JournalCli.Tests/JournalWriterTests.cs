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
    public class JournalWriterTests
    {
        [Fact]
        public void RenameTag_CombinesNewFrontMatterWithExistingBody_Always()
        {
            var fileSystem = new MockFileSystem();
            var filePath = "J:\\JournalRoot\\2019\\03 March\\2019.01.01.md";
            var entryText = System.IO.File.ReadAllText(System.IO.Path.Combine(Environment.CurrentDirectory, "TestData", "SampleJournalEntry.md"));
            fileSystem.AddFile(filePath, new MockFileData(entryText, Encoding.UTF8));
            var writer = new JournalWriter(fileSystem, "J:\\Current");
            var originalReader = new JournalReader(fileSystem, filePath);
            originalReader.FrontMatter.Tags.Should().OnlyContain(x => new List<string> { "doh", "blah" }.Contains(x));

            writer.RenameTag(originalReader, "blah", "horseman", false);
            var newReader = new JournalReader(fileSystem, filePath);

            newReader.Body.Should().Be(originalReader.Body);
            newReader.FrontMatter.Tags.Should().OnlyContain(x => new List<string> { "doh", "horseman" }.Contains(x));
        }
    }
}
