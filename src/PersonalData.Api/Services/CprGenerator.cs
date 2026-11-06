using PersonalData.Api.Models;

namespace PersonalData.Api.Services;

public sealed class CprGenerator(Random random) : ICprGenerator
{
    public string Generate(DateOnly birthDate, Gender gender)
    {
        var date = birthDate.ToString("ddMMyy", System.Globalization.CultureInfo.InvariantCulture);
        var firstThreeDigits = random.Next(1000).ToString("D3", System.Globalization.CultureInfo.InvariantCulture);
        var lastDigit = gender switch
        {
            Gender.Female => random.Next(5) * 2,
            Gender.Male => random.Next(5) * 2 + 1,
            _ => throw new ArgumentOutOfRangeException(nameof(gender), gender, "Unknown gender.")
        };

        return $"{date}{firstThreeDigits}{lastDigit}";
    }
}
