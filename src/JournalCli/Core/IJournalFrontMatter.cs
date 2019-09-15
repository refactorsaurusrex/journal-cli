using System.Collections.Generic;

namespace JournalCli.Core
{
    public interface IJournalFrontMatter
    {
        ICollection<string> Tags { get; }
        string Readme { get; }
        string ToString(bool asFrontMatter);
        string ToString();
    }
}