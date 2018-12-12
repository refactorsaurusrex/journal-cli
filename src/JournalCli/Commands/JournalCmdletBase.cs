using System.Management.Automation;

namespace JournalCli.Commands
{
    public abstract class JournalCmdletBase : CmdletBase
    {
        private readonly string _error = $"{nameof(RootDirectory)} was not provided and no default location exists. One or the other is required";

        [Parameter]
        public string RootDirectory { get; set; }

        protected string GetResolvedRootDirectory()
        {
            if (!string.IsNullOrEmpty(RootDirectory))
                return ResolvePath(RootDirectory);

            if (!UserSettings.Exists())
                throw new PSInvalidOperationException(_error);

            var settings = UserSettings.Load();
            if (string.IsNullOrEmpty(settings.DefaultJournalRoot))
                throw new PSInvalidOperationException(_error);

            return settings.DefaultJournalRoot;
        }
    }
}