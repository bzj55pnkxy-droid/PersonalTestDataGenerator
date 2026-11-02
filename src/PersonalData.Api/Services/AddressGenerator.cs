using PersonalData.Api.Data;
using PersonalData.Api.Models;

namespace PersonalData.Api.Services;

public sealed class AddressGenerator(IPostalCodeRepository postalCodes, Random random) : IAddressGenerator
{
    public Task<Address> GenerateAsync(CancellationToken cancellationToken = default) =>
        throw new NotImplementedException();
}
