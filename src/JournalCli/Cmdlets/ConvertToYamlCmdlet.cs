using System.Management.Automation;
using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsData.ConvertTo, "Yaml")]
    [OutputType(typeof(string))]
    public class ConvertToYamlCmdlet : CmdletBase
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
        public PSObject Value { get; set; }

        protected override void ProcessRecord()
        {
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(Value.BaseObject);
            WriteObject(yaml);
        }
    }
}
