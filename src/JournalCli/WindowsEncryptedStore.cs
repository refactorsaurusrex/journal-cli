using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using YamlDotNet.Serialization;

namespace JournalCli
{
    internal class WindowsEncryptedStore : EncryptedStore
    {
        private readonly string _cipherPath;
        private readonly string _entropyPath;

        public WindowsEncryptedStore()
        {
            _cipherPath = Path.Combine(StorageLocation, "c");
            _entropyPath = Path.Combine(StorageLocation, "e");
        }

        public override void Save<T>(T target)
        {
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(target);
            var tokenBytes = Encoding.UTF8.GetBytes(yaml);

            var entropy = new byte[255];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(entropy);

            var cipher = ProtectedData.Protect(tokenBytes, entropy, DataProtectionScope.CurrentUser);

            Directory.CreateDirectory(StorageLocation);
            File.WriteAllBytes(_cipherPath, cipher);
            File.WriteAllBytes(_entropyPath, entropy);
        }

        public override T Load<T>()
        {
            if (!File.Exists(_cipherPath) || !File.Exists(_entropyPath))
                return null;

            var cipher = File.ReadAllBytes(_cipherPath);
            var entropy = File.ReadAllBytes(_entropyPath);

            var resultBytes = ProtectedData.Unprotect(cipher, entropy, DataProtectionScope.CurrentUser);
            var yaml = Encoding.UTF8.GetString(resultBytes);

            var deserializer = new DeserializerBuilder().Build();
            return deserializer.Deserialize<T>(yaml);
        }
    }
}
