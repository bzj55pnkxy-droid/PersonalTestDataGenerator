using System.Globalization;
using PersonalData.Api.Models;
using PersonalData.Api.Services;

namespace PersonalData.UnitTests;

/// <summary>Document sections 6, 7 and 12: consistent person data and bulk-count EP/BV cases.</summary>
public sealed class FakePersonServiceTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(101)]
    [InlineData(150)]
    public void GetPersonsThrowsWhenCountIsOutsideAllowedRange(int count)
    {
        // Arrange
        var service = CreateService();

        // Act
        Action generatePersons = () => service.GetPersons(count);

        // Assert
        Assert.Throws<ArgumentOutOfRangeException>(generatePersons);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(50)]
    [InlineData(99)]
    [InlineData(100)]
    public void GetPersonsReturnsRequestedNumberOfPeople(int count)
    {
        // Arrange
        var service = CreateService();

        // Act
        var persons = service.GetPersons(count);

        // Assert
        Assert.Equal(count, persons.Count);
    }

    [Theory]
    [InlineData(Gender.Female, "2005-06-15", "150605", "02468")]
    [InlineData(Gender.Male, "2005-06-15", "150605", "13579")]
    [InlineData(Gender.Female, "2005-01-06", "060105", "02468")]
    public void GetPersonReturnsCprMatchingBirthDateAndGender(
        Gender gender, string birthDateText, string expectedDatePart, string allowedLastDigits)
    {
        // Arrange
        var birthDate = DateOnly.ParseExact(birthDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var service = CreateService(gender, birthDate);

        // Act
        var person = service.GetPerson();

        // Assert
        Assert.Equal(birthDate, person.BirthDate);
        Assert.Equal(gender, person.Gender);
        Assert.StartsWith(expectedDatePart, person.Cpr);
        Assert.Contains(person.Cpr[^1], allowedLastDigits);
    }

    [Theory]
    [InlineData(Gender.Female, "2005-06-15", "150605", "02468")]
    [InlineData(Gender.Male, "2005-06-15", "150605", "13579")]
    [InlineData(Gender.Female, "2005-01-06", "060105", "02468")]
    public void GetCprNameGenderAndBirthDateReturnsMatchingDateAndGender(
        Gender gender, string birthDateText, string expectedDatePart, string allowedLastDigits)
    {
        // Arrange
        var birthDate = DateOnly.ParseExact(birthDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        var service = CreateService(gender, birthDate);

        // Act
        var person = service.GetCprNameGenderAndBirthDate();

        // Assert
        Assert.Equal(birthDate, person.BirthDate);
        Assert.Equal(gender, person.Gender);
        Assert.StartsWith(expectedDatePart, person.Cpr);
        Assert.Contains(person.Cpr[^1], allowedLastDigits);
    }

    private static FakePersonService CreateService(
        Gender gender = Gender.Female, DateOnly? birthDate = null)
    {
        var selectedDate = birthDate ?? new DateOnly(2005, 6, 15);
        var today = new DateOnly(2026, 6, 15);
        var earliestDate = today.AddYears(-100);
        var numberOfDates = today.DayNumber - earliestDate.DayNumber + 1;
        var dateOffset = selectedDate.DayNumber - earliestDate.DayNumber;
        // The implementation's date window only steers test setup.
        var random = new LoadedRandom { [(0, numberOfDates)] = dateOffset };
        var clock = new FixedTimeProvider(new DateTimeOffset(2026, 6, 15, 12, 0, 0, TimeSpan.Zero));
        var towns = new StubPostalCodeRepository(new PostalTown("2100", "København Ø"));
        var name = gender == Gender.Female
            ? new NameAndGender("Annemette P.", "Nilsson", Gender.Female)
            : new NameAndGender("Nicolai T.", "Bech", Gender.Male);
        var names = new StubPersonNameRepository(name);

        return new FakePersonService(
            new NameGenderGenerator(names, random),
            new BirthDateGenerator(random, clock),
            new CprGenerator(random),
            new AddressGenerator(towns, random),
            new PhoneNumberGenerator(random));
    }
}
