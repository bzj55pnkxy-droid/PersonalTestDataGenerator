using PersonalData.Api.Services;

namespace PersonalData.UnitTests;

/// <summary>
/// Tests that <see cref="PhoneNumberGenerator"/> picks a valid prefix and fills up to 8 digits,
/// using fake dice that always roll the lowest or the highest number.
/// </summary>
public sealed class PhoneNumberGeneratorTests
{
    [Fact]
    public void Lowest_rolls_give_20000000()
    {
        // Arrange
        var generator = new PhoneNumberGenerator(new LowestRandom());

        // Act
        var phoneNumber = generator.Generate();

        // Assert
        // The first prefix is "2", followed by seven zeros.
        Assert.Equal("20000000", phoneNumber);
    }

    [Fact]
    public void Highest_rolls_give_82999999()
    {
        // Arrange
        var generator = new PhoneNumberGenerator(new HighestRandom());

        // Act
        var phoneNumber = generator.Generate();

        // Assert
        // The last prefix is "829", followed by five nines.
        Assert.Equal("82999999", phoneNumber);
    }
}
