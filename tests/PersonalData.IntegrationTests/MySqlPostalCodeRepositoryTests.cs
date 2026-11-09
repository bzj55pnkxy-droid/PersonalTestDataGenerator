using Microsoft.Extensions.Configuration;
using PersonalData.Api.Data;

namespace PersonalData.IntegrationTests;

public class MySqlPostalCodeRepositoryTests
{
    [Fact]
    public void GetAll_LoadsKnownPostalTownFromDatabase()
    {
        // Arrange
        var settings = new Dictionary<string, string?>
        {
            ["ConnectionStrings:Addresses"] =
        "Server=localhost;Port=3307;Database=addresses;User ID=app;Password=app"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        // Act
        var repository = new MySqlPostalCodeRepository(configuration);
        var postalCodes = repository.GetAll();

        // Assert
        Assert.Contains(postalCodes, p =>
            p.PostalCode == "8000" &&
            p.TownName == "Aarhus C");

    }
}