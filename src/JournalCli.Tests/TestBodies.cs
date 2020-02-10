using System;
using System.IO;

namespace JournalCli.Tests
{
    public static class TestBodies
    {
        static TestBodies()
        {
            var files = Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "TestBodies"), "*.md");
            foreach (var path in files)
            {
                var text = File.ReadAllText(path);
                var fileName = Path.GetFileName(path);
                switch (fileName)
                {
                    case "AllTextHasExactlyOneHeader.md":
                        AllTextHasExactlyOneHeader = text;
                        break;
                    case "MultipleHeadersIncludingDefaultH1.md":
                        MultipleHeadersIncludingDefaultH1 = text;
                        break;
                    case "MultipleHeadersWithoutAssociatedText.md":
                        MultipleHeadersWithoutAssociatedText = text;
                        break;
                    case "NoHeadersOnlyText.md":
                        NoHeadersOnlyText = text;
                        break;
                    case "OnlyFirstParagraphHasNoHeader.md":
                        OnlyFirstParagraphHasNoHeader = text;
                        break;
                    case "NestedHeaders.md":
                        NestedHeaders = text;
                        break;
                    default:
                        throw new NotSupportedException($"The file '{fileName}' is not yet supported");
                }
            }
        }

        public static string NestedHeaders { get; }

        public static string AllTextHasExactlyOneHeader { get; }

        public static string MultipleHeadersIncludingDefaultH1 { get; }

        public static string MultipleHeadersWithoutAssociatedText { get; }

        public static string NoHeadersOnlyText { get; }

        public static string OnlyFirstParagraphHasNoHeader { get; }
    }
}
