using PersonalData.Api.Models;

namespace PersonalData.Api.Services;

public sealed class CprGenerator(Random random) : ICprGenerator
{
    public string Generate(DateOnly birthDate, Gender gender) => throw new NotImplementedException();
}
