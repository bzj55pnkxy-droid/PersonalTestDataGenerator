using PersonalData.Api.Models;

namespace PersonalData.Api.Services;

/// <summary>
/// One method per required functionality; each maps to one endpoint in <c>FakePersonController</c>.
/// CPR, gender and date of birth returned together must be consistent with each other.
/// </summary>
public interface IFakePersonService
{
    string GetCpr();

    NameAndGender GetNameAndGender();

    NameGenderAndBirthDate GetNameGenderAndBirthDate();

    CprNameAndGender GetCprNameAndGender();

    CprNameGenderAndBirthDate GetCprNameGenderAndBirthDate();

    Task<Address> GetAddressAsync(CancellationToken cancellationToken = default);

    string GetPhoneNumber();

    Task<Person> GetPersonAsync(CancellationToken cancellationToken = default);

    /// <summary>All information for <paramref name="count"/> persons; valid range is 2-100.</summary>
    Task<IReadOnlyList<Person>> GetPersonsAsync(int count, CancellationToken cancellationToken = default);
}
