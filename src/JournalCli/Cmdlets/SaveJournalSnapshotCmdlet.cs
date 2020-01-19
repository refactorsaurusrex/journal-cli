using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsData.Save, "JournalSnapshot")]
    [Alias("sjs")]
    public class SaveJournalSnapshotCmdlet : JournalCmdletBase
    {
        [Parameter(Position = 0)]
        [ValidateLength(5, 60)]
        public string Message { get; set; }

        protected override void RunJournalCommand()
        {
            if (string.IsNullOrEmpty(Message))
                Commit(GitCommitType.Manual);
            else
                Commit(Message);
        }
    }
}
