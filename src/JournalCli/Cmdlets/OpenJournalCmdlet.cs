using System.IO.Abstractions;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    // TODO: "DefaultJournal" or just "Journal"? This will have an impact when named journals are introduced.
    [PublicAPI]
    [Cmdlet(VerbsCommon.Open, "Journal")]
    [Alias("oj")]
    public class OpenJournalCmdlet : JournalCmdletBase
    {
        [Parameter]
        [ValidateSet("CurrentMonth", "CurrentYear", "Root")]
        public string To { get; set; } = "CurrentMonth";

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            
            var fileSystem = new FileSystem();
            string path;
            var year = Journal.YearDirectoryPattern.Format(Today.Date());
            var month = Journal.MonthDirectoryPattern.Format(Today.Date());

            switch (To)
            {
                case "CurrentMonth":
                    path = fileSystem.Path.Combine(RootDirectory, year, month);
                    break;
                case "CurrentYear":
                    path = fileSystem.Path.Combine(RootDirectory, year);
                    break;
                default:
                    path = fileSystem.Path.Combine(RootDirectory);
                    break;
            }

            if (!fileSystem.Directory.Exists(path))
                throw new PSInvalidOperationException("No directory currently exists for the selected periods.");

            var systemProcess = new SystemProcess();
            systemProcess.Start(path);
        }
    }
}
