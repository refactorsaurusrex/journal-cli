using System;
using System.Collections.ObjectModel;
using System.IO.Abstractions;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using System.Threading.Tasks;
using JournalCli.Core;
using JournalCli.Infrastructure;
using Git = LibGit2Sharp;

namespace JournalCli.Cmdlets
{
    public abstract class JournalCmdletBase : CmdletBase
    {
        private bool _beenWarned;
        private const string Error = "Journal location was not provided and no default location exists. One or the other is required";
        private const string MissingGitBinaryWarning = "You're missing a native binary that's required to enable git integration. " +
            "Click here for more information:\r\n\r\nhttps://journalcli.me/docs/faq#i-got-a-missing-git-binary-warning-whats-that-about\r\n";

        [Parameter]
        public string Location { get; set; }

        protected abstract void RunJournalCommand();

        protected sealed override void ProcessRecord()
        {
            if (!string.IsNullOrEmpty(Location))
            {
                Location = ResolvePath(Location);
                return;
            }

            var encryptedStore = EncryptedStoreFactory.Create<UserSettings>();
            var settings = UserSettings.Load(encryptedStore);

            if (string.IsNullOrEmpty(settings.DefaultJournalRoot))
                throw new PSInvalidOperationException(Error);

                Location = settings.DefaultJournalRoot;

                RunJournalCommand();
                CheckForUpdates();
            }
            catch (Exception e)
            {
                Log.Error(e, "Error encountered during ProcessRecord");
                throw;
            }
        }

        private protected Journal OpenJournal()
        {
            var fileSystem = new FileSystem();
            var ioFactory = new JournalReaderWriterFactory(fileSystem, Location);
            var markdownFiles = new MarkdownFiles(fileSystem, Location);
            return Journal.Open(ioFactory, markdownFiles, SystemProcess);
        }

        private protected DateRange GetRangeOrNull(DateTime? from, DateTime to) => from.HasValue ? new DateRange(from.Value, to) : null;

        protected void Commit(GitCommitType commitType)
        {
#if !DEBUG
            try
            {
                ValidateGitRepo();
                var message = GitCommitMessage.Get(commitType);
                CommitCore(message);
            }
            catch (TypeInitializationException e) when (e.InnerException is DllNotFoundException)
            {
                if (!_beenWarned)
                {
                    WriteWarning(MissingGitBinaryWarning);
                    _beenWarned = true;
                }
            }
#endif
        }

        protected void Commit(string message)
        {
#if !DEBUG
            try
            {
                ValidateGitRepo();
                CommitCore(message);
            }
            catch (TypeInitializationException e) when (e.InnerException is DllNotFoundException)
            {
                if (!_beenWarned)
                {
                    WriteWarning(MissingGitBinaryWarning);
                    _beenWarned = true;
                }
            }
#endif
        }

        private void CommitCore(string message)
        {
            using var repo = new Git.Repository(Location);
            var statusOptions = new Git.StatusOptions
            {
                DetectRenamesInIndex = true,
                DetectRenamesInWorkDir = true,
                IncludeIgnored = false,
                IncludeUntracked = true,
                RecurseUntrackedDirs = true,
                RecurseIgnoredDirs = false
            };

            if (!repo.RetrieveStatus(statusOptions).IsDirty)
                return;

            Git.Commands.Stage(repo, "*");

            var author = new Git.Signature("JournalCli", "@journalCli", DateTime.Now);
            var committer = author;

            var options = new Git.CommitOptions { PrettifyMessage = true };
            var commit = repo.Commit(message, author, committer, options);
        }

        private void ValidateGitRepo()
        {
            if (Git.Repository.IsValid(Location))
                return;

            Git.Repository.Init(Location);
            using var repo = new Git.Repository(Location);
            Git.Commands.Stage(repo, "*");

            var author = new Git.Signature("JournalCli", "@journalCli", DateTime.Now);
            var committer = author;

            var options = new Git.CommitOptions { PrettifyMessage = true };
            var commit = repo.Commit("Initial commit", author, committer, options);
        }

        private void CheckForUpdates()
        {
            ProgressRecord progressRecord = null;
            try
            {
                var encryptedStore = EncryptedStoreFactory.Create<UserSettings>();
                var settings = UserSettings.Load(encryptedStore);
                if (settings.NextUpdateCheck != null && DateTime.Now <= settings.NextUpdateCheck)
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

                if (newVersion > installedVersion)
                {
                    WriteHostInverted("***** Update Available! *****");
                    WriteHostInverted($"You're currently using version {installedVersion}. Run 'Update-Module JournalCli' " +
                                      $"to upgrade to version {newVersion}, or run 'Suspend-JournalCliUpdateChecks' to snooze these notifications.");
                }

                settings.NextUpdateCheck = DateTime.Now.AddDays(7);
                settings.Save(encryptedStore);
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
    }
}
