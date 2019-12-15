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
            if (from >= to)
                throw new ArgumentException("'From' date must be earlier than 'To' date.");

            From = from;
            To = to;
        }

        public DateRange(DateTime from, DateTime to)
        {
            if (from >= to)
                throw new ArgumentException("'From' date must be earlier than 'To' date.");

            From = LocalDate.FromDateTime(from);
            To = LocalDate.FromDateTime(to);
        }

        public DateRange(string from, string to)
        {
            var fromParsed = DateTime.Parse(from);
            var toParsed = DateTime.Parse(to);

            if (fromParsed >= toParsed)
                throw new ArgumentException("'From' date must be earlier than 'To' date.");

            From = LocalDate.FromDateTime(fromParsed);
            To = LocalDate.FromDateTime(toParsed);
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

        public override string ToString() => $"{From} to {To}";
    }
}
