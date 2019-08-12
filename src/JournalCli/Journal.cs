using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace JournalCli
{
    public class Journal
    {
        public static void OpenRandomEntry(string rootDirectory, string[] tags)
        {
            var journalIndex = CreateIndex(rootDirectory, false);
            if (journalIndex.Count == 0)
                throw new PSInvalidOperationException("I couldn't find any journal entries. Did you pass in the right root directory?");

            var allTaggedEntries = journalIndex.Where(x => tags.Contains(x.Tag)).ToList();
            var random = new Random();
            var randomIndex1 = random.Next(0, allTaggedEntries.Count - 1);
            var entries = allTaggedEntries[randomIndex1].Entries;
            var randomIndex2 = random.Next(0, entries.Count - 1);

            Process.Start(new ProcessStartInfo(entries.ElementAt(randomIndex2).FilePath)
            {
                UseShellExecute = true
            });
        }

        public static void OpenRandomEntry(string rootDirectory)
        {
            var di = new DirectoryInfo(rootDirectory);
            var entries = di.GetFiles("*.md", SearchOption.AllDirectories).ToList();

            if (entries.Count == 0)
                throw new PSInvalidOperationException("I couldn't find any journal entries. Did you pass in the right root directory?");

            var index = new Random().Next(0, entries.Count - 1);
            Process.Start(new ProcessStartInfo(entries[index].FullName)
            {
                UseShellExecute = true
            });
        }

        public static JournalIndex CreateIndex(string rootDirectory, bool includeHeaders)
        {
            var index = new JournalIndex();

            foreach (var file in MarkdownFiles.FindAll(rootDirectory))
            {
                var entry = new JournalEntry(file, includeHeaders);
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
    }
}