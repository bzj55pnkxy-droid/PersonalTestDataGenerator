namespace PersonalData.Api.Models;

public sealed record CprNameGenderAndBirthDate(string Cpr, string FirstName, string LastName, Gender Gender, DateOnly BirthDate);
