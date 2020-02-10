using JournalCli.Core;
using NodaTime;

namespace JournalCli.Infrastructure
{
    internal interface IJournalWriter
    {
        void RenameTag(IJournalReader journalReader, string oldTag, string newTag);
        string GetJournalEntryFilePath(LocalDate entryDate);
        bool EntryExists(string path);
        string GetCompiledJournalEntryFilePath(DateRange range);
        void CreateCompiled(IJournalFrontMatter journalFrontMatter, string filePath, string content);
        void Create(string filePath, IJournalFrontMatter journalFrontMatter, string content);
    }
}