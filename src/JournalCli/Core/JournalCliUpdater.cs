using System;
using System.Linq;
using System.Management.Automation;
using JournalCli.Infrastructure;

namespace JournalCli.Core
{
    public class JournalCliUpdater
    {
        public void CheckForUpdates()
        {
            var installedVersionResult = ScriptBlock.Create("Get-Module JournalCli -ListAvailable | select version").Invoke();
            InstalledVersion = (Version)installedVersionResult[0].Properties["Version"].Value;

            var sb = ScriptBlock.Create("Find-Module JournalCli | select version");
            var ps = sb.GetPowerShell();
            var result = ps.BeginInvoke();

            if (!result.AsyncWaitHandle.WaitOne(12000))
                throw new TimeoutException("Unable to retrieve module update information within 12 seconds.");

            var availableVersionsResults = ps.EndInvoke(result).ReadAll();
            var availableVersions = availableVersionsResults.Select(x => new Version((string)x.Properties["Version"].Value)).ToList();
            NewVersion = availableVersions.FirstOrDefault(x => x.IsBeta() == InstalledVersion.IsBeta());

            if (NewVersion == null)
                throw new InvalidOperationException("Unable to locate an appropriate new version of the module. Missing registered repository?");
        }

        public void InstallUpdate()
        {
            if (!System.Diagnostics.Debugger.IsAttached)
                ScriptBlock.Create("Update-Module JournalCli").Invoke();
        }

        public Version InstalledVersion { get; private set; }
        
        public Version NewVersion { get; private set; }

        public bool IsMajorUpgrade => NewVersion?.Major > InstalledVersion?.Major;

        public bool IsUpdateAvailable => NewVersion != InstalledVersion;

        public string ReleaseNotesUrl => "https://github.com/refactorsaurusrex/journal-cli/releases/latest";
    }
}