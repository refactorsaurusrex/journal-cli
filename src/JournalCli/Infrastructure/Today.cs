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
            var periodBuilder = new PeriodBuilder { Days = count };
            return SubtractFromToday(periodBuilder.Build());
        }

        public static LocalDate MinusMonths(int count)
        {
            var periodBuilder = new PeriodBuilder { Months = count };
            return SubtractFromToday(periodBuilder.Build());
        }

        public static LocalDate MinusWeeks(int count)
        {
            var periodBuilder = new PeriodBuilder { Weeks = count };
            return SubtractFromToday(periodBuilder.Build());
        }

        public static LocalDate MinusYears(int count)
        {
            var periodBuilder = new PeriodBuilder { Years = count };
            return SubtractFromToday(periodBuilder.Build());
        }

        public static LocalDate PlusDays(int count)
        {
            var periodBuilder = new PeriodBuilder { Days = count };
            return AddToToday(periodBuilder.Build());
        }

        public static LocalDate PlusMonths(int count)
        {
            var periodBuilder = new PeriodBuilder { Months = count };
            return AddToToday(periodBuilder.Build());
        }

        public static LocalDate PlusYears(int count)
        {
            var periodBuilder = new PeriodBuilder { Years = count };
            return AddToToday(periodBuilder.Build());
        }

        private static LocalDate SubtractFromToday(Period period)
        {
            var now = LocalDate.FromDateTime(DateTime.Now);
            return now.Minus(period);
        }

        private static LocalDate AddToToday(Period period)
        {
            var now = LocalDate.FromDateTime(DateTime.Now);
            return now.Plus(period);
        }
    }
}
