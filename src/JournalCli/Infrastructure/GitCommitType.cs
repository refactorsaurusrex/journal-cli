namespace JournalCli.Infrastructure
{
    public enum GitCommitType
    {
        PreNewJournalEntry,
        PostNewJournalEntry,
        PreRenameTag,
        PostRenameTag,
        Manual
    }
}