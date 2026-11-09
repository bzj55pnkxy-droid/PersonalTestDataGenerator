# Personal Test Data Generator

Generates fake personal data for non-existing Danish persons (CPR, name, gender, date of birth, address, mobile number) through a REST API and a simple web UI.

ASP.NET Core (.NET 10) · MySQL 8 · HTML/CSS/JS · xUnit · Playwright · Postman/Newman · SonarQube

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Docker with Compose (Docker Desktop, OrbStack, …)
- `make`: preinstalled on macOS/Linux. On Windows, run `winget install ezwinports.make` and use it from Git Bash (`make help` needs Git Bash's `awk`; the other targets also work in PowerShell)

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
| `make test` | Run all tests (`make db` first if tests need MySQL) |
| `make api-test` | Run the Postman collection `tests/Postman/PersonalData.postman_collection.json` with Newman (Docker) against the API container (starts it if needed) |
| `make ci` | Local CI pipeline, see below |
| `make sonar` | SonarQube on :9000, dashboard open without login |
| `make logs` | Follow container logs |
| `make down` | Stop containers, keep data |
| `make clean` | Stop containers and delete data (DB is re-imported next start) |

## Local CI

`make ci` runs `scripts/ci.sh`:

1. Starts MySQL + API in Docker and waits until both are healthy
2. Builds and runs all test projects with coverage, wrapped in a SonarQube analysis
3. Runs the Postman collection with Newman against the API container
4. Stops MySQL + API (also on failure; data is kept)

On screen it shows the lines that matter per step: containers healthy, build result and warnings, the result per test project (with the error of every failed test), SonarQube warnings plus quality gate and coverage, and every Postman request with any failed assertions and the summary table. The full output of every step is in `TestResults/logs/`.

Needs SonarQube running. No token or login: the script creates its own analysis token with the default `admin` / `admin`, which stays unchanged because the dashboard needs no login.

```sh
make sonar
make ci
```

Results: <http://localhost:9000/dashboard?id=personal-test-data-generator>

## Project layout

```
src/PersonalData.Api/          API: Controllers, Services, Models, Data
src/PersonalData.Api/wwwroot/  Frontend, served by the API at /
src/DataFiles/                 person-names.json, addresses.sql
tests/                         Unit, integration, E2E (Playwright) and Postman tests
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
