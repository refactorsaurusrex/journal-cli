using System.IO.Abstractions;
using JournalCli.Infrastructure;

namespace JournalCli.Core
{
    internal class JournalReaderWriterFactory : IJournalReaderWriterFactory
    {
        private readonly IFileSystem _fileSystem;
        private readonly string _rootDirectory;
        private readonly int _bodyWrapWidth;

        public JournalReaderWriterFactory(IFileSystem fileSystem, string rootDirectory, int bodyWrapWidth)
        {
            _fileSystem = fileSystem;
            _rootDirectory = rootDirectory;
            _bodyWrapWidth = bodyWrapWidth;
        }

        public IJournalReader CreateReader(string filePath) => new JournalReader(_fileSystem, filePath, _bodyWrapWidth);

        public IJournalWriter CreateWriter() => new JournalWriter(_fileSystem, _rootDirectory);
    }
}