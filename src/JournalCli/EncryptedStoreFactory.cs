using System.IO.Abstractions;
using System.Runtime.InteropServices;

namespace JournalCli
{
    internal class EncryptedStoreFactory
    {
        public static IEncryptedStore Create()
        {
            var fileSystem = new FileSystem();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new WindowsEncryptedStore(fileSystem);

            return new MacEncryptedStore(fileSystem);
        }
    }
}