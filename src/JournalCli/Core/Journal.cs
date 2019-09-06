using System;
using System.IO.Abstractions;
using System.Linq;
using JournalCli.Infrastructure;
using SysIO = System.IO;

namespace JournalCli.Core
{
    internal class Journal
    {
        private readonly IFileSystem _fileSystem;
        private readonly ISystemProcess _systemProcess;
        private readonly string _rootDirectory;

        public static Journal Open(IFileSystem fileSystem, ISystemProcess systemProcess, string rootDirectory)
        {
            return new Journal(fileSystem, systemProcess, rootDirectory);
        }

        private Journal(IFileSystem fileSystem, ISystemProcess systemProcess, string rootDirectory)
        {
            _fileSystem = fileSystem;
            _systemProcess = systemProcess;
            _rootDirectory = rootDirectory;
        }

        public void OpenRandomEntry(string[] tags)
        {
            var journalIndex = CreateIndex(false);
            if (journalIndex.Count == 0)
                throw new InvalidOperationException("I couldn't find any journal entries. Did you pass in the right root directory?");

            var allTaggedEntries = journalIndex.Where(x => tags.Contains(x.Tag)).ToList();
            var random = new Random();
            var randomIndex1 = random.Next(0, allTaggedEntries.Count - 1);
            var entries = allTaggedEntries[randomIndex1].Entries;
            var randomIndex2 = random.Next(0, entries.Count - 1);

            _systemProcess.Start(entries.ElementAt(randomIndex2).FilePath);
        }

        public void OpenRandomEntry()
        {
            var di = _fileSystem.DirectoryInfo.FromDirectoryName(_rootDirectory);
            var entries = di.GetFiles("*.md", SysIO.SearchOption.AllDirectories).ToList();

            if (entries.Count == 0)
                throw new InvalidOperationException("I couldn't find any journal entries. Did you pass in the right root directory?");

            var index = new Random().Next(0, entries.Count - 1);
            _systemProcess.Start(entries[index].FullName);
        }

        public JournalIndex CreateIndex(bool includeHeaders)
        {
            var index = new JournalIndex();

            foreach (var file in MarkdownFiles.FindAll(_fileSystem, _rootDirectory))
            {
                var entry = new JournalEntry(_fileSystem, file, includeHeaders);
                foreach (var tag in entry.Tags)
                {
                    if (index.Contains(tag))
                    {
                        index[tag].Entries.Add(entry);
                    }
                    else
                    {
                        var journalIndexEntry = new JournalIndexEntry(tag, entry);
                        index.Add(journalIndexEntry);
                    }
                }
            }

            return index;
        }

        public void CreateNewEntry(DateTime entryDate, string[] tags)
        {
            var year = entryDate.Year.ToString();
            var month = $"{entryDate.Month:00} {entryDate:MMMM}";
            var parent = _fileSystem.Path.Combine(_rootDirectory, year, month);

            if (!_fileSystem.Directory.Exists(parent))
                _fileSystem.Directory.CreateDirectory(parent);

            var fileName = entryDate.ToString("yyyy.MM.dd.'md'");
            var fullPath = _fileSystem.Path.Combine(parent, fileName);

            if (_fileSystem.File.Exists(fullPath))
                throw new InvalidOperationException($"Journal entry already exists: '{fullPath}'");

            using (var fs = _fileSystem.File.CreateText(fullPath))
            {
                fs.WriteLine("---");
                fs.WriteLine("tags:");

                if (tags == null || tags.Length == 0)
                {
                    fs.WriteLine("  - ");
                }
                else
                {
                    foreach (var tag in tags)
                        fs.WriteLine($"  - {tag}");
                }

                fs.WriteLine("---");
                fs.WriteLine($"# {entryDate.ToLongDateString()}");
                fs.Flush();
            }

            _systemProcess.Start(fullPath);
        }
    }
}