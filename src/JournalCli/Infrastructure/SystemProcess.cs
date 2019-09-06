using System.Diagnostics;

namespace JournalCli.Infrastructure
{
    internal class SystemProcess : ISystemProcess
    {
        public void Start(string filePath)
        {
            Process.Start(new ProcessStartInfo(filePath)
            {
                UseShellExecute = true
            });
        }
    }
}
