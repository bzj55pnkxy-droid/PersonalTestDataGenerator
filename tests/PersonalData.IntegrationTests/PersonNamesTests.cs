using PersonalData.Api.Data;
using PersonalData.Api.Models;
using PersonalData.Api.Services;

namespace PersonalData.IntegrationTests;

/// <summary>Black-box design section 3: name and gender with the real <c>person-names.json</c>.</summary>
public sealed class PersonNamesTests
{
    [Fact]
    public void Repository_returns_every_record_of_the_file_in_order()
    {
        var expected = TestData.NameRecords
            .Select(r => new NameAndGender(r.FirstName, r.LastName, ParseGender(r.Gender)))
            .ToList();

        Assert.Equal(expected, new JsonPersonNameRepository().GetAll());
    }

    [Fact]
    public void Generator_returns_whole_records_of_the_file_and_both_genders()
    {
        var generator = new NameGenderGenerator(new JsonPersonNameRepository(), new Random(42));

        var results = Enumerable.Range(0, 1000).Select(_ => generator.Generate()).ToList();

        // C3: first name, last name and gender come from the same record (A2).
        var records = TestData.NameRecords.ToHashSet();
        Assert.All(results, r => Assert.Contains((r.FirstName, r.LastName, r.Gender == Gender.Female ? "female" : "male"), records));
        Assert.Contains(results, r => r.Gender == Gender.Female);
        Assert.Contains(results, r => r.Gender == Gender.Male);
    }

    private static Gender ParseGender(string gender) => gender switch
    {
        "female" => Gender.Female,
        "male" => Gender.Male,
        _ => throw new InvalidDataException($"Unexpected gender '{gender}' in person-names.json."),
    };
}
