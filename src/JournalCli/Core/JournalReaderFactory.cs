using System.IO.Abstractions;
using JournalCli.Infrastructure;

namespace JournalCli.Core
{
    internal class JournalReaderFactory : IJournalReaderFactory
    {
        private readonly IFileSystem _fileSystem;

        public JournalReaderFactory(IFileSystem fileSystem) => _fileSystem = fileSystem;

        public JournalReader CreateReader(string filePath) => new JournalReader(_fileSystem, filePath);
    }
}