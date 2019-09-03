using System;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Management.Automation;
using JetBrains.Annotations;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.New, "JournalEntry")]
    [Alias("nj")]
    public class NewJournalEntryCmdlet : JournalCmdletBase
    {
        [Parameter]
        public int DateOffset { get; set; }

        [Parameter]
        public string[] Tags { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            
            var entryDate = DateTime.Today.AddDays(DateOffset);
            var year = entryDate.Year.ToString();
            var month = $"{entryDate.Month:00} {entryDate:MMMM}";
            var fileSystem = new FileSystem();
            var parent = fileSystem.Path.Combine(RootDirectory, year, month);

            if (!fileSystem.Directory.Exists(parent))
                fileSystem.Directory.CreateDirectory(parent);

            var fileName = entryDate.ToString("yyyy.MM.dd.'md'");
            var fullPath = fileSystem.Path.Combine(parent, fileName);

            if (fileSystem.File.Exists(fullPath))
                ThrowTerminatingError($"File already exists: '{fullPath}'", ErrorCategory.InvalidOperation);

            using (var fs = fileSystem.File.CreateText(fullPath))
            {
                fs.WriteLine("---");
                fs.WriteLine("tags:");

                if (Tags == null || Tags.Length == 0)
                {
                    fs.WriteLine("  - ");
                }
                else
                {
                    foreach (var tag in Tags)
                        fs.WriteLine($"  - {tag}");
                }

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