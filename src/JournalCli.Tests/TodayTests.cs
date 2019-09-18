using System;
using AutoFixture;
using FluentAssertions;
using JournalCli.Infrastructure;
using NodaTime;
using Xunit;

namespace JournalCli.Tests
{
    public class TodayTests
    {
        [Fact]
        public void MinusDays_ReturnsExpectedDate()
        {
            var fixture = new Fixture();
            var count = fixture.Create<int>();
            var result = Today.MinusDays(count);
            var expected = LocalDate.FromDateTime(DateTime.Now.AddDays(count * -1).Date);

            result.Should().Be(expected);
        }

        [Fact]
        public void MinusMonths_ReturnsExpectedDate()
        {
            var fixture = new Fixture();
            var count = fixture.Create<int>();
            var result = Today.MinusMonths(count);
            var expected = LocalDate.FromDateTime(DateTime.Now.AddMonths(count * -1).Date);

            result.Should().Be(expected);
        }

        [Fact]
        public void MinusYears_ReturnsExpectedDate()
        {
            var fixture = new Fixture();
            var count = fixture.Create<int>();
            var result = Today.MinusYears(count);
            var expected = LocalDate.FromDateTime(DateTime.Now.AddYears(count * -1).Date);

            result.Should().Be(expected);
        }

        [Fact]
        public void MinusDays_ThrowsArgumentException_WhenCountIsLessThanOne()
        {
            Assert.Throws<ArgumentException>(() => Today.MinusDays(-1));
        }

        [Fact]
        public void MinusMonths_ThrowsArgumentException_WhenCountIsLessThanOne()
        {
            Assert.Throws<ArgumentException>(() => Today.MinusMonths(-1));
        }

        [Fact]
        public void MinusYears_ThrowsArgumentException_WhenCountIsLessThanOne()
        {
            Assert.Throws<ArgumentException>(() => Today.MinusYears(-1));
        }
    }
}
