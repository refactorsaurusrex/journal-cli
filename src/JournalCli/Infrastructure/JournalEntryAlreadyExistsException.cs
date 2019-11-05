using System;

namespace JournalCli.Infrastructure
{
    public class JournalEntryAlreadyExistsException : Exception
    {
        private const string DefaultMessage = "Journal entry already exists.";

        public JournalEntryAlreadyExistsException(string entryFilePath)
            : base(DefaultMessage)
        {
            EntryFilePath = entryFilePath;
        }

        public JournalEntryAlreadyExistsException(string entryFilePath, string message) 
            : base(message)
        {
            EntryFilePath = entryFilePath;
        }

        public JournalEntryAlreadyExistsException(string entryFilePath, string message, Exception innerException) 
            : base(message, innerException)
        {
            EntryFilePath = entryFilePath;
        }

        public string EntryFilePath { get; }
    }
}
