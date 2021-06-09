using System.IO;
using System.Reflection;

namespace JournalCli.Infrastructure
{
    public static class EmbeddedResource
    {
        public static string Get(string fullyQualifiedName)
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(fullyQualifiedName);
            using var reader = new StreamReader(stream!);
            return reader.ReadToEnd();
        }
    }
}