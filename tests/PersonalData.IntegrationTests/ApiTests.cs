using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace PersonalData.IntegrationTests;

/// <summary>
/// The whole app in-process (controllers, DI, real <c>person-names.json</c> and MySQL), checking what the Postman
/// collection cannot without the source data: C3 (black-box design section 8) and postal code/town rows (6.5).
/// Needs <c>make db</c>.
/// </summary>
public sealed class ApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Bulk_persons_use_name_records_and_postal_rows_that_exist()
    {
        var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/persons?count=100");
        response.EnsureSuccessStatusCode();
        using var body = JsonDocument.Parse(await response.Content.ReadAsStreamAsync());

        var nameRecords = TestData.NameRecords.ToHashSet();
        var postalRows = TestData.ReadPostalRows().ToHashSet();
        var persons = body.RootElement.EnumerateArray().ToList();

        Assert.Equal(100, persons.Count);
        Assert.All(persons, p =>
        {
            Assert.Contains((Text(p, "firstName"), Text(p, "lastName"), Text(p, "gender")), nameRecords);
            var address = p.GetProperty("address");
            Assert.Contains((Text(address, "postalCode"), Text(address, "townName")), postalRows);
        });
    }

    private static string Text(JsonElement element, string property) => element.GetProperty(property).GetString()!;
}
