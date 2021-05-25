using System;
using System.Diagnostics.CodeAnalysis;
using JournalCli.Infrastructure;

namespace JournalCli.Core
{
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    internal class UserSettings : IEquatable<UserSettings>
    {
        public static UserSettings Load(IEncryptedStore<UserSettings> encryptedStore) => encryptedStore.Load();

        public string DefaultJournalRoot { get; set; }

        [Obsolete("Time for this to go")]
        public string BackupLocation { get; set; }

        [Obsolete("Time for this to go")]
        public string BackupPassword { get; set; }

        public bool HideWelcomeScreen { get; set; }

        public DateTime? NextUpdateCheck { get; set; }

        [Obsolete("If the backup password is removed, do we really need to encrypt the file?")]
        public void Save(IEncryptedStore<UserSettings> encryptedStore) => encryptedStore.Save(this);

        public bool Equals(UserSettings other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(DefaultJournalRoot, other.DefaultJournalRoot) && 
                string.Equals(BackupLocation, other.BackupLocation) &&
                string.Equals(BackupPassword, other.BackupPassword);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((UserSettings)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = DefaultJournalRoot != null ? DefaultJournalRoot.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (BackupLocation != null ? BackupLocation.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (BackupPassword != null ? BackupPassword.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}