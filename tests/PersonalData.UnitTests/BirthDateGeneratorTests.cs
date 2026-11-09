using System.Globalization;
using PersonalData.Api.Services;

namespace PersonalData.UnitTests;

public sealed class BirthDateGeneratorTests
{
    [Theory]
    [InlineData("2005-01-31")]
    [InlineData("2005-02-01")]
    [InlineData("2005-04-30")]
    [InlineData("2005-05-01")]
    [InlineData("2005-02-28")]
    [InlineData("2005-03-01")]
    [InlineData("2004-02-28")]
    [InlineData("2004-02-29")]
    [InlineData("2004-03-01")]
    public void GenerateReturnsValidDateAtCalendarBoundary(string expectedDateText)
    {
        // Arrange
        var expectedDate = DateOnly.ParseExact(expectedDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var generator = CreateGeneratorForDate(expectedDate);

        // Act
        var birthDate = generator.Generate();

        // Assert
        Assert.Equal(expectedDate, birthDate);
    }

    private static BirthDateGenerator CreateGeneratorForDate(DateOnly date)
    {
        var today = new DateOnly(2026, 6, 15);
        var clock = new FixedTimeProvider(new DateTimeOffset(2026, 6, 15, 12, 0, 0, TimeSpan.Zero));
        // These offsets only steer the random selection; the assertions do not require this date range.
        var earliestDate = today.AddYears(-100);
        var random = new LoadedRandom
        {
            [(0, today.DayNumber - earliestDate.DayNumber + 1)] = date.DayNumber - earliestDate.DayNumber
        };
        return new BirthDateGenerator(random, clock);
    }
}
