using PersonalData.Api.Models;

namespace PersonalData.Api.Data;

/// <summary>Postal codes and towns from the MySQL <c>addresses.postal_code</c> table.</summary>
public interface IPostalCodeRepository
{
    IReadOnlyList<PostalTown> GetAll();
}
