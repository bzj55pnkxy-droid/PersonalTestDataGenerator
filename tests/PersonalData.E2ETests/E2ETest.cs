using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace PersonalData.E2ETests;

/// <summary>
/// Browser tests against the running app (default: http://localhost:8080; override with E2E_BASE_URL).
/// </summary>
public abstract class E2ETest : PageTest
{
    private static readonly string BaseUrl =
        Environment.GetEnvironmentVariable("E2E_BASE_URL") ?? "http://localhost:8080";

    // Install Chromium once before creating a browser.
    static E2ETest()
    {
        var exitCode = Microsoft.Playwright.Program.Main(["install", "chromium"]);
        if (exitCode != 0)
        {
            throw new InvalidOperationException($"'playwright install chromium' failed with exit code {exitCode}.");
        }
    }

    public override BrowserNewContextOptions ContextOptions() =>
        new(base.ContextOptions()) { BaseURL = BaseUrl };

    protected ILocator EndpointSelector => Page.GetByLabel("Hvad vil du generere?");

    protected ILocator PersonCountInput => Page.GetByLabel("Antal personer");

    protected ILocator GenerateButton => Page.GetByRole(AriaRole.Button, new() { Name = "Generér data" });

    protected ILocator ResultCards => Page.GetByRole(AriaRole.Article);
}
