using System;
using System.Diagnostics.CodeAnalysis;
using NodaTime;

namespace JournalCli.Core
{
    internal class SystemSettings : IEquatable<SystemSettings>
    {
        public bool HideWelcomeScreen { get; set; }

        public LocalDate? NextUpdateCheck { get; set; }

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
                // ReSharper disable NonReadonlyMemberInGetHashCode
                return (HideWelcomeScreen.GetHashCode() * 397) ^ NextUpdateCheck.GetHashCode();
                // ReSharper restore NonReadonlyMemberInGetHashCode
            }
        }

        public static bool operator ==(SystemSettings left, SystemSettings right) => Equals(left, right);

        public static bool operator !=(SystemSettings left, SystemSettings right) => !Equals(left, right);
    }
}