using System.IO;
using System.Management.Automation;

namespace JournalCli.Infrastructure
{
    public class JournalFileInfo
    {
        public JournalFileInfo(FileInfo fileInfo)
        {
            File = fileInfo;
            Path = fileInfo.FullName;
        }

        public JournalFileInfo(string filePath)
        {
            File = new FileInfo(filePath);
            Path = filePath;
        }

        public string Path { get; }

        [Hidden]
        public FileInfo File { get; }
    }
}
