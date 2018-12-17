using System;
using System.Collections.Generic;
using System.IO;

namespace JournalCli
{
    public class JournalEntry
    {
        public JournalEntry(string filePath, bool includeHeaders)
        {
            FilePath = filePath;
            var entryFile = new JournalEntryFile(filePath);
            Tags = entryFile.GetTags();

            if (includeHeaders)
                Headers = entryFile.GetHeaders();
        }

        public string FilePath { get; }

        public ICollection<string> Tags { get; set; }

        public ICollection<string> Headers { get; set; }

        public override string ToString() => Path.GetFileNameWithoutExtension(FilePath) ?? throw new InvalidOperationException();
    }
}