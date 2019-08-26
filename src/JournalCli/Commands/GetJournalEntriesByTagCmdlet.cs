using System.Management.Automation;
using JetBrains.Annotations;
using System.Linq;

namespace JournalCli.Commands
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "JournalEntriesByTag")]
    public class GetJournalEntriesByTagCmdlet : JournalCmdletBase
    {
        [Parameter(Mandatory = true)]
        public string[] Tags { get; set; }

        [Parameter]
        public SwitchParameter IncludeHeaders { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            var index = Journal.CreateIndex(RootDirectory, IncludeHeaders);

            var result = index.Where(x => Tags.Contains(x.Tag));
            WriteObject(result, true);
        }
    }
}