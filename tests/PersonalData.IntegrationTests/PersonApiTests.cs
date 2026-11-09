using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using PersonalData.Api.Data;
using PersonalData.Api.Models;

namespace PersonalData.IntegrationTests;

public class PersonApiTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly Dictionary<Gender, string> AllowedLastDigits = new()
    {
        [Gender.Female] = "02468",
        [Gender.Male] = "13579"
    };

    [Fact]
    public async Task GetPersonReturnsOk()
    {
        // Arrange
        using var client = CreateClient();

        // Act
        using var response = await client.GetAsync("/api/person");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetPersonsReturnsRequestedNumberOfPersons()
    {
        // Arrange
        using var client = CreateClient();

        // Act
        var persons = await client.GetFromJsonAsync<List<Person>>("/api/persons?count=2");

        // Assert
        Assert.NotNull(persons);
        Assert.Equal(2, persons.Count);
    }

    [Fact]
    public async Task GetCprReturnsTenDigitsWithValidCalendarDate()
    {
        // Arrange
        using var client = CreateClient();

        // Act
        var response = await client.GetFromJsonAsync<CprResponse>("/api/cpr");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(10, response.Cpr.Length);
        Assert.All(response.Cpr, digit => Assert.InRange(digit, '0', '9'));
        Assert.True(DateOnly.TryParseExact(
            response.Cpr[..6], "ddMMyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _));
    }

    [Theory]
    [InlineData("/api/name-gender")]
    [InlineData("/api/name-gender-dob")]
    public async Task NameEndpointsReturnOneCompleteRepositoryRow(string endpoint)
    {
        // Arrange
        using var client = CreateClient();
        var names = GetNameRecords();

        // Act
        var person = await client.GetFromJsonAsync<NameAndGender>(endpoint);

        // Assert
        Assert.NotNull(person);
        Assert.Contains(person, names);
    }

    [Theory]
    [InlineData("/api/cpr-name-gender")]
    [InlineData("/api/cpr-name-gender-dob")]
    public async Task CombinedNameEndpointsReturnKnownNameWithMatchingCprGender(string endpoint)
    {
        // Arrange
        using var client = CreateClient();
        var names = GetNameRecords();

        // Act
        var person = await client.GetFromJsonAsync<CprNameAndGender>(endpoint);

        // Assert
        Assert.NotNull(person);
        Assert.Contains(new NameAndGender(person.FirstName, person.LastName, person.Gender), names);
        Assert.Contains(person.Cpr[^1], AllowedLastDigits[person.Gender]);
    }

    [Theory]
    [InlineData("/api/cpr-name-gender-dob")]
    [InlineData("/api/person")]
    public async Task PersonEndpointsReturnCprMatchingBirthDate(string endpoint)
    {
        // Arrange
        using var client = CreateClient();

        // Act
        var person = await client.GetFromJsonAsync<CprNameGenderAndBirthDate>(endpoint);

        // Assert
        Assert.NotNull(person);
        Assert.StartsWith(person.BirthDate.ToString("ddMMyy", CultureInfo.InvariantCulture), person.Cpr);
    }

    [Fact]
    public async Task GetNameAndBirthDateIncludesValidCalendarDate()
    {
        // Arrange
        using var client = CreateClient();

        // Act
        var person = await client.GetFromJsonAsync<JsonElement>("/api/name-gender-dob");

        // Assert
        var birthDateText = person.GetProperty("birthDate").GetString();
        Assert.True(DateOnly.TryParseExact(
            birthDateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _));
    }

    [Fact]
    public async Task GetAddressReturnsAlphabeticStreetAndExistingPostalTown()
    {
        // Arrange
        using var client = CreateClient();
        var postalTowns = GetPostalTowns();

        // Act
        var address = await client.GetFromJsonAsync<Address>("/api/address");

        // Assert
        Assert.NotNull(address);
        Assert.NotEmpty(address.Street);
        Assert.All(address.Street, character => Assert.True(char.IsLetter(character)));
        Assert.Contains(new PostalTown(address.PostalCode, address.TownName), postalTowns);
    }

    [Fact]
    public async Task GetPhoneNumberReturnsEightDigits()
    {
        // Arrange
        using var client = CreateClient();

        // Act
        var response = await client.GetFromJsonAsync<PhoneNumberResponse>("/api/phone");

        // Assert
        Assert.NotNull(response);
        Assert.Equal(8, response.PhoneNumber.Length);
        Assert.All(response.PhoneNumber, digit => Assert.InRange(digit, '0', '9'));
    }

    private HttpClient CreateClient() => factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost")
    });

    private IReadOnlyList<NameAndGender> GetNameRecords()
    {
        using var scope = factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IPersonNameRepository>().GetAll();
    }

    private IReadOnlyList<PostalTown> GetPostalTowns()
    {
        using var scope = factory.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IPostalCodeRepository>().GetAll();
    }
}