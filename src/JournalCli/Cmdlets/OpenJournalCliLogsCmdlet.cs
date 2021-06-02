using System.IO;
using System.Management.Automation;
using JetBrains.Annotations;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Open, "JournalCliLogs")]
    public class OpenJournalCliLogsCmdlet : CmdletBase
    {
        protected override void ProcessRecord()
        {
            if (!Directory.Exists(LogsDirectory))
            {
                WriteHostInverted("No logs have been created yet.");
                return;
            }

            SystemProcess.Start(LogsDirectory);
        }
    }
}
