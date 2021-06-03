using System.IO.Abstractions;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "JournalSettings")]
    [OutputType(typeof(UserSettings))]
    public class GetJournalSettingsCmdlet : CmdletBase
    {
        protected override void ProcessRecord()
        {
            var fileSystem = new FileSystem();
            var fileStore = new FileStore<UserSettings>(fileSystem);
            var settings = fileStore.Load();
            WriteObject(settings);
        }
    }
}