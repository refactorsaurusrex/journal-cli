using System;
using System.Management.Automation;

namespace JournalCli.Commands
{
    public abstract class CmdletBase : PSCmdlet
    {
        protected string ResolvePath(string path) => GetUnresolvedProviderPathFromPSPath(path);

        protected void ThrowTerminatingError(string message, ErrorCategory category)
        {
            var errorRecord = new ErrorRecord(new Exception(message), category.ToString(), category, null);
            ThrowTerminatingError(errorRecord);
        }

        protected void WriteHost(string text, ConsoleColor foregroundColor = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black)
        {
            Host.UI.WriteLine(foregroundColor, backgroundColor, text);
        }
    }
}
