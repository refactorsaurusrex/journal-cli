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
        private const string Error = "Journal location was not provided and no default location exists. One or the other is required";
        private readonly UserSettings _settings;
        private readonly IEncryptedStore<UserSettings> _encryptedStore;

        protected JournalCmdletBase()
        {
            _encryptedStore = EncryptedStoreFactory.Create<UserSettings>();
            _settings = UserSettings.Load(_encryptedStore);
        }

        [Parameter]
        public string Location { get; set; }

        // TODO: Rethink this pattern. Process record should only be used if data will be piped in. Otherwise, EndProcessing should be used.
        protected abstract void RunJournalCommand();

        protected sealed override void ProcessRecord()
        {
            try
            {
                if (string.IsNullOrEmpty(Location))
                {
                    if (string.IsNullOrEmpty(_settings.DefaultJournalRoot))
                        throw new PSInvalidOperationException(Error);

                    Location = _settings.DefaultJournalRoot;
                }
                else
                {
                    Location = ResolvePath(Location);
                }

                RunJournalCommand();
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

        protected sealed override void EndProcessing()
        {
            CheckForUpdates();
        }

        protected sealed override void BeginProcessing()
        {
            if (_settings.HideWelcomeScreen)
                return;

            ShowSplashScreen("Welcome! I hope you love using JournalCli. For help and other information, visit https://journalcli.me. Send feedback to hi@journalcli.me.");
            _settings.HideWelcomeScreen = true;
            _settings.Save(_encryptedStore);
        }

        private protected Journal OpenJournal()
        {
            var fileSystem = new FileSystem();
            var ioFactory = new JournalReaderWriterFactory(fileSystem, Location);
            var markdownFiles = new MarkdownFiles(fileSystem, Location);
            return Journal.Open(ioFactory, markdownFiles, SystemProcess);
        }

        private protected DateRange GetRangeOrNull(DateTime? from, DateTime to) => from.HasValue ? new DateRange(from.Value, to) : null;

        private void CheckForUpdates()
        {
            ProgressRecord progressRecord = null;
            try
            {
                if (_settings.NextUpdateCheck == null)
                {
                    _settings.NextUpdateCheck = DateTime.Now.AddDays(7);
                    _settings.Save(_encryptedStore);
                    return;
                }

                if (DateTime.Now <= _settings.NextUpdateCheck)
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

                _settings.NextUpdateCheck = DateTime.Now.AddDays(7);
                _settings.Save(_encryptedStore);
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
