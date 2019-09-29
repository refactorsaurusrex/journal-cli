using System;
using System.Linq;
using System.Management.Automation;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    public abstract class CmdletBase : PSCmdlet
    {
        protected string ResolvePath(string path) => GetUnresolvedProviderPathFromPSPath(path);

        protected void ThrowTerminatingError(string message, ErrorCategory category)
        {
            var errorRecord = new ErrorRecord(new Exception(message), category.ToString(), category, null);
            ThrowTerminatingError(errorRecord);
        }

        /// <summary>
        /// Prompts the user to confirm their actions before proceeding. 
        /// </summary>
        /// <param name="warning">A statement (not a question) about what is about to happen.</param>
        /// <returns>True if the user wants to proceed. False if they want to abort.</returns>
        protected bool AreYouSure(string warning) => ShouldContinue("Are you certain you want to continue?", "ACTION: " + warning);

        protected void WriteHeader(string title, ConsoleColor color)
        {
            var windowWidth = Host.UI.RawUI.WindowSize.Width;
            var headerWidth = Math.Min(75, windowWidth);

            WriteHost(new string('=', headerWidth), color);
            var wrapped = LineWrap(title, 75);
            WriteHost(wrapped, color);
            WriteHost(new string('=', headerWidth), color);
        }

        protected void WriteHost(string text, ConsoleColor foregroundColor = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            Host.UI.WriteLine(foregroundColor, backgroundColor, text);
        }

        private static string LineWrap(string text, int width)
        {
            if (text.Length <= width)
                return text;

            const char delimiter = ' ';
            var words = text.Split(delimiter);
            var allLines = words.Skip(1).Aggregate(words.Take(1).ToList(), (lines, word) =>
            {
                if (lines.Last().Length + word.Length >= width - 1) // Minus 1, to allow for newline char
                    lines.Add(word);
                else
                    lines[lines.Count - 1] += delimiter + word;
                return lines;
            });

            return string.Join(Environment.NewLine, allLines.ToArray());
        }
    }
}
