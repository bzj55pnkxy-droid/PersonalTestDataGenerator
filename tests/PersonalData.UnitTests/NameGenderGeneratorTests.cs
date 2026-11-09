using PersonalData.Api.Models;
using PersonalData.Api.Services;

namespace PersonalData.UnitTests;

/// <summary>
/// Tests that <see cref="NameGenderGenerator"/> picks a name from the list, using a stub name list
/// and fake dice that always roll the lowest or the highest number.
/// </summary>
public sealed class NameGenderGeneratorTests
{
    private static readonly NameAndGender Annemette = new("Annemette P.", "Nilsson", Gender.Female);
    private static readonly NameAndGender Jens = new("Jens", "Hansen", Gender.Male);

    [Fact]
    public void Lowest_roll_picks_the_first_name_in_the_list()
    {
        // Arrange
        var generator = new NameGenderGenerator(new StubPersonNameRepository(Annemette, Jens), new LowestRandom());

        // Act
        var nameAndGender = generator.Generate();

        // Assert
        Assert.Equal(Annemette, nameAndGender);
    }

    [Fact]
    public void Highest_roll_picks_the_last_name_in_the_list()
    {
        // Arrange
        var generator = new NameGenderGenerator(new StubPersonNameRepository(Annemette, Jens), new HighestRandom());

        // Act
        var nameAndGender = generator.Generate();

        // Assert
        Assert.Equal(Jens, nameAndGender);
    }

    [Fact]
    public void No_names_throws()
    {
        // Arrange
        var generator = new NameGenderGenerator(new StubPersonNameRepository(), new LowestRandom());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => generator.Generate());
    }
}
