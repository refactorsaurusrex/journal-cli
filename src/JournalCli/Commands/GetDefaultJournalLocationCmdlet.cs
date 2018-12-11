using System.IO;
using System.Management.Automation;
using JetBrains.Annotations;

namespace JournalCli.Commands
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "DefaultJournalLocation")]
    public class GetDefaultJournalLocationCmdlet : CmdletBase
    {
        protected override void ProcessRecord()
        {
            if (!UserSettings.Exists())
                return;

            var settings = File.ReadAllText(UserSettings.GetStorageLocation());
            WriteObject(settings);
        }
    }
}