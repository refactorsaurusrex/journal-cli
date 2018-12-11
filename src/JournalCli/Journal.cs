using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace JournalCli
{
    public class Journal
    {
        public static void OpenRandomJournalEntry(string rootDirectory)
        {
            var di = new DirectoryInfo(rootDirectory);
            var entries = di.GetFiles("*.md", SearchOption.AllDirectories).Where(x => x.LastWriteTime < DateTime.Now.AddMonths(-1)).ToList();

            if (entries.Count == 0)
                throw new InvalidOperationException("I couldn't find any entries older than a month. Did you pass in the right root directory?");

            var index = new Random().Next(0, entries.Count - 1);
            Process.Start(new ProcessStartInfo(entries[index].FullName)
            {
                UseShellExecute = true
            });
        }

        public static JournalIndex CreateIndex(string rootDirectory, bool includeHeaders)
        {
            var index = new JournalIndex();

            foreach (var file in Directory.EnumerateFiles(rootDirectory, "*.md", SearchOption.AllDirectories))
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