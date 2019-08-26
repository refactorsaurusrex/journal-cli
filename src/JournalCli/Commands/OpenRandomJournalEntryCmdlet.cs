using System.Management.Automation;
using JetBrains.Annotations;

namespace JournalCli.Commands
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

            if (Tags.Length == 0)
                Journal.OpenRandomEntry(RootDirectory);
            else
                Journal.OpenRandomEntry(RootDirectory, Tags);
        }
    }
}