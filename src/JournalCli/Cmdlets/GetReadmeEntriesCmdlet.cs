using System;
using System.IO.Abstractions;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "ReadMeEntries")]
    [OutputType(typeof(ReadmeJournalEntryCollection))]
    public class GetReadmeEntriesCmdlet : JournalCmdletBase
    {
        [Parameter(DontShow = true)]
        public SwitchParameter IncludeFuture { get; set; }

        [Parameter(ParameterSetName = "Range")]
        [ValidateSet("Years", "Months", "Days")]
        public string Period { get; set; }

        [Parameter(ParameterSetName = "Range")]
        public int Duration { get; set; }

        [Parameter(ParameterSetName = "All")]
        public SwitchParameter All { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var fileSystem = new FileSystem();
            var ioFactory = new JournalReaderWriterFactory(fileSystem, RootDirectory);
            var markdownFiles = new MarkdownFiles(fileSystem, RootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, new SystemProcess());

            var readMeEntries = journal.GetReadmeEntries(EarliestDate, IncludeFuture);
            WriteObject(readMeEntries, true);
        }

        private NodaTime.LocalDate EarliestDate
        {
            get
            {
                if (All)
                    return new NodaTime.LocalDate();

                switch (Period)
                {
                    case "Days":
                        return Today.MinusDays(Duration);
                    case "Months":
                        return Today.MinusMonths(Duration);
                    case "Years":
                        return Today.MinusYears(Duration);
                    default:
                        throw new NotSupportedException();
                }
            }
        }
    }
}