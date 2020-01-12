using System;
using System.IO.Abstractions;
using System.Management.Automation;
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

        protected override void ProcessRecord()
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
    }
}