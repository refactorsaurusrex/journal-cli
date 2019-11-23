using System;
using System.Globalization;
using JournalCli.Core;
using NodaTime;

namespace JournalCli.Infrastructure
{
    internal class DateRange
    {
        public DateRange(LocalDate from, LocalDate to)
        {
            From = from;
            To = to;
        }

        public DateRange(DateTime from, DateTime to)
        {
            From = LocalDate.FromDateTime(from);
            To = LocalDate.FromDateTime(to);
        }

        public LocalDate From { get; }

        public LocalDate To { get; }

        public bool Includes(LocalDate date) => date >= From && date <= To;

        public string ToJournalEntryFileName()
        {
            var pattern = Journal.FileNamePattern.PatternText;
            var culture = CultureInfo.CurrentCulture;
            return $"{From.ToString(pattern, culture)}-{To.ToString(pattern, culture)}.md";
        }
    }
}
