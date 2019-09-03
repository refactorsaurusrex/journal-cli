using System.Management.Automation;
using JetBrains.Annotations;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "DefaultJournalLocation")]
    [OutputType(typeof(string))]
    public class GetDefaultJournalLocationCmdlet : CmdletBase
    {
        protected override void ProcessRecord()
        {
            var encryptedStore = EncryptedStoreFactory.Create<UserSettings>();
            var settings = UserSettings.Load(encryptedStore);
            WriteObject(settings.DefaultJournalRoot);
        }
    }
}