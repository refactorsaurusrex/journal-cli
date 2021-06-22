using System.Management.Automation;

namespace JournalCli.Cmdlets
{
    public abstract class JournalSyncCmdletBase : JournalCmdletBase
    {
        protected const string PrivateKeyName = "JournalCliPrivateKey";
        protected string GetPrivateKey() => ScriptBlock.Create($"Get-Secret -Name '{PrivateKeyName}' -Vault 'JournalCli'").Invoke()[0].BaseObject.ToString();
        protected bool PrivateKeyExists() => ScriptBlock.Create($"Get-SecretInfo -Name '{PrivateKeyName}'").Invoke().Count != 0;
    }
}