using System.Management.Automation;
using JetBrains.Annotations;

namespace JournalCli.Commands
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Open, "RandomJournalEntry")]
    public class OpenRandomJournalEntryCmdlet : JournalCmdletBase
    {
        [Parameter]
        public string[] Tags { get; set; }

        protected override void ProcessRecord()
        {
            var root = GetResolvedRootDirectory();

            if (Tags.Length == 0)
                Journal.OpenRandomEntry(root);
            else
                Journal.OpenRandomEntry(root, Tags);
        }
    }
}