using System;
using System.IO.Abstractions;

namespace JournalCli
{
    internal abstract class EncryptedStore : IEncryptedStore
    {
        protected EncryptedStore()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var fileSystem = new FileSystem();
            var path = fileSystem.Path.Combine(appData, "JournalCli");
            StorageLocation = path;
        }

        protected readonly string StorageLocation;
        public abstract void Save<T>(T target);
        public abstract T Load<T>() where T : class, new();
    }
}