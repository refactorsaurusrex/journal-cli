using System.Management.Automation;
using JetBrains.Annotations;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsData.Save, "JournalSnapshot")]
    public class SaveJournalSnapshotCmdlet : JournalCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0)]
        [ValidateLength(5, 60)]
        public string Message { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            Commit(Message);
        }
    }
}
