using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using PersonalData.Api.Models;

namespace PersonalData.IntegrationTests;

public class PersonApiTests
{
    [Fact]
    public async Task GetPerson_ReturnsOk()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        // Act
        using var response = await client.GetAsync("/api/person");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPersons_ReturnsRequestedNumberOfPersons()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        // Act
        using var response = await client.GetAsync("/api/persons?count=2");

        response.EnsureSuccessStatusCode();

        var persons = await response.Content.ReadFromJsonAsync<List<Person>>()
            ?? throw new InvalidOperationException("API'et returnerede null.");

        // Assert
        Assert.Equal(2, persons.Count);
    }


}