using PersonalData.Api.Models;
using PersonalData.Api.Services;

namespace PersonalData.UnitTests;

public sealed class AddressGeneratorTests
{
    private const int RightDoorChoice = 1;
    private const int LeftDoorChoice = 8;
    private const int MiddleDoorChoice = 15;

    private static readonly StubPostalCodeRepository Towns = new(
        new PostalTown("2100", "København Ø"),
        new PostalTown("9990", "Skagen"));

    [Theory]
    [InlineData(1, null, "1")]
    [InlineData(999, null, "999")]
    [InlineData(1, 'A', "1A")]
    [InlineData(999, 'Z', "999Z")]
    [InlineData(43, 'B', "43B")]
    public void GenerateReturnsHouseNumberWithOptionalUppercaseLetter(
        int number, char? letter, string expected)
    {
        // Arrange
        var generator = CreateGeneratorForHouseNumber(number, letter);

        // Act
        var address = generator.Generate();

        // Assert
        Assert.Equal(expected, address.Number);
    }

    [Theory]
    [InlineData(null, "st")]
    [InlineData(1, "1")]
    [InlineData(50, "50")]
    [InlineData(99, "99")]
    public void GenerateReturnsGroundFloorOrValidFloorNumber(int? floorNumber, string expected)
    {
        // Arrange
        var generator = CreateGeneratorForFloor(floorNumber);

        // Act
        var address = generator.Generate();

        // Assert
        Assert.Equal(expected, address.Floor);
    }

    [Theory]
    [InlineData(RightDoorChoice, "th")]
    [InlineData(LeftDoorChoice, "tv")]
    [InlineData(MiddleDoorChoice, "mf")]
    public void GenerateReturnsNamedDoorPosition(int doorChoice, string expected)
    {
        // Arrange
        var generator = CreateGeneratorForNamedDoor(doorChoice);

        // Act
        var address = generator.Generate();

        // Assert
        Assert.Equal(expected, address.Door);
    }

    [Theory]
    [InlineData(1, "1")]
    [InlineData(25, "25")]
    [InlineData(50, "50")]
    public void GenerateReturnsValidNumericDoor(int number, string expected)
    {
        // Arrange
        var generator = CreateGeneratorForNumericDoor(number);

        // Act
        var address = generator.Generate();

        // Assert
        Assert.Equal(expected, address.Door);
    }

    [Theory]
    [InlineData(false, 3, "c3")]
    [InlineData(false, 14, "c14")]
    [InlineData(false, 123, "c123")]
    [InlineData(true, 3, "c-3")]
    [InlineData(true, 14, "c-14")]
    [InlineData(true, 123, "c-123")]
    public void GenerateReturnsLetterDoorWithOneToThreeDigitsAndOptionalDash(
        bool hasDash, int number, string expected)
    {
        // Arrange
        var generator = CreateGeneratorForLetterDoor(hasDash, number);

        // Act
        var address = generator.Generate();

        // Assert
        Assert.Equal(expected, address.Door);
    }

    [Theory]
    [InlineData("2100", "København Ø")]
    [InlineData("9990", "Skagen")]
    public void GenerateKeepsPostalCodeAndTownFromSameRow(string expectedPostalCode, string expectedTown)
    {
        // Arrange
        var generator = CreateGeneratorForTown(expectedPostalCode);

        // Act
        var address = generator.Generate();

        // Assert
        Assert.Equal((expectedPostalCode, expectedTown), (address.PostalCode, address.TownName));
    }

    [Theory]
    [InlineData('a')] // The formerly failing lowest choice must not fill the street with spaces.
    [InlineData('z')]
    [InlineData('A')]
    [InlineData('æ')]
    [InlineData('ø')]
    [InlineData('å')]
    [InlineData('Æ')]
    [InlineData('Ø')]
    [InlineData('Å')]
    public void GenerateReturnsNonEmptyAlphabeticStreet(char streetCharacter)
    {
        // Arrange
        var generator = CreateGeneratorForStreetCharacter(streetCharacter);

        // Act
        var address = generator.Generate();

        // Assert
        Assert.NotEmpty(address.Street);
        Assert.All(address.Street, character => Assert.True(char.IsLetter(character)));
    }

    [Fact]
    public void GenerateThrowsWhenNoPostalCodesAreAvailable()
    {
        // Arrange
        var generator = new AddressGenerator(new StubPostalCodeRepository(), new LowestRandom());

        // Act
        Action generateAddress = () => generator.Generate();

        // Assert
        Assert.Throws<InvalidOperationException>(generateAddress);
    }

    private static AddressGenerator CreateGeneratorForHouseNumber(int number, char? letter)
    {
        var random = new LoadedRandom
        {
            [(1, 1000)] = number,
            [(0, 100)] = letter.HasValue ? 0 : 99,
            [(0, 26)] = letter.HasValue ? letter.Value - 'A' : 0
        };
        return new AddressGenerator(Towns, random);
    }

    private static AddressGenerator CreateGeneratorForFloor(int? floorNumber)
    {
        var random = new LoadedRandom
        {
            [(0, 100)] = floorNumber.HasValue ? 99 : 0,
            [(1, 100)] = floorNumber ?? 1
        };
        return new AddressGenerator(Towns, random);
    }

    private static AddressGenerator CreateGeneratorForNamedDoor(int doorChoice)
    {
        var random = new LoadedRandom { [(1, 21)] = doorChoice };
        return new AddressGenerator(Towns, random);
    }

    private static AddressGenerator CreateGeneratorForNumericDoor(int number)
    {
        var random = new LoadedRandom { [(1, 21)] = 17, [(1, 51)] = number };
        return new AddressGenerator(Towns, random);
    }

    private static AddressGenerator CreateGeneratorForLetterDoor(bool hasDash, int number)
    {
        var random = new LoadedRandom
        {
            [(1, 21)] = hasDash ? 20 : 19,
            [(0, 29)] = 2, // Select the letter 'c'.
            [(1, 1000)] = number
        };
        return new AddressGenerator(Towns, random);
    }

    private static AddressGenerator CreateGeneratorForTown(string postalCode)
    {
        var townChoice = postalCode switch
        {
            "2100" => 0,
            "9990" => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(postalCode))
        };
        var random = new LoadedRandom { [(0, 2)] = townChoice };
        return new AddressGenerator(Towns, random);
    }

    private static AddressGenerator CreateGeneratorForStreetCharacter(char streetCharacter)
    {
        const string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZæøåÆØÅ";
        var random = new LoadedRandom { [(0, 58)] = characters.IndexOf(streetCharacter) };
        return new AddressGenerator(Towns, random);
    }
}
