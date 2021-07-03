using System.Globalization;
using NodaTime;

namespace JournalCli.Infrastructure
{
    // Tests for this class are included in the ReadmeParserTests class.
    public class ReadmeExpression
    {
        public static ReadmeExpression Empty() => new();

        private ReadmeExpression() => FormattedExpirationDate = null;

        public ReadmeExpression(IReadmeParser readmeParser, LocalDate relativeTo)
        {
            if (!readmeParser.IsValid)
            {
                FormattedExpirationDate = null;
                return;
            }
            
            if (readmeParser.ExactDate.HasValue)
            {
                ExpirationDate = readmeParser.ExactDate.Value;
                FormattedExpirationDate = ExpirationDate.Value.ToString("d", CultureInfo.CurrentCulture);
                return;
            }

            switch (readmeParser.Period)
            {
                case ReadmeParser.TimePeriod.Days:
                    ExpirationDate = relativeTo.PlusDays(readmeParser.PeriodDuration);
                    FormattedExpirationDate = ExpirationDate.Value.ToString("d", CultureInfo.CurrentCulture);
                    break;
                case ReadmeParser.TimePeriod.Weeks:
                    ExpirationDate = relativeTo.PlusWeeks(readmeParser.PeriodDuration);
                    FormattedExpirationDate = ExpirationDate.Value.ToString("d", CultureInfo.CurrentCulture);
                    break;
                case ReadmeParser.TimePeriod.Months:
                    ExpirationDate = relativeTo.PlusMonths(readmeParser.PeriodDuration);
                    FormattedExpirationDate = ExpirationDate.Value.ToString("d", CultureInfo.CurrentCulture);
                    break;
                case ReadmeParser.TimePeriod.Years:
                    ExpirationDate = relativeTo.PlusYears(readmeParser.PeriodDuration);
                    FormattedExpirationDate = ExpirationDate.Value.ToString("d", CultureInfo.CurrentCulture);
                    break;
                default:
                    FormattedExpirationDate = null;
                    break;
            }
        }

        public LocalDate? ExpirationDate { get; }
        
        public string FormattedExpirationDate { get; }
    }
}
