using System;
using System.IO.Abstractions;
using AuthenticatedEncryption;
using YamlDotNet.Serialization;

namespace JournalCli.Infrastructure
{
    internal class MacEncryptedStore<T> : EncryptedStore<T>
        where T : class, new()
    {
        private readonly IFileSystem _fileSystem;
        private readonly string _cryptKeyPath;
        private readonly string _authKeyPath;

        public MacEncryptedStore(IFileSystem fileSystem)
            : base(fileSystem)
        {
            _fileSystem = fileSystem;
            _cryptKeyPath = _fileSystem.Path.Combine(StorageLocation, "ck");
            _authKeyPath = _fileSystem.Path.Combine(StorageLocation, "ak");

            if (!_fileSystem.File.Exists(_cryptKeyPath) || !_fileSystem.File.Exists(_authKeyPath))
            {
                fileSystem.Directory.CreateDirectory(StorageLocation);
                var cryptKey = Encryption.NewKey();
                var authKey = Encryption.NewKey();

                _fileSystem.File.WriteAllBytes(_cryptKeyPath, cryptKey);
                _fileSystem.File.WriteAllBytes(_authKeyPath, authKey);
            }
        }

        public override void Save(T target)
        {
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(target);

            var cryptKey = _fileSystem.File.ReadAllBytes(_cryptKeyPath);
            var authKey = _fileSystem.File.ReadAllBytes(_authKeyPath);
            var cipherText = Encryption.Encrypt(yaml, cryptKey, authKey);
            var cipherPath = _fileSystem.Path.Combine(StorageLocation, target.GetType().Name);
            _fileSystem.File.WriteAllText(cipherPath, cipherText);
        }

        public override T Load()
        {
            try
            {
                var cryptKey = _fileSystem.File.ReadAllBytes(_cryptKeyPath);
                var authKey = _fileSystem.File.ReadAllBytes(_authKeyPath);
                var cipherPath = _fileSystem.Path.Combine(StorageLocation, typeof(T).Name);
                var cipherText = _fileSystem.File.ReadAllText(cipherPath);
                var plainText = Encryption.Decrypt(cipherText, cryptKey, authKey);
                var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
                return deserializer.Deserialize<T>(plainText);
            }
            catch (Exception)
            {
                return new T();
            }
        }
    }
}