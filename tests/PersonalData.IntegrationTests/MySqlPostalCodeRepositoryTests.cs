using Microsoft.Extensions.Configuration;
using PersonalData.Api.Data;
using PersonalData.Api.Models;

namespace PersonalData.IntegrationTests;

public class MySqlPostalCodeRepositoryTests
{
    [Fact]
    public void GetAllLoadsKnownPostalTownFromDatabase()
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
        var repository = new MySqlPostalCodeRepository(configuration);

        // Act
        var postalTowns = repository.GetAll();

        // Assert
        Assert.Contains(new PostalTown("8000", "Aarhus C"), postalTowns);
    }
}