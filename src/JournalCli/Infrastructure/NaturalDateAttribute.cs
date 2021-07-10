using System;
using System.Management.Automation;
using Chronic.Core;
using Chronic.Core.Tags;
using NodaTime;
using NodaTime.Text;

namespace JournalCli.Infrastructure
{
    public class NaturalDateAttribute : ArgumentTransformationAttribute
    {
        private readonly RoundTo _round;
        private readonly Parser _parser = new(new Options { Context = Pointer.Type.Past });
        private readonly LocalDatePattern _yearPattern = LocalDatePattern.CreateWithCurrentCulture("yyyy");
        private readonly LocalDatePattern _fullMonthYearPattern = LocalDatePattern.CreateWithCurrentCulture("MMMM yyyy");
        private readonly LocalDatePattern _shortMonthYearPattern = LocalDatePattern.CreateWithCurrentCulture("MMM yyyy");

        public NaturalDateAttribute(RoundTo round) => _round = round;

        public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
        {
            var input = inputData.ToString().Replace('\\', '/');
            
            try
            {
                var yearResult = _yearPattern.Parse(input);
                if (yearResult.Success)
                    return _round == RoundTo.StartOfPeriod ? yearResult.Value : yearResult.Value.PlusYears(1).PlusDays(-1);

                var fullMonthYearResult = _fullMonthYearPattern.Parse(input);
                if (fullMonthYearResult.Success)
                    return _round == RoundTo.StartOfPeriod ? fullMonthYearResult.Value : fullMonthYearResult.Value.With(DateAdjusters.EndOfMonth);

                var shortMonthYearResult = _shortMonthYearPattern.Parse(input);
                if (shortMonthYearResult.Success)
                    return _round == RoundTo.StartOfPeriod ? shortMonthYearResult.Value : shortMonthYearResult.Value.With(DateAdjusters.EndOfMonth);

                if (DateTime.TryParse(input, out var genericResult))
                    return LocalDate.FromDateTime(genericResult);

                var naturalResult = _parser.Parse(input);
                if (naturalResult?.Start != null && naturalResult.End.HasValue)
                {
                    var start = naturalResult.Start.Value;
                    var end = naturalResult.End.Value;

                    if (start.Year != end.Year || start.Month != end.Month || start.Day != end.Day)
                    {
                        end = end.AddDays(-1);
                    }

                    var roundedNaturalResult = _round == RoundTo.StartOfPeriod ? start : end;
                    return LocalDate.FromDateTime(roundedNaturalResult);
                }
            }
            catch
            {
                // ignored
            }

            throw new PSArgumentException($"Unable to parse the phrase '{input}'. Try expressing it in another way.");
        }
    }
}