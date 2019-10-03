using System.Collections.Generic;
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
    }
}
