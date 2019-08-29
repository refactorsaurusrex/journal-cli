namespace JournalCli
{
    internal class UserSettings
    {
        public static UserSettings Load(IEncryptedStore<UserSettings> encryptedStore) => encryptedStore.Load();

        public string DefaultJournalRoot { get; set; }

        public string BackupLocation { get; set; }

        public string BackupPassword { get; set; }

        public void Save(IEncryptedStore<UserSettings> encryptedStore) => encryptedStore.Save(this);
    }
}