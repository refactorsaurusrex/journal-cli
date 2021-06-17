using JournalCli.Core;
using Xunit;

namespace JournalCli.Tests
{
    public class JournalSynchronizerTests
    {
        [Fact]
        public void CreateBucket_CreatesNewBucket()
        {
            var key = JournalSynchronizer.CreatePrivateKey();
            var settings = new SyncSettings
            {
                AwsRegion = "us-east-1",
                AwsProfileName = "Default" 
            };
            var sync = new JournalSynchronizer(key, settings);
            sync.CreateBucket();
        }
    }
}