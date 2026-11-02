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

    public NameAndGender GetNameAndGender() => throw new NotImplementedException();

    public NameGenderAndBirthDate GetNameGenderAndBirthDate() => throw new NotImplementedException();

    public CprNameAndGender GetCprNameAndGender() => throw new NotImplementedException();

    public CprNameGenderAndBirthDate GetCprNameGenderAndBirthDate() => throw new NotImplementedException();

    public Task<Address> GetAddressAsync(CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public string GetPhoneNumber() => throw new NotImplementedException();

    public Task<Person> GetPersonAsync(CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();

    public Task<IReadOnlyList<Person>> GetPersonsAsync(int count, CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
