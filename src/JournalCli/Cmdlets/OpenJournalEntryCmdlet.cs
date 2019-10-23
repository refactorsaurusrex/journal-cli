using System;
using System.Globalization;
using System.IO.Abstractions;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Open, "JournalEntry")]
    public class OpenJournalEntryCmdlet : JournalCmdletBase
    {
        // TODO: open entry via date or natural language input

        [Parameter(ValueFromPipeline = true, Position = 0, ParameterSetName = "Name")]
        public string EntryName { get; set; }

        [Parameter(ValueFromPipeline = true, Position = 0, ParameterSetName = "Date")]
        public DateTime Date { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var fileSystem = new FileSystem();
            string path;

            if (ParameterSetName == "Name")
            {
                if (!EntryName.EndsWith(".md"))
                    EntryName += ".md";
                var date = Journal.FileNameWithExtensionPattern.Parse(EntryName).Value;
                var year = date.ToString(Journal.YearDirectoryPattern.PatternText, CultureInfo.CurrentCulture);
                var month = date.ToString(Journal.MonthDirectoryPattern.PatternText, CultureInfo.CurrentCulture);
                path = fileSystem.Path.Combine(Location, year, month, EntryName);
            }
            else if (ParameterSetName == "Date")
            {
                var date = LocalDate.FromDateTime(Date);
                var year = date.ToString(Journal.YearDirectoryPattern.PatternText, CultureInfo.CurrentCulture);
                var month = date.ToString(Journal.MonthDirectoryPattern.PatternText, CultureInfo.CurrentCulture);
                var name = LocalDate.FromDateTime(Date).ToJournalEntryFileName();
                path = fileSystem.Path.Combine(Location, year, month, name);
            }
            else
            {
                throw new NotSupportedException();
            }

            if (!fileSystem.File.Exists(path))
                throw new PSInvalidOperationException("That journal entry doesn't currently exist.");

            var systemProcess = new SystemProcess();
            systemProcess.Start(path);
        }
    }
}