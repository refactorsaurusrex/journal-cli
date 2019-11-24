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
    [Cmdlet(VerbsCommon.Get, "JournalEntriesByTag")]
    [OutputType(typeof(IEnumerable<JournalIndexEntry<MetaJournalEntry>>))]
    [OutputType(typeof(IEnumerable<JournalIndexEntry<CompleteJournalEntry>>))]
    [Alias("gjt")]
    public class GetJournalEntriesByTagCmdlet : JournalCmdletBase
    {
        [Parameter(Mandatory = true)]
        public string[] Tags { get; set; }

        [Parameter]
        public SwitchParameter IncludeBodies { get; set; }

        [Parameter]
        public SwitchParameter All { get; set; }

        [Parameter(ParameterSetName = "Range")]
        public DateTime From { get; set; }

        [Parameter(ParameterSetName = "Range")]
        public DateTime To { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            var fileSystem = new FileSystem();
            var systemProcess = new SystemProcess();
            var ioFactory = new JournalReaderWriterFactory(fileSystem, Location);
            var markdownFiles = new MarkdownFiles(fileSystem, Location);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);

            switch (All.ToBool())
            {
                case true when IncludeBodies:
                    WriteAllTagResults<CompleteJournalEntry>(journal);
                    break;
                case true when !IncludeBodies:
                    WriteAllTagResults<MetaJournalEntry>(journal);
                    break;
                case false when IncludeBodies:
                    WriteAnyTagResults<CompleteJournalEntry>(journal);
                    break;
                case false when !IncludeBodies:
                    WriteAnyTagResults<MetaJournalEntry>(journal);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void WriteAllTagResults<T>(Journal journal)
            where T : class, IJournalEntry
        {
            var allIndex = journal.CreateIndex<T>(requiredTags: Tags).OrderByDescending(x => x.Entries.Count);
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