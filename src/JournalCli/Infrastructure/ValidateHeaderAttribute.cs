using System.Management.Automation;

namespace JournalCli.Infrastructure
{
    public class ValidateHeaderAttribute : ValidateArgumentsAttribute
    {
        protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
        {
            var header = arguments.ToString();
            if (!HeaderValidator.IsValid(header))
                throw new PSArgumentException(HeaderValidator.ErrorMessage);
        }
    }
}