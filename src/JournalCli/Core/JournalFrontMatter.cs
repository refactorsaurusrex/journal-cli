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
            var fileName = fileSystem.Path.GetFileNameWithoutExtension(filePath);

            // Compiled entries do not have a single entry date, nor any readme values.
            if (Journal.IsCompiledEntry(fileName))
                return new JournalFrontMatter(yaml, null);

            var journalEntryDate = Journal.FileNamePattern.Parse(fileName).Value;
            return new JournalFrontMatter(yaml, journalEntryDate);
        }

        private JournalFrontMatter() { }

        public JournalFrontMatter(IEnumerable<string> tags, ReadmeParser readmeParser = null)
        {
            Tags = tags?.Distinct().OrderBy(x => x).ToList();

            if (readmeParser == null)
            {
                Readme = null;
                ReadmeDate = null;
            }
            else
            {
                Readme = readmeParser.FrontMatterValue;
                ReadmeDate = readmeParser.ExpirationDate;
            }
        }

        public JournalFrontMatter(string yamlFrontMatter, LocalDate? journalEntryDate)
        {
            if (string.IsNullOrWhiteSpace(yamlFrontMatter))
                return;

            var skipChars = Environment.NewLine.ToCharArray().Concat(new[] { '-' });
            if (yamlFrontMatter.All(x => skipChars.Contains(x)))
                return;

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
                    var node = yamlStream.Documents[0].RootNode[tagsKey];
                    if (node.NodeType == YamlNodeType.Sequence)
                    {
                        var tags = (YamlSequenceNode)node;
                        Tags = tags.Select(x => x.ToString()).Where(x => x != "(untagged)").Distinct().OrderBy(x => x).ToList();
                    }
                }

                if (readMeKey != null && journalEntryDate != null)
                {
                    var readme = (YamlScalarNode)yamlStream.Documents[0].RootNode[readMeKey];
                    var parser = new ReadmeParser(readme.Value, journalEntryDate.Value);
                    Readme = parser.FrontMatterValue;
                    ReadmeDate = parser.ExpirationDate;
                }
            }
        }

        [YamlMember(Alias = "tags")]
        public IReadOnlyCollection<string> Tags { get; private set; }

        [YamlMember(Alias = "readme")]
        public string Readme { get; }

        [YamlIgnore]
        public LocalDate? ReadmeDate { get; }

        /// <inheritdoc />
        public bool IsEmpty() => string.IsNullOrWhiteSpace(Readme) && (Tags == null || Tags.Count == 0);

        /// <inheritdoc />
        public bool HasTags() => Tags != null && Tags.Count > 0;

        public void AppendTags(ICollection<string> tags)
        {
            if (tags == null || !tags.Any())
                return;

            if (Tags == null)
                Tags = tags.ToList();
            else 
                Tags = Tags.Concat(tags).Distinct().OrderBy(x => x).ToList().AsReadOnly();
        }

        public string ToString(bool asFrontMatter)
        {
            var target = IsEmpty() ? new JournalFrontMatter(new[] { "(untagged)" }) : this;
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(target).Replace("- ", "  - ").Trim();
            return asFrontMatter ? $"{BlockIndicator}{Environment.NewLine}{yaml}{Environment.NewLine}{BlockIndicator}{Environment.NewLine}" : yaml;
        }

        public override string ToString() => ToString(false);
    }
}
