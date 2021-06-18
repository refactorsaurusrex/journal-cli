using System;
using System.Threading.Tasks;
using JournalCli.Core;
using Xunit;

namespace JournalCli.Tests
{
    public class JournalSynchronizerTests
    {
        [Fact]
        public async Task CreateBucket_CreatesNewBucket()
        {
            throw new Exception("enable mocking this");
            // var key = JournalSynchronizer.CreatePrivateKey();
            // var settings = new SyncSettings
            // {
            //     AwsRegion = "us-east-1",
            //     AwsProfileName = "default" 
            // };
            // var sync = new JournalSynchronizer(key, settings);
            // await sync.CreateBucket();
        }
    }
}