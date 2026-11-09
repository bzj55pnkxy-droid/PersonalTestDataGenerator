using Microsoft.Playwright;

namespace PersonalData.E2ETests;

/// <summary>
/// The generator page in a real browser against the running app: what the user sees and can do.
/// The field formats (CPR, phone, ...) are checked on every response by the Postman collection, not here.
/// </summary>
public sealed class GeneratorPageTests : E2ETest
{
    [Fact]
    public async Task Single_person_shows_one_card_with_all_field_labels()
    {
        // Arrange
        await Page.GotoAsync("/");

        // Act
        await GenerateButton.ClickAsync();

        // Assert
        await Expect(Cards).ToHaveCountAsync(1);
        await Expect(Cards.GetByRole(AriaRole.Term)).ToHaveTextAsync(
            ["CPR", "Fornavn", "Efternavn", "Køn", "Fødselsdato", "Adresse", "Postnr. og by", "Telefon"]);
    }

    [Fact]
    public async Task Single_person_reports_one_result_in_status()
    {
        // Arrange
        await Page.GotoAsync("/");

        // Act
        await GenerateButton.ClickAsync();

        // Assert
        await Expect(Status).ToHaveTextAsync("1 resultat genereret");
    }

    [Fact]
    public async Task Count_field_appears_when_multiple_persons_is_chosen()
    {
        // Arrange
        await Page.GotoAsync("/");
        await Expect(Count).ToBeHiddenAsync();

        // Act
        await Endpoint.SelectOptionAsync("Flere personer · alle oplysninger");

        // Assert
        await Expect(Count).ToBeVisibleAsync();
    }

    [Fact]
    public async Task Multiple_persons_show_one_card_per_person()
    {
        // Arrange
        await Page.GotoAsync("/");
        await Endpoint.SelectOptionAsync("Flere personer · alle oplysninger");
        await Count.FillAsync("100");

        // Act
        await GenerateButton.ClickAsync();

        // Assert
        await Expect(Cards).ToHaveCountAsync(100);
    }

    [Fact]
    public async Task Multiple_persons_report_how_many_were_generated_in_status()
    {
        // Arrange
        await Page.GotoAsync("/");
        await Endpoint.SelectOptionAsync("Flere personer · alle oplysninger");
        await Count.FillAsync("2");

        // Act
        await GenerateButton.ClickAsync();

        // Assert
        await Expect(Status).ToHaveTextAsync("2 personer genereret");
    }

    [Fact]
    public async Task Phone_number_shows_one_card_titled_resultat_1_with_only_a_telefon_row()
    {
        // Arrange
        await Page.GotoAsync("/");
        await Endpoint.SelectOptionAsync("Telefonnummer");

        // Act
        await GenerateButton.ClickAsync();

        // Assert
        await Expect(Cards).ToHaveCountAsync(1);
        await Expect(Cards.GetByRole(AriaRole.Heading)).ToHaveTextAsync("Resultat 1");
        await Expect(Cards.GetByRole(AriaRole.Term)).ToHaveTextAsync(["Telefon"]);
    }

    [Fact]
    public async Task Count_above_maximum_is_blocked_by_the_form()
    {
        // Arrange
        await Page.GotoAsync("/");
        await Endpoint.SelectOptionAsync("Flere personer · alle oplysninger");
        await Count.FillAsync("101");

        // Act
        await GenerateButton.ClickAsync();

        // Assert
        await Expect(Cards).ToHaveCountAsync(0);
        await Expect(Status).ToHaveTextAsync("Klar til at generere");
    }

    [Fact]
    public async Task Api_error_is_shown_to_the_user()
    {
        // Arrange
        await Page.RouteAsync("**/api/person", route => route.FulfillAsync(new() { Status = 500 }));
        await Page.GotoAsync("/");

        // Act
        await GenerateButton.ClickAsync();

        // Assert
        await Expect(Page.GetByRole(AriaRole.Alert)).ToHaveTextAsync(
            "API’et svarede med HTTP 500. Prøv igen, og kontrollér at databasen kører.");
    }
}
