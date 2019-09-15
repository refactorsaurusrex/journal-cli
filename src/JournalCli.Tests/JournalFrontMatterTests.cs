using System.Collections.Generic;
using FluentAssertions;
using JournalCli.Core;
using Xunit;
using NodaTime;

namespace JournalCli.Tests
{
    public class JournalFrontMatterTests
    {
        [Theory]
        [InlineData("Tags:\r\n  - one\r\n  - one", "Tags:\r\n  - one", 1)]
        [InlineData("Tags:\r\n  - one\r\n  - one\r\n  - two", "Tags:\r\n  - one\r\n  - two", 2)]
        public void This_RemovesDuplicateTags_WhenPresent(string duplicateTags, string expectedYaml, int expectedCount)
        {
            var journalDate = new LocalDate(2019, 9, 8);
            var frontMatter = new JournalFrontMatter(duplicateTags, journalDate);

            frontMatter.Tags.Count.Should().Be(expectedCount);
            frontMatter.ToString().Should().BeEquivalentTo(expectedYaml);
        }

        [Fact]
        public void ToString_IgnoresNullValues_Always()
        {
            const string yaml = "Tags:\r\n  - one\r\n  - two";
            var journalDate = new LocalDate(2019, 9, 8);
            var frontMatter = new JournalFrontMatter(yaml, journalDate);
            var text = frontMatter.ToString();

            // Should include nothing but the originally supplied tags.
            text.Should().BeEquivalentTo(yaml);
        }

        [Theory]
        [InlineData("Tags:\r\n  - one\r\n  - two", "---\r\nTags:\r\n  - one\r\n  - two\r\n---")]
        [InlineData("Tags:\r\n  - one\r\n  - two\r\nReadme: 8/1/2017", "---\r\nTags:\r\n  - one\r\n  - two\r\nReadme: 8-1-2017\r\n---")]
        [InlineData("Tags:\r\n  - Thing\r\nReadme: 2 years", "---\r\nTags:\r\n  - Thing\r\nReadme: 2 years\r\n---")]
        public void ToString_ReturnsValidFrontMatter_Always(string input, string expectedResult)
        {
            var journalDate = new LocalDate(2019, 9, 8);
            var frontMatter = new JournalFrontMatter(input, journalDate);
            var text = frontMatter.ToString(true);
            text.Should().BeEquivalentTo(expectedResult);
        }

        [Theory]
        [MemberData(nameof(GetYamlBlockData))]
        public void Constructor_CanAcceptYamlBlockIndicators(string input, string toString, string expectedReadmeString, IEnumerable<string> expectedTags)
        {
            var journalDate = new LocalDate(2019, 9, 8);
            var frontMatter = new JournalFrontMatter(input, journalDate);
            frontMatter.ToString().Should().BeEquivalentTo(toString);
            frontMatter.Readme.Should().Be(expectedReadmeString);
            frontMatter.Tags.Should().BeEquivalentTo(expectedTags);
        }

        public static IEnumerable<object[]> GetYamlBlockData()
        {
            yield return new object[]
            {
                "---\r\nTags:\r\n  - one\r\n  - two\r\n---", "Tags:\r\n  - one\r\n  - two", null, new List<string> { "one", "two" }
            };
            yield return new object[]
            {
                "---\r\nTags:\r\n  - one\r\n  - two\r\nReadme: 8/1/2017\r\n---", "Tags:\r\n  - one\r\n  - two\r\nReadme: 8-1-2017", "8-1-2017", new List<string> { "one", "two" }
            };
            yield return new object[]
            {
                "---\r\nTags:\r\n  - Thing\r\nReadme: 2 years\r\n---", "Tags:\r\n  - Thing\r\nReadme: 2 years", "2 years", new List<string> { "Thing" }
            };
        }
    }
}
