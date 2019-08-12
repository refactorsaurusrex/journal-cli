using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace JournalCli
{
    public class MarkdownFiles
    {
        private const FileAttributes Attributes = FileAttributes.Hidden | FileAttributes.System;

        public static List<string> FindAll(string rootDirectory)
        {
            var root = new DirectoryInfo(rootDirectory);
            var allFiles = new List<string>();

            foreach (var dir in root.EnumerateDirectories().Where(d => (d.Attributes & Attributes) == 0))
            {
                var result = FindAll(dir.FullName);
                allFiles.AddRange(result);
            }

            var files = Directory.GetFiles(rootDirectory, "*.md");
            allFiles.AddRange(files);
            return allFiles;
        }
    }
}
