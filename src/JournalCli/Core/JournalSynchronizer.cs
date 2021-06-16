using System.Security.Cryptography;
using Amazon.Extensions.S3.Encryption;
using Amazon.Extensions.S3.Encryption.Primitives;
using Amazon.Runtime.CredentialManagement;

namespace JournalCli.Core
{
    public class JournalSynchronizer
    {
        private readonly SyncSettings _settings;
        private AmazonS3EncryptionClientV2 _s3Client;

        public JournalSynchronizer(string privateKey, SyncSettings settings)
        {
            _settings = settings;
            var rsa = RSA.Create();
            rsa.FromXmlString(privateKey);
            
            var encryptionMaterials = new EncryptionMaterialsV2(rsa, AsymmetricAlgorithmType.RsaOaepSha1);
            var config = new AmazonS3CryptoConfigurationV2(SecurityProfile.V2)
            {
                StorageMode = CryptoStorageMode.ObjectMetadata
            };

            // TEST: Validate flow if profile name is missing or wrong
            new SharedCredentialsFile().TryGetProfile(settings.AwsProfileName, out var profile);
            var credentials = profile.GetAWSCredentials(null);
            _s3Client = new AmazonS3EncryptionClientV2(credentials, config, encryptionMaterials);
        }

        public void Sync(bool force = false)
        {
            // 1. Pull everything from S3, merge in memory with local entries on disk
            // 2. Missing from S3? Upload. Missing from disk, download. Entry is in both places? Will have to ask user what to do. 
            // 3. Allow user to abort entire process if conflicts are found.
            // 4. Remember, entries are versioned in S3 but not on disk, so uploading is always safer than overwriting on disk.
            

        }

        public void CreateOrVerifyBucket()
        {
            // Be sure to create a "Default" directory in the bucket to allow for named journals in the future.
            // _s3Client.Putbucket
        }
        
        // Add ability to list versions of specific files and show diff
        // Allow replacing an entry with a previous version? Might be an easy way to resolve newer entries accidentally being overwritten by older ones.

        public static string CreatePrivateKey()
        {
            var rsa = RSA.Create();
            return rsa.ToXmlString(true);
        }
    }
}