using System;
using System.IO;
using System.Management.Automation;
using ICSharpCode.SharpZipLib.Zip;
using JetBrains.Annotations;

namespace JournalCli.Commands
{
    /// <summary>
    /// <para type="synopsis">Creates a complete backup of your journal entries.</para>
    /// <para type="description">Creates a password-protected zip archive of all files contained in the journal root directory and
    /// saves it to the specified location.</para>
    /// <para type="link" uri="https://github.com/refactorsaurusrex/journal-cli/wiki">The journal-cli wiki.</para>
    /// <example>
    ///   <code>Backup-Journal</code>
    ///   <para>Backup your journal using previously saved location and password. Note that this will fail a password and location
    ///   have not been previously saved.</para>
    /// </example>
    /// <example>
    ///   <code>Backup-Journal -BackupLocation C:\My\Secret\Location -SaveLocation -Password "secret123" -SavePassword</code>
    ///   <para>Backup your journal to specified location with provided password, and persist location and password for future use.</para>
    /// </example>
    /// </summary>
    [PublicAPI]
    [Cmdlet(VerbsData.Backup, "Journal")]
    public class BackupJournalCmdlet : JournalCmdletBase
    {
        [Parameter]
        public string Blah { get; set; }
        [Parameter]
        public string BackupLocation { get; set; }

        [Parameter]
        public string Password { get; set; }

        [Parameter]
        public SwitchParameter SaveLocation { get; set; }

        [Parameter]
        public SwitchParameter SavePassword { get; set; }

        /// <inheritdoc />
        protected override void ProcessRecord()
        {
            ResolveBackupLocation();
            ResolvePassword();

            var fileName = $"{DateTime.Now:yyyy.MM.dd.H.mm.ss.FFF}.zip";
            var destinationPath = Path.Combine(BackupLocation, fileName);
            var journalRoot = GetResolvedRootDirectory();

            var zip = new FastZip { CreateEmptyDirectories = true, Password = Password };
            zip.CreateZip(destinationPath, journalRoot, true, null);
        }

        private void ResolveBackupLocation()
        {
            if (string.IsNullOrEmpty(BackupLocation))
            {
                var settings = UserSettings.Load();
                if (string.IsNullOrEmpty(settings.BackupLocation))
                    throw new PSInvalidOperationException("Backup location not provided and no location was previously saved.");

                BackupLocation = settings.BackupLocation;
            }
            else
            {
                if (!Directory.Exists(BackupLocation))
                    Directory.CreateDirectory(BackupLocation);

                BackupLocation = ResolvePath(BackupLocation);
                if (SaveLocation)
                {
                    var settings = UserSettings.Load();
                    settings.BackupLocation = BackupLocation;
                    settings.Save();
                }
            }
        }

        private void ResolvePassword()
        {
            if (string.IsNullOrEmpty(Password))
            {
                var settings = UserSettings.Load();
                if (string.IsNullOrEmpty(settings.BackupPassword))
                {
                    Password = null;
                    return;
                }

                Password = settings.BackupPassword;
            }
            else
            {
                if (SavePassword)
                {
                    var settings = UserSettings.Load();
                    settings.BackupPassword = Password;
                    settings.Save();
                }
            }
        }
    }
}
