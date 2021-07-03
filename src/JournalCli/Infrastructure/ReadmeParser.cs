using System;
using System.Text.RegularExpressions;
using NodaTime;

namespace JournalCli.Infrastructure
{
    public class ReadmeParser : IReadmeParser
    {
        public enum TimePeriod
        {
            None,
            Days,
            Weeks,
            Months,
            Years
        }
        
        public ReadmeParser(string readmeText)
        {
            if (string.IsNullOrWhiteSpace(readmeText))
                return;

            var parsed = readmeText.ToLocalDate();
            if (parsed.HasValue)
            {
                ExactDate = parsed.Value;
                IsValid = true;
                return;
            }

            if (!readmeText.Contains(" ")) return;

            var readmeArray = readmeText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (!int.TryParse(readmeArray[0], out var duration))
                return;

            PeriodDuration = duration;
            if (Regex.IsMatch(readmeArray[1], @"days?"))
            {
                Period = TimePeriod.Days;
                IsValid = true;
            }
            else if (Regex.IsMatch(readmeArray[1], @"weeks?"))
            {
                Period = TimePeriod.Weeks;
                IsValid = true;
            }
            else if (Regex.IsMatch(readmeArray[1], @"months?"))
            {
                Period = TimePeriod.Months;
                IsValid = true;
            }
            else if (Regex.IsMatch(readmeArray[1], @"years?"))
            {
                Period = TimePeriod.Years;
                IsValid = true;
            }
        }

        public bool IsValid { get; }
        
        public TimePeriod Period { get; }
        
        public LocalDate? ExactDate { get; }

        public int PeriodDuration { get; }

        public ReadmeExpression ToExpression(LocalDate relativeTo) => new(this, relativeTo);
    }
}