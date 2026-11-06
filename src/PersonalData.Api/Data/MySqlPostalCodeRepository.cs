using MySqlConnector;
using PersonalData.Api.Models;

namespace PersonalData.Api.Data;

/// <summary>Uses connection string <c>ConnectionStrings:Addresses</c>.</summary>
public sealed class MySqlPostalCodeRepository(IConfiguration configuration) : IPostalCodeRepository
{
    public IReadOnlyList<PostalTown> GetAll()
    {
        var connectionString = configuration.GetConnectionString("Addresses")
            ?? throw new InvalidOperationException("Connection string 'Addresses' is missing.");

        using var connection = new MySqlConnection(connectionString);
        connection.Open();

        const string sql = "SELECT cPostalCode, cTownName FROM postal_code WHERE cTownName IS NOT NULL";
        using var command = new MySqlCommand(sql, connection);
        using var reader = command.ExecuteReader();

        var towns = new List<PostalTown>();
        while (reader.Read())
        {
            // Handle Danish UTF-8 characters that were previously interpreted as Latin-1.
            var townName = reader.GetString(1)
                .Replace("Ã¦", "æ")
                .Replace("Ã¸", "ø")
                .Replace("Ã¥", "å")
                .Replace("Ã†", "Æ")
                .Replace("Ã˜", "Ø")
                .Replace("Ã…", "Å");

            towns.Add(new PostalTown(reader.GetString(0), townName));
        }

        return towns;
    }
}
