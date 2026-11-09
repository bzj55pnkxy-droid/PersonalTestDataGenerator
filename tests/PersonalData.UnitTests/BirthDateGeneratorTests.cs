using PersonalData.Api.Services;

namespace PersonalData.UnitTests;

/// <summary>
/// Tests that <see cref="BirthDateGenerator"/> stays within the last 100 years, using a fixed clock
/// and fake dice that always roll the lowest or the highest number.
/// </summary>
public sealed class BirthDateGeneratorTests
{
    private static readonly FixedTimeProvider Today = new(new DateTimeOffset(2026, 6, 15, 12, 0, 0, TimeSpan.Zero));

    // Both ends are included: the assignment's range is today - 100 years up to and including today.
    [Fact]
    public void Lowest_roll_gives_the_date_100_years_ago()
    {
        // Arrange
        var generator = new BirthDateGenerator(new LowestRandom(), Today);

        // Act
        var birthDate = generator.Generate();

        // Assert
        Assert.Equal(new DateOnly(1926, 6, 15), birthDate);
    }

    [Fact]
    public void Highest_roll_gives_today()
    {
        // Arrange
        var generator = new BirthDateGenerator(new HighestRandom(), Today);

        // Act
        var birthDate = generator.Generate();

        // Assert
        Assert.Equal(new DateOnly(2026, 6, 15), birthDate);
    }
}
