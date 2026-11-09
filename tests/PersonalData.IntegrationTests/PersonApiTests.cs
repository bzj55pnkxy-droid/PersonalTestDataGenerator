using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PersonalData.IntegrationTests;

public class PersonApiTests
{
    [Fact]
    public void GetPerson_ReturnsOk()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                BaseAddress = new Uri("https://localhost")
            });

        // Act
        using var response = client.GetAsync("/api/person")
            .GetAwaiter().GetResult();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}