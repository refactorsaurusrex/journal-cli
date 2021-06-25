using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommunications.Read, "JournalEntries")]
    public class ReadJournalEntriesCmdlet : JournalCmdletBase
    {
        private readonly HashSet<CompleteJournalEntry> _filteredIndex = new HashSet<CompleteJournalEntry>();

        [Parameter(ValueFromPipeline = true, Position = 0, ParameterSetName = "Index")]
        public JournalIndexEntry<CompleteJournalEntry> Index { get; set; }

        [Parameter(ValueFromPipeline = true, Position = 0, ParameterSetName = "Meta")]
        public MetaJournalEntry MetaEntry { get; set; }

        [Parameter(ValueFromPipeline = true, Position = 0, ParameterSetName = "ReadMe")]
        public ReadmeJournalEntry ReadMeEntry { get; set; }
        
        [Parameter(ValueFromPipeline = true, Position = 0, ParameterSetName = "Names")]
        public string[] EntryNames { get; set; }

        [Parameter(ParameterSetName = "Index")]
        [Parameter(ParameterSetName = "Meta")]
        [Parameter(ParameterSetName = "ReadMe")]
        [Parameter(ParameterSetName = "Names")]
        public SwitchParameter Reverse { get; set; }

        [Parameter(ParameterSetName = "Index")]
        [Parameter(ParameterSetName = "Meta")]
        [Parameter(ParameterSetName = "ReadMe")]
        [Parameter(ParameterSetName = "Names")]
        public JournalView View { get; set; } = JournalView.MultiPage;

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            switch (ParameterSetName)
            {
                case "Index":
                    ProcessIndexEntries();
                    break;
                case "Meta":
                    ProcessMetaEntries();
                    break;
                case "ReadMe":
                    ProcessReadMeEntries();
                    break;
                case "Names":
                    ProcessEntryNames();
                    break;
            }
        }

        protected override void EndProcessing()
        {
            base.EndProcessing();

            var entries = Reverse ? 
                _filteredIndex.OrderBy(x => x.EntryDate).ToList() : 
                _filteredIndex.OrderByDescending(x => x.EntryDate).ToList();

            if (View == JournalView.SinglePage) Console.Clear();

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                var tagList = entry.Tags.Select(t => $"#{t}").ToList();
                var tags = string.Join(", ", tagList);
                var titles = new List<string> { entry.EntryDate.ToString(), tags };

                if (entry.IsReadMe())
                {
                    var readme = $"ReadMe: {entry.ReadMeDate}";
                    titles.Add(readme);
                }
                
                WriteHeader(titles, ConsoleColor.Cyan);
                WriteObject(entry.Body);

                if (View != JournalView.Dump)
                {
                    if (!YesOrNo($"[{i + 1}/{entries.Count}] Continue?", ConsoleColor.Green, Console.BackgroundColor, allowEnterForYes: true))
                        break;

                    if (View == JournalView.SinglePage) Console.Clear();
                }
            }
        }

        private void ProcessReadMeEntries()
        {
            var entry = ReadMeEntry.GetReader().ToJournalEntry<CompleteJournalEntry>();
            _filteredIndex.Add(entry);
        }

        private void ProcessMetaEntries()
        {
            var entry = MetaEntry.GetReader().ToJournalEntry<CompleteJournalEntry>();
            _filteredIndex.Add(entry);
        }

        private void ProcessIndexEntries()
        {
            foreach (var entry in Index.Entries)
            {
                _filteredIndex.Add(entry);
            }
        }
        
        private void ProcessEntryNames()
        {
            var journal = OpenJournal();
            foreach (var name in EntryNames)
            {
                var entry = journal.GetEntryFromName(name).GetReader().ToJournalEntry<CompleteJournalEntry>();
                _filteredIndex.Add(entry);
            }
        }
    }
}