using System;
using System.Collections.Generic;
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

        [Parameter]
        [ValidateSet("Ascending", "Descending")]
        public string SortDirection { get; set; } = "Descending";

        [Parameter]
        public int Limit { get; set; }

        protected override void RunJournalCommand()
        {
            var dateRange = GetRangeOrNull(From, To);

            if (dateRange == null && Tags == null)
                FromAll();
            else
                FromDate(dateRange);
        }

        private void FromAll()
        {
            var fileSystem = new FileSystem();
            var markdownFiles = new MarkdownFiles(fileSystem, Location);

            var sorted = SortDirection == "Descending" ?
                markdownFiles.FindAll().OrderByDescending(FileNameToDate) :
                markdownFiles.FindAll().OrderBy(FileNameToDate);

            var filtered = Limit > 0 ? 
                sorted.Take(Limit).Select(x => new JournalFileInfo(x)) : 
                sorted.Select(x => new JournalFileInfo(x));

            WriteObject(filtered, true);
        }

        private void FromDate(DateRange dateRange)
        {
            var journal = OpenJournal();
            var index = journal.CreateIndex<JournalEntryFile>(dateRange, Tags);

            var sorted = SortDirection == "Descending"
                ? index.SelectMany(x => x.Entries).Distinct().OrderByDescending(x => x.EntryDate)
                : index.SelectMany(x => x.Entries).Distinct().OrderBy(x => x.EntryDate);

            var filtered = Limit > 0 ? 
                sorted.Take(Limit).Select(x => new JournalFileInfo(x.FilePath)) : 
                sorted.Select(x => new JournalFileInfo(x.FilePath));

            WriteObject(filtered, true);
        }

        private DateTime FileNameToDate(string fileName)
        {
            var withoutExt = Path.GetFileNameWithoutExtension(fileName);
            return DateTime.Parse(withoutExt);
        }
    }
}
