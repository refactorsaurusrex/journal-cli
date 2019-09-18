using System.Collections.Generic;
using NodaTime;

namespace JournalCli.Core
{
    public interface IJournalFrontMatter
    {
        ICollection<string> Tags { get; }
        string Readme { get; }
        LocalDate? ReadmeDate { get; }
        string ToString(bool asFrontMatter);
        string ToString();

        /// <summary>
        /// True if neither a readme nor tags exist. Otherwise, false.
        /// </summary>
        bool IsEmpty();

        /// <summary>
        /// True if at least one tag exists. Otherwise, false.
        /// </summary>
        bool HasTags();
    }
}