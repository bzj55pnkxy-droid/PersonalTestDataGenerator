using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PersonalData.Api.Data;
using PersonalData.Api.Models;

namespace PersonalData.IntegrationTests;

/// <summary>
/// Broad integration test: the whole app in-process (controllers, DI, real <c>person-names.json</c> and MySQL).
/// Checks what the Postman collection cannot see: every name and postal code/town in the response comes from the
/// app's own repositories. The repository tests prove those read the file and the database correctly.
/// Needs <c>make db</c>.
/// </summary>
public sealed class ApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Bulk_persons_use_name_records_and_postal_rows_that_exist()
    {
        // Arrange
        var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var names = scope.ServiceProvider.GetRequiredService<IPersonNameRepository>().GetAll();
        var postalTowns = scope.ServiceProvider.GetRequiredService<IPostalCodeRepository>().GetAll();

        // Act
        var persons = await client.GetFromJsonAsync<List<Person>>("/api/persons?count=100", JsonSerializerOptions.Web);

        // Assert
        Assert.NotNull(persons);
        Assert.Equal(100, persons.Count);
        Assert.All(persons, p =>
        {
            Assert.Contains(new NameAndGender(p.FirstName, p.LastName, p.Gender), names);
            Assert.Contains(new PostalTown(p.Address.PostalCode, p.Address.TownName), postalTowns);
        });
    }
}
