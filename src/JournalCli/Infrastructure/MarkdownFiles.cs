using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using SysIO = System.IO;

namespace JournalCli.Infrastructure
{
    public class MarkdownFiles : IMarkdownFiles
    {
        private readonly IFileSystem _fileSystem;
        private readonly string _rootDirectory;
        private const SysIO.FileAttributes ExcludedAttributes = SysIO.FileAttributes.Hidden | SysIO.FileAttributes.System;

        public MarkdownFiles(IFileSystem fileSystem, string rootDirectory)
        {
            _fileSystem = fileSystem;
            _rootDirectory = rootDirectory;
        }

        public List<string> FindAll(bool fileNamesOnly = false) => FindAll(_rootDirectory, fileNamesOnly);

        private List<string> FindAll(string rootDirectory, bool fileNamesOnly = false)
        {
            var root = _fileSystem.DirectoryInfo.FromDirectoryName(rootDirectory);
            var allFiles = new List<string>();

            foreach (var dir in root.EnumerateDirectories().Where(d => (d.Attributes & ExcludedAttributes) == 0))
            {
                if (dir.Name == "Compiled")
                    continue;

                var result = FindAll(dir.FullName, fileNamesOnly);
                allFiles.AddRange(result);
            }

            var files = _fileSystem.Directory.GetFiles(rootDirectory, "*.md");
            allFiles.AddRange(fileNamesOnly ? files.Select(_fileSystem.Path.GetFileName) : files);
            return allFiles;
        }
    }
}
