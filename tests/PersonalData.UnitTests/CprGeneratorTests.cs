using PersonalData.Api.Models;
using PersonalData.Api.Services;

namespace PersonalData.UnitTests;

/// <summary>
/// Tests that <see cref="CprGenerator"/> builds the CPR number from the birth date, three random digits
/// and a gender digit, using fake dice that always roll the lowest or the highest number.
/// </summary>
public sealed class CprGeneratorTests
{
    // 06-01-2005 gives "060105". Day and month differ, so it also proves the day comes before the month.
    private static readonly DateOnly BirthDate = new(2005, 1, 6);

    // CPR = date (ddMMyy), then 3 digits 000-999, then a last digit that is even for female and odd for male.
    [Theory]
    [InlineData(Gender.Female, "0601050000")]
    [InlineData(Gender.Male, "0601050001")]
    public void Lowest_rolls_give_the_lowest_cpr_for_the_gender(Gender gender, string expectedCpr)
    {
        // Arrange
        var generator = new CprGenerator(new LowestRandom());

        // Act
        var cpr = generator.Generate(BirthDate, gender);

        // Assert
        Assert.Equal(expectedCpr, cpr);
    }

    [Theory]
    [InlineData(Gender.Female, "0601059998")]
    [InlineData(Gender.Male, "0601059999")]
    public void Highest_rolls_give_the_highest_cpr_for_the_gender(Gender gender, string expectedCpr)
    {
        // Arrange
        var generator = new CprGenerator(new HighestRandom());

        // Act
        var cpr = generator.Generate(BirthDate, gender);

        // Assert
        Assert.Equal(expectedCpr, cpr);
    }
}
