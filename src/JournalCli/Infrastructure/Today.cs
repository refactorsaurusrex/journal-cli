using System;
using NodaTime;
using NodaTime.Extensions;

namespace JournalCli.Infrastructure
{
    internal class Today
    {
        public static LocalDate Date() => SystemClock.Instance.InTzdbSystemDefaultZone().GetCurrentLocalDateTime().Date;

        public static LocalDate MinusDays(int count)
        {
            ValidateCount(count);
            var periodBuilder = new PeriodBuilder { Days = count };
            return SubtractFromToday(periodBuilder.Build());
        }

        public static LocalDate MinusMonths(int count)
        {
            ValidateCount(count);
            var periodBuilder = new PeriodBuilder { Months = count };
            return SubtractFromToday(periodBuilder.Build());
        }

        public static LocalDate MinusYears(int count)
        {
            ValidateCount(count);
            var periodBuilder = new PeriodBuilder { Years = count };
            return SubtractFromToday(periodBuilder.Build());
        }

        private static void ValidateCount(int count)
        {
            if (count < 1)
                throw new ArgumentException($"'{nameof(count)}' must be greater than or equal to 1.", nameof(count));
        }

        private static LocalDate SubtractFromToday(Period period)
        {
            var now = LocalDate.FromDateTime(DateTime.Now);
            return now.Minus(period);
        }
    }
}
