using System.IO.Abstractions;
using System.Management.Automation;
using JetBrains.Annotations;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Open, "RandomJournalEntry")]
    public class OpenRandomJournalEntryCmdlet : JournalCmdletBase
    {
        [Parameter]
        public string[] Tags { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            var fileSystem = new FileSystem();
            var journal = Journal.Open(fileSystem, RootDirectory);

            if (Tags == null || Tags.Length == 0)
                journal.OpenRandomEntry();
            else
                journal.OpenRandomEntry(Tags);
        }
    }
}