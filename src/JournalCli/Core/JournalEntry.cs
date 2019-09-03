using System;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace JournalCli.Core
{
    public class JournalEntry
    {
        private readonly IFileSystem _fileSystem;

        public JournalEntry(IFileSystem fileSystem, string filePath, bool includeHeaders)
        {
            _fileSystem = fileSystem;
            FilePath = filePath;
            var entryFile = new JournalEntryFile(fileSystem, filePath);
            Tags = entryFile.GetTags();

            if (includeHeaders)
                Headers = entryFile.GetHeaders();
        }

        public string FilePath { get; }

        public ICollection<string> Tags { get; set; }

        public ICollection<string> Headers { get; set; }

        public override string ToString() => _fileSystem.Path.GetFileNameWithoutExtension(FilePath) ?? throw new InvalidOperationException();
    }
}