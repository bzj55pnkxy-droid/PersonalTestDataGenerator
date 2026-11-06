using MySqlConnector;
using PersonalData.Api.Models;

namespace PersonalData.Api.Data;

/// <summary>Uses connection string <c>ConnectionStrings:Addresses</c>.</summary>
public sealed class MySqlPostalCodeRepository(IConfiguration configuration) : IPostalCodeRepository
{
    public async Task<IReadOnlyList<PostalTown>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = configuration.GetConnectionString("Addresses")
            ?? throw new InvalidOperationException("Connection string 'Addresses' is missing.");

        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = "SELECT cPostalCode, cTownName FROM postal_code WHERE cTownName IS NOT NULL";
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var towns = new List<PostalTown>();
        while (await reader.ReadAsync(cancellationToken))
        {
            towns.Add(new PostalTown(reader.GetString(0), reader.GetString(1)));
        }

        return towns;
    }
}
