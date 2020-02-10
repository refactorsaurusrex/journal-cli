using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Text;
using FakeItEasy;
using FluentAssertions;
using JournalCli.Core;
using JournalCli.Infrastructure;
using Xunit;

namespace JournalCli.Tests
{
    public class JournalWriterTests : TestBase
    {
        [Fact]
        public void RenameTag_CombinesNewFrontMatterWithExistingBody_Always()
        {
            var fileSystem = new MockFileSystem();
            var filePath = "J:\\JournalRoot\\2019\\03 March\\2019.01.01.md";
            fileSystem.AddFile(filePath, new MockFileData(TestEntries.WithTags1, Encoding.UTF8));
            var writer = new JournalWriter(fileSystem, "J:\\Current");
            var originalReader = new JournalReader(fileSystem, filePath);
            originalReader.FrontMatter.Tags.Should().OnlyContain(x => new List<string> { "doh", "blah" }.Contains(x));

            writer.RenameTag(originalReader, "blah", "horseman");
            var newReader = new JournalReader(fileSystem, filePath);

            newReader.RawBody.Should().Be(originalReader.RawBody);
            newReader.FrontMatter.Tags.Should().OnlyContain(x => new List<string> { "doh", "horseman" }.Contains(x));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(" ")]
        public void RenameTag_ThrowsArgumentNullException_IfOldTagIsNullEmptyOrWhitespace(string oldTag)
        {
            var fileSystem = new MockFileSystem();
            var writer = new JournalWriter(fileSystem, "J:\\Current");
            var reader = A.Fake<IJournalReader>();
            Assert.Throws<ArgumentNullException>(() => writer.RenameTag(reader, oldTag, "valid-value"));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData(" ")]
        public void RenameTag_ThrowsArgumentNullException_IfNewTagIsNullEmptyOrWhitespace(string newTag)
        {
            var fileSystem = new MockFileSystem();
            var writer = new JournalWriter(fileSystem, "J:\\Current");
            var reader = A.Fake<IJournalReader>();
            Assert.Throws<ArgumentNullException>(() => writer.RenameTag(reader, "valid-value", newTag));
        }

        [Fact]
        public void RenameTag_ThrowsInvalidOperationException_WhenOldTagDoesNotExist()
        {
            var fileSystem = CreateVirtualJournal(2017, 2020);
            var ioFactory = new JournalReaderWriterFactory(fileSystem, "J:\\Current");
            var writer = ioFactory.CreateWriter();
            var reader = ioFactory.CreateReader(fileSystem.AllFiles.First(x => x.EndsWith(".md")));
            Assert.Throws<InvalidOperationException>(() => writer.RenameTag(reader, "oldTag", "newTag"));
        }

        [Fact]
        public void RenameTag_ThrowsException_WhenNoTagsExist()
        {
            var fileSystem = new MockFileSystem();
            const string entryPath = @"J:\Current\2019\01 January\2019.01.01.md";
            fileSystem.AddFile(entryPath, new MockFileData(TestEntries.WithoutFrontMatter));
            var ioFactory = new JournalReaderWriterFactory(fileSystem, "J:\\Current");
            var writer = ioFactory.CreateWriter();
            var reader = ioFactory.CreateReader(entryPath);
            Assert.Throws<InvalidOperationException>(() => writer.RenameTag(reader, "oldTag", "newTag"));
        }
    }
}
