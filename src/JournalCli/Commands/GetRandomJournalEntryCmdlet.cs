using System.Management.Automation;
using JetBrains.Annotations;

namespace JournalCli.Commands
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "RandomJournalEntry")]
    public class GetRandomJournalEntryCmdlet : JournalCmdletBase
    {
        protected override void ProcessRecord()
        {
            var root = GetResolvedRootDirectory();
            Journal.OpenRandomJournalEntry(root);
        }
    }
}