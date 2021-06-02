using System.Collections.Generic;

namespace JournalCli.Infrastructure
{
    public interface IMarkdownFiles
    {
        List<string> FindAll(bool fileNamesOnly = false);
    }
}