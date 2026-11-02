using PersonalData.Api.Models;

namespace PersonalData.Api.Services;

/// <summary>
/// 10 digits: <paramref name="birthDate"/> as <c>ddMMyy</c> followed by 4 random digits.
/// The last digit is even for <see cref="Gender.Female"/> and odd for <see cref="Gender.Male"/>.
/// </summary>
public interface ICprGenerator
{
    string Generate(DateOnly birthDate, Gender gender);
}
