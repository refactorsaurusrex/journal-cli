using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Open, "JournalEntries", DefaultParameterSetName = "Date")]
    [Alias("oje")]
    public class OpenJournalEntriesCmdlet : JournalCmdletBase
    {
        private readonly Queue<JournalEntryFile> _entries = new Queue<JournalEntryFile>();

        [Parameter(ValueFromPipeline = true, Position = 0, ParameterSetName = "Entry")]
        public IJournalEntry Entry { get; set; }

        [Parameter(Position = 0, ParameterSetName = "Last")]
        public SwitchParameter Last { get; set; }

        [Parameter(ValueFromPipeline = true, Position = 0, ParameterSetName = "Name")]
        public string EntryName { get; set; }

        [Parameter(ValueFromPipeline = true, Position = 0, ParameterSetName = "Date")]
        [NaturalDate(RoundTo.StartOfPeriod)]
        public LocalDate Date { get; set; } = Today.Date();

        [Parameter(ParameterSetName = "Entry")]
        public SwitchParameter NoWait { get; set; }

        protected override void EndProcessing()
        {
            base.EndProcessing();

            if (NoWait && _entries.Count > 9)
            {
                var question = $"You're about to open {_entries.Count} journal entries at the same time. Are you sure you want to do that?";
                if (!YesOrNo(question, ConsoleColor.Red, Console.BackgroundColor))
                {
                    WriteHostInverted("Good choice. I'll open your selected entries one at a time instead.");
                    NoWait = false;
                }
            }

            if (NoWait)
            {
                while (_entries.Any())
                {
                    var entry = _entries.Dequeue();
                    SystemProcess.Start(entry.FilePath);
                }

                return;
            }

            var paginate = _entries.Count > 1;
            var count = 1;
            var total = _entries.Count;
            while (true)
            {
                var entry = _entries.Dequeue();
                SystemProcess.Start(entry.FilePath);

                if (!_entries.Any() || paginate && !YesOrNo($"[{count++}/{total}] Open next entry?", ConsoleColor.Green, Console.BackgroundColor, allowEnterForYes: true))
                    break;
            }
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            var journal = OpenJournal();

            if (Last)
            {
                var lastEntry = journal.GetEntries<JournalEntryFile>(null, TagOperator.Any, SortOrder.Descending, null, 1).First();
                if (lastEntry == null)
                    WriteWarning("No entries found!");
                else
                    _entries.Enqueue(lastEntry);
                return;
            }

            switch (ParameterSetName)
            {
                case "Name":
                    var entryFromName = journal.GetEntryFromName(EntryName);
                    _entries.Enqueue(entryFromName);
                    break;
                case "Date":
                    var entryFromDate = journal.GetEntryFromDate(Date);
                    _entries.Enqueue(entryFromDate);
                    break;
                case "Entry":
                    var entryFromIJournal = Entry.GetReader().ToJournalEntry<JournalEntryFile>();
                    _entries.Enqueue(entryFromIJournal);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }
}