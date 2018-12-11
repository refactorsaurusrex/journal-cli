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
        public string OrderBy { get; set; } = "Name";

        [Parameter]
        [ValidateSet("Ascending", "Descending")]
        public string Direction { get; set; } = "Ascending";

        [Parameter]
        public SwitchParameter IncludeHeaders { get; set; }

        protected override void ProcessRecord()
        {
            var root = GetResolvedRootDirectory();
            var index = Journal.CreateIndex(root, IncludeHeaders);

            switch (OrderBy)
            {
                case var order when order == "Name" && Direction == "Ascending":
                    WriteObject(index, true);
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