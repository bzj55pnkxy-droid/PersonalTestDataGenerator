using PersonalData.Api.Models;

namespace PersonalData.Api.Data;

public sealed class JsonPersonNameRepository : IPersonNameRepository
{
    /// <summary>Where the build copies <c>src/DataFiles/person-names.json</c> (see the .csproj).</summary>
    public static string DefaultFilePath { get; } = Path.Combine(AppContext.BaseDirectory, "DataFiles", "person-names.json");

    public IReadOnlyList<NameAndGender> GetAll() => throw new NotImplementedException();
}
