using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using SysIO = System.IO;

namespace JournalCli.Infrastructure
{
    public class MarkdownFiles
    {
        private const SysIO.FileAttributes Attributes = SysIO.FileAttributes.Hidden | SysIO.FileAttributes.System;

        public static List<string> FindAll(IFileSystem fileSystem, string rootDirectory)
        {
            var root = fileSystem.DirectoryInfo.FromDirectoryName(rootDirectory);
            var allFiles = new List<string>();

            foreach (var dir in root.EnumerateDirectories().Where(d => (d.Attributes & Attributes) == 0))
            {
                var result = FindAll(fileSystem, dir.FullName);
                allFiles.AddRange(result);
            }

            var files = fileSystem.Directory.GetFiles(rootDirectory, "*.md");
            allFiles.AddRange(files);
            return allFiles;
        }
    }
}
