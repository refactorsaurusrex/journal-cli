using System;
using System.Management.Automation;

namespace JournalCli.Infrastructure
{
    public class ValidateReadmeAttribute : ValidateArgumentsAttribute
    {
        protected override void Validate(object arguments, EngineIntrinsics engineIntrinsics)
        {
            var exp = arguments.ToString();
            var parser = new ReadmeParser(arguments.ToString());
            if (parser.IsValid) return;
            var message = $"{Environment.NewLine}The value you entered '{exp}' is not a valid readme expression. Valid expressions are specific dates, " +
                          "such as 12/19/2021, or a time period written in the form of '{integer} {time period}', where 'integer' " +
                          "is any positive number greater than zero and 'time period' is either 'days', 'weeks', 'months', or 'years'. " +
                          "For example, '15 weeks' or '12 years'.";
            throw new PSArgumentException(message.Wrap());
        }
    }
}