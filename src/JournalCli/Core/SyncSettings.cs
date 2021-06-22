using System;
using System.Diagnostics.CodeAnalysis;
using Amazon.Runtime;

namespace JournalCli.Core
{
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public class SyncSettings : IEquatable<SyncSettings>
    {
        public string AwsProfileName { get; set; }

        public string BucketName { get; set; }

        public string AwsRegion { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(AwsProfileName) &&
                   !string.IsNullOrWhiteSpace(BucketName) &&
                   !string.IsNullOrWhiteSpace(AwsRegion);
        }

        public bool Equals(SyncSettings other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return AwsProfileName == other.AwsProfileName && BucketName == other.BucketName;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((SyncSettings)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (AwsProfileName != null ? AwsProfileName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (BucketName != null ? BucketName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (AwsRegion != null ? AwsRegion.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(SyncSettings left, SyncSettings right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SyncSettings left, SyncSettings right)
        {
            return !Equals(left, right);
        }
    }
}