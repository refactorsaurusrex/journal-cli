﻿using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using JetBrains.Annotations;

namespace JournalCli.Commands
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

                if (Tags.Length == 0)
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