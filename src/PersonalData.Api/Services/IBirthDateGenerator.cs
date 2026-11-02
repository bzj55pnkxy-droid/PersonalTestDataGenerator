namespace PersonalData.Api.Services;

/// <summary>Random date of birth, never in the future.</summary>
public interface IBirthDateGenerator
{
    DateOnly Generate();
}
