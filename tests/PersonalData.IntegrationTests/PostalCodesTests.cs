using PersonalData.Api.Data;
using PersonalData.Api.Models;
using PersonalData.Api.Services;

namespace PersonalData.IntegrationTests;

/// <summary>Black-box design section 6.5: postal code and town with the real MySQL table. Needs <c>make db</c>.</summary>
public sealed class PostalCodesTests
{
    [Fact]
    public void Repository_returns_every_row_of_the_table_unchanged()
    {
        var towns = new MySqlPostalCodeRepository(TestData.Configuration).GetAll()
            .OrderBy(t => t.PostalCode, StringComparer.Ordinal)
            .Select(t => (t.PostalCode, t.TownName));

        Assert.Equal(TestData.ReadPostalRows(), towns);
    }

    // Experience-based: Danish letters survive the trip from MySQL. Comparing with ReadPostalRows alone would not
    // catch a wrong connection charset, because both reads would be garbled the same way.
    [Theory]
    [InlineData("2100", "København Ø")]
    [InlineData("8882", "Fårvang")]
    [InlineData("4200", "Slagelse")]
    public void Repository_returns_town_names_as_stored(string postalCode, string townName)
    {
        var towns = new MySqlPostalCodeRepository(TestData.Configuration).GetAll();

        Assert.Contains(new PostalTown(postalCode, townName), towns);
    }

    [Fact]
    public void Generated_addresses_use_postal_code_and_town_of_the_same_row()
    {
        var rows = TestData.ReadPostalRows().ToHashSet();
        var generator = new AddressGenerator(new MySqlPostalCodeRepository(TestData.Configuration), new Random(42));

        var addresses = Enumerable.Range(0, 200).Select(_ => generator.Generate()).ToList();

        Assert.All(addresses, a => Assert.Contains((a.PostalCode, a.TownName), rows));
    }
}
