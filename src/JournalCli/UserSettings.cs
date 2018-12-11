using System.IO;
using System.Reflection;
using YamlDotNet.Serialization;

namespace JournalCli
{
    public class UserSettings
    {
        public static bool Exists() => File.Exists(GetStorageLocation());

        public static string GetStorageLocation()
        {
            var currentDirectory = Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;
            var path = Path.Combine(currentDirectory, "userSettings");
            return path;
        }

        public static UserSettings Load()
        {
            if (!Exists())
                return new UserSettings();

            var deserializer = new DeserializerBuilder().Build();

            using (var settingsFile = File.OpenText(GetStorageLocation()))
            {
                return deserializer.Deserialize<UserSettings>(settingsFile);
            }
        }

        public string DefaultJournalRoot { get; set; }
    }
}