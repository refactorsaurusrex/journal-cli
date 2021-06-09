using System;
using System.IO.Abstractions;
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
        protected int WrapWidth { get; private set; }

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

            WrapWidth = Math.Min(Host.UI.RawUI.WindowSize.Width, 120);
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
            var ioFactory = new JournalReaderWriterFactory(_fileSystem, Location, WrapWidth);
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
                    _systemSettings.NextUpdateCheck = Today.PlusDays(7);
                    _systemSettingsStore.Save(_systemSettings);
                    return;
                }

                if (Today.Date() <= _systemSettings.NextUpdateCheck)
                    return;

                var updater = new JournalCliUpdater();

                try
                {
                    progressRecord = new ProgressRecord(0, "Checking For Updates", "This won't take long...");
                    WriteProgress(progressRecord);
                    updater.CheckForUpdates();
                }
                finally
                {
                    if (progressRecord != null)
                    {
                        progressRecord.RecordType = ProgressRecordType.Completed;
                        WriteProgress(progressRecord);
                    }
                }

                if (updater.IsUpdateAvailable)
                {
                    WriteHeader(new[] { "A new version of journal-cli is available!" });

                    string message;
                    string title;
                    if (updater.IsMajorUpgrade)
                    {
                        title = "***Major Upgrade***";
                        message = $"Howdy! {updater.NewVersion.ToSemVer()} is ready for installation. However, you're currently using {updater.InstalledVersion.ToSemVer()}. " +
                            "This means upgrading will include some breaking changes, in addition to adding new features and bug fixes. Would you like to review the release " +
                            "notes first? Or should I go ahead and install the update now?";
                    }
                    else
                    {
                        title = "***New Version***";
                        message = $"Hey there! {updater.NewVersion.ToSemVer()} is ready for installation. You're currently using {updater.InstalledVersion.ToSemVer()}. " +
                            "The new version will include new features and/or bug fixes. Would you like to review the release notes first? Or should I go ahead " +
                            "and install the update now?";
                    }
                    
                    var result = Choice(title, message.Wrap(WrapWidth), 0, "&View release notes", "&Install new version", "&Remind me later");
                    switch (result)
                    {   
                        case 0:
                            SystemProcess.Start(updater.ReleaseNotesUrl);
                            if (YesOrNo("Ready to install now?", ConsoleColor.Green, Console.BackgroundColor))
                            {
                                updater.InstallUpdate();
                                ShowSuccessfulUpdateMessage(updater.NewVersion);
                            }
                            else
                            {
                                WriteHost("It's cool. I'll remind you later...", ConsoleColor.Yellow);
                            }
                            break;
                        case 1:
                            updater.InstallUpdate();
                            ShowSuccessfulUpdateMessage(updater.NewVersion);
                            break;
                        case 2:
                            WriteHost("Roger that. I'll remind you later...", ConsoleColor.Yellow);
                            break;
                        default:
                            throw new InvalidOperationException("Unexpected choice returned.");
                    }
                }

                _systemSettings.NextUpdateCheck = Today.PlusDays(7);
                _systemSettingsStore.Save(_systemSettings);
            }
            catch (Exception e)
            {
                WriteWarning("Unexpected error encountered while checking for updates. Check the logs for more information.");
                Log.Error(e, "Attempt to perform module update check failed");
            }
        }

        private void ShowSuccessfulUpdateMessage(Version newVersion)
        {
            var message = $"{newVersion.ToSemVer()} has been installed! Restart your terminal to load the latest goodies.";
            ShowSplashScreen(message);
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
