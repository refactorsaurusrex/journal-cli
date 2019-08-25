namespace JournalCli
{
    internal interface IEncryptedStore
    {
        void Save<T>(T target);
        T Load<T>() where T : class;
    }
}