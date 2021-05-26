using System.Management.Automation;
using JetBrains.Annotations;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsData.Sync, "Journal")]
    public class SyncJournalCmdlet : JournalCmdletBase
    {
        protected override void RunJournalCommand()
        {

        }
    }
}