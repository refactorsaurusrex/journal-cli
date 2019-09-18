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
    public class JournalReaderTests : TestBase
    {
        [Theory]
        [MemberData(nameof(JournalTestData))]
        public void This_CanParseJournalFiles_WhenValid(string entry, string readme, int headerCount, int bodyLength, List<string> tags)
        {
            var fileSystem = new MockFileSystem();
            var filePath = "J:\\JournalRoot\\2019\\03 March\\2019.01.01.md";
            fileSystem.AddFile(filePath, new MockFileData(entry, Encoding.UTF8));
            IJournalReader reader = new JournalReader(fileSystem, filePath);

            reader.Body.Length.Should().Be(bodyLength);
            reader.EntryDate.Should().Be(new NodaTime.LocalDate(2019, 1, 1));
            reader.EntryName.Should().Be("2019.01.01");
            reader.FilePath.Should().Be(filePath);

            if (tags == null)
                reader.FrontMatter.Tags.Should().BeNull();
            else
                reader.FrontMatter.Tags.Should().OnlyContain(s => tags.Contains(s));
            reader.FrontMatter.Readme.Should().Be(readme);
            reader.Headers.Should().HaveCount(headerCount);
        }

        public static IEnumerable<object[]> JournalTestData()
        {
            yield return new object[] { EntryWithoutFrontMatter, null, 1, 72, null };
            yield return new object[] { EmptyEntry, null, 0, 0, null };
            yield return new object[] { EntryWithTags, null, 3, 129, new List<string> { "blah", "doh" } };
            yield return new object[] { EntryWithTagsAndReadme, "5 years", 2, 75, new List<string> { "blah", "doh" } };
        }
    }
}
