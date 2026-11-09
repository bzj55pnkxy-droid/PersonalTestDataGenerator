using System.Text.Json;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace PersonalData.IntegrationTests;

/// <summary>Expected data, read straight from the sources and independently of the code under test.</summary>
internal static class TestData
{
    /// <summary>The API's development settings (copied into the test output). Environment variables such as
    /// <c>ConnectionStrings__Addresses</c> override them.</summary>
    public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
        .AddJsonFile("appsettings.Development.json")
        .AddEnvironmentVariables()
        .Build();

    /// <summary>Every record of <c>person-names.json</c> as raw strings, in file order.</summary>
    public static IReadOnlyList<(string FirstName, string LastName, string Gender)> NameRecords { get; } = ReadNameRecords();

    /// <summary>Every <c>postal_code</c> row with a town, ordered by postal code. Needs MySQL (<c>make db</c>).</summary>
    public static List<(string PostalCode, string TownName)> ReadPostalRows()
    {
        using var connection = new MySqlConnection(Configuration.GetConnectionString("Addresses"));
        connection.Open();
        using var command = new MySqlCommand(
            "SELECT cPostalCode, cTownName FROM postal_code WHERE cTownName IS NOT NULL ORDER BY cPostalCode", connection);
        using var reader = command.ExecuteReader();

        var rows = new List<(string, string)>();
        while (reader.Read())
        {
            rows.Add((reader.GetString(0), reader.GetString(1)));
        }

        return rows;
    }

    private static List<(string, string, string)> ReadNameRecords()
    {
        using var stream = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "DataFiles", "person-names.json"));
        using var document = JsonDocument.Parse(stream);

        return document.RootElement.GetProperty("persons").EnumerateArray()
            .Select(p => (
                p.GetProperty("firstName").GetString()!,
                p.GetProperty("lastName").GetString()!,
                p.GetProperty("gender").GetString()!))
            .ToList();
    }
}
