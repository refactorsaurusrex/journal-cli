using System;

namespace JournalCli.Infrastructure
{
    public class SyncSetupAbortedException : Exception
    {
        private const string DefaultMessage = "Journal sync setup process aborted. Re-run the process at any time to complete setup.";
        public SyncSetupAbortedException() : base(DefaultMessage) { }
        public SyncSetupAbortedException(string message) : base(message) { }
        public SyncSetupAbortedException(string message, Exception innerException) : base(message, innerException) { }
    }
}