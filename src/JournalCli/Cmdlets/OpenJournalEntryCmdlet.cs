using System;
using System.IO.Abstractions;
using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;
using NodaTime;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Open, "JournalEntry")]
    [Alias("oje")]
    public class OpenJournalEntryCmdlet : JournalCmdletBase
    {
        // TODO: open entry via date or natural language input

        [Parameter(ValueFromPipeline = true, Position = 0, ParameterSetName = "Entry")]
        public IJournalEntry Entry { get; set; }

        [Parameter(ValueFromPipeline = true, Position = 0, ParameterSetName = "Name")]
        public string EntryName { get; set; }

        [Parameter(ValueFromPipeline = true, Position = 0, ParameterSetName = "Date")]
        public DateTime Date { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var fileSystem = new FileSystem();
            var journalWriter = new JournalWriter(fileSystem, Location);
            string path;

            switch (ParameterSetName)
            {
                case "Name":
                    {
                        var entryDate = EntryName.EndsWith(".md") ?
                            Journal.FileNameWithExtensionPattern.Parse(EntryName).Value :
                            Journal.FileNamePattern.Parse(EntryName).Value;
                        path = journalWriter.GetJournalEntryFilePath(entryDate);
                        break;
                    }
                case "Date":
                    {
                        var entryDate = LocalDate.FromDateTime(Date);
                        path = journalWriter.GetJournalEntryFilePath(entryDate);
                        break;
                    }
                case "Entry":
                    {
                        var entryDate = Journal.FileNamePattern.Parse(Entry.EntryName).Value;
                        path = journalWriter.GetJournalEntryFilePath(entryDate);
                        break;
                    }
                default:
                    throw new NotSupportedException();
            }

            if (!fileSystem.File.Exists(path))
                throw new PSInvalidOperationException("The provided journal entry does not exist.");

            var systemProcess = new SystemProcess();
            systemProcess.Start(path);
        }
    }
}