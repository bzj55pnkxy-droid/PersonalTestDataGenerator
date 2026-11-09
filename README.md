# Personal Test Data Generator

Generates fake personal data for non-existing Danish persons (CPR, name, gender, date of birth, address, mobile number) through a REST API and a simple web UI.

ASP.NET Core (.NET 10) · MySQL 8 · HTML/CSS/JS · xUnit · Playwright · Postman/Newman · SonarQube

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Docker with Compose (Docker Desktop, OrbStack, …)
- `make`: preinstalled on macOS/Linux. On Windows, run `winget install ezwinports.make` and use it from Git Bash (`make help` needs Git Bash's `awk`; the other targets also work in PowerShell)

### VS Code

C# files are formatted automatically on save using the [Microsoft C# extension](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp). Install the extension to enable the formatter; workspace settings are in `.vscode/settings.json`.

## Quick start

```sh
make dev
```

Starts MySQL in Docker and runs the API with hot reload. Open <http://localhost:5277>.

On first start, MySQL imports `src/DataFiles/addresses.sql`. That takes a few seconds; `make` waits for it.

Without `make`, the same thing is:

```sh
docker compose up -d --wait db
dotnet watch --project src/PersonalData.Api
```

No .NET SDK? `make app` runs both MySQL and the API in Docker on <http://localhost:8080>.

## Commands

| Command | What it does |
|---|---|
| `make dev` | MySQL in Docker + API natively with hot reload on :5277 |
| `make app` | MySQL + API in Docker on :8080 |
| `make db` | MySQL only, on :3307 |
| `make test` | Run all tests; needs `make app` first (MySQL for the integration tests, the app on :8080 for the E2E tests) |
| `make e2e` | Start the app (`make app`) and run only the Playwright E2E tests in `tests/PersonalData.E2ETests` |
| `make api-test` | Run the Postman collection `tests/Postman/PersonalData.postman_collection.json` with Newman (Docker) against the API container (starts it if needed) |
| `make ci` | Local CI pipeline, see below |
| `make sonar` | SonarQube on :9000, dashboard open without login |
| `make logs` | Follow container logs |
| `make down` | Stop containers, keep data |
| `make clean` | Stop containers and delete data (DB is re-imported next start) |

The unit tests in `tests/PersonalData.UnitTests` need no running API or database: `dotnet test tests/PersonalData.UnitTests`.

The E2E tests download Chromium on the first run, so no manual Playwright setup is needed. They test `http://localhost:8080`; set `E2E_BASE_URL` to use another address.

## Unit tests

Related cases use xUnit `[Theory]` / `[InlineData]`, with Arrange–Act–Assert, fixed expected values, a fixed clock and controlled random selections from `Fakes.cs`. Each case exercises the real public generator or service; repository stubs keep JSON files and MySQL outside the unit-test suite.

Across unit, integration and E2E tests, methods use descriptive PascalCase names and explicit Arrange–Act–Assert. Parameterized cases show readable domain values such as ISO dates, letters and phone prefixes. Small private factories hide random selectors and repeated client/page setup; assertions remain visible in the test methods.

The cases follow the team's [current black-box design](docs/Black-box-testdesign.docx):

| Test file | Behavior covered |
|---|---|
| `NameGenderGeneratorTests.cs` | Complete female/male name rows, including punctuation and Danish characters; empty repository |
| `BirthDateGeneratorTests.cs` | All nine valid calendar examples around month transitions, including 29 February 2004 |
| `CprGeneratorTests.cs` | Hardcoded `ddMMyy` examples, leading date zeroes, ten ASCII digits and allowed gender parity |
| `AddressGeneratorTests.cs` | House-number and numeric floor/door boundaries; optional uppercase house letter; named and letter/number doors; postal/town row integrity; alphabetic streets; empty repository |
| `PhoneNumberGeneratorTests.cs` | Representative 1-, 2- and 3-digit allowed prefixes with eight-digit outputs and zero/nine suffix extremes |
| `FakePersonServiceTests.cs` | Bulk counts 0, 1, 2, 3, 50, 99, 100, 101 and 150; CPR/date/gender consistency in complete and combined person responses |

The implementation's 100-year birth-date window is used only to arrange deterministic date selection, not asserted as an assignment requirement. Street tests do not impose an exact length or allow spaces.

Forbidden output examples (such as a nine-digit CPR, an impossible date, a mismatched name/gender row or a disallowed phone prefix) are not inputs accepted by these generators. The unit suite checks actual generated output; it does not add a validator merely to exercise illustrative invalid strings or claim that every permitted prefix or output can occur. Real dataset loading, API response validation and HTTP errors remain integration/API-test responsibilities.

## Integration tests

The integration tests use the real API, name data and MySQL database.

`PersonApiTests` exercises all nine API functionalities in-process and checks actual output: complete name rows from JSON, CPR format and date/gender relationships, valid birth-date JSON, postal/town row integrity and eight-digit phones. These requests cover the corresponding `FakePersonService` paths in the collected .NET reports. Newman and E2E requests target the separate Docker API, whose execution is not included in those reports.

Start the database before running them:

```sh
docker compose up -d --wait db
dotnet test tests/PersonalData.IntegrationTests/PersonalData.IntegrationTests.csproj
```

To run only the person endpoint tests, append `--filter FullyQualifiedName~PersonApiTests` to the test command.

## E2E tests

The browser tests verify actual result cards and fields, bulk counts 2 and 100, form boundaries 1 and 101, and a visible HTTP 500 error. Small page-opening helpers arrange each scenario before the user action. Exact status sentences and incidental card-title wording are not pinned.

## Local CI

`make ci` runs `scripts/ci.sh`:

1. Starts MySQL + API in Docker and waits until both are healthy
2. Builds and runs all test projects (unit, integration, Playwright E2E) with coverage, wrapped in a SonarQube analysis
3. Runs the Postman collection with Newman against the API container
4. Stops MySQL + API (also on failure; data is kept)

On screen it shows the lines that matter per step: containers healthy, build result and warnings, the result per test project (with the error of every failed test), SonarQube warnings plus quality gate and coverage, and every Postman request with any failed assertions and the summary table. The full output of every step is in `TestResults/logs/`.

Needs SonarQube running. No token or login: the script creates its own analysis token with the default `admin` / `admin`, which stays unchanged because the dashboard needs no login.

```sh
make sonar
make ci
```

Results: <http://localhost:9000/dashboard?id=personal-test-data-generator>

`make sonar` only starts the server; it does not upload source code. Run `make ci` to publish an analysis. If the pipeline fails before the SonarQube upload step, no new analysis is published. In the project sidebar, choose **Project → Code**, or open <http://localhost:9000/code?id=personal-test-data-generator>, to browse the analyzed source files.

CI passes `sonar.coverage.exclusions=**/wwwroot/**` to the scanner. Frontend files remain included in static analysis and the code browser, but do not contribute to coverage; the .NET coverage reports measure the backend.

## Project layout

```
src/PersonalData.Api/          API: Controllers, Services, Models, Data
src/PersonalData.Api/wwwroot/  Frontend, served by the API at /
src/DataFiles/                 person-names.json, addresses.sql
tests/                         Unit, integration, E2E (Playwright) and Postman tests
docs/                         Black-box test design and screenshots
```

## API

All endpoints are `GET` and return JSON. OpenAPI description: `/openapi/v1.json`.

| Endpoint | Returns |
|---|---|
| `/api/cpr` | CPR |
| `/api/name-gender` | First name, last name, gender |
| `/api/name-gender-dob` | First name, last name, gender, date of birth |
| `/api/cpr-name-gender` | CPR, first name, last name, gender |
| `/api/cpr-name-gender-dob` | CPR, first name, last name, gender, date of birth |
| `/api/address` | Address |
| `/api/phone` | Mobile phone number |
| `/api/person` | All information for one person |
| `/api/persons?count=N` | All information for N persons (2-100) |

### HTTP status codes

- `200 OK`: successful generation on all endpoints; the response contains JSON.
- `400 Bad Request`: `/api/persons` requires an integer `count` between 2 and 100 (inclusive). Missing, empty, non-integer, or out-of-range values return `ValidationProblemDetails` with errors for `count`.

These responses and their schemas are documented in OpenAPI.

Request flow: `Controllers/FakePersonController` → `Services/FakePersonService` (one method per endpoint) → the generators in `Services/` → `Data/` (`person-names.json`, MySQL). `Random` and `TimeProvider` are injected so tests can control randomness and "today".

## Database

| | |
|---|---|
| Host | `localhost:3307` (3307, not 3306, so it doesn't clash with a locally installed MySQL) |
| Database | `addresses` |
| User / password | `app` / `app` (root: `root`) |

These credentials are for local development only. The API reads the connection string `ConnectionStrings:Addresses` from `appsettings.Development.json`.
