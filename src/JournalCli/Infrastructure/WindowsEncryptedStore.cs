using System;
using System.IO.Abstractions;
using System.Security.Cryptography;
using System.Text;
using YamlDotNet.Serialization;

namespace JournalCli.Infrastructure
{
    internal class WindowsEncryptedStore<T> : EncryptedStore<T>
        where T : class, new()
    {
        private readonly IFileSystem _fileSystem;
        private readonly string _entropyPath;

        public WindowsEncryptedStore(IFileSystem fileSystem)
            : base(fileSystem)
        {
            _fileSystem = fileSystem;
            _entropyPath = _fileSystem.Path.Combine(StorageLocation, "e");
        }

        public override void Save(T target)
        {
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(target);
            var tokenBytes = Encoding.UTF8.GetBytes(yaml);

            var entropy = new byte[255];
            using (var rng = new RNGCryptoServiceProvider())
                rng.GetBytes(entropy);

            var cipher = ProtectedData.Protect(tokenBytes, entropy, DataProtectionScope.CurrentUser);

            _fileSystem.Directory.CreateDirectory(StorageLocation);
            var cipherPath = _fileSystem.Path.Combine(StorageLocation, target.GetType().Name);
            _fileSystem.File.WriteAllBytes(cipherPath, cipher);
            _fileSystem.File.WriteAllBytes(_entropyPath, entropy);
        }

        public override T Load()
        {
            var cipherPath = _fileSystem.Path.Combine(StorageLocation, typeof(T).Name);
            if (!_fileSystem.File.Exists(cipherPath) || !_fileSystem.File.Exists(_entropyPath))
                return new T();

            try
            {
                var cipher = _fileSystem.File.ReadAllBytes(cipherPath);
                var entropy = _fileSystem.File.ReadAllBytes(_entropyPath);

                var resultBytes = ProtectedData.Unprotect(cipher, entropy, DataProtectionScope.CurrentUser);
                var yaml = Encoding.UTF8.GetString(resultBytes);

                var deserializer = new DeserializerBuilder().IgnoreUnmatchedProperties().Build();
                return deserializer.Deserialize<T>(yaml);
            }
            catch (Exception)
            {
                return new T();
            }
        }
    }
}
