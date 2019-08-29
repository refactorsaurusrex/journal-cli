using System.IO.Abstractions;
using System.Runtime.InteropServices;

namespace JournalCli
{
    internal class EncryptedStoreFactory
    {
        public static IEncryptedStore<T> Create<T>()
            where T : class, new()
        {
            var fileSystem = new FileSystem();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new WindowsEncryptedStore<T>(fileSystem);

            return new MacEncryptedStore<T>(fileSystem);
        }
    }
}