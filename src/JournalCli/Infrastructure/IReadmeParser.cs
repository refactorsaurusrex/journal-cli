using NodaTime;

namespace JournalCli.Infrastructure
{
    public interface IReadmeParser
    {
        bool IsValid { get; }
        ReadmeParser.TimePeriod Period { get; }
        LocalDate? ExactDate { get; }
        int PeriodDuration { get; }
        ReadmeExpression ToExpression(LocalDate relativeTo);
    }
}