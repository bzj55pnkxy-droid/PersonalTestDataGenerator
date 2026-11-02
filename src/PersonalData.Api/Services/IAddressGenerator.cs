using PersonalData.Api.Models;

namespace PersonalData.Api.Services;

/// <summary>Random Danish address; postal code and town come from the database. Rules: see <see cref="Address"/>.</summary>
public interface IAddressGenerator
{
    Task<Address> GenerateAsync(CancellationToken cancellationToken = default);
}
