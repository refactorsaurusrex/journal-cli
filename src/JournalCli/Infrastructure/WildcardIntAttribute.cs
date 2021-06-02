using System.Management.Automation;

namespace JournalCli.Infrastructure
{
    public class WildcardIntAttribute : ArgumentTransformationAttribute
    {
        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {
            return inputData switch
            {
                string s when s == "*" => null,
                int i when i > 0 => i,
                int _ => throw new PSArgumentOutOfRangeException("Values must be greater than zero."),
                _ => throw new PSArgumentException($"Unable to parse {inputData}.")
            };
        }
    }
}