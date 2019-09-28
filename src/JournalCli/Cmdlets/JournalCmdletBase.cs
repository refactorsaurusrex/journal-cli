using System;
using System.Management.Automation;
using JournalCli.Core;
using JournalCli.Infrastructure;
using Git = LibGit2Sharp;

namespace JournalCli.Cmdlets
{
    public abstract class JournalCmdletBase : CmdletBase
    {
        private readonly string _error = $"{nameof(RootDirectory)} was not provided and no default location exists. One or the other is required";

        protected JournalCmdletBase()
        {
            NativeBinaries.CopyIfNotExists();
        }

        [Parameter]
        public string RootDirectory { get; set; }

        protected override void ProcessRecord()
        {
            // TODO: Can this logic be move to the constructor so it's not done for each record processed?
            if (!string.IsNullOrEmpty(RootDirectory))
            {
                RootDirectory = ResolvePath(RootDirectory);
                return;
            }

            var encryptedStore = EncryptedStoreFactory.Create<UserSettings>();
            var settings = UserSettings.Load(encryptedStore);

            if (string.IsNullOrEmpty(settings.DefaultJournalRoot))
                throw new PSInvalidOperationException(_error);

            RootDirectory = settings.DefaultJournalRoot;
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

        protected void RevertHead()
        {
            throw new NotImplementedException();
        }

        protected void RevertLast(GitCommitType commitType)
        {
            throw new NotImplementedException();
        }

        private void CommitCore(string message)
        {
            using (var repo = new Git.Repository(RootDirectory))
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
            if (Git.Repository.IsValid(RootDirectory))
                return;

            Git.Repository.Init(RootDirectory);
            using (var repo = new Git.Repository(RootDirectory))
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