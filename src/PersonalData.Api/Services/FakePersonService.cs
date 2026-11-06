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

    public Task<Address> GetAddressAsync(CancellationToken cancellationToken = default) =>
        addressGenerator.GenerateAsync(cancellationToken);

    public string GetPhoneNumber() => throw new NotImplementedException();

    public Task<Person> GetPersonAsync(CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<IReadOnlyList<Person>> GetPersonsAsync(int count, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
