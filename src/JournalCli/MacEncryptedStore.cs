using System.IO;
using YamlDotNet.Serialization;

namespace JournalCli
{
    using AuthenticatedEncryption;

    internal class MacEncryptedStore : EncryptedStore
    {
        private readonly string _cryptKeyPath;
        private readonly string _authKeyPath;
        private readonly string _cipherPath;

        public MacEncryptedStore()
        {
            _cryptKeyPath = Path.Combine(StorageLocation, "ck");
            _authKeyPath = Path.Combine(StorageLocation, "ak");
            _cipherPath = Path.Combine(StorageLocation, "c");

            if (!File.Exists(_cryptKeyPath) || !File.Exists(_authKeyPath))
            {
                var cryptKey = AuthenticatedEncryption.NewKey();
                var authKey = AuthenticatedEncryption.NewKey();

                File.WriteAllBytes(_cryptKeyPath, cryptKey);
                File.WriteAllBytes(_authKeyPath, authKey);
            }
        }

        public override void Save<T>(T target)
        {
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(target);

            var cryptKey = File.ReadAllBytes(_cryptKeyPath);
            var authKey = File.ReadAllBytes(_authKeyPath);
            var cipherText = AuthenticatedEncryption.Encrypt(yaml, cryptKey, authKey);
            File.WriteAllText(_cipherPath, cipherText);
        }

        public override T Load<T>()
        {
            var cryptKey = File.ReadAllBytes(_cryptKeyPath);
            var authKey = File.ReadAllBytes(_authKeyPath);
            var cipherText = File.ReadAllText(_cipherPath);
            var plainText = AuthenticatedEncryption.Decrypt(cipherText, cryptKey, authKey);
            var deserializer = new DeserializerBuilder().Build();
            return deserializer.Deserialize<T>(plainText);
        }
    }
}