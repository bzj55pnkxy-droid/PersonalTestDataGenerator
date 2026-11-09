using PersonalData.Api.Models;
using PersonalData.Api.Services;

namespace PersonalData.UnitTests;

/// <summary>
/// Tests that <see cref="FakePersonService"/> accepts only 2 to 100 persons, using real generators
/// with fake dice. 1/2 and 100/101 are the boundary values of the assignment's 2-100 rule.
/// </summary>
public sealed class FakePersonServiceTests
{
    private static FakePersonService CreateService()
    {
        var random = new LowestRandom();
        var today = new FixedTimeProvider(new DateTimeOffset(2026, 6, 15, 12, 0, 0, TimeSpan.Zero));
        var towns = new StubPostalCodeRepository(new PostalTown("2100", "København Ø"));
        var names = new StubPersonNameRepository(new NameAndGender("Annemette P.", "Nilsson", Gender.Female));

        return new FakePersonService(
            new NameGenderGenerator(names, random),
            new BirthDateGenerator(random, today),
            new CprGenerator(random),
            new AddressGenerator(towns, random),
            new PhoneNumberGenerator(random));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(101)]
    public void Count_outside_2_to_100_throws(int count)
    {
        // Arrange
        var service = CreateService();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => service.GetPersons(count));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(100)]
    public void Count_from_2_to_100_gives_that_many_persons(int count)
    {
        // Arrange
        var service = CreateService();

        // Act
        var persons = service.GetPersons(count);

        // Assert
        Assert.Equal(count, persons.Count);
    }
}
