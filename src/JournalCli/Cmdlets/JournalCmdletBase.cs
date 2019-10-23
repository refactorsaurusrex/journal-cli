using System;
using System.Management.Automation;
using JournalCli.Core;
using JournalCli.Infrastructure;
using Git = LibGit2Sharp;

namespace JournalCli.Cmdlets
{
    public abstract class JournalCmdletBase : CmdletBase
    {
        private const string Error = "Journal location was not provided and no default location exists. One or the other is required";

        protected JournalCmdletBase()
        {
            NativeBinaries.CopyIfNotExists();
        }

        [Parameter]
        [Obsolete("'RootDirectory' is obsolete. Use 'Location' instead.")]
        public string RootDirectory { get; set; }

        [Parameter]
        public string Location { get; set; }

        protected override void ProcessRecord()
        {
            // Just temporary until RootDirectory is removed.
            if (string.IsNullOrEmpty(Location) && !string.IsNullOrEmpty(RootDirectory))
                Location = RootDirectory;

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

        protected void Commit(GitCommitType commitType)
        {
            ValidateGitRepo();
            var message = GitCommitMessage.Get(commitType);
            CommitCore(message);
        }

        protected void Commit(string message)
        {
            ValidateGitRepo();
            CommitCore(message);
        }

        private void CommitCore(string message)
        {
            using (var repo = new Git.Repository(Location))
            {
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
        }

        private void ValidateGitRepo()
        {
            if (Git.Repository.IsValid(Location))
                return;

            Git.Repository.Init(Location);
            using (var repo = new Git.Repository(Location))
            {
                Git.Commands.Stage(repo, "*");

                var author = new Git.Signature("JournalCli", "@journalCli", DateTime.Now);
                var committer = author;

                var options = new Git.CommitOptions { PrettifyMessage = true };
                var commit = repo.Commit("Initial commit", author, committer, options);
            }
        }
    }
}