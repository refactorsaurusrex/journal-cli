using System.IO.Abstractions;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Set, "JournalSettings")]
    public class SetJournalSettingsCmdlet : CmdletBase
    {
        [Parameter]
        [ValidateNotNullOrEmpty]
        public string Location { get; set; }

        protected override void ProcessRecord()
        {
            if (!string.IsNullOrWhiteSpace(Location))
                Location = ResolvePath(Location);
            var fileSystem = new FileSystem();
            var store = new FileStore<UserSettings>(fileSystem);
            var settings = store.Load();
            settings.DefaultJournalRoot = Location;
            store.Save(settings);
        }
    }
}
