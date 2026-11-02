using PersonalData.Api.Models;

namespace PersonalData.Api.Data;

/// <summary>Uses connection string <c>ConnectionStrings:Addresses</c>.</summary>
public sealed class MySqlPostalCodeRepository : IPostalCodeRepository
{
    public Task<IReadOnlyList<PostalTown>> GetAllAsync(CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
