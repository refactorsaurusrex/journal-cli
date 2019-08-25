using System.Runtime.InteropServices;

namespace JournalCli
{
    internal class EncryptedStoreFactory
    {
        public IEncryptedStore Create()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return new WindowsEncryptedStore();

            return new MacEncryptedStore();
        }
    }
}