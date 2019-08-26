using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using YamlDotNet.RepresentationModel;
using SysIO = System.IO;

namespace JournalCli
{
    public class JournalEntryFile
    {
        private readonly IFileSystem _fileSystem;
        private readonly string _filePath;

        public JournalEntryFile(IFileSystem fileSystem, string filePath)
        {
            _fileSystem = fileSystem;
            _filePath = filePath;
        }

        public ICollection<string> GetHeaders() => _fileSystem.File.ReadAllLines(_filePath).Where(x => x.StartsWith("#")).ToList();

        public ICollection<string> GetTags()
        {
            StringBuilder sb;
            using (var fs = _fileSystem.File.OpenText(_filePath))
            {
                var firstLine = fs.ReadLine();
                if (firstLine != "---")
                    return new List<string>();

                sb = new StringBuilder(firstLine + Environment.NewLine);

                while (!fs.EndOfStream)
                {
                    var next = fs.ReadLine();
                    sb.Append(next + Environment.NewLine);

                    if (next == "---")
                        break;
                }
            }

            var yaml = sb.ToString();
            using (var reader = new SysIO.StringReader(yaml))
            {
                var yamlStream = new YamlStream();
                yamlStream.Load(reader);
                var tags = (YamlSequenceNode)yamlStream.Documents[0].RootNode["tags"];
                return tags.Select(x => x.ToString()).ToList();
            }
        }

        public void WriteTags(ICollection<string> tags, bool createBackup)
        {
            if (createBackup)
            {
                var backupName = $"{_filePath}{Constants.BackupFileExtension}";

                var i = 0;
                while (_fileSystem.File.Exists(backupName))
                    backupName = $"{_filePath}({i++}){Constants.BackupFileExtension}";

                _fileSystem.File.Copy(_filePath, backupName);
            }

            var frontMatter = new List<string>
            {
                "---",
                "tags:"
            };

            frontMatter.AddRange(tags.Distinct().Select(tag => $"  - {tag}"));
            frontMatter.Add("---");

            var originalEntry = _fileSystem.File.ReadAllLines(_filePath).ToList();
            if (originalEntry[0] == "---")
            {
                var startIndex = originalEntry.FindIndex(1, line => line == "---") + 1;
                var newEntry = frontMatter.Concat(originalEntry.Skip(startIndex));
                _fileSystem.File.WriteAllLines(_filePath, newEntry);
            }
            else
            {
                var newEntry = frontMatter.Concat(originalEntry);
                _fileSystem.File.WriteAllLines(_filePath, newEntry);
            }

        }
    }
}
