﻿using System.Management.Automation;
using JetBrains.Annotations;
using JournalCli.Core;
using JournalCli.Infrastructure;

namespace JournalCli.Cmdlets
{
    [PublicAPI]
    [Cmdlet(VerbsCommon.Get, "JournalDefaultLocation")]
    [OutputType(typeof(string))]
    public class GetJournalDefaultLocationCmdlet : CmdletBase
    {
        protected override void ProcessRecord()
        {
            var encryptedStore = EncryptedStoreFactory.Create<UserSettings>();
            var settings = UserSettings.Load(encryptedStore);
            WriteObject(settings.DefaultJournalRoot);
        }
    }
}