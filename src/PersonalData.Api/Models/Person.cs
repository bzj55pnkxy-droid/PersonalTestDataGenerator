namespace PersonalData.Api.Models;

public sealed record Person(
    string Cpr,
    string FirstName,
    string LastName,
    Gender Gender,
    DateOnly BirthDate,
    Address Address,
    string PhoneNumber);
