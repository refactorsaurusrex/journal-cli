using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using YamlDotNet.Serialization;

namespace JournalCli
{
    public class UserSettings
    {
        private static readonly string CipherPath;
        private static readonly string EntropyPath;
        private static readonly string StorageLocation;

        static UserSettings()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = Path.Combine(appData, "JournalCli");
            StorageLocation = path;
            CipherPath = Path.Combine(StorageLocation, "c");
            EntropyPath = Path.Combine(StorageLocation, "e");
        }

        public static bool Exists() => File.Exists(CipherPath) && File.Exists(EntropyPath);

        public static UserSettings Load()
        {
            if (!Exists())
                return new UserSettings();

            var cipher = File.ReadAllBytes(CipherPath);
            var entropy = File.ReadAllBytes(EntropyPath);

            var resultBytes = ProtectedData.Unprotect(cipher, entropy, DataProtectionScope.CurrentUser);
            var yaml = Encoding.UTF8.GetString(resultBytes);

            var deserializer = new DeserializerBuilder().Build();
            return deserializer.Deserialize<UserSettings>(yaml);
        }

        public string DefaultJournalRoot { get; set; }

        public string BackupLocation { get; set; }

        public string BackupPassword { get; set; }

        public void Save()
        {
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(this);

            var tokenBytes = Encoding.UTF8.GetBytes(yaml);

            var entropy = new byte[255];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(entropy);

            var cipher = ProtectedData.Protect(tokenBytes, entropy, DataProtectionScope.CurrentUser);

            Directory.CreateDirectory(StorageLocation);
            File.WriteAllBytes(CipherPath, cipher);
            File.WriteAllBytes(EntropyPath, entropy);
        }
    }
}