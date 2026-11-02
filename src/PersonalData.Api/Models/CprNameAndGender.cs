namespace PersonalData.Api.Models;

public sealed record CprNameAndGender(string Cpr, string FirstName, string LastName, Gender Gender);
