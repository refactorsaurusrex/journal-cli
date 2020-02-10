using System;
using System.Text.RegularExpressions;

namespace JournalCli.Infrastructure
{
    internal class HeaderValidator
    {
        private const string ValidationExpression = @"^#{1,6} .+";

        /// <summary>
        /// Verifies that the specified string qualifies as a markdown header. If not, throws an argument exception.
        /// </summary>
        /// <param name="header">The header text to be examined.</param>
        public static void ValidateOrThrow(string header)
        {
            if (!Regex.IsMatch(header, ValidationExpression))
                throw new ArgumentException("Header must start with between 1 and 6 '#' characters, followed by a single space and the header text.");
        }

        public static bool IsValid(string header) => Regex.IsMatch(header, ValidationExpression);
    }
}
