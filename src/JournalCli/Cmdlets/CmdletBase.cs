using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Reflection;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    public abstract class CmdletBase : PSCmdlet
    {
        private readonly ISystemProcess _systemProcess = SystemProcessFactory.Create();
        private readonly Lazy<string> _assemblyName = new Lazy<string>(() => Assembly.GetExecutingAssembly().FullName);

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

        /// <summary>
        /// Sends and informational message to the pipeline.
        /// </summary>
        /// <param name="message"></param>
        protected void WriteInformation(string message) => WriteInformation(new InformationRecord(message, _assemblyName.Value));

        /// <summary>
        /// Prints a message to the PowerShell host.
        /// </summary>
        protected void WriteHost(string message) => Host.UI.WriteLine(message);

        /// <summary>
        /// Prints a message to the PowerShell host with the specified fore- and background colors.
        /// </summary>
        protected void WriteHost(string message, ConsoleColor backgroundColor, ConsoleColor foregroundColor) =>
            Host.UI.WriteLine(foregroundColor, backgroundColor, message);

        /// <summary>
        /// Prints a message to the PowerShell host with the fore- and background colors inverted.
        /// </summary>
        protected void WriteHostInverted(string message) => Host.UI.WriteLine(Host.UI.RawUI.BackgroundColor, Host.UI.RawUI.ForegroundColor, message);

        /// <summary>
        /// Asks the user a yes or no question and returns true if the user selects yes, or false for no.
        /// </summary>
        /// <param name="question">A question to ask the user. Include a question mark at the end.</param>
        protected bool YesOrNo(string question)
        {
            return YesOrNo(question, Host.UI.RawUI.BackgroundColor, Host.UI.RawUI.ForegroundColor);
        }

        /// <summary>
        /// Asks the user a yes or no question and returns true if the user selects yes, or false for no.
        /// </summary>
        /// <param name="question">A question to ask the user. Include a question mark at the end.</param>
        /// <param name="foreground">The foreground color to use.</param>
        /// <param name="background">The background color to use.</param>
        protected bool YesOrNo(string question, ConsoleColor foreground, ConsoleColor background)
        {
            KeyInfo key;
            do
            {
                Host.UI.Write(foreground, background, $" {question} (y/n) ");
                key = Host.UI.RawUI.ReadKey();
                Host.UI.WriteLine();
            } while (key.Character != 'y' && key.Character != 'n');

            return key.Character == 'y';
        }

        /// <summary>
        /// Presents the user with an ordered list of options to choose from and returns the index of the selection.
        /// </summary>
        /// <param name="caption">The title to display.</param>
        /// <param name="message">The message body to display. </param>
        /// <param name="defaultChoice">The index of the value in 'choices' which should be the default.</param>
        /// <param name="choices">A list of strings which represent choices. Insert an ampersand before a single letter of
        /// each item to be used as a shortcut key. Example: ("&amp;one", &amp;two", "t&amp;hree"), where 'O' would be
        /// the shortcut for "one", 'T' for "two", and 'H' for "three".</param>
        protected int Choice(string caption, string message, int defaultChoice, params string[] choices)
        {
            var origBackground = Host.UI.RawUI.BackgroundColor;
            var origForeground = Host.UI.RawUI.ForegroundColor;

            Host.UI.RawUI.ForegroundColor = origBackground;
            Host.UI.RawUI.BackgroundColor = origForeground;

            var choiceDescriptions = choices.Select(c => new ChoiceDescription(c) { HelpMessage = $"Choose {c.Replace("&", "")}." }).ToList();
            var result = Host.UI.PromptForChoice(caption, message, new Collection<ChoiceDescription>(choiceDescriptions), defaultChoice);

            Host.UI.RawUI.ForegroundColor = origForeground;
            Host.UI.RawUI.BackgroundColor = origBackground;

            return result;
        }

        private protected ISystemProcess SystemProcess => _systemProcess;
    }
}
