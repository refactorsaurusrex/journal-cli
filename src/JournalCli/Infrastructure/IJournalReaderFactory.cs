using JournalCli.Core;

namespace JournalCli.Infrastructure
{
    internal interface IJournalReaderFactory
    {
        JournalReader CreateReader(string filePath);
    }
}