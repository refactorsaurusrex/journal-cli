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
    public class GetJournalEntriesByTagCmdlet : JournalCmdletBase
    {
        [Parameter(Mandatory = true)]
        public string[] Tags { get; set; }

        [Parameter]
        [Obsolete("IncludeHeaders has been deprecated and is no longer in use.")]
        public SwitchParameter IncludeHeaders { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            var fileSystem = new FileSystem();
            var systemProcess = new SystemProcess();
            var ioFactory = new JournalReaderWriterFactory(fileSystem, RootDirectory);
            var markdownFiles = new MarkdownFiles(fileSystem, RootDirectory);
            var journal = Journal.Open(ioFactory, markdownFiles, systemProcess);
            var index = journal.CreateIndex<MetaJournalEntry>();

            var result = index.Where(x => Tags.Contains(x.Tag));
            WriteObject(result, true);
        }
    }
}