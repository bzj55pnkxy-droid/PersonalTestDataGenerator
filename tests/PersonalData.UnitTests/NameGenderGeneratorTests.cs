using PersonalData.Api.Models;
using PersonalData.Api.Services;

namespace PersonalData.UnitTests;

/// <summary>Document section 1: select a complete name/gender row without changing its contents.</summary>
public sealed class NameGenderGeneratorTests
{
    [Theory]
    [InlineData(Gender.Female, "Annemette P.", "Nilsson")]
    [InlineData(Gender.Male, "Simon L.", "Østergaard")]
    public void GenerateReturnsNameAndGenderFromSameRow(
        Gender gender, string expectedFirstName, string expectedLastName)
    {
        // Arrange
        var generator = CreateGeneratorForGender(gender);

        // Act
        var nameAndGender = generator.Generate();

        // Assert
        Assert.Equal(new NameAndGender(expectedFirstName, expectedLastName, gender), nameAndGender);
    }

    [Fact]
    public void GenerateThrowsWhenNoNamesAreAvailable()
    {
        // Arrange
        var generator = new NameGenderGenerator(new StubPersonNameRepository(), new LowestRandom());

        // Act
        Action generateName = () => generator.Generate();

        // Assert
        Assert.Throws<InvalidOperationException>(generateName);
    }

    private static NameGenderGenerator CreateGeneratorForGender(Gender gender)
    {
        var names = new StubPersonNameRepository(
            new NameAndGender("Annemette P.", "Nilsson", Gender.Female),
            new NameAndGender("Simon L.", "Østergaard", Gender.Male));
        var selectedRow = gender == Gender.Female ? 0 : 1;
        var random = new LoadedRandom { [(0, 2)] = selectedRow };
        return new NameGenderGenerator(names, random);
    }
}
