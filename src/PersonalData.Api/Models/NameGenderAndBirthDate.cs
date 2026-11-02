namespace PersonalData.Api.Models;

public sealed record NameGenderAndBirthDate(string FirstName, string LastName, Gender Gender, DateOnly BirthDate);
