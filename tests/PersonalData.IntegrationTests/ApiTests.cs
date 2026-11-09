using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PersonalData.Api.Data;
using PersonalData.Api.Models;

namespace PersonalData.IntegrationTests;

// Verifies generated people against the real JSON and MySQL repositories.
public sealed class ApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task BulkPersonsUseExistingNamesAndPostalTowns()
    {
        // Arrange
        using var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var names = scope.ServiceProvider.GetRequiredService<IPersonNameRepository>().GetAll();
        var postalTowns = scope.ServiceProvider.GetRequiredService<IPostalCodeRepository>().GetAll();

        // Act
        var persons = await client.GetFromJsonAsync<List<Person>>("/api/persons?count=100", JsonSerializerOptions.Web);

        // Assert
        Assert.NotNull(persons);
        Assert.Equal(100, persons.Count);
        Assert.All(persons, person =>
        {
            Assert.Contains(new NameAndGender(person.FirstName, person.LastName, person.Gender), names);
            Assert.Contains(new PostalTown(person.Address.PostalCode, person.Address.TownName), postalTowns);
        });
    }
}
