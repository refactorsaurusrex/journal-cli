using System;
using System.IO.Abstractions;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Cmdlets
{
    // TODO: "DefaultJournal" or just "Journal"? This will have an impact when named journals are introduced.
    // https://github.com/refactorsaurusrex/journal-cli/issues/23
    [PublicAPI]
    [Cmdlet(VerbsCommon.Open, "Journal")]
    [Alias("oj")]
    public class OpenJournalCmdlet : JournalCmdletBase
    {
        [Parameter(ParameterSetName = "Current")]
        [ValidateSet("CurrentMonth", "CurrentYear", "Root")]
        public string To { get; set; } = "CurrentMonth";

        [Parameter(ParameterSetName = "Date")]
        public DateTime Date { get; set; }

        protected override void RunJournalCommand()
        {
            switch (ParameterSetName)
            {
                case "Current":
                    OpenToCurrent();
                    break;
                case "Date":
                    OpenToDate();
                    break;
                default:
                    throw new NotSupportedException("Unexpected parameter set.");
            }
        }

        private void OpenToDate()
        {
            var year = Journal.YearDirectoryPattern.Format(LocalDate.FromDateTime(Date));
            var month = Journal.MonthDirectoryPattern.Format(LocalDate.FromDateTime(Date));
            var fileSystem = new FileSystem();
            var path = fileSystem.Path.Combine(Location, year, month);

            if (!fileSystem.Directory.Exists(path))
                throw new PSInvalidOperationException("No directory currently exists for the selected period.");

            SystemProcess.Start(path);
        }

        private void OpenToCurrent()
        {
            var fileSystem = new FileSystem();
            string path;
            var year = Journal.YearDirectoryPattern.Format(Today.Date());
            var month = Journal.MonthDirectoryPattern.Format(Today.Date());

            switch (To)
            {
                case "CurrentMonth":
                    path = fileSystem.Path.Combine(Location, year, month);
                    break;
                case "CurrentYear":
                    path = fileSystem.Path.Combine(Location, year);
                    break;
                default:
                    path = fileSystem.Path.Combine(Location);
                    break;
            }

            if (!fileSystem.Directory.Exists(path))
                throw new PSInvalidOperationException("No directory currently exists for the selected period.");

            SystemProcess.Start(path);
        }
    }
}
