using System.IO.Abstractions.TestingHelpers;

namespace JournalCli.Tests
{
    public class VirtualJournal : MockFileSystem
    {
        public int TotalReadmeEntries { get; set; }
    }
}