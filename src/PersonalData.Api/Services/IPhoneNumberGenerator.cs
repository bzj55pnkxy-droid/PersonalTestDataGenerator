namespace PersonalData.Api.Services;

/// <summary>8 digits starting with one of the allowed Danish mobile prefixes (see the assignment).</summary>
public interface IPhoneNumberGenerator
{
    string Generate();
}
