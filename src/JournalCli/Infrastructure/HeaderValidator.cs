using System;
using System.Text.RegularExpressions;

namespace JournalCli.Infrastructure
{
    public class HeaderValidator
    {
        private const string ValidationExpression = @"^#{1,6} .+";
        public const string ErrorMessage = "Header must start with between 1 and 6 '#' characters, followed by a single space and the header text.";
        public static bool IsValid(string header) => Regex.IsMatch(header, ValidationExpression);
    }
}
