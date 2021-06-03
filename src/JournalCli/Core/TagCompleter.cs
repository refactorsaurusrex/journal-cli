using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text.RegularExpressions;

namespace JournalCli.Core
{
    public class TagCompleter : IArgumentCompleter
    {
        public IEnumerable<CompletionResult> CompleteArgument(string commandName, string parameterName, string wordToComplete, CommandAst commandAst, IDictionary fakeBoundParameters)
        {
            var cache = new JournalTagCache();
            return string.IsNullOrEmpty(wordToComplete)
                ? cache.Select(x => new CompletionResult(x))
                : cache.Where(x => Regex.IsMatch(x, WildCardToRegex(wordToComplete))).Select(x => new CompletionResult(x));
        }

        private static string WildCardToRegex(string value)
        {
            return "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }
    }
}
