using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using JournalCli.Infrastructure;
using NodaTime;
using Xunit;

namespace JournalCli.Tests
{
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public class ReadmeParserTests
    {
        [Theory]
        [InlineData("9/7/2019", "9-7-2019")]
        [InlineData("9.7.2019", "9-7-2019")]
        [InlineData("09/07/2019", "9-7-2019")]
        [InlineData("9/07/2019", "9-7-2019")]
        [InlineData("09/7/2019", "9-7-2019")]
        [InlineData("09\\7\\2019", "9-7-2019")]
        [InlineData("9-7-19", "9-7-2019")]
        [InlineData("09-07-19", "9-7-2019")]
        [InlineData("09.7.19", "9-7-2019")]
        [InlineData("1 year", "4-25-2020")]
        [InlineData("6 months", "10-25-2019")]
        [InlineData("56 days", "6-20-2019")]
        [InlineData("27 week", "10-31-2019")]
        [InlineData("27 weeks", "10-31-2019")]
        [InlineData("1 week", "5-2-2019")]
        public void This_CanParse_AllValidDateFormats(string readmeValue, string expectedResult)
        {
            // TODO: Eventually, perhaps, this will need to be adjust to allow for non-American formats. 
            var journalDate = LocalDate.FromDateTime(DateTime.Parse("4-25-2019"));
            var parser = new ReadmeParser(readmeValue, journalDate);
            parser.FormattedExpirationDate.Should().BeEquivalentTo(expectedResult);
        }

        [Theory]
        [InlineData("not valid")]
        [InlineData("notvalid")]
        [InlineData("123")]
        [InlineData("asdf")]
        [InlineData("this is a test")]
        [InlineData("!@#$%^")]
        public void This_ThrowsException_WhenReadmeIsInvalid(string invalidReadme)
        {
            var journalDate = LocalDate.FromDateTime(DateTime.Parse("4-25-2019"));
            Assert.Throws<FormatException>(() => new ReadmeParser(invalidReadme, journalDate));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void This_ThrowsException_WhenReadmeIsNullEmptyOrWhitespace(string invalidReadme)
        {
            var journalDate = LocalDate.FromDateTime(DateTime.Parse("4-25-2019"));
            Assert.Throws<ArgumentException>(() => new ReadmeParser(invalidReadme, journalDate));
        }
    }
}
