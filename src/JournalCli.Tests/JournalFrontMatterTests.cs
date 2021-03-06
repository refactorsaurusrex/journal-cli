﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using JournalCli.Core;
using JournalCli.Infrastructure;
using Xunit;
using NodaTime;

namespace JournalCli.Tests
{
    public class JournalFrontMatterTests
    {
        [Fact]
        public void AppendTags_AddsTags_WhenNoCurrentTagsExist()
        {
            var frontMatter = new JournalFrontMatter(string.Empty, null);
            frontMatter.AppendTags(new[] { "test" });
            frontMatter.Tags.Should().Contain("test");
        }

        [Fact]
        public void ToString_ShouldInsertDefaultTags_IfNoneArePresent()
        {
            var frontMatter = new JournalFrontMatter(string.Empty, Today.Date());
            var yaml = frontMatter.ToString(true);
            yaml.Should().Contain("- (untagged)");

            var text = frontMatter.ToString();
            text.Should().Contain("- (untagged)");
        }

        [Theory]
        [InlineData("---\r\nTags:\r\n  - one\r\n  - two\r\nReadme: 1 year\r\n---", "7/10/2021", "7/10/2022")]
        [InlineData("---\r\nTags:\r\n  - one\r\n  - two\r\nReadme: 12 days\r\n---", "7/30/2021", "8/11/2021")]
        [InlineData("---\r\nTags:\r\n  - one\r\n  - two\r\nReadme: 4 months\r\n---", "2/28/2021", "6/28/2021")]
        [InlineData("---\r\nTags:\r\n  - one\r\n  - two\r\nReadme: 3 weeks\r\n---", "7/10/2021", "7/31/2021")]
        public void This_ParsesLegacyReadmeValues_WhenPresent(string yaml, string entryDate, string readmeDate)
        {
            var journalDate = entryDate.ToLocalDate();
            var frontMatter = new JournalFrontMatter(yaml, journalDate);

            frontMatter.Tags.Should().Contain(new List<string> { "one", "two" });
            frontMatter.Readme.Should().Be(readmeDate);
            frontMatter.ReadmeDate.Should().Be(readmeDate.ToLocalDate());
            frontMatter.IsEmpty().Should().BeFalse();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("---\r\n\r\n---\r\n")]
        public void This_DoesNotThrowExceptions_WhenYamlStringIsNullOrEmpty(string yaml)
        {
            var frontMatter = new JournalFrontMatter(yaml, null);

            frontMatter.Tags.Should().BeNull();
            frontMatter.Readme.Should().BeNull();
            frontMatter.ReadmeDate.Should().BeNull();
            frontMatter.IsEmpty().Should().BeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("---\r\n\r\n---\r\n")]
        public void This_DoesNotThrowExceptions_WhenYamlIsNullOrEmptyButDateIsValid(string yaml)
        {
            var frontMatter = new JournalFrontMatter(yaml, Today.Date());

            frontMatter.Tags.Should().BeNull();
            frontMatter.Readme.Should().BeNull();
            frontMatter.ReadmeDate.Should().BeNull();
            frontMatter.IsEmpty().Should().BeTrue();
        }

        [Fact]
        [SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
        public void This_DoesNotThrowExceptions_WhenTagsAndReadmeParserAreNull()
        {
            IEnumerable<string> tags = null;
            ReadmeExpression readmeExpression = null;
            var frontMatter = new JournalFrontMatter(tags, readmeExpression);

            frontMatter.Tags.Should().BeNull();
            frontMatter.Readme.Should().BeNull();
            frontMatter.ReadmeDate.Should().BeNull();
            frontMatter.IsEmpty().Should().BeTrue();
        }

        [Fact]
        public void This_DoesNotThrowExceptions_WhenEntryFileHasNoYaml()
        {
            var fileSystem = new MockFileSystem();
            var filePath = @"D:\TempJournal\2020\02 February\2020.02.12.md";
            fileSystem.AddFile(filePath, new MockFileData("Here is some text without front matter"));
            var fm4 = JournalFrontMatter.FromFilePath(fileSystem, filePath);

            fm4.Tags.Should().BeNull();
            fm4.Readme.Should().BeNull();
            fm4.ReadmeDate.Should().BeNull();
            fm4.IsEmpty().Should().BeTrue();
        }

        [Theory]
        [InlineData("Tags:\r\n  - one\r\n  - one", "tags:\r\n  - one", 1)]
        [InlineData("Tags:\r\n  - one\r\n  - one\r\n  - two", "tags:\r\n  - one\r\n  - two", 2)]
        public void This_RemovesDuplicateTags_WhenPresent(string duplicateTags, string expectedYaml, int expectedCount)
        {
            var journalDate = new LocalDate(2019, 9, 8);
            var frontMatter = new JournalFrontMatter(duplicateTags, journalDate);

            frontMatter.Tags.Count.Should().Be(expectedCount);
            frontMatter.ToString().Should().Be(expectedYaml);
        }

        [Fact]
        public void ToString_IgnoresNullValues_Always()
        {
            const string yaml = "Tags:\r\n  - one\r\n  - two";
            var journalDate = new LocalDate(2019, 9, 8);
            var frontMatter = new JournalFrontMatter(yaml, journalDate);
            var text = frontMatter.ToString();

            // Should include nothing but the originally supplied tags.
            text.Should().Be(yaml.ToLowerInvariant());
        }

        [Theory]
        [InlineData("Tags:\r\n  - one\r\n  - two", "---\r\ntags:\r\n  - one\r\n  - two\r\n---\r\n")]
        [InlineData("Tags:\r\n  - one\r\n  - two\r\nReadme: 8-1-2017", "---\r\ntags:\r\n  - one\r\n  - two\r\nreadme: 8/1/2017\r\n---\r\n")]
        [InlineData("Tags:\r\n  - Thing\r\nReadme: 9/8/2021", "---\r\ntags:\r\n  - Thing\r\nreadme: 9/8/2021\r\n---\r\n")]
        public void ToString_ReturnsValidFrontMatter_Always(string input, string expectedResult)
        {
            var journalDate = new LocalDate(2019, 9, 8);
            var frontMatter = new JournalFrontMatter(input, journalDate);
            var text = frontMatter.ToString(true);
            text.Should().Be(expectedResult);
        }

        [Theory]
        [MemberData(nameof(GetYamlBlockData))]
        public void Constructor_CanAcceptYamlBlockIndicators(string input, string toString, string expectedReadmeString, IEnumerable<string> expectedTags)
        {
            var journalDate = new LocalDate(2019, 9, 8);
            var frontMatter = new JournalFrontMatter(input, journalDate);
            frontMatter.ToString().Should().Be(toString);
            frontMatter.Readme.Should().Be(expectedReadmeString);
            frontMatter.Tags.Should().BeEquivalentTo(expectedTags);
        }

        public static IEnumerable<object[]> GetYamlBlockData()
        {
            yield return new object[]
            {
                "---\r\nTags:\r\n  - one\r\n  - two\r\n---", "tags:\r\n  - one\r\n  - two", null, new List<string> { "one", "two" }
            };
            yield return new object[]
            {
                "---\r\nTags:\r\n  - one\r\n  - two\r\nReadme: 8/1/2017\r\n---", "tags:\r\n  - one\r\n  - two\r\nreadme: 8/1/2017", "8/1/2017", new List<string> { "one", "two" }
            };
            yield return new object[]
            {
                "---\r\nTags:\r\n  - Thing\r\nReadme: 9/8/2021\r\n---", "tags:\r\n  - Thing\r\nreadme: 9/8/2021", "9/8/2021", new List<string> { "Thing" }
            };
        }
    }
}
