using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Set, "JournalDefaultLocation")]
    [Alias("Set-DefaultJournalLocation")]
    public class SetJournalDefaultLocationCmdlet : CmdletBase
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Location { get; set; }

        protected override void ProcessRecord()
        {
            if (MyInvocation.InvocationName == "Set-DefaultJournalLocation")
                WriteWarning("'Set-DefaultJournalLocation' is obsolete and will be removed in a future release. Use 'Set-JournalDefaultLocation' instead.");

            var encryptedStore = EncryptedStoreFactory.Create<UserSettings>();
            var settings = UserSettings.Load(encryptedStore);
            settings.DefaultJournalRoot = Location;
            settings.Save(encryptedStore);
        }
    }
}
