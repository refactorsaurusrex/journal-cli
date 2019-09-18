using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using JournalCli.Infrastructure;
using NodaTime;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace JournalCli.Core
{
    internal class JournalFrontMatter : IJournalFrontMatter
    {
        public static string BlockIndicator = "---";

        public static JournalFrontMatter FromFilePath(IFileSystem fileSystem, string filePath)
        {
            StringBuilder sb;
            using (var fs = fileSystem.File.OpenText(filePath))
            {
                var firstLine = fs.ReadLine();
                if (firstLine != "---")
                    return new JournalFrontMatter();

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
            var journalEntryDate = JournalEntry.FileNamePattern.Parse(fileSystem.Path.GetFileNameWithoutExtension(filePath)).Value;
            return new JournalFrontMatter(yaml, journalEntryDate);
        }

        private JournalFrontMatter() { }

        public JournalFrontMatter(IEnumerable<string> tags, string readme, LocalDate journalDate)
        {
            Tags = tags?.Distinct().ToList();

            if (string.IsNullOrWhiteSpace(readme))
            {
                Readme = null;
                ReadmeDate = null;
            }
            else
            {
                var parser = new ReadmeParser(readme, journalDate);
                Readme = parser.FrontMatterValue;
                ReadmeDate = parser.ExpirationDate;
            }
        }

        public JournalFrontMatter(string yamlFrontMatter, LocalDate journalEntryDate)
        {
            yamlFrontMatter = yamlFrontMatter.Trim();
            var yamlLines = yamlFrontMatter.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var originalLineCount = yamlLines.Count;

            if (yamlLines[0] == BlockIndicator)
                yamlLines.RemoveAt(0);

            var lastIndex = yamlLines.Count - 1;
            if (yamlLines[lastIndex] == BlockIndicator)
                yamlLines.RemoveAt(lastIndex);

            if (originalLineCount != yamlLines.Count)
                yamlFrontMatter = string.Join(Environment.NewLine, yamlLines);

            using (var reader = new System.IO.StringReader(yamlFrontMatter))
            {
                var yamlStream = new YamlStream();
                yamlStream.Load(reader);

                var keys = yamlStream.Documents[0].RootNode.AllNodes
                    .Where(x => x.NodeType == YamlNodeType.Mapping)
                    .Cast<YamlMappingNode>()
                    .SelectMany(x => x.Children.Keys)
                    .Cast<YamlScalarNode>()
                    .ToList();

                var tagsKey = keys.FirstOrDefault(k => k.Value.ToLowerInvariant() == "tags");
                var readMeKey = keys.FirstOrDefault(k => k.Value.ToLowerInvariant() == "readme");

                if (tagsKey != null)
                {
                    var tags = (YamlSequenceNode)yamlStream.Documents[0].RootNode[tagsKey];
                    Tags = tags.Select(x => x.ToString()).Distinct().ToList();
                }

                if (readMeKey != null)
                {
                    var readme = (YamlScalarNode)yamlStream.Documents[0].RootNode[readMeKey];
                    var parser = new ReadmeParser(readme.Value, journalEntryDate);
                    Readme = parser.FrontMatterValue;
                    ReadmeDate = parser.ExpirationDate;
                }
            }
        }

        [YamlMember(Alias = "tags")]
        public ICollection<string> Tags { get; }

        [YamlMember(Alias = "readme")]
        public string Readme { get; }

        [YamlIgnore]
        public LocalDate? ReadmeDate { get; }

        /// <inheritdoc />
        public bool IsEmpty() => string.IsNullOrWhiteSpace(Readme) && (Tags == null || Tags.Count == 0);

        /// <inheritdoc />
        public bool HasTags() => Tags != null && Tags.Count > 0;

        public string ToString(bool asFrontMatter)
        {
            if (IsEmpty())
                return "";

            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(this).Replace("- ", "  - ").Trim();
            return asFrontMatter ? $"{BlockIndicator}{Environment.NewLine}{yaml}{Environment.NewLine}{BlockIndicator}{Environment.NewLine}" : yaml;
        }

        public override string ToString() => ToString(false);
    }
}
