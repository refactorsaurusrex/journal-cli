using System;
using System.Text.RegularExpressions;

namespace JournalCli.Infrastructure
{
    public class HeaderValidator
    {
        private const string ValidationExpression = @"^#{1,6} .+";
        public static bool IsValid(string header) => Regex.IsMatch(header, ValidationExpression);
    }
}
