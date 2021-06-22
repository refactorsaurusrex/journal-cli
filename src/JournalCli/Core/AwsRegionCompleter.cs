using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text.RegularExpressions;
using JournalCli.Infrastructure;

namespace JournalCli.Core
{
    public class AwsRegionCompleter : IArgumentCompleter
    {
        private readonly List<string> _regions;
        
        public AwsRegionCompleter() => _regions = Amazon.RegionEndpoint.EnumerableAllRegions.Select(x => x.SystemName).ToList();

        public IEnumerable<CompletionResult> CompleteArgument(string commandName, string parameterName, string wordToComplete, CommandAst commandAst, IDictionary fakeBoundParameters)
        {
            return _regions.Where(x => Regex.IsMatch(x, wordToComplete.WildCardsToRegex())).Select(x => new CompletionResult(x));
        }
    }
}