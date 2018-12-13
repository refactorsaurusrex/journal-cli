using System.Management.Automation;
using JetBrains.Annotations;

namespace JournalCli.Commands
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Set, "DefaultJournalLocation")]
    public class SetDefaultJournalLocationCmdlet : CmdletBase
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Location { get; set; }

        protected override void ProcessRecord()
        {
            var settings = UserSettings.Load();
            settings.DefaultJournalRoot = Location;
            settings.Save();
        }
    }
}
