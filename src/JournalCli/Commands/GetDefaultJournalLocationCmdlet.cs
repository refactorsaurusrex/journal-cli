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

            var settings = UserSettings.Load();
            WriteObject($"Default journal root directory: {settings.DefaultJournalRoot}");
        }
    }
}