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
            var encryptedStore = EncryptedStoreFactory.Create<UserSettings>();
            var settings = UserSettings.Load(encryptedStore);
            settings.DefaultJournalRoot = Location;
            settings.Save(encryptedStore);
        }
    }
}
