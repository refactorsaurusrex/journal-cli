using System;
using FluentAssertions;
using JournalCli.Infrastructure;
using Xunit;

namespace JournalCli.Tests
{
    public class GitCommitMessageTests
    {
        [Fact]
        public void Get_ReturnsCommitMessage_ForEveryValidScenario()
        {
            foreach (var commitType in (GitCommitType[])Enum.GetValues(typeof(GitCommitType)))
            {
                var result = GitCommitMessage.Get(commitType);
                result.Should().NotBeNullOrEmpty();
            }
        }
    }
}
