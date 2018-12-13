using System.IO;
using System.Reflection;
using YamlDotNet.Serialization;

namespace JournalCli
{
    public class UserSettings
    {
        public static bool Exists() => File.Exists(StorageLocation);

        private static string StorageLocation
        {
            get
            {
                var currentDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
                var path = Path.Combine(currentDirectory, "userSettings");
                return path;
            }
        }

        public static UserSettings Load()
        {
            if (!Exists())
                return new UserSettings();

            var deserializer = new DeserializerBuilder().Build();

            using (var settingsFile = File.OpenText(StorageLocation))
            {
                return deserializer.Deserialize<UserSettings>(settingsFile);
            }
        }

        public string DefaultJournalRoot { get; set; }

        public string BackupLocation { get; set; }

        public string BackupPassword { get; set; }

        public void Save()
        {
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(this);
            File.WriteAllText(StorageLocation, yaml);
        }
    }
}