using System.Management.Automation;

namespace JournalCli.Infrastructure
{
    public class ValidateHeaderAttribute : ValidateArgumentsAttribute
    {
        protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
        {
            var header = arguments.ToString();
            if (!HeaderValidator.IsValid(header))
                throw new PSArgumentException("Header must start with between 1 and 6 '#' characters, followed by a single space and the header text.");
        }
    }
}