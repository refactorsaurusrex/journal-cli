using System;
using System.Diagnostics.CodeAnalysis;

namespace JournalCli.Core
{
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    internal class UserSettings : IEquatable<UserSettings>
    {
        public string DefaultJournalRoot { get; set; }

        public bool Equals(UserSettings other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(this, other) || string.Equals(DefaultJournalRoot, other.DefaultJournalRoot);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((UserSettings)obj);
        }

        public override int GetHashCode() => DefaultJournalRoot != null ? DefaultJournalRoot.GetHashCode() : 0;

        public static bool operator ==(UserSettings left, UserSettings right) => Equals(left, right);

        public static bool operator !=(UserSettings left, UserSettings right) => !Equals(left, right);
    }
}