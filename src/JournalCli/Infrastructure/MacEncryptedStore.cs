using System;
using System.IO.Abstractions;
using YamlDotNet.Serialization;

namespace JournalCli.Infrastructure
{
    using AuthenticatedEncryption;

    internal class MacEncryptedStore<T> : EncryptedStore<T>
        where T : class, new()
    {
        private readonly IFileSystem _fileSystem;
        private readonly string _cryptKeyPath;
        private readonly string _authKeyPath;

        public MacEncryptedStore(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            _cryptKeyPath = _fileSystem.Path.Combine(StorageLocation, "ck");
            _authKeyPath = _fileSystem.Path.Combine(StorageLocation, "ak");

            if (!_fileSystem.File.Exists(_cryptKeyPath) || !_fileSystem.File.Exists(_authKeyPath))
            {
                fileSystem.Directory.CreateDirectory(StorageLocation);
                var cryptKey = AuthenticatedEncryption.NewKey();
                var authKey = AuthenticatedEncryption.NewKey();

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
            var cipherText = AuthenticatedEncryption.Encrypt(yaml, cryptKey, authKey);
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
                var plainText = AuthenticatedEncryption.Decrypt(cipherText, cryptKey, authKey);
                var deserializer = new DeserializerBuilder().Build();
                return deserializer.Deserialize<T>(plainText);
            }
            catch (Exception)
            {
                return new T();
            }
        }
    }
}