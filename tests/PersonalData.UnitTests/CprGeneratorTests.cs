using System.Globalization;
using PersonalData.Api.Models;
using PersonalData.Api.Services;

namespace PersonalData.UnitTests;

public sealed class CprGeneratorTests
{
    [Theory]
    [InlineData("2005-01-31", "310105")]
    [InlineData("2005-02-01", "010205")]
    [InlineData("2005-04-30", "300405")]
    [InlineData("2005-05-01", "010505")]
    [InlineData("2005-02-28", "280205")]
    [InlineData("2005-03-01", "010305")]
    [InlineData("2004-02-28", "280204")]
    [InlineData("2004-02-29", "290204")]
    [InlineData("2004-03-01", "010304")]
    [InlineData("2005-06-15", "150605")]
    [InlineData("2005-01-06", "060105")]
    public void GenerateStartsCprWithFormattedBirthDate(string birthDateText, string expectedDatePart)
    {
        // Arrange
        var birthDate = DateOnly.ParseExact(birthDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var generator = new CprGenerator(new LowestRandom());

        // Act
        var cpr = generator.Generate(birthDate, Gender.Female);

        // Assert
        Assert.Equal(expectedDatePart, cpr[..6]);
    }

    [Theory]
    [InlineData(Gender.Female, 0)]
    [InlineData(Gender.Male, 7)]
    [InlineData(Gender.Female, 123)]
    [InlineData(Gender.Male, 999)]
    public void GenerateReturnsExactlyTenAsciiDigits(Gender gender, int serialNumber)
    {
        // Arrange
        var generator = CreateGeneratorForSerialNumber(serialNumber);
        var birthDate = new DateOnly(2005, 1, 6);

        // Act
        var cpr = generator.Generate(birthDate, gender);

        // Assert
        Assert.Equal(10, cpr.Length);
        Assert.All(cpr, digit => Assert.InRange(digit, '0', '9'));
    }

    [Theory]
    [InlineData(Gender.Female, 0, "02468")]
    [InlineData(Gender.Female, 1, "02468")]
    [InlineData(Gender.Female, 2, "02468")]
    [InlineData(Gender.Female, 3, "02468")]
    [InlineData(Gender.Female, 4, "02468")]
    [InlineData(Gender.Male, 0, "13579")]
    [InlineData(Gender.Male, 1, "13579")]
    [InlineData(Gender.Male, 2, "13579")]
    [InlineData(Gender.Male, 3, "13579")]
    [InlineData(Gender.Male, 4, "13579")]
    public void GenerateUsesLastDigitMatchingGender(Gender gender, int lastDigitChoice, string allowedLastDigits)
    {
        // Arrange
        var generator = CreateGeneratorForLastDigitChoice(lastDigitChoice);
        var birthDate = new DateOnly(2005, 6, 15);

        // Act
        var cpr = generator.Generate(birthDate, gender);

        // Assert
        Assert.Contains(cpr[^1], allowedLastDigits);
    }

    private static CprGenerator CreateGeneratorForSerialNumber(int serialNumber)
    {
        var random = new LoadedRandom { [(0, 1000)] = serialNumber };
        return new CprGenerator(random);
    }

    private static CprGenerator CreateGeneratorForLastDigitChoice(int lastDigitChoice)
    {
        var random = new LoadedRandom { [(0, 5)] = lastDigitChoice };
        return new CprGenerator(random);
    }
}
