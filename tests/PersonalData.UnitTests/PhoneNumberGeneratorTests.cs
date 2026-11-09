using PersonalData.Api.Services;

namespace PersonalData.UnitTests;

public sealed class PhoneNumberGeneratorTests
{
    [Theory]
    [InlineData("2", 0, "20000000")]
    [InlineData("2", 1, "21111111")]
    [InlineData("2", 9, "29999999")]
    [InlineData("30", 0, "30000000")]
    [InlineData("30", 1, "30111111")]
    [InlineData("30", 9, "30999999")]
    [InlineData("344", 0, "34400000")]
    [InlineData("344", 1, "34411111")]
    [InlineData("344", 9, "34499999")]
    public void GenerateReturnsEightDigitsWithAllowedPrefix(
        string prefix, int suffixDigit, string expected)
    {
        // Arrange
        var generator = CreateGeneratorForPrefix(prefix, suffixDigit);

        // Act
        var phoneNumber = generator.Generate();

        // Assert
        Assert.Equal(expected, phoneNumber);
    }

    private static PhoneNumberGenerator CreateGeneratorForPrefix(string prefix, int suffixDigit)
    {
        var prefixChoice = prefix switch
        {
            "2" => 0,
            "30" => 1,
            "344" => 18,
            _ => throw new ArgumentOutOfRangeException(nameof(prefix))
        };
        var random = new LoadedRandom { [(0, 94)] = prefixChoice, [(0, 10)] = suffixDigit };
        return new PhoneNumberGenerator(random);
    }
}
