using System;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsData.Sync, "Journal")]
    public class SyncJournalCmdlet : JournalSyncCmdletBase
    {
        protected override void EndProcessing()
        {
            base.EndProcessing();
            WriteWarning("JournalCli sync is in beta and has not been thoroughly tested. " +
                "Please report bugs to https://github.com/refactorsaurusrex/journal-cli/issues.".Wrap(WrapWidth));
        }
    }
}
