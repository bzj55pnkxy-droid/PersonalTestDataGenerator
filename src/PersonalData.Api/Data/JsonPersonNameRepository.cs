using System.Text.Json;
using PersonalData.Api.Models;

namespace PersonalData.Api.Data;

public sealed class JsonPersonNameRepository : IPersonNameRepository
{
    /// <summary>Where the build copies <c>src/DataFiles/person-names.json</c> (see the .csproj).</summary>
    public static string DefaultFilePath { get; } = Path.Combine(AppContext.BaseDirectory, "DataFiles", "person-names.json");

    private readonly IReadOnlyList<NameAndGender> names = LoadNames();

    public IReadOnlyList<NameAndGender> GetAll() => names;

    private static IReadOnlyList<NameAndGender> LoadNames()
    {
        using var stream = File.OpenRead(DefaultFilePath);
        var data = JsonSerializer.Deserialize<PersonNamesFile>(stream, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        if (data?.Persons is not { Length: > 0 } persons)
        {
            throw new InvalidDataException("person-names.json must contain a non-empty persons array.");
        }

        return Array.AsReadOnly(persons);
    }

    private sealed record PersonNamesFile(NameAndGender[] Persons);
}
