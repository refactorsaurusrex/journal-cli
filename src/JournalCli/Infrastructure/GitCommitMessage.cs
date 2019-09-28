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
                case GitCommitType.PreNewJournalEntry:
                    return "PRE: " + NewJournalEntry;
                case GitCommitType.PostNewJournalEntry:
                    return "POST: " + NewJournalEntry;
                case GitCommitType.PreRenameTag:
                    return "PRE: " + RenameTag;
                case GitCommitType.PostRenameTag:
                    return "POST: " + RenameTag;
            }
        }

        private const string NewJournalEntry = "Add new journal entry";
        private const string RenameTag = "Rename tag";
    }
}
