using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;

namespace PersonalData.E2ETests;

/// <summary>
/// Base class for the browser tests: installs Chromium once, points every page at the running app and exposes the
/// page's controls by their user-facing labels.
/// The app must be running (<c>make app</c>, http://localhost:8080). Set <c>E2E_BASE_URL</c> to test another address.
/// </summary>
public abstract class E2ETest : PageTest
{
    private static readonly string BaseUrl =
        Environment.GetEnvironmentVariable("E2E_BASE_URL") ?? "http://localhost:8080";

    // Runs once, before the first test creates a browser. Does nothing if Chromium is already installed, so the tests
    // need no manual setup on a laptop or a CI runner (no PowerShell needed).
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

    protected ILocator Endpoint => Page.GetByLabel("Hvad vil du generere?");

    protected ILocator Count => Page.GetByLabel("Antal personer");

    protected ILocator GenerateButton => Page.GetByRole(AriaRole.Button, new() { Name = "Generér data" });

    protected ILocator Status => Page.GetByRole(AriaRole.Status);

    protected ILocator Cards => Page.GetByRole(AriaRole.Article);
}
