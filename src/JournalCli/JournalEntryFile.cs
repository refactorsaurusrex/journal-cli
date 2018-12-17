using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.RepresentationModel;

namespace JournalCli
{
    public class JournalEntryFile
    {
        private readonly string _filePath;
        public JournalEntryFile(string filePath) => _filePath = filePath;

        public ICollection<string> GetHeaders() => File.ReadAllLines(_filePath).Where(x => x.StartsWith("#")).ToList();

        public ICollection<string> GetTags()
        {
            StringBuilder sb;
            using (var fs = File.OpenText(_filePath))
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
            using (var reader = new StringReader(yaml))
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
                var backupName = $"{_filePath}.old";

                var i = 0;
                while (File.Exists(backupName))
                    backupName = $"{_filePath}({i++}).old";

                File.Copy(_filePath, backupName);
            }

            var frontMatter = new List<string>
            {
                "---",
                "tags:"
            };

            frontMatter.AddRange(tags.Distinct().Select(tag => $"  - {tag}"));
            frontMatter.Add("---");

            var originalEntry = File.ReadAllLines(_filePath).ToList();
            if (originalEntry[0] == "---")
            {
                var startIndex = originalEntry.FindIndex(1, line => line == "---") + 1;
                var newEntry = frontMatter.Concat(originalEntry.Skip(startIndex));
                File.WriteAllLines(_filePath, newEntry);
            }
            else
            {
                var newEntry = frontMatter.Concat(originalEntry);
                File.WriteAllLines(_filePath, newEntry);
            }

        }
    }
}
