using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Rename, "JournalTag", ConfirmImpact = ConfirmImpact.High)]
    public class RenameJournalTagCmdlet : JournalCmdletBase
    {
        [Parameter]
        public SwitchParameter DryRun { get; set; }

        [Parameter(Mandatory = true)]
        public string OldName { get; set; }

        [Parameter(Mandatory = true)]
        public string NewName { get; set; }

        protected override void RunJournalCommand()
        {
            if (!DryRun && !YesOrNo($"Are you sure you want to rename all '{OldName}' tags to '{NewName}'?"))
                return;

            var journal = OpenJournal();
            ICollection<string> effectedEntries;

            if (DryRun)
            {
                effectedEntries = journal.RenameTagDryRun(OldName);
                var count = effectedEntries.Count == 1 ? "1 entry:" : $"{effectedEntries.Count} entries:";
                var warning = $"All instances of the tag '{OldName}' would be replaced with '{NewName}' in the following {count} ";
                WriteHostInverted(warning);
            }
            else
            {
                Commit(GitCommitType.PreRenameTag);
                effectedEntries = journal.RenameTag(OldName, NewName);
                Commit(GitCommitType.PostRenameTag);

                var notice = $"The tag '{OldName}' has been successfully replaced with '{NewName}' in all {effectedEntries.Count} entries.";
                WriteHostInverted(notice);
            }

            // Project into a new sequence of POCOs, instead of a list of strings. This allow users to manipulate the content
            // with `Format-Wide -Column 6` for example.
            WriteObject(effectedEntries.Select(x => new { EntryName = x }), true);
        }
    }
}
