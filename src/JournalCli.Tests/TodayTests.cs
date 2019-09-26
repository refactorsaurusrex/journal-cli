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
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-11)]
        [InlineData(151)]
        public void MinusDays_ReturnsExpectedDate(int count)
        {
            var result = Today.MinusDays(count);
            var expected = LocalDate.FromDateTime(DateTime.Now.AddDays(count * -1).Date);

            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-11)]
        [InlineData(151)]
        public void MinusMonths_ReturnsExpectedDate(int count)
        {
            var result = Today.MinusMonths(count);
            var expected = LocalDate.FromDateTime(DateTime.Now.AddMonths(count * -1).Date);

            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-11)]
        [InlineData(151)]
        public void MinusYears_ReturnsExpectedDate(int count)
        {
            var result = Today.MinusYears(count);
            var expected = LocalDate.FromDateTime(DateTime.Now.AddYears(count * -1).Date);

            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-11)]
        [InlineData(151)]
        public void PlusDays_ReturnsExpectedDate(int count)
        {
            var result = Today.PlusDays(count);
            var expected = LocalDate.FromDateTime(DateTime.Now.AddDays(count).Date);

            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-11)]
        [InlineData(151)]
        public void PlusMonths_ReturnsExpectedDate(int count)
        {
            var result = Today.PlusMonths(count);
            var expected = LocalDate.FromDateTime(DateTime.Now.AddMonths(count).Date);

            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-11)]
        [InlineData(151)]
        public void PlusYears_ReturnsExpectedDate(int count)
        {
            var result = Today.PlusYears(count);
            var expected = LocalDate.FromDateTime(DateTime.Now.AddYears(count).Date);

            result.Should().Be(expected);
        }
    }
}
