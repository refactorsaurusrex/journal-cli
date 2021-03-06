﻿using System;
using System.IO.Abstractions;
using YamlDotNet.Serialization;

namespace JournalCli.Infrastructure
{
    internal class FileStore<T> : IFileStore<T>
        where T : class, new()
    {
        private readonly IFileSystem _fileSystem;

        public FileStore(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var path = fileSystem.Path.Combine(appData, "JournalCli");
            StorageLocation = path;
            FilePath = _fileSystem.Path.ChangeExtension(_fileSystem.Path.Combine(StorageLocation, typeof(T).Name), ".yaml");
        }

        protected readonly string StorageLocation;

        public string FilePath { get; }

        public void Save(T target)
        {
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(target);

            _fileSystem.Directory.CreateDirectory(StorageLocation);
            _fileSystem.File.WriteAllText(FilePath, yaml);
        }

        public T Load()
        {
            if (!_fileSystem.File.Exists(FilePath))
                return new T();

            try
            {
                var yaml = _fileSystem.File.ReadAllText(FilePath);
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