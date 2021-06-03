using System.IO.Abstractions;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsData.Edit, "JournalSettings")]
    public class EditJournalSettingsCmdlet : CmdletBase
    {
        protected override void ProcessRecord()
        {
            var fileSystem = new FileSystem();
            var store = new FileStore<UserSettings>(fileSystem);
            SystemProcess.Start(store.FilePath);
        }
    }
}