using System;
using System.Globalization;
using System.Text.RegularExpressions;
using NodaTime;

namespace JournalCli.Infrastructure
{
    internal class ReadmeParser
    {
        public ReadmeParser(string readmeText, LocalDate journalDate)
        {
            if (readmeText.Contains(" "))
            {
                var readmeArray = readmeText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                var duration = int.Parse(readmeArray[0]);

                LocalDate expiration;
                string period;

                if (Regex.IsMatch(readmeArray[1], @"days?"))
                {
                    expiration = journalDate.PlusDays(duration);
                    period = duration == 1 ? "day" : "days";
                }
                else if (Regex.IsMatch(readmeArray[1], @"weeks?"))
                {
                    expiration = journalDate.PlusWeeks(duration);
                    period = duration == 1 ? "week" : "weeks";
                }
                else if (Regex.IsMatch(readmeArray[1], @"months?"))
                {
                    expiration = journalDate.PlusMonths(duration);
                    period = duration == 1 ? "month" : "months";
                }
                else if (Regex.IsMatch(readmeArray[1], @"years?"))
                {
                    expiration = journalDate.PlusYears(duration);
                    period = duration == 1 ? "year" : "years";
                }
                else
                {
                    throw new NotSupportedException();
                }

                ExpirationDate = expiration;
                FormattedExpirationDate = expiration.ToString("M-d-yyyy", CultureInfo.CurrentCulture);
                FrontMatterValue = $"{duration} {period}";
            }
            else
            {
                readmeText = readmeText.Replace('\\', '/'); // Just in case these were used.
                var dt = DateTime.Parse(readmeText);
                ExpirationDate = LocalDate.FromDateTime(dt);
                FrontMatterValue = FormattedExpirationDate = dt.ToString("M-d-yyyy", CultureInfo.CurrentCulture);
            }
        }

        // TODO: Test that ExpirationDate is correct value
        public LocalDate ExpirationDate { get; }

        public string FormattedExpirationDate { get; }

        public string FrontMatterValue { get; }
    }
}
