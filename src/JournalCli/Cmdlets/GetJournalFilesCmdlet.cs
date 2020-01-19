using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "JournalFiles")]
    public class GetJournalFilesCmdlet : JournalCmdletBase
    {
        [Parameter]
        public DateTime? From { get; set; }

        [Parameter]
        public DateTime To { get; set; } = DateTime.Now;

        [Parameter]
        public string[] Tags { get; set; }

        protected override void RunJournalCommand()
        {
            var dateRange = GetRangeOrNull(From, To);

            if (dateRange == null && Tags == null)
            {
                var fileSystem = new FileSystem();
                var markdownFiles = new MarkdownFiles(fileSystem, Location);
                var results = markdownFiles.FindAll().Select(x => new FileInfo(x));
                WriteObject(results, true);

                return;
            }

            var journal = OpenJournal();
            var index = journal.CreateIndex<JournalEntryFile>(dateRange, Tags);
            var entries = index.SelectMany(x => x.Entries).Distinct().Select(x => new FileInfo(x.FilePath));
            WriteObject(entries, true);
        }
    }
}
