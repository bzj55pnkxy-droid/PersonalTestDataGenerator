using PersonalData.Api.Data;
using PersonalData.Api.Models;

namespace PersonalData.Api.Services;

public sealed class NameGenderGenerator(IPersonNameRepository personNames, Random random) : INameGenderGenerator
{
    public NameAndGender Generate() => throw new NotImplementedException();
}
