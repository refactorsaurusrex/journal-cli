using System.Management.Automation;
using JetBrains.Annotations;
using System.Linq;

namespace JournalCli.Commands
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "JournalIndex")]
    [OutputType(typeof(JournalIndex))]
    public class GetJournalIndexCmdlet : JournalCmdletBase
    {
        [Parameter]
        [ValidateSet("Count", "Name")]
        public string OrderBy { get; set; } = "Count";

        [Parameter]
        [ValidateSet("Ascending", "Descending")]
        public string Direction { get; set; } = "Descending";

        [Parameter]
        public SwitchParameter IncludeHeaders { get; set; }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            var index = Journal.CreateIndex(RootDirectory, IncludeHeaders);

            switch (OrderBy)
            {
                case var order when order == "Name" && Direction == "Ascending":
                    var ascByName = index.OrderBy(x => x.Tag);
                    WriteObject(ascByName, true);
                    break;

                case var order when order == "Name" && Direction == "Descending":
                    var descByName = index.OrderByDescending(x => x.Tag);
                    WriteObject(descByName, true);
                    break;

                case var order when order == "Count" && Direction == "Descending":
                    var descByCount = index.OrderByDescending(x => x.Entries.Count);
                    WriteObject(descByCount, true);
                    break;

                case var order when order == "Count" && Direction == "Ascending":
                    var ascByCount = index.OrderBy(x => x.Entries.Count);
                    WriteObject(ascByCount, true);
                    break;
            }
        }
    }
}