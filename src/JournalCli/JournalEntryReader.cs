using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.RepresentationModel;

namespace JournalCli
{
    public class JournalEntryReader
    {
        private readonly string _filePath;
        public JournalEntryReader(string filePath) => _filePath = filePath;

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
    }
}
