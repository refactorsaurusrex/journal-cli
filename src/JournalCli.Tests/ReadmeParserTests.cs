using System;
using System.Diagnostics.CodeAnalysis;
using FakeItEasy;
using FluentAssertions;
using JournalCli.Infrastructure;
using NodaTime;
using Xunit;

namespace JournalCli.Tests
{
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public class ReadmeParserTests
    {
        [Fact]
        public void EmptyExpression_ReturnsNullProperties_Always()
        {
            var exp = ReadmeExpression.Empty();
            exp.ExpirationDate.Should().BeNull();
            exp.FormattedExpirationDate.Should().BeNull();
        }
        
        [Fact]
        public void ExpirationDate_IsNull_WhenReadmeParserIsNotValid()
        {
            var readme = A.Fake<IReadmeParser>();
            A.CallTo(() => readme.IsValid).Returns(false);
            var exp = readme.ToExpression(new LocalDate(2011, 1, 1));
            exp.ExpirationDate.Should().BeNull();
        }
        
        [Fact]
        public void FormattedExpirationDate_IsNull_WhenReadmeParserIsNotValid()
        {
            var readme = A.Fake<IReadmeParser>();
            A.CallTo(() => readme.IsValid).Returns(false);
            var exp = readme.ToExpression(new LocalDate(2011, 1, 1));
            exp.FormattedExpirationDate.Should().BeNull();
        }
        
        [Theory]
        [InlineData("9/7/2019", "9/7/2019")]
        [InlineData("9/7/19", "9/7/2019")]
        [InlineData("9.7.2019", "9/7/2019")]
        [InlineData("09/07/2019", "9/7/2019")]
        [InlineData("9/07/2019", "9/7/2019")]
        [InlineData("09/7/2019", "9/7/2019")]
        [InlineData("09\\7\\2019", "9/7/2019")]
        [InlineData("9-7-19", "9/7/2019")]
        [InlineData("09-07-19", "9/7/2019")]
        [InlineData("09.7.19", "9/7/2019")]
        [InlineData("1 year", "4/25/2020")]
        [InlineData("5 years", "4/25/2024")]
        [InlineData("6 months", "10/25/2019")]
        [InlineData("56 days", "6/20/2019")]
        [InlineData("27 week", "10/31/2019")]
        [InlineData("27 weeks", "10/31/2019")]
        [InlineData("1 week", "5/2/2019")]
        public void This_CanParse_AllValidDateFormats(string readmeValue, string expectedResult)
        {
            // Eventually, perhaps, this will need to be adjust to allow for non-American formats. 
            var journalDate = LocalDate.FromDateTime(DateTime.Parse("4-25-2019"));
            var parser = new ReadmeParser(readmeValue);
            parser.IsValid.Should().BeTrue();
            var exp = parser.ToExpression(journalDate);
            exp.FormattedExpirationDate.Should().BeEquivalentTo(expectedResult);
            exp.ExpirationDate.Should().Be(LocalDate.FromDateTime(DateTime.Parse(expectedResult)));
        }

        [Theory]
        [InlineData("not valid")]
        [InlineData("notvalid")]
        [InlineData("123")]
        [InlineData("asdf")]
        [InlineData("this is a test")]
        [InlineData("!@#$%^")]
        [InlineData("2.8 years")]
        [InlineData("1 thing")]
        [InlineData("13 horses")]
        [InlineData("blah")]
        [InlineData("Merry christmas!")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void This_IsNotValid_WhenReadmeFormatIsInvalid(string invalidReadme)
        {
            var parser = new ReadmeParser(invalidReadme);
            parser.IsValid.Should().BeFalse();
        }
    }
}
