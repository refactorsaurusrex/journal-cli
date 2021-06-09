using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Reflection;
using System.Text;
using JournalCli.Infrastructure;
using Serilog;

namespace JournalCli.Cmdlets
{
    public abstract class CmdletBase : PSCmdlet
    {
        private readonly ISystemProcess _systemProcess = SystemProcessFactory.Create();
        private readonly Lazy<string> _assemblyName = new(() => Assembly.GetExecutingAssembly().FullName);

        protected CmdletBase()
        {
            // ReSharper disable AssignNullToNotNullAttribute
            LogsDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "logs");
            var path = Path.Combine(LogsDirectory, ".log");
            // ReSharper restore AssignNullToNotNullAttribute
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(path, rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        protected string LogsDirectory { get; }

        protected string CollectSecret(string prompt, ConsoleColor color)
        {
            WriteHost(prompt, color);
            
            var secret = new StringBuilder();
            var nextKey = Console.ReadKey(true);
            while (nextKey.Key != ConsoleKey.Enter)
            {
                if (nextKey.Key == ConsoleKey.Backspace)
                {
                    if (secret.Length > 0)
                    {
                        secret.Remove(secret.Length - 1, 1);
                        Console.Write(nextKey.KeyChar);
                        Console.Write(" ");
                        Console.Write(nextKey.KeyChar);
                    }
                }
                else
                {
                    secret.Append(nextKey.KeyChar);
                    Console.Write("*");
                }
                nextKey = Console.ReadKey(true);
            }

            return secret.ToString();
        }

        protected string ResolvePath(string path) => GetUnresolvedProviderPathFromPSPath(path);

        protected void ThrowTerminatingError(string message, ErrorCategory category)
        {
            var errorRecord = new ErrorRecord(new Exception(message), category.ToString(), category, null);
            ThrowTerminatingError(errorRecord);
        }

        /// <summary>
        /// The statement should be in the form of "You are about to _______________________."
        /// Fill in the blank with the scary thing the user is about to do.
        /// </summary>
        /// <returns>True, if the user wants to continue. If false, the command should exit immediately.</returns>
        protected bool AreYouSure(string statement, bool defaultToYes = false)
        {
            var result = Choice($"You are about to {statement}.", "Are you sure you want to continue?", defaultToYes ? 0 : 1, "&Yes", "&No");
            return result == 0;
        }

        /// <summary>
        /// Sends and informational message to the pipeline.
        /// </summary>
        /// <param name="message"></param>
        protected void WriteInformation(string message) => WriteInformation(new InformationRecord(message, _assemblyName.Value));

        /// <summary>
        /// Prints a message to the PowerShell host.
        /// </summary>
        protected void WriteHost(string message, bool lineBreak = true)
        {
            if (lineBreak)
                Host.UI.WriteLine(message);
            else
                Host.UI.Write(message);
        }
        
        // protected void WriteHost(object obj) => Host.UI.WriteLine(obj);

        /// <summary>
        /// Prints a message to the PowerShell host with the specified fore- and background colors.
        /// </summary>
        protected void WriteHost(string message, ConsoleColor backgroundColor, ConsoleColor foregroundColor) =>
            Host.UI.WriteLine(foregroundColor, backgroundColor, message);

        /// <summary>
        /// Prints a message to the PowerShell host with the specified foreground color.
        /// </summary>
        protected void WriteHost(string message, ConsoleColor foregroundColor) =>
            Host.UI.WriteLine(foregroundColor, Host.UI.RawUI.BackgroundColor, message);

        /// <summary>
        /// Prints a message to the PowerShell host with the fore- and background colors inverted.
        /// </summary>
        protected void WriteHostInverted(string message)
        {
            Host.UI.Write(Host.UI.RawUI.BackgroundColor, Host.UI.RawUI.ForegroundColor, message);
            Host.UI.WriteLine();
        }

        /// <summary>
        /// Asks the user a yes or no question and returns true if the user selects yes, or false for no.
        /// </summary>
        /// <param name="question">A question to ask the user. Include a question mark at the end.</param>
        protected bool YesOrNo(string question)
        {
            return YesOrNo(question, Host.UI.RawUI.ForegroundColor, Host.UI.RawUI.BackgroundColor);
        }

        /// <summary>
        /// Asks the user a yes or no question and returns true if the user selects yes, or false for no.
        /// </summary>
        /// <param name="question">A question to ask the user. Include a question mark at the end.</param>
        /// <param name="foreground">The foreground color to use.</param>
        /// <param name="background">The background color to use.</param>
        /// <param name="allowEnterForYes">Allow the Enter key to be used for Yes.</param>
        protected bool YesOrNo(string question, ConsoleColor foreground, ConsoleColor background, bool allowEnterForYes = false)
        {
            var allowed = new List<char> { 'y', 'Y', 'n', 'N' };
            if (allowEnterForYes)
                allowed.Add('\r');

            KeyInfo key;
            do
            {
                Host.UI.Write(foreground, background, $"{question} (y/n) ");
                key = Host.UI.RawUI.ReadKey();
                Host.UI.WriteLine();
            } while (!allowed.Contains(key.Character));

            return key.Character == 'y' || key.Character == 'Y' || allowEnterForYes && key.Character == '\r';
        }

        /// <summary>
        /// Presents the user with an ordered list of options to choose from and returns the index of the selection.
        /// </summary>
        /// <param name="title">The title to display.</param>
        /// <param name="message">The message body to display. </param>
        /// <param name="defaultChoice">The index of the value in 'choices' which should be the default.</param>
        /// <param name="choices">A list of strings which represent choices. Insert an ampersand before a single letter of
        /// each item to be used as a shortcut key. Example: ("&amp;one", &amp;two", "t&amp;hree"), where 'O' would be
        /// the shortcut for "one", 'T' for "two", and 'H' for "three".</param>
        protected int Choice(string title, string message, int defaultChoice, params string[] choices)
        {
            var choiceDescriptions = choices.Select(c => new ChoiceDescription(c) { HelpMessage = $"Choose {c.Replace("&", "")}." }).ToList();
            var result = Host.UI.PromptForChoice(title, message, new Collection<ChoiceDescription>(choiceDescriptions), defaultChoice);

            return result;
        }

        protected void WriteHeader(IEnumerable<string> titles, ConsoleColor foregroundColor = ConsoleColor.Green)
        {
            var width = Host.UI.RawUI.WindowSize.Width - 3;
            var backgroundColor = Host.UI.RawUI.BackgroundColor;

            WriteHost(string.Empty);
            WriteHost(new string('=', width), backgroundColor, foregroundColor);
            WriteHost(string.Empty);
            foreach (var title in titles)
                WriteHost(title.PadLeft(3 + title.Length), backgroundColor, foregroundColor);
            WriteHost(string.Empty);
            WriteHost(new string('=', width), backgroundColor, foregroundColor);
            WriteHost(string.Empty);
        }

        private protected ISystemProcess SystemProcess => _systemProcess;
    }
}
