using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using JetBrains.Annotations;

namespace JournalCli.Commands
{
    /// <summary>
    /// <para type="synopsis">Creates a new journal entry.</para>
    /// <para type="description">Creates a new markdown-based journal entry file in the specified root directory.</para>
    /// <para type="link" uri="https://github.com/refactorsaurusrex/journal-cli/wiki">The journal-cli wiki.</para>
    /// <example>
    ///   <para>Create a journal entry for today</para>
    ///   <code>New-JournalEntry</code>
    /// </example>
    /// <example>
    ///   <para>Create a journal entry for yesterday</para>
    ///   <code>New-JournalEntry -DateOffset -1</code>
    /// </example>
    /// </summary>
    [PublicAPI]
    [Cmdlet(VerbsCommon.New, "JournalEntry")]
    [Alias("nj")]
    public class NewJournalEntryCmdlet : JournalCmdletBase
    {
        /// <summary>
        /// <para type="description">An integer representing the number of days to offset from today's date. For example,
        /// -1 represents yesterday. The default is 0, meaning today.</para>
        /// </summary>
        [Parameter]
        public int DateOffset { get; set; }

        /// <inheritdoc />
        protected override void ProcessRecord()
        {
            var root = GetResolvedRootDirectory();
            var entryDate = DateTime.Today.AddDays(DateOffset);
            var year = entryDate.Year.ToString();
            var month = $"{entryDate.Month:00} {entryDate:MMMM}";
            var parent = Path.Combine(root, year, month);

            if (!Directory.Exists(parent))
                Directory.CreateDirectory(parent);

            var fileName = entryDate.ToString("yyyy.MM.dd.'md'");
            var fullPath = Path.Combine(parent, fileName);

            if (File.Exists(fullPath))
                ThrowTerminatingError($"File already exists: '{fullPath}'", ErrorCategory.InvalidOperation);

            using (var fs = File.CreateText(fullPath))
            {
                fs.WriteLine("---");
                fs.WriteLine("tags:");
                fs.WriteLine("  - ");
                fs.WriteLine("---");
                fs.WriteLine($"# {entryDate.ToLongDateString()}");
                fs.Flush();
            }

            Process.Start(new ProcessStartInfo(fullPath)
            {
                UseShellExecute = true
            });
        }
    }
}