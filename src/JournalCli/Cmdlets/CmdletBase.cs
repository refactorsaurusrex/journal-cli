using System;
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
            WriteHost(title, color);
            WriteHost(new string('=', headerWidth), color);
        }

        protected void WriteHost(string text, ConsoleColor foregroundColor = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            Host.UI.WriteLine(foregroundColor, backgroundColor, text);
        }
    }
}
