using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Management.Automation;
using JournalCli.Core;
using JournalCli.Infrastructure;
using Serilog;

namespace JournalCli.Cmdlets
{
    public abstract class JournalCmdletBase : CmdletBase
    {
        private const string Error = "Journal not found! Use 'Set-JournalSettings' to set the path for your journal.";
        private readonly SystemSettings _systemSettings;
        private readonly UserSettings _userSettings;
        private readonly IFileStore<SystemSettings> _systemSettingsStore;
        private readonly FileSystem _fileSystem = new FileSystem();

        protected JournalCmdletBase()
        {
            _systemSettingsStore = new FileStore<SystemSettings>(_fileSystem);
            _systemSettings = _systemSettingsStore.Load();
            var userSettingsStore = new FileStore<UserSettings>(_fileSystem);
            _userSettings = userSettingsStore.Load();
        }

        protected string Location { get; private set; }

        protected override void BeginProcessing()
        {
            ResolveJournalLocation();

            if (!_systemSettings.HideWelcomeScreen)
            {
                ShowSplashScreen("Welcome! I hope you love using JournalCli. For help and other information, visit https://journalcli.me. Send feedback to hi@journalcli.me.");
                _systemSettings.HideWelcomeScreen = true;
                _systemSettingsStore.Save(_systemSettings);
            }
        }

        protected override void EndProcessing() => CheckForUpdates();

        private void ResolveJournalLocation()
        {
            try
            {
                if (string.IsNullOrEmpty(Location))
                {
                    if (string.IsNullOrEmpty(_userSettings.DefaultJournalRoot))
                        throw new PSInvalidOperationException(Error);

                    Location = _userSettings.DefaultJournalRoot;
                }
                else
                {
                    Location = ResolvePath(Location);
                }
            }
            catch (Exception e)
            {
                if (e is PipelineStoppedException ||
                    e is PipelineClosedException)
                    throw;

                Log.Error(e, "Error encountered during ProcessRecord");
                throw;
            }
        }

        private protected Journal OpenJournal()
        {
            var wrap = Math.Min(Host.UI.RawUI.WindowSize.Width, 120);
            var ioFactory = new JournalReaderWriterFactory(_fileSystem, Location, wrap);
            var markdownFiles = new MarkdownFiles(_fileSystem, Location);
            return Journal.Open(ioFactory, markdownFiles, SystemProcess);
        }

        private void CheckForUpdates()
        {
            ProgressRecord progressRecord = null;
            try
            {
                if (_systemSettings.NextUpdateCheck == null)
                {
                    _systemSettings.NextUpdateCheck = DateTime.Now.AddDays(7);
                    _systemSettingsStore.Save(_systemSettings);
                    return;
                }

                if (DateTime.Now <= _systemSettings.NextUpdateCheck)
                    return;

                progressRecord = new ProgressRecord(0, "Checking For Updates", "This won't take long...");
                WriteProgress(progressRecord);

                var installedVersionResult = ScriptBlock.Create("Get-Module JournalCli -ListAvailable | select version").Invoke();
                var installedVersion = (Version)installedVersionResult[0].Properties["Version"].Value;

                var sb = ScriptBlock.Create("Find-Module JournalCli | select version");
                var ps = sb.GetPowerShell();
                var result = ps.BeginInvoke();

                if (!result.AsyncWaitHandle.WaitOne(12000))
                    throw new TimeoutException("Unable to retrieve module update information within 12 seconds.");

                var availableVersionsResults = ps.EndInvoke(result).ReadAll();
                var availableVersions = availableVersionsResults.Select(x => new Version((string)x.Properties["Version"].Value)).ToList();
                var newVersion = availableVersions.FirstOrDefault(x => x.IsBeta() == installedVersion.IsBeta());

                if (newVersion == null)
                    throw new InvalidOperationException("Unable to locate an appropriate new version of the module. Missing registered repository?");

                if (newVersion > installedVersion)
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                        WriteHostInverted("DEBUGGER ATTACHED: New module not installed.");
                    else
                        ScriptBlock.Create("Update-Module JournalCli").Invoke();
                    var message = $"v{newVersion.Major}.{newVersion.Minor}.{newVersion.Build} has been installed! Restart " +
                        "your terminal to load the latest goodies.";
                    ShowSplashScreen(message);
                }

                _systemSettings.NextUpdateCheck = DateTime.Now.AddDays(7);
                _systemSettingsStore.Save(_systemSettings);
            }
            catch (Exception e)
            {
                Log.Error(e, "Attempt to perform module update check failed.");
            }
            finally
            {
                if (progressRecord != null)
                {
                    progressRecord.RecordType = ProgressRecordType.Completed;
                    WriteProgress(progressRecord);
                }
            }
        }

        private void ShowSplashScreen(string message)
        {
            var logo = $@"
       __                             ___________ 
      / /___  __  ___________  ____ _/ / ____/ (_)
 __  / / __ \/ / / / ___/ __ \/ __ `/ / /   / / / 
/ /_/ / /_/ / /_/ / /  / / / / /_/ / / /___/ / /  
\____/\____/\__,_/_/  /_/ /_/\__,_/_/\____/_/_/   
{message}                                                  
";
            WriteHost(logo);
        }
    }
}
