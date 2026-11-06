namespace PersonalData.Api.Services;

/// <summary>Random date of birth within the last 100 years, including today.</summary>
public interface IBirthDateGenerator
{
    DateOnly Generate();
}
