using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Schema;
using Amazon;
using Amazon.Extensions.S3.Encryption;
using Amazon.Extensions.S3.Encryption.Primitives;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon.S3;
using Amazon.S3.Model;

namespace JournalCli.Core
{
    public class JournalSynchronizer
    {
        private readonly SyncSettings _settings;
        private readonly AmazonS3EncryptionClientV2 _s3Client;

        public JournalSynchronizer(string privateKey, SyncSettings settings)
        {
            _settings = settings;
            var rsa = RSA.Create();
            rsa.FromXmlString(privateKey);
            
            var encryptionMaterials = new EncryptionMaterialsV2(rsa, AsymmetricAlgorithmType.RsaOaepSha1);
            var config = new AmazonS3CryptoConfigurationV2(SecurityProfile.V2)
            {
                StorageMode = CryptoStorageMode.ObjectMetadata,
                RegionEndpoint = RegionEndpoint.GetBySystemName(settings.AwsRegion)
            };

            // TEST: Validate flow if profile name is missing or wrong
            new SharedCredentialsFile().TryGetProfile(settings.AwsProfileName, out var profile);

            if (profile == null)
                throw new InvalidOperationException($"Unable to locate the AWS profile named '{settings.AwsProfileName}'.");
            
            var credentials = profile.GetAWSCredentials(null);
            _s3Client = new AmazonS3EncryptionClientV2(credentials, config, encryptionMaterials);
        }

        public void Sync(bool force = false)
        {
            // TODO: In anticipation of "named" journals, be sure to create root-level directories in S3 that represent a named journal. 
            // The first named directory should just be "default".
            
            
            // 1. Pull everything from S3, merge in memory with local entries on disk
            // 2. Missing from S3? Upload. Missing from disk, download. Entry is in both places? Will have to ask user what to do. 
            // 3. Allow user to abort entire process if conflicts are found.
            // 4. Remember, entries are versioned in S3 but not on disk, so uploading is always safer than overwriting on disk.
        }

        public async Task<string> CreateBucket()
        {
            var bucketName = $"journal-cli-{Guid.NewGuid()}";
            var bucketRequest = new PutBucketRequest
            {
                UseClientRegion = true,
                BucketName = bucketName
            };
            
            var bucketResponse = await _s3Client.PutBucketAsync(bucketRequest);
            if (bucketResponse.HttpStatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException($"Failed to create new bucket '{bucketName}'. Received status code '{bucketResponse.HttpStatusCode}'.");
            
            var encryptionRequest = new PutBucketEncryptionRequest
            {
                BucketName = bucketName,
                ServerSideEncryptionConfiguration = new ServerSideEncryptionConfiguration
                {
                    ServerSideEncryptionRules = new List<ServerSideEncryptionRule>
                    {
                        new()
                        {
                            ServerSideEncryptionByDefault = new ServerSideEncryptionByDefault
                            {
                                ServerSideEncryptionAlgorithm = ServerSideEncryptionMethod.AES256
                            }
                        }
                    }
                }
            };
            
            var encryptionResponse = await _s3Client.PutBucketEncryptionAsync(encryptionRequest);
            if (encryptionResponse.HttpStatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException($"Failed to apply server-side encryption to bucket '{bucketName}'. Received status code '{encryptionResponse.HttpStatusCode}'.");
            
            var versioningRequest = new PutBucketVersioningRequest()
            {
                BucketName = bucketName,
                VersioningConfig = new S3BucketVersioningConfig()
                {
                    Status = VersionStatus.Enabled
                }
            };

            var versioningResponse = await _s3Client.PutBucketVersioningAsync(versioningRequest);
            if (versioningResponse.HttpStatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException($"Failed to enable object versioning for bucket '{bucketName}'. Received status code '{versioningResponse.HttpStatusCode}'.");
            
            return bucketName;
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