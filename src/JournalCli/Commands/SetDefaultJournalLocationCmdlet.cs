using System.IO;
using System.Management.Automation;
using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace JournalCli.Commands
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Set, "DefaultJournalLocation")]
    public class SetDefaultJournalLocationCmdlet : CmdletBase
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Location { get; set; }

        protected override void ProcessRecord()
        {
            var settings = new UserSettings
            {
                DefaultJournalRoot = Location
            };

            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(settings);
            var path = UserSettings.GetStorageLocation();
            File.WriteAllText(path, yaml);
        }
    }
}
