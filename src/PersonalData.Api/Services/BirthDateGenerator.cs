namespace PersonalData.Api.Services;

public sealed class BirthDateGenerator(Random random, TimeProvider timeProvider) : IBirthDateGenerator
{
    public DateOnly Generate()
    {
        var today = DateOnly.FromDateTime(timeProvider.GetLocalNow().DateTime);
        var earliestDate = today.AddYears(-100);
        var numberOfDays = today.DayNumber - earliestDate.DayNumber + 1;

        return earliestDate.AddDays(random.Next(numberOfDays));
    }
}
