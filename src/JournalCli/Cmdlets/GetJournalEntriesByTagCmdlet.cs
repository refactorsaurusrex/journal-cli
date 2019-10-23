using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "JournalEntriesByTag", DefaultParameterSetName = "Any")]
    [OutputType(typeof(IEnumerable<JournalIndexEntry<MetaJournalEntry>>))]
    public class GetJournalEntriesByTagCmdlet : JournalCmdletBase
    {
        [Parameter(Mandatory = true)]
        public string[] Tags { get; set; }

        [Parameter]
        public SwitchParameter IncludeBodies { get; set; }

        [Parameter(ParameterSetName = "Any")]
        public SwitchParameter Any { get; set; }

        [Parameter(ParameterSetName = "All")]
        public SwitchParameter All { get; set; }

        [Parameter]
        [Obsolete("IncludeHeaders has been deprecated and is no longer in use.")]
        public SwitchParameter IncludeHeaders { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            var fileSystem = new FileSystem();
            var systemProcess = new SystemProcess();
            var ioFactory = new JournalReaderWriterFactory(fileSystem, Location);
            var markdownFiles = new MarkdownFiles(fileSystem, Location);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);

            switch (ParameterSetName)
            {
                case "Any" when IncludeBodies:
                    WriteAnyTagResults<CompleteJournalEntry>(journal);
                    break;
                case "Any":
                    WriteAnyTagResults<MetaJournalEntry>(journal);
                    break;
                case "All" when IncludeBodies:
                    WriteAllTagResults<CompleteJournalEntry>(journal);
                    break;
                case "All":
                    WriteAllTagResults<MetaJournalEntry>(journal);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void WriteAllTagResults<T>(Journal journal)
            where T : class, IJournalEntry
        {
            var allIndex = journal.CreateIndex<T>(Tags).OrderByDescending(x => x.Entries.Count);
            WriteObject(allIndex, true);
        }

        private void WriteAnyTagResults<T>(Journal journal)
            where T : class, IJournalEntry
        {
            var anyIndex = journal.CreateIndex<T>();
            var result = anyIndex.Where(x => Tags.Contains(x.Tag)).OrderByDescending(x => x.Entries.Count);
            WriteObject(result, true);
        }
    }
}