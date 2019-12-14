using System;
using FluentAssertions;
using JournalCli.Infrastructure;
using NodaTime;
using Xunit;

namespace JournalCli.Tests
{
    public class DateRangeTests
    {
        [Theory]
        [InlineData("12-1-2019", "11-30-2019")]
        [InlineData("12-1-2019", "12-1-2019")]
        public void DateTimeConstructor_ThrowsException_WhenFromDateIsLaterThanToDate(DateTime from, DateTime to)
        {
            Assert.Throws<ArgumentException>(() => new DateRange(from, to));
        }

        [Theory]
        [InlineData("12-1-2019", "11-30-2019")]
        [InlineData("12-1-2019", "12-1-2019")]
        public void LocalDateConstructor_ThrowsException_WhenFromDateIsLaterThanToDate(DateTime from, DateTime to)
        {
            Assert.Throws<ArgumentException>(() => new DateRange(LocalDate.FromDateTime(from), LocalDate.FromDateTime(to)));
        }

        [Theory]
        [InlineData("12-1-2019", "12-30-2019", "12-15-2019")]
        [InlineData("12-1-2019", "12-30-2019", "12-1-2019")]
        [InlineData("12-1-2019", "12-30-2019", "12-30-2019")]
        public void Includes_ReturnsTrue_WhenDateIsWithinRange(DateTime from, DateTime to, DateTime targetDate)
        {
            var range = new DateRange(from, to);
            range.Includes(LocalDate.FromDateTime(targetDate)).Should().BeTrue();
        }

        [Theory]
        [InlineData("12-1-2019", "12-30-2019", "11-30-2019")]
        [InlineData("12-1-2019", "12-30-2019", "12-31-2019")]
        [InlineData("12-1-2019", "12-30-2019", "12-30-2018")]
        public void Includes_ReturnsFalse_WhenDateIsOutOfRange(DateTime from, DateTime to, DateTime targetDate)
        {
            var range = new DateRange(from, to);
            range.Includes(LocalDate.FromDateTime(targetDate)).Should().BeFalse();
        }

        [Theory]
        [InlineData("1-2-2017", "3-3-2017", "2017.01.02-2017.03.03.md")]
        [InlineData("4-25-2019", "12-31-2019", "2019.04.25-2019.12.31.md")]
        public void ToJournalEntryFileName_ReturnsValidName(DateTime from, DateTime to, string expectedFileName)
        {
            var range = new DateRange(from, to);
            range.ToJournalEntryFileName().Should().Be(expectedFileName);
        }
    }
}
