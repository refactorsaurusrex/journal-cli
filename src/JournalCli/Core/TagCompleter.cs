using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text.RegularExpressions;
using JournalCli.Infrastructure;

namespace JournalCli.Core
{
    public class TagCompleter : IArgumentCompleter
    {
        private readonly JournalTagCache _cache = new();
        
        public IEnumerable<CompletionResult> CompleteArgument(string commandName, string parameterName, string wordToComplete, CommandAst commandAst, IDictionary fakeBoundParameters)
        {
            return string.IsNullOrEmpty(wordToComplete)
                ? _cache.Select(x => new CompletionResult(x))
                : _cache.Where(x => Regex.IsMatch(x, wordToComplete.WildCardsToRegex())).Select(x => new CompletionResult(x));
        }
    }
}
