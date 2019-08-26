﻿using System;
using System.IO.Abstractions;
using YamlDotNet.Serialization;

namespace JournalCli
{
    using AuthenticatedEncryption;

    internal class MacEncryptedStore : EncryptedStore
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
                var cryptKey = AuthenticatedEncryption.NewKey();
                var authKey = AuthenticatedEncryption.NewKey();

                _fileSystem.File.WriteAllBytes(_cryptKeyPath, cryptKey);
                _fileSystem.File.WriteAllBytes(_authKeyPath, authKey);
            }
        }

        public override void Save<T>(T target)
        {
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(target);

            var cryptKey = _fileSystem.File.ReadAllBytes(_cryptKeyPath);
            var authKey = _fileSystem.File.ReadAllBytes(_authKeyPath);
            var cipherText = AuthenticatedEncryption.Encrypt(yaml, cryptKey, authKey);
            var cipherPath = _fileSystem.Path.Combine(StorageLocation, target.GetType().FullName);
            _fileSystem.File.WriteAllText(cipherPath, cipherText);
        }

        public override T Load<T>()
        {
            try
            {
                var cryptKey = _fileSystem.File.ReadAllBytes(_cryptKeyPath);
                var authKey = _fileSystem.File.ReadAllBytes(_authKeyPath);
                var cipherPath = _fileSystem.Path.Combine(StorageLocation, typeof(T).FullName);
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