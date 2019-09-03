namespace JournalCli.Infrastructure
{
    internal interface IEncryptedStore<T>
        where T : class, new()
    {
        void Save(T target);
        T Load();
    }
}