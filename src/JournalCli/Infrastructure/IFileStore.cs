namespace JournalCli.Infrastructure
{
    internal interface IFileStore<T>
        where T : class, new()
    {
        void Save(T target);
        T Load();
        string FilePath { get; }
    }
}