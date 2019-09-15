using System;
using System.Management.Automation;

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
        protected bool AreYouSure(string warning)
        {
            return ShouldContinue("Do you want to continue?", warning);
        }

        protected void WriteHost(string text, ConsoleColor foregroundColor = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            Host.UI.WriteLine(foregroundColor, backgroundColor, text);
        }
    }
}
