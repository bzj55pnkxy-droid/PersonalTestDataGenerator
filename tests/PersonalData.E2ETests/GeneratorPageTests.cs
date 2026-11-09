using Microsoft.Playwright;

namespace PersonalData.E2ETests;

/// <summary>
/// User flows in a real browser against the running app.
/// </summary>
public sealed class GeneratorPageTests : E2ETest
{
    [Fact]
    public async Task SinglePersonShowsOneCardWithAllFieldLabels()
    {
        // Arrange
        await OpenGeneratorAsync();

        // Act
        await GenerateButton.ClickAsync();

        // Assert
        await Expect(ResultCards).ToHaveCountAsync(1);
        await Expect(ResultCards.GetByRole(AriaRole.Term)).ToHaveTextAsync(
            ["CPR", "Fornavn", "Efternavn", "Køn", "Fødselsdato", "Adresse", "Postnr. og by", "Telefon"]);
    }

    [Fact]
    public async Task SelectingMultiplePersonsRevealsPersonCount()
    {
        // Arrange
        await OpenGeneratorAsync();
        await Expect(PersonCountInput).ToBeHiddenAsync();

        // Act
        await EndpointSelector.SelectOptionAsync("Flere personer · alle oplysninger");

        // Assert
        await Expect(PersonCountInput).ToBeVisibleAsync();
    }

    [Theory]
    [InlineData(2)]
    [InlineData(100)]
    public async Task MultiplePersonsShowOneCardPerPerson(int personCount)
    {
        // Arrange
        await OpenBulkGeneratorAsync(personCount);

        // Act
        await GenerateButton.ClickAsync();

        // Assert
        await Expect(ResultCards).ToHaveCountAsync(personCount);
    }

    [Fact]
    public async Task PhoneNumberShowsOneTitledCardWithOnlyPhoneField()
    {
        // Arrange
        await OpenGeneratorAsync();
        await EndpointSelector.SelectOptionAsync("Telefonnummer");

        // Act
        await GenerateButton.ClickAsync();

        // Assert
        await Expect(ResultCards).ToHaveCountAsync(1);
        var resultHeading = ResultCards.GetByRole(AriaRole.Heading);
        await Expect(resultHeading).ToBeVisibleAsync();
        await Expect(resultHeading).Not.ToBeEmptyAsync();
        await Expect(ResultCards.GetByRole(AriaRole.Term)).ToHaveTextAsync(["Telefon"]);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(101)]
    public async Task PersonCountOutsideAllowedRangeIsBlockedByForm(int personCount)
    {
        // Arrange
        await OpenBulkGeneratorAsync(personCount);

        // Act
        await GenerateButton.ClickAsync();

        // Assert
        Assert.False(await PersonCountInput.EvaluateAsync<bool>("input => input.validity.valid"));
        await Expect(ResultCards).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task ApiFailureShowsHttpStatusInVisibleAlert()
    {
        // Arrange
        await Page.RouteAsync("**/api/person", route => route.FulfillAsync(new() { Status = 500 }));
        await OpenGeneratorAsync();

        // Act
        await GenerateButton.ClickAsync();

        // Assert
        var errorAlert = Page.GetByRole(AriaRole.Alert);
        await Expect(errorAlert).ToBeVisibleAsync();
        await Expect(errorAlert).ToContainTextAsync("HTTP 500");
    }

    private async Task OpenGeneratorAsync()
    {
        await Page.GotoAsync("/");
    }

    private async Task OpenBulkGeneratorAsync(int personCount)
    {
        await OpenGeneratorAsync();
        await EndpointSelector.SelectOptionAsync("Flere personer · alle oplysninger");
        await PersonCountInput.FillAsync(personCount.ToString());
    }
}
