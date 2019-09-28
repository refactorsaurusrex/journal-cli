using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

// ReSharper disable AssignNullToNotNullAttribute
namespace JournalCli.Infrastructure
{
    public class NativeBinaries
    {
        private static string _moduleRootPath;
        private static string _nativePath;

        public static void CopyIfNotExists()
        {
            if (string.IsNullOrEmpty(_moduleRootPath))
                GetPaths();

            if (!File.Exists(_moduleRootPath))
                File.Copy(_nativePath, _moduleRootPath);
        }

        private static void GetPaths()
        {
            var errorMessage = "Apologies, but your platform is not currently supported. However, I would really like to get it supported ASAP " +
                "and YOU can help! Please open a GitHub issue in the journal-cli repository explaining that you received this error and include your " +
                "operating system name and version number. I will follow up as soon as I can. Thanks!" +
                $"{Environment.NewLine}{Environment.NewLine}https://github.com/refactorsaurusrex/journal-cli/issues";
            string processArch;
            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.X64:
                    processArch = "x64";
                    break;
                case Architecture.X86:
                    processArch = "x86";
                    break;
                default:
                    throw new PlatformNotSupportedException(errorMessage);
            }

            string os;
            string ext;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                os = "win-";
                ext = ".dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                os = "osx-";
                ext = ".dylib";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                os = "linux-";
                ext = ".so";
            }
            else
            {
                throw new PlatformNotSupportedException(errorMessage);
            }

            var rootDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var nativeDir = Path.Combine(rootDir, "runtimes", $"{os}{processArch}", "native");

            _nativePath = Directory.GetFiles(nativeDir).First(f => Path.HasExtension(ext));
            _moduleRootPath = Path.Combine(rootDir, Path.GetFileName(_nativePath));
        }
    }
}
