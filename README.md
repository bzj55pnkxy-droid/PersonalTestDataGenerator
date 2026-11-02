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
| `make sonar` | SonarQube on :9000 (first login `admin` / `admin`) |
| `make logs` | Follow container logs |
| `make down` | Stop containers, keep data |
| `make clean` | Stop containers and delete data (DB is re-imported next start) |

## Project layout

```
src/PersonalData.Api/          API: Controllers, Services, Models, Data
src/PersonalData.Api/wwwroot/  Frontend, served by the API at /
src/DataFiles/                 person-names.json, addresses.sql
tests/                         Unit, integration, E2E (Playwright) and Postman tests
```

## Database

| | |
|---|---|
| Host | `localhost:3307` (3307, not 3306, so it doesn't clash with a locally installed MySQL) |
| Database | `addresses` |
| User / password | `app` / `app` (root: `root`) |

These credentials are for local development only. The API reads the connection string `ConnectionStrings:Addresses` from `appsettings.Development.json`.
