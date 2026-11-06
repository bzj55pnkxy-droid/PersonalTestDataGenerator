using PersonalData.Api.Models;

namespace PersonalData.Api.Services;

public sealed class FakePersonService(
    INameGenderGenerator nameGenderGenerator,
    IBirthDateGenerator birthDateGenerator,
    ICprGenerator cprGenerator,
    IAddressGenerator addressGenerator,
    IPhoneNumberGenerator phoneNumberGenerator) : IFakePersonService
{
    public string GetCpr() => throw new NotImplementedException();

    public NameAndGender GetNameAndGender() => nameGenderGenerator.Generate();

    public NameGenderAndBirthDate GetNameGenderAndBirthDate()
    {
        var nameAndGender = nameGenderGenerator.Generate();
        var birthDate = birthDateGenerator.Generate();

        return new NameGenderAndBirthDate(
            nameAndGender.FirstName,
            nameAndGender.LastName,
            nameAndGender.Gender,
            birthDate);
    }

    public CprNameAndGender GetCprNameAndGender()
    {
        var nameAndGender = nameGenderGenerator.Generate();
        var birthDate = birthDateGenerator.Generate();
        var cpr = cprGenerator.Generate(birthDate, nameAndGender.Gender);

        return new CprNameAndGender(
            cpr,
            nameAndGender.FirstName,
            nameAndGender.LastName,
            nameAndGender.Gender);
    }

    public CprNameGenderAndBirthDate GetCprNameGenderAndBirthDate()
    {
        var nameAndGender = nameGenderGenerator.Generate();
        var birthDate = birthDateGenerator.Generate();
        var cpr = cprGenerator.Generate(birthDate, nameAndGender.Gender);

        return new CprNameGenderAndBirthDate(
            cpr,
            nameAndGender.FirstName,
            nameAndGender.LastName,
            nameAndGender.Gender,
            birthDate);
    }

    public Address GetAddress() => addressGenerator.Generate();

    public string GetPhoneNumber() => phoneNumberGenerator.Generate();

    public Person GetPerson()
    {
        var nameAndGender = nameGenderGenerator.Generate();
        var birthDate = birthDateGenerator.Generate();
        var cpr = cprGenerator.Generate(birthDate, nameAndGender.Gender);
        var address = addressGenerator.Generate();
        var phoneNumber = phoneNumberGenerator.Generate();

        return new Person(
            cpr,
            nameAndGender.FirstName,
            nameAndGender.LastName,
            nameAndGender.Gender,
            birthDate,
            address,
            phoneNumber);
    }

    public IReadOnlyList<Person> GetPersons(int count)
    {
        if (count < 2 || count > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be between 2 and 100.");
        }

        var persons = new List<Person>();
        for (var i = 0; i < count; i++)
        {
            persons.Add(GetPerson());
        }

        return persons;
    }
}
