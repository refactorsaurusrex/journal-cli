using System;
using System.Diagnostics.CodeAnalysis;

namespace JournalCli.Core
{
    [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
    internal class SystemSettings : IEquatable<SystemSettings>
    {
        public bool HideWelcomeScreen { get; set; }

        public DateTime? NextUpdateCheck { get; set; }

        public bool Equals(SystemSettings other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return HideWelcomeScreen == other.HideWelcomeScreen && Nullable.Equals(NextUpdateCheck, other.NextUpdateCheck);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj.GetType() == this.GetType() && Equals((SystemSettings)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (HideWelcomeScreen.GetHashCode() * 397) ^ NextUpdateCheck.GetHashCode();
            }
        }

        public static bool operator ==(SystemSettings left, SystemSettings right) => Equals(left, right);

        public static bool operator !=(SystemSettings left, SystemSettings right) => !Equals(left, right);
    }
}