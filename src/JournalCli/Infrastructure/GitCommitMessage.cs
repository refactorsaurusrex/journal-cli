using System;

namespace JournalCli.Infrastructure
{
    internal class GitCommitMessage
    {
        public static string Get(GitCommitType commitType)
        {
            switch (commitType)
            {
                default:
                    throw new NotSupportedException($"The commit type '{commitType}' is not currently supported");
                case GitCommitType.PreOpenJournalEntry:
                    return Pre + OpenForEdit;

                case GitCommitType.PostOpenJournalEntry:
                    return Post + OpenForEdit;

                case GitCommitType.PreAppendJournalEntry:
                    return Pre + AppendJournalEntry;

                case GitCommitType.PostAppendJournalEntry:
                    return Post + AppendJournalEntry;

                case GitCommitType.PreNewJournalEntry:
                    return Pre + NewJournalEntry;

                case GitCommitType.PostNewJournalEntry:
                    return Post + NewJournalEntry;

                case GitCommitType.PreRenameTag:
                    return Pre + RenameTag;

                case GitCommitType.PostRenameTag:
                    return Post + RenameTag;

                case GitCommitType.Manual:
                    return "Manual snapshot";
            }
        }

        private const string Pre = "PRE:  ";
        private const string Post = "POST: ";
        private const string AppendJournalEntry = "Append content to journal entry";
        private const string NewJournalEntry = "Add new journal entry";
        private const string RenameTag = "Rename tag";
        private const string OpenForEdit = "Open entry for editing";
    }
}
