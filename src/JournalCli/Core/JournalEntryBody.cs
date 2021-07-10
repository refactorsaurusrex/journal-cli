using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JournalCli.Infrastructure;
using NodaTime;
using NodaTime.Text;
// ReSharper disable StringLiteralTypo

namespace JournalCli.Core
{
    internal class JournalEntryBody : IEnumerable<(string header, string text)>
    {
        private readonly List<(string header, string text)> _bodyStructure;
        private static readonly string DoubleNewLine = $"{Environment.NewLine}{Environment.NewLine}";

        public JournalEntryBody() => _bodyStructure = new List<(string header, string text)> {($"# {Today.Date().ToString()}", string.Empty)};

        public JournalEntryBody(string rawBody)
        {
            var lines = rawBody.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            var headers = lines.Where(HeaderValidator.IsValid).ToList();
            var headerIndices = new List<(int index, string header)>();

            foreach (var header in headers)
            {
                var start = headerIndices.Any() ? headerIndices.Last().index + headerIndices.Last().header.Length : 0;
                var index = rawBody.IndexOf(header, start, StringComparison.CurrentCulture);
                headerIndices.Add((index, header));
            }

            _bodyStructure = new List<(string header, string text)>();

            if (string.IsNullOrWhiteSpace(rawBody))
                return;

            if (!headerIndices.Any())
            {
                _bodyStructure.Add((string.Empty, rawBody));
                return;
            }

            // Grabs text which precedes any headers
            if (headerIndices.First().index > 0) 
                _bodyStructure.Add((string.Empty, rawBody.Substring(0, headerIndices.First().index).Trim()));

            for (var i = 0; i < headerIndices.Count; i++)
            {
                var start = headerIndices[i].index + headerIndices[i].header.Length;
                var end = i == headerIndices.Count - 1 ? rawBody.Length : headerIndices[i + 1].index;

                _bodyStructure.Add((headerIndices[i].header, rawBody.Substring(start, end - start).Trim()));
            }

        }

        public void AddOrAppendToDefaultHeader(LocalDate date, ICollection<string> lines)
        {
            if (lines == null || !lines.Any())
                throw new ArgumentException($"'{nameof(lines)}' cannot be null or empty.");

            var text = string.Join(DoubleNewLine, lines);
            var element = _bodyStructure.FirstOrDefault(x => IsDate(x.header));

            if (element == default((string, string)))
            {
                _bodyStructure.Insert(0, ($"# {date.ToString()}", text));
            }
            else
            {
                _bodyStructure.Remove(element);
                element.text += $"{DoubleNewLine}{text}";
                _bodyStructure.Insert(0, element);
            }
        }

        public void AddOrAppendToCustomHeader(string header, ICollection<string> lines)
        {
            if (lines == null || !lines.Any())
                throw new ArgumentException($"'{nameof(lines)}' cannot be null or empty.");

            var text = string.Join(DoubleNewLine, lines);
            var element = _bodyStructure.FirstOrDefault(x => x.header == header);
            if (element == default((string, string)))
            {
                _bodyStructure.Add(($"{header}", text));
            }
            else
            {
                var index = _bodyStructure.IndexOf(element);
                _bodyStructure.Remove(element);
                element.text += $"{DoubleNewLine}{text}";
                _bodyStructure.Insert(index, element);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var (header, text) in this)
            {
                if (!string.IsNullOrWhiteSpace(header))
                {
                    sb.AppendLine(header);
                    sb.AppendLine();
                }

                if (!string.IsNullOrWhiteSpace(text))
                {
                    sb.AppendLine(text);
                    sb.AppendLine();
                }
            }

            return sb.ToString().Trim();
        }

        public IEnumerator<(string header, string text)> GetEnumerator() => _bodyStructure.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private bool IsDate(string headerString)
        {
            var pattern = LocalDatePattern.CreateWithCurrentCulture("dddd, MMMM d, yyyy");
            return pattern.Parse(headerString.Replace("#", "").Trim()).Success;
        }
    }
}
