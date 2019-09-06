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

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            
            var entryDate = DateTime.Today.AddDays(DateOffset);
            var journal = Journal.Open(new FileSystem(), new SystemProcess(), RootDirectory);
            journal.CreateNewEntry(entryDate, Tags);
        }
    }
}