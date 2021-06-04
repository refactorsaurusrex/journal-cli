using System;
using System.Collections.Generic;
using FluentAssertions;
using JournalCli.Infrastructure;
using NodaTime;
using Xunit;

namespace JournalCli.Tests
{
    public class NaturalDateTests
    {
        [Theory]
        [MemberData(nameof(GetTestDates))]
        public void NaturalDate_ParsesTextValues_Always(string input, LocalDate expectedDate, RoundTo roundTo)
        {
            var naturalDateAttribute = new NaturalDateAttribute(roundTo);
            var result = naturalDateAttribute.Transform(null, input);
            result.Should().Be(expectedDate);
        }

        public static IEnumerable<object[]> GetTestDates()
        {
            yield return new object[] { "1 year ago", Today.MinusYears(1), RoundTo.StartOfPeriod };
            yield return new object[] { "3 months ago", Today.MinusMonths(3), RoundTo.StartOfPeriod };
            yield return new object[] { "10 days ago", Today.MinusDays(10), RoundTo.EndOfPeriod };
            yield return new object[] { "1051 days ago", Today.MinusDays(1051), RoundTo.EndOfPeriod };
            yield return new object[] { "15 weeks ago", Today.MinusWeeks(15), RoundTo.StartOfPeriod };
            yield return new object[] { "this year", new LocalDate(DateTime.Now.Year, 1, 1), RoundTo.StartOfPeriod };
            yield return new object[] { "this year", Today.MinusDays(1), RoundTo.EndOfPeriod };

            yield return new object[] { "april", new LocalDate(DateTime.Now.Year, 4, 1), RoundTo.StartOfPeriod };
            yield return new object[] { "april", new LocalDate(DateTime.Now.Year, 4, 30), RoundTo.EndOfPeriod };
            yield return new object[] { "last november", new LocalDate(DateTime.Now.Year - 1, 11, 30), RoundTo.EndOfPeriod };
            yield return new object[] { "last november", new LocalDate(DateTime.Now.Year - 1, 11, 1), RoundTo.StartOfPeriod };
            yield return new object[] { "this june", new LocalDate(DateTime.Now.Year, 6, 1), RoundTo.StartOfPeriod };

            yield return new object[] { "april 15", new LocalDate(DateTime.Now.Year, 4, 15), RoundTo.StartOfPeriod };
            yield return new object[] { "december 31 2017", new LocalDate(2017, 12, 31), RoundTo.StartOfPeriod };
            yield return new object[] { "december 2017", new LocalDate(2017, 12, 1), RoundTo.StartOfPeriod };
            yield return new object[] { "January 2017", new LocalDate(2017, 1, 31), RoundTo.EndOfPeriod };

            yield return new object[] { "apr 2020", new LocalDate(2020, 4, 1), RoundTo.StartOfPeriod };
            yield return new object[] { "jan 2019", new LocalDate(2019, 1, 31), RoundTo.EndOfPeriod };

            yield return new object[] { "5/27/2010", new LocalDate(2010, 5, 27), RoundTo.StartOfPeriod };
            yield return new object[] { "9/2/2017", new LocalDate(2017, 9, 2), RoundTo.EndOfPeriod };
            yield return new object[] { "3.16.2013", new LocalDate(2013, 3, 16), RoundTo.EndOfPeriod };
            yield return new object[] { "8-22-2020", new LocalDate(2020, 8, 22), RoundTo.StartOfPeriod };

            yield return new object[] { "2019", new LocalDate(2019, 1, 1), RoundTo.StartOfPeriod };
            yield return new object[] { "2019", new LocalDate(2019, 12, 31), RoundTo.EndOfPeriod };
        }
    }
}
