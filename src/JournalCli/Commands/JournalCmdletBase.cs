using System.Management.Automation;

namespace JournalCli.Commands
{
    public abstract class JournalCmdletBase : CmdletBase
    {
        [Parameter]
        public string RootDirectory { get; set; }

        protected string GetResolvedRootDirectory()
        {
            if (!string.IsNullOrEmpty(RootDirectory))
                return ResolvePath(RootDirectory);

            if (!UserSettings.Exists())
                return ResolvePath(".");

            var settings = UserSettings.Load();
            return string.IsNullOrEmpty(settings.DefaultJournalRoot) ? ResolvePath(".") : settings.DefaultJournalRoot;
        }
    }
}