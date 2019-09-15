using System.IO.Abstractions;
using JournalCli.Infrastructure;

namespace JournalCli.Core
{
    internal class JournalReaderWriterFactory : IJournalReaderWriterFactory
    {
        private readonly IFileSystem _fileSystem;
        private readonly string _rootDirectory;

        public JournalReaderWriterFactory(IFileSystem fileSystem, string rootDirectory)
        {
            _fileSystem = fileSystem;
            _rootDirectory = rootDirectory;
        }

        public IJournalReader CreateReader(string filePath) => new JournalReader(_fileSystem, filePath);

        public IJournalWriter CreateWriter() => new JournalWriter(_fileSystem, _rootDirectory);
    }
}