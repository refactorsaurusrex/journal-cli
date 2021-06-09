using System;
using System.Diagnostics.CodeAnalysis;

namespace JournalCli.Core
{
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    public class SyncSettings : IEquatable<SyncSettings>
    {
        public string AwsProfileName { get; set; }
        
        public string BucketName { get; set; }

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
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SyncSettings)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((AwsProfileName != null ? AwsProfileName.GetHashCode() : 0) * 397) ^ (BucketName != null ? BucketName.GetHashCode() : 0);
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