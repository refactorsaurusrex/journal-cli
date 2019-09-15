using System;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.New, "JournalEntry")]
    [Alias("nj")]
    public class NewJournalEntryCmdlet : JournalCmdletBase
    {
        [Parameter]
        public int DateOffset { get; set; }

        [Parameter]
        public string[] Tags { get; set; }

        [Parameter]
        public string Readme { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            
            var entryDate = DateTime.Today.AddDays(DateOffset);
            var fileSystem = new FileSystem();
            var readerFactory = new JournalReaderFactory(fileSystem);
            var journal = Journal.Open(readerFactory, fileSystem, new SystemProcess(), RootDirectory);
            journal.CreateNewEntry(entryDate, Tags, Readme);
        }
    }
}