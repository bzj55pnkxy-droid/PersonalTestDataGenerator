using PersonalData.Api.Data;
using PersonalData.Api.Models;

namespace PersonalData.Api.Services;

public sealed class NameGenderGenerator(IPersonNameRepository personNames, Random random) : INameGenderGenerator
{
    public NameAndGender Generate()
    {
        var names = personNames.GetAll();
        if (names.Count == 0)
        {
            throw new InvalidOperationException("No person names are available.");
        }

        return names[random.Next(names.Count)];
    }
}
