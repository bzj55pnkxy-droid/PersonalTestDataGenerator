using PersonalData.Api.Models;
using PersonalData.Api.Services;

namespace PersonalData.UnitTests;

/// <summary>
/// Tests AddressGenerator with fake dice. The lowest/highest rolls check both ends of every range;
/// the loaded rolls reach the branches those two never take (door kinds, door number, floor number)
/// - that is the white-box part, found via coverage.
/// </summary>
public sealed class AddressGeneratorTests
{
    private static readonly StubPostalCodeRepository Towns = new(
        new PostalTown("2100", "København Ø"),
        new PostalTown("9990", "Skagen"));

    [Fact]
    public void Lowest_rolls_give_the_lowest_address()
    {
        // Arrange
        var generator = new AddressGenerator(Towns, new LowestRandom());

        // Act
        var address = generator.Generate();

        // Assert
        // The street is 40 characters: the first must be a letter so it is 'a',
        // the rest are the lowest character, a space.
        Assert.Equal(
            new Address("a" + new string(' ', 39), "1A", "st", "th", "2100", "København Ø"),
            address);
    }

    [Fact]
    public void Highest_rolls_give_the_highest_address()
    {
        // Arrange
        var generator = new AddressGenerator(Towns, new HighestRandom());

        // Act
        var address = generator.Generate();

        // Assert
        Assert.Equal(
            new Address(new string('Å', 40), "999", "99", "å-999", "9990", "Skagen"),
            address);
    }

    // The generator rolls 1-20 for the kind of door:
    // 1-7 th, 8-14 tv, 15-16 mf, 17-18 a number 1-50, 19 letter + number, 20 letter-dash-number.
    // All other rolls are the lowest.
    [Theory]
    [InlineData(8, "tv")]
    [InlineData(15, "mf")]
    [InlineData(17, "1")]
    [InlineData(19, "a1")]
    public void Door_type_roll_picks_the_kind_of_door(int doorTypeRoll, string expectedDoor)
    {
        // Arrange
        var random = new LoadedRandom { [(1, 21)] = doorTypeRoll };
        var generator = new AddressGenerator(Towns, random);

        // Act
        var address = generator.Generate();

        // Assert
        Assert.Equal(expectedDoor, address.Door);
    }

    [Fact]
    public void Highest_numbered_door_is_50()
    {
        // Arrange
        var random = new LoadedRandom { [(1, 21)] = 17, [(1, 51)] = 50 };
        var generator = new AddressGenerator(Towns, random);

        // Act
        var address = generator.Generate();

        // Assert
        Assert.Equal("50", address.Door);
    }

    [Fact]
    public void Lowest_numbered_floor_is_1()
    {
        // Arrange
        // Next(100) decides the house letter (< 20) and "st" (< 30).
        // 99 means a floor number, and that roll is the lowest, 1.
        var random = new LoadedRandom { [(0, 100)] = 99 };
        var generator = new AddressGenerator(Towns, random);

        // Act
        var address = generator.Generate();

        // Assert
        Assert.Equal("1", address.Floor);
    }

    [Fact]
    public void No_postal_codes_throws()
    {
        // Arrange
        var generator = new AddressGenerator(new StubPostalCodeRepository(), new LowestRandom());

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => generator.Generate());
    }
}
