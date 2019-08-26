namespace JournalCli
{
    internal class UserSettings
    {
        public static UserSettings Load(IEncryptedStore encryptedStore) => encryptedStore.Load<UserSettings>();

        public string DefaultJournalRoot { get; set; }

        public string BackupLocation { get; set; }

        public string BackupPassword { get; set; }

        public void Save(IEncryptedStore encryptedStore) => encryptedStore.Save(this);
    }
}