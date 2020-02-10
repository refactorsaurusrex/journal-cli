namespace JournalCli.Infrastructure
{
    public enum GitCommitType
    {
        PreNewJournalEntry,
        PostNewJournalEntry,
        PreAppendJournalEntry,
        PostAppendJournalEntry,
        PreRenameTag,
        PostRenameTag,
        Manual
    }
}