using JournalCli.Core;
using NodaTime;

namespace JournalCli.Infrastructure
{
    internal interface IJournalWriter
    {
        void Create(IJournalFrontMatter journalFrontMatter, string filePath, LocalDate entryDate);
        void RenameTag(IJournalReader journalReader, string oldTag, string newTag);
        string GetJournalEntryFilePath(LocalDate entryDate);
        bool EntryExists(string path);
        string GetCompiledJournalEntryFilePath(DateRange range);
        void CreateCompiled(IJournalFrontMatter journalFrontMatter, string filePath, string content);
    }
}