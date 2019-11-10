using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using JournalCli.Core;
using NodaTime;

namespace JournalCli.Infrastructure
{
    internal static class Extensions
    {
        public static bool ContainsAll<T>(this IEnumerable<T> target, IEnumerable<T> matches) => matches.All(target.Contains);

        public static string ToJournalEntryFileName(this LocalDate date) => date.ToString(Journal.FileNameWithExtensionPattern.PatternText, CultureInfo.CurrentCulture);

        public static string Wrap(this string text, int width = 120)
        {
            if (text.Length <= width)
                return text;

            const char delimiter = ' ';
            var words = text.Split(delimiter);
            var allLines = words.Skip(1).Aggregate(words.Take(1).ToList(), (lines, word) =>
            {
                if (lines.Last().Length + word.Length >= width - 1) // Minus 1, to allow for newline char
                    lines.Add(word);
                else
                    lines[lines.Count - 1] += delimiter + word;
                return lines;
            });

            return string.Join(Environment.NewLine, allLines.ToArray());
        }

        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        public static string ToChoiceString(this IsoDayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case IsoDayOfWeek.Monday:
                    return "&Monday";
                case IsoDayOfWeek.Tuesday:
                    return "&Tuesday";
                case IsoDayOfWeek.Wednesday:
                    return "&Wednesday";
                case IsoDayOfWeek.Thursday:
                    return "T&hursday";
                case IsoDayOfWeek.Friday:
                    return "&Friday";
                case IsoDayOfWeek.Saturday:
                    return "S&aturday";
                case IsoDayOfWeek.Sunday:
                    return "S&unday";
                default:
                    throw new ArgumentOutOfRangeException(nameof(dayOfWeek), dayOfWeek, null);
            }
        }
    }
}
